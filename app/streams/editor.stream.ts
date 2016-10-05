import { Injectable } from '@angular/core';
import { Subject, Observable } from 'rxjs/Rx';
import { TextUpdate } from '../models/index';
// todo: can't use index because we loose metadata?
import { SessionStream } from './session.stream';
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
    private subject: Subject<Message> = new Subject<Message>();

    constructor(
        private session: SessionStream
    ) {
        const editMsgs = session
                    .events
                    .filter(msg => msg.name === EventName.SessionCreate)
                    .flatMap(msg => this.subject.filter(e => e.name === EventName.EditorUpdate && e.id === msg.id))
                    .scan((buffers: BufferText[], editor: Message, index: number) => {
                        if (!buffers.find(x => x.id === editor.id)) {
                            buffers.push(new BufferText(editor.id));
                        }
                        const b = buffers.find(x => x.id === editor.id);
                        b.edit(editor.data);
                        return buffers;
                    }, [])
                    .publish();
        editMsgs.connect();
        const runCodeText = session.events.filter(msg => msg.name === EventName.SessionExecuteBuffer)
            .withLatestFrom(editMsgs)
            .map(val => {
                const b = val[1].find(x => x.id === val[0].id);
                return new Message(EventName.EditorExecuteText, val[0].id, b.getText(), val[0].timestamp);
            });

        const contextText = session.events.filter(msg => msg.name === EventName.SessionContext)
            .withLatestFrom(editMsgs)
            .map(val => {
                const b = val[1].find(x => x.id === val[0].id);
                return new Message(EventName.EditorBufferText, val[0].id, b.getText(), val[0].timestamp);
            })
            .publishReplay(1);

        // cleanup this stuff, either we mix or we dont. not this triple-type broadcast
        const obs = this.subject
            .merge(runCodeText)
            .publish();
        
        this.bufferedTexts = contextText;
        this.events = obs;
        obs.connect();
        contextText.connect();
    }

    public edit(id: string, data: TextUpdate) {
        data.timestamp = performance.now();
        this.subject.next(new Message(EventName.EditorUpdate, id, data));
    }
}
