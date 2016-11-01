import * as chai from 'chai';
import { expect } from 'chai';
import * as sinon from 'sinon';
import * as sinonChai from 'sinon-chai';
import { ReflectiveInjector, enableProdMode } from '@angular/core';
import { Http, XHRBackend, ConnectionBackend, BrowserXhr, ResponseOptions,
    BaseResponseOptions, RequestOptions, BaseRequestOptions } from '@angular/http';
import { Observable } from 'rxjs/Rx';
import { EditorStream } from './index';
import { InputStream } from './input.stream';
import { TextUpdate, Connection } from '../models/index';
import { ProcessHelper } from '../utils/process-helper';
import { EventName, Message } from './api';
import * as uuid from 'node-uuid';
import config from '../config';
import XSRFStrategyMock from '../test/xsrf-strategy-mock';
const backendTimeout = config.unitTestData.backendTimeout;
const fs = electronRequire('fs');
import { randomTestData } from '../test/editor-testdata';
import { check, checkAndExit, replaySteps } from '../test/test-helpers';

describe('editor.stream', function() {
    this.timeout(30 * 1000);

    let injector: ReflectiveInjector;
    let editor: EditorStream = null;
    let input: InputStream = null;
    let processHelper = new ProcessHelper();
    
    before(function() {
        chai.expect();
        chai.use(sinonChai);
        injector = ReflectiveInjector.resolveAndCreate([
            Http, BrowserXhr, XSRFStrategyMock,
            { provide: ConnectionBackend, useClass: XHRBackend },
            { provide: ResponseOptions, useClass: BaseResponseOptions },
            { provide: RequestOptions, useClass: BaseRequestOptions },
            EditorStream,
            InputStream
        ]);
        input = injector.get(InputStream);
        editor = injector.get(EditorStream);
    });

    it('emits expected texts when runCode is invoked', function(done) {
        this.timeout(backendTimeout);
        input.connect();
        randomTestData.forEach((test, idx: number) => {
            const id = uuid.v4();
            const sub = editor.events.filter(x => x.id === id).subscribe(msg => {
                if (msg.name === EventName.EditorExecuteText) {
                    sub.unsubscribe();
                    if (idx === randomTestData.length - 1) {
                        checkAndExit(done, () => {
                            expect(msg.data).to.equal(test.output);
                        });
                    } else {
                        check([done, () => {}], () => {
                            expect(msg.data).to.equal(test.output);
                        });
                    }
                }
            });
            replaySteps([
                () => input.new(id),
                { 
                    for: test.events,
                    fn: (evt) => input.edit(id, evt)
                },
                () => input.executeBuffer(id)
            ], 0, 0);
        });
    });

    it('emits expected texts when context is invoked', function(done) {
        this.timeout(backendTimeout);
        randomTestData.forEach((test, idx: number) => {
            const sub = editor.events.subscribe(msg => {
                if (msg.name === EventName.EditorBufferText) {
                    sub.unsubscribe();
                    expect(msg.data).to.equal(test.output);
                }
            });
            const id = uuid.v4();
            input.new(id);
            test.events.forEach(data => {
                input.edit(id, data);
            });
            input.setContext(id, new Connection('foo', 'sqlite'));
            if (idx === randomTestData.length - 1) {
                done();
            }
        });
    });
});

