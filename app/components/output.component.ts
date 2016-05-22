import { Component } from '@angular/core';
import { RouteParams, Router } from '@angular/router-deprecated';
import { Subject } from 'rxjs/Subject';
import { QueryService } from '../services/query.service';
import { TabService } from '../services/tab.service';
import { QueryResult } from '../models/query-result';
import { ResultPage } from '../models/result-page';

@Component({
    selector: 'f-output',
    template: `
<div class="row" style="margin-bottom:10px">
    <div class="col-md-12">
        <div class="btn-group" role="group">
            <button 
                *ngFor="let page of pages"
                (click)="showResult(page.index)"
                type="button" class="btn btn-default {{page.index === activeResultPage ? 'active' : ''}}">
                {{page.text}}
            </button>
        </div>
    </div>
</div>
<div class="row" style="overflow: auto" *ngIf="activeResultPage > -1">
    <div class="col-md-12" *ngFor="let page of pages">
        <div *ngIf="page.index === activeResultPage">
            <table class="table table-condensed output-table">
                <thead>
                    <tr>
                        <th *ngFor="let head of page.columns" [innerText]="head"></th>
                    </tr>
                </thead>
                <tbody>
                    <tr *ngFor="let row of page.rows">
                        <td *ngFor="let cell of row"
                            ><textarea readonly rows="1" [value]="cell"></textarea></td>
                    </tr>
                </tbody>
            </table>
        </div>
    </div>
</div>
`
})
export class OutputComponent {
    tabId: number;
    connectionId: number;
    activeResultPage: number = null;
    pages: any[] = [];
    results: any[][] = [];
    
    constructor(
        private queryService: QueryService,
        private tabService: TabService,
        private router: Router,
        private routeParams: RouteParams
    ) {
        this.tabId = parseInt(routeParams.get('tab'), 10);
        const tab = tabService.get(this.tabId);
        this.connectionId = parseInt(routeParams.get('connection'), 10);
        let mapper = (x: ResultPage, i) => { 
            return { text: x.title, index: i, rows: x.rows, columns: x.columns }; 
        };
        let previous = queryService.loaded(this.tabId);
        this.pages = previous.length > 0 ? previous[0].pages.map(mapper) : [];
        this.activeResultPage = tab.output;

        queryService
            .results(this.tabId)
            .subscribe(result => {
                this.pages = result.pages.map(mapper);
                if (this.pages.length > 0) {
                    let tab = this.tabService.get(this.tabId);
                    tab.output = 0;
                    this.activeResultPage = tab.output;
                    this.tabService.updateTab(tab);
                }
            });
    }
    
    showResult(idx: number) {
        let tab = this.tabService.get(this.tabId);
        tab.output = idx;
        this.activeResultPage = idx;
        this.tabService.updateTab(tab);
    }
}
