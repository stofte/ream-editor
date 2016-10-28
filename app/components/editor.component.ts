import { Component, AfterViewInit, ElementRef } from '@angular/core';
import { TabService } from '../services/index';
import * as CodeMirror from 'codemirror';

@Component({
    selector: 'rm-editor',
    template: `
<div class="rm-editor">
    <textarea></textarea>
</div>
`
})
export class EditorComponent implements AfterViewInit {
    documents: any = {};
    sessionId: string;
    mirror: any;
    constructor(
        private tabs: TabService,
        private elm: ElementRef
    ) {
    }
    ngAfterViewInit() {
        const txtElm = this.elm.nativeElement.querySelector('textarea');
        this.mirror = CodeMirror.fromTextArea(txtElm, this.editorOptions());
        this.tabs.currentSessionId.subscribe(id => {
            let doc = null;
            if (!this.documents[id]) {
                doc = CodeMirror.Doc('');
                this.documents[id] = doc;
            }
            doc = this.documents[id];
            const oldDoc = this.mirror.swapDoc(doc);
            this.documents[this.sessionId] = oldDoc;
            this.sessionId = id;
        });
    }

    private editorOptions() {
        return {
            lineNumbers: false,
            gutters: ['CodeMirror-lint-markers'],
            lint: true,
            smartIndent: false,
            matchBrackets: true,
            viewportMargin: Infinity,
            showCursorWhenSelecting: true,
            mode: 'text/x-csharp'
        };
    }
}
