export enum EventName {
    ProcessStarting,
    ProcessFailed,
    ProcessReady,
    ProcessClosing,
    ProcessClosed,

    SessionCreate,
    SessionExecuteBuffer,
    SessionCodeCheck,
    SessionAutocompletion,
    SessionContext,
    
    EditorExecuteText, // text intended for execution
    EditorBufferText, // full text of a given buffer, for templating a new session, with the current buffer contents in it
    EditorUpdate, // individual updates from editor

    ResultStart,
    ResultUpdate,
    ResultDone,

    QueryExecuteResponse,
    QueryTemplateResponse,
    QuerySocketOutput,

    OmniSharpAutocompletion,
    OmniSharpCodeCheck,
}

export class Message {
    public timestamp: number;
    constructor(
        public name: EventName,
        public id: string = null,
        public data: any = null,
        public originalTimestamp: number = null
    ) {
        this.timestamp = performance.now();
    }
}

// ReamQuery API types for socket messages
export type WebSocketMessageType = 'table'|'header'|'row'|'tableClose'|'singleAtomic'|'singleTabular'|'empty'|'close';

export interface WebSocketMessage {
    session: string;
    id: any;
    parent: any;
    type: WebSocketMessageType;
    values: any[];
}

// only used internally by omnisharp stream/synchronizer
type sessionEventType = 'context' | 'buffer-template' | 'edit' | 'codecheck' | 'autocompletion';

export class OmnisharpSessionMessage {
    constructor(
        public sessionId: string,
        public timestamp: number,
        public type: sessionEventType,
        public fileName: string,
        public lineOffset: number,
        public columnOffset: number,
        public template: string,
        public request: any,
        public connectionId: number
    ) { }
}
