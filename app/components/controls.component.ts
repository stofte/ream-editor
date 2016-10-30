import { Component, AfterViewInit, ElementRef } from '@angular/core';
import { Observable } from 'rxjs/Rx';
import { ConnectionService } from '../services/connection.service';
import { TabService } from '../services/tab.service';
import { EditorService } from '../services/editor.service';
import { ExecuteQueryComponent } from './execute-query.component';
import { ResultListComponent } from './result-list.component';
import { EditorDirective } from '../directives/editor.directive';
import { Connection } from '../models/index';
import { InputStream, OutputStream, EventName } from '../streams/index';

@Component({
    selector: 'rm-controls',
    template: `
    <paper-button raised [disabled]="playDisabled" (click)="playClickHandler()"><mdl-icon>play_arrow</mdl-icon></paper-button>
    <paper-dropdown-menu label="Context">
        <paper-listbox class="dropdown-content"
            [selected]="0"
            (iron-select)="contextSelected($event)">
            <paper-item *ngFor="let ctx of contextList">{{ctx}}</paper-item>
        </paper-listbox>
    </paper-dropdown-menu>
`
})
export class ControlsComponent implements AfterViewInit {
    contextList: string[] = ['Code'];
    connections: Connection[] = [];
    selector: any;
    sessionId: string = null;
    playDisabled = false;
    constructor(
        connections: ConnectionService,
        private tabs: TabService,
        private input: InputStream,
        private output: OutputStream,
        private elm: ElementRef
    ) {
        connections.all.subscribe(conns => {
            this.connections = conns;
            this.contextList = ['Code'].concat(conns.map(x => x.connectionString));
        });
        tabs.currentSessionId.filter(x => !!x).subscribe(id => {
            this.sessionId = id;
            if (this.selector) {
                const context = tabs.sessionContext(id);
                if (context !== null && context !== undefined) {
                    for (let i = 0; i < this.connections.length; i++) {
                        if (this.connections[i].id === context) {
                            this.selector.selected = i + 1;
                            break;
                        }
                    }
                } else {
                    // code context
                    this.selector.selected = 0;
                }
            }
        });
    }

    ngAfterViewInit() {
        this.selector = this.elm.nativeElement.querySelector('paper-listbox');
    }

    contextSelected(event: any) {
        const idx = this.selector.selected;
        const ctx = idx === 0 ? null : this.connections[idx - 1];
        console.log('contextSelected');
        this.tabs.setContext(this.sessionId, ctx);
    }

    playClickHandler() {
        const id = this.sessionId;
        this.playDisabled = true;
        this.output.events
            .first(msg => msg.id === id && msg.name === EventName.ResultDone)
            .subscribe(() => {
                this.playDisabled = false;
            });
        this.input.executeBuffer(id);
    }
}
