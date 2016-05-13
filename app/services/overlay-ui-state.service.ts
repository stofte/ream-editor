import { Injectable, NgZone } from '@angular/core';
import { Subject } from 'rxjs/Subject';
import { Observable } from 'rxjs/Observable';

const ipc = electronRequire('electron').ipcRenderer;

@Injectable()
export class OverlayUiStateService {
    private visible = false;
    private connections: Subject<boolean> = new Subject<boolean>();
    
    constructor(
        private ngZone: NgZone
    ) {
        ipc.on('application-event', this.applicationEventHandler.bind(this));
    }
    
    private applicationEventHandler(event : any, msg : string) {
        if (msg === 'connections-panel') {
            this.ngZone.run(() => this.toggleConnections());
        }
    }
    
    public toggleConnections() {
        this.visible = !this.visible;
        this.connections.next(this.visible);
    }
    
    public get connectionsVisible() : Observable<boolean> {
        return this.connections;
    }
}
