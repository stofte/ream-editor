import { Component } from '@angular/core';
import { RouteParams } from '@angular/router-deprecated';
import { BUTTON_DIRECTIVES } from 'ng2-bootstrap/ng2-bootstrap';
import { EditorService } from '../services/editor.service';
import { MonitorService } from '../services/monitor.service';
import { ConnectionService } from '../services/connection.service';
import { QueryService } from '../services/query.service';
import { TabService } from '../services/tab.service';
import { Tab } from '../models/tab';
import { Connection } from '../models/connection';

@Component({
    selector: 'f-execute-query',
    directives: [BUTTON_DIRECTIVES],
    template: `
<button class="btn btn-primary form-control int-test-execute-btn"
    type="button"
    (click)="run()" 
    disabled="{{ isDisabled ? 'disabled' : '' }}"
    >
    <span class="glyphicon glyphicon-play"></span>
</button>
`
})
export class ExecuteQueryComponent {
    
    private tab: Tab;
    private conn: Connection;
    private id: number;
    private isDisabled = true;
    
    constructor(
        private editorService: EditorService,
        private queryService: QueryService,
        private routeParams: RouteParams,
        monitorService: MonitorService,
        tabService: TabService,
        connectionService: ConnectionService
    ) {
        this.id = parseInt(this.routeParams.get('tab'), 10);
        this.tab = tabService.get(this.id);
        monitorService.queryReady.then(() => {
            this.isDisabled = false;
        });
        const connId = parseInt(this.routeParams.get('connection'), 10);
        this.conn = connectionService.get(connId);
    }
    
    private run(): void {
        const query = this.editorService.get(this.tab);
        this.queryService.run(this.id, this.conn, query);
    }
}
