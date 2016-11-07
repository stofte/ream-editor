import { Component, AfterViewInit, AfterViewChecked, ElementRef, Input } from '@angular/core';
import { TabService } from '../services/index';
import { InputStream, OutputStream, EventName, Message } from '../streams/index';
import { TextUpdate, AutocompletionQuery } from '../models/index';
import * as _ from 'lodash';
import * as CodeMirror from 'codemirror';
import 'codemirror/addon/hint/show-hint';
import 'codemirror/mode/clike/clike';

@Component({
    selector: 'rm-editor',
    template: `
<div class="rm-editor">
    <textarea></textarea>
</div>
`
})
export class EditorComponent implements AfterViewInit, AfterViewChecked {
    documents: any = {};
    completions: any = {};
    sessionId: string;
    @Input('editor-props') public editorProps: any;
    private mirror: CodeMirror.Editor;
    private prevHeight: number;
    private firstLoad = true;
    constructor(
        private tabs: TabService,
        private input: InputStream,
        private output: OutputStream,
        private elm: ElementRef
    ) {
        tabs.tabDragging.subscribe(dragging => {
            this.mirror.setOption('dragDrop', !dragging);
        });
        this.initializeGlobalCodeMirrorObject();
        output.events
            .filter(msg => msg.name === EventName.OmniSharpAutocompletion)
            .subscribe(this.completionSubscriber);
    }

    initializeGlobalCodeMirrorObject() {
        CodeMirror.commands.autocomplete = function(cm: CodeMirror.Editor) {
            cm.showHint({ hint: CodeMirror.hint.ajax });
        };
        const mac = CodeMirror.keyMap.default === CodeMirror.keyMap.macDefault;
        CodeMirror.keyMap.default[(mac ? 'Cmd' : 'Ctrl') + '-Space'] = 'autocomplete';
        CodeMirror.registerHelper('hint', 'ajax', this.hintHandler);
        CodeMirror.hint.ajax.async = true;
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
            requestAnimationFrame(() => {
                if (this.firstLoad) {
                    this.mirror.refresh();
                    this.firstLoad = false;
                    requestAnimationFrame(() => this.mirror.focus());
                } else {
                    this.mirror.focus();
                }
            });
        });
    }

    ngAfterViewChecked() {
        const elm = this.elm.nativeElement.querySelector('.CodeMirror');
        if (elm) {
            if (this.editorProps.height !== this.prevHeight) {
                elm.style.height = this.editorProps.height + 'px';
                this.prevHeight = this.editorProps.height;
                this.mirror.refresh();
            }
        }
    }

    private completionSubscriber = (msg: Message) => {
        const fn = this.completions[msg.id];
        if (fn) {
            this.completions[msg.id] = null;
            // only resolve if the relevant
            if (this.sessionId === msg.id) {
                const completions = this.mapToCodeMirror(msg.data);
                fn(completions);
            }
        }
    }

    private mapToCodeMirror(result: any[]): any[] {
        return _.chain(result)
            .groupBy(r => r.CompletionText)
            .map(nameList => {
                // filter out duplicates of the same name and type
                let byType = _.reduce(nameList, (saved, item: any) => {
                    if (!this.filterDumpMethod(item) &&
                        !saved.find(s => s.Kind === item.Kind)) {
                        saved.push(item);
                    }
                    return saved;
                }, []);
                let res = _.sortBy(byType, 'Kind');
                return res;
            })
            .flatten()
            .map((i: any) => {
                return {
                    sortKey: i.CompletionText.toLocaleLowerCase(), 
                    text: i.CompletionText,
                    className: `prop-kind-${i.Kind.toLocaleLowerCase()}`
                };
            })
            .sortBy('sortKey')
            .value();
    }

    private filterDumpMethod(item: any): boolean {
        const isCore = item.MethodHeader.indexOf('Dump(Emitter emitter)') !== -1 &&
            // todo for some reason omnisharp doesnt see doc comments?
            // item.Description.indexOf('ReamQuery.Core.Dumper.Dump') !== -1;
            item.Description === '';
        const isWrapper = item.MethodHeader.indexOf('Dump()') !== -1 &&
            item.Description.indexOf('DumpWrapper.Dump') !== -1;
        return isCore || isWrapper;
    }

    private changeHandler = (mirror: CodeMirror.Editor, event: CodeMirror.EditorChange): void => {
        const sessionId = mirror.getOption('sessionId');
        this.input.edit(sessionId, <TextUpdate> {
            text: event.text,
            removed: event.removed,
            from: event.from,
            to: event.to
        });
    }

    private hintHandler = (mirror: CodeMirror.Editor, callback: Function): void => {
        const cur = mirror.getCursor();
        const wordRange = mirror.findWordAt(cur);
        const wordAt = mirror.getRange(wordRange.anchor, wordRange.head);
        let fragment = wordAt;
        let range = {
            // anchor is the start of the range
            anchor: { line: wordRange.anchor.line, ch: wordRange.anchor.ch },
            head: { line: wordRange.head.line, ch: wordRange.head.ch }
        };
        console.log('completion text', `"${wordAt}"`);
        if (/\.\s*$/.test(fragment)) {
            console.log('dot access');
            fragment = '';
            range.anchor.ch = cur.ch;
        } else if (/^\s+$/.test(fragment)) {
            console.log('was all whitespace');
            fragment = '';
            range.anchor.ch = cur.ch;
        }
        console.log('omnisharp fragment', `"${fragment}"`);
        let request = <AutocompletionQuery> {
            column: cur.ch,
            line: cur.line,
            wantKind: true,
            wantDocumentationForEveryCompletionResult: true,
            wordToComplete: fragment,
            wantReturnType: true,
            wantMethodHeader: true
        };
        // queue the resolver function, to be resolved in completionSubscriber,
        // passing a list of completions mapped to codemirror, with type classNames. 
        let resolver = null;
        const p = new Promise(done => resolver = done);
        this.completions[this.sessionId] = resolver;
        p.then(list => callback({ list, from: range.anchor, to: range.head }));

        this.input.autoComplete(this.sessionId, request);
    }

    private editorOptions() {
        return {
            lineNumbers: true,
            gutters: ['CodeMirror-lint-markers'],
            lint: true,
            smartIndent: false,
            showCursorWhenSelecting: true,
            mode: 'text/x-csharp'
        };
    }
}
