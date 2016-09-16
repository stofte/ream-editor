import { Injectable } from '@angular/core';
import { Subject } from 'rxjs/Rx';
import { EditorMessage } from '../messages/index';

@Injectable()
export class EditorStream {
    public events: Subject<EditorMessage>;
    constructor() {
        
    }
}
