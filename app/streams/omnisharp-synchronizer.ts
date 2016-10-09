import { Observable, Subject } from 'rxjs/Rx';
import { OmnisharpSessionMessage } from './api';
import { AutocompletionQuery } from '../models/index';
import config from '../config';
import * as uuid from 'node-uuid';
const path = electronRequire('path');

class Queued {
    public msg: OmnisharpSessionMessage;
    public dequeue: Function;
}

class FileNames {
    public used: string[] = [];
    public free: string[] = [];

    constructor(
        public connectionId: number
    ) { }

    public belongs(fileName: string): boolean {
        return !!(this.used.find(x => x === fileName) || this.free.find(x => x === fileName));
    }
    
    public getFree(): string {
        if (this.free.length === 0) {
            const bufferType = this.connectionId ? `db${this.connectionId}` : 'code';
            const newName = path.normalize(`${config.omnisharpProjectPath}/${bufferType}${uuid.v4().replace(/\-/g, '')}.cs`);
            this.used.push(newName);
            return newName;
        } else {
            const newName = this.free[0];
            this.free = this.free.slice(1);
            this.used.push(newName);
            return newName;
        }
    }

    public markFree(name: string): void {
        Assert(this.used.indexOf(name) > -1, 'Was not in used list');
        Assert(this.free.indexOf(name) === -1, 'Was already in free list');
        this.used = this.used.filter(x => x !== name);
        this.free = this.free.concat([name]);
    }
}

// OmniSharp does not have a removal api, so we reuse the filenames, preferring to reuse a 
// filename that already uses the same context as the new one (not sure if omnisharp cares)
class FileNameService {
    private connections: FileNames[] = [];
    private codes = new FileNames(null);

    public getFileCount() {
        return this.connections.reduce((acc, val) => {
            return acc + (val.free.length + val.used.length);
        },<number> (this.codes.free.length + this.codes.used.length));
    }

    public get(connectionId: number): string {
        if (connectionId) {
            if (!this.connections.find(x => x.connectionId === connectionId)) {
                this.connections = [new FileNames(connectionId)].concat(this.connections);
            }
            const fns = this.connections.find(x => x.connectionId === connectionId);
            return fns.getFree();
        } else {
            return this.codes.getFree();
        }
    }

    public release(fileName: string) {
        let found = false;
        if (this.codes.belongs(fileName)) {
            this.codes.markFree(fileName);
            found = true;
        } else {
            for(let i = 0; i < this.connections.length; i++) {
                if (this.connections[i].belongs(fileName)) {
                    this.connections[i].markFree(fileName);
                    found = true;
                    break;
                }
            }
        }
        Assert(found, 'Did not find owner of ' + fileName);
    }
}

class SessionState {
    public id: string;
    public ready: boolean = false;
    public fileName: string = null;
    public columnOffset: number = null;
    public lineOffset: number = null;
    public queue: Queued[] = [];
    public awaiting: Queued[] = [];
    public pendingDeletion = false;

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
            } else if (msg.type === 'destroy') {
                this.pendingDeletion = true;
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
                        // we have multiple codecheck/autocompletion ops queued on the same buffer timestamp
                        Assert(this.queue[0].msg.type === 'codecheck' ||
                                this.queue[0].msg.type === 'autocompletion',
                                'First element is expected operation');
                        const ops = this.queue.splice(0, multOpsIdx);
                        Assert(ops.length > 0 && ops.map(x => x.msg.type === 'codecheck' ||
                            x.msg.type === 'autocompletion'), 'Found multiple ops');
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
                        } else if (nextOp.msg.type === 'destroy') {
                            this.pendingDeletion = true;
                        }
                        setTimeout(() => nextOp.dequeue());
                    }
                }
            } else {
                // scan for a queued buffer-template and prioritize it to resolve the lock
                const index = this.getBufferTemplateOffset();
                if (index !== undefined) {
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
        } else if (msg.type === 'context' || msg.type === 'destroy') {
            // context/destroy blocks on anything in the queues.
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
        for (let i = 0; i < this.queue.length; i++) {
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
    public fileNames = new FileNameService();
    private session: SessionState[] = [];
    private deadSessions: string[] = [];
    
    public resolveOperation(msg: OmnisharpSessionMessage): void {
        Assert(!this.deadSessions.find(x => x === msg.sessionId), 'Got message on dead session: ' + msg.sessionId);
        if (msg.type === 'destroy') {
            const toRemove = this.session.find(x => x.id === msg.sessionId); 
            Assert(toRemove, 'Found no session to destroy');
            this.deadSessions = [toRemove.id].concat(this.deadSessions);
            this.fileNames.release(toRemove.fileName);
            this.session = this.session.filter(x => x.id !== msg.sessionId);
        } else {
            const session = this.getSession(msg);
            session.resolveOperation(msg);
        }
    }

    public queueOperation(msg: OmnisharpSessionMessage): Promise<number> {
        const session = this.getSession(msg);
        return session.queueOperation(msg);
    }

    public mapMessage(msg: OmnisharpSessionMessage): OmnisharpSessionMessage {
        const session = this.getSession(msg);
        if (msg.type === 'context') {
            if (session.fileName) {
                this.fileNames.release(session.fileName);
            }
        } else if (msg.type === 'edit') {
            const mapEdit = (x) => {
                return {
                    from: { 
                        line: x.from.line + session.lineOffset + 1,
                        ch: x.from.ch + (x.from.line === 0 ? session.columnOffset : 0) + 1
                    },
                    to: { 
                        line: x.to.line + session.lineOffset + 1,
                        ch: x.to.ch + (x.to.line === 0 ? session.columnOffset : 0) + 1
                    },
                    text: x.text.concat([])
                };
            };
            return <OmnisharpSessionMessage> {
                sessionId: msg.sessionId,
                edit: msg.edit && mapEdit(msg.edit),
                edits: msg.edits && msg.edits.map(mapEdit),
                type: msg.type,
                timestamp: msg.timestamp,
                fileName: session.fileName
            };
        } else if (msg.type === 'autocompletion') {
            const ac = msg.autocompletion;
            return <OmnisharpSessionMessage> {
                sessionId: msg.sessionId,
                type: msg.type,
                timestamp: msg.timestamp,
                fileName: session.fileName,
                autocompletion: <AutocompletionQuery> {
                    line: ac.line + 1 + session.lineOffset,
                    column: ac.column + 1 + (ac.line === 0 ? session.columnOffset : 0),
                    fileName: session.fileName,
                    wantDocumentationForEveryCompletionResult: ac.wantDocumentationForEveryCompletionResult,
                    wantReturnType: ac.wantReturnType,
                    wantKind: ac.wantKind,
                    wantImportableTypes: ac.wantImportableTypes,
                    wantMethodHeader: ac.wantMethodHeader,
                    wantSnippet: ac.wantSnippet,
                    wordToComplete: ac.wordToComplete
                }
            };
        } else if (msg.type === 'codecheck') {
            return <OmnisharpSessionMessage> {
                sessionId: msg.sessionId,
                type: msg.type,
                timestamp: msg.timestamp,
                fileName: session.fileName,
                // used to map line/col numbers
                lineOffset: session.lineOffset,
                columnOffset: session.columnOffset
            };
        } else if (msg.type === 'buffer-template') {
            const fileName = this.fileNames.get(msg.connectionId);
            return <OmnisharpSessionMessage> {
                fileName,
                sessionId: msg.sessionId,
                type: msg.type,
                timestamp: msg.timestamp,
                template: msg.template,
                lineOffset: msg.lineOffset,
                columnOffset: msg.columnOffset
            };
        }
        return msg;
    }

    private getSession(msg: OmnisharpSessionMessage) {
        if (!this.session.find(x => x.id === msg.sessionId)) {
            Assert(msg.type === 'context', `Type context expected when no session found, found ${msg.type}`);
            const newSession = new SessionState(msg.sessionId);
            this.session.push(newSession);
        }
        return this.session.find(x => x.id === msg.sessionId);
    }
}
