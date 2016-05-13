import { Injectable } from '@angular/core';
import { EditorChange } from '../models/editor-change';
import { Tab } from '../models/tab';
import { Subject } from 'rxjs/Rx';

@Injectable()
export class EditorService {
    private buffers: any = {};
    public changes: Subject<EditorChange>;
    
    constructor() {
        this.changes = new Subject<EditorChange>(); 
    }
    
    public get(tab: Tab): string {
        return this.buffers[tab.id] || '\n\n';
    }
    
    public set(tab: Tab, text: string) {
        let change = new EditorChange();
        change.startColumn = 1;
        change.startLine = 1;
        change.endColumn = text.length;
        change.endLine = 1;
        change.fileName = tab.fileName;
        change.newText = text;
        this.changes.next(change);
        this.buffers[tab.id] = text;
    }
}
