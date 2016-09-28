import { Injectable } from '@angular/core';
import { Subject, Observable } from 'rxjs/Rx';
import { SessionMessage } from '../messages/index';
import { AutocompletionQuery, Connection } from '../models/index';
import * as uuid from 'node-uuid';

@Injectable()
export class SessionStream {
    public events: Observable<SessionMessage>;
    private subject: Subject<SessionMessage>;

    constructor() {
        this.subject = new Subject<SessionMessage>();
        const stream = this.subject.publish();
        this.events = stream;
        stream.connect();
    }

    public new(id: string, connection: Connection = null) {
        this.subject.next(new SessionMessage('create', id, performance.now(), null, connection));
    }

    public run(id: string) {
        this.subject.next(new SessionMessage('run', id));
    }

    public codeCheck(id: string) {
        const now = performance.now();
        this.subject.next(new SessionMessage('codecheck', id, now));
    }

    public autoComplete(id: string, query: AutocompletionQuery) {
        this.subject.next(new SessionMessage('autocomplete', id, performance.now(), query));
    }

    public setContext(id: string, connection: Connection) {
        this.subject.next(new SessionMessage('context', id, performance.now(), null, connection));
    }
}
