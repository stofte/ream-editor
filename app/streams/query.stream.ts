import { Injectable } from '@angular/core';
import { Observable, Subscription } from 'rxjs/Rx';
import { QueryMessage, ProcessMessage } from '../messages/index';
import { ProcessStream } from '../streams/index';
import { ProcessHelper } from '../utils/process-helper';
import config from '../config';

@Injectable()
export class QueryStream {
    public events: Observable<QueryMessage>;
    constructor(private process: ProcessStream) {
        let helper = new ProcessHelper();
        let cmd = helper.query(config.queryEnginePort);
        this.process.start(cmd.command, cmd.directory, config.queryEnginePort);
        this.events = this.process.status.map(msg => new QueryMessage(msg.type, msg.value));
    }

    public stopServer() {
        this.process.close();
    }
}
