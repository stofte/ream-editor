import { Connection } from './connection';

export class Tab {
    public id: number; // primary key for tab
    public connection: Connection; // stores a connection reference for the given tab
    public title: string; // not used
    public active: boolean; // is the visible tab
    public output: any; // which output tab is active for the tab
    public fileName: string; // used by omnisharp, a tab should have a fixed fileName for a given connection
    public templateHeader: string;
    public templateFooter: string;
    public templateLineOffset: number; // used by omnisharp, maps the partial view into the full template. columns not mapped
}
