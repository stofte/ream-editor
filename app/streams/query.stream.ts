import { Inject, Injectable, Provider } from '@angular/core';
import { Http, Response } from '@angular/http';
import { Observable, Observer, Subscription, Subject } from 'rxjs/Rx';
import { QueryMessage, ProcessMessage, WebSocketMessage } from '../messages/index';
import { ProcessStream, EditorStream } from './index';
import { SessionStream } from './session.stream';
import { ProcessHelper } from '../utils/process-helper';
import { CodeRequest, CodeTemplateRequest, QueryTemplateRequest, TemplateResponse } from './interfaces';
import config from '../config';

class TemplateRequest {
    constructor(
        public sessionId: string,
        public codeTemplate: CodeTemplateRequest = null,
        public queryTemplate: QueryTemplateRequest = null,
        public template: TemplateResponse = null
    ) {}
}

@Injectable()
export class QueryStream {
    public events: Observable<QueryMessage>;
    private socket: Subject<QueryMessage> = new Subject<QueryMessage>();
    private process: ProcessStream = null;
    constructor(
        private editor: EditorStream,
        private session: SessionStream,
        private http: Http
    ) {
        this.process = new ProcessStream(http);
        const executeCodeResponses = session.events.filter(msg => msg.type === 'create')
            .flatMap(sessionMsg => {
                return editor.events
                    .filter(msg => msg.type === 'buffer-text' && msg.id === sessionMsg.id)
                    .flatMap(msg => {
                        let request: any = null;
                        let actionName: string = null;
                        if (!sessionMsg.connection) {
                            request = { id: msg.id, text: msg.text };
                            actionName = this.action('executecode');
                        } else {
                            actionName = this.action('executequery');
                            request = {
                                id: msg.id,
                                text: msg.text,
                                connectionString: sessionMsg.connection.connectionString,
                                serverType: sessionMsg.connection.type
                            };
                        }
                        return this.http
                            .post(actionName, request)
                            .map(res => {
                                const data = res.json();
                                return new QueryMessage('execute-response', msg.id, null, {
                                    code: data.Code,
                                    message: data.Message
                                });
                        });
                    });
            })
            .publish();

        const templateResponses = session.events
            .filter(msg => msg.type === 'create')
            .flatMap(msg => {
                let req: any = null;
                let method: string = null;
                if (msg.connection) {
                    method = this.action('querytemplate');
                    req = { text: '', id: msg.id, serverType: msg.connection.type, connectionString: msg.connection.connectionString };
                } else {
                    method = this.action('codetemplate');
                    req = { text: '', id: msg.id };
                }
                return this.http.post(method, JSON.stringify(req))
                    .map(res => {
                        const data = res.json();
                        return new QueryMessage('buffer-template', req.id, null, null, {
                            code: data.Code,
                            message: data.Message,
                            namespace: data.Namespace,
                            template: data.Template,
                            header: data.Header,
                            footer: data.Footer,
                            columnOffset: data.ColumnOffset,
                            lineOffset: data.LineOffset,
                            defaultQuery: data.DefaultQuery
                        });
                    })
            })
            .publish();

        this.events = this.process
            .status
            .map(msg => new QueryMessage(msg.type))
            .merge(this.socket)
            .merge(executeCodeResponses)
            .merge(templateResponses);

        let helper = new ProcessHelper();
        let cmd = helper.query(config.queryEnginePort);
        this.process.start('query', cmd.command, cmd.directory, config.queryEnginePort);
        this.once(msg => msg.type === 'ready', () => {
            executeCodeResponses.connect();
            templateResponses.connect();
            Observable.webSocket(`ws://localhost:${config.queryEnginePort}/ws`).subscribe(
                this.socketMessageHandler,
                this.socketErrorHandler,
                this.socketDoneHandler
            );
        });
    }

    public once(pred: (msg: QueryMessage) => boolean, handler: (msg: QueryMessage) => void) {
        const sub = this.events.filter(msg => pred(msg)).subscribe(msg => {
            sub.unsubscribe();
            handler(msg);
        });
    }

    public stopServer() {
        this.process.close();
    }

    private action(name: string) {
        return `http://localhost:${config.queryEnginePort}/${name}`;
    }

    private socketMessageHandler = (msg: any) => {
        const message: WebSocketMessage = {
            session: msg.Session,
            id: msg.Id,
            parent: msg.Parent,
            type: msg.Type.substring(0, 1).toLowerCase() + msg.Type.substring(1),
            values: msg.Values
        };
        this.socket.next(new QueryMessage('message', msg.Session, message));
    }

    private socketErrorHandler = (err) => {
        console.error('socket error', err);
    }

    private socketDoneHandler = () => { }
}
