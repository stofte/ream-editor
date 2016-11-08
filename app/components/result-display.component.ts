import { Component, Input, ElementRef, ChangeDetectorRef, AfterViewInit, AfterContentChecked, EventEmitter } from '@angular/core';
import { OutputStream, EventName } from '../streams/index';
import { TabService } from '../services/index';
import { QueryResult, ResultPage, SessionLogMessage } from '../models/index';

class ColumnSizing {
    width: number;
    column: number;
    fixed: boolean;
}

@Component({
    selector: 'rm-result-display',
    template: `
    <div class="rm-result-display__output-list">
        <div class="rm-result-display__tab {{ consoleActive ? 'rm-result-display__tab--active' : '' }}">
        <button (click)="selectConsole()"
            ><i class="material-icons">message</i><span>Console</span></button></div>
        <div *ngFor="let page of results; let idx = index;"
            class="rm-result-display__tab
                {{ page.resultId === activeId ? 'rm-result-display__tab--active' : '' }}">
            <button (click)="selectResult(page.resultId)"
                ><i class="vaadin-icons" *ngIf="page.isAtomic">&#xe7e0;</i><i
                    class="vaadin-icons" *ngIf="page.isTabular">&#xe7a5;</i><span>{{page.title
                }}</span></button></div>
    </div>
    <div class="rm-result-display__table {{ consoleActive ? 'rm-result-display__table--inactive' : '' }}">
        <div class="rm-result-display__table__hot"></div>
        <div class="rm-result-display__table__hider" *ngIf="enableHider"></div>
    </div>
    <div class="rm-result-display__console">
        <ul class="rm-result-display__console__listing">
            <li *ngFor="let msg of logMessages"
                class="rm-result-display__console__log">
                <span class="rm-result-display__console__date">
                    {{formatLogDate(msg.created)}}
                </span>
                <span class="rm-result-display__console__log--{{msg.type}}">{{msg.text}}</span>
            </li>
        </ul>
    </div>
`
})
export class ResultDisplayComponent implements AfterViewInit {
    @Input('view-height') public viewHeight: EventEmitter<number>;
    private results: ResultPage[] = [];
    private activeResult: ResultPage = null;
    private activeId: string = null;
    private sessionId: string;
    private scrollToTop = false;
    // hot scroll widget, used for blurActiveElement
    private scrollWidget = null;
    
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
        // todo do something to style inactive selection
        outsideClickDeselects: true,
        // copy-limit is confusing
        copyRowsLimit: 32000,
        copyColsLimit: 32000,
        afterLoadData: null,
        afterUpdateSettings: null,
        afterScrollVertically: null,
        afterScrollHorizontally: null,
        afterRender: null,
        afterSelection: null
    };

    private consoleActive = true;
    private logMessages: SessionLogMessage[] = null;
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
                this.handsontableElm.deselectCell();
                this.scrollToTop = false;
                const colOffset = this.activeResult.viewColumnOffset;
                const rowOffset = this.activeResult.viewRowOffset;
                this.handsontableElm.scrollViewportTo(rowOffset, colOffset);
            }
            if (!this.scrollWidget) {
                this.scrollWidget = this.elm.nativeElement.querySelector('.wtHolder');
                this.scrollWidget.addEventListener('mousedown', this.blurActiveElement);
            }
        };
        this.tableOptions.afterUpdateSettings = () => {
            if (this.dataLoader) {
                this.dataLoader();
                this.dataLoader = null;
            }
        };

        this.tableOptions.afterSelection = this.blurActiveElement;
        const scrollH = () => {
            const colOffset = this.hotColPlugin.getFirstVisibleColumn();
            const rowOffset = this.hotRowPlugin.getFirstVisibleRow();
            this.blurActiveElement();
            this.tabs.setResultPageView(
                this.sessionId,
                this.activeResult.resultId,
                colOffset,
                rowOffset
            );
        };
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

    // todo crude blurring of other elements when doing stuff in table.
    // hot doesnt seem to fire mouse events against document like other elements.
    // fx codemirror looses it's selection if the user clicks the scrollbar dragger.
    blurActiveElement = () => {
        const elm = <HTMLElement> document.activeElement;
        if (elm && elm.blur) {
            elm.blur();
        }
    }

    setSessionResults = (id: string, isSession = false) => {
        if (id) {
            const tab = this.tabs.sessions.find(x => x.id === id);
            this.logMessages = tab.sessionLog;
            if (tab && tab.results) {
                this.results = tab.results;
                const restoring = isSession && tab.consoleActive;
                if (this.results.length > 0 && !restoring) {
                    const resultId = tab.activeResultId || this.results[0].resultId;
                    this.selectResult(resultId);
                } else {
                    this.selectConsole();
                }
                this.enableHider = this.results.length === 0;
            }
        }
    }

    selectConsole() {
        this.consoleActive = true;
        this.activeId = null;
        this.tabs.setConsoleActive(this.sessionId);
    }

    selectResult(id: string) {
        this.consoleActive = false;
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

    formatLogDate(date: Date) {
        const h = date.getHours();
        const m = date.getMinutes();
        const s = date.getSeconds();
        const ms = date.getMilliseconds();

        const hStr = (h < 10 ? '0' : '') + h;
        const mStr = (m < 10 ? '0' : '') + m;
        const sStr = (s < 10 ? '0' : '') + s;
        let msStr = ('' + ms).substring(0, 3);
        while (msStr.length < 3) {
            msStr = '0' + msStr;
        }
        return `${hStr}:${mStr}:${sStr}.${msStr}`;
    }
}
