import { Injectable, NgZone } from '@angular/core';
import { Observable, Subject } from 'rxjs/Rx';

const ipc = electronRequire('electron').ipcRenderer;


@Injectable()
export class HotkeyService {
    private connMan = new Subject<boolean>();
    private exeQuery = new Subject<boolean>();
    public connectionManager: Observable<boolean>;
    public executeQuery: Observable<boolean>;
    
    constructor(ngZone: NgZone) {
        this.executeQuery = this.exeQuery
            .asObservable()
            ;
        this.connectionManager = this.connMan
            .asObservable()
            ;
        ipc.on('application-event', (event: any, msg: string) => {
            ngZone.run(() => {
                if (msg === 'connections-panel') {
                    this.connMan.next(true);
                } else if (msg === 'execute-query') {
                    this.exeQuery.next(true);
                }
            });
        });
    }    
}
