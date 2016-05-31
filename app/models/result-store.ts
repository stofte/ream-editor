import { Observable } from 'rxjs/Rx';
import { QueryResult } from './query-result';
import * as _ from 'lodash';

export class ResultStore {
    public data: any;
    
    public tab(id: number): QueryResult[] {
        let l  = <QueryResult[]> this.data[id];
        return _.chain(l).sortBy(x => x.created).reverse().value();
    }
    
    public add(id: number, result: QueryResult) {
        if (!this.data) {
            this.data = {};
        }
        if (!this.data[id]) {
            this.data[id] = [];
        }
        this.data[id].push(result);
    }
}
