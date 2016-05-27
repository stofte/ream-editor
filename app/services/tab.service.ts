import { Injectable } from '@angular/core';
import { Router } from '@angular/router-deprecated';
import { OmnisharpService } from '../services/omnisharp.service';
import { QueryService } from '../services/query.service';
import { MonitorService } from '../services/monitor.service';
import { BufferNameStream } from '../streams/buffer-name.stream';
import { Connection } from '../models/connection';
import { Tab } from '../models/tab';

@Injectable()
export class TabService {
    private id: number = 1;
    public tabs: Tab[] = [];
    
    constructor(
        private router: Router,
        private omnisharpService: OmnisharpService,
        private queryService: QueryService,
        private monitorService: MonitorService,
        private bufferNameStream: BufferNameStream
    ) {
        
    }
    
    public newForeground(connection: Connection, navigate = true) {
        const tab = new Tab();
        tab.id = this.id++;
        tab.title = `Query ${tab.id}`;
        tab.output = null; // new tab has no output set
        tab.connection = connection == null ? this.tabs.find(x => x.active).connection : connection;
        tab.fileName = this.bufferNameStream.newName(tab.id);
        tab.omnisharp = new Promise<void>((done, err) => {
            tab.omnisharpReady = () => done();
        });
        this.omnisharpService.initializeTab(tab);
        this.tabs.forEach(t => t.active = false);
        tab.active = true;
        this.tabs.push(tab);
        if (navigate) {
            this.goto(tab);
        }       
    }
    
    public updateTabId(tabId: number, connection: Connection): void {
        const tab = this.get(tabId);
        tab.connection = connection;
        this.updateTab(tab);
    }
    
    public updateTab(tab: Tab): void {
        this.tabs.find(t => {
            if (t.id === tab.id) {
                let updateTemplate = tab.connection && t.connection && 
                    t.connection.id !== tab.connection.id;
                t.connection = tab.connection;
                t.output = tab.output; 
                t.title = tab.title;
                if (updateTemplate) {
                    this.omnisharpService.initializeTab(t);
                }
                // might not actually go anywhere but of well
                this.goto(t);
                return true;
            }
            return false;
        });
    }
    
    public get(id: number) {
        const tab = this.tabs.find(x => x.id === id);
        return tab && tab.clone(); 
    }
    
    public get active(): Tab {
        let a = this.tabs.find(x => x.active);
        return a && a.clone();
    }
    
    public routedTo(id: number): void {
        this.tabs.forEach(t => {
            t.active = t.id === id;
        });
    }
    
    private goto(tab: Tab): void {
        this.router.navigate(['EditorTab', { tab: tab.id, connection: tab.connection.id }]);
    }
}
