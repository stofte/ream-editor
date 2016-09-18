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
import * as uuid from 'node-uuid';
const http = electronRequire('http');
const backendTimeout = config.unitTestData.backendTimeout;

describe('query.stream int-test', function() {
    this.timeout(backendTimeout * 3);
    let queryStream: QueryStream = null;
    
    before(function() {
        chai.expect();
        chai.use(sinonChai);
        const injector = ReflectiveInjector.resolveAndCreate([
            Http, BrowserXhr, XSRFStrategyMock,
            { provide: ConnectionBackend, useClass: XHRBackend },
            { provide: ResponseOptions, useClass: BaseResponseOptions },
            { provide: RequestOptions, useClass: BaseRequestOptions },
            QueryStream,
            ProcessStream
        ]);
        queryStream = injector.get(QueryStream);
    });

    it('receives expected value when executeCode is called', function(done) {
        this.timeout(backendTimeout);
        let sawValue = false;
        const queryId = uuid.v4();
        const text = `var x = 21;\nx*2`;
        queryStream.once((msg: QueryMessage) => msg.type === 'ready', () => {
            queryStream
                .executeCode({ id: queryId, text })
                .subscribe();
        });
        queryStream.once((msg: QueryMessage) => 
            msg.type === 'message' &&
            msg.data.session === queryId &&
            msg.data.type === 'singleAtomic',
            (msg: QueryMessage) => {
                sawValue = msg.data.values[1] === 42;
            });
        queryStream.once((msg: QueryMessage) =>
            msg.type === 'message' &&
            msg.data.session === queryId &&
            msg.data.type === 'close',
            () => {
                sawValue ? done() : done(new Error('No value received'));
            });
    });

    it('stops dotnet process when stopServer is called', function(done) {
        this.timeout(backendTimeout);
        queryStream.once(msg => msg.type === 'closed', () => {
            setTimeout(() => {
                let url = `http://localhost:${config.queryEnginePort}/checkreadystate`;
                http.get(url, res => { done(new Error('response received')); })
                    .on('error', () => { done(); });
            }, 500);
        });
        setTimeout(() => {
            queryStream.stopServer();
        }, 0);
    });
});
