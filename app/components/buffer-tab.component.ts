import { Component } from '@angular/core';
import { RouteParams } from '@angular/router-deprecated';
import { ConnectionService } from '../services/connection.service';
import { TabService } from '../services/tab.service';
import { ConnectionSelectorComponent } from './connection-selector.component';
import { ExecuteQueryComponent } from './execute-query.component';
import { OutputComponent } from './output.component'
import { EditorDirective } from '../directives/editor.directive';

@Component({
    selector: 'f-buffer-tab',
    directives: [OutputComponent, EditorDirective, ExecuteQueryComponent, ConnectionSelectorComponent],
    template: `
<div class="container-fluid">
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
    <f-output></f-output>
</div>
`
})
export class BufferTabComponent {
    constructor(
        private connectionService: ConnectionService,
        private tabService: TabService,
        private routeParams: RouteParams
    ) {
        
    }
    
    routerOnActivate() {
        const id = parseInt(this.routeParams.get('tab'), 10);
        this.tabService.routedTo(id);
    }
}
