import { Component, Input, ElementRef, ChangeDetectorRef, AfterViewInit } from '@angular/core';
import { TableOptions, ColumnMode, SelectionType } from 'angular2-data-table';
import { QueryResult } from '../models/query-result';
import { ResultPage } from '../models/result-page';
import { OutputStream, EventName } from '../streams/index';

class ColumnSizing {
    width: number;
    column: number;
    fixed: boolean;
}

@Component({
    selector: 'rm-result-display',
    template: `
    <div class="rm-result-display__output-list">
        <div *ngFor="let output of outputList; let idx = index;">
            <paper-button (click)="selectResult(idx)" class="{{ idx === activePage ? 'active' : '' }}">
                <iron-icon icon="vaadin-icons:{{output.icon}}"></iron-icon>
                {{output.text}}
            </paper-button>
        </div>
    </div>
    <div class="rm-result-display__table">
        <div class="rm-result-display__table__hot"></div>
        <div class="rm-result-display__table__hider" *ngIf="enableHider"></div>
    </div>
`
})
export class ResultDisplayComponent implements AfterViewInit {
    resultPages: ResultPage[] = [];
    outputList = [];
    activePage: number = null;

    loadingPages: ResultPage[] = [];
    loadingList = [];

    enableHider = true;

    handsontableElm: any;
    tableOptions = {
        data: [],
        columns: [],
        colHeaders: [],
        afterLoadData: null,
        stretchH: 'last',
        height: null,
        rowHeaders: true,
        manualColumnResize: true,
        manualRowResize: true,
        fillHandle: false,
        // copy-limit is very confusing for enduser, maybe do something else ...
        copyRowsLimit: 32000,
        copyColsLimit: 32000
    };

    constructor(
        changeRef: ChangeDetectorRef,
        output: OutputStream,
        private elm: ElementRef
    ) {
        output.events
            .filter(msg => msg.name === EventName.ResultStart)
            .subscribe(msg => {
                this.loadingPages = [];
                this.loadingList = [];
            });

        output.events
            .filter(msg => msg.name === EventName.ResultUpdate)
            .subscribe(msg => {
                const tbl = <ResultPage> msg.data;
                const isFirst = this.resultPages.length === 0;
                this.loadingPages.push(tbl);
                this.loadingList.push({
                    text: tbl.title,
                    icon: 'table',
                    count: tbl.rows.length
                });
            });
        output.events
            .filter(msg => msg.name === EventName.ResultDone)
            .subscribe(msg => {
                this.enableHider = false;
                this.outputList = [...this.loadingList];
                this.resultPages = [...this.loadingPages];
                if (this.resultPages.length > 0) {
                    this.selectResult(0);
                }
            });
    }

    ngAfterViewInit() {
        const container = this.elm.nativeElement.querySelector('.rm-result-display__table__hot');
        this.tableOptions.afterLoadData = (isFirstLoad) => {
            if (!isFirstLoad) {
                this.handsontableElm.render();
            }
        };
        this.tableOptions.height = () => {
            const h = this.elm.nativeElement.querySelector('.rm-result-display__table').clientHeight;
            return ;
        };
        this.handsontableElm = new Handsontable(container, this.tableOptions);
    }

    selectResult(idx: number) {
        this.activePage = idx;
        const page = this.resultPages[idx];
        const data = [];
        page.rows.forEach(row => {
            data.push(page.isAtomic ? [row] : row);
        });
        this.handsontableElm.updateSettings({
            colHeaders: [...page.columns],
            columns: page.columns.map(col => {
                return {
                    editor: false
                };
            })
        });
        this.handsontableElm.loadData(data);
    }
}
