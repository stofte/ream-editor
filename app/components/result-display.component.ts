import { Component, Input, ElementRef, ChangeDetectorRef, AfterViewInit, AfterContentChecked, EventEmitter } from '@angular/core';
import { OutputStream, EventName } from '../streams/index';
import { TabService } from '../services/index';
import { QueryResult, ResultPage, SessionLogMessage } from '../models/index';

const Hypergrid = require('fin-hypergrid');
// disables the checkboxes
const hgImages = require('fin-hypergrid/images');
const hgDefaults = require('fin-hypergrid/src/defaults');
hgImages.checked = new Image();
hgImages.unchecked = new Image();
hgDefaults.columnAutosizing = false;
hgDefaults.showFilterRow = false;
hgDefaults.unsortable = true;
hgDefaults.editable = false;
hgDefaults.editorActivationKeys = [];
hgDefaults.columnHeaderHalign = 'left';
hgDefaults.halign = 'left';

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
                    class="vaadin-icons" *ngIf="page.isTabular">&#xe7a5;</i><span 
                        class="int-test-result-tab-title">{{
                        formatTabularName(page.title, page.rows.length)
                }}</span></button></div>
    </div>
    <div class="rm-result-display__tables {{ consoleActive ? 'rm-result-display__tables--inactive' : '' }}">
        <div class="rm-result-display__table"></div>
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
    private grids: any[] = [];

    // temp
    private grid: any;
    private gridContainer: any;

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
        this.gridContainer = this.elm.nativeElement.querySelector('.rm-result-display__table');

        this.grid = new Hypergrid(this.gridContainer, { data: [] });
        // console.log('grid', this.grid);
        this.viewHeight.filter(x => x > 0).subscribe(h => {
            this.gridContainer.style.height = `${h - 30}px`;
            // this.handsontableElm.updateSettings({ height: h - 30 });
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
        // console.log('data:', data);
        this.grid.behavior.setData(data, {
            filterable: false
        });
        // this.grid.addProperties();
        console.log('grid', this.grid);
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

    formatTabularName(name: string, count: number) {
        if (name.endsWith('[]')) {
            return `${name.substring(0, name.length - 2)}[${count}]`;
        }
        return name;
    }
}
