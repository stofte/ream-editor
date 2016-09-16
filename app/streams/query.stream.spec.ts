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

//     it('does stuff', function(done) {
//         this.timeout(backendTimeout);
//         const text = `
//                     var x = 10;
//                     x + 1
// `;
//         setTimeout(function() {
//             queryStream
//                 .executeCode({ id: uuid.v4(), text })
//                 .subscribe(res => {
//                     console.log(`HTTP RESPONSE: ${res}`);
//                 });
//             setTimeout(() => {
//                 done();
//             }, 3000); // let the query execute a bit
//         }, 2000); // startup time
//     });

    it('stops dotnet process when stopServer is called', function(done) {
        this.timeout(backendTimeout);
        queryStream.events.subscribe(msg => {
            if (msg.type === 'closed') {
                setTimeout(() => {
                    let url = `http://localhost:${config.queryEnginePort}/checkreadystate`;
                    http.get(url, res => { done(new Error('response received')); })
                        .on('error', () => { done(); });
                }, 1000);
            }
        });
        setTimeout(function() {
            queryStream.stopServer();
        }, 2000);
    });
});
