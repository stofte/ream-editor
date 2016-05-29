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
    
    public initMirror(mirror: CodeMirror.Editor) {
        mirror.on('change', (mirror, cs) => {
            this.sub.next(this.mapEvent(cs));
        });
    }
    
    public get stream(): Observable<EditorChange> {
        return this.sub
            .asObservable()
            ;
    }
    
    private mapEvent(val: any): EditorChange {
        return <EditorChange> {
            startLine: val.from.line,
            startColumn: val.from.ch,
            endLine: val.to.line,
            endColumn: val.to.ch,
            newText: val.text.join('\n')
        };
    }
}
