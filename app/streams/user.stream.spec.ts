import * as chai from 'chai';
import { expect } from 'chai';
import * as sinon from 'sinon';
import * as sinonChai from 'sinon-chai';
import { UserStream } from './user.stream';
import { Observable } from 'rxjs/Rx';

describe('user.editor', function() {
    let instance: UserStream = null;
    
    before(function() {
        chai.expect();
        chai.use(sinonChai);
    });

    beforeEach(function() {
        instance = new UserStream();
    });
    
    it('emits no events when initially subscribing', function(done) {
        this.timeout(1000);
        const cb = sinon.spy();
        instance.events.subscribe(cb);
        setTimeout(function() {
            expect(cb).to.not.have.been.called;
            done();
        });
    });
});
