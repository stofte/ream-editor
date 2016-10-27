import { Injectable } from '@angular/core';
import { ReplaySubject, Observable, Subject } from 'rxjs/Rx';
import { Tab } from '../models/tab';
import { Connection } from '../models/connection';
import { ConnectionService } from './connection.service';

@Injectable()
export class TabService {
    public currentSessionId: Observable<string>;
    private subject = new Subject<string>();
    private sessions: Tab[] = [];

    constructor() {
        const stream = this.subject.publishReplay(1);
        this.currentSessionId = stream;
        stream.connect();
    }

    public currentSession(id: string) {
        this.subject.next(id);
    }

    public newSession(id: string) {
        this.sessions.push(<Tab> { id });
        this.subject.next(id);
    }

    public setContext(id: string, conn: Connection) {
        const newTab = conn ? <Tab> { id, connectionId: conn.id }
            : <Tab> { id, connectionId: null };
        this.sessions = this.sessions
            .filter(x => x.id !== id)
            .concat([newTab]);
    }

    public sessionContext(id: string) {
        return this.sessions.find(x => x.id === id).connectionId;
    }
}
