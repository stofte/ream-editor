type eventType = 'execute-buffer';

export class UserMessage {
    constructor(
        public type: eventType
    ) {}
}
