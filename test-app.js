const isCI = process.env['CI']; // set by appveyor
const Application = require('spectron').Application
const assert = require('assert')
const path = require('path');
const chai = require('chai');
const http = require('http');
const appPath = path.normalize(`${__dirname}/linq-editor-win32-x64/linq-editor.exe`);
const connectionString = 
    isCI ? 'Data Source=(local)\SQL2014; User=sa; Password=Password12!; Initial Catalog=testdb'  
         : 'Data Source=.\\sqlexpress;Integrated Security=True;Initial Catalog=testdb';
const queryText = 'Foo.Select(x => new { Ident = x.Id, SomeDesc = x.Description }).Dump();';
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

const timeTotal = 5 * 60 * 1000; // locally test runs at < 30 secs
const timeForBackend = 10 * 1000;
const timeStep = 1 * 1000;
const timeStepMax = 1.5 * 1000;
const timeStepMin = 500;

// first create checks for all cell values, ensuring we keep passing the client around, 
// and return it in the end, so it can be awaited. total bullshit logic ftw
function checkTable(client, rows, rowIndex, isBody) {
    if (!rows || rows.length === 0) {
        return client;
    }
    let rowIdx = rowIndex || 1;
    let cellSelector = isBody ? 'td' : 'th';
    let rowSelector = isBody ? 'tbody' : 'thead';
    let rowCheckingClient = rows[0].reduce((wrapped, cellValue, idx) => {
        let selector = `table.table.table-condensed ${isBody ? 'tbody' : 'thead'} 
            tr:nth-child(${rowIdx}) ${isBody ? 'td' : 'th'}:nth-child(${idx + 1})`;
        return wrapped
            .waitForExist(selector, timeForBackend)
            .catch(function(e) { throw e })
            .getText(selector)
            .then(function (val) {
                val.should.equal(cellValue);
            });
    }, client);
    return checkTable(rowCheckingClient, rows.slice(1), isBody ? rowIdx + 1 : 1, true);
}

function checkHints(client, hints) {
    if (!hints || hints.length === 0) {
        return client;
    }
    return hints.reduce((wrapped, hint, idx) => {
        let selector = `ul.CodeMirror-hints li:nth-child(${idx + 1})`;
        return wrapped
            .waitForExist(selector, timeForBackend)
            .catch(function (e) { throw e; })
            .getText(selector)
            .then(function(val) {
                val.should.equal(hint);
            });
    }, client);
}

describe('app with no saved db connections', function() {
    this.timeout(timeTotal);
    before(function () {
        chai.should();
        this.app = new Application({
            path: appPath
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
