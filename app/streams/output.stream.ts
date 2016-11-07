import { Injectable } from '@angular/core';
import { Http } from '@angular/http';
import { Observable, Subject } from 'rxjs/Rx';
import { Message, EventName } from './api';
import { QueryStream, EditorStream, ResultStream, OmnisharpStream } from './index';
import { Connection, TextUpdate, AutocompletionQuery } from '../models/index';

@Injectable()
export class OutputStream {
    public events: Observable<Message>;
    private subject = new Subject<Message>();

    constructor(
        result: ResultStream,
        query: QueryStream,
        omnisharp: OmnisharpStream
    ) {
        this.events = result.events
            .merge(omnisharp.events)
            .merge(query.events);
    }
}
