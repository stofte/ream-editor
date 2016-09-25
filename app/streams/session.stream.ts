import { Injectable } from '@angular/core';
import { Subject, Observable } from 'rxjs/Rx';
import { SessionMessage } from '../messages/index';
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

    public new(id: string) {
        this.subject.next(new SessionMessage('create', id));
    }

    // todo this needs to be a simple run method, and not code specific. same with msg stuff
    public runCode(id: string) {
        this.subject.next(new SessionMessage('run-code', id));
    }

    public codeCheck(id: string) {
        const now = performance.now();
        this.subject.next(new SessionMessage('codecheck', id, now));
    }
}
