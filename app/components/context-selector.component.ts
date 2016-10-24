import { Component, Output } from '@angular/core';
// import { DROPDOWN_DIRECTIVES } from 'ng2-bootstrap/ng2-bootstrap';
import { ConnectionService } from '../services/connection.service';
import { Connection } from '../models/connection';
import { TabService } from '../services/tab.service';

@Component({
    selector: 'rm-context-selector',
    template: `
<div class="rm-context-selector">
    <button class="mdl-button" (click)="toggleContextList()">
        Data Source=C:&#92;foo&#92;bar.sqlite
    </button>
    <div class="rm-context-selector__dropdown {{contextListVisible ? 'is-active' : ''}}">
        <ul>
            <li><button class="mdl-button" (click)="setContext(null)">Code</button></li>
            <li><button class="mdl-button" (click)="setContext(42)">World.sqlite</button></li>
        </ul>
    </div>
</div>
`
})
export class ContextSelectorComponent {
    @Output('rm-current-context-id') public currentContextId: string = null;
    
    private tabId: number = null;
    private connId: number = null;
    private connections: Connection[] = [];
    private contextListVisible = false;
    constructor(
        
    ) {
        
    }

    private toggleContextList() {
        this.contextListVisible = !this.contextListVisible;
    }
    
    private getText() {
        const elm = this.connections.find(x => x.id === this.connId);
        return elm && elm.connectionString || '';
    }
    
    private select(conn: Connection) {
        
    }
}
