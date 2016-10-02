import { CodeCheckResult } from '../models/index';
import { AutoCompletionItem } from '../streams/interfaces';

type eventType = 'starting' | 'failed' | 'ready' | 'closing' | 'closed' | 'codecheck' | 'buffer-created' | 'autocompletion';
type sessionEventType = 'context' | 'buffer-template' | 'edit' | 'codecheck' | 'autocompletion';

export class OmnisharpMessage {
    constructor(
        public type: eventType,
        public sessionId: string = null,
        public requestId: string = null,
        public completions: AutoCompletionItem[] = null,
        public checks: CodeCheckResult[] = null,
        public timestamp: number = null
    ) {}
}

export class OmnisharpSessionMessage {
    constructor(
        public sessionId: string,
        public timestamp: number,
        public type: sessionEventType,
        public fileName: string,
        public lineOffset: number,
        public columnOffset: number,
        public request: any
    ) { }
}
