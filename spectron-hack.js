const fs = require('fs');
const path = require('path');
const fileName = path.resolve(`${__dirname}/node_modules/spectron/lib/api.js`);

// see https://github.com/electron/spectron/issues/29
fs.readFile(fileName, 'utf-8', (err, data) => {
    if (data.indexOf('electronRequire') !== -1) {
        const fixed = data.replace(/ require\(/, ' electronRequire(');
        fs.writeFile(fileName, fixed);        
    }
    console.log('spectron hack active');
});
