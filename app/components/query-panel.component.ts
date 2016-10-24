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
    <div class="rm-query-panel">
        <button mdl-button mdl-button-type="raised">
            <mdl-icon>play_arrow</mdl-icon>
        </button>
        <rm-context-selector></rm-context-selector>
        <div>
            {{sessionId}}
        </div>
    </div>
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
