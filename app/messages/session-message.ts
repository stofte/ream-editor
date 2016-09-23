type eventType = 'create' | 'run-code' | 'codecheck';

export class SessionMessage {
    constructor(
        public type: eventType,
        public id: string
    ) {}
}
