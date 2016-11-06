import { Injectable } from '@angular/core';
import { ReplaySubject, Observable, Subject } from 'rxjs/Rx';
import { Tab } from '../models/tab';
import { Connection } from '../models/connection';
import { ConnectionService } from './connection.service';
import { InputStream } from '../streams/index';
import * as uuid from 'node-uuid';

@Injectable()
export class TabService {
    public currentSessionId: Observable<string>;
    private subject = new Subject<string>();
    public sessions: Tab[] = [];
    private currentId: string;
    private history: string[] = [];
    private tabCounter = 0;

    constructor(private input: InputStream) {
        const stream = this.subject.publishReplay(1);
        this.currentSessionId = stream;
        stream.connect();
    }

    public currentSession(id: string) {
        if (this.currentId !== id) {
            this.history = [id].concat(this.history.filter(x => x !== id));
            this.currentId = id;
            this.subject.next(id);
        }
    }

    public newSession(): string {
        const id = uuid.v4();
        this.sessions.push(<Tab> {
            id, 
            title: `Untitled ${this.tabCounter++}`,
            editorHeight: (150 + 65),
            executePending: false
        });
        this.history = [id].concat(this.history);
        this.currentId = id;
        this.input.new(id);
        this.subject.next(id);
        return id;
    }

    public closeSession(id: string) {
        this.sessions = this.sessions.filter(x => x.id !== id);
        this.history = this.history.filter(x => x !== id);
        this.input.destroy(id);
        if (this.currentId === id) {
            this.currentId = null;
            if (this.sessions.length > 0) {
                const nextId = this.history.shift();
                this.currentId = nextId;
                this.subject.next(nextId);
            } else {
                this.subject.next(null);
            }
        } // else inactive tab, so nothing to announce
    }

    public setContext(id: string, conn: Connection) {
        const editorHeight = this.sessions.find(x => x.id === id).editorHeight;
        const newTab = conn ? <Tab> { id, connectionId: conn.id, editorHeight }
            : <Tab> { id, connectionId: null, editorHeight };
        this.sessions = this.sessions
            .filter(x => x.id !== id)
            .concat([newTab]);
        this.input.setContext(id, conn);
    }

    public setEditorHeight(id: string, height: number) {
        const session = this.sessions.find(x => x.id === id);
        session.editorHeight = height;
    }

    public setExecutePending(id: string, pending: boolean) {
        const session = this.sessions.find(x => x.id === id);
        session.executePending = pending;
    }

    public sessionContext(id: string) {
        return this.sessions.find(x => x.id === id).connectionId;
    }

    public sessionExecutePending(id: string) {
        return this.sessions.find(x => x.id === id).executePending;
    }
}
