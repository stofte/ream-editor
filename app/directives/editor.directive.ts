// tsdm doesn't include this file
/// <reference path="../../node_modules/retyped-codemirror-tsd-ambient/codemirror-showhint.d.ts" />

import { Directive, ElementRef, Renderer, OnInit } from '@angular/core';
import { Router, RouteParams } from '@angular/router-deprecated';

import { EditorService } from '../services/editor.service';
import { TabService } from '../services/tab.service';
import { EditorChange } from '../models/editor-change';
import { OmnisharpService } from '../services/omnisharp.service';
import { MirrorChangeStream } from '../streams/mirror-change.stream';
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
        private mirrorChangeStream: MirrorChangeStream,
        private route: Router,
        private routeParams: RouteParams,
        public element: ElementRef, 
        public renderer: Renderer
    ) {
        const tabId = parseInt(routeParams.get('tab'), 10);
        this.current = tabService.get(tabId);
        this.editor = CodeMirror.fromTextArea(element.nativeElement, this.editorOptions());
        mirrorChangeStream.initMirror(this.editor, tabId);
        // todo: hack somewhere else. service ref should be passable
        if (!onetimeBullshit) {
            onetimeBullshit = true;
            CodeMirror.registerHelper('hint', 'ajax', (mirror, callback) => {
                // todo: test if syntax mode changes anything,
                // otherwise findWordAt seems pretty useless, 
                // returning random whitespace/syntax as words.
                // for now this manual parsing, that doesn't work cross lines
                const memberAccessTest = /\.$/; 
                const partialMemberAccessTest = /\.(\w*)$/; // this cant work
                const tab = this.tabService.get(mirror._tab);
                const cur = mirror.getCursor();
                const editorLine: string = mirror.getRange({line: cur.line, ch: 0}, cur);
                let fragment: string = null;
                let range = { 
                    head: { line: cur.line, ch: cur.ch },
                    anchor: { line: cur.line, ch: cur.ch }
                };
                if (!memberAccessTest.test(editorLine)) {
                    let match = editorLine.match(partialMemberAccessTest);
                    if (match[1] && match[1].length > 0) { // had partial access
                        fragment = match[1];
                        range.anchor.ch = match.index + 1;
                    }
                }
                let request = <AutocompletionQuery> {
                    fileName: tab.fileName,
                    column: cur.ch + 1,
                    line: cur.line + tab.templateLineOffset,
                    buffer: tab.templateHeader + mirror.getValue() + tab.templateFooter,
                    wantKind: true,
                    wantDocumentationForEveryCompletionResult: true,
                    wordToComplete: fragment
                };
                omnisharpService
                    .autocomplete(request)
                    .subscribe(list => {
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
        
    private codemirrorValueChanged(mirror: any) {
        this.touched = true;
        let newValue = mirror.getValue();
        let cur = mirror.getCursor();
        let range = mirror.findWordAt(cur);
        let fragment = mirror.getRange(range.anchor, range.head);
        if (fragment === '.' || fragment === ').') {
            CodeMirror.commands.autocomplete(mirror);
        }
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
