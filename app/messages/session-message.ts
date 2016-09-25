import { AutocompletionQuery, Connection } from '../models/index';

type eventType = 'create' | 'run-code' | 'codecheck' | 'autocomplete' | 'context';

export class SessionMessage {
    constructor(
        public type: eventType,
        public id: string,
        public timestamp: number = null,
        public autoComplete: AutocompletionQuery = null,
        public connection: Connection = null
    ) {}
}
