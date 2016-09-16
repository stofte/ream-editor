import { Injectable } from '@angular/core';
import { Subject } from 'rxjs/Rx';
import { SessionMessage } from '../messages/index';

@Injectable()
export class SessionStream {
    public events: Subject<SessionMessage>;
    constructor() {
        
    }

    public new(connection: string, serverType: string) {
        
    }
}
