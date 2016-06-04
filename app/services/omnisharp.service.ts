import { Injectable} from '@angular/core';
import { Http } from '@angular/http';
import { Observable, Subject, ConnectableObservable, Observer, ReplaySubject } from 'rxjs/Rx';
import 'rxjs/Rx';
import * as _ from 'lodash';
import * as uuid from 'node-uuid';
// import { QueryService } from '../services/query.service';
// import { EditorService } from '../services/editor.service';
import { TabService } from '../services/tab.service';
import { MonitorService } from '../services/monitor.service';
import { AutocompletionQuery } from '../models/autocompletion-query';
import { AutocompletionResult } from '../models/autocompletion-result';
import { MirrorChangeStream } from '../services/mirror-change.stream';
import { EditorChange } from '../models/editor-change';
import { ConnectionService } from '../services/connection.service';
import { Tab } from '../models/tab';
import { CodeCheckResult } from '../models/code-check-result';
import { Connection } from '../models/connection';
import { TemplateResult } from '../models/template-result';
import config from '../config';
const path = electronRequire('path');

// all templates have the same offset
let TEMPLATE_LINE_OFFSET = null;

const isProduction = MODE !== 'DEVELOPMENT';
// __dirname doesn't seem to work in bundle mode
const dirname = path.resolve(`${process.env['LOCALAPPDATA']}/LinqEditor/omnisharp`);

class SessionMap {
    fileName: string;
    tabId: number;
    connId: number;
}

class UpdateMap {
    status: boolean;
    fileName: string;
}

class QueryTemplateResult {
    buffer: string;
    tabId: number;
    connId: number;
}

@Injectable()
export class OmnisharpService {
    private port: number = config.omnisharpPort;
    private initialized: any = {};
    
    private sessions: Observable<SessionMap[]>;
    private ready: Observable<boolean>;
    private fileName: Observable<string>;
    private changeStream: Observable<number>;
    private readyState: Observable<UpdateMap>;
    private readyState2: ReplaySubject<UpdateMap>;
    private codecheck: Observable<CodeCheckResult[]>;
    
    constructor(
        private conns: ConnectionService,
        private monitorService: MonitorService,
        private mirrorChangeStream: MirrorChangeStream,
        private tabs: TabService,
        private http: Http
    ) {
        const startUpPauser = Observable.fromPromise(monitorService.ready)
            .startWith(false);

        this.sessions = startUpPauser
            .switchMap(ready => {
                return ready ? tabs.newContext : Observable.never<Tab>();
            })
            .withLatestFrom(conns.all, (tab, newConns) => { return { tab, conns: newConns }; })
            .scan((ctx, newCtx) => {
                const conn = newCtx.conns.find(x => x.id === newCtx.tab.connectionId);
                const tabId = newCtx.tab.id;
                return { conn, tabId };
            }, {})
            .flatMap((x: {conn: Connection, tabId: number }) => {
                let req = { connectionString: x.conn.connectionString, text: '' };
                return new Observable<QueryTemplateResult>((obs: Observer<QueryTemplateResult>) => {
                    http.post('http://localhost:8111/querytemplate', JSON.stringify(req))
                        .map(res => res.json())
                        .subscribe(data => {
                            if (!TEMPLATE_LINE_OFFSET) {
                                TEMPLATE_LINE_OFFSET = data.LineOffset;
                            }
                            obs.next(<QueryTemplateResult> {
                                tabId: x.tabId,
                                buffer: <string> data.Template,
                                connId: x.conn.id
                            });
                            obs.complete();
                        });
                });
            })
            .flatMap(x => {
                let json = {
                    FileName: `${dirname}/b${uuid.v4().replace(/\-/g, '')}.cs`,
                    FromDisk: false,
                    Buffer: x.buffer,
                };
                return new Observable<SessionMap>((obs: Observer<SessionMap>) => {
                    http.post('http://localhost:2000/updatebuffer', JSON.stringify(json))
                        .map(res => res.json)
                        .subscribe(data => {
                            obs.next(<SessionMap> {
                                fileName: json.FileName,
                                tabId: x.tabId,
                                connId: x.connId
                            });
                            obs.complete();
                        });
                });
            })
            .startWith(<SessionMap> null)
            .scan((sessions, newSession) => {
                if (newSession) {
                    return [newSession, ...sessions];
                }
                return sessions;
            }, [])
            .publishReplay()
            .refCount();
        
        this.ready = tabs.active
            .combineLatest(this.sessions, (tab, sessions) => {
                let session = sessions.find(x => x.tabId === tab.id && x.connId === tab.connectionId);
                return session !== undefined;
            })
            .distinctUntilChanged()
            ;
        
        this.fileName = tabs.active
            .combineLatest(this.sessions, (tab, sessions) => {
                let session = sessions.find(x => x.tabId === tab.id && x.connId === tab.connectionId);
                return session !== undefined && session.fileName;
            })
            .filter(x => !!x) // if session wasn't found, filter undefined/false
            ;
        
        let mirrorBuffer: Observable<boolean> = Observable.timer(0, 250)
            .combineLatest(this.ready, (val, status) => {
                return status; // ? 42 : status;
            })
            .filter(x => x)
            ;
        
        this.readyState = mirrorChangeStream.changes
            .buffer(mirrorBuffer)
            .filter(x => x.length > 0)
            .withLatestFrom(this.fileName, (changes, fileName) => {
                return changes.map(x => {
                    x.fileName = fileName;
                    return x;
                });
            })
            .flatMap(changes => {
                let json = {
                    FileName: changes[0].fileName,
                    FromDisk: false,
                    Changes: changes.map(c => {
                        return <EditorChange> {
                            newText: c.newText,
                            startLine: c.startLine + 1 + TEMPLATE_LINE_OFFSET,
                            startColumn: c.startColumn + 1,
                            endLine: c.endLine + 1 + TEMPLATE_LINE_OFFSET,
                            endColumn: c.endColumn + 1
                        };
                    }),
                };
                return new Observable<number>((obs: Observer<number>) => {
                    http.post('http://localhost:2000/updatebuffer', JSON.stringify(json))
                        .map(res => res.status === 200)
                        .subscribe(data => {
                            obs.next(performance.now());
                            obs.complete();
                        });
                });
            })
            .combineLatest(mirrorChangeStream.changes, (latest, change) => {
                return <UpdateMap> {
                    status: change.created - latest > 0,
                    fileName: change.fileName
                }
            })
            .publishReplay()
            .refCount()
            ;

        // todo fix this bullshit
        this.readyState2 = new ReplaySubject<UpdateMap>(1);
        this.readyState
            .subscribe(x => this.readyState2.next(x));
            
        this.codecheck = this.readyState2
            .delay(500)
            .flatMap(update => {
                 return new Observable<CodeCheckResult[]>((obs: Observer<CodeCheckResult[]>) => {
                    http.post('http://localhost:2000/codecheck', JSON.stringify({ FileName: update.fileName }))
                        .map(res => res.json())
                        .map(this.mapQuickFixes)
                        .subscribe(data => {
                            obs.next(data);
                            obs.complete();
                        });
                });               
            })
            .map(fixes => fixes.filter(fix => fix.logLevel !== 'Hidden'))
            .filter(fixes => fixes.length > 0)
            ;
            
        this.codecheck.subscribe(x => {
            console.log('codecheck', x);
        });
    }
    
    public autocomplete(request: AutocompletionQuery): Observable<any[]> {
        let mapCodeMirror: (AutocompletionResult) => any[] = this.mapToCodeMirror.bind(this);
        return this.readyState2
            .filter(x => x.status)
            .take(1)
            .withLatestFrom(this.fileName, (status, fileName) => {
                return fileName;
            })
            .flatMap(fileName => {
                let json = request;
                json.fileName = fileName;
                json.column += 1;
                json.line += 1 + TEMPLATE_LINE_OFFSET;
                return new Observable<any>((obs: Observer<any>) => {
                    this.http.post('http://localhost:2000/autocomplete', JSON.stringify(json))
                        .map(res => res.json())
                        .map(mapCodeMirror)
                        .subscribe(data => {
                            obs.next(data);
                            obs.complete();
                        });
                });                
            })
            ;
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
    
    private mapQuickFixes(result: any): CodeCheckResult[] {
        let fixes: any[] = result.QuickFixes;
        return fixes.map(x => {
            return <CodeCheckResult> {
                text: x.Text,
                logLevel: x.LogLevel,
                fileName: x.FileName,
                line: x.Line,
                endLine: x.EndLine,
                column: x.Column,
                endColumn: x.EndColumn
            };
        })
    }
    
    private action(name: string) {
        return `http://localhost:${this.port}/${name}`;
    }
}
