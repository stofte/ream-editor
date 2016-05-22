import { Component,  } from '@angular/core';
import { OverlayUiStateService } from '../services/overlay-ui-state.service';
import { ConnectionService } from '../services/connection.service';
import { TabService } from '../services/tab.service';
import { Connection } from '../models/connection';

@Component({
    selector: 'f-connection-manager',
    template: `
<div class="container-fluid int-test-conn-man">
    <nav class="navbar navbar-default navbar-fixed-top">
        <div class="container-fluid">
            <div class="navbar-header">
                <a class="navbar-brand">Connection Manager</a>
            </div>
        </div>
    </nav>
    <div class="jumbotron center-block">
        <div class="row">
            <div class="col-md-12">
                <p>
                    <label>
                        <input placeholder="Add new connectionstring" #newconnection 
                            [(ngModel)]="newConnectionStringText" 
                            (keyup.enter)="addNewConnection(newconnection.value)">
                    </label>
                </p>
            </div>
        </div>
        <div class="row">
            <div class="col-md-12">
                <ul>
                    <li *ngFor="let connection of connectionService.connections">
                        <div *ngIf="!connection.Editing">
                            <label (dblclick)="editConnection(connection)">{{connection.connectionString}}</label>
                            <button (click)="removeConnection(connection)">remove</button>
                        </div>
                        <input #editedconn
                            *ngIf="connection.editing" 
                            [value]="connection.temporary" 
                            (blur)="stopEditing(connection, editedconn.value)" 
                            (keyup.enter)="updateEditing(connection, editedconn.value)" 
                            (keyup.escape)="cancelEditing(connection)">
                    </li>
                </ul>
                <p><a (click)="closeManager()">close connection manager</a></p>
            </div>
        </div>
    </div>
</div>
`
})
export class ConnectionManagerComponent {
    private newConnectionStringText: string;
    constructor(
        private overlayUiStateService: OverlayUiStateService,
        private connectionService: ConnectionService,
        private tabService: TabService
    ) {

    }
    
    private closeManager() {
        const isStart = location.hash.indexOf('/start') !== -1;
        if (isStart) {
            const conn = this.connectionService.defaultConnection;
            if (conn) {
                this.tabService.newForeground(conn);
            }
        }
        this.overlayUiStateService.toggleConnections();
    }
    
    private addNewConnection(value: string) {
        if (value.length > 0) {
            this.connectionService.addNew(new Connection(value));
            this.newConnectionStringText = '';
        }
    }
    
    private editConnection(connection: Connection) {
        connection.temporary = connection.connectionString;
        connection.editing = true;
    }
    
    private removeConnection(connection: Connection) {
        this.connectionService.remove(connection);
    }
    
    private stopEditing(connection: Connection, value: string) {
        connection.temporary = value;
    }
    
    private updateEditing(connection: Connection, value: string) {
        connection.connectionString = value;
        this.cancelEditing(connection);
        this.connectionService.update(connection);
    }
    
    private cancelEditing(connection: Connection) {
        connection.editing = false;
        connection.temporary = null;
    }
}
