import { Observable, Subject } from 'rxjs/Rx';
import { OmnisharpSessionMessage } from './api';

class Queued {
    public msg: OmnisharpSessionMessage;
    public dequeue: Function;
}

class SessionState {
    public id: string;
    public ready: boolean = false;
    public fileName: string = null;
    public columnOffset: number = null;
    public lineOffset: number = null;
    public queue: Queued[] = [];
    public awaiting: Queued[] = [];

    constructor(id: string) {
        this.id = id;
    }

    public queueOperation(msg: OmnisharpSessionMessage): Promise<number> {
        if (this.isBlocked(msg)) {
            // console.log(`queueOperation BLOCK ${msg.type}:${msg.timestamp}`);
            let dequeue: Function = null;
            const p = new Promise((done) => dequeue = done);
            this.queue.push({msg , dequeue});
            return p;
        } else {
            // console.log(`queueOperation PASS ${msg.type}:${msg.timestamp}`);
            if (msg.type === 'context') {
                this.ready = false; // if we pass a context, we immediatly invalidate the session
            } 
            this.awaiting.push({msg, dequeue: null});
            return new Promise((done) => done(1));
        }
    }

    public resolveOperation(msg: OmnisharpSessionMessage): void {
        // todo would be nice with internal id
        const op = this.awaiting.find(x => x.msg.timestamp === msg.timestamp);
        Assert(op !== null, 'resolveOperation msg found in awaiting queue');
        const prevLength = this.awaiting.length;
        this.awaiting = this.awaiting.filter(x => x.msg.timestamp !== msg.timestamp);
        Assert(prevLength === this.awaiting.length + 1, 'Removed single element from awaiting');
        if (msg.type === 'buffer-template') {
            this.lineOffset = msg.lineOffset;
            this.columnOffset = msg.columnOffset;
            this.fileName = msg.fileName;
            this.ready = true;
        }
        // console.log('resolveOperation.awaiting', this.awaiting.map(x => x.msg.type + ':' + x.msg.timestamp));
        // console.log('resolveOperation.queue', this.queue.map(x => x.msg.type + ':' + x.msg.timestamp));
        // determine if any queued operations should be resolved
        if (this.awaiting.length === 0) {
            if (this.ready) {
                if (this.queue.length > 0) {
                    const multOpsIdx = this.getInitalOperationCount();
                    if (multOpsIdx) {
                        Assert(this.queue[0].msg.type === 'codecheck' || this.queue[0].msg.type === 'autocompletion', 'First element is expected operation');
                        // we have multiple codecheck/autocompletion ops queued on the same buffer timestamp
                        const ops = this.queue.splice(0, multOpsIdx);
                        Assert(ops.length > 0 && ops.map(x => x.msg.type === 'codecheck' || x.msg.type === 'autocompletion'), 'Found multiple ops');
                        ops.forEach(x => {
                            this.awaiting.push(x);
                            // console.log('multi dequeue', x.msg.type + ':' + x.msg.timestamp);
                            setTimeout(() => x.dequeue());
                        });
                    } else {
                        // otherwise, we only dequeue the next operation 
                        const nextOp = this.queue.shift();
                        this.awaiting.push(nextOp);
                        // console.log('single dequeue', nextOp.msg.type + ':' + nextOp.msg.timestamp);
                        if (nextOp.msg.type === 'context') {
                            // if we dequeue a context, we instantly flag the session as invalid
                            this.ready = false;
                        }
                        setTimeout(() => nextOp.dequeue());
                    }
                }
            } else {
                // scan for a queued buffer-template and prioritize it to resolve the lock
                const index = this.getBufferTemplateOffset();
                if (index) {
                    const nextOp = this.queue.splice(index, 1)[0];
                    this.awaiting.push(nextOp);
                    setTimeout(() => nextOp.dequeue());
                }
            }
        }
    }

    public isBlocked(msg: OmnisharpSessionMessage): boolean {
        if (msg.type === 'buffer-template') {
            // if the session is already initialized, it should belong to a previously queued context,
            // or if there's already an inflight buffer-template
            // otherwise, it should not block as we're waiting for this
            return this.ready || this.templatedBlockedOnSession(); 
        } else if (msg.type === 'edit') {
            // edits block on everything
            // console.log('isBlocked edit (this.ready)', !this.ready);
            return !this.ready || this.editBlockedOnOperation();
        } else if (msg.type === 'codecheck' || msg.type === 'autocompletion') {
            // these operations only block if there's an edit inflight/queued or if the session isnt ready
            return !this.ready || this.operationBlockedOnEdit();
        } else if (msg.type === 'context') {
            // context blocks on anything in the queues.
            return this.queue.length > 0 || this.awaiting.length > 0;
        }
        return true;
    }

    private getBufferTemplateOffset() {
        let index = undefined;
        this.queue.forEach((x, idx) => {
            if (index === undefined && x.msg.type === 'buffer-template') {
                index = idx;
            }
        });
        return index;
    }

    private templatedBlockedOnSession(): boolean {
        return this.awaiting.filter(x => ['buffer-template', 'context'].indexOf(x.msg.type) > -1).length > 0;
    }

    private operationBlockedOnEdit(): boolean {
        return this.awaiting.filter(x => ['edit'].indexOf(x.msg.type) > -1).length > 0 ||
            this.queue.filter(x => ['edit', 'context'].indexOf(x.msg.type) > -1).length > 0;
    }

    private editBlockedOnOperation(): boolean {
        return this.awaiting.filter(x => ['edit', 'codecheck', 'autocompletion', 'buffer-template'].indexOf(x.msg.type) > -1).length > 0;
    }

    private getInitalOperationCount() {
        let index = undefined;
        for(let i = 0; i < this.queue.length; i++) {
            if (['codecheck', 'autocompletion'].indexOf(this.queue[i].msg.type) === -1) {
                break;
            } else {
                index = i;
            }
        }
        return index !== undefined ? index + 1 : index;
    }
}

export class OmnisharpSynchronizer {
    private session: SessionState[] = [];
    
    public resolveOperation(msg: OmnisharpSessionMessage): void {
        const session = this.getSession(msg);
        session.resolveOperation(msg);
    }

    public queueOperation(msg: OmnisharpSessionMessage): Promise<number> {
        const session = this.getSession(msg);
        return session.queueOperation(msg);
    }

    private getSession(msg: OmnisharpSessionMessage) {
        if (!this.session.find(x => x.id === msg.sessionId)) {
            Assert(msg.type === 'context', 'Type context msg when no session found');
            const newSession = new SessionState(msg.sessionId)
            this.session.push(newSession);
        }
        return this.session.find(x => x.id === msg.sessionId);
    }
}