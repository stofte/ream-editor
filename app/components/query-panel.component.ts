import { Component, ElementRef, AfterViewInit, EventEmitter } from '@angular/core';
import { TabService } from '../services/tab.service';

@Component({
    selector: 'rm-query-panel',
    template: `
    <div class="rm-query-panel__session {{ this.sessionId ? 'rm-query-panel__session--active' : ''}}">
        <div class="rm-query-panel__editor">
            <rm-controls></rm-controls>
            <rm-editor [editor-props]="editorProps"></rm-editor>
        </div>
        <div class="rm-query-panel__seperator" (mousedown)="dragStart()"></div>
        <div class="rm-query-panel__result">
            <rm-result-display [view-height]="resultHeightEvents"></rm-result-display>
        </div>
    </div>
    <div class="rm-query-panel__blank" *ngIf="!this.sessionId">
        no session
    </div>
`
})
export class QueryPanelComponent implements AfterViewInit {
    sessionId: string = null;
    seperatorDragging = false;
    editorProps = { height: 150 };
    editorPanel: HTMLElement;
    resultPanel: HTMLElement;
    resultHeightEvents = new EventEmitter<number>();
    editorDefaultHeight = 160 + 65;
    constructor(
        private elm: ElementRef,
        tabs: TabService
    ) {
        tabs.currentSessionId.subscribe(id => {
            this.sessionId = id;
        });
    }

    ngAfterViewInit() {
        this.editorPanel = this.elm.nativeElement.querySelector('.rm-query-panel__editor');
        this.resultPanel = this.elm.nativeElement.querySelector('.rm-query-panel__result');
        this.layout(this.editorDefaultHeight);
        window.addEventListener('resize', () => {
            this.layout(this.editorDefaultHeight);
        });
    }

    dragStart() {
        console.log('dragStart');
        this.seperatorDragging = true;
        window.addEventListener('mousemove', this.dragging);
        window.addEventListener('mouseup', this.dragStop);
    }

    dragStop = (event: MouseEvent) => {
        this.seperatorDragging = true;
        window.removeEventListener('mousemove', this.dragging);
        window.removeEventListener('mouseup', this.dragStop);
    }

    dragging = (event: MouseEvent) => {
        this.editorDefaultHeight = event.clientY;
        this.layout(event.clientY);
    }

    layout(seperatorOffset: number) {
        const totalAvail = this.editorPanel.parentElement.clientHeight;
        const cmOffset = Math.max(0, seperatorOffset - 65);
        const edOffset = Math.max(5, seperatorOffset - 30);
        const resHeight = totalAvail - edOffset - 5;
        this.editorProps.height = cmOffset;
        this.editorPanel.style.height = edOffset + 'px';
        this.resultPanel.style.height = resHeight + 'px';
        this.resultHeightEvents.emit(resHeight);
    }
}
