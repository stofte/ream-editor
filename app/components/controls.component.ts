import { Component } from '@angular/core';
import { Observable } from 'rxjs/Rx';
import { ConnectionService } from '../services/connection.service';
import { TabService } from '../services/tab.service';
import { EditorService } from '../services/editor.service';
import { ExecuteQueryComponent } from './execute-query.component';
import { ResultListComponent } from './result-list.component';
import { EditorDirective } from '../directives/editor.directive';

@Component({
    selector: 'rm-controls',
    template: `
    <paper-button raised><mdl-icon>play_arrow</mdl-icon></paper-button>
    <paper-dropdown-menu label="Context">
        <paper-listbox class="dropdown-content">
            <paper-item>Croissant Croissant Croissant</paper-item>
            <paper-item>Donut</paper-item>
            <paper-item>Financier</paper-item>
            <paper-item>Madeleine</paper-item>
        </paper-listbox>
    </paper-dropdown-menu>
`
})
export class ControlsComponent {
    sessionId: string = null;
    constructor(tabs: TabService) {
        tabs.currentSessionId.subscribe(id => {
            this.sessionId = id;
        });
    }
}
