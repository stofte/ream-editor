import * as chai from 'chai';
import { expect } from 'chai';
import * as sinon from 'sinon';
import * as sinonChai from 'sinon-chai';
import { ReflectiveInjector, enableProdMode } from '@angular/core';
import { Http, XHRBackend, ConnectionBackend, BrowserXhr, ResponseOptions, 
    BaseResponseOptions, RequestOptions, BaseRequestOptions } from '@angular/http';
import { Observable } from 'rxjs/Rx';
import { QueryStream, EditorStream, ResultStream, OmnisharpStream, InputStream, StreamStarter } from './index';
import { OutputStream } from './output.stream';
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
    let input: InputStream = null;
    let injector: ReflectiveInjector = null;
    let query: QueryStream = null;
    let omnisharp: OmnisharpStream = null;
    let output: OutputStream = null;
    
    before(function() {
        chai.expect();
        chai.use(sinonChai);
        const injector = ReflectiveInjector.resolveAndCreate([
            Http, BrowserXhr, XSRFStrategyMock,
            { provide: ConnectionBackend, useClass: XHRBackend },
            { provide: ResponseOptions, useClass: BaseResponseOptions },
            { provide: RequestOptions, useClass: BaseRequestOptions },
            InputStream,
            OutputStream,
            StreamStarter,
            EditorStream,
            ResultStream,
            QueryStream,
            OmnisharpStream
        ]);
        input = injector.get(InputStream);
        output = injector.get(OutputStream);
        query = injector.get(QueryStream);
        omnisharp = injector.get(OmnisharpStream);
        injector.get(StreamStarter);
    });

    [
        [0, 0], [0, 0]
        // steps below take about 1 hour
        // [0, 0], [0, 0], [0, 0], [0, 0], [0, 0],
        // [0, 0], [0, 0], [0, 0], [0, 0], [0, 0],
        // [0, 0], [0, 0], [0, 0], [0, 0], [0, 0],
        // [5, 30], [5, 30], [5, 30], [5, 30], [5, 30],
        // [5, 30], [5, 30], [5, 30], [5, 30], [5, 30],
        // [5, 30], [5, 30], [5, 30], [5, 30], [5, 30],
        // [5, 60], [5, 60], [5, 60], [5, 60], [5, 60],
        // [5, 60], [5, 60], [5, 60], [5, 60], [5, 60],
        // [5, 60], [5, 60], [5, 60], [5, 60], [5, 60],
        // [10, 50], [10, 50], [10, 50], [10, 50], [10, 50],
        // [10, 50], [10, 50], [10, 50], [10, 50], [10, 50],
        // [10, 50], [10, 50], [10, 50], [10, 50], [10, 50],
        // [50, 50], [50, 50], [50, 50], [50, 50], [50, 50],
        // [50, 50], [50, 50], [50, 50], [50, 50], [50, 50],
        // [50, 50], [50, 50], [50, 50], [50, 50], [50, 50],
        // [10, 100], [10, 100], [10, 100], [10, 100], [10, 100],
        // [10, 100], [10, 100], [10, 100], [10, 100], [10, 100],
        // [10, 100], [10, 100], [10, 100], [10, 100], [10, 100],
        // [50, 100], [50, 100], [50, 100], [50, 100], [50, 100],
        // [50, 100], [50, 100], [50, 100], [50, 100], [50, 100],
        // [50, 100], [50, 100], [50, 100], [50, 100], [50, 100],
        // [50, 150], [50, 150], [50, 150], [50, 150], [50, 150],
        // [50, 150], [50, 150], [50, 150], [50, 150], [50, 150],
        // [50, 150], [50, 150], [50, 150], [50, 150], [50, 150],
        // [50, 200], [50, 200], [50, 200], [50, 200], [50, 200],
        // [50, 200], [50, 200], [50, 200], [50, 200], [50, 200],
        // [50, 200], [50, 200], [50, 200], [50, 200], [50, 200],
        // [100, 200], [100, 200], [100, 200], [100, 200], [100, 200],
        // [100, 200], [100, 200], [100, 200], [100, 200], [100, 200],
        // [100, 200], [100, 200], [100, 200], [100, 200], [100, 200]
    ].forEach(([replayMinDelay, replayMaxDelay]) => {
    describe(`delay: ${replayMinDelay} -> ${replayMaxDelay}ms`, () => {

    it('emits result for simple value expressions', function(done) {
        this.timeout(backendTimeout * (cSharpTestData.length + 1));
        let verifyCount = 0;
        cSharpTestData.forEach((testData, idx: number) => {
            // only a single page
            const expectedPage = cSharpTestDataExpectedResult[idx][0];
            const id = uuid.v4();
            const resultSub = output.events
                .filter(msg => msg.id === id)
                .subscribe(msg => {
                    if (msg.name === EventName.ResultDone) {
                        resultSub.unsubscribe();
                        if (verifyCount === cSharpTestData.length) {
                            expect(verifyCount).to.equal(cSharpTestData.length, 
                                `verifyCount (${verifyCount}) should match length of test data: ${cSharpTestData.length}`);
                            verifyCount++; // to avoid running check in other thread
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
                () => input.new(id),
                { 
                    for: testData.events,
                    fn: (evt) => input.edit(id, evt)
                },
                () => input.executeBuffer(id)
            ], replayMaxDelay, replayMinDelay);
        });
    });

    it('emits results for linq based query against sqlite database', function(done) {
        this.timeout(backendTimeout * 3);
        const expectedPage = cSharpCityFilteringQueryEditorTestData[0]; 
        const id = uuid.v4();
        let rowCount = 0;
        let headers: any[] = null;
        let rows: any[] = null;
        const resultSub = output.events
            .filter(msg => msg.id === id)
            .subscribe(msg => {
                if (msg.name === EventName.ResultDone) {
                    resultSub.unsubscribe();
                    checkAndExit(done, () => {
                        let cityColIdx = 0;
                        expect(headers).to.contain('Name', '"Name" column on table');
                        for (let i = 0; i < headers.length; i++) {
                            if (headers[i] === 'Name') {
                                cityColIdx = i;
                                break;
                            }
                        }
                        for (let i = 0; i < rows.length; i++) {
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
            () => input.new(id, sqliteConnection),
            { 
                for: cSharpCityFilteringQueryEditorTestData[0].events,
                fn: (evt) => input.edit(id, evt)
            }, () => input.executeBuffer(id)
        ], replayMaxDelay, replayMinDelay);
    });

    it('emits codecheck for simple statement', function(done) {
        this.timeout(backendTimeout * 2);
        const id = uuid.v4();
        const firstEdits = codecheckEditorTestData[0].events.filter(x => x.time < 6000);
        const secondEdits = codecheckEditorTestData[0].events.filter(x => x.time >= 6000);
        let codechecks = 0;
        const codecheckSub = output.events.filter(msg => msg.name === EventName.OmniSharpCodeCheck && msg.id === id).subscribe(msg => {
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
            () => input.new(id),
            {
                for: firstEdits,
                fn: (evt) => input.edit(id, evt)
            },
            () => input.codeCheck(id),
            {
                for: secondEdits,
                fn: (evt) => input.edit(id, evt)
            },
            () => input.codeCheck(id)
        ], replayMaxDelay, replayMinDelay);
    });


    it('emits codecheck for database query', function(done) {
        this.timeout(backendTimeout * 2);
        const id = uuid.v4();
        const firstEdits = cSharpDatabaseCodeCheckEditorTestData[0].events.filter(x => x.time < 6000);
        const secondEdits = cSharpDatabaseCodeCheckEditorTestData[0].events.filter(x => x.time >= 6000);
        let codechecks = 0;
        let isFirstCheck = true;
        const codecheckSub = output.events.filter(msg => msg.name === EventName.OmniSharpCodeCheck && msg.id === id).subscribe(msg => {
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
            () => input.new(id, sqliteConnection),
            {
                for: firstEdits,
                fn: (evt) => input.edit(id, evt)
            },
            () => input.codeCheck(id),
            {
                for: secondEdits,
                fn: (evt) => input.edit(id, evt)
            },
            () => input.codeCheck(id)
        ], replayMaxDelay, replayMinDelay);
    });

    it('emits autocompletion for simple statement', function(done) {
        this.timeout(backendTimeout * 2);
        const completionSub = output.events.filter(msg => msg.name === EventName.OmniSharpAutocompletion).subscribe(msg => {
            completionSub.unsubscribe();
            const items = msg.data.map(x => x.CompletionText);
            Assert(cSharpAutocompletionExpectedValues[0].length > 0, 'Found no completion items');
            cSharpAutocompletionExpectedValues[0].forEach(expectedEntry => {
                expect(items).to.contain(expectedEntry, `Expected completion item "${expectedEntry}"`);
            });
            done();
        });
        const id = uuid.v4();
        replaySteps([
            () => input.new(id),
            {
                for: cSharpAutocompletionEditorTestData[0].events,
                fn: (evt) => input.edit(id, evt)
            },
            () => input.autoComplete(id, cSharpAutocompletionRequestTestData[0])
        ], replayMaxDelay, replayMinDelay);
    });

    it('emits codecheck after switching buffer context', function(done) {
        this.timeout(backendTimeout * 3);
        sqliteConnection.id = 42;
        const id = uuid.v4();
        const firstEdits = cSharpContextSwitchEditorTestData[0].events.filter(x => x.time < 5000);
        const secondEdits = cSharpContextSwitchEditorTestData[0].events.filter(x => x.time >= 5000);
        const expectedCheck = cSharpContextSwitchExpectedCodeChecks[0];
        let codechecks = 0;
        const codecheckSub = output.events
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
            () => input.new(id),
            {
                for: firstEdits, // yields text "city", which is illegal in code buffer
                fn: (evt) => input.edit(id, evt)
            },
            () => input.codeCheck(id),
            // switch
            () => input.setContext(id, sqliteConnection),
            () => { },
            {
                for: secondEdits,
                fn: (evt) => input.edit(id, evt)
            },
            () => input.codeCheck(id)
        ], replayMaxDelay, replayMinDelay);
    });

    })}); // end main test loop

    describe('shutdowns', () => {
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
