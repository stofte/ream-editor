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
    
    public set(tab: Tab, text: string, isDirective = true) {
        let change = new EditorChange();
        change.newText = text;
        change.tabId = tab.id;
        if (!isDirective) {
            this.changes.next(change);            
        }
        this.buffers[tab.id] = text;
    }
}
