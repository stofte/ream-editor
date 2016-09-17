import { Injectable } from '@angular/core';
import { Http, Response } from '@angular/http';
import { Observable, Subscription } from 'rxjs/Rx';
import { QueryMessage, ProcessMessage } from '../messages/index';
import { ProcessStream } from '../streams/index';
import { ProcessHelper } from '../utils/process-helper';
import { CodeRequest } from './interfaces';
import config from '../config';

@Injectable()
export class QueryStream {
    public events: Observable<QueryMessage>;
    constructor(private process: ProcessStream, private http: Http) {
        let helper = new ProcessHelper();
        let cmd = helper.query(config.queryEnginePort);
        this.events = this.process
            .status
            .map(msg => new QueryMessage(msg.type, msg.value));
        this.process.start('query', cmd.command, cmd.directory, config.queryEnginePort);
        const statusSub = this.events.subscribe(msg => {
            if (msg.type === 'ready') {
                const ws = Observable.webSocket(`ws://localhost:${config.queryEnginePort}/ws`);
                ws.next({ foo: 42 });
                ws.subscribe(msg => {
                    console.log('ws:msg', msg);
                }, err => {
                    console.log('ws:err', err);
                }, () => {
                    console.log('ws:done');
                });
                statusSub.unsubscribe();
            }
        });
    }

    public stopServer() {
        this.process.close();
    }

    public executeCode(request: CodeRequest) : Observable<Response> {
        return this.http.post(this.action('executecode'), request);
    }

    private action(name: string) {
        return `http://localhost:${config.queryEnginePort}/${name}`;
    }
}
