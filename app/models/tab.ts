import { Connection, ResultPage, SessionLogMessage } from './index';

export class Tab {
    public id: string; // primary key for tab
    public connectionId: number; // stores a connection reference for the given tab
    public title: string;
    public editorHeight: number;
    public executePending: boolean;
    public results: ResultPage[];
    public activeResultId: string;
    public consoleActive: boolean;
    public sessionLog: SessionLogMessage[];
}
