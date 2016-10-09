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
        if (update.removed.length !== 1 || update.removed[0] !== '') {
            for (let i = 1; i <= update.removed.length; i++) {
                const line = from.line + i - 1;
                if (i === 1) {
                    const newLineVal = `${this.lines[line].substring(0, from.ch)}`;
                    this.lines.splice(line, 1, newLineVal);
                } else if (i < update.removed.length) {
                    this.lines.splice(line, 1);
                } else {
                    Assert(this.lines[line - 1], 'Expected previous line');
                    const newLineVal = `${this.lines[line - 1]}${
                        this.lines[line].substring(to.ch)}`;
                    this.lines.splice(line - 1, 2, newLineVal);
                }
            }
        }
        if (update.text.length !== 1) {
            for (let i = 1; i < update.text.length; i++) {
                this.lines.splice(from.line + i, 0, update.text[i]);        
            }
        } else if (update.text[0] !== '') {
            const line = from.line;
            this.lines[line] = `${this.lines[line].substring(0, from.ch)}${
                update.text[0]}${this.lines[line].substring(to.ch)}`;
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
                            .takeUntil(input.events.first(x => x.name === EventName.SessionDestroy && x.id === msg.id));
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
                const b = val[1].find(x => x.id === val[0].id);
                return new Message(EventName.EditorBufferText, val[0].id, b.getText(), val[0].timestamp);
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
