// replaces index.html's shenanigans when using mocha.
global.electronRequire = require;
global.IS_WINDOWS= true;
global.IS_LINUX = false;
global.MODE = 'DEVELOPMENT';
global.Assert = function Assert(cond, msg) {
    if (!cond) {
        throw (msg ? msg : 'assert failed');
    }
}
// I would assume electron-mocha should provide this,
// but somehow not able to run in renderer with this available
global.XMLHttpRequest = require('xmlhttprequest').XMLHttpRequest;
global.WebSocket = require('websocket').client;
