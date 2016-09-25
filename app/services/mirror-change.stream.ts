import { Injectable } from '@angular/core';
import { EditorChange } from '../models/editor-change';
import { TabService } from '../services/tab.service';
import * as CodeMirror from 'codemirror';
import { Observable, Subject, ReplaySubject } from 'rxjs/Rx';

class BufferMap {
    tabs: BufferValue[];
}

class BufferValue {
    tabId: number;
    connId: number;
    value: string;
    active: boolean;
}

let THERE_CAN_BE_ONLY_ONE = false;

@Injectable()
export class MirrorChangeStream {
    private sub = new Subject<EditorChange>();
    private buffers: Observable<BufferMap>;
    private bufferOps = new ReplaySubject<IStreamOperation>();
    // the current value, used for executing queries
    public executing: Observable<string>;
    private currentOps = new ReplaySubject<IStreamOperation>();
    private mirror: CodeMirror.Editor = null;
    
    constructor(
        private tabs: TabService
    ) {
        this.buffers = this.bufferOps
            .scan((bfs: BufferMap, op) => {
                let res = op(bfs);
                return res;
            }, <BufferMap> { tabs: [] })
            .publishReplay()
            .refCount()
            ;
        this.executing = this.currentOps
            .scan((str, op) => {
                return op(str);
            }, '')
            .publishReplay()
            .refCount()
            ;
        this.buffers.subscribe();
    }
    
    public execute() {
        this.currentOps.next((str) => {
            return this.mirror.getDoc().getValue();
        });
    }
    
    public initMirror(mirror: CodeMirror.Editor) {
        Assert(this.mirror === null, 'mirror was set');
        this.mirror = mirror;
        this.tabs.activeBase
            .subscribe(tabs => {
                this.bufferOps.next((m: BufferMap) => {
                    let toTab = tabs[0];
                    Assert(toTab, 'no active tab');
                    let buffers = m.tabs;
                    let bufferFilter = x => x.tabId === toTab.id && x.connId === toTab.connectionId;
                    if (!buffers.find(bufferFilter)) {
                        // if the target buffer doesn't exist, create it
                        buffers = [
                            <BufferValue> {
                                tabId: toTab.id,
                                connId: toTab.connectionId,
                                active: false,
                                value: ''
                            },
                            ...buffers
                        ];
                    }
                    let fromBuffer = buffers.find(x => x.active);
                    let toBuffer = buffers.find(bufferFilter);
                    // highlander is set further down, the first time here, it'll be false
                    let sameTab = fromBuffer && toTab.id === fromBuffer.tabId || !THERE_CAN_BE_ONLY_ONE;
                    let there_was_one = false;
                    Assert(toBuffer, `no toBuffer found`);
                    if (fromBuffer) {

                        // update the value of the fromBuffer, if it existed
                        buffers = buffers.map(x => {
                            if (x.tabId === fromBuffer.tabId && x.connId === fromBuffer.connId) {
                                x.value = this.mirror.getDoc().getValue();
                            }
                            return x;
                        });
                        fromBuffer = buffers.find(x => x.active); // update
                    } else {
                        // fromBuffer should only be missing for the very first tab created
                        Assert(!THERE_CAN_BE_ONLY_ONE, 'ZOMG');                     
                        there_was_one = THERE_CAN_BE_ONLY_ONE = true;
                    }
                    
                    // set active
                    buffers = buffers.map(x => {
                        x.active = x.tabId === toTab.id && x.connId === toTab.connectionId;
                        return x;
                    });
                    
                    // branch, depending on what we think happened.
                    if (!sameTab) {
                        Assert(toBuffer, `not same tab, but no toBuffer ${toBuffer}`);
                        // switched to different tab, set mirror text to restored value
                        mirror.getDoc().setValue(toBuffer.value);
                    } else if (!there_was_one) {
                        Assert(toBuffer && fromBuffer, 'same tab, but no fromBuffer');
                        // flush an update to omnisharp, accounts for how
                        // the buffer was modified while we were using 
                        // another connection context.
                        let lines = toBuffer ? toBuffer.value.split('\n') : [];
                        let endLineOffset = toBuffer ? lines.length - 1 : 0;
                        let endColumnOffset = toBuffer ? lines[lines.length - 1].length : 0;
                        this.sub.next(<EditorChange> {
                            newText: fromBuffer.value,
                            origin: 'totally-not-fake',
                            timestamp: performance.now(),
                            startColumn: 0,
                            startLine: 0,
                            endColumn: 0 + endColumnOffset,
                            endLine: 0 + endLineOffset
                        });
                    }
                    
                    return <BufferMap> {
                        tabs: buffers
                    };
                });
            });
        mirror.on('change', (_, cs) => {
            if (cs.origin !== 'setValue') {
                this.sub.next(this.mapEvent(cs));
            }
        });
    }
    
    public get changes(): Observable<EditorChange> {
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
            newText: val.text.join('\n'),
            origin: val.origin,
            timestamp: performance.now()
        };
    }
}
