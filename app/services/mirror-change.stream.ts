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

@Injectable()
export class MirrorChangeStream {
    private sub = new Subject<EditorChange>();
    private buffers: Observable<BufferMap>;
    private ops = new ReplaySubject<IStreamOperation>();
    
    constructor(
        private tabs: TabService
    ) {
        this.buffers = this.ops
            .scan((bfs: BufferMap, op) => {
                let res = op(bfs);
                return res;
            }, <BufferMap> { tabs: [] })
            .publishReplay()
            .refCount()
            ;
            
        this.buffers.subscribe(x => {
            //console.log('buffers', x);
        });
    }
    
    public initMirror(mirror: CodeMirror.Editor) {
        this.tabs.connectionChanged
            .subscribe(tabs => {
                let toTab = tabs[0];
                console.log('conn, changed', tabs[0].connectionId, 'from tab:', tabs[1]);
                this.ops.next((m: BufferMap) => {
                    let hasActive = m.tabs.find(t => t.active);
                    let newTabs = m.tabs.map(x => {
                            if (x.active) {
                                x.value = mirror.getDoc().getValue();
                            }
                            x.active = x.tabId === toTab.id && x.connId === toTab.connectionId;
                            return x;
                        })
                        ;
                    // updates omnisharp with the buffers current value
                    let current = mirror.getDoc().getValue();
                    let lines = hasActive ? hasActive.value.split('\n') : [];
                    let endLineOffset = hasActive ? lines.length - 1 : 0;
                    let endColumnOffset = hasActive ? lines[lines.length - 1].length : 0;
                    console.log(`mirror: ${current}, buffer: ${JSON.stringify(hasActive)} ... (${endLineOffset},${endColumnOffset})`)
                    this.sub.next(<EditorChange> {
                        newText: current,
                        origin: 'totally-not-fake',
                        created: performance.now(),
                        startColumn: 0,
                        startLine: 0,
                        endColumn: 0 + endColumnOffset,
                        endLine: 0 + endLineOffset
                    });
                    return <BufferMap> {
                        tabs: newTabs
                    };
                });
            });
        this.tabs.activeTab
            .subscribe(tabs => {
                let toTab = tabs[0];
                let fromTab = tabs[1];
                this.ops.next((m: BufferMap) => {
                    let oldBuffers = m.tabs;
                    let buffer = oldBuffers.find(b => b.tabId === toTab.id);
                    let newVal = buffer ? buffer.value : ''; 
                    let oldVal = mirror.getDoc().getValue();
                    let newBuffers = !fromTab ? oldBuffers : [
                        <BufferValue> {
                            tabId: fromTab.id,
                            value: oldVal,
                            connId: fromTab.connectionId
                        },
                        ...oldBuffers.filter(b => b.tabId === fromTab.id)
                    ].map(x => {
                        x.active = x.tabId === toTab.id && x.connId === toTab.connectionId;
                        return x;
                    });
                    // console.log(`mirror.tab(${fromTab} => ${toId}): "${oldVal}" => "${newVal}"\nusing buffers:\n${JSON.stringify(oldBuffers)}\nnew buffers\n${JSON.stringify(newBuffers)}`);
                    mirror.getDoc().setValue(newVal);
                    return <BufferMap> { tabs : newBuffers };
                });
            });
        mirror.on('change', (mirror, cs) => {
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
            created: performance.now()
        };
    }
}
