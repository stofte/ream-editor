import { Injectable } from '@angular/core';
import { EditorChange } from '../models/editor-change';
import config from '../config';
import * as uuid from 'node-uuid';
import { ReplaySubject, Observable, Subject } from 'rxjs/Rx';
const path = electronRequire('path');

const isProduction = MODE !== 'DEVELOPMENT';
// __dirname doesn't seem to work in bundle mode
const dirname: string = path.resolve(`${process.env['LOCALAPPDATA']}/LinqEditor/omnisharp`);

class BufferMap {
    public tabId: number;
    public connId: number;
    public fileName: string;
}

@Injectable()
export class BufferNameService {
    private dotnetPath = dirname;
    private buffers: Observable<EditorChange[]>;
    private ops = new ReplaySubject<IStreamOperation>();
    
    constructor() {
        this.buffers = this.ops
            .scan((bs: BufferMap[], op) => {
                return op(bs);
            }, []);
    }
    
    public setConnection(tabId: number, connId: number): void {
        this.ops.next((bs: BufferMap[]) => {
            if (!bs.find(x => x.connId === connId && x.tabId == tabId)) {
                bs.push(<BufferMap> {
                    tabId,
                    connId,
                    fileName: `${dirname}/b${uuid.v4().replace(/\-/g, '')}.cs`
                });
            }
            return bs;
        });
        // const fileName = `${dirname}/b${uuid.v4().replace(/\-/g, '')}.cs`;
        // this.buffers.next(<EditorChange> { fileName, tabId });
        // return fileName;
    }
    
    public get byTabId(): Observable<Map<number, string>> {
        return this.buffers
            .scan((map, value) => {
                return map; //.set(value.tabId, value.fileName);
            }, new Map<number, string>());
    }
}
