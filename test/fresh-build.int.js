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

const queryText = 'Foo.Select(x => new { SomeDesc = x.Description, Ident = x.IdAuto }).Dump();';
const queryText2 = 'TypeTest.Take(10).Dump();';
const queryText3 = 'TypeTest.Select(x => x.';

let expectedData = null;
let expectedCompletions = null;

function setExpectedData(data) {
    expectedData = _.cloneDeep(data);
    // since we do some mapping in our query, we modify the header values
    expectedData[0][0][0] = 'SomeDesc';
    expectedData[0][0][1] = 'Ident';
    expectedCompletions = [
        _.sortBy(data[0][0].concat(objectMethods), str => str.toLocaleLowerCase()),
        _.sortBy(data[1][0].concat(objectMethods), str => str.toLocaleLowerCase())
    ];
}

const err = function waitErrorHandler(e) { throw e; };

function queryFooUsingCurrentConnectionAndCheckResults() {
        let executingClient = this.app.client
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
    }

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
    
    before(function() {
        return sqlData.then(setExpectedData);
    });
    
    it('starts on the start page', function() {
        return this.app.client
            .pause(timeStep)
            .getText('.int-test-tab-list .navbar-brand')
            .then(function (val) {
                val.should.equal('Hello!');
            })
            ;
    });
    
    it('can add a new connection and close connection manager', function () {        
        return this.app.client
            .click('.int-test-start-page .btn-default')
            .pause(timeStepMin)
            .click('.int-test-conn-man form input')
            // setValue seems to fail, the output gets messed up (must be some parsing going on)
            .executeAsync(function(str, done) {
                document.querySelector('.int-test-conn-man form input').value = str;
                done(document.querySelector('.int-test-conn-man form input').value);
            }, connectionString)
            .then(function(ret) {
                ret.value.should.equal(connectionString);
            })
            .keys('Enter')
            .pause(timeStepMin)
            .click('.int-test-conn-man p > button')
            .pause(timeStep);
    });
       
    it('can query using new connection', queryFooUsingCurrentConnectionAndCheckResults);
    
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
            .click('.int-test-tab-list ul li:first-child')
            .pause(timeStep)
            .keys('\uE009')
            .keys('d')
            .keys('\uE000')
            .pause(timeStep)
            .click('.int-test-conn-man form input')
            // setValue seems to fail, the output gets messed up (must be some parsing going on)
            .executeAsync(function(str, done) {
                document.querySelector('.int-test-conn-man form input').value = str;
                done(document.querySelector('.int-test-conn-man form input').value);
            }, connectionString2)
            .then(function(ret) {
                ret.value.should.equal(connectionString2);
            })
            .keys('Enter')
            .pause(timeStep)
            .keys('\uE009')
            .keys('d')
            .keys('\uE000')
            ;
    });
    
    it('can open a new tab and change the connection to the newly created', function() {
        return this.app.client
            .click('.int-test-tab-list .glyphicon.glyphicon-plus')
            .pause(timeStep)
            .getText('.int-test-tab-list li:nth-child(2) a')
            .then(function(val) {
                val.should.equal('Query 2');
            })
            .click('#connection-selector-btn-keyboard-nav')
            .waitForExist('.int-test-conn-sel .dropdown-menu li:nth-child(2) a')
            .keys('\uE015')
            .keys('\uE015')
            .keys('Enter')
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
    
    it('can provide autocompletions for TypeTest table', function() {
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
    
    it('shuts down', function() {
        this.timeout(timeForBackend);
        this.app.client
            .timeoutsAsyncScript(timeStepMax)
            .executeAsync(function() {
                // this sends the close event to the regular shutdown handler.
                // other ways to close the window seems to fail.
                const win = electronRequire('electron').remote.getCurrentWindow();
                win.emit('close');
            });
        return new Promise((succ, err) => {
            setTimeout(succ, timeStepMax * 3); // ~ 5 secs
        });        
    });
    
    describe('second run', function() {
        this.timeout(timeTotal);

        before(function () {
            chai.should();
            this.app = new Application({
                path: appPath,
                requireName: 'electronRequire'
            });
            return this.app.start();
        });
              
        it('starts in editor mode', function() {
            return this.app.client
                .pause(timeStepMax)
                .getText('.int-test-tab-list .navbar-brand')
                .then(function (val) {
                    val.should.equal(''); 
                });
        });
        
        it('uses testdb connection and can query Foo', 
            queryFooUsingCurrentConnectionAndCheckResults);

        describe('when closing', function () {
            before(function() {
                this.timeout(timeStepMax);
                this.app.client
                    .timeoutsAsyncScript(timeStepMax)
                    .executeAsync(function() {
                        localStorage.clear(); // for next run
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
});
