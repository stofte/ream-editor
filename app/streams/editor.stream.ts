import { Injectable } from '@angular/core';
import { Subject } from 'rxjs/Rx';
import { EditorEvent } from '../models/index';

export class EditorStream {
    public events: Subject<EditorEvent>;
}
