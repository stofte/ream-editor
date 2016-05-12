import { Injectable } from '@angular/core';
import { Router } from '@angular/router-deprecated';
import { OmnisharpService } from '../services/omnisharp.service';
import { QueryService } from '../services/query.service';
import { Connection } from '../models/connection';
import { Tab } from '../models/tab';

@Injectable()
export class TabService {
    private id : number = 0;
    public tabs: Tab[] = [];
    
    constructor(
        private router: Router,
        private omnisharpService: OmnisharpService,
        private queryService: QueryService
    ) {
        
    }
    
    public newForeground(connection: Connection, navigate: boolean = true) {
        var tab = new Tab();
        this.queryService
            .queryTemplate(connection)
            .subscribe(template => {
                // set template when possible
                tab.templateLineOffset = template.lineOffset;
                tab.templateHeader = template.header;
                tab.templateFooter = template.footer;                
            });
        tab.id = this.id++;
        tab.output = 'console';
        tab.connection = connection == null ? this.tabs.find(x => x.active).connection : connection;
        tab.fileName = this.omnisharpService.randomFile(tab.id);
        this.omnisharpService.initializeTab(tab);
        this.tabs.forEach(t => t.active = false);
        tab.active = true;
        this.tabs.push(tab);
        if (navigate) {
            this.goto(tab);
        }       
    }
    
    public updateTabId(tabId: number, connection: Connection, navigate: boolean = true): void{
        const tab = this.tabs.find(t => t.id === tabId);
        tab.connection = connection;
        this.updateTab(tab, navigate);
    }
    
    public updateTab(tab: Tab, navigate: boolean = true): void{
        this.tabs.find(t => {
            if (t.id === tab.id) {
                t.connection = tab.connection;
                t.output = tab.output 
                t.title = tab.title;
                this.goto(tab);
                return true;
            }
            return false;
        });
    }
    
    public get(id: number) {
        console.log('tabservice.get', this.tabs);
        return this.tabs.find(x => x.id === id); 
    }
    
    public get active(): Tab {
        return this.tabs.find(x => x.active);
    }
    
    public routedTo(id: number): void {
        this.tabs.forEach(t => {
            t.active = t.id === id;
        });
    }
    
    private goto(tab: Tab): void {
        console.log('goto', tab.id);
        this.router.navigate(["EditorTab", { tab: tab.id, connection: tab.connection.id, output: tab.output }]);
    }
}
