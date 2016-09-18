import * as chai from 'chai';
import { expect } from 'chai';
import * as sinon from 'sinon';
import * as sinonChai from 'sinon-chai';
import { ReflectiveInjector, enableProdMode } from '@angular/core';
import { Http, XHRBackend, ConnectionBackend, BrowserXhr, ResponseOptions, BaseResponseOptions, RequestOptions, BaseRequestOptions } from '@angular/http';
import { Observable } from 'rxjs/Rx';
import { EditorStream, SessionStream } from './index';
import { TextUpdate } from '../models/index';
import { ProcessHelper } from '../utils/process-helper';
import * as uuid from 'node-uuid';
import config from '../config';
import XSRFStrategyMock from '../test/xsrf-strategy-mock';
const backendTimeout = config.unitTestData.backendTimeout;
const fs = electronRequire('fs');
import { randomTestData } from '../test/editor-testdata';

describe('editor.stream int-test', function() {
    this.timeout(30 * 1000);

    let injector: ReflectiveInjector;
    let editor: EditorStream = null;
    let session: SessionStream = null;
    let processHelper = new ProcessHelper();
    
    before(function(done) {
        setTimeout(() => {
            chai.expect();
            chai.use(sinonChai);
            injector = ReflectiveInjector.resolveAndCreate([
                Http, BrowserXhr, XSRFStrategyMock,
                { provide: ConnectionBackend, useClass: XHRBackend },
                { provide: ResponseOptions, useClass: BaseResponseOptions },
                { provide: RequestOptions, useClass: BaseRequestOptions },
                EditorStream,
                SessionStream
            ]);
            session = injector.get(SessionStream);
            editor = injector.get(EditorStream);
            done();
        }, 1000);
    });

    it('emits expected texts when runCode is invoked', function(done) {
        this.timeout(backendTimeout);
        randomTestData.forEach((test, idx: number) => {
            const sub = editor.events.subscribe(msg => {
                if (msg.type === 'run-code-request') {
                    sub.unsubscribe();
                    expect(msg.text).to.equal(test.output);
                }
            });
            const id = uuid.v4();
            session.new(id);
            test.events.forEach(data => {
                editor.edit(id, data);
            });
            session.runCode(id);
            if (idx === randomTestData.length - 1) {
                done();
            }
        });
    });
});

