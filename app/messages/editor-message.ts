import { TextUpdate } from '../models/index';

type eventType = 'edit';

export class EditorMessage {
    constructor(
        public type: eventType,
        public data: TextUpdate,
        public timestamp: number
    ) {}
}
