import { Injectable } from '@angular/core';
import { ReplaySubject, Observable, Subject } from 'rxjs/Rx';
import { Tab } from '../models/tab';
import { Connection } from '../models/connection';
import { ConnectionService } from './connection.service';

@Injectable()
export class TabService {
    public currentSessionId: Observable<string>;
    private subject = new Subject<string>();

    constructor() {
        const stream = this.subject.publishReplay(1);
        this.currentSessionId = stream;
        stream.connect();
    }

    public currentSession(id: string) {
        this.subject.next(id);
    }

    public newSession(id: string) {
        this.subject.next(id);
    }
}
