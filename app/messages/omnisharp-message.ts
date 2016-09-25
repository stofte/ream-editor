import { CodeCheckResult } from '../models/index';

type eventType = 'starting' | 'failed' | 'ready' | 'closing' | 'closed' | 'codecheck' | 'buffer-created' | 'autocompletion';

export class OmnisharpMessage {
    constructor(
        public type: eventType,
        public sessionId: string = null,
        public requestId: string = null,
        public completions: string[] = null,
        public checks: CodeCheckResult[] = null
    ) {}
}
