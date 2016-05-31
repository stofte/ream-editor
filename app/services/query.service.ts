import { Injectable } from '@angular/core';
import { Http, Response } from '@angular/http';
import { Subject, Observable, Observer } from 'rxjs/Rx';
import 'rxjs/Rx';
import { union, find, values } from 'lodash';
import { MirrorChangeStream } from '../services/mirror-change.stream';
import { ConnectionService } from '../services/connection.service';
import { TabService } from '../services/tab.service';
import { Connection } from '../models/connection';
import { QueryResult } from '../models/query-result';
import { TemplateResult } from '../models/template-result';
import { ResultPage } from '../models/result-page';
import { QueryRequest } from '../models/query-request';
import config from '../config';

@Injectable()
export class QueryService {
    private port: number = config.queryEnginePort;
    private subs: any = {};
    private data: any = {};
    private cachedTemplate = {};
    constructor(
        private tabs: TabService,
        private conns: ConnectionService,
        private mirror: MirrorChangeStream,
        private http: Http
    ) {
        let ts = tabs.tabs.publishReplay(1).refCount();
        let cs = conns.all.publishReplay(1).refCount();
        mirror
            .executing
            .withLatestFrom(tabs.tabs.filter(x => x !== null && x !== undefined), (queryText, tabs) => {
                return <QueryRequest> {
                    connectionId: tabs[0].connectionId,
                    text: queryText
                };
            })
            .withLatestFrom(conns.all.filter(x => x !== null && x !== undefined), (req, conns) => {
                return <QueryRequest> {
                    connectionString: conns.find(x => x.id === req.connectionId).connectionString,
                    text: req.text
                };
            })
            .flatMap(req => {
                return new Observable<QueryResult>((obs: Observer<QueryResult>) => {
                    const mapper: (value: any, int: number) => QueryResult = this.extractQueryResult.bind(this);
                    http.post(this.action('executequery'), JSON.stringify(req))
                        .map(mapper)
                        .subscribe(data => {
                            obs.next(data);
                            obs.complete();
                        });
                });
            })
            .subscribe(buf => {
                console.log('query saw execution of', buf);
            })
    }
    
    private extractQueryResult(res: Response): QueryResult {
        if (res.status < 200 || res.status >= 300) {
            throw new Error('Bad response status: ' + res.status);
        }
        let result = new QueryResult();
        let body = res.json();
        // todo need smarter dumper code in query-engine
        Object.keys(body.Results).forEach(key => {
            const raw = body.Results[key];
            const page = this.transformSet(raw);
            page.title = key;
            result.pages.push(page);
        });
        return result;
    }
    
    private transformSet(data: any[]): ResultPage {
        let page = new ResultPage();
        let cols: string[] = [];
        let rows: any[] = [];
        data.forEach(row => {
            cols = union(Object.keys(row), cols);
            // this also assumes all objects have same lay
            rows.push(values(row));
        });
        page.columns = cols;
        page.rows = rows;
        return page;
    }
    
    private action(name: string) {
        return `http://localhost:${this.port}/${name}`;
    }
}
