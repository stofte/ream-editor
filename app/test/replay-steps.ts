// rx streams require events to be replayed over time to make any sense.
export default function replaySteps(steps: any[], timeoutMax: number = 100, timeoutMin: number = 10) {
    if (steps.length > 0) {
        let head = steps[0];
        let headArg = undefined;
        let remaining = steps.slice(1);
        let timeout = Math.random() * timeoutMax + timeoutMin;
        if (typeof head === 'number') {
            timeout = head;
            head = remaining[0];
            remaining = remaining.slice(1);
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
        setTimeout(() => {
            head(headArg);
            replaySteps(remaining, timeoutMax, timeoutMin);
        }, timeout);
    }
    return;
}
