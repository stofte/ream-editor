import { Injectable } from '@angular/core';
import { Http, Response } from '@angular/http';
import { Observable, Observer, Subscription, Subject } from 'rxjs/Rx';
import { QueryMessage, OmnisharpMessage, WebSocketMessage } from '../messages/index';
import { ProcessStream, EditorStream, QueryStream, SessionStream } from './index';
import { ProcessHelper } from '../utils/process-helper';
import { CodeRequest, UpdateBufferRequest } from './interfaces';
import { CodeCheckResult } from '../models/index';
import * as uuid from 'node-uuid';
import config from '../config';

@Injectable()
export class OmnisharpStream {
    public events: Observable<OmnisharpMessage>;
    private process: ProcessStream;
    constructor(
        private editor: EditorStream,
        private query: QueryStream,
        private session: SessionStream,
        private http: Http
    ) {
        this.process = new ProcessStream(http);
        const bufferCreated = query.events
            .filter(msg => msg.type === 'code-template-response')
            .map(msg => {
                const mapped: UpdateBufferRequest = {
                    SessionId: msg.id,
                    FileName: `${config.omnisharpProjectPath}/b${msg.id.replace(/\-/g, '')}.cs`,
                    FromDisk: false,
                    Buffer: msg.codeTemplate.template
                };
                return mapped;
            })
            .flatMap(req => {
                return new Observable<OmnisharpMessage>((obs: Observer<OmnisharpMessage>) => {
                    this.http.post(this.action('updatebuffer'), JSON.stringify(req))
                        .map(x => x.json())
                        .subscribe(data => {
                            console.log('buffer-created', data)
                            obs.next(new OmnisharpMessage('buffer-created', req.SessionId, null, null, null));
                            obs.complete();
                        });
                });
            })
            .publish();

        const bufferUpdated = session.events
            .filter(msg => msg.type === 'create')
            .delayWhen(msg => bufferCreated.filter(x => x.sessionId === msg.id))
            .subscribe(x => {
                console.log('bufferUpdated.subscribe', x);
            });
        
        this.events = this.process
            .status
            .map(msg => new OmnisharpMessage(msg.type))
            .merge(bufferCreated);

        let helper = new ProcessHelper();
        let cmd = helper.omnisharp(config.omnisharpPort);
        this.process.start('omnisharp', cmd.command, cmd.directory, config.omnisharpPort);
        this.once(msg => msg.type === 'ready', () => {
            bufferCreated.connect();
        });
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
