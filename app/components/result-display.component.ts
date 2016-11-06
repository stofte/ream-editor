import { Component, Input, ElementRef, ChangeDetectorRef, AfterViewInit, AfterContentChecked, EventEmitter } from '@angular/core';
import { TableOptions, ColumnMode, SelectionType } from 'angular2-data-table';
import { QueryResult } from '../models/query-result';
import { ResultPage } from '../models/result-page';
import { OutputStream, EventName } from '../streams/index';
import { TabService } from '../services/index';

class ColumnSizing {
    width: number;
    column: number;
    fixed: boolean;
}

@Component({
    selector: 'rm-result-display',
    template: `
    <div class="rm-result-display__output-list">
        <div *ngFor="let page of results; let idx = index;"
            class="rm-result-display__tab
                {{ page.resultId === activeId ? 'rm-result-display__tab--active' : '' }}">
            <button (click)="selectResult(page.resultId)">
                <i class="vaadin-icons" *ngIf="page.isAtomic">&#xe7e0;</i>
                <i class="vaadin-icons" *ngIf="page.isTabular">&#xe7a5;</i>
                <span>{{page.title}}</span>
            </button>
        </div>
    </div>
    <div class="rm-result-display__table">
        <div class="rm-result-display__table__hot"></div>
        <div class="rm-result-display__table__hider" *ngIf="enableHider"></div>
    </div>
`
})
export class ResultDisplayComponent implements AfterViewInit {
    @Input('view-height') public viewHeight: EventEmitter<number>;
    private results: ResultPage[] = [];
    private activeResult: ResultPage = null;
    private activeId: string = null;
    private sessionId: string;
    private resetActiveId = true;
    private scrollToTop = false;
    
    private enableHider = true;
    private firstLoad = true;
    private dataLoader: Function = null;
    private handsontableElm: any;
    private hotRowPlugin: any;
    private hotColPlugin: any;
    
    private tableOptions = {
        data: [],
        columns: [],
        colHeaders: [],
        stretchH: 'last',
        height: null,
        rowHeaders: true,
        manualColumnResize: true,
        manualRowResize: true,
        fillHandle: false,
        // copy-limit is confusing
        copyRowsLimit: 32000,
        copyColsLimit: 32000,
        afterLoadData: null,
        afterUpdateSettings: null,
        afterScrollVertically: null,
        afterScrollHorizontally: null,
        afterRender: null
    };

    constructor(
        private tabs: TabService,
        private output: OutputStream,
        private elm: ElementRef
    ) {
        tabs.currentSessionId.subscribe(id => {
            this.sessionId = id;
            this.setSessionResults(id, true);
        });
        tabs.tabResultsUpdated.subscribe(id => {
            if (this.sessionId === id) {
                this.setSessionResults(id);
            }
        });
        output.events
            .filter(msg => msg.name === EventName.ResultStart)
            .subscribe(msg => {
                if (msg.id === this.sessionId) {
                    this.resetActiveId = true;
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
        this.tableOptions.afterRender = (isForced: boolean) => {
            if (isForced && this.scrollToTop) {
                this.scrollToTop = false;
                const colOffset = this.activeResult.viewColumnOffset;
                const rowOffset = this.activeResult.viewRowOffset;
                this.handsontableElm.scrollViewportTo(rowOffset, colOffset);
            }
        };
        this.tableOptions.afterUpdateSettings = () => {
            if (this.dataLoader) {
                this.dataLoader();
                this.dataLoader = null;
            }
        };
        const scrollH = () => {
            const colOffset = this.hotColPlugin.getFirstVisibleColumn();
            const rowOffset = this.hotRowPlugin.getFirstVisibleRow();
            this.tabs.setResultPageView(
                this.sessionId,
                this.activeResult.resultId,
                colOffset,
                rowOffset
            );
        }
        this.tableOptions.afterScrollVertically = scrollH;
        this.tableOptions.afterScrollHorizontally = scrollH;
        this.handsontableElm = new Handsontable(container, this.tableOptions);
        this.hotRowPlugin = this.handsontableElm.getPlugin('autoRowSize');
        this.hotColPlugin = this.handsontableElm.getPlugin('autoColumnSize');
        this.hotColPlugin.enablePlugin();
        this.hotRowPlugin.enablePlugin();
        
        this.viewHeight.filter(x => x > 0).subscribe(h => {
            this.handsontableElm.updateSettings({ height: h - 30 });
        });
    }

    setSessionResults = (id: string, forceSelect = false) => {
        if (id) {
            const tab = this.tabs.sessions.find(x => x.id === id);
            if (tab && tab.results) {
                this.results = tab.results;
                if (forceSelect || this.resetActiveId) {
                    if (this.results.length > 0) {
                        this.resetActiveId = false;
                        const resultId = tab.activeResultId || this.results[0].resultId;
                        this.selectResult(resultId);
                    }
                    this.enableHider = this.results.length === 0;
                }
            }
        }
    }

    selectResult(id: string) {
        this.activeId = id;
        const page = this.results.find(x => x.resultId === id);
        const data = [];
        page.rows.forEach(row => {
            data.push(page.isAtomic ? [row] : row);
        });
        // need to wait for updated settings before loading data
        this.scrollToTop = true;
        this.dataLoader = () => {
            this.handsontableElm.loadData(data);
        };
        this.activeResult = page;
        this.tabs.setActiveResult(this.sessionId, id);
        this.handsontableElm.updateSettings({
            colHeaders: [...page.columns],
            columns: page.columns.map(col => {
                return {
                    editor: false,
                    wordWrap: false
                };
            })
        });
    }
}
