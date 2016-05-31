import { Component, Input } from '@angular/core';
import { QueryResult } from '../models/query-result';
import { ResultPage } from '../models/result-page';

@Component({
    selector: 'f-result-display',
    template: `
<div class="row">
    <div class="col-md-12">
        <table class="table table-condensed output-table">
            <caption class="panel panel-default">
                <div class="panel-body">
                    <p class="pull-right">{{result.query}} @ {{result.created | date:'shortTime'}}</p>
                    <div class="btn-group" role="group">
                        <button 
                            *ngFor="let page of result.pages"
                            (click)="showResult(page)"
                            type="button" class="btn btn-default {{btnActive(page)}}">
                            {{page.title}}
                        </button>
                    </div>
                </div>
            </caption>
        </table>
    </div>
</div>
`
})
export class ResultDisplayComponent {
    @Input() public result: QueryResult;
    private expanded = true;
    private currentPage: ResultPage = null;
    
    private showResult(page: ResultPage) {
        this.currentPage = page;
    }
    
    private btnActive(page: ResultPage): string {
        return this.currentPage && this.currentPage.id === page.id ? 'active' : '';
    }
}
