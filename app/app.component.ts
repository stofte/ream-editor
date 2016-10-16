import { Component, ElementRef, OnInit } from '@angular/core';
import { OverlayService } from './services/overlay.service';
import { TabListComponent } from './components/tab-list.component';
import { ConnectionManagerComponent } from './components/connection-manager.component';
import { TabViewComponent } from './components/tab-view.component';

@Component({
    selector: 'rm-app',
    template: `
<mdl-layout mdl-layout-fixed-header #mdlLayout1="mdlLayout">
    <mdl-layout-header>
        <mdl-layout-header-row>
            <mdl-layout-spacer></mdl-layout-spacer>
            <button mdl-button class="mdl-button--minimize">
                <mdl-icon>remove</mdl-icon>
            </button>
            <button mdl-button class="mdl-button--toggle-maximized">
                <mdl-icon>fullscreen</mdl-icon>
            </button>
            <button mdl-button class="mdl-button--exit">
                <mdl-icon>clear</mdl-icon>
            </button>
        </mdl-layout-header-row>
    </mdl-layout-header>
    <mdl-layout-drawer>
        <mdl-layout-title>Ream</mdl-layout-title>
        <nav class="mdl-navigation">
            <a class="mdl-navigation__link" (click)="mdlLayout1.toggleDrawer()">Connections</a>
            <a class="mdl-navigation__link" (click)="mdlLayout1.toggleDrawer()">About</a>
        </nav>
    </mdl-layout-drawer>
    <mdl-layout-content>
        <rm-tab-list></rm-tab-list>
    </mdl-layout-content>
</mdl-layout>
`
})
export class AppComponent {
    constructor() {
    }
}
