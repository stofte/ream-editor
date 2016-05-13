import { Component } from '@angular/core';
import { Router } from '@angular/router-deprecated';
import { OverlayUiStateService } from '../services/overlay-ui-state.service';
import { ConnectionService } from '../services/connection.service';
import { TabService } from '../services/tab.service';

@Component({
    selector: 'f-start-page',
    template: `
    <div>
        <p *ngIf="!connectionService.defaultConnection">
            <a (click)="connectionsToggle()">click to open connection manager</a>
        </p>
        <p *ngIf="connectionService.defaultConnection">
            <a (click)="blankTab()">click to open a new tab</a>
        </p>
    </div>
`    
})
export class StartPageComponent {
    constructor(
        private overlayUiStateService : OverlayUiStateService,
        private connectionService: ConnectionService,
        private tabService: TabService,
        private router : Router
    ) {
        
    }
    
    private connectionsToggle() {
        this.overlayUiStateService.toggleConnections();
    }
    
    private blankTab() {
        this.tabService.newForeground(this.connectionService.defaultConnection);
    }
}
