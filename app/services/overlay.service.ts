import { Injectable, NgZone } from '@angular/core';
import { Subject, ReplaySubject } from 'rxjs/Rx';
import { Observable } from 'rxjs/Observable';
import { HotkeyService } from './hotkey.service';

@Injectable()
export class OverlayService {
    private connSub: ReplaySubject<boolean> = new ReplaySubject<boolean>(1);
    
    constructor(
        private ngZone: NgZone,
        hotkeys: HotkeyService
    ) {
        this.connSub.next(false); // set initial value
        hotkeys.connectionManager.subscribe(() => {
            this.toggleConnections();
        });
    }
    
    public showConnections() {
        this.connSub.next(true);
    }
    
    public hideConnections() {
        this.connSub.next(false);
    }
    
    public toggleConnections() {
        this.connections
            .take(1)
            .subscribe(x => 
                x ? this.hideConnections() : this.showConnections()
            );        
    }
    
    public get connections(): Observable<boolean> {
        return this.connSub
            .asObservable();
    }
}
