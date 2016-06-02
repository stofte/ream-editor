import { Injectable } from '@angular/core';
import { Http, Response } from '@angular/http';
import { ReplaySubject, Subject, Observable, Observer } from 'rxjs/Rx';
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
import { ResultStore } from '../models/result-store';
import config from '../config';

@Injectable()
export class QueryService {
    private port: number = config.queryEnginePort;
    private stream: Observable<ResultStore>;
    private resultOps = new ReplaySubject<IStreamOperation>();
    
    constructor(
        tabs: TabService,
        conns: ConnectionService,
        mirror: MirrorChangeStream,
        http: Http
    ) {
        this.stream = this.resultOps
            .scan((store: ResultStore, op: IStreamOperation) => {
                return <ResultStore> op(store);
            }, new ResultStore())
            ;
            
        let mirrorWithTab = mirror
            .executing
            .withLatestFrom(tabs.activeBase.filter(x => x !== null && x !== undefined), (queryText, tabs) => {
                return <QueryRequest> {
                    tabId: tabs[0].id,
                    connectionId: tabs[0].connectionId,
                    text: queryText
                };
            });
            
        mirrorWithTab
            .subscribe(executing => {
                this.resultOps.next((store: ResultStore) => {
                    store.addLoading(executing.tabId);
                    return store;
                });;
            })
        
        mirrorWithTab
            .withLatestFrom(conns.all.filter(x => x !== null && x !== undefined), (req, conns) => {
                return <QueryRequest> {
                    connectionString: conns.find(x => x.id === req.connectionId).connectionString,
                    tabId: req.tabId,
                    text: req.text
                };
            })
            .flatMap(req => {
                return new Observable<QueryResult>((obs: Observer<QueryResult>) => {
                    const mapper: (value: any, int: number) => QueryResult = this.extractQueryResult.bind(this);
                    http.post(this.action('executequery'), JSON.stringify(req))
                        .map(mapper)
                        .subscribe(data => {
                            data.tabId = req.tabId;
                            data.connectionString = req.connectionString;
                            data.query = req.text;
                            obs.next(data);
                            obs.complete();
                        });
                });
            })
            .subscribe(res => {
                this.resultOps.next((store: ResultStore) => {
                    store.add(res.tabId, res);
                    return store;
                });
            });
            ;
    }
    
    public get activeResult(): Observable<ResultStore> {
        return this
            .stream
            ;
    }
    
    public setActivePage(queryId: string, pageId: string) {
        this.resultOps.next((store: ResultStore) => {
            return store.setActive(queryId, pageId);
        });
    }
    
    private extractQueryResult(res: Response): QueryResult {
        if (res.status < 200 || res.status >= 300) {
            throw new Error('Bad response status: ' + res.status);
        }
        let result = new QueryResult();
        let body = res.json();
        
        // todo: dont use server field, but grab roundtime from header
        result.finished = new Date(body.Created);
        result.id = body.Id;
        
        // todo need smarter dumper code in query-engine
        Object.keys(body.Results).forEach((key, idx) => {
            const set = body.Results[key];
            const dataRows = set.Item2;
            const dataCols = set.Item1;
            const page = this.transformSet(dataCols, dataRows);
            page.title = key;
            page.active = idx === 0; // mark the first as active
            page.id = `${idx}-${result.id}`;
            result.pages.push(page);
        });
        return result;
    }
    
    private transformSet(columns: any[], data: any[]): ResultPage {
        let page = new ResultPage();
        let cols: string[] = [];
        let rows: any[] = columns
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
