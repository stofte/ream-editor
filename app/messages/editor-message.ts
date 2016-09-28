import { TextUpdate } from '../models/index';

type eventType = 'create' | 'edit' | 'buffer-text' | 'codecheck-request';

export class EditorMessage {
    constructor(
        public type: eventType,
        public id: string,
        public data: TextUpdate = null,
        public text: string = null,
        // indicates the session msg type that generated the buffer-text msg
        public bufferTextOrigin: string = null,
        // the timestamp of the msg that generated this msg
        public originTimestamp: number = null
    ) {}
}
