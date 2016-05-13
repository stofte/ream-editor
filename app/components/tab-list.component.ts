import { Component } from '@angular/core';
import { Router, ROUTER_DIRECTIVES } from '@angular/router-deprecated';
import { Location } from '@angular/common';
import { Observable } from 'rxjs/Observable';
import { TabService } from '../services/tab.service';

@Component({
    selector: 'f-tab-list',
    directives: [ROUTER_DIRECTIVES],
    template: `
<nav class="navbar navbar-default navbar-fixed-top">
    <div class="container-fluid" *ngIf="tabsEnabled">
        <div class="navbar-header">
            <ul class="nav navbar-nav">
                <li *ngFor="let tab of tabService.tabs"
                     class="{{tabService.active.id === tab.id ? 'active' : ''}}">
                    <a [routerLink]="['EditorTab', {tab: tab.id, connection: tab.connection.id, output: 'tab.output' }]">
                        Edit {{tab.title}}
                    </a>
                </li>
                <li>
                    <a (click)="newTab()" href="javascript:void(0)"><span class="glyphicon glyphicon-plus"></span></a>
                </li>
            </ul>
        </div>
    </div>
</nav>
`
})
export class TabListComponent {
    constructor(
        private tabService: TabService,
        
		private router: Router,
        private location: Location
    ) {
    }
    
    private newTab() {
		const activeConn = this.tabService.active.connection;
        this.tabService.newForeground(activeConn);
    }
    
    private get tabsEnabled(): boolean {
        // dont show tabs on start page
        return this.location.path().indexOf('/start') === -1;
    }
}
