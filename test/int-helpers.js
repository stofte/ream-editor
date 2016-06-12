const path = require('path');
const helper = require('../scripts/sql-helper');

const isAppveyor = process.env['APPVEYOR'];
const isTravis = process.env['TRAVIS'];
const isWin = process.env['APPDATA'];
const ciMod = isAppveyor || isTravis ? 3 : 1;

const suiteTimeout =  2 * 60 * 1000 * ciMod; // locally test runs at ~1 min
const backendTimeout = 20 * 1000 * ciMod;
const executeJsTimeout = 2 * 1000 * ciMod;
const pauseTimeout = 1000 * ciMod;

const IS_LINUX = !process.env.PATHEXT;

const appPath = path.normalize(`${path.dirname(__dirname)
    }/linq-editor-${IS_LINUX ? 'linux' : 'win32'}-x64/linq-editor${!IS_LINUX ? '.exe' : '' }`);

const connectionString = 
    isAppveyor  ? 'Data Source=.\\SQL2014;   User Id=sa; Password=Password12!; Initial Catalog=testdb' :
    isTravis ?    'Data Source=127.0.0.1;    User Id=postgres; Password=;      Initial Catalog=testdb' :
    isWin ?       'Data Source=.\\sqlexpress; Integrated Security=True;        Initial Catalog=testdb' :
                  'Data Source=192.168.56.2; User Id=sa; Password=Password12!; Initial Catalog=testdb';

const connectionString2 = 
    isAppveyor  ? 'Data Source=.\\SQL2014;   User Id=sa; Password=Password12!; Initial Catalog=testdb2' :
    isTravis ?    'Data Source=127.0.0.1;    User Id=postgres; Password=;      Initial Catalog=testdb2' :
    isWin ?       'Data Source=.\\sqlexpress; Integrated Security=True;        Initial Catalog=testdb2' :
                  'Data Source=192.168.56.2; User Id=sa; Password=Password12!; Initial Catalog=testdb2';

const objectMethods = [
    'Equals',
    'ToString',
    'GetType',
    'GetHashCode'  
];

const serverType = process.argv[3] && process.argv[3].indexOf('sqlserver') ?
    'sqlserver' : 'npgsql';  
const sqlData = new Promise((done, err) => {
    helper.load(serverType).then(data => {
        done(data);
    });
});

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
