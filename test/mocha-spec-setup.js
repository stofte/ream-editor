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
// angular requirement
global.XMLHttpRequest = require('xmlhttprequest').XMLHttpRequest;
// used by unit tests
global.__nodeHttp = require('http');
