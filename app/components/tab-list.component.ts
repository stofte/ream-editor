import { Component, ElementRef, OnInit } from '@angular/core';
// import { Router, ROUTER_DIRECTIVES } from '@angular/router-deprecated';
import { Location } from '@angular/common';
import { Tab } from '../models/tab';
import { Connection } from '../models/connection';
import { Observable } from 'rxjs/Observable';
import { TabService } from '../services/tab.service';
import { MonitorService } from '../services/monitor.service';
// import { DROPDOWN_DIRECTIVES } from 'ng2-bootstrap/ng2-bootstrap';

let tabId = 0;

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
        <div class="rm-tablist">
            <div *ngFor="let tab of currentTabs; let idx = index"
                class="rm-tab-list__tab {{ idx === activeIndex ? 'is-active' : '' }}">
                <button mdl-button mdl-ripple (click)="handleTabs(idx, tab.id)">
                    {{tab.title}}
                </button>
                <mdl-icon
                    class="rm-tab-list__tab-closebtn"
                    title="Close"
                    (click)="closeTab(tab.id)">clear</mdl-icon>
            </div>
            <div class="rm-tablist__newbtn">
                <button mdl-button mdl-ripple mdl-button-type="icon" (click)="newTab()" title="New tab">
                    <mdl-icon>add</mdl-icon>
                </button>
            </div>
        </div>
`
})
export class TabListComponent {
    private currentTabs: Tab[] = [];
    private activeIndex = 0;
    
    constructor() {
        this.newTab();
    }
    
    private closeTab(id: number) {
        this.currentTabs = this.currentTabs.filter(x => x.id !== id);
        if (this.activeIndex >= this.currentTabs.length) {
            this.activeIndex = this.currentTabs.length - 1;
        }
    }

    private newTab() {
        const newId = tabId++;
        this.currentTabs.push(<Tab> { id: newId, title: `Untitled ${newId}` });
        this.activeIndex = this.currentTabs.length - 1;
    }

    private handleTabs(index: number, tabId: number) {
        this.activeIndex = index;
    }
}
