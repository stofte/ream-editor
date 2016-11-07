import { Injectable } from '@angular/core';
import { Subject, Observable, ConnectableObservable } from 'rxjs/Rx';
import { TextUpdate } from '../models/index';
// todo: can't use index because we loose metadata?
import { InputStream } from './input.stream';
import { EventName, Message } from './api';

class BufferText {
    public lines: string[] = [''];
    constructor(public id: string) { }
    public edit(update: TextUpdate) {
        const from = update.from;
        const to = update.to;
        const additions = [...update.text];
        const deletions = [...update.removed];
        let idx = from.line;
        let isFirst = true;
        while (additions.length > 0 || deletions.length > 0) {
            const inserted = additions.shift();
            const removed = deletions.shift();
            // Since CM always puts an empty string in either arrays as a "noop",
            // we should always enter the first branch on the first iteration.
            // This removed isFirst from the logic of the remaining branches.
            if (removed !== undefined && inserted !== undefined) { 
                // both defined, keep line and modify it in place
                const keep = isFirst ? from.ch : 0;
                this.lines[idx] = `${this.lines[idx].substring(0, keep)}${inserted}${this.lines[idx].substring(keep + removed.length)}`;
                idx++; 
            } else if (inserted !== undefined && removed === undefined) {
                // only insert, so append new line
                this.lines.splice(idx, 0, inserted);
                idx += 2; // skip over the newly inserted line
            } else if (removed !== undefined) {
                // only delete, so merge with previous line, while removing the bit that was deleted
                const removedLine = this.lines[idx];
                this.lines.splice(idx, 1);
                this.lines[idx - 1] = `${this.lines[idx - 1]}${removedLine.substring(removed.length)}`;
            } // else both were empty, and we're done
            isFirst = false;
        }
    }
    public getText() {
        return this.lines.join('\n');
    }
}

@Injectable()
export class EditorStream {
    public events: Observable<Message>;
    public bufferedTexts: Observable<Message>;
    public subject: Subject<Message> = new Subject<Message>(); // used from stream.ts
    private eventsConnectable: ConnectableObservable<Message>;
    private bufferedConnectable: ConnectableObservable<Message>;
    private resolver: Function;

    constructor(
        private input: InputStream
    ) {
        const editMsgs = input
                    .events
                    .filter(msg => msg.name === EventName.SessionCreate)
                    .flatMap(msg => {
                        return input.events
                            .filter(e => e.name === EventName.EditorUpdate && e.id === msg.id)
                            .takeUntil(input.events.first(x => x.name === EventName.SessionDestroy && x.id === msg.id))
                            .startWith(new Message(EventName.EditorUpdate, msg.id, <TextUpdate> {
                                text: [''],
                                removed: [''],
                                from: { line: 0, ch: 0 },
                                to: { line: 0, ch: 0 }
                            }));
                    })
                    .merge(input.events.filter(x => x.name === EventName.SessionDestroy))
                    .scan((buffers: BufferText[], msg: Message, index: number) => {
                        if (msg.name === EventName.SessionDestroy) {
                            return buffers.filter(x => x.id !== msg.id);
                        } else {
                            if (!buffers.find(x => x.id === msg.id)) {
                                buffers.push(new BufferText(msg.id));
                            }
                            const b = buffers.find(x => x.id === msg.id);
                            b.edit(msg.data);
                            return buffers;
                        }
                    }, [])
                    .publish();
        editMsgs.connect();

        const runCodeText = input.events.filter(msg => msg.name === EventName.SessionExecuteBuffer)
            .withLatestFrom(editMsgs)
            .map(val => {
                const b = val[1].find(x => x.id === val[0].id);
                return new Message(EventName.EditorExecuteText, val[0].id, b.getText(), val[0].timestamp);
            });

        const contextText = input.events.filter(msg => msg.name === EventName.SessionContext)
            .withLatestFrom(editMsgs)
            .map(val => {
                let txt = '';
                const b = val[1].find(x => x.id === val[0].id);
                if (b) {
                    txt = b.getText();                    
                }
                return new Message(EventName.EditorBufferText, val[0].id, txt, val[0].timestamp);
            })
            .publishReplay(1);

        // cleanup this stuff, either we mix or we dont. not this triple-type broadcast
        const obs = input.events
            .merge(runCodeText)
            .publish();
        
        this.bufferedTexts = contextText;
        this.events = obs;
        obs.connect();
        contextText.connect();
    }
}
