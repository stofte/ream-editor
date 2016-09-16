type eventType = 'text' | 'edit';

export class EditorMessage {
    constructor(
        public type: eventType,
        public value: string = null
    ) {}
}
