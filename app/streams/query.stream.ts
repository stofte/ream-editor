import { Injectable } from '@angular/core';
import { Http, Response } from '@angular/http';
import { Observable, Observer, Subscription, Subject } from 'rxjs/Rx';
import { QueryMessage, ProcessMessage, WebSocketMessage } from '../messages/index';
import { ProcessStream, EditorStream, UserStream } from './index';
import { ProcessHelper } from '../utils/process-helper';
import { CodeRequest } from './interfaces';
import config from '../config';

@Injectable()
export class QueryStream {
    public events: Observable<QueryMessage>;
    private socket: Subject<QueryMessage> = new Subject<QueryMessage>();
    constructor(
        private process: ProcessStream,
        private editor: EditorStream,
        private http: Http
    ) {
        const responses = editor.events
            .filter(msg => msg.type === 'run-code-request')
            .map(msg => {
                const mapped: CodeRequest = {
                    id: msg.id,
                    text: msg.text
                };
                return mapped;
            })
            .flatMap(req => {
                return new Observable<QueryMessage>((obs: Observer<QueryMessage>) => {
                    this.http.post(this.action('executecode'), JSON.stringify(req))
                        .subscribe(data => {
                            obs.next(new QueryMessage('run-code-response'));
                            obs.complete();
                        });
                });
            })
            .publish();

        this.events = this.process
            .status
            .map(msg => new QueryMessage(msg.type))
            .merge(this.socket)
            .merge(responses);

        let helper = new ProcessHelper();
        let cmd = helper.query(config.queryEnginePort);
        this.process.start('query', cmd.command, cmd.directory, config.queryEnginePort);
        const statusSub = this.events.subscribe(msg => {
            if (msg.type === 'ready') {
                responses.connect();
                statusSub.unsubscribe();
                Observable.webSocket(`ws://localhost:${config.queryEnginePort}/ws`).subscribe(
                    this.socketMessageHandler,
                    this.socketErrorHandler,
                    this.socketDoneHandler
                );
            }
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

    public executeCode(request: CodeRequest): Observable<Response> {
        return this.http.post(this.action('executecode'), request);
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
