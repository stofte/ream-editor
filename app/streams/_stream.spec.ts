import * as chai from 'chai';
import { expect } from 'chai';
import * as sinon from 'sinon';
import * as sinonChai from 'sinon-chai';
import { ReflectiveInjector, enableProdMode } from '@angular/core';
import { Http, XHRBackend, ConnectionBackend, BrowserXhr, ResponseOptions, 
    BaseResponseOptions, RequestOptions, BaseRequestOptions } from '@angular/http';
import { Observable } from 'rxjs/Rx';
import { QueryStream, SessionStream, EditorStream, ResultStream, OmnisharpStream } from './index';
import config from '../config';
import { CodeCheckResult, AutocompletionQuery, Connection } from '../models';
import XSRFStrategyMock from '../test/xsrf-strategy-mock';
import { cSharpTestData, cSharpTestDataExpectedResult, cSharpTestDataExpectedCodeChecks,
    codecheckEditorTestData, cSharpAutocompletionEditorTestData, cSharpAutocompletionRequestTestData,
    cSharpAutocompletionExpectedValues, cSharpContextSwitchExpectedCodeChecks, 
    cSharpContextSwitchEditorTestData, cSharpCityFilteringQueryEditorTestData,
    cSharpDatabaseCodeCheckEditorTestData, cSharpDatabaseCodeCheckExpectedErrors } from '../test/editor-testdata';
import replaySteps from '../test/replay-steps';
import { EventName, Message } from './api';
import * as uuid from 'node-uuid';
const http = electronRequire('http');
const backendTimeout = config.unitTestData.backendTimeout;
const sqliteConnectionString = config.unitTestData.sqliteWorlddbConnectionString; 
const sqliteConnection = new Connection(sqliteConnectionString, 'sqlite');
sqliteConnection.id = 42;

describe('[int-test] streams', function() {
    this.timeout(backendTimeout * 100);
    let session: SessionStream = null;
    let editor: EditorStream = null;
    let result: ResultStream = null;
    let injector: ReflectiveInjector = null;
    let query: QueryStream = null;
    let omnisharp: OmnisharpStream = null;
    
    before(function() {
        chai.expect();
        chai.use(sinonChai);
        injector = ReflectiveInjector.resolveAndCreate([
            Http, BrowserXhr, XSRFStrategyMock,
            { provide: ConnectionBackend, useClass: XHRBackend },
            { provide: ResponseOptions, useClass: BaseResponseOptions },
            { provide: RequestOptions, useClass: BaseRequestOptions },
            QueryStream,
            SessionStream,
            EditorStream,
            ResultStream,
            OmnisharpStream
        ]);
        session = injector.get(SessionStream);
        editor = injector.get(EditorStream);
        result = injector.get(ResultStream);
        query = injector.get(QueryStream);
        omnisharp = injector.get(OmnisharpStream);
    });

    it('waits for backends to be ready', function(done) {
        this.timeout(backendTimeout);
        let queryReady = false;
        let omnisharpReady = false;
        query.once(msg => msg.name === EventName.ProcessReady, () => {
            queryReady = true;
            if (omnisharpReady) {
                done();
            }
        });
        omnisharp.once(msg => msg.name === EventName.ProcessReady, () => {
            omnisharpReady = true;
            if (queryReady) {
                done();
            }
        });
    });

    it('emits result for simple value expressions', function(done) {
        this.timeout(backendTimeout * (cSharpTestData.length + 1));
        let verifyCount = 0;
        cSharpTestData.forEach((testData, idx: number) => {
            // only a single page
            const expectedPage = cSharpTestDataExpectedResult[idx][0]; 
            const id = uuid.v4();
            const resultSub = result.events
                .filter(msg => msg.id === id)
                .subscribe(msg => {
                    if (msg.name === EventName.ResultDone) {
                        resultSub.unsubscribe();
                        if (idx === cSharpTestData.length - 1) {
                            expect(verifyCount).to.equal(cSharpTestData.length, 
                                `verifyCount (${verifyCount}) should match length of test data: ${cSharpTestData.length}`);
                            done();
                        }
                    } else if (msg.name === EventName.ResultUpdate) {
                        expect(msg.data.id).to.equal(id);
                        expect(msg.data.title).to.equal(expectedPage.title);
                        expect(msg.data.columns).to.deep.equal(expectedPage.columns);
                        expect(msg.data.columnTypes).to.deep.equal(expectedPage.columnTypes);
                        expect(msg.data.rows).to.deep.equal(expectedPage.rows);
                        verifyCount++;
                    }
                }); 
            
            replaySteps([
                () => session.new(id),
                { 
                    for: testData.events,
                    fn: (evt) => editor.edit(id, evt)
                },
                () => session.executeBuffer(id)
            ]);
        });
    });

    it('emits results for linq based query against sqlite database', function(done) {
        this.timeout(backendTimeout * 3);
        const expectedPage = cSharpCityFilteringQueryEditorTestData[0]; 
        const id = uuid.v4();
        let rowCount = 0;
        let headers: any[] = null;
        let rows: any[] = null;
        const resultSub = result.events
            .filter(msg => msg.id === id)
            .subscribe(msg => {
                if (msg.name === EventName.ResultDone) {
                    resultSub.unsubscribe();
                    checkAndExit(done, () => {
                        let cityColIdx = 0;
                        expect(headers).to.contain('Name', '"Name" column on table');
                        for(let i = 0; i < headers.length; i++) {
                            if (headers[i] === 'Name') {
                                cityColIdx = i;
                                break;
                            }
                        }
                        for(let i = 0; i < rows.length; i++) {
                            expect(rows[0][cityColIdx].substring(0, 2)).to.equal('Ca', 'Name of city starts with "Ca"');
                        }
                        expect(rows.length).to.equal(83, 'Row count from query');
                    });
                } else if (msg.name === EventName.ResultUpdate) {
                    rows = msg.data.rows;
                    headers = msg.data.columns;
                }
            });
        
        replaySteps([
            () => session.new(id, sqliteConnection),
            { 
                for: cSharpCityFilteringQueryEditorTestData[0].events,
                fn: (evt) => editor.edit(id, evt)
            }, () => session.executeBuffer(id)
        ]);
    });

    it('emits codecheck for simple statement', function(done) {
        this.timeout(backendTimeout * 2);
        const id = uuid.v4();
        const firstEdits = codecheckEditorTestData[0].events.filter(x => x.time < 6000);
        const secondEdits = codecheckEditorTestData[0].events.filter(x => x.time >= 6000);
        let codechecks = 0;
        const codecheckSub = omnisharp.events.filter(msg => msg.name === EventName.OmniSharpCodeCheck && msg.id === id).subscribe(msg => {
            const expectedCheck = cSharpTestDataExpectedCodeChecks[codechecks];
            codechecks++;
            check([done, codecheckSub], () => {
                expect(msg.data.length).to.equal(1, 'Expected number of codechecks');
                expect(msg.data[0].text).to.equal(expectedCheck.text, 'text');
                expect(msg.data[0].logLevel).to.equal(expectedCheck.logLevel, 'logLevel');
                expect(msg.data[0].line).to.equal(expectedCheck.line, 'line');
                expect(msg.data[0].column).to.equal(expectedCheck.column, 'column');
                expect(msg.data[0].endLine).to.equal(expectedCheck.endLine, 'endLine');
                expect(msg.data[0].endColumn).to.equal(expectedCheck.endColumn, 'endColumn');
            });
            if (codechecks >= cSharpTestDataExpectedCodeChecks.length) {
                codecheckSub.unsubscribe();
                done();
            }
        });

        replaySteps([
            () => session.new(id),
            {
                for: firstEdits,
                fn: (evt) => editor.edit(id, evt)
            },
            () => session.codeCheck(id),
            {
                for: secondEdits,
                fn: (evt) => editor.edit(id, evt)
            },
            () => session.codeCheck(id)
        ]);
    });


    it('emits codecheck for database query', function(done) {
        this.timeout(backendTimeout * 2);
        const id = uuid.v4();
        const firstEdits = cSharpDatabaseCodeCheckEditorTestData[0].events.filter(x => x.time < 6000);
        const secondEdits = cSharpDatabaseCodeCheckEditorTestData[0].events.filter(x => x.time >= 6000);
        let codechecks = 0;
        let isFirstCheck = true;
        const codecheckSub = omnisharp.events.filter(msg => msg.name === EventName.OmniSharpCodeCheck && msg.id === id).subscribe(msg => {
            const expectedCheck = cSharpDatabaseCodeCheckExpectedErrors[codechecks];
            if (isFirstCheck) {
                isFirstCheck = false;
                check([done, codecheckSub], () => {
                    expect(msg.data.length).to.equal(1);
                    expect(msg.data[0].line).to.equal(expectedCheck.line, 'line');
                    expect(msg.data[0].column).to.equal(expectedCheck.column, 'column');
                    expect(msg.data[0].endLine).to.equal(expectedCheck.endLine, 'endLine');
                    expect(msg.data[0].endColumn).to.equal(expectedCheck.endColumn, 'endColumn');
                    expect(msg.data[0].logLevel).to.equal(expectedCheck.logLevel, 'logLevel');
                    expect(msg.data[0].text).to.equal(expectedCheck.text, 'text');
                });
            } else {
                codecheckSub.unsubscribe();
                checkAndExit(done, () => {
                    expect(msg.data.length).to.equal(0);
                });
            }
        });

        replaySteps([
            () => session.new(id, sqliteConnection),
            {
                for: firstEdits,
                fn: (evt) => editor.edit(id, evt)
            },
            () => session.codeCheck(id),
            {
                for: secondEdits,
                fn: (evt) => editor.edit(id, evt)
            },
            () => session.codeCheck(id)
        ]);
    });

    it('emits autocompletion for simple statement', function(done) {
        this.timeout(backendTimeout * 2);
        const completionSub = omnisharp.events.filter(msg => msg.name === EventName.OmniSharpAutocompletion).subscribe(msg => {
            const items = msg.data.map(x => x.CompletionText);
            Assert(cSharpAutocompletionExpectedValues[0].length > 0, 'Found no completion items');
            cSharpAutocompletionExpectedValues[0].forEach(expectedEntry => {
                expect(items).to.contain(expectedEntry, `Expected completion item "${expectedEntry}"`);
            });
            completionSub.unsubscribe();
            done();
        });
        const id = uuid.v4();
        replaySteps([
            () => session.new(id),
            {
                for: cSharpAutocompletionEditorTestData[0].events,
                fn: (evt) => editor.edit(id, evt)
            },
            () => session.autoComplete(id, cSharpAutocompletionRequestTestData[0])
        ]);
    });

    it('emits codecheck after switching buffer context', function(done) {
        this.timeout(backendTimeout * 3);
        sqliteConnection.id = 42;
        const id = uuid.v4();
        const firstEdits = cSharpContextSwitchEditorTestData[0].events.filter(x => x.time < 5000);
        const secondEdits = cSharpContextSwitchEditorTestData[0].events.filter(x => x.time >= 5000);
        const expectedCheck = cSharpContextSwitchExpectedCodeChecks[0];
        let codechecks = 0;
        const codecheckSub = omnisharp.events
            .filter(msg => msg.name === EventName.OmniSharpCodeCheck && msg.id === id)
            .subscribe(msg => {
                codechecks++;
                if (codechecks === 1) {
                    check([done, codecheckSub], () => {
                        expect(msg.data.length).to.equal(1);
                        expect(msg.data[0].text).to.equal(expectedCheck.text);
                        expect(msg.data[0].logLevel).to.equal(expectedCheck.logLevel);
                    });
                } else {
                    codecheckSub.unsubscribe();
                    checkAndExit(done, () => {
                        expect(msg.data.length).to.equal(0);
                    });
                }
            });

        replaySteps([
            () => session.new(id),
            {
                for: firstEdits, // yields text "city", which is illegal in code buffer
                fn: (evt) => editor.edit(id, evt)
            },
            () => session.codeCheck(id),
            // switch
            () => session.setContext(id, sqliteConnection),
            () => { },
            {
                for: secondEdits,
                fn: (evt) => editor.edit(id, evt)
            },
            () => session.codeCheck(id)
        ]);
    });

    it('stops query process when stopServer is called', function(done) {
        this.timeout(backendTimeout);
        query.once(msg => msg.name === EventName.ProcessClosed, () => {
            let url = `http://localhost:${config.queryEnginePort}/checkreadystate`;
            http.get(url, res => { done(new Error('response received')); })
                .on('error', () => { done(); });
        });
        replaySteps([
            () => query.stopServer()
        ]);
    });

    it('stops omnisharp process when stopServer is called', function(done) {
        this.timeout(backendTimeout);
        omnisharp.once(msg => msg.name === EventName.ProcessClosed, () => {
            let url = `http://localhost:${config.omnisharpPort}/checkreadystate`;
            http.get(url, res => { done(new Error('response received')); })
                .on('error', () => { done(); });
        });
        replaySteps([
            () => omnisharp.stopServer()
        ]);
    });
});

// Since "expect" throws inside a subscription handler, the stream crashes as a result.
// These helper functions aid in avoid crashing the suite

function check([done, subber], pred) {
    try {
        pred();
    } catch (exn) {
        subber.unsubscribe();
        done(exn);
    }
}

function checkAndExit(done, pred) {
    try {
        pred();
        done();
    } catch (exn) {
        done(exn);
    }
}
