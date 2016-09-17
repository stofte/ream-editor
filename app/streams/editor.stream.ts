import { Injectable } from '@angular/core';
import { Subject } from 'rxjs/Rx';
import { TextUpdate } from '../models/index';
import { EditorMessage } from '../messages/index';

@Injectable()
export class EditorStream {
    public events: Subject<EditorMessage> = new Subject<EditorMessage>();
    constructor() {
        
    }

    public edit(data: TextUpdate) {
        this.events.next(new EditorMessage('edit', data, performance.now()));
    }
}
