// rx streams require events to be replayed over time to make any sense.
export function replaySteps(steps: any[], timeoutMax = 100, timeoutMin = 10) {
    if (steps.length > 0) {
        let head = steps[0];
        let headArg = undefined;
        let remaining = steps.slice(1);
        let timeout = Math.random() * timeoutMax + timeoutMin;
        let isPromiseStep = false;
        if (typeof head === 'number') {
            timeout = head;
            head = remaining[0];
            remaining = remaining.slice(1);
        } else if (head.constructor && head.constructor.name === WaitUntil.name) {
            isPromiseStep = true;
        } else if (typeof head !== 'function') {
            let forObj = head;
            timeout = forObj.wait || timeout;
            if (forObj.for.length > 1) {
                remaining = [{
                    for: forObj.for.slice(1),
                    wait: forObj.wait,
                    fn: forObj.fn
                }].concat(remaining);
            }
            head = forObj.fn;
            headArg = forObj.for[0];
        }
        if (!isPromiseStep) {
            setTimeout(() => {
                head(headArg);
                replaySteps(remaining, timeoutMax, timeoutMin);
            }, timeout);
        } else {
            head.then(() => {
                replaySteps(remaining, timeoutMax, timeoutMin);
            })
        }
    }
    return;
}

// Since "expect" throws inside a subscription handler, the stream crashes as a result.
// These helper functions aid in avoid crashing the suite
export function check([done, subber], pred) {
    try {
        pred();
    } catch (exn) {
        subber.unsubscribe();
        done(exn);
    }
}

export function checkAndExit(done, pred) {
    try {
        pred();
        done();
    } catch (exn) {
        done(exn);
    }
}

export function WaitUntil(gen: () => Promise<boolean>) {
    this.then = function(cb) {
        gen().then(cb);
    };
}
