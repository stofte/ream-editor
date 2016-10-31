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
    // the backend uses a single context per connection, so cannot
    // handle concurrent queries on the same connection.
    // layout { connectionId : lockStatus }
    private contextLock = {};
    // Promise resolvers awaiting the context
    // layout { connectionId : PromiseResolveFunc[] }
    private contextQueue = {};
    constructor(
        private editor: EditorStream,
        private input: InputStream,
        private http: Http
    ) {
        this.process = new ProcessStream(http);
        const executeCodeResponses = input.events.filter(msg => msg.name === EventName.SessionCreate)
            .flatMap(sessionMsg => {
                const dstrStream = input.events
                    .filter(msg => msg.id === sessionMsg.id && msg.name === EventName.SessionDestroy);
                const ctxStream = input.events
                    .filter(msg => msg.id === sessionMsg.id && msg.name === EventName.SessionContext)
                    .startWith(sessionMsg)
                    .takeUntil(dstrStream);
                return editor.events
                    .filter(msg => msg.name === EventName.EditorExecuteText && msg.id === sessionMsg.id)
                    .withLatestFrom(ctxStream)
                    .delayWhen(([msg, ctx]) => {
                        // data is the connection, if any, of the query
                        if (ctx.data && ctx.data.id) {
                            return Observable.fromPromise(this.obtainContext(ctx.data.id, msg.id));
                        } else {
                            return Observable.from([1]);
                        }
                    })
                    .flatMap(([msg, ctx]) => {
                        let request: any = null;
                        let actionName: string = null;
                        if (!ctx.data) {
                            request = { id: msg.id, text: msg.data };
                            actionName = this.action('executecode');
                        } else {
                            actionName = this.action('executequery');
                            request = {
                                id: msg.id,
                                text: msg.data,
                                connectionString: ctx.data.connectionString,
                                serverType: ctx.data.type
                            };
                        }
                        return this.http
                            .post(actionName, request)
                            .map(res => {
                                const data = res.json();
                                // use same guard as in the delayWhen, to ensure we release the context
                                if (ctx.data && ctx.data.id) {
                                    this.releaseContext(ctx.data.id, msg.id);
                                }
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
                                    this.templateCache.add(
                                        connectionId,
                                        data.Template,
                                        initialText,
                                        data.LineOffset,
                                        data.ColumnOffset,
                                        data.Namespace
                                    );
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

    private obtainContext(connectionId: number, sessionId: string): Promise<boolean> {
        if (!this.contextQueue[connectionId]) {
            this.contextQueue[connectionId] = [];
        }
        if (!this.contextLock[connectionId]) {
            this.contextLock[connectionId] = true;
            return new Promise<boolean>(done => done(true));
        } else {
            let resolver = null;
            const p = new Promise<boolean>((done) => resolver = done);
            this.contextQueue[connectionId].push(resolver);
            return p;
        }
    }

    private releaseContext(connectionId: number, sessionId: string) {
        if (this.contextQueue[connectionId].length > 0) {
            const [fn] = this.contextQueue[connectionId].splice(0, 1);
            fn(true);
        } else {
            this.contextLock[connectionId] = false;
        }
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
        // console.log('socket', msg.Session, msg.Type, msg.Values);
        this.socket.next(new Message(EventName.QuerySocketOutput, msg.Session, message));
    }

    private socketErrorHandler = (err) => {
        console.error('socket error', err);
    }

    private socketDoneHandler = () => { }
}
