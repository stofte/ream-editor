import { Component } from '@angular/core';
import { MonitorService } from './services/monitor.service';
import { ConnectionService } from './services/connection.service';
import { TabService } from './services/tab.service';
import { OverlayService } from './services/overlay.service';
import { StartPageComponent } from './components/start-page.component';
import { BufferTabComponent } from './components/buffer-tab.component';
import { TabListComponent } from './components/tab-list.component';
import { ConnectionManagerComponent } from './components/connection-manager.component';
import { TabViewComponent } from './components/tab-view.component';

@Component({
    selector: 'f-app',
    directives: [
        TabListComponent, ConnectionManagerComponent, TabViewComponent 
    ],
    template: `
<div class="main-layer">
    <f-tab-list></f-tab-list>
    <f-tab-view></f-tab-view>
</div>
<div class="main-cover {{connectionsVisible ? 'layer-visible' : ''}}">
</div>
<div class="over-layer {{connectionsVisible ? 'layer-visible' : ''}}">
    <f-connection-manager></f-connection-manager>
</div>
`
})
export class AppComponent {
    private connectionsVisible: boolean = false;
    
    constructor(
        private overlayService: OverlayService
        // private monitorService: MonitorService, 
        // private connectionService: ConnectionService,
        // private overlayUiStateService: OverlayUiStateService,
        // private tabService: TabService
        // private router: Router
        ) {
        //monitorService.start();
        overlayService.connections
            .subscribe(visible => {
                this.connectionsVisible = visible;
            });
        // // decide where to go
        // let connection = connectionService.defaultConnection;
        // if (!connection) {
        //     //router.navigate(['StartPage']);
        // } else {
        //     // check if there are any open tabs ...
        //     if (!tabService.active) {
        //         //this.tabService.newForeground(connection);    
        //     }
        // }
        
        // // setup subs for toggle between overlays
        // overlayUiStateService.connectionsVisible.subscribe(val => {
        //     this.connectionsVisible = val;
        // });
    }
}
