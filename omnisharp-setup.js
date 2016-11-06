// sets up a folder for omnisharp to run off of, using the same dependencies 
// as the query program. for windows, use LOCALAPPDATA (passed from electron-main),
// to avoid clashing with other projects/nuget configurations

const fs = require('fs');
const path = require('path');
const copy = require('copy');

const appPath = process.platform !== 'win32' ?
    `${process.env.HOME}/.ream-editor/` :
    `${process.env.LOCALAPPDATA}\\ReamEditor\\`;

const dependencies = [
    // unless we glob, the folder path is mirroed.
    // this should only match nuget.config
    'query/*.config', 
    'query/query/src/ReamQuery/*.json'
];
const omnisharpPath = path.normalize(appPath + 'omnisharp');

dependencies.forEach(dep => 
    copy(dep, omnisharpPath, (err) => {
        if (err) console.log('Setup failed', err);
    }));
