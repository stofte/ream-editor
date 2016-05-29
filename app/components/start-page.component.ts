import { Component } from '@angular/core';
import { OverlayService } from '../services/overlay.service';
import { ConnectionService } from '../services/connection.service';

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
        private overlay: OverlayService,
        private connectionService: ConnectionService
    ) {
        
    }
    
    private connectionsToggle() {
        this.overlay.showConnections();
    }
}
