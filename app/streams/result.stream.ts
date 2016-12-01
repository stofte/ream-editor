import { Injectable } from '@angular/core';
import { Observable, Subject, Observer } from 'rxjs/Rx';
import { QueryStream } from './index';
import { InputStream } from './input.stream';
import { EventName, Message } from './api';
import { WebSocketMessage } from './interfaces';
import { ResultPage } from '../models/index';
import * as uuid from 'node-uuid';

@Injectable()
export class ResultStream {
    public events: Observable<Message>;
    constructor(private query: QueryStream, input: InputStream) {
        const stream = input
            .events
            .filter(msg => msg.name === EventName.SessionExecuteBuffer)
            .flatMap(this.handleRunCode)
            .publish();
        this.events = stream;
        stream.connect();
    }

    private handleRunCode = (req: Message): Observable<Message> => {
        return new Observable<Message>((obs: Observer<Message>) => {
            // for tables, we need to gather across multiple messages, so store
            // an optional top level ref for the table
            // todo: this will fail with interleaving table dumps from the same session, index by table id property 
            let tableResult: ResultPage = null;
            // todo make backend emit full objs?
            let rowMapper: Function = null;
            obs.next(new Message(EventName.ResultStart, req.id));
            const socketSub = this.query.events
                .filter(msg => msg.name === EventName.QuerySocketOutput && msg.data.session === req.id)
                .map(msg => msg.data)
                .merge(this.query.events.filter(x =>
                        // if the query fails for some reason, we need to close the stream
                        x.id === req.id && x.name === EventName.QueryExecuteResponse &&
                        x.originalTimestamp === req.timestamp &&
                        // the api only returns errors, so if there's anything here, the query didn't run
                        x.data.diagnostics && x.data.diagnostics.length > 0
                    ).map(x => { return { type: 'close', session: x.id }; }))
                .subscribe((socket: WebSocketMessage) => {
                    let page: ResultPage = null;
                    switch (socket.type) {
                        case 'list':
                            Assert(tableResult == null, 'tableResult was set');
                            // assumes message order is preserved
                            tableResult = {
                                id: socket.session,
                                resultId: uuid.v4(),
                                title: socket.title,
                                columns: null,
                                columnTypes: null,
                                rows: [],
                                isAtomic: false,
                                isTabular: true,
                                viewColumnOffset: 0,
                                viewRowOffset: 0
                            };
                            break;
                        case 'listValues':
                            Assert(tableResult != null, 'tableResult was null');
                            const prevLen = tableResult.rows.length;
                            tableResult.rows = tableResult.rows.concat(socket.values); 
                            break;
                        case 'listClose':
                            obs.next(new Message(EventName.ResultUpdate, socket.session, tableResult));
                            tableResult = null; // allows next table from session
                            break;
                        case 'single':
                            obs.next(new Message(EventName.ResultUpdate, socket.session, this.mapSingleAtomic(socket)));
                            break;
                        case 'close':
                            socketSub.unsubscribe();
                            obs.next(new Message(EventName.ResultDone, socket.session));
                            obs.complete();
                            break;
                    }
                });
        });
    }

    private mapSingleAtomic = (msg: WebSocketMessage): ResultPage => {
        // todo make server json camelCased
        const obj: ResultPage = {
            id: msg.session,
            resultId: uuid.v4(),
            title: msg.title,
            columns: [msg.values[0].Name],
            columnTypes: [msg.values[0].Type],
            rows: [...msg.values],
            isAtomic: true,
            isTabular: false,
            viewColumnOffset: 0,
            viewRowOffset: 0
        };
        return obj;
    }
}
