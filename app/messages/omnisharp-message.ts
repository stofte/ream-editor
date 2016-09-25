import { CodeCheckResult } from '../models/index';
import { AutoCompletionItem } from '../streams/interfaces';

type eventType = 'starting' | 'failed' | 'ready' | 'closing' | 'closed' | 'codecheck' | 'buffer-created' | 'autocompletion';

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
