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
    
    constructor(overlayService: OverlayService) {
        overlayService
            .connections
            .subscribe(visible => {
                this.connectionsVisible = visible;
            });
    }
}
