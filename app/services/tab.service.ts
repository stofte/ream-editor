import { Injectable, EventEmitter } from '@angular/core';
import { ReplaySubject, Observable, Subject } from 'rxjs/Rx';
import { Tab, ResultPage, SessionLogMessage } from '../models/index';
import { Connection } from '../models/connection';
import { ConnectionService } from './connection.service';
import { InputStream, OutputStream, EventName } from '../streams/index';

import * as uuid from 'node-uuid';

@Injectable()
export class TabService {
    public tabDragging = new EventEmitter<boolean>();
    public logUpdated = new EventEmitter<string>();
    public tabResultsUpdated = new EventEmitter<string>();
    public currentSessionId: Observable<string>;
    private subject = new Subject<string>();
    public sessions: Tab[] = [];
    private currentId: string;
    private history: string[] = [];
    private tabCounter = 0;

    constructor(
        private input: InputStream,
        private output: OutputStream
    ) {
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
            executePending: false,
            results: [],
            sessionLog: []
        });
        this.history = [id].concat(this.history);
        this.currentId = id;
        this.input.new(id);
        this.subject.next(id);
        this.output.events
            .filter(x => x.name === EventName.QueryExecuteResponse)
            .subscribe(msg => {
                if (msg.data && msg.data.diagnostics && msg.data.diagnostics.length > 0) {
                    msg.data.diagnostics.forEach(diag => {
                        this.sessionLog(msg.id, diag.Message, true);
                    });
                    this.sessionLog(msg.id, 'Query errors:', true);
                }
            });
        this.output.events
            .filter(x => x.id === id && (
                x.name === EventName.ResultStart ||
                x.name === EventName.ResultUpdate ||
                x.name === EventName.ResultDone
            ))
            .takeUntil(this.input.events
                .first(x => x.id === id && x.name === EventName.SessionDestroy))
            .subscribe(res => {
                const tab = this.sessions.find(x => x.id === id);
                if (tab) {
                    if (res.name === EventName.ResultStart) {
                        tab.activeResultId = null;
                        tab.results.splice(0);
                    } else if (res.name === EventName.ResultUpdate) {
                        tab.results.push(<ResultPage> res.data);
                    }
                    this.tabResultsUpdated.emit(id);
                }
            });
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
        const session = this.sessions.find(x => x.id === id);
        Assert(session, 'setContext on unknown session');
        session.connectionId = conn ? conn.id : null;
        this.sessions = this.sessions
            .filter(x => x.id !== id)
            .concat([session]);
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

    public setResultPageView(id: string, resultId: string, colOffset: number, rowOffset: number) {
        const session = this.sessions.find(x => x.id === id);
        const page = session.results.find(x => x.resultId === resultId);
        page.viewColumnOffset = colOffset;
        page.viewRowOffset = rowOffset;
    }
    
    public setActiveResult(id: string, resultId: string) {
        const session = this.sessions.find(x => x.id === id);
        session.activeResultId = resultId;
        session.consoleActive = false;
    }
    
    public setConsoleActive(id: string) {
        const session = this.sessions.find(x => x.id === id);
        session.consoleActive = true;
        session.activeResultId = null;
    }

    public sessionContext(id: string) {
        return this.sessions.find(x => x.id === id).connectionId;
    }

    public sessionExecutePending(id: string) {
        return this.sessions.find(x => x.id === id).executePending;
    }

    public sessionLog(id: string, text: string, isError = false) {
        const tab = this.sessions.find(x => x.id === id);
        tab.sessionLog.unshift({
            text,
            type: isError ? 'error' : 'info',
            created: new Date()
        });
        this.logUpdated.emit(id);
    }
}
