import { Observable } from 'rxjs/Rx';
import { QueryResult } from './query-result';
import * as _ from 'lodash';

export class ResultStore {
    private data: Map<number, QueryResult[]>;
    
    public tab(id: number): QueryResult[] {
        let l = this.data.get(id);
        return _.chain(l).sortBy(x => x.created).reverse().value();
    }
    
    public setActive(queryResultId: string, pageId: string): ResultStore {
        let tabId: number = null; // todo: pass from component instead
        let newList: QueryResult[] = null;

        for(let [key, queries] of this.data) {
            let query = queries.find(q => q.id === queryResultId); 
            if (query) {
                newList = queries;
                tabId = key;
                break;
            }
        }
        this.data.set(tabId, newList.map(qr => {
            if (qr.id === queryResultId) {
                qr.pages = qr.pages.map(p => {
                    const match = p.id === pageId;
                    // if we mark the current active, it will invert
                    p.active = match && p.active ? false : match;
                    return p; 
                });
            }
            return qr;
        }));
        return this;
    }
    
    public add(tabId: number, result: QueryResult): ResultStore {
        if (!this.data) {
            this.data = new Map<number, QueryResult[]>();
        }
        if (!this.data.has(tabId)) {
            this.data.set(tabId, []);
        }
        this.data.set(tabId, [result, ...this.data.get(tabId)]);
        return this;
    }
}
