const path = require('path');
const helper = require('../scripts/sql-helper');

const isCI = process.env['CI']; // set by appveyor
const ciMod = isCI ? 5 : 1;
const timeTotal = 2 * 60 * 1000 * ciMod; // locally test runs at ~1 min
const timeForBackend = 10 * 1000 * ciMod;
const timeStep = 1 * 1000 * ciMod;
const timeStepMax = 1.5 * 1000 * ciMod;
const timeStepMin = 500 * ciMod;

const appPath = path.normalize(`${path.dirname(__dirname)
    }/linq-editor-win32-x64/linq-editor.exe`);

const connectionString = 
    isCI ? 'Data Source=.\\SQL2014; User Id=sa; Password=Password12!; Initial Catalog=testdb'  
         : 'Data Source=.\\sqlexpress;Integrated Security=True;Initial Catalog=testdb';

const connectionString2 = 
    isCI ? 'Data Source=.\\SQL2014; User Id=sa; Password=Password12!; Initial Catalog=testdb2'
         : 'Data Source=.\\sqlexpress;Integrated Security=True;Initial Catalog=testdb2';

const objectMethods = [
    'Equals',
    'ToString',
    'GetType',
    'GetHashCode'  
];

const sqlData = new Promise((done, err) => {
    helper.load().then(data => {
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
            .timeoutsAsyncScript(timeStepMax)
            .waitForExist(selector, timeForBackend)
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
            .waitForExist(selector, timeForBackend)
            .catch(function (e) { throw e; })
            .getText(selector)
            .then(function(val) {
                val.should.equal(hint);
            });
    }, client);
}

module.exports = {
    timeTotal,
    timeForBackend,
    timeStep,
    timeStepMax,
    timeStepMin,
    appPath,
    connectionString,
    connectionString2,
    checkTable,
    checkHints,
    sqlData,
    objectMethods
};
