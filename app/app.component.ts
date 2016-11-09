import { Component, ElementRef, ChangeDetectorRef, EventEmitter } from '@angular/core';
import { HotkeyService } from './services/index';
import { TabListComponent } from './components/tab-list.component';
import { ConnectionManagerComponent } from './components/connection-manager.component';
import { TabViewComponent } from './components/tab-view.component';
import { StreamManager } from './streams/index';
const electron = electronRequire('electron').remote;
const { webFrame } = electronRequire('electron');

@Component({
    selector: 'rm-app',
    template: `
    <rm-connection-manager></rm-connection-manager>
    <rm-tab-list></rm-tab-list>
    <rm-query-panel></rm-query-panel>
`
})
export class AppComponent {

    private isMaximized = true;

    constructor(
        private streamManager: StreamManager,
        private hotkeys: HotkeyService,
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
        hotkeys.zoomView.subscribe(zoomIn => {
            const lvl = webFrame.getZoomLevel();
            webFrame.setZoomLevel(lvl + (zoomIn ? 1 : -1));
        })
    }

    minimizeApp() {
        const win = electron.getCurrentWindow();
        win.minimize();
    }

    toggleMaximizeApp() {
        const win = electron.getCurrentWindow();
        if (this.isMaximized) {
            win.unmaximize();
        } else {
            win.maximize();
        }
        this.isMaximized = !this.isMaximized;
    }

    closeApp() {
        const win = electron.getCurrentWindow();
        win.close();
    }
}
