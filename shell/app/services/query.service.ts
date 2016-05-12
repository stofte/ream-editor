import { Injectable } from '@angular/core';
import { Http, Response } from '@angular/http';
import { Router } from '@angular/router-deprecated';
import { Subject, Observable } from 'rxjs/Rx';
import 'rxjs/Rx';
import { union, find, values } from 'lodash';
import { Connection } from '../models/connection';
import { QueryResult } from '../models/query-result';
import { TemplateResult } from '../models/template-result';
import { ResultPage } from '../models/result-page';
import config from '../config';

class SubscriberMap {
    
}

@Injectable()
export class QueryService {
    private port: number = config.queryEnginePort;
    private subs: any = {};
    private data: any = {};
    private cachedTemplate = {};
    constructor(
        private http : Http,
        private router: Router
    ) {
    }
    
    public run(tabId: number, connection: Connection, text: string)  {
        const json = JSON.stringify({
            connectionString: connection.connectionString,
            text: text
        });
        const result = new QueryResult();
        const f: (value: any, int: number) => QueryResult = this.extractQueryResult.bind(this);
        let k ='_p_' + tabId;
        this.data[k] = []; // dump any previous results
        this.http
            .post(this.action('executequery'), json)
            .map(f)
            .subscribe(result => {
                this.data[k].push(result);
                this.subs[k].next(result);
            });
    }
    
    public queryTemplate(connection: Connection): Observable<TemplateResult> {
        if (this.cachedTemplate[connection.id]) {
            return new Observable<TemplateResult>(obs => {
                obs.next(this.cachedTemplate[connection.id]);
                obs.completed();
            });
        }
        let data = {
            connectionString: connection.connectionString,
            text: ''  
        };
        return this.http
            .post(this.action('querytemplate'), JSON.stringify(data))
            .map(res => res.json())
            .map(json => {
                let m = new TemplateResult();
                m.namespace = json.Namespace;
                m.template = json.Template;
                m.header = json.Header;
                m.footer = json.Footer;
                m.lineOffset = json.LineOffset;
                return m;
            });
    }
    
    public loaded(tabId: number): QueryResult[] {
        return this.data['_p_' + tabId] || [];
    }
    
    public results(tabId: number): Subject<QueryResult> {
        let sub = new Subject<QueryResult>();
        this.subs['_p_' + tabId] = sub;
        return sub;
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
        var page = new ResultPage();
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
    
    private action(name : string) {
        return `http://localhost:${this.port}/${name}`;
    }
}