import { Injectable } from '@angular/core';
import { Subject, Observable } from 'rxjs/Rx';
import { Tab } from '../models/tab.ts';

@Injectable()
export class TabStream {
    private sub = new Subject<Tab>();
    
    public newTab() {
        this.sub.next(<Tab> {
             // todo
        });
    }
    
    public get tabs(): Observable<Tab[]> {
        return this.sub
            .scan((tabs, newTab) => {
                return [newTab, ...tabs];
            }, []);
    }
}
