import { Injectable } from '@angular/core';
import { Http, Response } from '@angular/http';
import { Observable, Observer, Subscription, Subject } from 'rxjs/Rx';
import { QueryMessage, OmnisharpMessage, WebSocketMessage, SessionMessage } from '../messages/index';
import { ProcessStream, EditorStream, QueryStream, SessionStream } from './index';
import { ProcessHelper } from '../utils/process-helper';
import { CodeRequest, UpdateBufferRequest } from './interfaces';
import { CodeCheckResult, EditorChange } from '../models/index';
import * as uuid from 'node-uuid';
import config from '../config';

class SessionTemplateMap {
    constructor(
        public columnOffset: number,
        public lineOffset: number,
        public fileName: string,
        public sessionId: string,
        public templateRequest: UpdateBufferRequest = null,
        public edits: EditorChange[] = null,
        public timestamp: number = null,
    ) {}
}

class SessionUpdated {
    constructor(
        public sessionId: string,
        public timestamp: number
    ) {}    
}

class SessionReadyState {
    constructor(
        public sessionId: string,
        public ready: boolean
    ) {}
}


class CodeCheckMap {
    sessionId: string;
    fileName: string;
    timestamp: number;
    fixes: CodeCheckResult[];
}

@Injectable()
export class OmnisharpStream {
    public events: Observable<OmnisharpMessage>;
    private process: ProcessStream;
    constructor(
        private editor: EditorStream,
        private query: QueryStream,
        private session: SessionStream,
        private http: Http
    ) {
        this.process = new ProcessStream(http);

        const sessionMaps = query.events
            .filter(msg => msg.type === 'code-template-response')
            .map(msg => {
                const update: UpdateBufferRequest = {
                    SessionId: msg.id,
                    FileName: `${config.omnisharpProjectPath}\\b${msg.id.replace(/\-/g, '')}.cs`,
                    FromDisk: false,
                    Buffer: msg.codeTemplate.template
                };
                return new SessionTemplateMap(
                    msg.codeTemplate.columnOffset,
                    msg.codeTemplate.lineOffset,
                    update.FileName,
                    msg.id,
                    update
                );
            })
            .flatMap(sessionMap => 
                this.http
                    .post(this.action('updatebuffer'), JSON.stringify(sessionMap.templateRequest))
                    .map(res => sessionMap)) // todo check result
            .publish();

        const sessionReady = session.events
            .filter(msg => msg.type === 'create')
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
                return editor.events
                    .filter(x => x.type === 'edit' && x.id === msg.id)
                    .delayWhen(x => Observable.fromPromise(ready))
                    .map(msg => {
                        return <EditorChange> {
                            newText: msg.data.text.join('\n'),
                            startLine: msg.data.from.line + sessionMap.lineOffset + 1,
                            startColumn: msg.data.from.ch + (msg.data.from.line === 0 ? sessionMap.columnOffset : 0) + 1,
                            endLine: msg.data.to.line + sessionMap.lineOffset + 1,
                            endColumn: msg.data.to.ch + (msg.data.to.line === 0 ? sessionMap.columnOffset : 0) + 1,
                            timestamp: msg.data.timestamp
                        };
                    })
                    .bufferTime(500)
                    .filter(msgs => msgs.length > 0)
                    .map(edits => new SessionTemplateMap(
                        sessionMap.columnOffset,
                        sessionMap.lineOffset,
                        sessionMap.fileName,
                        sessionMap.sessionId,
                        null,
                        edits
                    ));
            })
            .flatMap(sessionMap => 
                this.http
                    .post(this.action('updatebuffer'), JSON.stringify({
                        FromDisk: false,
                        FileName: sessionMap.fileName,
                        Changes: sessionMap.edits
                    }))
                    //.delay(1000)
                    .map(res =>
                        new SessionUpdated(
                            sessionMap.sessionId,
                            sessionMap.edits[sessionMap.edits.length-1].timestamp)))
            .publish();

        sessionReady.connect();
        
        const sessionFiles: any = {};
        const templateMaps = {};
        sessionMaps.subscribe(map => {
            if (!sessionFiles[map.sessionId]) {
                sessionFiles[map.sessionId] = map.fileName;
            }
            if (!templateMaps[map.sessionId]) {
                templateMaps[map.sessionId] = new SessionTemplateMap(map.columnOffset, map.lineOffset, map.fileName, null);
            }
        });

        const sessionStatus = session.events.filter(msg => msg.type === 'create')
            .flatMap(msg => {
                return new Observable<SessionReadyState>((obs: Observer<SessionReadyState>) => {
                    let lastEdit = 0;
                    let lastUpdate = 1;
                    let lastState = false;
                    editor.events.filter(x => x.type === 'edit' && x.id === msg.id).subscribe(x => {
                        Assert(lastEdit < x.data.timestamp, 'edit timestamp did not increase');
                        const wasFirst = lastEdit === 0;
                        lastEdit = x.data.timestamp;
                        if (lastState || wasFirst) {
                            lastState = false;
                            obs.next(new SessionReadyState(msg.id, false));
                        }
                    });
                    sessionReady.filter(x => x.sessionId === msg.id).subscribe(x => {
                        Assert(lastUpdate < x.timestamp, 'update timestamp did not increase');
                        lastUpdate = x.timestamp;
                        if (lastState !=  lastEdit <= lastUpdate) {
                            lastState = lastEdit <= lastUpdate;
                            obs.next(new SessionReadyState(msg.id, lastState));
                        }
                    });
                });
            })
            .scan((validSessions: string[], session: SessionReadyState) => {
                const without = validSessions.filter(x => x !== session.sessionId);
                if (session.ready) {
                    return [session.sessionId].concat(validSessions);
                }
                return without;
            }, [])
            .publishReplay();

        sessionStatus
            .connect();

        const codeChecks = session.events
            .filter(msg => msg.type === 'codecheck')
            .delayWhen(msg => {
                return new Observable<number>((obs: Observer<number>) => {
                    let done = false;
                    const sub = sessionStatus.filter(s => s.indexOf(msg.id) !== -1).subscribe(() => {
                        if (!done) {
                            done = true;
                            obs.next(1);
                            obs.complete();
                            // timing sensitive, if we dont wait, sub might be undefined
                            setTimeout(() => {
                                sub.unsubscribe();
                            },200);
                        }
                    });
                });
            })
            .map(msg => {
                return new SessionTemplateMap(null, null, sessionFiles[msg.id], msg.id, null, null, performance.now());
            })
            .flatMap(msg => 
                this.http
                    .post(this.action('codecheck'), JSON.stringify({ FileName: msg.fileName }))
                    .map(res => res.json())
                    .map(res => this.mapQuickFixes(res, msg.sessionId, templateMaps))
                    // .map(checks => <CodeCheckMap> {
                    //     sessionId: msg.sessionId,
                    //     fileName: msg.fileName,
                    //     timestamp: msg.timestamp,
                    //     fixes: this.filterCodeChecks(checks)
                    // })
                    .map(checks => {
                        return new OmnisharpMessage('codecheck', msg.sessionId, null, null, this.filterCodeChecks(checks));
                    }));

        // codechecks
        //     .subscribe(x => {
        //         console.log('codecheck response', x);
        //     });

        // codechecks.flatMap(msg => {
        //     return this.http
        //             .post(this.action('CodeFormat'), JSON.stringify({ FileName: msg.fileName }));
        // }).subscribe(x => {
        //     console.log('codeformat', x.json().Buffer)
        // })

        // editor.events.filter(msg => msg.type === 'edit')
        //     .flatMap(msg => {
        //         return sessionReady.filter(x => x.timestamp === msg.data.timestamp)
        //     })
        //     .subscribe(x => {
        //         console.log('session')
        //     });

        // keep timestamps for all sessions, to simplify codecheck stream
        // const sessionTimestamp = <any> {};

        // const codechecks = session.events
        //     .filter(msg => msg.type === 'codecheck')

        //     .flatMap(msg => {
        //         const ready = new Promise((done) => {
        //             if (sessionTimestamp[msg.id] >= msg.timestamp) {
        //                 done();
        //             } else {
        //                 console.log('waiting for state to catch up', msg.id, msg.timestamp, performance.now());
        //                 const sub = sessionReady
        //                     .do(x => {
        //                         if (x.sessionId === msg.id) {
        //                             console.log('while waiting', x.timestamp, performance.now());
        //                         }
        //                     })
        //                     .filter(state => state.sessionId === msg.id && state.timestamp >= msg.timestamp)
        //                     .subscribe(() => {
        //                         sub.unsubscribe();
        //                         done();
        //                     });
        //             }
        //         });
        //         return Observable.fromPromise(ready).map(() => msg);
        //     })
        //     .subscribe(x => {
        //         console.log('codecheck response', x.id, performance.now() - x.timestamp);
        //     });

        // const codeCheckResults = session.events
        //     .filter(msg => msg.type === 'codecheck')
        //     .delayWhen(msg => sessionReadyStates.filter(x => x.sessionId === msg.id && x.timestamp >= msg.timestamp))
        //     .subscribe(msg => {
        //         console.log('codechecking now', performance.now())
        //     });
        
        this.events = this.process
            .status
            .map(msg => new OmnisharpMessage(msg.type))
            .merge(codeChecks);

        let helper = new ProcessHelper();
        let cmd = helper.omnisharp(config.omnisharpPort);
        this.process.start('omnisharp', cmd.command, cmd.directory, config.omnisharpPort);
        this.once(msg => msg.type === 'ready', () => {
            sessionMaps.connect();
            // sessionReadyStates.connect();
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
            const endColumn = x.EndColumn - 1 - (endLine === 0 ? map.lineOffset : 0);
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
