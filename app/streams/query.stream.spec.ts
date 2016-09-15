import * as chai from 'chai';
import { expect } from 'chai';
import * as sinon from 'sinon';
import * as sinonChai from 'sinon-chai';
import { ReflectiveInjector, enableProdMode } from '@angular/core';
import { HTTP_PROVIDERS } from '@angular/http';
import { Observable } from 'rxjs/Rx';
import { QueryStream } from './query.stream';
import { QueryMessage } from '../messages/index';
import { ProcessStream } from './process.stream';
import config from '../config';

const http = electronRequire('http');
const backendTimeout = config.unitTestData.backendTimeout;

describe('query.stream int-test', function() {
    this.timeout(backendTimeout * 3);
    let injector: ReflectiveInjector;
    let instance: QueryStream = null;
    
    before(function() {
        chai.expect();
        chai.use(sinonChai);
        injector = ReflectiveInjector.resolveAndCreate([
            HTTP_PROVIDERS,
            QueryStream,
            ProcessStream
        ]);
        instance = injector.get(QueryStream);
    });

    it('handles shutdown', function(done) {
        this.timeout(backendTimeout);
        instance.events.subscribe(msg => {
            if (msg.type === 'closed') {
                setTimeout(() => {
                    let url = `http://localhost:${config.queryEnginePort}/checkreadystate`;
                    http.get(url, res => { done(new Error('response received')); })
                        .on('error', () => { done(); });
                }, 1000);
            }
        });
        // some startup time
        setTimeout(function() {
            instance.shutdown();
        }, 2000);
    });
});
