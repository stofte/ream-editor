import { Injectable, NgZone, EventEmitter } from '@angular/core';
import { Observable, Subject } from 'rxjs/Rx';

const ipc = electronRequire('electron').ipcRenderer;


@Injectable()
export class HotkeyService {
    private connMan = new Subject<boolean>();
    private exeQuery = new Subject<boolean>();
    public connectionManager: Observable<boolean>;
    public executeQuery: Observable<boolean>;
    public zoomView = new EventEmitter<boolean>();
    
    constructor(ngZone: NgZone) {
        this.executeQuery = this.exeQuery.asObservable();
        this.connectionManager = this.connMan.asObservable();
        ipc.on('application-event', (event: any, msg: string) => {
            ngZone.run(() => {
                if (msg === 'connections-panel') {
                    this.connMan.next(true);
                } else if (msg === 'execute-query') {
                    this.exeQuery.next(true);
                } else if (msg === 'zoom-view-in') {
                    this.zoomView.emit(true);
                } else if (msg === 'zoom-view-out') {
                    this.zoomView.emit(false);
                }
            });
        });
    }    
}
