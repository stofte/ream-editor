import { Injectable } from '@angular/core';
import { EditorChange } from '../models/editor-change';
import { TabService } from '../services/tab.service';
import * as CodeMirror from 'codemirror';
import { Observable, Subject, ReplaySubject } from 'rxjs/Rx';

class BufferValue {
    tabId: number;
    value: string;
}

@Injectable()
export class MirrorChangeStream {
    private sub = new Subject<EditorChange>();
    private buffers: Observable<BufferValue[]>;
    private ops = new ReplaySubject<IStreamOperation>();
    
    constructor(
        private tabs: TabService
    ) {
        this.buffers = this.ops
            .scan((bfs: BufferValue[], op) => {
                let res = op(bfs);
                return res;
            }, [])
            .publishReplay()
            .refCount()
            ;
            
        this.buffers.subscribe(x => {
            //console.log('buffers', x);
        });
    }
    
    public initMirror(mirror: CodeMirror.Editor) {
        this.tabs.activeTab
            .subscribe(tabs => {
                let toId = tabs[0].id;
                let fromTab = tabs[1];
                this.ops.next((oldBuffers: BufferValue[]) => {
                    let buffer = oldBuffers.find(b => b.tabId === toId);
                    let newVal = buffer ? buffer.value : ''; 
                    let oldVal = mirror.getDoc().getValue();
                    let newBuffers = !fromTab ? oldBuffers : [
                        <BufferValue> {
                            tabId: fromTab.id,
                            value: oldVal
                        },
                        ...oldBuffers.filter(b => b.tabId === fromTab.id)
                    ];
                    // console.log(`mirror.tab(${fromTab} => ${toId}): "${oldVal}" => "${newVal}"\nusing buffers:\n${JSON.stringify(oldBuffers)}\nnew buffers\n${JSON.stringify(newBuffers)}`);
                    mirror.getDoc().setValue(newVal);
                    return newBuffers;
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
