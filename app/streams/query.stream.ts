import { Inject, Injectable, Provider } from '@angular/core';
import { Http, Response } from '@angular/http';
import { Observable, Observer, Subscription, Subject } from 'rxjs/Rx';
import { QueryMessage, ProcessMessage, WebSocketMessage } from '../messages/index';
import { ProcessStream, EditorStream } from './index';
import { SessionStream } from './session.stream';
import { ProcessHelper } from '../utils/process-helper';
import { CodeRequest, CodeTemplateRequest } from './interfaces';
import config from '../config';

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
        const executeCodeResponses = editor.events
            .filter(msg => msg.type === 'run-code-request')
            .map(msg => {
                const mapped: CodeRequest = {
                    id: msg.id,
                    text: msg.text
                };
                return mapped;
            })
            .flatMap(req => 
                this.http
                    .post(this.action('executecode'), JSON.stringify(req))
                    .map(res => {
                        const data = res.json();
                        return new QueryMessage('run-code-response', req.id, null, {
                            code: data.Code,
                            message: data.Message
                        });
                    }))
            .publish();

        const codeTemplateResponses = session.events
            .filter(msg => msg.type === 'create')
            .map(msg => {
                const mapped: CodeTemplateRequest = {
                    text: '',
                    id: msg.id
                };
                return mapped;
            })
            .flatMap(req => 
                this.http.post(this.action('codetemplate'), JSON.stringify(req))
                    .map(res => {
                        const data = res.json();
                        return new QueryMessage('code-template-response', req.id, null, null, {
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
                    }))
            .publish();

        this.events = this.process
            .status
            .map(msg => new QueryMessage(msg.type))
            .merge(this.socket)
            .merge(executeCodeResponses)
            .merge(codeTemplateResponses);

        let helper = new ProcessHelper();
        let cmd = helper.query(config.queryEnginePort);
        this.process.start('query', cmd.command, cmd.directory, config.queryEnginePort);
        this.once(msg => msg.type === 'ready', () =>{
            executeCodeResponses.connect();
            codeTemplateResponses.connect();
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
        // todo make json camelCased from backend
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
