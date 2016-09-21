import * as chai from 'chai';
import { expect } from 'chai';
import * as sinon from 'sinon';
import * as sinonChai from 'sinon-chai';
import { ReflectiveInjector, enableProdMode } from '@angular/core';
import { Http, XHRBackend, ConnectionBackend, BrowserXhr, ResponseOptions, 
    BaseResponseOptions, RequestOptions, BaseRequestOptions } from '@angular/http';
import { Observable } from 'rxjs/Rx';
import { QueryMessage } from '../../messages/index';
import { ProcessStream, QueryStream, SessionStream, EditorStream, ResultStream } from '../index';
import config from '../../config';
import XSRFStrategyMock from '../../test/xsrf-strategy-mock';
import { cSharpTestData, cSharpTestDataExpectedResult } from '../../test/editor-testdata';
import replaySteps from '../../test/replay-steps';
import * as uuid from 'node-uuid';
const http = electronRequire('http');
const backendTimeout = config.unitTestData.backendTimeout;

describe('everything int-test', function() {
    this.timeout(backendTimeout * 3000);
    let session: SessionStream = null;
    let editor: EditorStream = null;
    let result: ResultStream = null;
    let injector: ReflectiveInjector = null;
    let query: QueryStream = null;
    
    before(function() {
        chai.expect();
        chai.use(sinonChai);
        injector = ReflectiveInjector.resolveAndCreate([
            Http, BrowserXhr, XSRFStrategyMock,
            { provide: ConnectionBackend, useClass: XHRBackend },
            { provide: ResponseOptions, useClass: BaseResponseOptions },
            { provide: RequestOptions, useClass: BaseRequestOptions },
            QueryStream,
            ProcessStream,
            SessionStream,
            EditorStream,
            ResultStream
        ]);
        session = injector.get(SessionStream);
        editor = injector.get(EditorStream);
        result = injector.get(ResultStream);
        query = injector.get(QueryStream);
    });

    it('emits result pages for legal csharp', function(done) {
        this.timeout(backendTimeout * cSharpTestData.length * 1000);
        setTimeout(() => {
            cSharpTestData.forEach((test, idx: number) => {
                const expectedPage = cSharpTestDataExpectedResult[idx];
                const id = uuid.v4();
                let verifyCount = 0;
                // todo, if events isn't hot, nothing will happen in query, so we need to also
                // listen for something, so we check that we get no errors from the request
                const querySub = query.events.filter(msg => msg.type === 'run-code-response').subscribe(x => {
                    console.log('query res', x);
                });
                const resultSub = result.events
                    .filter(msg => msg.id === id)
                    .subscribe(msg => {
                        if (msg.type === 'done') {
                            resultSub.unsubscribe();
                            if (idx === cSharpTestData.length - 1) {
                                expect(verifyCount).to.equal(cSharpTestData.length);
                                done();
                            }
                        } else if (msg.type === 'update') {
                            expect(msg.data.id).to.equal(id);
                            expect(msg.data.title).to.equal(expectedPage.title);
                            expect(msg.data.columns).to.deep.equal(expectedPage.columns);
                            expect(msg.data.columnTypes).to.deep.equal(expectedPage.columnTypes);
                            expect(msg.data.rows).to.deep.equal(expectedPage.rows);
                            verifyCount++;
                        }
                    }); 
                
                replaySteps([
                    500, () => session.new(id),
                    { 
                        for: test.events,
                        wait: 100,
                        fn: (evt) => editor.edit(id, evt)
                    },
                    1000, () => session.runCode(id)
                ]);
            });
        }, 5000);
    });

    it('stops dotnet process when stopServer is called', function(done) {
        this.timeout(backendTimeout);
        query.once(msg => msg.type === 'closed', () => {
            setTimeout(() => {
                let url = `http://localhost:${config.queryEnginePort}/checkreadystate`;
                http.get(url, res => { done(new Error('response received')); })
                    .on('error', () => { done(); });
            }, 500);
        });
        setTimeout(() => {
            query.stopServer();
        }, 0);
    });
});
