import { Component, ViewChildren, QueryList } from '@angular/core';
import { Router, RouteParams } from '@angular/router-deprecated';
import { ConnectionService } from '../services/connection.service';
import { Connection } from '../models/connection';
import { TabService } from '../services/tab.service';
import { DROPDOWN_DIRECTIVES } from 'ng2-bootstrap/ng2-bootstrap';

@Component({
    selector: 'f-connection-selector',
    directives: [DROPDOWN_DIRECTIVES],
    template: `
<div class="input-group" (click)="$event.preventDefault()">
    <div class="input-group-btn btn-group" dropdown keyboardNav="true">
        <button id="simple-btn-keyboard-nav" type="button" class="btn btn-default" dropdownToggle>
            Connection <span class="caret"></span>
        </button>
        <ul ref="menulist" class="dropdown-menu" role="menu" aria-labelledby="simple-btn-keyboard-nav">
            <li role="menuitem" *ngFor="let conn of connectionService.connections">
                <a class="dropdown-item" href="javascript:void(0)" (click)="select(conn)">{{conn.connectionString}}</a>
            </li>
        </ul>
    </div>
    <span class="form-control">
        <span>{{connectionService.get(connId).connectionString}}</span>
    </span>
</div>
`
})
export class ConnectionSelectorComponent {
    private tabId: number = null;
    private connId: number = null;
    constructor(
        private connectionService: ConnectionService,
        private tabService: TabService,
        private router: Router,
        private routeParams: RouteParams
    ) {
        this.tabId = parseInt(routeParams.get('tab'), 10);
        this.connId = parseInt(routeParams.get('connection'), 10);
    }
    
    private select(conn: Connection) {
        this.tabService.updateTabId(this.tabId, conn);
    }
}