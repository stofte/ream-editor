import { Component } from '@angular/core';
import { RouteParams } from '@angular/router-deprecated';
import { BUTTON_DIRECTIVES } from 'ng2-bootstrap/ng2-bootstrap';
import { EditorService } from '../services/editor.service';
import { ConnectionService } from '../services/connection.service';
import { QueryService } from '../services/query.service';
import { TabService } from '../services/tab.service';

@Component({
    selector: 'f-execute-query',
    directives: [BUTTON_DIRECTIVES],
    template: `
<button class="btn btn-primary form-control" type="button" (click)="run()">
    <span class="glyphicon glyphicon-play"></span>
</button>
`
})
export class ExecuteQueryComponent {
    
    constructor(
        private editorService: EditorService,
        private connectionService: ConnectionService,
        private tabService: TabService,
        private queryService: QueryService,
        private routeParams: RouteParams
    ) {
        
    }
    
    private run(): void {
        console.log('execute', this.routeParams.get('tab'), this.routeParams.get('connection'))
        const id = parseInt(this.routeParams.get('tab'), 10);
        const tab = this.tabService.get(id);
        const connId = parseInt(this.routeParams.get('connection'), 10);
        const conn = this.connectionService.get(connId);
        const query = this.editorService.get(tab);
        this.queryService.run(id, conn, query);
    }
}
