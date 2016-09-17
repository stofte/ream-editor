import { WebSocketMessage } from './web-socket-message';

type eventType = 'starting' | 'failed' | 'ready' | 'closing' | 'closed' | 'message';
export class QueryMessage {
    constructor(
        public type: eventType,
        public data: WebSocketMessage = null
    ) {}
}
