import { Injectable } from '@angular/core';
import { Subject, Observable } from 'rxjs/Rx';
import { EventName, Message } from './api';
import { AutocompletionQuery, Connection } from '../models/index';
import * as uuid from 'node-uuid';

@Injectable()
export class SessionStream {
    public events: Observable<Message>;
    private subject: Subject<Message>;

    constructor() {
        this.subject = new Subject<Message>();
        const stream = this.subject.publish();
        this.events = stream;
        stream.connect();
    }

    public new(id: string, connection: Connection = null) {
        this.subject.next(new Message(EventName.SessionCreate, id, connection));
    }

    public executeBuffer(id: string) {
        this.subject.next(new Message(EventName.SessionExecuteBuffer, id));
    }

    public codeCheck(id: string) {
        this.subject.next(new Message(EventName.SessionCodeCheck, id));
    }

    public autoComplete(id: string, query: AutocompletionQuery) {
        this.subject.next(new Message(EventName.SessionAutocompletion, id, query));
    }

    public setContext(id: string, connection: Connection) {
        this.subject.next(new Message(EventName.SessionContext, id, connection));
    }
}
