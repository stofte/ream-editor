import { provide, Component } from '@angular/core';
import { RouteConfig, Router, ROUTER_DIRECTIVES, AuxRoute } from '@angular/router-deprecated';

import { MonitorService } from './services/monitor.service';
import { ConnectionService } from './services/connection.service';
import { TabService } from './services/tab.service';
import { OverlayUiStateService } from './services/overlay-ui-state.service';

import { StartPageComponent } from './components/start-page.component';
import { BufferTabComponent } from './components/buffer-tab.component';
import { TabListComponent } from './components/tab-list.component';
import { ConnectionManagerComponent } from './components/connection-manager.component';
import { OutputComponent } from './components/output.component';


@Component({
    selector: 'chat',
    template: `<h1>Chat Component</h1>`
})
export class ChatComponent {}

@RouteConfig([
    { path: '/start', as: 'StartPage', component: StartPageComponent, useAsDefault: true },
    { path: '/guide', as: 'GuidePage', component: StartPageComponent },
    { path: '/tab/:tab/:connection/:output', as: 'EditorTab', component: BufferTabComponent }
])
@Component({
    selector: 'f-app',
    directives: [TabListComponent, ConnectionManagerComponent, ROUTER_DIRECTIVES],
    template: `
<div class="main-layer {{connectionsVisible ? '' : 'layer-visible'}}">
    <f-tab-list></f-tab-list>
    <router-outlet></router-outlet>
</div>
<f-connection-manager class="main-layer {{connectionsVisible ? 'layer-visible' : ''}}"></f-connection-manager>
`
})
export class AppComponent {
    private connectionsVisible: boolean = false;
    
    constructor(
        private monitorService : MonitorService, 
        private connectionService: ConnectionService,
        private overlayUiStateService: OverlayUiStateService,
        private tabService: TabService,
        private router : Router) {
        monitorService.start();
        
        // decide where to go
        let connection = connectionService.defaultConnection;
        if (!connection) {
            router.navigate(['StartPage']);
        } else {
            // check if there are any open tabs ...
            if (!tabService.active) {
                this.tabService.newForeground(connection);    
            }
        }
        
        // setup subs for toggle between overlays
        overlayUiStateService.connectionsVisible.subscribe(val => {
            this.connectionsVisible = val;
        });
    }
}
