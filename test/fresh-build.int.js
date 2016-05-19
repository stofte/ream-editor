const Application = require('spectron').Application
const chai = require('chai');
const http = require('http');
const { 
    checkTable,
    checkHints,
    timeTotal,
    timeForBackend,
    timeStep,
    timeStepMax,
    timeStepMin,
    appPath,
    connectionString,
    queryText
} = require('./int-helpers');

const expectedQueryResults = [
    ['Ident', 'SomeDesc'],
    ['0', 'Foo 0'],
    ['1', 'Foo 1'],
    ['2', 'Foo 2'],
    ['3', 'Foo 3']
];

const expectedAutocompletion = [
    'Description',
    'Dump',
    'Equals',
    'GetHashCode',
    'GetType',
    'Id',
    'ToString'
];

describe('fresh build', function() {
    this.timeout(timeTotal);
    before(function () {
        chai.should();
        this.app = new Application({
            path: appPath,
            requireName: 'electronRequire'
        });
        return this.app.start();
    });
    
    it('can add a new connection and perform a query and receive data', function () {
        // waitForX and similar functions will pass to catch if not fulfilled,
        // to pass the error up to mocha and fail the test, we just rethrow. 
        let err = function(e) { throw e; };
        let executingClient = this.app.client
            // assume timeout applies to all js invocations, individually, not combined
            .timeoutsAsyncScript(timeStepMax)
            .waitUntil(function () {
                return this.getText('.int-test-start-page > p > a')
                    .then((val) => val.should.equal('click to open connection manager'));
            })
            .catch(err)
            .click('.int-test-start-page > p > a')
            .waitUntil(function() {
                return this.getText('.int-test-conn-man > h1')
                    .then((val) => val.should.equal('connection manager'));
            })
            .catch(err)
            .click('.int-test-conn-man p input')
            // setValue seems to fail, the output gets messed up (must be some parsing going on)
            .executeAsync(function(str, done) {
                document.querySelector('.int-test-conn-man p input').value = str;
                done(document.querySelector('.int-test-conn-man p input').value);
            }, connectionString)
            .then(function(ret) {
                ret.value.should.equal(connectionString);
            })
            .keys('Enter')
            .pause(timeStepMin)
            .click('.int-test-conn-man > p > a')
            .pause(timeStepMin)
            .click('.int-test-start-page > p > a')
            .executeAsync(function(query, done) {
                // codemirror saves a reference to itself in the DOM weee
                done(document.querySelector('.CodeMirror').CodeMirror.setValue(query));
            }, queryText)
            .waitForVisible('.backend-timer-pulse', timeForBackend, true)
            .catch(err)
            .pause(timeStepMin)
            .click('.int-test-execute-btn');
        
        return checkTable(executingClient, expectedQueryResults)
            .pause(timeStep);
    });
    
    it('provides the expected member completions for Foo entity', function() {
        // cursor index, given the query
        let cursorCol = queryText.indexOf('x.') + 2;
        let cursorRow = 0;
        let suggestionClient = this.app.client
            // setting the cursor by codemirror api alone doesn't seem good enough. 
            // seems to work if we click in the editor first, using this approach.
            .moveToObject('.CodeMirror')
            .click('.CodeMirror')
            .timeoutsAsyncScript(timeStepMax)
            .executeAsync(function(row, col, done) {
                document.querySelector('.CodeMirror').CodeMirror.setCursor(row, col);
                done(document.querySelector('.CodeMirror').CodeMirror.getCursor());
            }, cursorRow, cursorCol)
            .then(function(cursor) {
                cursor.value.ch.should.equal(cursorCol);
                cursor.value.line.should.equal(cursorRow);
            })
            .pause(timeStepMin)
            .keys('\uE009') // press down ctrl
            .keys('\uE00D') // space
            .keys('\uE000'); // lift modifier (ctrl)
            
        return checkHints(suggestionClient, expectedAutocompletion)
            .pause(timeStep);
    });
    
    // seems tricky to close the window without bypassing the 
    // close event handler that in turn closes the backend services
    it('closes the single window', function () {
        this.timeout(timeStepMax);
        this.app.client
            .timeoutsAsyncScript(timeForBackend)
            .executeAsync(function() {
                // for debug purposes
                localStorage.clear();
                // this sends the close event to the regular shutdown handler.
                // other ways to close the window seems to fail.
                const win = electronRequire('electron').remote.getCurrentWindow();
                win.emit('close');
            });
        return new Promise((succ, err) => {
            setTimeout(succ, timeStep);
        });
    });
    
    // check via http if services are still up and going.
    it('closes omnisharp service', function(done) {
        this.timeout(timeForBackend);
        let url = `http://localhost:2000/checkreadystate`;
        http.get(url, res => { done(new Error('response received')); })
            .on('error', () => { done(); });
    });
    
    it('closes query service', function(done) {
        this.timeout(timeForBackend);
        let url = `http://localhost:8111/checkreadystate`;
        http.get(url, res => { done(new Error('response received')); })
            .on('error', () => { done(); });
    });
});
