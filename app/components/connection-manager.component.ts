import { Component, ViewChild, AfterViewInit, ElementRef, Input, EventEmitter, ViewEncapsulation } from '@angular/core';
import { OverlayService } from '../services/overlay.service';
import { ConnectionService } from '../services/connection.service';
import { TabService } from '../services/tab.service';
import { HotkeyService } from '../services/index';
import { Connection } from '../models/connection';

@Component({
    selector: 'rm-connection-manager',
    template: `
<paper-button raised onclick="scrolling.open()">scrolling dialog</paper-button>

<paper-dialog id="scrolling">
    <h2>Connection Manager</h2>
    <form class="rm-connection-manager__add-new">
        <paper-input
            required
            label="Connection string"
            (keydown)="connectionInput($event)"></paper-input>
        <paper-dropdown-menu label="Provider type">
            <paper-listbox class="dropdown-content" [selected]="0">
                <paper-item *ngFor="let ptype of providerTypes">{{ptype}}</paper-item>
            </paper-listbox>
        </paper-dropdown-menu>
        <paper-button raised (click)="addConnection()">Add</paper-button>
    </form>
    <paper-dialog-scrollable>
        <div *ngFor="let conn of connections" class="rm-connection-manager__card">
            <div elevation="1" class="rm-connection-manager__connection">
                <div class="rm-connection-manager__connection__text-block">
                    <div class="rm-connection-manager__connection__text-block__title">
                        {{conn.connectionString}}
                    </div>
                    <div class="rm-connection-manager__connection__text-block__type">
                        {{providerDisplayName(conn.type)}}
                    </div>
                </div>
                <paper-icon-button icon="icons:delete" (click)="removeConnection(conn)"></paper-icon-button>
            </div>
        </div>
    </paper-dialog-scrollable>
    <div class="buttons">
      <paper-button dialog-dismiss>Close</paper-button>
    </div>
</paper-dialog>
`
})
export class ConnectionManagerComponent implements AfterViewInit  {
    @Input('show-dialog') showDialog: EventEmitter<boolean>;
    private newConnectionStringText: string;
    private connectionsSubTitle: string;
    private connections: Connection[];

    private providerTypes = [
        'SQL Server',
        'PostgreSQL',
        'SQLite'
    ];

    // polymer elements
    private dialogElm: any;
    private connStrInputElm: any;
    private connTypeElm: any;
    constructor(
        private hotkey: HotkeyService,
        private element: ElementRef,
        private conns: ConnectionService
        // private connectionManager: OverlayService,
        // private tabService: TabService
    ) {
        conns.all
            .subscribe(cs => {
                this.connections = cs;
                this.connectionsSubTitle = cs.length > 0 ? 
                    'Current connections' : '<i>No connections</i>';
            });
    }

    ngAfterViewInit() {
        this.dialogElm = this.element.nativeElement.querySelector('paper-dialog');
        this.connStrInputElm = this.element.nativeElement
            .querySelector('.rm-connection-manager__add-new paper-input');
        this.connTypeElm = this.element.nativeElement
            .querySelector('.rm-connection-manager__add-new paper-listbox');
        this.showDialog.subscribe(() => {
            this.dialogElm.open();
        });
        this.hotkey.connectionManager.subscribe(val => {
            if (this.dialogElm.opened) {
                this.dialogElm.close();
            } else {
                this.dialogElm.open();
            }
        });
    }

    private providerDisplayName(connType: ConnectionType) {
        return connType === 'sqlserver' ? this.providerTypes[0] :
            connType === 'npgsql' ? this.providerTypes[1] : this.providerTypes[2];
    }

    private connectionInput(event: KeyboardEvent) {
        if (event.which === 13) {
            this.addConnection();
        }
    }

    private addConnection() {
        const value = this.connStrInputElm.value;
        const typeIdx = this.connTypeElm.selected;
        const serverType: ConnectionType = 
            typeIdx === 0 ? 'sqlserver' : typeIdx === 1 ? 'npgsql' : 'sqlite';
        if (value.length === 0) {
            this.connStrInputElm.errorMessage = 'Enter connection string';
        } else {
            // switch on provider type
            if (serverType === 'sqlite') {
                this.connStrInputElm.pattern = 'Data Source\\s*=\\s*[^\\s].+';
                this.connStrInputElm.errorMessage = 'Format must be "Data Source = C:\\some\\where"';
            }
        }
        if (this.connStrInputElm.validate()) {
            this.conns.add(new Connection(value, serverType));
            this.connStrInputElm.value = '';
        }
    }

    private removeConnection(connection: Connection) {
        this.conns.remove(connection);
    }


    // private addNewConnection(value: string, sqlserver: boolean, npgsql: boolean) {
    //     const serverType: ConnectionType = npgsql ? 'npgsql' : 'sqlserver';
    //     if (value.length > 0) {
    //         this.conns.add(new Connection(value, serverType));
    //         this.newConnectionStringText = '';
    //     }
    // }
    
    // private editConnection(connection: Connection) {
    //     connection.temporary = connection.connectionString;
    //     connection.editing = true;
    // }
    
    // private removeConnection(connection: Connection) {
    //     this.conns.remove(connection);
    // }
    
    // private stopEditing(connection: Connection, value: string) {
    //     connection.temporary = value;
    // }
    
    // private updateEditing(connection: Connection, value: string) {
    //     connection.connectionString = value;
    //     this.cancelEditing(connection);
    //     this.conns.update(connection);
    // }
    
    // private cancelEditing(connection: Connection) {
    //     connection.editing = false;
    //     connection.temporary = null;
    // }
}


// <div class="container-fluid int-test-conn-man" style="background:transparent">
//     <div class="jumbotron center-block">
//         <div class="row">
//             <div class="col-md-12">
//                 <h2>Connection Manager</h2>
//             </div>
//         </div>
//         <form>
//             <div class="form-group">
//                 <label for="connectringStringInp">Add new</label>
//                 <input type="string" class="form-control" 
//                     id="connectringStringInp" placeholder="Type/paste connection string and press enter to add"
//                     #newconnection [(ngModel)]="newConnectionStringText"
//                     (keyup.enter)="addNewConnection(newconnection.value, typesqlserver.checked, typenpgsql.checked)">
//             </div>
//             <div class="radio">
//                 <label>
//                     <input type="radio" #typesqlserver name="sqltype" value="sqlserver" checked />
//                     MS SQLServer
//                 </label>
//             </div>
//             <div class="radio">
//                 <label>
//                     <input type="radio" #typenpgsql name="sqltype" value="npgsql" />
//                     PostgreSQL
//                 </label>
//             </div>
//         </form>
//         <div class="row">
//             <div class="col-md-12">
//                 <table class="table">
//                     <thead><caption style="white-space: pre" [innerHTML]="connectionsSubTitle"></caption></thead>
//                     <tbody>
//                         <tr *ngFor="let conn of connections">
//                             <td style="vertical-align: middle;">
//                                 <p *ngIf="!conn.editing" class="pull-right" style="margin-bottom: 0">
//                                     <span style="font-size: 80%;">
//                                         {{ conn.type === 'sqlserver' ? 'MS SQLServer' : 'PostgreSQL' }}
//                                     </span>
//                                 </p>
//                                 <p *ngIf="!conn.editing" style="margin-bottom: 0">
//                                     <span (dblclick)="editConnection(conn)" 
//                                         style="font-size: 80%;"
//                                         title="Double-click to edit">{{conn.connectionString}}</span>
                                   
//                                 </p>
//                                 <p *ngIf="conn.editing" style="margin-bottom: 0">
//                                     <input #editedconn class="form-control"
//                                         [value]="conn.temporary" 
//                                         (blur)="stopEditing(conn, editedconn.value)" 
//                                         (keyup.enter)="updateEditing(conn, editedconn.value)" 
//                                         (keyup.escape)="cancelEditing(conn)">
//                                 </p>
//                             </td>
//                             <td>
//                                 <button (click)="removeConnection(conn)" class="btn btn-default pull-right">Remove</button>
//                             </td>
//                         </tr>
//                     </tbody>
//                 </table>
//             </div>
//         </div>
//         <div class="row">
//             <div class="col-md-12">
//                 <p><button type="button" (click)="closeManager()" class="btn btn-default">Close</button></p>
//             </div>
//         </div>
//     </div>
// </div>
