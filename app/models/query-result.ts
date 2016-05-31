import { ResultPage } from './result-page';

export class QueryResult {
    public id: string; // random guid generated on server
    public tabId: number;
    public query: string;
    public connectionString: string;
    public created: Date;
    public finished: Date;
    public loading: boolean;
    public pages: ResultPage[] = [];
}
