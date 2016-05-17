const Application = require('spectron').Application
const assert = require('assert')
const path = require('path');

const appPath = path.normalize(`${__dirname}/linq-editor-win32-x64/linq-editor.exe`);
// double escape!
const connectionString = 'Data Source=.\\\\sqlexpress;Integrated Security=True;Initial Catalog=testdb';
const queryText = 'Foo.Take(10).Dump();';

function sendString(app, str) {
    for(var i = 0; i < str.length; i++) {
        console.log('char sending:', str[i], '=>', str.charCodeAt(i));
        app.webContents.sendInputEvent({
            type: 'keyDown',
            keyCode: str.charCodeAt(i)
        });
        app.webContents.sendInputEvent({
            type: 'keyUp',
            keyCode: str.charCodeAt(i)
        });
        app.webContents.sendInputEvent({
            type: 'char',
            keyCode: str.charCodeAt(i)
        });
    }
}

describe('application launch', function () {
    this.timeout(5 * 60 * 1000);

    before(function (done) {
        this.timeout(5 * 1000);
        this.app = new Application({
            path: appPath,
            requirePath: 'electronRequire'
        });
        this.app.start();
        setTimeout(function() {
            done();
        }, 4 * 1000);
    });

    
    it('shows an initial window', function () {
        return this.app.client.getWindowCount().then(function (count) {
            assert.equal(count, 1);
        });
    });
    
    it('starts on the start page with link to open connection manager', function () {
        return this.app.client
            .waitUntilTextExists('.test-open-conn-man', 'click to open connection manager', 5 * 1000);
    });
    
    describe('adding first connection flow', function() {
        this.timeout(4.5 * 60 * 1000);
        
        let testOutput = [];
        
        before(function(done) {
            this.timeout(3 * 1000);
            this.app.webContents.executeJavaScript(`
                document.querySelector('.int-test-start-page > p > a').click();
            `);
            setTimeout(function() {
                done();
            }, 2 * 1000);
        });
        
        it ('1. opens connection manager when clicking the link', function() {
            this.app.electron.remote.ipcMain.on('application-event', function(event, msg) {
                console.log('ipc', msg);
                let msgs = [
                    'int-test-result-1="1"',
                    'int-test-result-2="2"',
                    'int-test-result-3="4"',
                    'int-test-result-4="4"'
                ];
                if (msgs.indexOf(msg) !== -1) {
                    testOutput.push(msg);
                }
            });
            return this.app.client.waitUntilTextExists('.int-test-conn-man h1', 'connection manager');      
        });
        
        it('2. enters the new connection string value and focuses the field', function(done) {
            this.timeout(3 * 1000);
            this.app.webContents.executeJavaScript(`
                document.querySelector('.int-test-conn-man input').value = "${connectionString}";
                document.querySelector('.int-test-conn-man input').focus();
            `);
            setTimeout(function() {
                done();
            }, 2 * 1000);
        });
        
        it('3. receives the enter key to submit new connection string', function(done) {
            this.timeout(3 * 1000);
            var code = '\u000d';
            this.app.webContents.sendInputEvent({
                type: 'keyDown',
                keyCode: code
            });
            this.app.webContents.sendInputEvent({
                type: 'char',
                keyCode: code
            });
            this.app.webContents.sendInputEvent({
                type: 'keyUp',
                keyCode: code
            });
            setTimeout(function() {
                done();
            }, 2 * 1000);
        });
        
        it('4. closes the connection manager', function(done) {
            this.timeout(3 * 1000);
            this.app.webContents.executeJavaScript(`
                document.querySelector('.int-test-conn-man > p > a').click();
            `);
            setTimeout(function() {
                done();
            }, 2 * 1000);
        });
        
        it('5. opens a new tab (should be only visible link now)', function(done) {
            this.app.webContents.executeJavaScript(`
                document.querySelector('.int-test-start-page > p > a').click();
            `);
            setTimeout(function() {
                done();
            }, 2 * 1000);
        });
        
        it('6. updates the editor field', function(done) {
            this.timeout(3 * 1000);
            this.app.webContents.executeJavaScript(`
                document.querySelector('.CodeMirror.form-control').CodeMirror.setValue('${queryText}');
            `);
            setTimeout(function() {
                done();
            }, 2 * 1000);
        });
        
        it('7. execute the query', function(done) {
            this.timeout(10 * 1000);
            this.app.webContents.executeJavaScript(`
                document.querySelector('.int-test-execute-btn').click();
            `);
            setTimeout(function() {
                done();
            }, 9 * 1000);
        });
        
        // check for four entries 'Foo n', n = 0 - 3
        [1,2,3,4].map(i => {
            it('checks for result nr ' + i, function() {
                let src = `
                    (function() {
                        let ipc = electronRequire('electron').ipcRenderer;
                        let result = document.querySelector('table.table tbody tr:nth-child(${i}) td:nth-child(2)').innerText;
                        console.log('int-test-result-${i}="' + result + '"');
                        console.log(ipc.send('application-event', 'int-test-result-${i}="' + result + '"'));
                    })();
                `;
                return this.app.webContents.executeJavaScript(src);
            });
        });
        
        it('clears localStorage for next run', function(done) {
            this.timeout(3 * 1000);
            this.app.webContents.executeJavaScript(`
                localStorage.clear();
            `);
            setTimeout(function() {
                done();
            },2 * 1000); 
        });
        
        it('checks the result output', function(done) {
            this.timeout(10 * 1000);
            setTimeout(function() {
                console.log('testOutput', testOutput);
                assert(testOutput.length === 4);                
            }, 9 * 1000);
        });
    });
    
    // after(function () {
    //     if (this.app && this.app.isRunning()) {
    //         return this.app.stop();
    //     }
    // });
})
