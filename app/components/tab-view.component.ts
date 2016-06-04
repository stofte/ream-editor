import { Component } from '@angular/core';
import { TabService } from '../services/tab.service';
import { QuerySuiteComponent } from './query-suite.component';
import { StartPageComponent } from './start-page.component';
import { Observable } from 'rxjs/Observable';

@Component({
    selector: 'f-tab-view',
    directives: [QuerySuiteComponent, StartPageComponent],
    template: `
<div *ngIf="editorVisible" class="my-editor">
    <f-query-suite></f-query-suite>
</div>
<div *ngIf="startVisible" class="my-start">
    <f-start-page></f-start-page>
</div>
`
})
export class TabViewComponent {
    private startVisible: boolean;
    private editorVisible: boolean;
    
    constructor(
        private tabs: TabService
    ) {
        tabs.hasTabs
            .subscribe(hasTabs => {
                this.startVisible = !hasTabs;
                this.editorVisible = hasTabs;
            });
    }
}
