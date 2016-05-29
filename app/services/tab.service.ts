import { Injectable } from '@angular/core';
import { ReplaySubject, Observable, Subject } from 'rxjs/Rx';
import { Tab } from '../models/tab';
import { Connection } from '../models/connection';
import { ConnectionService } from './connection.service';
// import { OmnisharpService } from './omnisharp.service';

@Injectable()
export class TabService {
    private nextId = 0;
    private stream: Observable<Tab[]>;
    private ops = new ReplaySubject<IStreamOperation>();
    
    constructor(
        private conns: ConnectionService
    ) {
        this.stream = this.ops
            .scan((tabs: Tab[], op) => {
                let res = op(tabs);
                // console.log('scan', res.map(x => x.id + ':' + x.connectionId + ':' + x.active))
                return res;
            }, []);
        conns.all.subscribe(this.handleConnections.bind(this));
    }
    
    public get tabs(): Observable<Tab[]> {
        return this.stream;
    }
    
    public get hasTabs(): Observable<boolean> {
        return this
            .tabs
            .map(x => x.length > 0);
    }
    
    // notify only when active tab id changes
    public get activeTab(): Observable<Tab> {
        return this.activeBase
            .distinctUntilChanged((x, y) => x.id === y.id);
    }
    
    // is notified when
    // - connection id for current tab is changed
    // - active tab is changed
    public get active(): Observable<Tab> {
        let detect = this.activeBase
            // having issues detecting changes otherwise?
            .map(x => `${x.id}:${x.connectionId}`)
            .distinctUntilChanged((x, y) => x === y);
        // whenever we detect, we emit the latest from the active tabs list
        return detect.withLatestFrom(this.activeTab, (x, tab) => tab);
    }
    
    // is notified when a tab enters a new db context for the first time
    public get newContext(): Observable<Tab> {
        return this.active
            .scan((ctx, tab) => {
                if (!ctx.conns.find(x => x === tab.id+':'+tab.connectionId)) {
                    ctx.conns = [tab.id+':'+tab.connectionId, ...ctx.conns];
                    ctx.updated = true;
                } else {
                    ctx.updated = false;
                }
                ctx.tab = tab;
                return ctx;
            }, {tab: null, conns:[], updated: false})
            .filter(ctx => ctx.updated)
            .map(x => x.tab);
    }
        
    public goto(tabId: number) {
        this.ops.next((tabs: Tab[]) => {
            return tabs.map(t => {
                t.active = tabId === t.id;
                return t;
            });
        });
    }
    
    public newTab() {
          this.ops.next((tabs: Tab[]) => {
                const conn = tabs.find(t => t.active).connectionId;
                const tab = this.getNewTab(tabs, conn);
                    // console.log('inserting new tab', newTab.id, tabs.length, newTab.active);
                    return [
                        ...tabs.map(tab => {
                            tab.active = false; 
                            return tab;
                        }),
                        tab
                    ] 
                });
    }
    
    public setConnection(tabId: number, conn: Connection) {
        this.ops.next((tabs: Tab[]) => {
            return tabs.map(x => {
                if (x.id === tabId) {
                    //console.log(`setting tab id ${x.id} (has conn ${JSON.stringify(x.connectionId)}) to conn id ${JSON.stringify(conn.id)}`);
                    x.connectionId = conn.id;
                }
                return x;
            });
        });
    }

    // handles updates to the tabs as the connections change.
    private handleConnections(conns: Connection[]) {
        this.ops.next((tabs: Tab[]) => {
            let filtered = tabs.filter(tab => {
                return conns.find(c => c.id === tab.connectionId) !== undefined;
            });
            if (filtered.length > 0 && !filtered.find(x => x.active)  && tabs.length !== filtered.length) {
                // set a new active
                filtered[0].active = true;
            } else if (filtered.length === 0 && conns.length > 0) {
                // id will take account for old tabs
                filtered.push(this.getNewTab(tabs, conns[0].id));
            }
            return filtered;
        });
    }
    
    private getNewTab(tabs: Tab[], connectionId: number, active = true): Tab {
        const id = tabs.reduce((max, val) => val.id >= max ? val.id + 1 : max, 0);
        return <Tab> {
            id,
            active,
            connectionId,
            title: `Query ${id}`,
        };
    }
    
    private get activeBase(): Observable<Tab> {
        return this.stream
            .filter(ts => ts.length > 0)
            .map(ts => {
                let active = ts.find(t => t.active);
                if (!active) {
                    throw 'no active found';
                }
                return active;
            });
    }
}
