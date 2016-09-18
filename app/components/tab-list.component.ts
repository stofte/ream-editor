import { Component } from '@angular/core';
// import { Router, ROUTER_DIRECTIVES } from '@angular/router-deprecated';
import { Location } from '@angular/common';
import { Tab } from '../models/tab';
import { Connection } from '../models/connection';
import { Observable } from 'rxjs/Observable';
import { TabService } from '../services/tab.service';
import { MonitorService } from '../services/monitor.service';
// import { DROPDOWN_DIRECTIVES } from 'ng2-bootstrap/ng2-bootstrap';

@Component({
    selector: 'f-tab-list',
    // directives: [ROUTER_DIRECTIVES, DROPDOWN_DIRECTIVES],
    template: `
<nav class="navbar navbar-default navbar-fixed-top">
    <div class="container-fluid int-test-tab-list">
        <div class="navbar-header {{viewTitleClass}}">
            <a class="navbar-brand">{{viewTitle}}</a>
        </div>
        <div class="navbar-collapse collapse" *ngIf="tabsEnabled">
            <ul class="nav navbar-nav">
                <li *ngFor="let tab of currentTabs"
                     class="{{tab.active ? 'active' : ''}}">
                    <a (click)="goto(tab.id)" href="javascript:void 0">
                        {{tab.title}}
                    </a>
                </li>
                <li>
                    <a (click)="newTab()" href="javascript:void 0"><span class="glyphicon glyphicon-plus"></span></a>
                </li>
            </ul>
        </div>
    </div>
</nav>
`
})
export class TabListComponent {
    private title = '';
    private currentTabs: Tab[] = [];
    private tabsEnabled = false;
    constructor(
        private tabs: TabService
        // private tabService: TabService,
        // private router: Router,
        // private location: Location
    ) {
        tabs.tabs
            .subscribe(ts => {
                this.currentTabs = ts;
                this.tabsEnabled = ts.length > 0;
            });
    }
    
    private newTab() {
        this.tabs.newTab();
    }
    
    private goto(id: number) {
        this.tabs.goto(id);
    }
    
    private get viewTitle(): string {
        return this.tabsEnabled ? '' : 'Hello!';
    }
    
    private get viewTitleClass(): string {
        return this.tabsEnabled ? 'hidden' : '';
    }
}
