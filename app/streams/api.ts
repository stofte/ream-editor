export enum EventName {
    Unknown,

    ProcessStarting,
    ProcessFailed,
    ProcessReady,
    ProcessClosing,
    ProcessClosed,

    SessionCreate,
    SessionExecuteBuffer,
    // the editor stream doesnt participate in autocompletion, so its placed in session instead.
    SessionAutocompletion,
    SessionContext,
    SessionDestroy,
    
    EditorExecuteText, // text intended for execution
    EditorBufferText, // full text of a given buffer, for templating a new session, with the current buffer contents in it
    EditorUpdate, // individual updates from editor
    EditorCodeCheck,

    ResultStart,
    ResultUpdate,
    ResultDone,

    QueryExecuteResponse,
    QueryTemplateResponse,
    QuerySocketOutput,

    OmniSharpAutocompletion,
    OmniSharpCodeCheck
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

// only used internally by omnisharp stream/synchronizer
type sessionEventType = 'context' | 'buffer-template' | 'edit' | 'codecheck' | 'autocompletion' | 'timer' | 'destroy';

export class OmnisharpSessionMessage {
    constructor(
        public sessionId: string,
        public timestamp: number,
        public type: sessionEventType,
        public fileName: string,
        public lineOffset: number,
        public columnOffset: number,
        public template: string,
        public edit: any,
        public edits: any[],
        public autocompletion: any,
        public connectionId: number
    ) { }
}
