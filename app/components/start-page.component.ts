import { Component } from '@angular/core';
import { Router } from '@angular/router-deprecated';
import { OverlayUiStateService } from '../services/overlay-ui-state.service';
import { ConnectionService } from '../services/connection.service';
import { TabService } from '../services/tab.service';

@Component({
    selector: 'f-start-page',
    template: `
<div class="container-fluid int-test-start-page">
    <div class="row">
        <div *ngIf="!connectionService.defaultConnection">
            <div class="col-md-12">
                <p>
                    Start by adding a database connection 
                    using the Connection Manager. <em>(shortcut: ctrl + d)</em>
                </p>
                <p>
                    <button (click)="connectionsToggle()" class="btn btn-default">Open Connection Manager</button>
                </p>
            </div>
        </div>
    </div>
</div>
`    
})
export class StartPageComponent {
    constructor(
        private overlayUiStateService: OverlayUiStateService,
        private connectionService: ConnectionService,
        private tabService: TabService,
        private router: Router
    ) {
        
    }
    
    private connectionsToggle() {
        this.overlayUiStateService.toggleConnections();
    }
    
    private blankTab() {
        this.tabService.newForeground(this.connectionService.defaultConnection);
    }
}
