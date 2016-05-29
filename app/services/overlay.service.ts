import { Injectable, NgZone } from '@angular/core';
import { Subject, ReplaySubject } from 'rxjs/Rx';
import { Observable } from 'rxjs/Observable';

const ipc = electronRequire('electron').ipcRenderer;

@Injectable()
export class OverlayService {
    private connSub: ReplaySubject<boolean> = new ReplaySubject<boolean>(1);
    
    constructor(
        private ngZone: NgZone
    ) {
        this.connSub.next(false); // set initial value
        ipc.on('application-event', this.applicationEventHandler.bind(this));
    }
    
    private applicationEventHandler(event: any, msg: string) {
        if (msg === 'connections-panel') {
            this.ngZone.run(() => 
                // seems silly but works to keep state inside rx
                this.connections
                    .take(1)
                    .subscribe(x => 
                        x ? this.hideConnections() : this.showConnections()
                    ));
        }
    }
    
    public showConnections() {
        this.connSub.next(true);
    }
    
    public hideConnections() {
        this.connSub.next(false);
    }
    
    public get connections(): Observable<boolean> {
        return this.connSub
            .asObservable();
    }
}
