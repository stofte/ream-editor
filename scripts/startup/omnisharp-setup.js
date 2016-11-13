// sets up a folder for omnisharp to run off of, using the same dependencies 
// as the query program. uses platform appropiate persisted user storage
const fs = require('fs');
const path = require('path');
const unzip = require('unzip');

const userStore = process.platform !== 'win32' ?
    `${process.env.HOME}/.ream-editor/` :
    `${process.env.LOCALAPPDATA}\\ReamEditor\\`;

const dstPath = path.resolve(`${userStore}/dist`);
const zipPath = path.resolve(`${__dirname}/reamquery.zip`);

// todo error handling and all that
rmdir(dstPath, () => {
    fs.createReadStream(zipPath)
        .pipe(unzip.Extract({ path: dstPath }));
});

// https://gist.github.com/tkihira/2367067
function rmdir(dir, cb) {
	var list = fs.readdirSync(dir);
	for(var i = 0; i < list.length; i++) {
		var filename = path.join(dir, list[i]);
		var stat = fs.statSync(filename);
		
		if(filename == "." || filename == "..") {
			// pass these files
		} else if(stat.isDirectory()) {
			// rmdir recursively
			rmdir(filename);
		} else {
			// rm fiilename
			fs.unlinkSync(filename);
		}
	}
	fs.rmdirSync(dir);
    if (cb) {
        cb();
    }
}
