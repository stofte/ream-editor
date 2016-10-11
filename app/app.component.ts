import { Component } from '@angular/core';
import { OverlayService } from './services/overlay.service';
import { TabListComponent } from './components/tab-list.component';
import { ConnectionManagerComponent } from './components/connection-manager.component';
import { TabViewComponent } from './components/tab-view.component';

@Component({
    selector: 'ream-app',
    template: `
<div class="main-layer">
    hello
</div>
`
})
export class AppComponent {
    constructor() {
    }
}
