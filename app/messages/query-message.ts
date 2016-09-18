import { WebSocketMessage } from './web-socket-message';

type eventType = 'starting' | 'failed' | 'ready' | 'closing' | 'closed' | 'message' | 'run-code-response';
export class QueryMessage {
    constructor(
        public type: eventType,
        public socket: WebSocketMessage = null,
        public response: any = null
    ) {}
}
