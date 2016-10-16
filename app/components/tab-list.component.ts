import { Component, ElementRef, OnInit } from '@angular/core';
// import { Router, ROUTER_DIRECTIVES } from '@angular/router-deprecated';
import { Location } from '@angular/common';
import { Tab } from '../models/tab';
import { Connection } from '../models/connection';
import { Observable } from 'rxjs/Observable';
import { TabService } from '../services/tab.service';
import { MonitorService } from '../services/monitor.service';
// import { DROPDOWN_DIRECTIVES } from 'ng2-bootstrap/ng2-bootstrap';

@Component({
    selector: 'rm-tab-list',
    // directives: [ROUTER_DIRECTIVES, DROPDOWN_DIRECTIVES],
    template: `
        <mdl-tabs mdl-ripple mdl-tab-active-index="0">
            <mdl-tab-panel>
                <mdl-tab-panel-title>
                    <span>Untitled 1</span>
                </mdl-tab-panel-title>
                <mdl-tab-panel-content></mdl-tab-panel-content>
            </mdl-tab-panel>
            <mdl-tab-panel>
                <mdl-tab-panel-title>
                    <mdl-icon>add</mdl-icon>
                </mdl-tab-panel-title>
                <mdl-tab-panel-content></mdl-tab-panel-content>
            </mdl-tab-panel>
        </mdl-tabs>
`
})
export class TabListComponent {
    private title = '';
    private currentTabs: Tab[] = [];
    private tabsEnabled = false;
    constructor() {
        // tabs.tabs
        //     .subscribe(ts => {
        //         this.currentTabs = ts;
        //         this.tabsEnabled = ts.length > 0;
        //     });
    }
    
    private newTab() {
        // this.tabs.newTab();
    }

    public ngOnInit() {
        // componentHandler.upgradeElement(this.element.nativeElement);
    }
    
    private goto(id: number) {
        // this.tabs.goto(id);
    }
    
    private get viewTitle(): string {
        return this.tabsEnabled ? '' : 'Hello!';
    }
    
    private get viewTitleClass(): string {
        return this.tabsEnabled ? 'hidden' : '';
    }
}
