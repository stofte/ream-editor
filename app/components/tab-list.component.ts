import { Component, EventEmitter, Output, ChangeDetectorRef } from '@angular/core';
import { Location } from '@angular/common';
import { Tab } from '../models/tab';
import { Connection } from '../models/connection';
import { Observable } from 'rxjs/Observable';
import { TabService } from '../services/tab.service';
import { MonitorService } from '../services/monitor.service';
import * as uuid from 'node-uuid';

let runningCount = 0;

function findParent(elm: HTMLElement, cond: (elm: HTMLElement) => boolean) {
    if (cond(elm)) {
        return elm;
    } else {
        return findParent(elm.parentElement, cond);
    }
}

@Component({
    selector: 'rm-tab-list',
    template: `
        <div class="rm-tablist" #tablistElm>
            <div *ngFor="let tab of currentTabs; let idx = index"
                class="rm-tab-list__tab {{ idx === activeIndex ? 'is-active' : '' }}
                    {{ dragTargetTab === idx ? 'rm-tablist__current-droptarget' : '' }}
                    {{ hoverTargetTab === idx ? 'is-visible' : '' }}"
                    (mouseenter)="mouseEnter(idx)"
                    (mouseleave)="mouseLeave(idx)">
                <button mdl-button
                    draggable="true"
                    (mousedown)="handleTabs(idx, tab.id)"
                    (dragstart)="dragStart(idx, tab.id)"
                    (dragend)="dragEnd(tablistElm)">
                    <span>{{tab.title}}</span>
                </button>
                <mdl-icon
                    class="rm-tab-list__tab-closebtn"
                    title="Close"
                    (click)="closeTab(tab.id)">close</mdl-icon>
                <div *ngIf="isDragging"
                    class="rm-tab-list__tab-droptarget"
                    (dragover)="$event.preventDefault()"
                    (dragenter)="dragEnterTab(idx)"
                    (dragleave)="dragLeaveTab(idx)"
                    (drop)="dropTab(idx, tab.id)"></div>
            </div>
            <div class="rm-tablist__newbtn">
                <button mdl-button mdl-button-type="icon" (click)="newTab()" title="New tab">
                    <mdl-icon>add</mdl-icon>
                </button>
            </div>
            <div *ngIf="isDragging"
                class="rm-tablist__tab-droptarget"
                (dragover)="$event.preventDefault()"
                (dragenter)="dragEnter(tablistElm)"
                (dragleave)="dragLeave(tablistElm)"
                (drop)="dropTabList()"
                ></div>
        </div>
`
})
export class TabListComponent {
    private currentTabs: Tab[] = [];
    private activeIndex = 0;
    private isDragging = false;
    private draggedTab: Tab = null;
    private draggedTabIndex: number = null;
    private dragTargetTab: number = null;
    private hoverTargetTab: number = null;
    private isDraggingTimeout = null;
    constructor(
        private tabService: TabService,
        private ref: ChangeDetectorRef 
    ) {
        this.newTab();
    }
    
    private closeTab(id: string) {
        this.currentTabs = this.currentTabs.filter(x => x.id !== id);
        if (this.activeIndex >= this.currentTabs.length) {
            this.activeIndex = this.currentTabs.length - 1;
        }
        const newSession = this.currentTabs[this.activeIndex].id;
        this.tabService.currentSession(newSession);
    }

    private mouseEnter(idx: number) {
        this.hoverTargetTab = idx;
    }

    private mouseLeave(idx: number) {
        if (this.hoverTargetTab === idx) {
            this.hoverTargetTab = null;
        }
    }

    private newTab() {
        const newId = uuid.v4();
        const runId = runningCount++;
        this.currentTabs.push(<Tab> { id: newId, title: `Untitled ${runId}` });
        this.activeIndex = this.currentTabs.length - 1;
        this.tabService.newSession(newId);
    }

    private handleTabs(index: number, id: string) {
        const changed = this.activeIndex !== index;
        this.activeIndex = index;
        if (changed) {
            this.tabService.currentSession(id);
        }
    }

    private dragStart(idx: number, id: string) {
        this.draggedTab = this.currentTabs.find(x => x.id === id);
        this.draggedTabIndex = idx;
        // needs to run in a timeout, as otherwise the droptarget layers
        // will interfere with the dragging action and cancel it out.
        // if the user drops the tab within 100ms, we cancel this timeout.
        this.isDraggingTimeout = setTimeout(() => {
            this.isDraggingTimeout = null;
            this.isDragging = true;
        }, 100);
    }

    private dragEnd(tablist: HTMLElement) {
        if (this.isDraggingTimeout) { // set in this.dragStart
            clearTimeout(this.isDraggingTimeout);
        }
        this.isDragging = false;
        this.dragTargetTab = null;
        tablist.classList.remove('rm-tablist__current-droptarget');
    }

    private dragEnter(elm: HTMLElement) {
        elm.classList.add('rm-tablist__current-droptarget');
    }

    private dragLeave(elm: HTMLElement) {
        elm.classList.remove('rm-tablist__current-droptarget');
    }

    private dragEnterTab(idx: number) {
        this.dragTargetTab = idx;
    }

    private dragLeaveTab(idx: number) {
        if (this.dragTargetTab === idx) {
            this.dragTargetTab = null;
        }
    }

    private dropTabList() {
        setTimeout(() => {
            this.currentTabs = this.currentTabs
                .filter(x => x.id !== this.draggedTab.id)
                .concat([
                    <Tab> {
                        id: this.draggedTab.id,
                        title: this.draggedTab.title,
                        connectionId: this.draggedTab.connectionId
                    }]);
            this.activeIndex = this.currentTabs.length - 1;
        }, 100);
    }

    private dropTab(idx: number, id: string) {
        if (id === this.draggedTab.id) {
            return;
        }
        setTimeout(() => {
            const idxMod = idx < this.draggedTabIndex ? 0 : 1;
            const firstTabs = this.currentTabs.slice(0, idx + idxMod).filter(x => x.id !== this.draggedTab.id);
            const secondTabs = this.currentTabs.slice(idx + idxMod).filter(x => x.id !== this.draggedTab.id);
            this.currentTabs = firstTabs.concat([
                <Tab> {
                    id: this.draggedTab.id,
                    title: this.draggedTab.title,
                    connectionId: this.draggedTab.connectionId
                }
            ]).concat(secondTabs);
            this.activeIndex = firstTabs.length;
            const elm = <HTMLElement> document.querySelector('.rm-tablist');
            elm.style.opacity = '0.999';
            requestAnimationFrame(() => {
                elm.style.opacity = '1';
            });
        }, 100);
    }
}
