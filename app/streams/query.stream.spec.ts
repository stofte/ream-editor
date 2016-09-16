import * as chai from 'chai';
import { expect } from 'chai';
import * as sinon from 'sinon';
import * as sinonChai from 'sinon-chai';
import { ReflectiveInjector, enableProdMode } from '@angular/core';
import { Http, XHRBackend, ConnectionBackend, BrowserXhr, ResponseOptions, BaseResponseOptions, RequestOptions, BaseRequestOptions } from '@angular/http';
import { Observable } from 'rxjs/Rx';
import { QueryStream } from './query.stream';
import { QueryMessage } from '../messages/index';
import { ProcessStream } from './process.stream';
import config from '../config';
import XSRFStrategyMock from '../test/xsrf-strategy-mock';

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
            Http, BrowserXhr, XSRFStrategyMock,
            { provide: ConnectionBackend, useClass: XHRBackend },
            { provide: ResponseOptions, useClass: BaseResponseOptions },
            { provide: RequestOptions, useClass: BaseRequestOptions },
            QueryStream,
            ProcessStream
        ]);
        instance = injector.get(QueryStream);
    });

    it('stops dotnet process when stopServer is called', function(done) {
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
            instance.stopServer();
        }, 2000);
    });
});
