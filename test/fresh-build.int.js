const Application = require('spectron').Application
const chai = require('chai');
const http = require('http');
const _ = require('lodash');
const { 
    checkTable,
    checkHints,
    suiteTimeout,
    backendTimeout,
    pauseTimeout,
    executeJsTimeout,
    appPath,
    connectionString,
    connectionString2,
    sqlData,
    objectMethods,
    serverType
} = require('./int-helpers');

const queryText = 'Foo.Select(x => new { SomeDesc = x.Description, Ident = x.IdAuto })';
const queryText2 = 'TypeTest.Take(10)';
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

function pasteQueryToEditorAndCheckResultsAfterExecuting(query, expected) {
        let executingClient = this.app.client
            .moveToObject('.CodeMirror')
            .click('.CodeMirror')
            .keys(query)
            .waitForEnabled('.int-test-execute-btn', backendTimeout)
            .click('.int-test-execute-btn')
            .waitForExist('.result-display-component', backendTimeout)
            // appears when result is ready
            .waitForExist('.output-table-overview button.btn', backendTimeout)
            .pause(pauseTimeout) // give it some time to render
            .catch(err)
            ;
        
        return checkTable(executingClient, expected)
            ;
}

describe('fresh build', function() {
    this.timeout(suiteTimeout);

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
            .waitForExist('.int-test-tab-list .navbar-brand')
            .getText('.int-test-tab-list .navbar-brand')
            .then(function (val) {
                val.should.equal('Hello!');
            })
            ;
    });
    
    it('can add a new connection and close connection manager', function () {        
        let c1 = this.app.client
            .click('.int-test-start-page .btn-default')
            .waitForVisible('.int-test-conn-man form input');
        
        if (serverType === 'npgsql') {
            c1 = c1
                .click(`input[value=npgsql]`)
        }

        return c1
            .pause(pauseTimeout)
            .click('.int-test-conn-man form input')
            .executeAsync(function(str, done) {
                document.querySelector('.int-test-conn-man form input').value = str;
                done(document.querySelector('.int-test-conn-man form input').value);
            }, connectionString)
            .then(function(ret) {
                ret.value.should.equal(connectionString);
            })
            .keys('Enter')
            .pause(pauseTimeout)
            .click('.int-test-conn-man p > button')
            .pause(pauseTimeout)
            ;
    }); 
       
    it('can query using new connection', function() {
        return pasteQueryToEditorAndCheckResultsAfterExecuting
            .call(this, queryText, expectedData[0]);
    });
    
    it('provides the expected member completions for Foo entity', function() {
        const fragment = 'x.Description';
        // cursor index, given the query, we're gonna have to delete the current member
        // otherwise we just autocomplete the current word and confuse the test 
        let cursorCol = queryText.indexOf(fragment) + fragment.length;
        let cursorRow = 0;
        let suggestionClient = this.app.client
            // setting the cursor by codemirror api alone doesn't seem good enough. 
            // seems to work if we click in the editor first, using this approach.
            .moveToObject('.CodeMirror')
            .click('.CodeMirror')
            .pause(pauseTimeout)
            .timeoutsAsyncScript(executeJsTimeout)
            .executeAsync(function(row, col, done) {
                document.querySelector('.CodeMirror').CodeMirror.setCursor(row, col);
                done(document.querySelector('.CodeMirror').CodeMirror.getCursor());
            }, cursorRow, cursorCol)
            .then(function(cursor) {
                cursor.value.ch.should.equal(cursorCol);
                cursor.value.line.should.equal(cursorRow);
            })
            .pause(pauseTimeout)
            .keys('\uE003')
            .keys('\uE003')
            .keys('\uE003')
            .keys('\uE003')
            .keys('\uE003')
            .keys('\uE003')
            .keys('\uE003')
            .keys('\uE003')
            .keys('\uE003')
            .keys('\uE003')
            .keys('\uE003')
            .pause(pauseTimeout)
            .keys('\uE009') // press down ctrl
            .keys('\uE00D') // space
            .keys('\uE000') // lift modifier (ctrl)
            ;
            
        return checkHints(suggestionClient, expectedCompletions[0])
            ;
    });
    
    it('can add another connection string via the connection manager', function() {
        let c1 = this.app.client 
            .timeoutsAsyncScript(executeJsTimeout)
            .click('.int-test-tab-list ul li:first-child')
            .keys('\uE009')
            .keys('d')
            .keys('\uE000')
            .pause(pauseTimeout)
            ;
        
        if (serverType === 'npgsql') {
            c1 = c1
                .click(`input[value=npgsql]`)
                .pause(pauseTimeout)
        }

        return c1
            .click('.int-test-conn-man form input')
            .executeAsync(function(str, done) {
                document.querySelector('.int-test-conn-man form input').value = str;
                done(document.querySelector('.int-test-conn-man form input').value);
            }, connectionString2)
            .then(function(ret) {
                ret.value.should.equal(connectionString2);
            })
            .keys('Enter')
            .pause(pauseTimeout)
            .keys('\uE009')
            .keys('d')
            .keys('\uE000')
            .pause(pauseTimeout)
            ;
    });
    
    it('can open a new tab and change the connection to the newly created', function() {
        let newTabSelector = '.int-test-tab-list li:nth-child(2) a';
        return this.app.client
            .click('.int-test-tab-list .glyphicon.glyphicon-plus')
            .waitForExist(newTabSelector)
            .pause(pauseTimeout)
            .getText(newTabSelector)
            .then(function(val) {
                val.should.equal('Query 2');
            })
            .click('#connection-selector-btn-keyboard-nav')
            .waitForExist('.int-test-conn-sel .dropdown-menu li:nth-child(1) a')
            .pause(pauseTimeout)
            .keys('\uE015')
            .keys('Enter')
            ;
    });


    it('can query TypeTest using new connection and receive expected results', function() {
        return pasteQueryToEditorAndCheckResultsAfterExecuting
            .call(this, queryText2, expectedData[1]);
    });
    
    it('can provide autocompletions for TypeTest table', function() {
        // cursor index, given the query
        let cursorCol = queryText3.indexOf('x.') + 2;
        let cursorRow = 0;
        let suggestionClient = this.app.client
            .moveToObject('.CodeMirror')
            .click('.CodeMirror')
            .keys('\uE009') // press down ctrl
            .keys('a')
            .keys('\uE000') // lift modifier (ctrl)      
            .keys('\uE003') // delete all      
            .pause(pauseTimeout)
            .keys(queryText3)
            .executeAsync(function(row, col, done) {
                document.querySelector('.CodeMirror').CodeMirror.setCursor(row, col);
                done(document.querySelector('.CodeMirror').CodeMirror.getCursor());
            }, cursorRow, cursorCol)
            .then(function(cursor) {
                cursor.value.ch.should.equal(cursorCol);
                cursor.value.line.should.equal(cursorRow);
            })
            .pause(pauseTimeout)
            .keys('\uE009') // press down ctrl
            .keys('\uE00D') // space
            .keys('\uE000') // lift modifier (ctrl)
            ;
        
        return checkHints(suggestionClient, expectedCompletions[1])
            ;
    });
    
    it('shuts down', function() {
        this.timeout(backendTimeout);
        this.app.client
            .timeoutsAsyncScript(backendTimeout - pauseTimeout)
            .executeAsync(function() {
                // this sends the close event to the regular shutdown handler.
                // other ways to close the window seems to fail.
                const win = electronRequire('electron').remote.getCurrentWindow();
                win.emit('close');
            });
        return new Promise((succ, err) => {
            setTimeout(succ, backendTimeout - pauseTimeout);
        });        
    });
    
    describe('second run', function() {
        this.timeout(suiteTimeout);

        before(function () {
            chai.should();
            this.app = new Application({
                path: appPath,
                requireName: 'electronRequire'
            });
            return this.app.start();
        });
              
        it('starts in editor mode and can change connection to initial connection', function() {
            return this.app.client
                .waitForExist('.int-test-tab-list .navbar-brand')
                .getText('.int-test-tab-list .navbar-brand')
                .then(function (val) {
                    val.should.equal(''); 
                })
                .click('#connection-selector-btn-keyboard-nav')
                .waitForExist('.int-test-conn-sel .dropdown-menu li:nth-child(2) a')
                .keys('\uE015')
                .keys('\uE015')
                .keys('Enter')
                .pause(pauseTimeout)
                ;
        });
       
        it('uses testdb connection and can query Foo', function() {
            return pasteQueryToEditorAndCheckResultsAfterExecuting
                .call(this, queryText, expectedData[0]);
        });

        describe('when closing', function () {
            before(function() {
                this.timeout(backendTimeout);
                this.app.client
                    .timeoutsAsyncScript(executeJsTimeout)
                    .executeAsync(function() {
                        localStorage.clear(); // for next run
                        const win = electronRequire('electron').remote.getCurrentWindow();
                        win.emit('close');
                    });
                return new Promise((succ, err) => {
                    // give it a bit more time before we check services
                    setTimeout(succ, backendTimeout * 0.7);
                });
            });

            // check via http if services are still up and going.
            it('also closes background omnisharp process', function(done) {
                this.timeout(backendTimeout);
                let url = `http://localhost:2000/checkreadystate`;
                http.get(url, res => { done(new Error('response received')); })
                    .on('error', () => { done(); });
            });
            
            it('also closes background query process', function(done) {
                this.timeout(backendTimeout);
                let url = `http://localhost:8111/checkreadystate`;
                http.get(url, res => { done(new Error('response received')); })
                    .on('error', () => { done(); });
            });

        });
    });
});
