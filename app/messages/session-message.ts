type eventType = 'create' | 'run-code';

export class SessionMessage {
    constructor(
        public type: eventType,
        public id: string
    ) {}
}
