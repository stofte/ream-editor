import * as chai from 'chai';
import { expect } from 'chai';
import * as sinon from 'sinon';
import * as sinonChai from 'sinon-chai';
import { ReflectiveInjector, enableProdMode } from '@angular/core';
import { Http, XHRBackend, ConnectionBackend, BrowserXhr, ResponseOptions, 
    BaseResponseOptions, RequestOptions, BaseRequestOptions } from '@angular/http';
import { Observable } from 'rxjs/Rx';
import { QueryMessage, OmnisharpMessage } from '../messages/index';
import { QueryStream, SessionStream, EditorStream, ResultStream, OmnisharpStream } from './index';
import config from '../config';
import { CodeCheckResult, AutocompletionQuery, Connection } from '../models';
import XSRFStrategyMock from '../test/xsrf-strategy-mock';
import { cSharpTestData, cSharpTestDataExpectedResult, cSharpTestDataExpectedCodeChecks,
    codecheckEditorTestData, cSharpAutocompletionEditorTestData, cSharpAutocompletionRequestTestData,
    cSharpAutocompletionExpectedValues, cSharpContextSwitchExpectedCodeChecks, 
    cSharpContextSwitchEditorTestData, cSharpCityFilteringQueryEditorTestData } from '../test/editor-testdata';
import replaySteps from '../test/replay-steps';
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
        query.once(msg => msg.type === 'ready', () => {
            queryReady = true;
            if (omnisharpReady) {
                done();
            }
        });
        omnisharp.once(msg => msg.type === 'ready', () => {
            omnisharpReady = true;
            if (queryReady) {
                done();
            }
        });
    });

    it('emits result messages for simple value expressions', function(done) {
        this.timeout(backendTimeout * (cSharpTestData.length + 1));
        let verifyCount = 0;
        cSharpTestData.forEach((testData, idx: number) => {
            // only a single page
            const expectedPage = cSharpTestDataExpectedResult[idx][0]; 
            const id = uuid.v4();
            const resultSub = result.events
                .filter(msg => msg.id === id)
                .subscribe(msg => {
                    if (msg.type === 'done') {
                        resultSub.unsubscribe();
                        if (idx === cSharpTestData.length - 1) {
                            expect(verifyCount).to.equal(cSharpTestData.length, 
                                `verifyCount (${verifyCount}) should match length of test data: ${cSharpTestData.length}`);
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
                100, () => session.new(id),
                { 
                    for: testData.events,
                    wait: 100,
                    fn: (evt) => editor.edit(id, evt)
                },
                500, () => session.run(id)
            ]);
        });
    });

    it('emits results messages for linq based query against sqlite database', function(done) {
        this.timeout(backendTimeout * 3);
        const expectedPage = cSharpCityFilteringQueryEditorTestData[0]; 
        const id = uuid.v4();
        let rowCount = 0;
        let headers: any[] = null;
        let rows: any[] = null;
        const resultSub = result.events
            .filter(msg => msg.id === id)
            .subscribe(msg => {
                if (msg.type === 'done') {
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
                } else if (msg.type === 'update') {
                    rows = msg.data.rows;
                    headers = msg.data.columns;
                }
            });
        
        replaySteps([
            100, () => session.new(id, sqliteConnection),
            { 
                for: cSharpCityFilteringQueryEditorTestData[0].events,
                wait: 100,
                fn: (evt) => editor.edit(id, evt)
            },
            500, () => session.run(id)
        ]);
    });

    it('emits codecheck messages for simple statement', function(done) {
        this.timeout(backendTimeout * 2);
        const id = uuid.v4();
        const firstEdits = codecheckEditorTestData[0].events.filter(x => x.time < 6000);
        const secondEdits = codecheckEditorTestData[0].events.filter(x => x.time >= 6000);
        let codechecks = 0;
        const codecheckSub = omnisharp.events.filter(msg => msg.type === 'codecheck' && msg.sessionId === id).subscribe(msg => {
            const expectedCheck = cSharpTestDataExpectedCodeChecks[codechecks];
            codechecks++;
            expect(msg.checks.length).to.equal(1);
            expect(msg.checks[0].line).to.equal(expectedCheck.line, 'line');
            expect(msg.checks[0].column).to.equal(expectedCheck.column, 'column');
            expect(msg.checks[0].endLine).to.equal(expectedCheck.endLine, 'endLine');
            expect(msg.checks[0].endColumn).to.equal(expectedCheck.endColumn, 'endColumn');
            expect(msg.checks[0].logLevel).to.equal(expectedCheck.logLevel, 'logLevel');
            expect(msg.checks[0].text).to.equal(expectedCheck.text, 'text');
            if (codechecks >= cSharpTestDataExpectedCodeChecks.length) {
                codecheckSub.unsubscribe();
                done();
            }
        });

        // timing sensitive. if we fire the codecheck too early, we might
        // accidentally codecheck an unintended buffer. this leads to 
        // unpredicted errors (eg "Identifier expected" if only the "int " has been processed)
        replaySteps([
            100, () => session.new(id),
            {
                for: firstEdits,
                wait: 100,
                fn: (evt) => editor.edit(id, evt)
            },
            2500, () => session.codeCheck(id),
            // give omnisharp some time since this is a first real op
            1000, () => { },
            {
                for: secondEdits,
                wait: 100,
                fn: (evt) => editor.edit(id, evt)
            },
            2500, () => session.codeCheck(id)
        ]);
    });

    it('emits autocompletion messages for simple statement', function(done) {
        this.timeout(backendTimeout * 2);
        const completionSub = omnisharp.events.filter(msg => msg.type === 'autocompletion').subscribe(msg => {
            const items = msg.completions.map(x => x.CompletionText);
            Assert(cSharpAutocompletionExpectedValues[0].length > 0, 'Found no completion items');
            cSharpAutocompletionExpectedValues[0].forEach(expectedEntry => {
                expect(items).to.contain(expectedEntry, `Expected completion item "${expectedEntry}"`);
            });
            completionSub.unsubscribe();
            done();
        });
        const id = uuid.v4();
        replaySteps([
            100, () => session.new(id),
            {
                for: cSharpAutocompletionEditorTestData[0].events,
                wait: 100,
                fn: (evt) => editor.edit(id, evt)
            },
            100, () => session.autoComplete(id, cSharpAutocompletionRequestTestData[0])
        ]);
    });

    it('emits codecheck messages after switching buffer context', function(done) {
        this.timeout(backendTimeout * 3);
        sqliteConnection.id = 42;
        const id = uuid.v4();
        const firstEdits = cSharpContextSwitchEditorTestData[0].events.filter(x => x.time < 5000);
        const secondEdits = cSharpContextSwitchEditorTestData[0].events.filter(x => x.time >= 5000);
        const expectedCheck = cSharpContextSwitchExpectedCodeChecks[0];
        let codechecks = 0;
        const codecheckSub = omnisharp.events
            .filter(msg => msg.type === 'codecheck' && msg.sessionId === id)
            .subscribe(msg => {
                codechecks++;
                if (codechecks === 1) {
                    console.log('doing first check', msg)
                    expect(msg.checks.length).to.equal(1);
                    expect(msg.checks[0].text).to.equal(expectedCheck.text);
                    expect(msg.checks[0].logLevel).to.equal(expectedCheck.logLevel);
                } else {
                    codecheckSub.unsubscribe();
                    // we should be in the correct context now, and not see any errors
                    console.log('doing second check', msg)
                    // expect(msg.checks.length).to.equal(0);
                    setTimeout(() => done(), 1000);
                }
            });

        // timing sensitive.
        replaySteps([
            100, () => session.new(id),
            {
                for: firstEdits, // yields text "city", which is illegal in code buffer
                wait: 100,
                fn: (evt) => editor.edit(id, evt)
            },
            2500, () => session.codeCheck(id),
            // switch
            100, () => session.setContext(id, sqliteConnection),
            100, () => { },
            {
                for: secondEdits,
                wait: 100,
                fn: (evt) => editor.edit(id, evt)
            },
            2500, () => session.codeCheck(id)
        ]);
    });

    it('stops query process when stopServer is called', function(done) {
        this.timeout(backendTimeout);
        query.once(msg => msg.type === 'closed', () => {
            setTimeout(() => {
                let url = `http://localhost:${config.queryEnginePort}/checkreadystate`;
                http.get(url, res => { done(new Error('response received')); })
                    .on('error', () => { done(); });
            }, 500);
        });
        replaySteps([
            500, () => query.stopServer()
        ]);
    });

    it('stops omnisharp process when stopServer is called', function(done) {
        this.timeout(backendTimeout);
        omnisharp.once(msg => msg.type === 'closed', () => {
            setTimeout(() => {
                let url = `http://localhost:${config.omnisharpPort}/checkreadystate`;
                http.get(url, res => { done(new Error('response received')); })
                    .on('error', () => { done(); });
            }, 500);
        });
        replaySteps([
            500, () => omnisharp.stopServer()
        ]);
    });
});

// Since "expect" throws inside a subscription handler, the stream crashes as a result.
// These helper functions aid in avoid crashing the suite

function check(done, pred) {
    try {
        pred();
    } catch (exn) {
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
