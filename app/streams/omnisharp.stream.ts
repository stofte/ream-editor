import { Injectable } from '@angular/core';
import { Http, Response } from '@angular/http';
import { Observable, Observer, Subscription, Subject } from 'rxjs/Rx';
import { ProcessStream, QueryStream, EditorStream } from './index';
import { InputStream } from './input.stream';
import { EventName, Message, OmnisharpSessionMessage } from './api';
import { ProcessHelper } from '../utils/process-helper';
import { CodeRequest, UpdateBufferRequest, AutoCompletionItem } from './interfaces';
import { CodeCheckResult, EditorChange, AutocompletionQuery, CodeCheckQuery, TextUpdate } from '../models/index';
import { OmnisharpSynchronizer } from './omnisharp-synchronizer';
import * as uuid from 'node-uuid';
import config from '../config';
const path = electronRequire('path');

@Injectable()
export class OmnisharpStream {
    public events: Observable<Message>;
    private process: ProcessStream;
    constructor(
        private query: QueryStream,
        private input: InputStream,
        private editor: EditorStream,
        private http: Http
    ) {
        // Used to sync all requests to omnisharp backend.
        const sync = new OmnisharpSynchronizer();
        
        // Create the omnisharp stream, first by gather all the Messages types we care for,
        // these are then mapped to OmnisharpSessionMessage for the synchronizer.
        // Every msg funnels into the queueOperation call, causing that msg to block on a promise,
        // which the synchronizer will resolve when appropriate.
        const stream = query.events
            .filter(msg => msg.name === EventName.QueryTemplateResponse)
            .merge(editor.events.filter(msg => msg.name === EventName.EditorCodeCheck))
            .merge(input.events.filter(msg => 
                msg.name === EventName.SessionCreate ||
                msg.name === EventName.SessionContext ||
                msg.name === EventName.SessionAutocompletion ||
                msg.name === EventName.SessionDestroy
            ))
            .merge(input.events.filter(msg => msg.name === EventName.EditorUpdate))
            // Map to internal OmnisharpSessionMessage
            .map(this.mapToInternal)
            // To buffer the edits as much as possible, while preserving absolute order with the other events,
            // we do some ghetto buffering here, which reduces updatebuffer calls significantly.
            // 
            // We use interval to generate timeout values, which are passed as type: 'timer'.
            // this.bufferEdits uses BufferedMessage.ready to indicate to downstream
            // if there's anything to process. BufferedMessage is only used for this purpose,
            // and to maintain state inside the scan operator itself.
            //
            // If ready, we map to the list of msgs, which should contain
            // - 1 or more edit messeges (1 per session found, if we had a timer event)
            // - 1 edit msg, 1 operation (meaning the op flushed the edits for the given session)
            // - 1 operation
            .merge(Observable.interval(500).map(n => <OmnisharpSessionMessage> { type: 'timer' }))
            .scan(this.bufferEdits, <BufferedMessage> { ready: false, list: [], edits: [] })
            .filter(x => x.ready)
            .concatMap(x => x.list) // map back to OmnisharpSessionMessage
            // obtain lock
            .delayWhen((msg: OmnisharpSessionMessage) => Observable.fromPromise(sync.queueOperation(msg)))
            .map((msg: OmnisharpSessionMessage) => sync.mapMessage(msg))
            .flatMap(msg => {
                if (msg.type === 'context' || msg.type === 'destroy') {
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

    private bufferEdits = (buffer: BufferedMessage, msg: OmnisharpSessionMessage): BufferedMessage => {
        if (msg.type === 'timer') { // flush for all session
            const ids = buffer.edits.reduce((ids, edit) => {
                return ids.indexOf(edit.sessionId) > -1 ? ids : ids.concat(edit.sessionId);
            }, []);
            const edits = ids.map(x => {
                const sessionEdits = buffer.edits.filter(y => y.sessionId === x);
                const singleEdit = this.mapMultipleEditsToOne(sessionEdits);
                return singleEdit;
            });
            return <BufferedMessage> {
                ready: true,
                list: edits,
                edits: []
            };
        } else if (msg.type !== 'edit') { // flush any edits for msg.sessionId
            const edit = this.mapMultipleEditsToOne(buffer.edits.filter(x => x.sessionId === msg.sessionId));
            const otherEdits = buffer.edits.filter(x => x.sessionId !== msg.sessionId);
            return <BufferedMessage> {
                ready: true,
                list: (edit ? [edit, msg] : [msg]),
                edits: otherEdits
            };
        } else if (msg.type === 'edit') { // else just append edit
            return <BufferedMessage> {
                ready: false,
                list: [],
                edits: buffer.edits.concat([msg])
            };
        }
    }

    private mapMultipleEditsToOne = (edits: OmnisharpSessionMessage[]): OmnisharpSessionMessage => {
        if (edits.length === 0) {
            return null;
        }
        let i = 0;
        let id = edits[i].sessionId;
        do {
            Assert(edits[i].type === 'edit', 'All messeges are "edits"');
            Assert(edits[i].sessionId === id, 'All messages have same id');
            i++;
        } while (i < edits.length);
        let ret = <OmnisharpSessionMessage> {
            sessionId: id,
            type: 'edit',
            edits: edits.map(x => x.edit),
            timestamp: edits[edits.length - 1].timestamp,
        };
        return ret;
    }

    private mapToInternal = (msg: Message): OmnisharpSessionMessage => {
        const event = (msg.name === EventName.SessionCreate || msg.name === EventName.SessionContext) ? 'context' :
            msg.name === EventName.QueryTemplateResponse ? 'buffer-template' :
            msg.name === EventName.SessionAutocompletion ? 'autocompletion' : 
            msg.name === EventName.EditorCodeCheck ? 'codecheck' : 
            msg.name === EventName.SessionDestroy ? 'destroy' : 'edit';

        Assert(event && event.length, 'Mapped Message to inner');

        let fileName = null;
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
    }

    private mapToRequest = (msg: OmnisharpSessionMessage): any => {
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

    private mapResponse = (msg: OmnisharpSessionMessage, response: any): Message => {
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
        return msg.edits.map(x => {
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
    
    private filterCodeChecks = (checks: CodeCheckResult[]): CodeCheckResult[] => {
        let filt = checks
            .filter(c => {
                const isMissingSemicolon = c.text === '; expected';
                const isHidden = c.logLevel === 'Hidden';
                const isBeforeUserStub = c.line < 0;
                return !(isHidden || isMissingSemicolon || isBeforeUserStub);
            });
        return filt;
    }
    
    private cleanupMessage = (text: string) => {
        let fluf = [
            / \(are you missing a using directive or an assembly reference\?\)/
        ];
        let s = text;
        fluf.forEach(r => {
            s = s.replace(r, '');
        });
        return s;
    }

    private action = (name: string) => {
        return `http://localhost:${config.omnisharpPort}/${name}`;
    }
}

class MessageMap {
    public inner: OmnisharpSessionMessage;
    public mapped: Message;
}

class BufferedMessage {
    public edits: OmnisharpSessionMessage[];
    public list: OmnisharpSessionMessage[];
    public ready: boolean;
}
