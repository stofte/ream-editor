import { Injectable } from '@angular/core';
import { Subject } from 'rxjs/Rx';
import { SessionMessage } from '../messages/index';

@Injectable()
export class SessionStream {
    public events: Subject<SessionMessage>;
    constructor() {
        
    }

    public createSqlSession(connection: string, serverType: string) {
        
    }

    public createCodeSession() {
        
    }
}
