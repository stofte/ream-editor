import { Injectable } from '@angular/core';
import { EditorChange } from '../models/editor-change';
import { BufferNameService } from './buffer-name.service';
import * as CodeMirror from 'codemirror';
import { Observable, Subject } from 'rxjs/Rx';

@Injectable()
export class MirrorChangeStream {
    private sub = new Subject<EditorChange>();
    
    constructor(
        private bufferNames: BufferNameService
    ) {
        
    }
    
    public initMirror(mirror: CodeMirror.Editor, tabId: number) {
        mirror.on('change', (mirror, cs) => {
            this.sub.next(this.mapEvent(cs, tabId));
        });
    }
    
    public get stream(): Observable<EditorChange> {
        return this.sub
            .asObservable()
            .withLatestFrom(this.bufferNames.byTabId, (change, names) => {
                change.fileName = names.get(change.tabId);
                return change;
            });
    }
    
    private mapEvent(val: any, tabId: number): EditorChange {
        return <EditorChange> {
            tabId,
            startLine: val.from.line,
            startColumn: val.from.ch,
            endLine: val.to.line,
            endColumn: val.to.ch,
            newText: val.text.join('\n')
        };
    }
}
