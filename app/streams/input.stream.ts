import { Injectable } from '@angular/core';
import { Http } from '@angular/http';
import { Observable, Subject, ConnectableObservable } from 'rxjs/Rx';
import { Message, EventName } from './api';
import { Connection, TextUpdate, AutocompletionQuery } from '../models/index';

@Injectable()
export class InputStream {
    public events: Observable<Message>;
    private subject = new Subject<Message>();
    private resolver: Function;
    private connectable: ConnectableObservable<Message>;

    constructor() {
        const promise = new Promise((done) => this.resolver = done);
        const stream = this.subject.publishReplay();
        this.events = this.connectable = Observable.fromPromise(promise)
            .startWith(false)
            .switchMap(ready => ready ? stream : Observable.empty<Message>()).publish();
        stream.connect();
    }

    public new(id: string, connection: Connection = null) {
        this.subject.next(new Message(EventName.SessionCreate, id, connection));
    }

    public edit(id: string, data: TextUpdate) {
        data.timestamp = performance.now();
        this.subject.next(new Message(EventName.EditorUpdate, id, data));
    }

    public executeBuffer(id: string): number {
        const msg = new Message(EventName.SessionExecuteBuffer, id);
        this.subject.next(msg);
        return msg.timestamp;
    }

    public codeCheck(id: string) {
        const msg = new Message(EventName.SessionCodeCheck, id);
        this.subject.next(msg);
    }

    public autoComplete(id: string, query: AutocompletionQuery) {
        this.subject.next(new Message(EventName.SessionAutocompletion, id, query));
    }

    public setContext(id: string, connection: Connection) {
        this.subject.next(new Message(EventName.SessionContext, id, connection));
    }

    public destroy(id: string) {
        this.subject.next(new Message(EventName.SessionDestroy, id));
    }

    public connect() {
        this.connectable.connect();
        this.resolver(true);
    }
}
