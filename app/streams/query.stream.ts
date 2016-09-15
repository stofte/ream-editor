import { Injectable } from '@angular/core';
import { Subject } from 'rxjs/Rx';
import { QueryMessage } from '../messages/index';
import { ProcessStream } from '../streams/index';
import { ProcessHelper } from '../utils/process-helper';
import config from '../config';

@Injectable()
export class QueryStream {
    public events: Subject<QueryMessage>;
    constructor(private process: ProcessStream) {
        let helper = new ProcessHelper();
        let cmd = helper.query(config.queryEnginePort);
        this.process.start(cmd.command, cmd.directory, config.queryEnginePort);
    }

    public shutdown() {
        this.process.close();
    }
}
