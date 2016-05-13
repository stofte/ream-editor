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
            <button type="button" class="btn btn-default" 
                (click)="showConsole()">Console</button>
            <button 
                
                *ngFor="let page of pages"
                (click)="showResult(page.index)"
                type="button" class="btn btn-default">
                {{page.text}}
            </button>
        </div>
    </div>
</div>
<div class="row" *ngIf="consoleVisible">
    <div class="col-md-12">
        <div class="well well-sm">
            <p>console</p>
        </div>
    </div>
</div>
<div class="row" *ngIf="activeResultPage > -1">
    <div class="col-md-12" *ngFor="let page of pages">
        <div *ngIf="page.index === activeResultPage">
            <table class="table table-condensed">
                <thead>
                    <tr>
                        <th *ngFor="let head of page.columns">
                            {{head}}
                        </th>
                    </tr>
                </thead>
                <tbody>
                    <tr *ngFor="let row of page.rows">
                        <td *ngFor="let cell of row">
                            {{cell}}
                        </td>
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
    consoleVisible: boolean;
    outputParam: any;
    activeResultPage: number = null;
    pages: any[] = [];
    results: any[][] = [];
    
    constructor(
        private queryService: QueryService,
        private tabService: TabService,
        private router : Router,
        private routeParams: RouteParams
    ) {
        this.tabId = parseInt(routeParams.get('tab'), 10);
        console.log('output.tabId', this.tabId);
        this.connectionId = parseInt(routeParams.get('connection'), 10);
        this.outputParam = this.routeParams.get('output');
        this.consoleVisible = this.outputParam === 'console';
        let mapper = (x: ResultPage, i) => { 
            return { text: x.title, index: i, rows: x.rows, columns: x.columns }; 
        };
        let previous = queryService.loaded(this.tabId);
        this.pages = previous.length > 0 ? previous[0].pages.map(mapper) : [];
        this.activeResultPage = parseInt(this.outputParam, 10);

        queryService
            .results(this.tabId)
            .subscribe(result => {
                // once results tick in, and user is looking at the console, 
                // then change tab to first data table
                if (this.consoleVisible && result.pages.length > 0) {
                    this.router.navigate(["EditorTab", { tab: this.tabId, connection: this.connectionId, output: 0 }]);
                }
                this.pages = result.pages.map(mapper);
            });
    }
    
    attached() {
        let tab = this.tabService.active;
        console.log('output.attached', tab.id, this.outputParam);
        tab.title = "foo";
        tab.output = this.outputParam;
        this.tabService.updateTab(tab);
    }
        
    showResult(idx: number) {
        this.router.navigate(["EditorTab", { tab: this.tabId, connection: this.connectionId, output: idx }]);
    }
    
    showConsole()  {
        this.router.navigate(["EditorTab", { tab: this.tabId, connection: this.connectionId, output: 'console' }]);
    }
}
