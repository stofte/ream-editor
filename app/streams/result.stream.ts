import { Injectable } from '@angular/core';
import { Observable, Subject, Observer } from 'rxjs/Rx';
import { QueryStream, SessionStream } from './index';
import { ResultMessage, WebSocketMessage, SessionMessage } from '../messages/index';
import { ResultPage } from '../models/index';

@Injectable()
export class ResultStream {
    public events: Observable<ResultMessage>;
    constructor(private query: QueryStream, session: SessionStream) {
        this.events = session
            .events
            .filter(msg => msg.type === 'run-code')
            .flatMap(this.handleRunCode);
    }

    private handleRunCode = (req: SessionMessage): Observable<ResultMessage> => {
        return new Observable<ResultMessage>((obs: Observer<ResultMessage>) => {
            obs.next(new ResultMessage('start', req.id));
            this.query.events
                .filter(msg => msg.type === 'message' && msg.socket.session === req.id)
                .map(msg => msg.socket)
                .subscribe(socket => {
                    let msg: ResultMessage = null;
                    let page: ResultPage = null;
                    switch (socket.type) {
                        case 'singleAtomic':
                            obs.next(new ResultMessage('update', socket.session, this.mapSingleAtomic(socket)));
                            break;
                        case 'close':
                            obs.next(new ResultMessage('done', socket.session));
                            obs.complete();
                            break;
                    }
                });
        });
    }

    private mapSingleAtomic = (msg: WebSocketMessage): ResultPage => {
        // todo make server json camelCased
        const obj: ResultPage= {
            id: msg.session,
            title: msg.values[0].Name,
            columns: [msg.values[0].Name],
            columnTypes: [msg.values[0].Type],
            rows: [msg.values[1]]
        };
        return obj;
    }
}
