// tsdm doesn't include this file
/// <reference path="../../node_modules/retyped-codemirror-tsd-ambient/codemirror-showhint.d.ts" />

import { Directive, ElementRef, Renderer, OnInit } from '@angular/core';
import { Router, RouteParams } from '@angular/router-deprecated';

import { EditorService } from '../services/editor.service';
import { TabService } from '../services/tab.service';
import { EditorChange } from '../models/editor-change';
import { OmnisharpService } from '../services/omnisharp.service';
import { AutocompletionQuery } from '../models/autocompletion-query';
import { AutocompletionResult } from '../models/autocompletion-result';
import { Tab } from '../models/tab';
import * as CodeMirror from 'codemirror';
import 'codemirror/addon/hint/show-hint';
let onetimeBullshit = false;
CodeMirror.commands.autocomplete = function(cm) {
    cm.showHint({ hint: CodeMirror.hint.ajax });
};

const mac = CodeMirror.keyMap.default === CodeMirror.keyMap.macDefault;
CodeMirror.keyMap.default[(mac ? 'Cmd' : 'Ctrl') + '-Space'] = 'autocomplete';

@Directive({
    selector: '[editor]'
})
export class EditorDirective implements OnInit {
    private current: Tab = null;
    private textContent: string = null;
    private touched = false;
    editor: any;
    constructor(
        private editorService: EditorService,
        private omnisharpService: OmnisharpService,
        private tabService: TabService,
        private route: Router,
        private routeParams: RouteParams,
        public element: ElementRef, 
        public renderer: Renderer
    ) {
        const tabId = parseInt(routeParams.get('tab'), 10);
        this.current = tabService.get(tabId);
        this.editor = CodeMirror.fromTextArea(element.nativeElement, this.editorOptions());
        // todo: hack somewhere else. service ref should be passable
        if (!onetimeBullshit) {
            onetimeBullshit = true;
            CodeMirror.registerHelper('hint', 'ajax', (mirror, callback) => {
                let tab = this.tabService.get(mirror._tab);
                let cur = mirror.getCursor();
                let range = mirror.findWordAt(cur);
                let fragment = mirror.getRange(range.anchor, range.head);
                let request = <AutocompletionQuery> {
                    fileName: tab.fileName,
                    column: cur.ch + 1,
                    line: cur.line + tab.templateLineOffset,
                    buffer: tab.templateHeader + mirror.getValue() + tab.templateFooter,
                    wantKind: true,
                    wantDocumentationForEveryCompletionResult: true,
                };
                omnisharpService
                    .autocomplete(request)
                    .subscribe(list => {
                        if (fragment === '.') {
                            range.anchor.ch = range.head.ch;
                        }
                        callback({
                            list,
                            from: range.anchor,
                            to: range.head
                        });
                    });
            });
            CodeMirror.hint.ajax.async = true;
        }
        editorService.changes.subscribe(this.editorValueUpdated.bind(this));
        this.editor._tab = this.current.id;
        const contents = editorService.get(this.current);
        this.editor.setValue(contents);
        const domElm = this.editor.getWrapperElement();
        domElm.classList.toggle('form-control');
        this.editor.on('change', this.codemirrorValueChanged.bind(this));
    }
    
    private editorValueUpdated(change: EditorChange) {
        if (this.current.id === change.tabId && !this.touched) {
            this.editor.setValue(change.newText);
        }
    }
        
    private codemirrorValueChanged(doc: any) {
        this.touched = true;
        let newValue = doc.getValue();
        this.editorService.set(this.current, newValue);
    }
    
    ngOnInit() {
        this.editor.refresh();
    }
    
    private editorOptions() {
        return {
            lineNumbers: false,
            matchBrackets: true,
            viewportMargin: Infinity,
            showCursorWhenSelecting: true,
            mode: 'text/x-csharp'
        };
    }
}
