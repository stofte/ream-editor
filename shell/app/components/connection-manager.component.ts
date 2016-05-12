import { Component } from '@angular/core';
import { OverlayUiStateService } from '../services/overlay-ui-state.service';
import { ConnectionService } from '../services/connection.service';
import { Connection } from '../models/connection';

@Component({
    selector: 'f-connection-manager',
    template: `
<div class="container">
    <h1>connection manager</h1>
    <p>
        <label>
            <input placeholder="Add new connectionstring" #newconnection 
                [(ngModel)]="newConnectionStringText" 
                (keyup.enter)="addNewConnection(newconnection.value)">
        </label>
    </p>
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
`
})
export class ConnectionManagerComponent {
    private visible : boolean;
    private newConnectionStringText : string;
    constructor(
        private overlayUiStateService : OverlayUiStateService,
        private connectionService: ConnectionService
    ) {

    }
    
    private closeManager() {
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
