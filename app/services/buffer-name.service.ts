import { Injectable } from '@angular/core';
import { EditorChange } from '../models/editor-change';
import config from '../config';
import * as uuid from 'node-uuid';
import { ReplaySubject, Observable, Subject } from 'rxjs/Rx';
import * as Rx from 'rxjs/Rx';
const path = electronRequire('path');

const isProduction = MODE !== 'DEVELOPMENT';
// __dirname doesn't seem to work in bundle mode
const dirname: string = path.resolve(`${process.env['LOCALAPPDATA']}/LinqEditor/omnisharp`);

@Injectable()
export class BufferNameService {
    private dotnetPath = dirname;
    private buffers = new ReplaySubject<EditorChange>();
    
    public newName(tabId: number): string {
        const fileName = `${dirname}/b${uuid.v4().replace(/\-/g, '')}.cs`;
        this.buffers.next(<EditorChange> { fileName, tabId });
        let z = new Observable<EditorChange>();
        return fileName;
    }
    
    public get byTabId(): Observable<Map<number, string>> {
        return this.buffers
            .scan((map, value) => {
                return map.set(value.tabId, value.fileName);
            }, new Map<number, string>());
    }
}
