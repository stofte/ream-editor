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

const codeLabel = '<Code>';
@Component({
    selector: 'rm-controls',
    template: `
    <button class="rm-controls__run"
        (click)="executeBuffer()"
        [disabled]="playDisabled"><i class="material-icons">play_arrow</i></button>
    <select #sel (change)="contextChanged($event, sel)">
        <option *ngFor="let ctx of contextList"
            [value]="ctx.id"
            [selected]="ctx.id === selectedContext">
                {{contextLabel(ctx.id)}}
        </option>
    </select>
`
})
export class ControlsComponent {
    contextList: Connection[] = [<Connection> {id: null}];
    sessionId: string = null;
    selectedContext = null;
    playDisabled = false;
    constructor(
        connections: ConnectionService,
        private tabs: TabService,
        private input: InputStream,
        private output: OutputStream,
        private elm: ElementRef
    ) {
        connections.all.subscribe(conns => {
            this.contextList = [<Connection> {connectionString: codeLabel}]
                .concat([...conns]);
        });
        tabs.currentSessionId.subscribe(id => {
            this.sessionId = id;
            if (id) {
                this.selectedContext = tabs.sessionContext(id);
            }
        });
    }

    contextChanged(event: any, select: HTMLSelectElement) {
        let ctx: any = null;
        if (select.value) {
            const id = parseInt(select.value, 10);
            ctx = this.contextList.find(x => x.id === id);
        }
        this.selectedContext = ctx;
        this.tabs.setContext(this.sessionId, ctx);
    }

    contextLabel(id: number) {
        return id ? this.contextList.find(x => x.id === id).connectionString : 'Code';
    }

    executeBuffer() {
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
