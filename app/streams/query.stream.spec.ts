import * as chai from 'chai';
import { expect } from 'chai';
import * as sinon from 'sinon';
import * as sinonChai from 'sinon-chai';
import { ReflectiveInjector, enableProdMode } from '@angular/core';
import { HTTP_PROVIDERS } from '@angular/http';
import { Observable } from 'rxjs/Rx';
import { QueryStream } from './query.stream';
import { ProcessStream } from './process.stream';
import config from '../config';

const backendTimeout = config.unitTestData.backendTimeout;
const executeJsTimeout = config.unitTestData.executeJsTimeout;
const http = __nodeHttp;

describe('query.stream int-test', function() {
    this.timeout(30 * 1000);

    let injector: ReflectiveInjector;
    let instance: QueryStream = null;
    
    before(function() {
        chai.expect();
        chai.use(sinonChai);
    });

    beforeEach(() => {
        injector = ReflectiveInjector.resolveAndCreate([
            HTTP_PROVIDERS,
            QueryStream,
            ProcessStream
        ]);
        instance = injector.get(QueryStream);
    });

    describe('when closing', function() {
        before(function() {
            this.timeout(backendTimeout * 1.3);
            instance.shutdown();
            return new Promise((succ, err) => {
                // give it a bit more time before we check services
                setTimeout(succ, backendTimeout);
            });
        });

        // check via http if services are still up and going.
        it('also closes background omnisharp process', function(done) {
            this.timeout(backendTimeout);
            let url = `http://localhost:${config.omnisharpPort}/checkreadystate`;
            http.get(url, res => { done(new Error('response received')); })
                .on('error', () => { done(); });
        });
        
        it('also closes background query process', function(done) {
            this.timeout(backendTimeout);
            let url = `http://localhost:${config.queryEnginePort}/checkreadystate`;
            http.get(url, res => { done(new Error('response received')); })
                .on('error', () => { done(); });
        });

    });
});
