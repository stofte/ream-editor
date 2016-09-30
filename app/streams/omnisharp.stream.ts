import { Injectable } from '@angular/core';
import { Http, Response } from '@angular/http';
import { Observable, Observer, Subscription, Subject } from 'rxjs/Rx';
import { QueryMessage, OmnisharpMessage, WebSocketMessage, SessionMessage, EditorMessage } from '../messages/index';
import { ProcessStream, EditorStream, QueryStream, SessionStream } from './index';
import { ProcessHelper } from '../utils/process-helper';
import { CodeRequest, UpdateBufferRequest, AutoCompletionItem } from './interfaces';
import { CodeCheckResult, EditorChange, AutocompletionQuery, CodeCheckQuery, TextUpdate } from '../models/index';
import * as uuid from 'node-uuid';
import config from '../config';

class SessionTemplateMap {
    public created: number;
    constructor(
        public columnOffset: number,
        public lineOffset: number,
        public fileName: string,
        public sessionId: string,
        public connectionId: number,
        public templateRequest: UpdateBufferRequest = null,
        public edits: EditorChange[] = null,
        public timestamp: number = null,
    ) {
        this.created = performance.now();
    }
}

class SessionUpdated {
    constructor(
        public sessionId: string,
        public timestamp: number
    ) {}    
}

class OperationInflightMap {
    // indicates the latest timestamp that we've sent to omnisharp
    public edits: any = {};
    // indicates the latest timestamp that we've seen come back from omnisharp
    public updated: any = {};
    private count: any = {};
    private resolvers: any[] = [];
    public start(sessionId: string): number {
        if (!this.count[sessionId]) {
            this.count[sessionId] = 0;
        }
        this.count[sessionId]++;
        return this.edits[sessionId];
    }
    public stop(sessionId: string) {
        this.count[sessionId]--;
        if (this.count[sessionId] === 0) {
            const l = this.resolvers[sessionId];
            this.resolvers[sessionId] = [];
            if (l && l.length > 0) {
                l.forEach(x => {
                    x.fn(1);
                    const prevTs = this.edits[sessionId];
                    Assert(prevTs < x.ts, 'Timestamp strictly increasing');
                    this.edits[sessionId] = x.ts;
                }); // x is the resolver for promises so return whatever
            }
        }
        Assert(this.count[sessionId] >= 0, 'Operation counter is positive');
    }

    public busy(sessionId: string, editTimestamp: number, resolve: any): boolean {
        const isBusy = this.count[sessionId] > 0; 
        if (isBusy) {
            if (!this.resolvers[sessionId]) {
                this.resolvers[sessionId] = [];
            }
            this.resolvers[sessionId].push({fn: resolve, ts: editTimestamp});
        } else {
            const prevTs = this.edits[sessionId];
            if (prevTs) {
                Assert(prevTs < editTimestamp, 'Timestamp strictly increasing');
            }
            this.edits[sessionId] = editTimestamp;
        }
        return isBusy;
    }

    public updateResponse(sessionId: string, editTimestamp: number) {
        if (!this.updated[sessionId]) {
            this.updated[sessionId] = 0;
        }
        const currentTs = this.updated[sessionId];
        if (currentTs < editTimestamp) {
            this.updated[sessionId] = editTimestamp;
        } else {
            // responses might get interleaved?
            console.log('response timestamp decreased!!!!!!!!!!!');
        }
    }
}

class SessionOperation {
    constructor(
        public sessionId: string,
        public operation: string,
        public timestamp: number,
        public waitForEdit: number,
        public request: any
    ) { }
}

@Injectable()
export class OmnisharpStream {
    public events: Observable<OmnisharpMessage>;
    private process: ProcessStream;
    private templateMap: any = {};
    private operationMap: OperationInflightMap = new OperationInflightMap();
    constructor(
        private editor: EditorStream,
        private query: QueryStream,
        private session: SessionStream,
        private http: Http
    ) {
        const sessionMaps = query.events
            .filter(msg => msg.type === 'buffer-template')
            .map(msg => {
                const bufferType = msg.template.connectionId ? `db${msg.template.connectionId}ctx` : 'code';
                const update: UpdateBufferRequest = {
                    SessionId: msg.id,
                    FileName: `${config.omnisharpProjectPath}\\${bufferType}${msg.id.replace(/\-/g, '')}.cs`,
                    FromDisk: false,
                    Buffer: msg.template.template
                };
                return new SessionTemplateMap(
                    msg.template.columnOffset,
                    msg.template.lineOffset,
                    update.FileName,
                    msg.id,
                    msg.template.connectionId,
                    update,
                    null,
                    performance.now()
                );
            })
            .flatMap(sessionMap => 
                this.http
                    .post(this.action('updatebuffer'), JSON.stringify(sessionMap.templateRequest))
                    .map(res => sessionMap)) // todo check result
            .publish();
        
        sessionMaps.subscribe(map => {
            this.templateMap[map.sessionId] = new SessionTemplateMap(map.columnOffset, map.lineOffset, map.fileName, null, null);
        });

        const updateBufferTimestamps = session.events
            .filter(msg => msg.type === 'create' || msg.type === 'context')
            .flatMap(msg => {
                // one time delay until session is ready
                let sessionMap: SessionTemplateMap = null;
                const ready = new Promise((done) => {
                    const sessionSub = sessionMaps.filter(x => x.sessionId === msg.id).subscribe(x => {
                        sessionMap = x;
                        done();
                        sessionSub.unsubscribe();
                    });
                });
                
                const pausedForOperation = (msg: EditorMessage): Observable<number> => {
                    const p = new Promise<number>((done) => {
                        // todo add timestamp to EditorMessage itself, instead of riding along in TextUpdate
                        if (!this.operationMap.busy(msg.id, msg.data.timestamp, done)) {
                            done(1);
                        }
                    });
                    return Observable.fromPromise(p);
                };

                const mapEditorMsg = (x: EditorMessage) => {
                    return <EditorChange> {
                        newText: x.data.text.join('\n'),
                        startLine: x.data.from.line + sessionMap.lineOffset + 1,
                        startColumn: x.data.from.ch + (x.data.from.line === 0 ? sessionMap.columnOffset : 0) + 1,
                        endLine: x.data.to.line + sessionMap.lineOffset + 1,
                        endColumn: x.data.to.ch + (x.data.to.line === 0 ? sessionMap.columnOffset : 0) + 1,
                        timestamp: x.data.timestamp
                    };
                };
                const mapEditorChanges = (edits: EditorChange[]) => {
                    return new SessionTemplateMap(
                            sessionMap.columnOffset,
                            sessionMap.lineOffset,
                            sessionMap.fileName,
                            sessionMap.sessionId,
                            null,
                            null,
                            edits
                    );
                };

                // if we're handling a context msg, we need to wait for the editor msg that contains the curent full buffer text
                const initialText = msg.type === 'create' ? Observable.empty<SessionTemplateMap>() :// Observable.empty<SessionTemplateMap>(); 
                        new Observable<SessionTemplateMap>((innerObs: Observer<SessionTemplateMap>) => {
                            const anotherSub = editor.bufferedTexts
                                .filter(x => x.type === 'buffer-text' && x.id === msg.id &&
                                    x.bufferTextOrigin === 'context' && x.originTimestamp === msg.timestamp)
                                .delayWhen(x => Observable.fromPromise(ready))
                                .subscribe(x => {
                                    const textUpdate: TextUpdate = {
                                        from: { line: 0, ch: 0 },
                                        to: { line: 0, ch: 0},
                                        text: x.text.split('\n'),
                                        removed: [""],
                                        timestamp: x.originTimestamp
                                    };
                                    const mappedEdit = new EditorMessage('edit', x.id, textUpdate);
                                    innerObs.next(mapEditorChanges([mapEditorMsg(mappedEdit)]));
                                    innerObs.complete();
                                    setTimeout(() => {
                                        anotherSub.unsubscribe();
                                    }, 200);
                                });
                        })
                        ;
                
                const editorSub = initialText.concat(
                    editor.events
                        .filter(x => x.type === 'edit' && x.id === msg.id)
                        .delayWhen(x => Observable.fromPromise(ready))
                        .delayWhen(pausedForOperation)
                        .map(mapEditorMsg)
                        .bufferTime(500)
                        .filter(edits => edits.length > 0)
                        .map(edits => new SessionTemplateMap(
                            sessionMap.columnOffset,
                            sessionMap.lineOffset,
                            sessionMap.fileName,
                            sessionMap.sessionId,
                            null,
                            null,
                            edits))
                        .takeUntil(sessionMaps.filter(x => sessionMap !== null && sessionMap.created !== x.created && x.sessionId === msg.id)));
                return editorSub;
            })
            .flatMap(sessionMap => {
                return this.http
                    .post(this.action('updatebuffer'), JSON.stringify({
                        FromDisk: false,
                        FileName: sessionMap.fileName,
                        Changes: sessionMap.edits
                    }))
                    .map(res =>
                        new SessionUpdated(
                            sessionMap.sessionId,
                            sessionMap.edits[sessionMap.edits.length - 1].timestamp));
            })
            .publish();

        const waitForBuffer: (op: SessionOperation) => Observable<number> = (op) => {
            const p = new Promise<number>((done) => {
                if (this.operationMap.updated[op.sessionId] >= op.waitForEdit) {
                    done(1);
                } else {
                    const sub = updateBufferTimestamps.filter(x => x.sessionId === op.sessionId && x.timestamp >= op.waitForEdit).subscribe(x => {
                        sub.unsubscribe();
                        done(1);
                    });
                }
            });
            return Observable.fromPromise(p);
        };
        updateBufferTimestamps.subscribe(ts => {
            this.operationMap.updateResponse(ts.sessionId, ts.timestamp);
        });
        const operationResponses = session.events
            .filter(msg => msg.type === 'autocomplete' || msg.type === 'codecheck')
            .map(msg => {
                const waitForEdit = this.operationMap.start(msg.id);
                let msgMap: SessionOperation = null;
                const tmpl: SessionTemplateMap = this.templateMap[msg.id];
                if (msg.type === 'autocomplete') {
                    const request = msg.autoComplete;
                    request.fileName = tmpl.fileName;
                    request.column += (request.line === 0 ? tmpl.columnOffset : 0) + 1;
                    request.line += tmpl.lineOffset + 1;
                    msgMap = new SessionOperation(msg.id, 'autocomplete', msg.timestamp, waitForEdit, request);
                } else if (msg.type === 'codecheck') {
                    msgMap = new SessionOperation(msg.id, 'codecheck', msg.timestamp, waitForEdit, { fileName: tmpl.fileName });
                }
                return msgMap;
            })
            .delayWhen(waitForBuffer)
            .flatMap(msg => {
                return this.http
                    .post(this.action(msg.operation), JSON.stringify(msg.request))
                    .map(res => {
                        this.operationMap.stop(msg.sessionId); 
                        let responseMsg: OmnisharpMessage = null;
                        const json = res.json();
                        if (msg.operation === 'autocomplete') {
                            const completions: AutoCompletionItem[] = json;
                            responseMsg = new OmnisharpMessage('autocompletion', msg.sessionId, null, completions, null, msg.timestamp);
                        } else if (msg.operation === 'codecheck') {
                            const mappedFixes = this.mapQuickFixes(json, msg.sessionId, this.templateMap);
                            const fixes = this.filterCodeChecks(mappedFixes);
                            responseMsg = new OmnisharpMessage('codecheck', msg.sessionId, null, null, fixes, msg.timestamp);
                        }
                        return responseMsg;
                    });
            })
            .publish();

        this.process = new ProcessStream(http);
        this.events = this.process
            .status
            .map(msg => new OmnisharpMessage(msg.type))
            .merge(operationResponses);

        updateBufferTimestamps.connect();
        operationResponses.connect();

        let helper = new ProcessHelper();
        let cmd = helper.omnisharp(config.omnisharpPort);
        this.process.start('omnisharp', cmd.command, cmd.directory, config.omnisharpPort);
        this.once(msg => msg.type === 'ready', () => {
            sessionMaps.connect();
        });
    }

    public once(pred: (msg: OmnisharpMessage) => boolean, handler: (msg: OmnisharpMessage) => void) {
        const sub = this.events.filter(msg => pred(msg)).subscribe(msg => {
            sub.unsubscribe();
            handler(msg);
        });
    }

    public stopServer() {
        this.process.close();
    }

    private mapQuickFixes = (result: any, mapKey: string, maps: any): CodeCheckResult[] => {
        let fixes: any[] = result.QuickFixes;
        const map = maps[mapKey];
        Assert(map != null, 'Did not find template map');
        return fixes.map(x => {
            const line = x.Line - 1 - (map.lineOffset);
            const column = x.Column - 1 - (line === 0 ? map.columnOffset : 0);
            const endLine = x.EndLine - 1 - (map.lineOffset);
            const endColumn = x.EndColumn - 1 - (endLine === 0 ? map.columnOffset : 0);
            return <CodeCheckResult> {
                text: this.cleanupMessage(x.Text),
                logLevel: x.LogLevel,
                fileName: x.FileName,
                line,
                column,
                endLine,
                endColumn
            };
        });
    }
    
    private filterCodeChecks(checks: CodeCheckResult[]): CodeCheckResult[] {
        let filt = checks
            .filter(c => {
                const isMissingSemicolon = c.text === '; expected';
                const isHidden = c.logLevel === 'Hidden';
                const isBeforeUserStub = c.line < 0;
                return !(isHidden || isMissingSemicolon || isBeforeUserStub);
            });
        return filt;
    }
    
    private cleanupMessage(text: string) {
        let fluf = [
            / \(are you missing a using directive or an assembly reference\?\)/
        ];
        let s = text;
        fluf.forEach(r => {
            s = s.replace(r, '');
        });
        return s;
    }

    private action(name: string) {
        return `http://localhost:${config.omnisharpPort}/${name}`;
    }
}
