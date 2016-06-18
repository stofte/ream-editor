import { Injectable } from '@angular/core';
import { Subject, Observable } from 'rxjs/Rx';
import { TabService } from './tab.service';
import { Tab, EditorChange, CodeCheckResult } from '../models/index';

@Injectable()
export class EditorService {
    private buffers: any = {};
    public changes: Subject<EditorChange>;
    public rawErrors: Subject<CodeCheckResult>;
    
    constructor(
        private tabs: TabService
    ) {
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
    
    public errors(tabId: number): Observable<CodeCheckResult> {
        return new Observable<CodeCheckResult>(obs => {
            setTimeout(function() {
                obs.next({
                    messages: []
                });
            }, 10000);
        });
    }
}
