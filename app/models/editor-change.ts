
export class EditorChange {
    public tabId: number;
    public fileName: string;
    public newText: string;
    public origin: string;
    public startLine: number;
    public startColumn: number;
    public endLine: number;
    public endColumn: number;
    public timestamp: number;
    public lineOffset: number; 
}
