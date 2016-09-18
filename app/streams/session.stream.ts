import { Injectable } from '@angular/core';
import { Subject } from 'rxjs/Rx';
import { SessionMessage } from '../messages/index';
import * as uuid from 'node-uuid';

@Injectable()
export class SessionStream {
    public events: Subject<SessionMessage> = new Subject<SessionMessage>();

    public new(id: string) {
        this.events.next(new SessionMessage('create', id));
    }

    public runCode(id: string) {
        this.events.next(new SessionMessage('run-code', id));
    }
}
