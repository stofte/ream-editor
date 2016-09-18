import { TextUpdate } from '../models/index';

type eventType = 'create' | 'edit' | 'run-code-text';

export class EditorMessage {
    constructor(
        public type: eventType,
        public id: string,
        public data: TextUpdate = null,
        public text: string = null
    ) {}
}
