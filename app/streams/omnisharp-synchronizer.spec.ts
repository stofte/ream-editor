import * as chai from 'chai';
import { expect } from 'chai';
import * as sinon from 'sinon';
import * as sinonChai from 'sinon-chai';
import { OmnisharpSessionMessage } from '../messages/index';
import { OmnisharpSynchronizer } from './omnisharp-synchronizer';
import { Observable, Subject } from 'rxjs/Rx';
import replaySteps from '../test/replay-steps';
import * as uuid from 'node-uuid';

describe('omnisharp-synchronizer', function() {
    let sync: OmnisharpSynchronizer = null;
    
    before(function() {
        chai.expect();
        chai.use(sinonChai);
    });

    beforeEach(function() {
        sync = new OmnisharpSynchronizer();
    });
    
    it('resolves events in expected order', function(done) {
        this.timeout(20000);
        const timestamps: any[] = [];
        const sessionId = uuid.v4();
        const subject = new Subject<OmnisharpSessionMessage>()
        let subjectCount = 0;
        let stepResolver = null; 
        const syntheticDelayMs = 250;
        const stepsDone = new Promise((done) => stepResolver = done);
        const stepsToReplay = [
            () => subject.next(<OmnisharpSessionMessage> { sessionId, timestamp: performance.now(), type: 'context' }),
            () => subject.next(<OmnisharpSessionMessage> { sessionId, timestamp: performance.now(), type: 'edit' }),
            // the template arrives a bit later
            () => subject.next(<OmnisharpSessionMessage> { sessionId, timestamp: performance.now(), type: 'buffer-template' }),
            () => subject.next(<OmnisharpSessionMessage> { sessionId, timestamp: performance.now(), type: 'edit' }),
            // these should be executed without blocking each other
            () => subject.next(<OmnisharpSessionMessage> { sessionId, timestamp: performance.now(), type: 'codecheck' }),
/* idx 5 */ () => subject.next(<OmnisharpSessionMessage> { sessionId, timestamp: performance.now(), type: 'autocompletion' }),
            // these depend on each other sequentially
/* idx 6 */ () => subject.next(<OmnisharpSessionMessage> { sessionId, timestamp: performance.now(), type: 'edit' }),
            () => subject.next(<OmnisharpSessionMessage> { sessionId, timestamp: performance.now(), type: 'codecheck' }),
            () => subject.next(<OmnisharpSessionMessage> { sessionId, timestamp: performance.now(), type: 'edit' }),
            () => subject.next(<OmnisharpSessionMessage> { sessionId, timestamp: performance.now(), type: 'autocompletion' }),
            // switch context
/* idx 10 */() => subject.next(<OmnisharpSessionMessage> { sessionId, timestamp: performance.now(), type: 'context' }),
            () => subject.next(<OmnisharpSessionMessage> { sessionId, timestamp: performance.now(), type: 'edit' }),
            () => subject.next(<OmnisharpSessionMessage> { sessionId, timestamp: performance.now(), type: 'edit' }),
            () => subject.next(<OmnisharpSessionMessage> { sessionId, timestamp: performance.now(), type: 'codecheck' }),
            () => subject.next(<OmnisharpSessionMessage> { sessionId, timestamp: performance.now(), type: 'autocompletion' }),
            () => subject.next(<OmnisharpSessionMessage> { sessionId, timestamp: performance.now(), type: 'buffer-template' }),
        ];
        // setup stream
        let idx = 0;
        subject 
            .delayWhen(msg => Observable.fromPromise(sync.queueOperation(msg)))
            .delay(syntheticDelayMs)
            .subscribe(msg => {
                // console.log('\n\n' + (idx++) + ' ' + msg.type.substring(0, 5) + '\t' + msg.timestamp);
                subjectCount++;
                sync.resolveOperation(msg);
                timestamps.push({ timestamp: msg.timestamp, resolvedTimestamp: performance.now(), type: msg.type });
                if (subjectCount >= stepsToReplay.length) stepResolver();
            });
        // start to inject events
        replaySteps(stepsToReplay);
        // check results
        stepsDone.then(() => {
            try {
                const eventOrder = timestamps.map(x => x.type).join(',');
                const r = '(codecheck|autocompletion)';
                const firstOrder = `^context,buffer-template,edit,edit,${r},${r},edit,codecheck,edit,autocompletion`;
                const secondOrder = `${firstOrder},context,buffer-template,edit,edit,${r},${r}$`;
                const expectedOrder = new RegExp(secondOrder);
                expect(timestamps.length).to.equal(stepsToReplay.length);
                // console.log('\n\n' + timestamps.map(x => x.type.substring(0, 7) + '\t' + x.resolvedTimestamp).join('\n'));
                expect(eventOrder).to.match(expectedOrder);
                timestamps.forEach((ts, idx) => {
                    if (idx === 5) {
                        // these two events should be executed without blocking as the operations depend on the same buffer
                        expect(ts.resolvedTimestamp - timestamps[idx - 1].resolvedTimestamp).to.be.lessThan(syntheticDelayMs, `Idx ${idx}`);
                    } else if (idx > 5 && idx < 10) {
                        // operations depend on the previous, so should be greater then the synthetic lag
                        expect(ts.resolvedTimestamp - timestamps[idx - 1].resolvedTimestamp).to.be.greaterThan(syntheticDelayMs, `Idx ${idx}`);
                    }
                });
                done();
            } catch (e) {
                done(e);
            }
        });
    });
});
