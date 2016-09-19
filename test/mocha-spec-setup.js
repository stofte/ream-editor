// replaces index.html's shenanigans when using mocha.
global.electronRequire = require;
const isWindows = process.platform === 'win32';
global.IS_WINDOWS = isWindows;
global.IS_LINUX = !isWindows; // todo
global.MODE = 'DEVELOPMENT';
global.Assert = function Assert(cond, msg) {
    if (!cond) {
        throw (msg ? msg : 'assert failed');
    }
}
