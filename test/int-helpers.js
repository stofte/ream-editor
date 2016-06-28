const path = require('path');
const helper = require('../scripts/sql-helper');
const connections = require('../scripts/connections.json');

// todo probably not the best, required for locating the electron output binary, and launching test
const IS_LINUX = !process.env.PATHEXT;
// appveyor/travisci sets this var
const CI_MOD = process.env['CI'] ? 3 : 1;

const suiteTimeout =  2 * 60 * 1000 * CI_MOD; // locally test runs at ~1 min
const backendTimeout = 20 * 1000 * CI_MOD;
const executeJsTimeout = 2 * 1000 * CI_MOD;
const pauseTimeout = 1000 * CI_MOD;

const appPath = path.normalize(`${path.dirname(__dirname)
    }/ream-editor-${IS_LINUX ? 'linux' : 'win32'}-x64/ream-editor${!IS_LINUX ? '.exe' : '' }`);

const [connectionString, connectionString2, serverType] = getContextInfo();

const objectMethods = [
    'Equals',
    'ToString',
    'GetType',
    'GetHashCode'  
];

const sqlData = new Promise((done, err) => {
    helper.load(serverType).then(data => {
        done(data);
    });
});

function getContextInfo() {
    let cmdLine = process.argv.join(' ');
    let npgsql = cmdLine.indexOf('--npgsql') !== -1;
    let sqlServer = cmdLine.indexOf('--sqlserver') !== -1;
    let appveyor = cmdLine.indexOf('--appveyor') !== -1;
    let travis = cmdLine.indexOf('--travis') !== -1;
    let local = cmdLine.indexOf('--local') !== -1;
    return [...connections[
        local ? 'local' : 
        appveyor ? 'appveyor' : 'travis'
    ][
        npgsql ? 'npgsql' : 'sqlserver'
    ], npgsql ? 'npgsql' : 'sqlserver'];
}

// first create checks for all cell values, ensuring we keep passing the client around, 
// and return it in the end, so it can be awaited
function checkTable(client, rows, rowIndex, isBody) {
    if (!rows || rows.length === 0) {
        return client;
    }
    let rowIdx = rowIndex || 1;
    let rowCheckingClient = rows[0].reduce((wrapped, expectedVal, idx) => {
        let selector = `.output-table-${isBody ? 'rows' : 'header'} 
            ${isBody ? `div:nth-child(${rowIdx})` : ''}
            div:nth-child(${idx + 2}) div:first-child`
            .split('\n').join('').replace(/\s+/g, ' '); // pretty up
        return wrapped
            .timeoutsAsyncScript(executeJsTimeout)
            .waitForExist(selector, backendTimeout)
            .catch(function(e) { throw e })
            // tab is filtered if using getValue so instead this workaround
            .executeAsync(function(sel, isBody, done) {
                done(document.querySelector(sel).innerText);
            }, selector, isBody)
            .then(function (val) {
                // for datetime/timespan and the binary cols, 
                // we just use a regex. for binary we dont care
                let op = expectedVal.constructor.name === 'RegExp' ? 'match' : 'equal';
                val.value.should[op](expectedVal);
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
            .waitForExist(selector, backendTimeout)
            .catch(function (e) { throw e; })
            .getText(selector)
            .then(function(val) {
                val.should.equal(hint);
            });
    }, client);
}


module.exports = {
    suiteTimeout,
    backendTimeout,
    pauseTimeout,
    executeJsTimeout,
    appPath,
    connectionString,
    connectionString2,
    checkTable,
    checkHints,
    sqlData,
    objectMethods,
    serverType
};
