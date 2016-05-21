const Application = require('spectron').Application
const chai = require('chai');
const http = require('http');
const _ = require('lodash');
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
    connectionString2,
    sqlData,
    objectMethods
} = require('./int-helpers');

const queryText = 'Foo.Select(x => new { Ident = x.Id, SomeDesc = x.Description }).Dump();';
const queryText2 = 'TypeTest.Take(10).Dump();';
const queryText3 = 'TypeTest.Select(x => x.';

let expectedData = null;
let expectedCompletions = null;

function setExpectedData(data) {
    expectedData = _.cloneDeep(data);
    // since we do some mapping in our query, we modify the header values
    expectedData[0][0][0] = 'Ident';
    expectedData[0][0][1] = 'SomeDesc';
    expectedCompletions = [
        _.sortBy(data[0][0].concat(objectMethods), str => str.toLocaleLowerCase()),
        _.sortBy(data[1][0].concat(objectMethods)
            .concat(['rowversioncol']), str => str.toLocaleLowerCase())
    ];
}

describe('fresh build', function() {
    let err = function(e) { throw e; };    
    this.timeout(timeTotal);

    before(function () {
        chai.should();
        this.app = new Application({
            path: appPath,
            requireName: 'electronRequire'
        });
        return this.app.start();
    });
    
    before(function() {
        return sqlData.then(setExpectedData);
    });
    
    it('can add a new connection and perform a query and receive data', function () {        
        let executingClient = this.app.client
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
            .waitForEnabled('.int-test-execute-btn', timeForBackend)
            .catch(err)
            .pause(timeStepMin)
            .click('.int-test-execute-btn')
            ;
        
        return checkTable(executingClient, expectedData[0])
            .pause(timeStep)
            ;
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
            .keys('\uE000') // lift modifier (ctrl)
            ;
            
        return checkHints(suggestionClient, expectedCompletions[0])
            .pause(timeStep)
            ;
    });
    
    it('can add another connection string via the connection manager', function() {
        return this.app.client 
            .timeoutsAsyncScript(timeStepMax)
            .click('.main-layer.layer-visible')
            .pause(timeStepMin)
            .keys('\uE009')
            .keys('d')
            .keys('\uE000')
            .pause(timeStepMin)
            .click('.int-test-conn-man p input')
            // setValue seems to fail, the output gets messed up (must be some parsing going on)
            .executeAsync(function(str, done) {
                document.querySelector('.int-test-conn-man p input').value = str;
                done(document.querySelector('.int-test-conn-man p input').value);
            }, connectionString2)
            .then(function(ret) {
                ret.value.should.equal(connectionString2);
            })
            .keys('Enter')
            .pause(timeStepMin)
            .keys('\uE009')
            .keys('d')
            .keys('\uE000')
            ;
    });
    
    it('can open a new tab and change the connection to the newly created', function() {
        return this.app.client
            .click('.int-test-tab-list .glyphicon.glyphicon-plus')
            .waitForText('.int-test-tab-list li:nth-child(2) a', 'Edit 1', timeStepMax)
            .catch(err)
            .click('#connection-selector-btn-keyboard-nav')
            .waitForExist('.int-test-conn-sel .dropdown-menu li:nth-child(2) a')
            .keys('\uE015')
            .keys('\uE015')
            .keys('Enter')
            // todo there's no indicator for this, so just wait some time
            .pause(timeStepMin)
            ;
    });
    
    it('can query TypeTest using new connection and receive expected results', function() {
        const executingClient = this.app.client
            .timeoutsAsyncScript(timeStepMax)
            .executeAsync(function(query, done) {
                done(document.querySelector('.CodeMirror').CodeMirror.setValue(query));
            }, queryText2)
            .pause(timeStepMin)
            .click('.int-test-execute-btn')
            ;
        
        return checkTable(executingClient, expectedData[1])
            .pause(timeStep)
            ;
    });
    
    // fix up omnisharp suggestions first
    it('can provide expected autocompletions for complex types', function() {
        // cursor index, given the query
        let cursorCol = queryText3.indexOf('x.') + 2;
        let cursorRow = 0;
        let suggestionClient = this.app.client
            .timeoutsAsyncScript(timeStepMax)
            .executeAsync(function(query, done) {
                done(document.querySelector('.CodeMirror').CodeMirror.setValue(query));
            }, queryText3)
            .pause(timeStepMin)
            .moveToObject('.CodeMirror')
            .click('.CodeMirror')
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
            .keys('\uE000') // lift modifier (ctrl)
            ;
        
        return checkHints(suggestionClient, expectedCompletions[1])
            .pause(timeStep)
            ;
    });
    
    describe('when closing', function () {
        before(function() {
            this.timeout(timeStepMax);
            this.app.client
                .timeoutsAsyncScript(timeStepMax)
                .executeAsync(function() {
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
        it('also closes background omnisharp process', function(done) {
            this.timeout(timeForBackend);
            let url = `http://localhost:2000/checkreadystate`;
            http.get(url, res => { done(new Error('response received')); })
                .on('error', () => { done(); });
        });
        
        it('also closes background query process', function(done) {
            this.timeout(timeForBackend);
            let url = `http://localhost:8111/checkreadystate`;
            http.get(url, res => { done(new Error('response received')); })
                .on('error', () => { done(); });
        });

    });
    

});

describe('instance with saved connections', function() {
    let err = function(e) { throw e; };    
    this.timeout(timeTotal);

    before(function () {
        chai.should();
        this.app = new Application({
            path: appPath,
            requireName: 'electronRequire'
        });
        return this.app.start();
    });    
});