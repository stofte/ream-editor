// sets up a folder for omnisharp to run off of, using the same dependencies 
// as the query program. for windows, use LOCALAPPDATA (passed from electron-main),
// to avoid clashing with other projects/nuget configurations
const fs = require('fs');
const path = require('path');

function createPath(folder) {
    try {
        fs.accessSync(folder);
    } catch (e) {
        createPath(path.dirname(folder));
        fs.mkdirSync(folder);
    }    
}

function copyFile(fileName, from, to, isDebug) {
    const toFile = path.join(to, fileName);
    try {
        if (isDebug) throw "control flow ftw";
        fs.accessSync(toFile);
    } catch (e) {{
        fs.readFile(path.join(from, fileName) , 'utf-8', (err, value) => {
            fs.writeFile(path.join(to, fileName), value);
        });        
    }}
}

module.exports = function omnisharpSetup(mode, folder) {
    const isDebug = mode === 'DEBUG';
    try {
        fs.accessSync(folder);
    } catch (e) {
        createPath(folder);
    }
    let dotnetDir = __dirname;
    [
        'project.json',
        'project.lock.json'
    ].forEach(file => {
        copyFile(file, dotnetDir, folder, isDebug);
    });
}