import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs/Rx';
import { EditorEvent } from '../models/index';

@Injectable()
export class UserStream {
    private events: Subject<EditorEvent>;
    public editor: Observable<EditorEvent>;

    constructor() {
        this.events = new Subject<EditorEvent>();
        this.editor = this.events
            .asObservable();
    }

    public editorEvent(evt: EditorEvent): void {
        this.events.next(evt);
    }
}
