import { Injectable } from '@angular/core';
import { Subject, Observable, Subscription } from 'rxjs/Rx';
import { QueryMessage, ProcessMessage } from '../messages/index';
import { ProcessStream } from '../streams/index';
import { ProcessHelper } from '../utils/process-helper';
import config from '../config';

@Injectable()
export class QueryStream {
    public events: Subject<QueryMessage>;
    constructor(private process: ProcessStream) {
        let helper = new ProcessHelper();
        let cmd = helper.query(config.queryEnginePort);
        this.events = new Subject<QueryMessage>();
        this.process.start(cmd.command, cmd.directory, config.queryEnginePort);
        this.process.status.subscribe(msg => {
            this.events.next(new QueryMessage(msg.type, msg.value));
        });
    }

    public shutdown() {
        this.process.close();
    }
}
