import { Injectable} from '@angular/core';
import { Http } from '@angular/http';
import { Observable, Subject, ConnectableObservable } from 'rxjs/Rx';
import 'rxjs/Rx';
import * as _ from 'lodash';
import * as uuid from 'node-uuid';
import { QueryService } from '../services/query.service';
import { EditorService } from '../services/editor.service';
import { TabService } from '../services/tab.service';
import { MonitorService } from '../services/monitor.service';
import { AutocompletionQuery } from '../models/autocompletion-query';
import { AutocompletionResult } from '../models/autocompletion-result';
import { MirrorChangeStream } from '../services/mirror-change.stream';
import { EditorChange } from '../models/editor-change';
import { Tab } from '../models/tab';
import { TemplateResult } from '../models/template-result';
import config from '../config';
const path = electronRequire('path');

const isProduction = MODE !== 'DEVELOPMENT';
// __dirname doesn't seem to work in bundle mode
const dirname = path.resolve(`${process.env['LOCALAPPDATA']}/LinqEditor/omnisharp`);

@Injectable()
export class OmnisharpService {
    private port: number = config.omnisharpPort;
    private initialized: any = {};
    private dotnetPath: string = null;
    
    constructor(
        private queryService: QueryService,
        private editorService: EditorService,
        private monitorService: MonitorService,
        private mirrorChangeStream: MirrorChangeStream,
        private tabs: TabService,
        private http: Http
    ) {
        let pauser = Observable.fromPromise(monitorService.omnisharpReady).map(x => !x);
        pauser.switchMap(paused => paused ? Observable.never() : tabs.newContext)
            .subscribe(x => {
                console.log('omnisharp ctx', x);
            });
        this.dotnetPath = dirname;
        mirrorChangeStream.stream
            .throttleTime(250)
            .subscribe(x => {
            console.log('omnisharp saw editor change:', x);
        })
    }
    
    private handleChange(change: EditorChange) {
        this.monitorService.omnisharpReady.then(() => {
            const json = {
                fileName: change.fileName,
                changes: [change]
            }; 
            this.http
                .post(this.action('updatebuffer'), JSON.stringify(json));
        });
    }
    
    public autocomplete(request: AutocompletionQuery): Observable<any[]> {
        let mapCodeMirror: (AutocompletionResult) => any[] = this.mapToCodeMirror.bind(this);
        return this.http
            .post(this.action('autocomplete'), JSON.stringify(request))
            .map(res => res.json())
            .map(mapCodeMirror);
    }
    
    public initializeTab(tab: Tab) {

        // if (this.initialized[tab.id] === tab.connectionId.id) {
        //     // todo: might be useless?
        //     return;
        // }
        
        // // wait for backends to start before doing this
        // this.monitorService.queryReady.then(() => {
        //     this.queryService.queryTemplate(tab.connectionId)
        //         .subscribe((result: TemplateResult) => {
        //             tab.templateHeader = result.header;
        //             tab.templateFooter = result.footer;
        //             tab.templateLineOffset = result.lineOffset;
        //             this.editorService.set(tab, result.defaultQuery, false);
        //             // need to update omnisharp with an initial buffer template
        //             // from which it can perform intellisense operations
        //             let json = JSON.stringify({
        //                 FileName: tab.fileName,
        //                 FromDisk: false,
        //                 Buffer: result.template,
        //             });
        //             this.http.post(this.action('updatebuffer'), json)
        //                 .subscribe(data => {
        //                     if (data.status === 200) {
        //                         this.initialized[tab.id] = tab.connectionId.id;
        //                         tab.omnisharpReady();
        //                     }
        //                 });
        //         });
        // });
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
            .post(this.action('updatebuffer'), json);
    }
    
    private mapToCodeMirror(result: any[]): any[] {
        return _.chain(result)
            .groupBy(r => r.CompletionText)
            .map(nameList => {
                // filter out duplicates of the same name and type
                let byType = _.reduce(nameList, (saved, item: any) => {
                    let isCustomDump = item.Description === 'QueryEngine.Inlined.Dumper';
                    if (!isCustomDump && !saved.find(s => s.Kind === item.Kind)) {
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
                    sortKey: i.CompletionText.toLocaleLowerCase(), 
                    text: i.CompletionText,
                    className: `foo`
                };
            })
            .sortBy('sortKey')
            .value();
    }
    
    private action(name: string) {
        return `http://localhost:${this.port}/${name}`;
    }
}
