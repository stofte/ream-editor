import * as chai from 'chai';
import { expect } from 'chai';
import { OmnisharpSessionMessage } from './api';
import { OmnisharpSynchronizer } from './omnisharp-synchronizer';
import { cSharpAutocompletionRequestTestData } from '../test/editor-testdata';
import { Observable, Subject } from 'rxjs/Rx';
import replaySteps from '../test/replay-steps';
import * as uuid from 'node-uuid';

describe('omnisharp-synchronizer', function() {
    let sync: OmnisharpSynchronizer = null;
    
    before(function() {
        chai.expect();
        sync = new OmnisharpSynchronizer();
    });

    [
        [0, 0, 2]
    ].forEach(([replayMinDelay, replayMaxDelay, stepCount]) => {
        describe(`input delay: ${replayMinDelay} -> ${replayMaxDelay} ms, repeat: ${stepCount}`, () => {
            let idx = 0;
            const l = [];
            l[stepCount - 1] = 1;
            for(let i = 0; i < l.length; i++) {
                l[i] = i;
            }
            l.forEach(() => {
                it('runs flows multiple times', function(done) {
                    let run1Resolver = null;
                    let run2Resolver = null;
                    let run3Resolver = null;
                    const run1Promise = new Promise((done) => run1Resolver = done);
                    const run2Promise = new Promise((done) => run2Resolver = done);
                    const run3Promise = new Promise((done) => run3Resolver = done);

                    omnisharpSynchronizerSuite(replayMinDelay, replayMaxDelay, sync, run1Resolver);
                    omnisharpSynchronizerSuite(replayMinDelay, replayMaxDelay, sync, run2Resolver);
                    omnisharpSynchronizerSuite(replayMinDelay, replayMaxDelay, sync, run3Resolver);

                    const failures = [];
                    // todo do something nicer
                    const doneHandler = (val) => {
                        if (val) {
                            failures.push(val);
                        }
                    }
                    run1Promise.then(doneHandler);
                    run2Promise.then(doneHandler);
                    run3Promise.then(doneHandler);
                    run1Promise.then(() => 
                        run2Promise.then(() =>  {
                            run3Promise.then(() =>  {
                                setTimeout(() => {
                                    if (failures.length > 0) {
                                        done(failures);
                                    } else {
                                        done();
                                    }
                                }, 100);
                            })
                        })
                    );
                });
            });
        });
    });
});

function omnisharpSynchronizerSuite(minDelayMs: number, maxDelayMs: number, sync: OmnisharpSynchronizer, done: Function) {
    const testMsgs: any[] = [];
    const sessionId = uuid.v4();
    const subject = new Subject<OmnisharpSessionMessage>();
    let subjectCount = 0;
    let stepResolver = null;
    const stepsDone = new Promise((x) => stepResolver = x);
    const auto1 = JSON.parse(JSON.stringify(cSharpAutocompletionRequestTestData[0]));
    auto1.line = 10;
    const auto2 = JSON.parse(JSON.stringify(cSharpAutocompletionRequestTestData[0]));
    auto2.line = 20;
    const auto3 = JSON.parse(JSON.stringify(cSharpAutocompletionRequestTestData[0]));
    auto3.line = 30;
    const stepsToReplay = [
        () => subject.next(<OmnisharpSessionMessage> {
            sessionId, timestamp: performance.now(), type: 'context' }),
        () => subject.next(<OmnisharpSessionMessage> {
            sessionId, timestamp: performance.now(), type: 'edit', edits: [] }),
        // the template arrives a bit later
        () => subject.next(<OmnisharpSessionMessage> {
            sessionId, timestamp: performance.now(), type: 'buffer-template', fileName: 'foo', lineOffset: 0 }),
        () => subject.next(<OmnisharpSessionMessage> {
            sessionId, timestamp: performance.now(), type: 'edit', edits: [] }),
        // these should be executed without blocking each other
        () => subject.next(<OmnisharpSessionMessage> {
            sessionId, timestamp: performance.now(), type: 'codecheck' }),
/* 5 */ () => subject.next(<OmnisharpSessionMessage> {
            sessionId, timestamp: performance.now(), type: 'autocompletion', autocompletion: auto1 }),
        // these depend on each other sequentially
/* 6 */ () => subject.next(<OmnisharpSessionMessage> {
            sessionId, timestamp: performance.now(), type: 'edit', edits: [] }),
        () => subject.next(<OmnisharpSessionMessage> {
            sessionId, timestamp: performance.now(), type: 'codecheck' }),
        () => subject.next(<OmnisharpSessionMessage> {
            sessionId, timestamp: performance.now(), type: 'edit', edits: [] }),
        () => subject.next(<OmnisharpSessionMessage> {
            sessionId, timestamp: performance.now(), type: 'autocompletion', autocompletion: auto2 }),
        // switch context
/* 10 */() => subject.next(<OmnisharpSessionMessage> {
            sessionId, timestamp: performance.now(), type: 'context' }),
        () => subject.next(<OmnisharpSessionMessage> {
            sessionId, timestamp: performance.now(), type: 'edit', edits: [] }),
        () => subject.next(<OmnisharpSessionMessage> {
            sessionId, timestamp: performance.now(), type: 'edit', edits: [] }),
        () => subject.next(<OmnisharpSessionMessage> {
            sessionId, timestamp: performance.now(), type: 'codecheck' }),
        () => subject.next(<OmnisharpSessionMessage> {
            sessionId, timestamp: performance.now(), type: 'autocompletion', autocompletion: auto3 }),
        () => subject.next(<OmnisharpSessionMessage> {
            sessionId, timestamp: performance.now(), type: 'buffer-template', fileName: 'bar', lineOffset: 1 }),
        // switch context
/* 16 */() => subject.next(<OmnisharpSessionMessage> {
            sessionId, timestamp: performance.now(), type: 'context' }),
        () => subject.next(<OmnisharpSessionMessage> {
            sessionId, timestamp: performance.now(), type: 'codecheck' }),
        () => subject.next(<OmnisharpSessionMessage> {
            sessionId, timestamp: performance.now(), type: 'buffer-template', fileName: 'baz', lineOffset: 2 }),
        () => subject.next(<OmnisharpSessionMessage> {
            sessionId, timestamp: performance.now(), type: 'edit', edits: [] })
    ];
    // setup stream
    let idx = 0;
    subject 
        .delayWhen(msg => Observable.fromPromise(sync.queueOperation(msg)))
        .map(msg => sync.mapMessage(msg))
        .subscribe(msg => {
            // context msgs should not be delayed, since it doesnt go to omnisharp
            const timeout = msg.type === 'context' ? 0 : (Math.random() * maxDelayMs) + minDelayMs;
            const autoline = msg.type === 'autocompletion' ? msg.autocompletion.line : -1;
            testMsgs.push({ timestamp: msg.timestamp, resolvedTimestamp: performance.now(),
                type: msg.type, fileName: msg.fileName, lineOffset: msg.lineOffset, autoline });
            subjectCount++;
            setTimeout(() => {
                sync.resolveOperation(msg);
                // seems more stable if we add a slight delay here?
                if (subjectCount >= stepsToReplay.length) {
                    setTimeout(() => stepResolver(), 100);
                }
            }, timeout);
        });
    // start to inject events
    replaySteps(stepsToReplay, maxDelayMs, minDelayMs);
    // check results
    stepsDone.then(() => {
        try {
            const events = testMsgs.map(x => x.type);
            testMsgs.forEach((x, i) => {
                if (i > 1) {
                    expect(x.resolvedTimestamp).to.be.greaterThan(testMsgs[i - 1].resolvedTimestamp);
                }
            });
            expect(testMsgs.length).to.equal(stepsToReplay.length);
            expect(events.slice(0, 4)).to.deep.equal(['context', 'buffer-template', 'edit', 'edit'], 'Event order, idx 0-3');
            // might be interleaved
            expect(events.slice(4, 6).sort()).to.deep.equal(['autocompletion', 'codecheck'], 'Idx 4-5');
            expect(events.slice(6, 10)).to.deep.equal(['edit', 'codecheck', 'edit', 'autocompletion'], 'Event order, idx 6-9');
            expect(events.slice(10, 14)).to.deep.equal(['context', 'buffer-template', 'edit', 'edit'], 'Event order, idx 10-13');
            // might be interleaved
            expect(events.slice(14, 16).sort()).to.deep.equal(['autocompletion', 'codecheck'], 'Event order, idx 14-15');
            expect(events.slice(16, 20)).to.deep.equal(['context', 'buffer-template', 'codecheck', 'edit'], 'Event order, idx 16-19');

            const dependents = testMsgs.filter(x => x.type === 'edit' || x.type === 'codecheck');
            const completions = testMsgs.filter(x => x.type === 'autocompletion').map(x => x.autoline);
            expect(completions).to.deep.equal([11, 21, 32], 'Correct lineOffset on autocompletions');

            const fileNames = dependents.map(x => x.fileName);
            expect(fileNames.slice(0, 6).join('')).to.match(/(foo){6}/, 'Filename, idx 0-5');
            expect(fileNames.slice(6, 9).join('')).to.match(/(bar){3}/, 'Filename, idx 6-9');
            expect(fileNames.slice(9, 11).join('')).to.match(/(baz){2}/, 'Filename, idx 9-11');
            const initTimestamps = dependents.map(x => x.timestamp);
            const doneTimestamps = dependents.map(x => x.resolvedTimestamp);
            // check that dependents were not reordered
            initTimestamps.forEach((x, i) => {
                if (i > 0) {
                    expect(x).to.be.greaterThan(initTimestamps[i - 1], `Init timestamp idx ${i}`);
                    expect(doneTimestamps[i]).to.be.greaterThan(doneTimestamps[i - 1], `Done timestamp idx ${i}`);
                }
            });
            done();
        } catch (e) {
            done(e);
        }
    });
}
