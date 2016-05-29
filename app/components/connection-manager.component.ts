import { Component,  } from '@angular/core';
import { OverlayService } from '../services/overlay.service';
import { ConnectionService } from '../services/connection.service';
import { TabService } from '../services/tab.service';
import { Connection } from '../models/connection';

@Component({
    selector: 'f-connection-manager',
    template: `
<div class="container-fluid int-test-conn-man" style="background:transparent">
    <div class="jumbotron center-block">
        <div class="row">
            <div class="col-md-12">
                <h2>Connection Manager</h2>
            </div>
        </div>
        <form>
            <div class="form-group">
                <label for="connectringStringInp">Add new</label>
                <input type="string" class="form-control" 
                    id="connectringStringInp" placeholder="Type/paste connection string and press enter to add"
                    #newconnection [(ngModel)]="newConnectionStringText"
                    (keyup.enter)="addNewConnection(newconnection.value)">
            </div>
        </form>
        <div class="row">
            <div class="col-md-12">
                <table class="table">
                    <thead><caption style="white-space: pre" [innerHTML]="connectionsSubTitle"></caption></thead>
                    <tbody>
                        <tr *ngFor="let conn of connections">
                            <td style="vertical-align: middle">
                                <p *ngIf="!conn.editing" style="margin-bottom: 0">
                                    <span (dblclick)="editConnection(conn)" title="Double-click to edit">{{conn.connectionString}}</span>
                                    
                                </p>
                                <p *ngIf="conn.editing" style="margin-bottom: 0">
                                    <input #editedconn class="form-control"
                                        [value]="conn.temporary" 
                                        (blur)="stopEditing(conn, editedconn.value)" 
                                        (keyup.enter)="updateEditing(conn, editedconn.value)" 
                                        (keyup.escape)="cancelEditing(conn)">
                                </p>
                            </td>
                            <td>
                                <button (click)="removeConnection(conn)" class="btn btn-default pull-right">Remove</button>
                                <p class="pull-right">{{conn.id}}</p>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
        <div class="row">
            <div class="col-md-12">
                <p><button type="button" (click)="closeManager()" class="btn btn-default">Close</button></p>
            </div>
        </div>
    </div>
</div>
`
})
export class ConnectionManagerComponent {
    private newConnectionStringText: string;
    private connectionsSubTitle: string;
    private connections: Connection[];
    constructor(
        private connectionManager: OverlayService,
        private conns: ConnectionService,
        private tabService: TabService
    ) {
        conns.all
            .subscribe(cs => {
                this.connections = cs;
                this.connectionsSubTitle = cs.length > 0 ? 
                    'Current connections' : '<i>No connections</i>';
            });
    }
    
    private closeManager() {
        // const isStart = location.hash.indexOf('/start') !== -1;
        // if (isStart) {
        //     const conn = this.connectionService.defaultConnection;
        //     if (conn) {
        //         this.tabService.newForeground(conn);
        //     }
        // }
        this.connectionManager.hideConnections();
    }
    
    private addNewConnection(value: string) {
        if (value.length > 0) {
            this.conns.add(new Connection(value));
            this.newConnectionStringText = '';
        }
    }
    
    private editConnection(connection: Connection) {
        connection.temporary = connection.connectionString;
        connection.editing = true;
    }
    
    private removeConnection(connection: Connection) {
        this.conns.remove(connection);
    }
    
    private stopEditing(connection: Connection, value: string) {
        connection.temporary = value;
    }
    
    private updateEditing(connection: Connection, value: string) {
        connection.connectionString = value;
        this.cancelEditing(connection);
        this.conns.update(connection);
    }
    
    private cancelEditing(connection: Connection) {
        connection.editing = false;
        connection.temporary = null;
    }
}
