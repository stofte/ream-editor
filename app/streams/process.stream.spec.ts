import * as chai from 'chai';
import { expect } from 'chai';
import * as sinon from 'sinon';
import * as sinonChai from 'sinon-chai';
import { ReflectiveInjector, enableProdMode } from '@angular/core';
import { HTTP_PROVIDERS } from '@angular/http';
import { Observable } from 'rxjs/Rx';
import { ProcessStream } from './process.stream';
import { ProcessHelper } from '../utils/process-helper';
import config from '../config';

describe('process.stream int-test', function() {
    this.timeout(30 * 1000);

    let injector: ReflectiveInjector;
    let instance: ProcessStream = null;
    let processHelper = new ProcessHelper();
    
    before(function() {
        chai.expect();
        chai.use(sinonChai);
    });

    beforeEach(() => {
        injector = ReflectiveInjector.resolveAndCreate([
            HTTP_PROVIDERS,
            ProcessStream
        ]);
        instance = injector.get(ProcessStream);
    });

    function lifecycleTest(name, done) {
        let cmd = processHelper[name](2000);
        let receivedStarting = false;
        let types = ['starting', 'ready', 'closing', 'closed'];
        let idx = 0;
        let sub = instance.status.subscribe(msg => {
            expect(msg.type).to.equal(types[idx++]);

            if (msg.type === 'ready') {
                instance.close();
            }
            if (msg.type === 'closed' || msg.type === 'failed') {
                sub.unsubscribe();
            }
        }).add(done);
        instance.start(cmd.command, cmd.directory, 2000);
    }

    it('emits expected application lifecycle events for omnisharp', function(done) {
        this.timeout(15 * 1000);
        lifecycleTest('omnisharp', done);
    });

    it('emits expected application lifecycle events for query', function(done) {
        this.timeout(15 * 1000);
        lifecycleTest('query', done);
    });
});
