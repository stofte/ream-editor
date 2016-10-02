import { Observable, Subject } from 'rxjs/Rx';
import { OmnisharpSessionMessage } from '../messages/index';

class Queued {
    public msg: OmnisharpSessionMessage;
    public dequeue: Function;
}

class SessionState {
    public id: string;
    public ready: boolean;
    public operationsInFlight: number;
    public lockedByOperation: boolean;
    public lockedByEdit: boolean;
    public queue: Queued[];
}

export class OmnisharpSynchronizer {
    private session: SessionState[] = [];
    
    public resolveOperation(msg: OmnisharpSessionMessage): void {
        const session = this.getSession(msg);
        this.releaseSession(msg.type, session);
        
        // if (!session.lockedByOperation && session.queue.length > 0) {
        //     const nextOp = session.queue.shift();
        //     this.aquireSession(nextOp.msg.type, session);
        //     nextOp.dequeue();
        //     if (!session.lockedByEdit) {
        //         // dequeue all other non-edit operations on the buffer
        //         while (session.queue.length > 0) {
        //             const nextType = session.queue[0].msg.type;
        //             if (nextType === 'edit' || nextType === 'context') {
        //                 break;
        //             }
        //             const nextOpOp = session.queue.shift();
        //             session.operationsInFlight++;
        //             nextOpOp.dequeue();
        //         }
        //     }
        // }
    }

    public queueOperation(msg: OmnisharpSessionMessage): Promise<number> {
        const session = this.getSession(msg);
        const isOperation = msg.type === 'codecheck' || msg.type === 'autocompletion';
        const isBufferTemplate = msg.type === 'buffer-template';
        const isContext = msg.type === 'context';
        const isEdit = msg.type === 'edit';
        const isBlocked = this.isBlocked(msg);
        // console.log('queueOperation.isBlocked', msg.type, isBlocked)
        if (isBlocked) {
            let dequeue: Function = null;
            const p = new Promise((done) => dequeue = done);
            session.queue.push({msg , dequeue});
            return p;
        } else {
            this.aquireSession(msg.type, session);
            return new Promise((done) => done(1));
        }
    }

    private isBlocked(msg: OmnisharpSessionMessage): boolean {
        const session = this.getSession(msg);
        if (msg.type === 'codecheck' || msg.type === 'autocompletion') {
            return session.lockedByEdit || !session.ready;
        } else if (msg.type === 'buffer-template') {
            return session.lockedByEdit || session.lockedByOperation;
        } else if (msg.type === 'context') {
            return session.lockedByEdit || session.lockedByOperation;
        } else if (msg.type === 'edit') {
            return session.lockedByEdit || session.lockedByOperation || !session.ready;
        }
    }

    private aquireSession(type: string, session: SessionState) {
        const currVal = session.ready;
        if (type === 'edit') {
            session.lockedByEdit = true;
        }
        if (type === 'codecheck' || type === 'autocompletion' || type === 'buffer-template') {
            session.operationsInFlight++;
            session.lockedByOperation = true;
        }
        if (type === 'context') {
            session.ready = false;
        }
        // console.log('aquireSession', performance.now(), type, currVal, '=>', session.ready);
    }

    private releaseSession(type: string, session: SessionState) {
        const currVal = session.ready;
        if (type === 'edit') {
            session.lockedByEdit = false;
        } else if (type === 'codecheck' || type === 'autocompletion' || type === 'buffer-template') {
            session.operationsInFlight--;
            session.lockedByOperation = session.operationsInFlight > 0;
        }
        if (type === 'buffer-template') {
            session.ready = true
        }
        if (session.ready && !session.lockedByEdit && session.queue.length > 0) {
            const nextOp = session.queue.shift();
            this.aquireSession(nextOp.msg.type, session);
            nextOp.dequeue();
            while (session.queue.length > 0 && !this.isBlocked(session.queue[0].msg)) {
                const nextOpOp = session.queue.shift();
                session.operationsInFlight++;
                nextOpOp.dequeue();
            }
        } else if (!session.ready && !session.lockedByOperation) {
            const bufferOp = session.queue.find(x => x.msg.type === 'buffer-template');
            if (bufferOp) {
                const idx = this.getBufferTemplateOffset(session);
                const nextOp = session.queue.splice(idx, 1)[0];
                this.aquireSession(nextOp.msg.type, session);
                nextOp.dequeue();
                // console.log('session not ready, but found buffer op');
            }
        }
    }

    private getBufferTemplateOffset(session: SessionState) {
        let index = undefined;
        // console.log('getBufferTemplateOffset', session.queue.map(x => x.msg.type).join(','))
        session.queue.forEach((x, idx) => {
            if (index === undefined && x.msg.type === 'buffer-template') {
                index = idx;
            }
        });
        return index;
    }

    private releasedNextOperation(session: SessionState) {
        if (session.ready && session.queue.length > 0) {
            // console.log('releasedNextOperation READY')
            const nextOp = session.queue.shift();
            this.aquireSession(nextOp.msg.type, session);
            nextOp.dequeue();
            // dequeue all other non-edit operations on the buffer
            while (session.queue.length > 0 && !this.isBlocked(session.queue[0].msg)) {
                const nextOpOp = session.queue.shift();
                session.operationsInFlight++;
                nextOpOp.dequeue();
            }
        } else {
            // console.log('releasedNextOperation NOT READY')
        }
    }

    private getSession(msg: OmnisharpSessionMessage) {
        if (!this.session.find(x => x.id === msg.sessionId)) {
            Assert(msg.type === 'context', 'Type context msg when no session found');
            this.session.push({
                ready: false,
                id: msg.sessionId,
                lockedByEdit: false,
                lockedByOperation: false,
                operationsInFlight: 0,
                queue: []
            });
        }
        return this.session.find(x => x.id === msg.sessionId);
    }
}