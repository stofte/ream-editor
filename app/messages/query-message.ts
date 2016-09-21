import { WebSocketMessage } from './web-socket-message';

interface BaseResponse { code: string; message: string; }

type eventType = 'starting' | 'failed' | 'ready' | 'closing' | 'closed' | 'message' | 'run-code-response';
export class QueryMessage {
    constructor(
        public type: eventType,
        public id: string = null,
        public socket: WebSocketMessage = null,
        public response: BaseResponse = null
    ) {}
}
