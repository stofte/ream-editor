import { ResultPage } from './result-page';

export class QueryResult {
    public tabId: number;
    public query: string;
    public connectionString: string;
    public created: number;
    public pages: ResultPage[] = [];
}
