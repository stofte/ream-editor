import { Component, ViewEncapsulation } from '@angular/core';
import { TabService } from '../services/tab.service';

@Component({
    selector: 'rm-query-panel',
    template: `
    <rm-controls></rm-controls>
    <rm-editor></rm-editor>
    <div>
        {{sessionId}}
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
