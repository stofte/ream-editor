import { Injectable } from '@angular/core';
import { Subject } from 'rxjs/Rx';
import { ResultPage } from '../models/index';

@Injectable()
export class ResultStream {
    public events: Subject<ResultPage>;
    constructor() {
        
    }
}
