import { Component, ViewEncapsulation } from '@angular/core';
import { TabService } from '../services/tab.service';

@Component({
    selector: 'rm-query-panel',
    template: `
    <div class="rm-query-panel__session" *ngIf="this.sessionId">
        <rm-controls></rm-controls>
        <rm-editor></rm-editor>
        <rm-result-display></rm-result-display>
    </div>
    <div class="rm-query-panel__blank" *ngIf="!this.sessionId">
        no session
    </div>
`
})
export class QueryPanelComponent {
    sessionId: string = null;
    constructor(tabs: TabService) {
        tabs.currentSessionId.subscribe(id => {
            this.sessionId = id;
        });
    }
}
