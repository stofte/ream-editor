type eventType = 'starting' | 'failed' | 'ready' | 'closing' | 'closed';

export class ProcessMessage {
    constructor(
        public type: eventType,
        public value: string = null
    ) {}
}
