import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs/Rx';
import { UserMessage } from '../messages/index';
import { EditorEvent } from '../models/index';

@Injectable()
export class UserStream {
    public events: Subject<UserMessage> = new Subject<UserMessage>();
    constructor() {
    }
}
