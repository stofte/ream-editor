import { Component, AfterViewInit, ElementRef } from '@angular/core';
import { TabService } from '../services/index';
import { InputStream } from '../streams/index';
import { TextUpdate } from '../models/index';
import * as CodeMirror from 'codemirror';
import 'codemirror/mode/clike/clike';

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
    mirror: CodeMirror.Editor;
    constructor(
        private tabs: TabService,
        private input: InputStream,
        private elm: ElementRef
    ) {
    }
    ngAfterViewInit() {
        const txtElm = this.elm.nativeElement.querySelector('textarea');
        this.mirror = CodeMirror.fromTextArea(txtElm, this.editorOptions());
        this.mirror.on('change', this.changeHandler);
        this.tabs.currentSessionId.subscribe(id => {
            let doc = null;
            if (!this.documents[id]) {
                doc = CodeMirror.Doc('');
                this.documents[id] = doc;
            }
            doc = this.documents[id];
            const oldDoc = this.mirror.swapDoc(doc);
            this.mirror.setOption('mode', 'text/x-csharp');
            this.mirror.setOption('sessionId', id);
            this.documents[this.sessionId] = oldDoc;
            this.sessionId = id;
        });
    }

    private changeHandler = (mirror: CodeMirror.Editor, event: CodeMirror.EditorChange): void => {
        const sessionId = mirror.getOption('sessionId');
        this.input.edit(sessionId, <TextUpdate> {
            text: event.text,
            removed: event.removed,
            from: event.from,
            to: event.to
        });
    };

    private editorOptions() {
        return {
            lineNumbers: true,
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
