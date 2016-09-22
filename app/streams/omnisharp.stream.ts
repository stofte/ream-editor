import { Injectable } from '@angular/core';
import { Http, Response } from '@angular/http';
import { Observable, Observer, Subscription, Subject } from 'rxjs/Rx';
import { QueryMessage, OmnisharpMessage, WebSocketMessage } from '../messages/index';
import { ProcessStream, EditorStream, QueryStream } from './index';
import { ProcessHelper } from '../utils/process-helper';
import { CodeRequest } from './interfaces';
import { CodeCheckResult } from '../models/index';
import config from '../config';

@Injectable()
export class OmnisharpStream {
    public events: Observable<OmnisharpMessage>;
    private process: ProcessStream;
    constructor(
        private editor: EditorStream,
        private query: QueryStream,
        private http: Http
    ) {
        this.process = new ProcessStream(http);
        this.events = this.process
            .status
            .map(msg => new OmnisharpMessage(msg.type));

        let helper = new ProcessHelper();
        let cmd = helper.omnisharp(config.omnisharpPort);
        this.process.start('omnisharp', cmd.command, cmd.directory, config.omnisharpPort);
    }

    public once(pred: (msg: OmnisharpMessage) => boolean, handler: (msg: OmnisharpMessage) => void) {
        const sub = this.events.filter(msg => pred(msg)).subscribe(msg => {
            sub.unsubscribe();
            handler(msg);
        });
    }

    public stopServer() {
        this.process.close();
    }
    
    private filterCodeChecks(checks: CodeCheckResult[]): CodeCheckResult[] {
        let filt = checks
            .filter(c => {
                const isMissingSemicolon = c.text === '; expected';
                const isHidden = c.logLevel === 'Hidden';
                return !(isHidden || isMissingSemicolon);
            });
        return filt;
    }

    private action(name: string) {
        return `http://localhost:${config.omnisharpPort}/${name}`;
    }
}
