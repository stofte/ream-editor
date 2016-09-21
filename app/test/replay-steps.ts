// rx streams require events to be replayed over time to make any sense.
export default function replaySteps(steps: any[]) {
    // console.log('REPLAYSTEPS', steps.length);
    if (steps.length > 0) {
        let head = steps[0];
        let headArg = undefined;
        let remaining = steps.slice(1);
        let timeout = 500;
        if (typeof head === 'number') {
            // console.log('number case')
            timeout = head;
            head = remaining[0];
            remaining = remaining.slice(1);
        } else if (typeof head !== 'function') {
            // console.log('head was object');
            let forObj = head;
            timeout = forObj.wait || timeout;
            if (forObj.for.length > 1) {
                // console.log()
                remaining = [{
                    for: forObj.for.slice(1),
                    wait: forObj.wait,
                    fn: forObj.fn
                }].concat(remaining);
            }
            head = forObj.fn;
            // console.log('headArg assign', forObj.for[0]);
            headArg = forObj.for[0];
        }
        // console.log(head.toString());
        setTimeout(() => {
            head(headArg);
            replaySteps(remaining);
        }, timeout);
    }
    return;
}
