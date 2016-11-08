type sessionLogMessageType = 'info' | 'error';
// presents the user facing log
export class SessionLogMessage {
    public created: Date;
    public type: sessionLogMessageType;
    public text: string;
}
