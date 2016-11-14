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
    packagerOutputPath,
    keyDefs,
    connectionString,
    objectMethods
    // ,


    // ,
    // connectionString2,
    // sqlData,
    // serverType
} = require('./int-helpers');

// const queryText = 'Foo.Select(x => new { SomeDesc = x.Description, Ident = x.IdAuto })';
// const queryText2 = 'TypeTest.Take(10)';
// const queryText3 = 'TypeTest.Select(x => x.';

// let expectedData = null;
// let expectedCompletions = null;

// function setExpectedData(data) {
//     expectedData = _.cloneDeep(data);
//     // since we do some mapping in our query, we modify the header values
//     expectedData[0][0][0] = 'SomeDesc';
//     expectedData[0][0][1] = 'Ident';
//     expectedCompletions = [
//         _.sortBy(data[0][0].concat(objectMethods), str => str.toLocaleLowerCase()),
//         _.sortBy(data[1][0].concat(objectMethods), str => str.toLocaleLowerCase())
//     ];
// }

const err = function waitErrorHandler(e) { throw e; };

function pasteQueryToEditorAndCheckResultsAfterExecuting(query, expectedTabTitle) {
    return this.app.client
        .moveToObject('.CodeMirror')
        .click('.CodeMirror')
        .keys(query)
        .waitForEnabled('.int-test-execute-btn', backendTimeout)
        .click('.int-test-execute-btn')
        .waitForExist('.int-test-result-tab-title', backendTimeout)
        .getText('.int-test-result-tab-title')
        .then(function (val) {
            val.should.equal(expectedTabTitle);
        })
        .catch(err)
        ;
}
    
function toggleConnectionMgr(done) {
    const win = electronRequire('electron').remote.getCurrentWindow();
    win.webContents.send('application-event', 'connections-panel');
    done();
}

describe('fresh build', function() {
    this.timeout(suiteTimeout);

    before(function () {
        chai.should();
        console.log(`CWD: "${process.cwd()}/${packagerOutputPath}"`)
        this.app = new Application({
            path: appPath,
            requireName: 'electronRequire',
            cwd: path.resolve(`${process.cwd()}/${packagerOutputPath}`)
        });
        return this.app.start();
    });
    
    // before(function() {
    //     return sqlData.then(setExpectedData);
    // });
    
    it('starts with tab open', function() {
        return this.app.client
            .waitForExist('.int-test-tab-list button')
            .getText('.int-test-tab-list button')
            .then(function (val) {
                val.should.equal('Untitled 0');
            })
            ;
    });

    const codeBufferText = 'var x = 21;\nx*2';
    it('returns expected results for code sample', function() {
        return pasteQueryToEditorAndCheckResultsAfterExecuting
            .call(this, codeBufferText, 'int')
            ;
    });

    it('returns expectd completions for integer variable', function() {
        let cursorCol = 3;
        let cursorRow = 1;
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
            .keys(keyDefs.Backspace)
            .keys(keyDefs.Backspace)
            .keys('.')
            .pause(pauseTimeout)
            .keys('\uE009') // press down ctrl
            .keys('\uE00D') // space
            .keys('\uE000') // lift modifier (ctrl)
            ;

        // int object doesnt do much
        return checkHints(suggestionClient, [
            'CompareTo',
            ... objectMethods
        ].sort());
    });
    
    it('can add a new connection using connection manager', function () {
        const contextSelector = `.int-test-conn-man form option:nth-child(3)`;

        return this.app.client
            .click('.int-test-tab-list button')
            // todo sending F4 using .keys only seems to function when window has focus
            .executeAsync(toggleConnectionMgr)
            .waitForVisible('.int-test-conn-man form input')
            .pause(pauseTimeout * 5)
            // add connection string value
            .executeAsync(function(str, done) {
                const elm = document.querySelector('.int-test-conn-man form input'); 
                elm.value = str;
                done(elm.value);
            }, connectionString)
            .then(function(ret) {
                ret.value.should.equal(connectionString);
            })
            // since we're setting the values via chrome driver,
            // we need to touch the value to make angular aware of it
            // todo zonejs might provide a way to refresh
            .click('.int-test-conn-man form input')
            .keys(keyDefs.Space)
            .keys(keyDefs.Backspace)
            // change connection type
            .click('.int-test-conn-man form select')
            .waitForVisible(contextSelector)
            .click(contextSelector)
            .click('.int-test-conn-man form button')
            .executeAsync(toggleConnectionMgr)
            .pause(pauseTimeout)
            ;
    });

    it('can open new tab and select new connection in dropdown', function() {
        const ctxSelector = '.int-test-context-dropdown option:last-child'; 
        const newTabSelector = '.int-test-tab-list .int-test-tab:nth-child(2) .int-test-tab-text'; 
        return this.app.client
            .click('.int-test-tab-new-button')
            .waitForExist(newTabSelector)
            .getText(newTabSelector)
            .then(function (val) {
                val.should.equal('Untitled 1');
            })
            .click('.int-test-context-dropdown')
            .waitForVisible(ctxSelector)
            .click(ctxSelector)            
            .pause(pauseTimeout)
            ;
    });


    const queryText = 'city.Where(x => x.Name.StartsWith("Ca"))';
    it('can query using new connection', function() {
        return pasteQueryToEditorAndCheckResultsAfterExecuting
            .call(this, queryText, 'city[83]');
    });
    
    it('provides the expected member completions for city entity', function() {
        const fragment = 'x.Name';
        // cursor index, given the query, we're gonna have to delete the current member
        // otherwise we just autocomplete the current word and confuse the test 
        let cursorCol = queryText.length;
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
            .keys(keyDefs.Backspace)
            .keys(keyDefs.Backspace)
            .keys(keyDefs.Backspace)
            .keys(keyDefs.Backspace)
            .keys(keyDefs.Backspace)
            .keys(keyDefs.Backspace)
            .keys(keyDefs.Backspace)
            .keys(keyDefs.Backspace)
            .keys(keyDefs.Backspace)
            .keys(keyDefs.Backspace)
            .keys(keyDefs.Backspace)
            .keys(keyDefs.Backspace)
            .keys(keyDefs.Backspace)
            .keys(keyDefs.Backspace)
            .keys(keyDefs.Backspace)
            .keys(keyDefs.Backspace)
            .keys(keyDefs.Backspace)
            .keys(keyDefs.Backspace)
            .keys(keyDefs.Backspace)
            .keys(keyDefs.Backspace)
            .keys(keyDefs.Backspace)
            .keys(keyDefs.Backspace)
            .pause(pauseTimeout)
            .keys('\uE009') // press down ctrl
            .keys('\uE00D') // space
            .keys('\uE000') // lift modifier (ctrl)
            ;

        return checkHints(suggestionClient, [
            'CountryCode',
            'District',
            'Id',
            'Name',
            'Population',
            ... objectMethods
        ].sort());
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
            this.app = new Application({
                path: appPath,
                requireName: 'electronRequire'
            });
            return this.app.start();
        });
              
        it('starts in editor mode and can change connection to initial connection', function() {
            const ctxSelector = '.int-test-context-dropdown option:last-child';
            return this.app.client
                .waitForExist('.int-test-tab-list button')
                .getText('.int-test-tab-list button')
                .then(function (val) {
                    val.should.equal('Untitled 0');
                })
                .click('.int-test-context-dropdown')
                .waitForVisible(ctxSelector)
                .click(ctxSelector)            
                .pause(pauseTimeout)
                ;
        });
       
        it('uses connection and can query database', function() {
            return pasteQueryToEditorAndCheckResultsAfterExecuting
                .call(this, queryText, 'city[83]');
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
