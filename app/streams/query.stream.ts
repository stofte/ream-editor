import { Inject, Injectable, Provider } from '@angular/core';
import { Http, Response } from '@angular/http';
import { Observable, Observer, Subscription, Subject } from 'rxjs/Rx';
import { EventName, Message } from './api';
import { ProcessStream, EditorStream } from './index';
import { InputStream } from './input.stream'; // todo es6 class bs
import { ProcessHelper } from '../utils/process-helper';
import { CodeRequest, CodeTemplateRequest, QueryTemplateRequest,
    TemplateResponse, WebSocketMessage } from './interfaces';
import { TemplateInfo, TemplateCache } from './template-cache';
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
    public events: Observable<Message>;
    private socket: Subject<Message> = new Subject<Message>();
    private process: ProcessStream = null;
    private templateCache = new TemplateCache();
    constructor(
        private editor: EditorStream,
        private input: InputStream,
        private http: Http
    ) {
        this.process = new ProcessStream(http);
        const executeCodeResponses = input.events.filter(msg => msg.name === EventName.SessionCreate)
            .flatMap(sessionMsg => {
                return editor.events
                    .filter(msg => msg.name === EventName.EditorExecuteText && msg.id === sessionMsg.id)
                    .flatMap(msg => {
                        let request: any = null;
                        let actionName: string = null;
                        if (!sessionMsg.data) { // data may be a connection
                            request = { id: msg.id, text: msg.data };
                            actionName = this.action('executecode');
                        } else {
                            actionName = this.action('executequery');
                            request = {
                                id: msg.id,
                                text: msg.data,
                                connectionString: sessionMsg.data.connectionString,
                                serverType: sessionMsg.data.type
                            };
                        }
                        return this.http
                            .post(actionName, request)
                            .map(res => {
                                const data = res.json();
                                return new Message(EventName.QueryExecuteResponse, msg.id, {
                                    code: data.Code,
                                    message: data.Message
                                });
                        });
                    });
            })
            .publish();

        const templateResponses = input.events
            .filter(msg => msg.name === EventName.SessionCreate || msg.name === EventName.SessionContext)
            .flatMap(sessionMsg => {
                const initialMessage = sessionMsg.name === EventName.SessionCreate ? 
                    Observable.from<Message>([new Message(EventName.EditorBufferText, sessionMsg.id, '')]) :
                    editor.bufferedTexts.first(msg => msg.name === EventName.EditorBufferText && msg.id === sessionMsg.id);
                return initialMessage
                    .flatMap(msg => {
                        const initialText = msg.data;
                        const connectionId = sessionMsg.data ? sessionMsg.data.id : null;
                        const cachedTemplated = this.templateCache.query(connectionId, initialText);
                        if (cachedTemplated) {
                            return Observable.from([
                                new Message(EventName.QueryTemplateResponse, sessionMsg.id, <TemplateResponse> {
                                    template: cachedTemplated.template,
                                    columnOffset: cachedTemplated.columnOffset,
                                    lineOffset: cachedTemplated.lineOffset,
                                    connectionId
                                }, sessionMsg.timestamp)
                            ]).delay(100); // todo something nasty
                        } else {
                            let req: any = null;
                            let method: string = null;
                            // inject possible connection info from sessionMsg
                            if (sessionMsg.data) {
                                method = this.action('querytemplate');
                                req = {
                                    text: initialText,
                                    id: sessionMsg.id,
                                    serverType: sessionMsg.data.type,
                                    connectionString: sessionMsg.data.connectionString
                                };
                            } else {
                                method = this.action('codetemplate');
                                req = { text: initialText, id: sessionMsg.id };
                            }
                            return this.http
                                .post(method, JSON.stringify(req))
                                .map(res => {
                                    const data = res.json();
                                    this.templateCache.add(connectionId, data.Template, initialText, data.LineOffset, data.ColumnOffset, data.Namespace);
                                    let x = new Message(EventName.QueryTemplateResponse, req.id, <TemplateResponse> {
                                        code: data.Code,
                                        message: data.Message,
                                        namespace: data.Namespace,
                                        template: data.Template,
                                        header: data.Header,
                                        footer: data.Footer,
                                        columnOffset: data.ColumnOffset,
                                        lineOffset: data.LineOffset,
                                        defaultQuery: data.DefaultQuery,
                                        connectionId
                                    }, sessionMsg.timestamp);
                                    return x;
                                });
                        }
                    });
            })
            .publish();

        this.events = this.process
            .status
            .merge(this.socket)
            .merge(executeCodeResponses)
            .merge(templateResponses);

        executeCodeResponses
            .merge(templateResponses)
            .first()
            .subscribe(x => this.process.confirmedReady());

        executeCodeResponses.connect();
        templateResponses.connect();

        let helper = new ProcessHelper();
        let cmd = helper.query(config.queryEnginePort);
        this.process.start('query', cmd.command, cmd.directory, config.queryEnginePort);
        this.once(msg => msg.name === EventName.ProcessReady, () => {
            Observable.webSocket(`ws://localhost:${config.queryEnginePort}/ws`).subscribe(
                this.socketMessageHandler,
                this.socketErrorHandler,
                this.socketDoneHandler
            );
        });
    }

    public once(pred: (msg: Message) => boolean, handler: (msg: Message) => void) {
        this.events.first(msg => pred(msg)).subscribe(msg => {
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
        this.socket.next(new Message(EventName.QuerySocketOutput, msg.Session, message));
    }

    private socketErrorHandler = (err) => {
        console.error('socket error', err);
    }

    private socketDoneHandler = () => { }
}
