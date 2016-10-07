import { Injectable } from '@angular/core';
import { Http } from '@angular/http';
import { Observable } from 'rxjs/Rx';
import { Message, EventName } from './api';
import { QueryStream, InputStream, EditorStream, ResultStream, OmnisharpStream } from './index';
import { Connection, TextUpdate, AutocompletionQuery } from '../models/index';

@Injectable()
export class StreamStarter {

    constructor(
        input: InputStream,
        query: QueryStream,
        omnisharp: OmnisharpStream
    ) {
        let queryResolver = null;
        let omnisharpResolver = null;
        const queryPromise = new Promise(done => queryResolver = done);
        const omnisharpPromise = new Promise(done => omnisharpResolver = done);
        query.once(msg => msg.name === EventName.ProcessReady, () => queryResolver());
        omnisharp.once(msg => msg.name === EventName.ProcessReady, () => omnisharpResolver());
        queryPromise.then(() => omnisharpPromise.then(() => input.connect()));
    }
}
