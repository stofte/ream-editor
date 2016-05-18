const fs = require('fs');
const path = require('path');
const fileName = path.resolve(`${__dirname}/node_modules/spectron/lib/api.js`);

console.log('patching', fileName);

// see https://github.com/electron/spectron/issues/29
fs.readFile(fileName, 'utf-8', (err, data) => {
    if (data.indexOf(' require(\'') !== -1) {
        const fixed = data.replace(/ require\(/g, ' electronRequire(');
        fs.writeFile(fileName, fixed);
        console.log('spectron hack performed');
    } else {
        console.log('spectron hack already done');
    }
});
