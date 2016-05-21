// input for omnisharp
export class AutocompletionQuery {
    public buffer: string;
    public line: number;
    public column: number;
    public fileName: string;
    public wantDocumentationForEveryCompletionResult: boolean;
    public wantReturnType: boolean;
    public wantKind: boolean;
    public wantImportableTypes: boolean;
    public wantMethodHeader: boolean;
    public wantSnippet: boolean; 
    public wordToComplete: string;
}
