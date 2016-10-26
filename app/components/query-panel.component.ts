import { Component } from '@angular/core';
import { Observable } from 'rxjs/Rx';
import { ConnectionService } from '../services/connection.service';
import { TabService } from '../services/tab.service';
import { EditorService } from '../services/editor.service';
import { ExecuteQueryComponent } from './execute-query.component';
import { ResultListComponent } from './result-list.component';
import { EditorDirective } from '../directives/editor.directive';

@Component({
    selector: 'rm-query-panel',
    template: `
    <rm-controls></rm-controls>
    <div>
        {{sessionId}}
    </div>
    <rm-connection-manager></rm-connection-manager>
`
})
export class QueryPanelComponent {
    sessionId: string = null;
    constructor(tabs: TabService) {
        tabs.currentSessionId.subscribe(id => {
            this.sessionId = id;
        });
    }
}
