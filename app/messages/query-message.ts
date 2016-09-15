type eventType = 'starting' | 'failed' | 'ready' | 'closing' | 'closed';

export class QueryMessage {
    constructor(
        public type: eventType,
        public value: string = null
    ) {}
}
