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
            <mdl-chip mdl-label="{{output.text}}" (click)="selectResult(idx)">
                <span mdl-chip-contact class="mdl-color--blue mdl-color-text--white">
                    <iron-icon icon="vaadin-icons:{{output.icon}}"></iron-icon>
                </span>
            </mdl-chip>
        </div>
    </div>
    <div class="rm-result-display__table">

    </div>
`
})
export class ResultDisplayComponent implements AfterViewInit {
    resultPages: ResultPage[] = [];
    outputList = [];

    handsontableElm: any;
    tableOptions = {
        data: [],
        colHeaders: [],
        afterLoadData: null
    };

    constructor(
        changeRef: ChangeDetectorRef,
        output: OutputStream,
        private elm: ElementRef
    ) {
        output.events
            .filter(msg => msg.name === EventName.ResultStart)
            .subscribe(msg => {
                this.outputList = [];
                this.resultPages = [];
            });
        output.events
            .filter(msg => msg.name === EventName.ResultUpdate)
            .subscribe(msg => {
                const tbl = <ResultPage> msg.data;
                const isFirst = this.resultPages.length === 0;
                this.resultPages.push(tbl);
                this.outputList.push({
                    text: tbl.title,
                    icon: 'table'
                });
                if (isFirst) {
                    this.selectResult(0);
                }
            });
        output.events
            .filter(msg => msg.name === EventName.ResultDone)
            .subscribe(msg => {
                console.log('results done', msg.id);
            });
    }

    ngAfterViewInit() {
        const container = this.elm.nativeElement.querySelector('.rm-result-display__table');
        this.tableOptions.afterLoadData = (isFirstLoad) => {
            if (!isFirstLoad) {
                this.handsontableElm.render();
            }
        }
        this.handsontableElm = new Handsontable(container, this.tableOptions);
    }

    selectResult(idx: number) {
        const page = this.resultPages[idx];
        const data = [];
        page.rows.forEach(row => {
            data.push(page.isAtomic ? [row] : row);
        });
        this.handsontableElm.updateSettings({
            colHeaders: [...page.columns]
        });
        this.handsontableElm.loadData(data);
    }
}
