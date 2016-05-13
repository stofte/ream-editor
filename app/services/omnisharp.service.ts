import { Injectable} from '@angular/core';
import { Http } from '@angular/http';
import { Observable } from 'rxjs/Rx';
import 'rxjs/Rx';
import * as _ from 'lodash';
import * as uuid from 'node-uuid';
import { QueryService } from '../services/query.service';
import { EditorService } from '../services/editor.service';
import { MonitorService } from '../services/monitor.service';
import { AutocompletionQuery } from '../models/autocompletion-query';
import { AutocompletionResult } from '../models/autocompletion-result';
import { EditorChange } from '../models/editor-change';
import { Tab } from '../models/tab';
import config from '../config';
const path = electronRequire('path');

@Injectable()
export class OmnisharpService {
    private port: number = config.omnisharpPort;
    private initialized: any = {};
    private dotnetPath: string = null;
    
    constructor(
        private queryService: QueryService,
        private editorService: EditorService,
        private monitorService: MonitorService, 
        private http: Http
    ) {
        // two levels up bla bla ...
        this.dotnetPath = path.dirname(path.dirname(path.dirname(__dirname))) + '/project';
        // let timer = Observable.timer(1000);
        // let throttled = editorService.changes
        //     .debounce(() => timer);
        // throttled.subscribe(this.handleChange.bind(this));
    }
    
    private handleChange(change: EditorChange) {
        this.monitorService.omnisharpReady.then(() => {
            const json = {
                fileName: change.fileName,
                changes: [change]
            }; 
            console.log('handleChange.json', json);
            this.http.post(this.action('updatebuffer'), JSON.stringify(json))
                .subscribe(result => {
                    console.log('handleChange.updatebuffer', result);
                });
        });
    }
    
    public autocomplete(request: AutocompletionQuery): Observable<any[]> {
        let mapCodeMirror: (AutocompletionResult) => any[] = this.mapToCodeMirror.bind(this);
        return this.http
            .post(this.action('autocomplete'), JSON.stringify(request))
            .map(res => res.json())
            .map(mapCodeMirror);
    }
    
    // when creating a new tab, this is used to generate the path used in omnisharp, to seperate it from other tabs
    // if a previously used tabId is seen, the previous fileName is returned instead.
    public randomFile(tabId: number) {
        return this.dotnetPath + '/b' + uuid.v4().replace(/\-/g, '') + '.cs';
    }
    
    public initializeTab(tab: Tab) {
        
        if (this.initialized[tab.id] == tab.connection.id) {
            console.log('already initted connection {0} for tab {1}', tab.connection.id, tab.id);
            return;
        }
        
        // wait for backends to start before doing this
        this.monitorService.queryReady.then(() => {
            this.queryService.queryTemplate(tab.connection)
                .subscribe(result => {
                    // need to update omnisharp with an initial buffer template
                    // from which it can perform intellisense operations
                    let json = JSON.stringify({
                        FileName: tab.fileName,
                        FromDisk: false,
                        Buffer: result.template,
                    });
                    this.http.post(this.action('updatebuffer'), json)
                        .subscribe(data => {
                            if (data.status === 200) {
                                console.log('template for tab {0} using conn {1} initialized', tab.id, tab.connection.id);
                                this.initialized[tab.id] = tab.connection.id;
                            }
                        });
                });
        });
    }
    
    private updateTab(tab: Tab, text: string) {
        let json = JSON.stringify({
            FileName: tab.fileName,
            FromDisk: false,
            Changes: [{
                NewText: text,
                StartLine: 1,
                StartColumn: 1,
                EndLine: 1,
                EndColumn: 1
            }]
        });
        this.http
            .post(this.action('updatebuffer'), json)
            .subscribe(data => {
                if (data.status === 200) {
                    console.log('template for tab {0} using conn {1} updated', tab.id, tab.connection.id);
                }
            });
    }
    
    private mapToCodeMirror(result: any[]): any[] {
        return _.chain(result)
            .groupBy(r => r.CompletionText)
            .map(nameList => {
                // filter out duplicates of the same name and type
                    let byType = _.reduce(nameList, (saved, item: any) => {
                        if (!_.includes(saved, i => i.Kind !== item.Kind)) {
                            saved.push(item);
                        }
                        return saved;
                    }, []);
                    let res = _.sortBy(byType, 'Kind');
                    return res;
            })
            .flatten()
            .map((i: any) => {
                return {
                    text: i.CompletionText,
                    className: `foo`
                };
            })
            .sortBy('text')
            .value();
    }
    
    private action(name : string) {
        return `http://localhost:${this.port}/${name}`;
    }
}
