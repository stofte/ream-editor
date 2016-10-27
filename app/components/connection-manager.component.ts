import { Component, ViewChild, AfterViewInit, ElementRef, Input, EventEmitter  } from '@angular/core';
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
    <paper-dialog-scrollable>
        <p>Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod 
        tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, 
        quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute
          irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat 
          nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui 
          officia deserunt mollit anim id est laborum.</p>
        <p>Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod 
        tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, 
        quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute
          irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat 
          nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui 
          officia deserunt mollit anim id est laborum.</p>
        <p>Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod 
        tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, 
        quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute
          irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat 
          nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui 
          officia deserunt mollit anim id est laborum.</p>
        <p>Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod 
        tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, 
        quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute
          irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat 
          nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui 
          officia deserunt mollit anim id est laborum.</p>
        <p>Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod 
        tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, 
        quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute
          irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat 
          nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui 
          officia deserunt mollit anim id est laborum.</p>

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
    private dialogElm: any;
    constructor(
      private hotkey: HotkeyService,
      private element: ElementRef
        // private connectionManager: OverlayService,
        // private conns: ConnectionService,
        // private tabService: TabService
    ) {
        // this.showDialog.subscribe(x => {
        //     console.log('showDialog subscriber', x);
        // })
        // conns.all
        //     .subscribe(cs => {
        //         this.connections = cs;
        //         this.connectionsSubTitle = cs.length > 0 ? 
        //             'Current connections' : '<i>No connections</i>';
        //     });
    }

    ngAfterViewInit() {
        this.dialogElm = this.element.nativeElement.querySelector('paper-dialog');
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
    
    // private closeManager() {
    //     this.connectionManager.hideConnections();
    // }
    
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
