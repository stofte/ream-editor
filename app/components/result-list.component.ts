import { Component } from '@angular/core';
import { Subject } from 'rxjs/Subject';
import { QueryService } from '../services/query.service';
import { TabService } from '../services/tab.service';
import { Tab } from '../models/tab';
import { QueryResult } from '../models/query-result';
import { ResultDisplayComponent } from './result-display.component';

class ResultMap {
    tab: Tab;
    results: QueryResult[]
}

@Component({
    selector: 'f-result-list',
    directives: [ResultDisplayComponent],
    template: `
<div class="container-fluid" *ngFor="let result of currentResults">
    <f-result-display [result]="result"></f-result-display>
</div>
`
})
export class ResultListComponent {
    private currentResults: QueryResult[] = [];
    constructor(
        query: QueryService,
        tabs: TabService
    ) {
        tabs.active
            .combineLatest(query.activeResult, (tab, store) => {
                let results = store.tab(tab.id);
                return <ResultMap> {
                    tab,
                    results
                };
            })
            .subscribe((x: ResultMap) => {
                this.currentResults = x.results;
                console.log('results', x.tab, x.results);
            });
    }
}
