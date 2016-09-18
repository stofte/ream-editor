import * as chai from 'chai';
import { expect } from 'chai';
import * as sinon from 'sinon';
import * as sinonChai from 'sinon-chai';
import { ReflectiveInjector, enableProdMode } from '@angular/core';
import { Http, XHRBackend, ConnectionBackend, BrowserXhr, ResponseOptions, BaseResponseOptions, RequestOptions, BaseRequestOptions } from '@angular/http';
import { Observable } from 'rxjs/Rx';
import { QueryMessage } from '../messages/index';
import { ProcessStream, QueryStream, SessionStream, EditorStream } from './index';
import config from '../config';
import XSRFStrategyMock from '../test/xsrf-strategy-mock';
import { cSharpTestData } from '../test/editor-testdata';
import * as uuid from 'node-uuid';
const http = electronRequire('http');
const backendTimeout = config.unitTestData.backendTimeout;

describe('query.stream int-test', function() {
    this.timeout(backendTimeout * 3);
    let query: QueryStream = null;
    let session: SessionStream = null;
    let editor: EditorStream = null;
    
    before(function() {
        chai.expect();
        chai.use(sinonChai);
        const injector = ReflectiveInjector.resolveAndCreate([
            Http, BrowserXhr, XSRFStrategyMock,
            { provide: ConnectionBackend, useClass: XHRBackend },
            { provide: ResponseOptions, useClass: BaseResponseOptions },
            { provide: RequestOptions, useClass: BaseRequestOptions },
            QueryStream,
            ProcessStream,
            SessionStream,
            EditorStream
        ]);
        session = injector.get(SessionStream);
        editor = injector.get(EditorStream);
        query = injector.get(QueryStream);
    });

    it('receives expected values when runCode is called after feeding legal text', function(done) {
        this.timeout(backendTimeout);
        let sawValue = false;
        const queryId = uuid.v4();
        const text = `var x = 21;\nx*2`;
        query.once((msg: QueryMessage) => msg.type === 'ready', () => {
            query
                .executeCode({ id: queryId, text })
                .subscribe();
        });
        query.once((msg: QueryMessage) => 
            msg.type === 'message' &&
            msg.socket.session === queryId &&
            msg.socket.type === 'singleAtomic',
            (msg: QueryMessage) => {
                sawValue = msg.socket.values[1] === 42;
            });
        query.once((msg: QueryMessage) =>
            msg.type === 'message' &&
            msg.socket.session === queryId &&
            msg.socket.type === 'close',
            () => {
                sawValue ? done() : done(new Error('No value received'));
            });
    });

    it('stops dotnet process when stopServer is called', function(done) {
        this.timeout(backendTimeout);
        query.once(msg => msg.type === 'closed', () => {
            setTimeout(() => {
                let url = `http://localhost:${config.queryEnginePort}/checkreadystate`;
                http.get(url, res => { done(new Error('response received')); })
                    .on('error', () => { done(); });
            }, 500);
        });
        setTimeout(() => {
            query.stopServer();
        }, 0);
    });
});
