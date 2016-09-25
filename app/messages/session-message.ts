import { AutocompletionQuery } from '../models/index';

type eventType = 'create' | 'run-code' | 'codecheck' | 'autocomplete';

export class SessionMessage {
    constructor(
        public type: eventType,
        public id: string,
        public timestamp: number = null,
        public autoComplete: AutocompletionQuery = null
    ) {}
}
