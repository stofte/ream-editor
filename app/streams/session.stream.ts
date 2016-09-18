import { Injectable } from '@angular/core';
import { Subject } from 'rxjs/Rx';
import { SessionMessage } from '../messages/index';
import * as uuid from 'node-uuid';

@Injectable()
export class SessionStream {
    public events: Subject<SessionMessage>;
    constructor() {
        this.events = new Subject<SessionMessage>();
    }

    public new(): string {
        const id = uuid.v4();
        this.events.next(new SessionMessage('create', id))
        return id;
    }

    public runCode(id: string) {
        this.events.next(new SessionMessage('run-code', id));
    }
}
