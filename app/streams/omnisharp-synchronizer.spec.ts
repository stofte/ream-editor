import * as chai from 'chai';
import { expect } from 'chai';
import { OmnisharpSessionMessage } from './api';
import { OmnisharpSynchronizer } from './omnisharp-synchronizer';
import { Observable, Subject } from 'rxjs/Rx';
import replaySteps from '../test/replay-steps';
import * as uuid from 'node-uuid';

describe('omnisharp-synchronizer', function() {
    let sync: OmnisharpSynchronizer = null;
    
    before(function() {
        chai.expect();
    });

    beforeEach(function() {
        sync = new OmnisharpSynchronizer();
    });
    // first number in list is min delay
    [
        [  10, 1, 1, 2]
        // these steps can take upward of an hour due to randomization in test
        // [  0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20],
        // [  0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20],
        // [  0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20],
        // [  0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20],
        // [  0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20],
        // [  0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20],
        // [100, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20],
        // [100, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20],
        // [100, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20],
        // [100, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20],
        // [100, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20],
        // [200, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20],
        // [200, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20],
        // [200, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20],
        // [200, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20],
        // [200, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20],
        // [250, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20],
        // [250, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20],
        // [250, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20],
        // [250, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20],
        // [250, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20],
        // [500, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20],
        // [500, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20],
        // [500, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20],
        // [500, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20],
        // [500, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20]
    ].forEach((runModifiers, idx) => {
        const minDelayMs = runModifiers[0];
        runModifiers.slice(1).forEach((modifier: number, idx2) => {
            const maxDelayMs = 50 * modifier;
            it(`resolves events in expected order (run ${idx < 10 ? '0': ''}${idx}, step ${idx2 < 10 ? '0': ''}${idx2}, delay ${minDelayMs}ms ~ ${maxDelayMs}ms)`, function(done) {
                omnisharpSynchronizerSuite.bind(this)(minDelayMs, maxDelayMs, sync, done);
            });
        })
    });
});

function omnisharpSynchronizerSuite(minDelayMs: number, maxDelayMs: number, sync: OmnisharpSynchronizer, done: Function) {
    this.timeout(((minDelayMs + maxDelayMs) * 2 * 20) + 5000);
    const timestamps: any[] = [];
    const sessionId = uuid.v4();
    const subject = new Subject<OmnisharpSessionMessage>()
    let subjectCount = 0;
    let stepResolver = null;
    const stepsDone = new Promise((done) => stepResolver = done);
    const stepsToReplay = [
        () => subject.next(<OmnisharpSessionMessage> { sessionId, timestamp: performance.now(), type: 'context' }),
        () => subject.next(<OmnisharpSessionMessage> { sessionId, timestamp: performance.now(), type: 'edit' }),
        // the template arrives a bit later
        () => subject.next(<OmnisharpSessionMessage> { sessionId, timestamp: performance.now(), type: 'buffer-template' }),
        () => subject.next(<OmnisharpSessionMessage> { sessionId, timestamp: performance.now(), type: 'edit' }),
        // these should be executed without blocking each other
        () => subject.next(<OmnisharpSessionMessage> { sessionId, timestamp: performance.now(), type: 'codecheck' }),
/* 5 */ () => subject.next(<OmnisharpSessionMessage> { sessionId, timestamp: performance.now(), type: 'autocompletion' }),
        // these depend on each other sequentially
/* 6 */ () => subject.next(<OmnisharpSessionMessage> { sessionId, timestamp: performance.now(), type: 'edit' }),
        () => subject.next(<OmnisharpSessionMessage> { sessionId, timestamp: performance.now(), type: 'codecheck' }),
        () => subject.next(<OmnisharpSessionMessage> { sessionId, timestamp: performance.now(), type: 'edit' }),
        () => subject.next(<OmnisharpSessionMessage> { sessionId, timestamp: performance.now(), type: 'autocompletion' }),
        // switch context
/* 10 */() => subject.next(<OmnisharpSessionMessage> { sessionId, timestamp: performance.now(), type: 'context' }),
        () => subject.next(<OmnisharpSessionMessage> { sessionId, timestamp: performance.now(), type: 'edit' }),
        () => subject.next(<OmnisharpSessionMessage> { sessionId, timestamp: performance.now(), type: 'edit' }),
        () => subject.next(<OmnisharpSessionMessage> { sessionId, timestamp: performance.now(), type: 'codecheck' }),
        () => subject.next(<OmnisharpSessionMessage> { sessionId, timestamp: performance.now(), type: 'autocompletion' }),
        () => subject.next(<OmnisharpSessionMessage> { sessionId, timestamp: performance.now(), type: 'buffer-template' }),
        // switch context
/* 16 */() => subject.next(<OmnisharpSessionMessage> { sessionId, timestamp: performance.now(), type: 'context' }),
        () => subject.next(<OmnisharpSessionMessage> { sessionId, timestamp: performance.now(), type: 'codecheck' }),
        () => subject.next(<OmnisharpSessionMessage> { sessionId, timestamp: performance.now(), type: 'buffer-template' }),
        () => subject.next(<OmnisharpSessionMessage> { sessionId, timestamp: performance.now(), type: 'edit' })
    ];
    // setup stream
    let idx = 0;
    subject 
        .delayWhen(msg => Observable.fromPromise(sync.queueOperation(msg)))
        .subscribe(msg => {
            // context msgs should not be delayed, since it doesnt go to omnisharp
            const timeout = msg.type === 'context' ? 0 : (Math.random() * maxDelayMs) + minDelayMs;
            timestamps.push({ timestamp: msg.timestamp, resolvedTimestamp: performance.now(), type: msg.type });
            subjectCount++;
            setTimeout(() => {
                sync.resolveOperation(msg);
                // seems more stable if we add a slight delay here?
                if (subjectCount >= stepsToReplay.length) setTimeout(() => stepResolver(), 100);
            }, timeout);
        });
    // start to inject events
    replaySteps(stepsToReplay, maxDelayMs, minDelayMs);
    // check results
    stepsDone.then(() => {
        try {
            const events = timestamps.map(x => x.type);
            timestamps.forEach((x, idx) => {
                if (idx > 1) {
                    expect(x.resolvedTimestamp).to.be.greaterThan(timestamps[idx - 1].resolvedTimestamp);
                }
            })
            expect(timestamps.length).to.equal(stepsToReplay.length);
            expect(events.slice(0, 4)).to.deep.equal(['context', 'buffer-template', 'edit', 'edit'], 'Idx 0-3');
            expect(events.slice(4, 6).sort()).to.deep.equal(['autocompletion','codecheck'], 'Idx 4-5'); // might be interleaved
            expect(events.slice(6, 10)).to.deep.equal(['edit', 'codecheck', 'edit', 'autocompletion'], 'Idx 6-9');
            expect(events.slice(10, 14)).to.deep.equal(['context', 'buffer-template', 'edit', 'edit'], 'Idx 10-13');
            expect(events.slice(14, 16).sort()).to.deep.equal(['autocompletion','codecheck'], 'Idx 14-15'); // might be interleaved
            expect(events.slice(16, 20)).to.deep.equal(['context','buffer-template', 'codecheck', 'edit'], 'Idx 16-19');

            const dependents = timestamps.filter(x => x.type === 'edit' || x.type === 'codecheck');
            const initTimestamps = dependents.map(x => x.timestamp);
            const doneTimestamps = dependents.map(x => x.resolvedTimestamp);
            // check that dependents were not reordered
            initTimestamps.forEach((x, idx) => {
                if (idx > 0) {
                    expect(x).to.be.greaterThan(initTimestamps[idx - 1], `Init timestamp idx ${idx}`);
                    expect(doneTimestamps[idx]).to.be.greaterThan(doneTimestamps[idx - 1], `Done timestamp idx ${idx}`);
                }
            });
            done();
        } catch (e) {
            // console.log('BOOOOOOOOOOOOOOOOOOOOOOOOOOOM')
            // console.log(e)
            // process.exit();
            done(e);
        }
    });
}
