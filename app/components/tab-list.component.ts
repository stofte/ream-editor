import { Component, EventEmitter, Output, ChangeDetectorRef, AfterViewInit } from '@angular/core';
import { Location } from '@angular/common';
import { Tab } from '../models/tab';
import { Connection } from '../models/connection';
import { Observable } from 'rxjs/Observable';
import { TabService } from '../services/tab.service';
import { MonitorService } from '../services/monitor.service';
import { SortablejsOptions } from 'angular-sortablejs';
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
    <div class="rm-tab-list__tabs" 
        [sortablejs]="currentTabs" [sortablejsOptions]="options">
        <div *ngFor="let tab of currentTabs; let idx = index"
            class="rm-tab-list__tab {{ currentId === tab.id ? 'rm-tab-list__tab--active' : '' }}">
            <button (click)="clickTab(tab.id)"><span>{{tab.title}}</span></button>
            <span 
                class="rm-tab-list__tab-closebtn"
                title="Close"
                (click)="closeTab(tab.id)">
                <i class="material-icons">close</i>
            </span>
        </div>
    </div>
    <div class="rm-tab-list__newbtn">
        <button (click)="newTab()" title="New tab">
            <span>+</span>
        </button>
    </div>
`
})
export class TabListComponent implements AfterViewInit {
    options: SortablejsOptions = {
        animation: 150,
        
    };
    private currentTabs: Tab[] = [];
    private currentId: string;
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
        
    }

    public ngAfterViewInit() {
        this.tabService.currentSessionId.subscribe(id => {
            if (!this.currentTabs.find(x => x.id === id)) {
                console.log('component saw new tab', id)
                const newTab = this.tabService.sessions.find(x => x.id === id);
                this.currentTabs.push(newTab);
            }
            this.currentId = id;
        });
        setTimeout(() => this.tabService.newSession());
    }

    private newTab() {
        this.tabService.newSession();
    }

    private clickTab(id: string) {
        console.log('clickTab', id);
        this.tabService.currentSession(id);
    }

    private closeTab(id: string) {
        let idx = -1;
        this.currentTabs.forEach((x, i) => {
            if (x.id === id) idx = i;
        });
        Assert(idx !== -1, 'Tab id not found when closing tab');
        this.currentTabs.splice(idx, 1);
        this.currentId = null;
        console.log('closed ' + id,JSON.stringify(this.currentTabs));
        this.tabService.closeSession(id);
    }

    // private getText(idx) {
    //     return this.currentTabs[idx].title;
    // }
    
    // private closeTab(id: string) {
    //     this.currentTabs = this.currentTabs.filter(x => x.id !== id);
    //     if (this.currentTabs.length > 0) {
    //         const newSession = this.currentTabs[this.activeId].id;
            
    //         this.tabService.currentSession(newSession);
    //     } else {
    //         this.tabService.currentSession(null);
    //         this.activeId = null;
    //     }
    //     this.tabService.deleteSession(id);
    // }

    // private mouseEnter(idx: number) {
    //     this.hoverTargetTab = idx;
    // }

    // private mouseLeave(idx: number) {
    //     if (this.hoverTargetTab === idx) {
    //         this.hoverTargetTab = null;
    //     }
    // }


    // private handleTabs(index: number, id: string) {
    //     const changed = this.activeId !== index;
    //     this.activeId = index;
    //     if (changed) {
    //         this.tabService.currentSession(id);
    //     }
    // }

    // private dragStart(idx: number, id: string) {
    //     this.draggedTab = this.currentTabs.find(x => x.id === id);
    //     this.draggedTabIndex = idx;
    //     // needs to run in a timeout, as otherwise the droptarget layers
    //     // will interfere with the dragging action and cancel it out.
    //     // if the user drops the tab within 100ms, we cancel this timeout.
    //     this.isDraggingTimeout = setTimeout(() => {
    //         this.isDraggingTimeout = null;
    //         this.isDragging = true;
    //     }, 100);
    // }

    // private dragEnd(tablist: HTMLElement) {
    //     if (this.isDraggingTimeout) { // set in this.dragStart
    //         clearTimeout(this.isDraggingTimeout);
    //     }
    //     this.isDragging = false;
    //     this.dragTargetTab = null;
    //     tablist.classList.remove('rm-tablist__current-droptarget');
    // }

    // private dragEnter(elm: HTMLElement) {
    //     elm.classList.add('rm-tablist__current-droptarget');
    // }

    // private dragLeave(elm: HTMLElement) {
    //     elm.classList.remove('rm-tablist__current-droptarget');
    // }

    // private dragEnterTab(idx: number) {
    //     this.dragTargetTab = idx;
    // }

    // private dragLeaveTab(idx: number) {
    //     if (this.dragTargetTab === idx) {
    //         this.dragTargetTab = null;
    //     }
    // }

    // private dropTabList() {
    //     setTimeout(() => {
    //         this.currentTabs = this.currentTabs
    //             .filter(x => x.id !== this.draggedTab.id)
    //             .concat([
    //                 <Tab> {
    //                     id: this.draggedTab.id,
    //                     title: this.draggedTab.title,
    //                     connectionId: this.draggedTab.connectionId
    //                 }]);
    //         this.activeId = this.currentTabs.length - 1;
    //     }, 100);
    // }

    // private dropTab(idx: number, id: string) {
    //     if (id === this.draggedTab.id) {
    //         return;
    //     }
    //     setTimeout(() => {
    //         const idxMod = idx < this.draggedTabIndex ? 0 : 1;
    //         const firstTabs = this.currentTabs.slice(0, idx + idxMod).filter(x => x.id !== this.draggedTab.id);
    //         const secondTabs = this.currentTabs.slice(idx + idxMod).filter(x => x.id !== this.draggedTab.id);
    //         this.currentTabs = firstTabs.concat([
    //             <Tab> {
    //                 id: this.draggedTab.id,
    //                 title: this.draggedTab.title,
    //                 connectionId: this.draggedTab.connectionId
    //             }
    //         ]).concat(secondTabs);
    //         this.activeId = firstTabs.length;
    //         const elm = <HTMLElement> document.querySelector('.rm-tablist');
    //         elm.style.opacity = '0.999';
    //         requestAnimationFrame(() => {
    //             elm.style.opacity = '1';
    //         });
    //     }, 100);
    // }
}
