import { Component } from '@angular/core';
import { ConnectionService } from '../services/connection.service';
import { TabService } from '../services/tab.service';
import { EditorService } from '../services/editor.service';
import { ConnectionSelectorComponent } from './connection-selector.component';
import { ExecuteQueryComponent } from './execute-query.component';
import { ResultListComponent } from './result-list.component';
import { EditorDirective } from '../directives/editor.directive';

@Component({
    selector: 'f-query-suite',
    directives: [EditorDirective, ExecuteQueryComponent, ConnectionSelectorComponent, ResultListComponent],
    template: `
<div class="container-fluid query-editor-suite">
    <div class="row">
        <div class="col-md-1">
            <p><f-execute-query></f-execute-query></p>
        </div>
        <div class="col-md-11">
            <p><f-connection-selector></f-connection-selector></p>
        </div>
    </div>
    <div class="row">
        <div class="col-md-12">
            <p><textarea editor></textarea></p>
        </div>
    </div>
</div>
<f-result-list></f-result-list>
`
})
export class QuerySuiteComponent { }
