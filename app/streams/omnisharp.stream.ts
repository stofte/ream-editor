import { Injectable } from '@angular/core';
import { Http, Response } from '@angular/http';
import { Observable, Observer, Subscription, Subject } from 'rxjs/Rx';
import { ProcessStream, EditorStream, QueryStream } from './index';
import { InputStream } from './input.stream';
import { EventName, Message, OmnisharpSessionMessage } from './api';
import { ProcessHelper } from '../utils/process-helper';
import { CodeRequest, UpdateBufferRequest, AutoCompletionItem } from './interfaces';
import { CodeCheckResult, EditorChange, AutocompletionQuery, CodeCheckQuery, TextUpdate } from '../models/index';
import { OmnisharpSynchronizer } from './omnisharp-synchronizer';
import * as uuid from 'node-uuid';
import config from '../config';
const path = electronRequire('path');

class MessageMap {
    public inner: OmnisharpSessionMessage;
    public mapped: Message;
}

@Injectable()
export class OmnisharpStream {
    public events: Observable<Message>;
    private process: ProcessStream;
    constructor(
        private editor: EditorStream,
        private query: QueryStream,
        private input: InputStream,
        private http: Http
    ) {
        const sync = new OmnisharpSynchronizer();
        const stream = query.events
            .filter(msg => msg.name === EventName.QueryTemplateResponse)
            .merge(input.events.filter(msg => 
                msg.name === EventName.SessionCreate ||
                msg.name === EventName.SessionContext ||
                msg.name === EventName.SessionAutocompletion || 
                msg.name === EventName.SessionCodeCheck
            ))
            .merge(editor.events.filter(msg => msg.name === EventName.EditorUpdate))
            .map(msg => {
                // edits are merged next and have their own mapping scheme
                const event = (msg.name === EventName.SessionCreate || msg.name === EventName.SessionContext) ? 'context' :
                    msg.name === EventName.QueryTemplateResponse ? 'buffer-template' :
                    msg.name === EventName.SessionAutocompletion ? 'autocompletion' : 
                    msg.name === EventName.SessionCodeCheck ? 'codecheck' : 'edit';

                Assert(event && event.length, 'Mapped Message to inner');

                let fileName = null;
                if (event === 'buffer-template') {
                    const bufferType = msg.data.connectionId ? `db${msg.data.connectionId}ctx` : 'code';
                    fileName = path.normalize(`${config.omnisharpProjectPath}/${bufferType}${msg.id.replace(/\-/g, '')}.cs`);
                }
                const lineOffset = msg.data && msg.data.lineOffset || null;
                const columnOffset = msg.data && msg.data.columnOffset || null;
                const template = msg.data && msg.data.template || null;
                const connectionId = msg.data && msg.data.connectionId || null;

                const mapped = <OmnisharpSessionMessage> {
                    sessionId: msg.id,
                    timestamp: msg.originalTimestamp || msg.timestamp,
                    type: event,
                    fileName,
                    lineOffset,
                    columnOffset,
                    template,
                    connectionId,
                    autocompletion: msg.data,
                    edit: msg.data
                };
                return mapped;
            })
            // obtain lock
            .delayWhen(msg => Observable.fromPromise(sync.queueOperation(msg)))
            .map(msg => sync.mapMessage(msg))
            .flatMap(msg => {
                if (msg.type === 'context') {
                    return Observable.from<MessageMap>([{ inner: msg, mapped: new Message(null) }]);
                } else {
                    const actionName = (msg.type === 'buffer-template' || msg.type === 'edit') ? this.action('updatebuffer') :
                        msg.type === 'autocompletion' ? this.action('autocomplete') : this.action('codecheck');
                    const request = this.mapToRequest(msg);
                    return this.http.post(actionName, JSON.stringify(request))
                        .map(x => this.mapResponse(msg, x.json()))
                        .map(x => <MessageMap> { inner: msg, mapped: x });
                }
            })
            // release lock
            .do(msg => sync.resolveOperation(msg.inner))
            // events that has no output gets filtered here
            .filter(msg => msg.mapped.name !== null)
            .map(msg => msg.mapped)
            .publish();


        this.process = new ProcessStream(http);
        this.events = this.process.status.merge(stream);
        stream.first().subscribe(x => this.process.confirmedReady());
        stream.connect();
        
        let helper = new ProcessHelper();
        let cmd = helper.omnisharp(config.omnisharpPort);
        this.process.start('omnisharp', cmd.command, cmd.directory, config.omnisharpPort);
    }

    public once(pred: (msg: Message) => boolean, handler: (msg: Message) => void) {
        const sub = this.events.filter(msg => pred(msg)).subscribe(msg => {
            sub.unsubscribe();
            handler(msg);
        });
    }

    public stopServer() {
        this.process.close();
    }

    private mapToRequest(msg: OmnisharpSessionMessage): any {
        let request: any = {
            FileName: msg.fileName
        };
        if (msg.type === 'buffer-template') {
            request.fromDisk = false;
            request.buffer = msg.template;
        } else if (msg.type === 'edit') {
            request.changes = this.mapCodeMirrorEditsToOmnisharp(msg);
            request.fromDisk = false;
        } else if (msg.type === 'autocompletion') {
            request = msg.autocompletion;
        }
        return request;
    }

    private mapResponse(msg: OmnisharpSessionMessage, response: any): Message {
        const name = msg.type === 'codecheck' ? EventName.OmniSharpCodeCheck :
            msg.type === 'autocompletion' ? EventName.OmniSharpAutocompletion : null;
        const data = msg.type === 'codecheck' ? this.mapQuickFixes(msg, response) : 
            msg.type === 'autocompletion' ? response : {};
        return <Message> {
            id: msg.sessionId,
            name,
            data
        };
    }

    private mapCodeMirrorEditsToOmnisharp = (msg: OmnisharpSessionMessage) => {
        Assert(msg.type === 'edit', `Found "${msg.type}" msg type, expecting "edit"`);
        return[msg.edit].map(x => {
            const mapped = <EditorChange> {
                newText: x.text.join('\n'),
                startLine: x.from.line,
                startColumn: x.from.ch,
                endLine: x.to.line,
                endColumn: x.to.ch,
                timestamp: x.timestamp
            };
            return mapped;
        });
    };

    private mapQuickFixes = (msg: OmnisharpSessionMessage, result: any): CodeCheckResult[] => {
        let fixes: any[] = result.QuickFixes;
        return this.filterCodeChecks(fixes.map(x => {
            const line = x.Line - 1 - (msg.lineOffset);
            const column = x.Column - 1 - (line === 0 ? msg.columnOffset : 0);
            const endLine = x.EndLine - 1 - (msg.lineOffset);
            const endColumn = x.EndColumn - 1 - (endLine === 0 ? msg.columnOffset : 0);
            return <CodeCheckResult> {
                text: this.cleanupMessage(x.Text),
                logLevel: x.LogLevel,
                fileName: x.FileName,
                line,
                column,
                endLine,
                endColumn
            };
        }));
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
