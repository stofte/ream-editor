class LineColumn {
    public line: number;
    public ch: number;
}

export class TextUpdate {
    public from: LineColumn;
    public to: LineColumn;
    public text: string[];
    public removed: string[];
    public timestamp: number;
}
