// sets up a folder for omnisharp to run off of, using the same dependencies 
// as the query program. uses platform appropiate persisted user storage
const fs = require('fs');
const path = require('path');
const { dialog } = require('electron');
const appPath = process.platform !== 'win32' ?
    `${process.env.HOME}/.ream-editor/` :
    `${process.env.LOCALAPPDATA}\\ReamEditor\\`;

function setupOmniSharpDependencies(MODE) {
    const omnisharpPath = path.resolve(appPath + 'omnisharp');
    const query1Prefix = MODE === 'DEVELOPMENT' ? 'query' : 'resources/app/query';
    const query2Prefix = MODE === 'DEVELOPMENT' ? 'query/query/src/ReamQuery' : 'resources/app/query';
    const dependencies = [
        [query1Prefix, 'NuGet.config'], 
        [query2Prefix, 'project.json'],
        [query2Prefix, 'project.lock.json']
    ].map(([prefix, file]) => [path.resolve(`${prefix}/${file}`), file]);

    Promise.all(dependencies.map(([dep, file]) => {
        return copyFile(dep, `${omnisharpPath}/${file}`);
    }))
    .catch(err => {
        console.log('omnisharp-setup failed');
        dialog.showErrorBox('omnisharp-setup failed', err);
    });
}

function copyFile(srcFile, dstFile) {
    return new Promise((done, fail) => {
        fs.exists(srcFile, (exists) => {
            if (!exists) {
                console.log('srcFile not found:', srcFile);
                fail()
            } else {
                var read = fs.createReadStream(srcFile);
                read.on('error', (err) => console.log('read failed', err));
                read.on('end', () => done());
                var write = fs.createWriteStream(dstFile);
                write.on('error', (err) => console.log('write failed', err));
                read.pipe(write);
            }
        });
    });
}

module.exports = setupOmniSharpDependencies;
