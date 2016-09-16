import { Injectable } from '@angular/core';
import { Http, Response } from '@angular/http';
import { ReplaySubject, Subject, Observable, Observer } from 'rxjs/Rx';
import 'rxjs/Rx';
import { union, find, values } from 'lodash';
import { MirrorChangeStream } from './mirror-change.stream';
import { ConnectionService } from './connection.service';
import { TabService } from './tab.service';
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
            .withLatestFrom(tabs.activeBase.filter(x => x !== null && x !== undefined), (queryText, activeTabs) => {
                return <QueryRequest> {
                    tabId: activeTabs[0].id,
                    connectionId: activeTabs[0].connectionId,
                    text: queryText
                };
            });
            
        mirrorWithTab
            .subscribe(executing => {
                this.resultOps.next((store: ResultStore) => {
                    store.addLoading(executing.tabId);
                    return store;
                });
            });
        
        mirrorWithTab
            .withLatestFrom(conns.all.filter(x => x !== null && x !== undefined), (req, currentConns) => {
                const conn = currentConns.find(x => x.id === req.connectionId);
                return <QueryRequest> {
                    connectionString: conn.connectionString,
                    serverType: conn.type,
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
        result.finished = new Date();
        result.id = body.Id;
        Object.keys(body.Results).forEach((key, idx) => {
            const set = body.Results[key];
            const dataRows = set.Item2;
            const dataCols = set.Item1;
            const page = new ResultPage();
            page.columns = dataCols.map(col => col[0]);
            page.columnTypes = dataCols.map(col => col[1]);
            page.rows = dataRows.map((row) => {
                return dataCols.map(col => {
                    return row[col[0]];
                });
            });
            page.title = key;
            page.active = idx === 0; // mark the first as active
            page.id = `${idx}-${result.id}`;
            result.pages.push(page);
        });
        return result;
    }
    
    private action(name: string) {
        return `http://localhost:${this.port}/${name}`;
    }
}
