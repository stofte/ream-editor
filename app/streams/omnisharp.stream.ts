import { Injectable } from '@angular/core';
import { Http, Response } from '@angular/http';
import { Observable, Observer, Subscription, Subject } from 'rxjs/Rx';
import { QueryMessage, ProcessMessage, WebSocketMessage } from '../messages/index';
import { ProcessStream, EditorStream, UserStream } from './index';
import { ProcessHelper } from '../utils/process-helper';
import { CodeRequest } from './interfaces';
import config from '../config';

@Injectable()
export class OmnisharpStream {
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
                        .map(x => x.json())
                        .subscribe(data => {
                            obs.next(new QueryMessage('run-code-response', req.id, null, {
                                code: data.Code,
                                message: data.Message
                            }));
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
        let cmd = helper.omnisharp(config.omnisharpPort);
        this.process.start('omnisharp', cmd.command, cmd.directory, config.omnisharpPort);
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
        return `http://localhost:${config.omnisharpPort}/${name}`;
    }
}
