import { Component } from '@angular/core';
import { Subject } from 'rxjs/Subject';
import { TabService } from '../services/tab.service';
import { Tab } from '../models/tab';
import { QueryResult } from '../models/query-result';
import { ResultDisplayComponent } from './result-display.component';

class ResultMap {
    tab: Tab;
    results: QueryResult[];
}

@Component({
    selector: 'f-result-list',
    // directives: [ResultDisplayComponent],
    template: `
<div class="result-display-container" *ngIf="currentResults.length > 0" [style.top]="editorOffset()">
    <f-result-display [result]="currentResults[0]"></f-result-display>
</div>
`
})
export class ResultListComponent {
    private currentResults: QueryResult[] = [];
    private currentId: string;
    private offset = 84;
    
    constructor(
    ) {
    }
    
    private editorOffset() {
        return (this.offset + 64 /* heder */) + 'px';
    }
    
    ngAfterContentChecked() {
        this.offset = document.querySelector('.query-editor-suite').clientHeight;
    }
}
