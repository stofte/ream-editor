import * as chai from 'chai';
import { expect } from 'chai';
import * as sinon from 'sinon';
import * as sinonChai from 'sinon-chai';
import { ReflectiveInjector, enableProdMode } from '@angular/core';
import { Http, XHRBackend, ConnectionBackend, BrowserXhr, ResponseOptions, 
    BaseResponseOptions, RequestOptions, BaseRequestOptions } from '@angular/http';
import { Observable } from 'rxjs/Rx';
import { QueryStream, EditorStream, ResultStream, OmnisharpStream, InputStream, StreamManager } from './index';
import { OutputStream } from './output.stream';
import config from '../config';
import { CodeCheckResult, AutocompletionQuery, Connection } from '../models';
import XSRFStrategyMock from '../test/xsrf-strategy-mock';
import { cSharpTestData, cSharpTestDataExpectedResult, cSharpTestDataExpectedCodeChecks,
    codecheckEditorTestData, cSharpAutocompletionEditorTestData, cSharpAutocompletionRequestTestData,
    cSharpAutocompletionExpectedValues, cSharpContextSwitchExpectedCodeChecks, 
    cSharpContextSwitchEditorTestData, cSharpCityFilteringQueryEditorTestData,
    cSharpDatabaseCodeCheckEditorTestData, cSharpDatabaseCodeCheckExpectedErrors, randomTestData } from '../test/editor-testdata';
import { EventName, Message } from './api';
import { check, checkAndExit, replaySteps } from '../test/test-helpers';
import * as uuid from 'node-uuid';
const http = electronRequire('http');
const backendTimeout = config.unitTestData.backendTimeout;
const sqliteConnectionString = config.unitTestData.sqliteWorlddbConnectionString; 
const sqliteConnection = new Connection(sqliteConnectionString, 'sqlite');
sqliteConnection.id = 42;

describe('[int-test] streams', function() {
    this.timeout(backendTimeout * 100);
    let input: InputStream = null;
    let query: QueryStream = null;
    let omnisharp: OmnisharpStream = null;
    let output: OutputStream = null;
    let streamManager: StreamManager = null;
    
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
            StreamManager,
            EditorStream,
            ResultStream,
            QueryStream,
            OmnisharpStream
        ]);
        input = injector.get(InputStream);
        output = injector.get(OutputStream);
        query = injector.get(QueryStream);
        omnisharp = injector.get(OmnisharpStream);
        streamManager = injector.get(StreamManager);
    });

    describe('single flows', function() {
        it('emits results for simple value expressions', done => {
            emitsResultsForSimpleValueExpressions(done, 10, 0);
        });
        it('emits results for linq based query against sqlite database', done => {
            emitsResultsForLinqBasedQueryAgainstSqliteDatabase(done, 10, 0);
        });
        it('emits codechecks for simple value expressions', done => {
            emitsCodecheckForSimpleStatement(done, 10, 0);
        });
        it('emits codechecks for database query', done => {
            emitsCodecheckForDatabaseQuery(done, 10, 0);
        });
        it('emits autocompletions for simple expression', done => {
            emitsAutocompletionForSimpleStatement(done, 10, 0);
        });
        it('emits autocompletions after switching buffer context', done => {
            emitsCodecheckAfterSwitchingBufferContext(done, 10, 0);
        });
        it('emits results after swithing buffer context', done => {
            emitsResultsAfterSwitchingBufferContext(done, 10, 0);
        });

        it('emits expected diagnostics for invalid query when calling query backend', done => {
            emitsExpectedDiagnosticsForInvalidQueryWhenCallingQueryBackend(done, 10, 0);
        });
    });

    let runTimings = {  };

    function getAllTimings() {
        return `all: ` + JSON.stringify(runTimings);
    }

    [
        //[0, 10, 20]
    ].forEach(([replayMinDelay, replayMaxDelay, stepCount]) => {
        describe(`input delay: ${replayMinDelay} -> ${replayMaxDelay} ms, repeat: ${stepCount}`, () => {
            function runAllAtOnce(allDone) {
                this.timeout(backendTimeout);

                let step1Resolver = null;
                let step2Resolver = null;
                let step3Resolver = null;
                let step4Resolver = null;
                let step5Resolver = null;
                let step6Resolver = null;
                let step7Resolver = null;
                const step1Promise = new Promise((done) => step1Resolver = done);
                const step2Promise = new Promise((done) => step2Resolver = done);
                const step3Promise = new Promise((done) => step3Resolver = done);
                const step4Promise = new Promise((done) => step4Resolver = done);
                const step5Promise = new Promise((done) => step5Resolver = done);
                const step6Promise = new Promise((done) => step6Resolver = done);
                const step7Promise = new Promise((done) => step7Resolver = done);

                emitsResultsForSimpleValueExpressions(step1Resolver, replayMaxDelay, replayMinDelay);
                emitsResultsForLinqBasedQueryAgainstSqliteDatabase(step2Resolver, replayMaxDelay, replayMinDelay);
                emitsCodecheckForSimpleStatement(step3Resolver, replayMaxDelay, replayMinDelay);
                emitsCodecheckForDatabaseQuery(step4Resolver, replayMaxDelay, replayMinDelay);
                emitsAutocompletionForSimpleStatement(step5Resolver, replayMaxDelay, replayMinDelay);
                emitsCodecheckAfterSwitchingBufferContext(step6Resolver, replayMaxDelay, replayMinDelay);
                emitsResultsAfterSwitchingBufferContext(step7Resolver, replayMaxDelay, replayMinDelay);

                const failures = [];
                // todo do something nicer
                const doneHandler = (val) => {
                    if (val) {
                        failures.push(val);
                    }
                };
                step1Promise.then(doneHandler);
                step2Promise.then(doneHandler);
                step3Promise.then(doneHandler);
                step4Promise.then(doneHandler);
                step5Promise.then(doneHandler);
                step6Promise.then(doneHandler);
                step7Promise.then(doneHandler);

                Promise
                    .all([
                        step1Promise,
                        step2Promise,
                        step3Promise,
                        step4Promise,
                        step5Promise,
                        step6Promise,
                        step7Promise
                    ])
                    .then(() => {
                        if (failures.length > 0) {
                            allDone(failures);
                        } else {
                            allDone();
                        }
                    });
            }

            let idx = 0;
            const l = [];
            l[stepCount - 1] = 1;
            for (let i = 0; i < l.length; i++) {
                l[i] = i;
            }
            l.forEach(() => {
                it('runs all flows at once /' + (idx++), runAllAtOnce);
            });
        });
    });


    function emitsResultsForSimpleValueExpressions(done, replayMaxDelay, replayMinDelay) {
        let verifyCount = 0;
        cSharpTestData.forEach((testData, idx: number) => {
            // only a single page
            const expectedPage = cSharpTestDataExpectedResult[idx][0];
            const id = uuid.v4();
            const ts = performance.now();
            // console.log(id, 'emitsResultsForSimpleValueExpressions');
            const resultSub = output.events
                .filter(msg => msg.id === id)
                .subscribe(msg => {
                    if (msg.name === EventName.ResultDone) {
                        input.destroy(id);
                        resultSub.unsubscribe();
                        if (verifyCount === cSharpTestData.length) {
                            expect(verifyCount).to.equal(cSharpTestData.length, 
                                `verifyCount (${verifyCount}) should match ${''
                                }length of test data: ${cSharpTestData.length}`);
                            verifyCount++; // to avoid running check in other thread
                            runTimings['emitsResultsForSimpleValueExpressions'] =  performance.now() - ts;
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
    }

    function emitsResultsForLinqBasedQueryAgainstSqliteDatabase(done, replayMaxDelay, replayMinDelay) {
        const expectedPage = cSharpCityFilteringQueryEditorTestData[0]; 
        const id = uuid.v4();
        const ts = performance.now();
        // console.log(id, 'emitsResultsForLinqBasedQueryAgainstSqliteDatabase');
        let rowCount = 0;
        let headers: any[] = null;
        let rows: any[] = null;
        let gotHeaders = false;
        let isDone = false;
        const resultSub = output.events
            .filter(msg => msg.id === id)
            .subscribe(msg => {
                if (msg.name === EventName.ResultDone) {
                    input.destroy(id);
                    isDone = true;
                } else if (msg.name === EventName.ResultUpdate) {
                    rows = msg.data.rows;
                    headers = msg.data.columns;
                    gotHeaders = true;
                }
                if (gotHeaders && isDone) {
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
                        runTimings['emitsResultsForLinqBasedQueryAgainstSqliteDatabase'] =  performance.now() - ts;
                    });
                }
            });
        
        replaySteps([
            () => input.new(id, sqliteConnection),
            {
                for: cSharpCityFilteringQueryEditorTestData[0].events,
                fn: (evt) => input.edit(id, evt)
            }, () => input.executeBuffer(id)
        ], replayMaxDelay, replayMinDelay);
    }


    function emitsResultsAfterSwitchingBufferContext(done, replayMaxDelay, replayMinDelay) {
        // is largely a copy of emitsResultsForLinqBasedQueryAgainstSqliteDatabase
        // but has a different setup
        const expectedPage = cSharpCityFilteringQueryEditorTestData[0]; 
        const id = uuid.v4();
        const ts = performance.now();
        // console.log(id, 'emitsResultsAfterSwitchingBufferContext');
        let rowCount = 0;
        let headers: any[] = null;
        let rows: any[] = null;
        let isDone = false;
        let gotHeaders = false;
        const resultSub = output.events
            .filter(msg => msg.id === id)
            .subscribe(msg => {
                if (msg.name === EventName.ResultDone) {
                    input.destroy(id);
                    isDone = true;
                } else if (msg.name === EventName.ResultUpdate) {
                    rows = msg.data.rows;
                    headers = msg.data.columns;
                    gotHeaders = true;
                }
                if (isDone && gotHeaders) {
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
                        runTimings['emitsResultsAfterSwitchingBufferContext'] =  performance.now() - ts;
                    });
                }
            });
        
        replaySteps([
            () => input.new(id),
            // this is what we're really testing, that switching on an empty buffer is ok.
            () => input.setContext(id, sqliteConnection),
            { 
                for: cSharpCityFilteringQueryEditorTestData[0].events,
                fn: (evt) => input.edit(id, evt)
            }, () => input.executeBuffer(id)
        ], replayMaxDelay, replayMinDelay);
    }

    function emitsCodecheckForSimpleStatement(done, replayMaxDelay, replayMinDelay) {
        const id = uuid.v4();
        const ts = performance.now();
        // console.log(id, 'emitsCodecheckForSimpleStatement');
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
                input.destroy(id);
                codecheckSub.unsubscribe();
                runTimings['emitsCodecheckForSimpleStatement'] =  performance.now() - ts;
                // console.log('emitsCodecheckForSimpleStatement', runTimings['emitsCodecheckForSimpleStatement']);
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
    }

    function emitsCodecheckForDatabaseQuery(done, replayMaxDelay, replayMinDelay) {
        const id = uuid.v4();
        const ts = performance.now();
        // console.log(id, 'emitsCodecheckForDatabaseQuery');
        const firstEdits = cSharpDatabaseCodeCheckEditorTestData[0].events.filter(x => x.time < 6000);
        const secondEdits = cSharpDatabaseCodeCheckEditorTestData[0].events.filter(x => x.time >= 6000);
        let codechecks = 0;
        let isFirstCheck = true;
        const codecheckSub = output.events.filter(msg => msg.name === EventName.OmniSharpCodeCheck && msg.id === id).subscribe(msg => {
            const expectedCheck = cSharpDatabaseCodeCheckExpectedErrors[codechecks];
            if (isFirstCheck) {
                isFirstCheck = false;
                check([done, codecheckSub], () => {
                    expect(msg.data.length).to.equal(1, 'first codecheck on db context should be one');
                    expect(msg.data[0].line).to.equal(expectedCheck.line, 'line');
                    expect(msg.data[0].column).to.equal(expectedCheck.column, 'column');
                    expect(msg.data[0].endLine).to.equal(expectedCheck.endLine, 'endLine');
                    expect(msg.data[0].endColumn).to.equal(expectedCheck.endColumn, 'endColumn');
                    expect(msg.data[0].logLevel).to.equal(expectedCheck.logLevel, 'logLevel');
                    expect(msg.data[0].text).to.equal(expectedCheck.text, 'text');
                });
            } else {
                input.destroy(id);
                codecheckSub.unsubscribe();
                checkAndExit(done, () => {
                    expect(msg.data.length).to.equal(0, 'Second codecheck on db context should have zero errors');
                    runTimings['emitsCodecheckForDatabaseQuery'] =  performance.now() - ts;
                    // console.log('emitsCodecheckForDatabaseQuery', runTimings['emitsCodecheckForDatabaseQuery']);
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
    }

    function emitsAutocompletionForSimpleStatement(done, replayMaxDelay, replayMinDelay) {
        const ts = performance.now();
        const id = uuid.v4();
        // console.log(id, 'emitsAutocompletionForSimpleStatement');
        const completionSub = output.events.filter(msg => msg.name === EventName.OmniSharpAutocompletion).subscribe(msg => {
            input.destroy(id);
            completionSub.unsubscribe();
            const items = msg.data.map(x => x.CompletionText);
            Assert(cSharpAutocompletionExpectedValues[0].length > 0, 'Found no completion items');
            cSharpAutocompletionExpectedValues[0].forEach(expectedEntry => {
                expect(items).to.contain(expectedEntry, `Expected completion item "${expectedEntry}"`);
            });
            runTimings['emitsAutocompletionForSimpleStatement'] =  performance.now() - ts;
            // console.log('emitsAutocompletionForSimpleStatement', runTimings['emitsAutocompletionForSimpleStatement']);
            done();
        });
        replaySteps([
            () => input.new(id),
            {
                for: cSharpAutocompletionEditorTestData[0].events,
                fn: (evt) => input.edit(id, evt)
            },
            () => input.autoComplete(id, cSharpAutocompletionRequestTestData[0])
        ], replayMaxDelay, replayMinDelay);
    }

    function emitsCodecheckAfterSwitchingBufferContext(done, replayMaxDelay, replayMinDelay) {
        sqliteConnection.id = 42;
        const ts = performance.now();
        const id = uuid.v4();
        // console.log(id, 'emitsCodecheckAfterSwitchingBufferContext');
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
                        expect(msg.data.length).to.equal(1, 'First code check should have one item');
                        expect(msg.data[0].text).to.equal(expectedCheck.text);
                        expect(msg.data[0].logLevel).to.equal(expectedCheck.logLevel);
                    });
                } else {
                    input.destroy(id);
                    codecheckSub.unsubscribe();
                    checkAndExit(done, () => {
                        expect(msg.data.length).to.equal(0, 'After switching ctx, no errors should appear');
                        runTimings['emitsCodecheckAfterSwitchingBufferContext'] =  performance.now() - ts;
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
    }


    function emitsExpectedDiagnosticsForInvalidQueryWhenCallingQueryBackend(done, replayMaxDelay, replayMinDelay) {
        const ts = performance.now();
        const id = uuid.v4();
        let executeTs: number = null;
        const responseSub = output.events.filter(msg => msg.name === EventName.QueryExecuteResponse).subscribe(msg => {
            input.destroy(id);
            responseSub.unsubscribe();
            checkAndExit(done, () => {
                expect(msg.data.diagnostics.length).to.be.equal(1, 'Expect single error in query');
                expect(msg.data.diagnostics[0].Message).to.contain("The name 'y' does not exist");
                expect(executeTs).to.not.be.null;
                expect(msg.originalTimestamp).to.equal(executeTs, 'Response should pass execute timestamp');
            });
            runTimings['emitsExpectedDiagnosticsForInvalidQueryWhenCallingQueryBackend'] =  performance.now() - ts;
        });
        replaySteps([
            () => input.new(id),
            {
                for: randomTestData[1].events,
                fn: (evt) => input.edit(id, evt)
            },
            () => executeTs = input.executeBuffer(id)
        ], replayMaxDelay, replayMinDelay);
    }

    describe('shutdowns', () => {
        it('can close processes when shutting down', function(done) {
            this.timeout(backendTimeout);
            streamManager.close().then(() => done());
        });
        it('stops query process when closed', function(done) {
            this.timeout(backendTimeout);
            let url = `http://localhost:${config.queryEnginePort}/checkreadystate`;
            http.get(url, res => { done(new Error('response received')); })
                .on('error', () => { done(); });
        });
        it('stops omnisharp process when closed', function(done) {
            this.timeout(backendTimeout);
            let url = `http://localhost:${config.omnisharpPort}/checkreadystate`;
            http.get(url, res => { done(new Error('response received')); })
                .on('error', () => { done(); });
        });
    });
});
