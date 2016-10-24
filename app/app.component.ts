import { Component, ElementRef, ChangeDetectorRef } from '@angular/core';
import { OverlayService } from './services/overlay.service';
import { TabListComponent } from './components/tab-list.component';
import { ConnectionManagerComponent } from './components/connection-manager.component';
import { TabViewComponent } from './components/tab-view.component';
import { StreamManager } from './streams/index';
const electron = electronRequire('electron').remote

@Component({
    selector: 'rm-app',
    template: `
<mdl-layout mdl-layout-fixed-header mdl-layout-header-seamed #mdlLayout1="mdlLayout">
    <mdl-layout-header>
        <mdl-layout-header-row>
            <mdl-layout-spacer></mdl-layout-spacer>
            <button mdl-button class="mdl-button--minimize" (click)="minimizeApp()">
                <mdl-icon>remove</mdl-icon>
            </button>
            <div *ngIf="isMaximized">
                <button mdl-button class="mdl-button--toggle-maximized" (click)="toggleMaximizeApp()">
                    <mdl-icon>fullscreen_exit</mdl-icon>
                </button>
            </div>
            <div *ngIf="!isMaximized">
                <button mdl-button class="mdl-button--toggle-maximized" (click)="toggleMaximizeApp()">
                    <mdl-icon>fullscreen</mdl-icon>
                </button>
            </div>
            <button mdl-button class="mdl-button--exit" (click)="closeApp()">
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
        <rm-query-panel></rm-query-panel>
    </mdl-layout-content>
</mdl-layout>
`
})
export class AppComponent {

    private isMaximized = true;

    constructor(
        private streamManager: StreamManager,
        private ref: ChangeDetectorRef
    ) {
        const win = electron.getCurrentWindow();
        this.isMaximized = win.isMaximized();
        // todo handle reloading shell (ie unregister callbacks onbeforeunload or something)
        win.on('unmaximize', () => {
            this.isMaximized = false;
            this.ref.detectChanges();
        });
        win.on('maximize', () => {
            this.isMaximized = true;
            this.ref.detectChanges();
        });
    }

    minimizeApp() {
        const win = electron.getCurrentWindow();
        win.minimize();
    }

    toggleMaximizeApp() {
        const win = electron.getCurrentWindow();
        if (this.isMaximized) {
            console.log('calling unmaximize')
            win.unmaximize();
        } else {
            console.log('calling maximize')
            win.maximize();
        }
        this.isMaximized = !this.isMaximized;
    }

    closeApp() {
        const win = electron.getCurrentWindow();
        win.close();
    }
}
