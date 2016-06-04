import { Component } from '@angular/core';
import { DROPDOWN_DIRECTIVES } from 'ng2-bootstrap/ng2-bootstrap';
import { ConnectionService } from '../services/connection.service';
import { Connection } from '../models/connection';
import { TabService } from '../services/tab.service';

@Component({
    selector: 'f-connection-selector',
    directives: [DROPDOWN_DIRECTIVES],
    template: `
<div class="input-group int-test-conn-sel" (click)="$event.preventDefault()">
    <div class="input-group-btn btn-group" dropdown keyboardNav="true">
        <button id="connection-selector-btn-keyboard-nav" type="button" class="btn btn-default" dropdownToggle>
            Connection <span class="caret"></span>
        </button>
        <ul ref="menulist" class="dropdown-menu" role="menu" aria-labelledby="connection-selector-btn-keyboard-nav">
            <li role="menuitem" *ngFor="let conn of connections">
                <a class="dropdown-item" href="javascript:void(0)" (click)="select(conn)">{{conn.connectionString}}</a>
            </li>
        </ul>
    </div>
    <div class="form-control" style="overflow: hidden">
        <span>{{getText()}}</span>
    </div>
</div>
`
})
export class ConnectionSelectorComponent {
    private tabId: number = null;
    private connId: number = null;
    private connections: Connection[] = [];
    constructor(
        private conns: ConnectionService,
        private tabs: TabService
    ) {
        tabs.active
            .subscribe(tab => {
                this.connId = tab.connectionId;
                this.tabId = tab.id;
            });
        conns.all
            .subscribe(cs => {
                this.connections = cs;
            });
    }
    
    private getText() {
        const elm = this.connections.find(x => x.id === this.connId);
        return elm && elm.connectionString || '';
    }
    
    private select(conn: Connection) {
        this.tabs.setConnection(this.tabId, conn);
    }
}
