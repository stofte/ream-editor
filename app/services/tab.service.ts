import { Injectable } from '@angular/core';
import { ReplaySubject, Observable, Subject } from 'rxjs/Rx';
import { Tab } from '../models/tab';
import { Connection } from '../models/connection';
import { ConnectionService } from './connection.service';
import { InputStream } from '../streams/index';

@Injectable()
export class TabService {
    public currentSessionId: Observable<string>;
    private subject = new Subject<string>();
    private sessions: Tab[] = [];

    constructor(private input: InputStream) {
        const stream = this.subject.publishReplay(1);
        this.currentSessionId = stream;
        stream.connect();
    }

    public currentSession(id: string) {
        this.subject.next(id);
    }

    public newSession(id: string) {
        this.sessions.push(<Tab> { id });
        this.input.new(id);
        this.subject.next(id);
    }

    public deleteSession(id: string) {
        this.sessions = this.sessions.filter(x => x.id !== id);
        this.input.destroy(id);
    }

    public setContext(id: string, conn: Connection) {
        const newTab = conn ? <Tab> { id, connectionId: conn.id }
            : <Tab> { id, connectionId: null };
        this.sessions = this.sessions
            .filter(x => x.id !== id)
            .concat([newTab]);
        this.input.setContext(id, conn);
    }

    public sessionContext(id: string) {
        return this.sessions.find(x => x.id === id).connectionId;
    }
}
