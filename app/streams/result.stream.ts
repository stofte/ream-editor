import { Injectable } from '@angular/core';
import { Observable, Subject, Observer } from 'rxjs/Rx';
import { QueryStream } from './index';
import { InputStream } from './input.stream';
import { EventName, Message } from './api';
import { WebSocketMessage } from './interfaces';
import { ResultPage } from '../models/index';

@Injectable()
export class ResultStream {
    public events: Observable<Message>;
    constructor(private query: QueryStream, input: InputStream) {
        this.events = input
            .events
            .filter(msg => msg.name === EventName.SessionExecuteBuffer)
            .flatMap(this.handleRunCode);
    }

    private handleRunCode = (req: Message): Observable<Message> => {
        return new Observable<Message>((obs: Observer<Message>) => {
            // for tables, we need to gather across multiple messages, so store
            // an optional top level ref for the table
            // todo: this will fail with interleaving table dumps from the same session, index by table id property 
            let tableResult: ResultPage = null;
            obs.next(new Message(EventName.ResultStart, req.id));
            // this sub is dependents on the close msg to complete.
            const socketSub = this.query.events
                .filter(msg => msg.name === EventName.QuerySocketOutput && msg.data.session === req.id)
                .map(msg => msg.data)
                .subscribe(socket => {
                    let page: ResultPage = null;
                    switch (socket.type) {
                        case 'table':
                            Assert(tableResult == null, 'tableResult was set');
                            // assumes message order is preserved
                            tableResult = {
                                id: socket.session,
                                title: socket.values[0],
                                columns: null,
                                columnTypes: null,
                                rows: [],
                                isAtomic: false,
                                isTabular: true
                            };
                            break;
                        case 'header': 
                            Assert(tableResult != null, 'tableResult was null');
                            tableResult.columns = socket.values.map(x => x.Name);
                            tableResult.columnTypes = socket.values.map(x => x.Type);
                            break;
                        case 'row':
                            Assert(tableResult != null, 'tableResult was null');
                            tableResult.rows.push(socket.values);
                            break;
                        case 'tableClose':
                            obs.next(new Message(EventName.ResultUpdate, socket.session, tableResult));
                            tableResult = null; // allows next table from session
                            break;
                        case 'singleAtomic':
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
            title: msg.values[0].Name,
            columns: [msg.values[0].Name],
            columnTypes: [msg.values[0].Type],
            rows: [msg.values[1]],
            isAtomic: true,
            isTabular: false
        };
        return obj;
    }
}
