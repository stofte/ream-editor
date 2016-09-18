import { ResultPage } from '../models/index';

type eventType = 'start' | 'update' | 'done';

export class ResultMessage {
    constructor(
        public type: eventType,
        public id: string,
        public data: ResultPage = null
    ) {}
}
