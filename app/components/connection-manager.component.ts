import { Component, ViewChild, AfterViewInit, ElementRef, Input, EventEmitter, ViewEncapsulation } from '@angular/core';
import { OverlayService } from '../services/overlay.service';
import { ConnectionService } from '../services/connection.service';
import { TabService } from '../services/tab.service';
import { HotkeyService } from '../services/index';
import { Connection } from '../models/connection';

@Component({
    selector: 'rm-connection-manager',
    template: `
<dialog id="connection-mgr-dialog" class="rm-connection-manager int-test-conn-man">
    <h2 class="rm-connection-manager__title">Connection Manager</h2>
    <div class="rm-connection-manager__container">
        <div class="rm-connection-manager__validation">
            <form class="rm-connection-manager__add-new" 
                (submit)="addConnection($event);">
                
                <input placeholder="Enter connection string ..." name="connStr" [(ngModel)]="newConnectionStringText">
                <select #sel (change)="changeProvider(sel.selectedIndex)">
                    <option *ngFor="let ptype of providerTypes; let idx = index"
                        [selected]="idx === newConnectionProviderIndex">{{ptype}}</option>
                </select>
                <button type="submit">Add</button>
            </form>
            <div class="rm-connection-manager__validation__output">
                <div>{{this.errorMessageText}}</div>
                <ul *ngIf="sqliteSyntaxHelp">
                    <li>Data Source=C:\some\where.db;Version=3;</li>
                </ul>
            </div>
        </div>
        <div>
            <div *ngFor="let conn of connections" class="rm-connection-manager__connection">
                <div class="rm-connection-manager__connection__text-block">
                    <div class="rm-connection-manager__connection__text-block__title">
                        {{conn.connectionString}}
                    </div>
                    <div class="rm-connection-manager__connection__text-block__type">
                        {{providerDisplayName(conn.type)}}
                    </div>
                </div>
                <button (click)="removeConnection(conn)">
                    <i class="material-icons">delete</i>
                </button>
            </div>
        </div>
    </div>
    <div class="rm-connection-manager__close">
        <button (click)="closeDialog()"><i class="material-icons">close</i></button>
    </div>
</dialog>
`
})
export class ConnectionManagerComponent implements AfterViewInit  {
    private newConnectionStringText: string;
    private newConnectionProviderIndex = 1;
    private errorMessageText = '';
    private connectionsSubTitle: string;
    private connections: Connection[];
    private sqliteSyntaxHelp = false;
    private providerTypes = [
        'SQL Server',
        'PostgreSQL',
        'SQLite'
    ];

    // dom ref to call dialog methods
    private dialogElm: any;
    constructor(
        private hotkey: HotkeyService,
        private element: ElementRef,
        private conns: ConnectionService
    ) {
        conns.all.subscribe(cs => this.connections = cs);
    }

    ngAfterViewInit() {
        this.dialogElm = this.element.nativeElement.querySelector('dialog');
        this.hotkey.connectionManager.subscribe(val => {
            if (this.dialogElm.open) {
                this.dialogElm.close();
            } else {
                this.reset();
                this.dialogElm.showModal();
            }
        });
    }

    private reset() {
        this.sqliteSyntaxHelp = false;
        this.errorMessageText = '';
        this.newConnectionStringText = '';
        this.newConnectionProviderIndex = 0;
    }

    private providerDisplayName(connType: ConnectionType) {
        return connType === 'sqlserver' ? this.providerTypes[0] :
            connType === 'npgsql' ? this.providerTypes[1] : this.providerTypes[2];
    }

    private addConnection() {
        const sqliteRegex = /Data Source\s*=\s*.+/;
        const value = this.newConnectionStringText;
        const provider: ConnectionType = 
             this.newConnectionProviderIndex === 0 ? 'sqlserver' : 
             this.newConnectionProviderIndex === 1 ? 'npgsql' : 'sqlite';
        let valid = true;
        this.errorMessageText = '';
        if (value.length === 0) {
            this.errorMessageText = 'Enter connection string';
        } else {
            // switch on provider type
            if (provider === 'sqlite' && !sqliteRegex.test(value)) {
                this.errorMessageText = 'Incorrect format, examples for SQLite:';
                this.sqliteSyntaxHelp = true;
            }
        }

        if (!this.errorMessageText) {
            this.conns.add(new Connection(value, provider));
            this.newConnectionStringText = '';
        }

        event.preventDefault();
        return false;
    }

    private removeConnection(connection: Connection) {
        const len = this.connections.length; 
        this.conns.remove(connection);
    }

    private closeDialog() {
        this.dialogElm.close();
    }

    private changeProvider(idx: number) {
        this.newConnectionProviderIndex = idx;
    }
}

