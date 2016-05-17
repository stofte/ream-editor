const fs = require('fs');
const path = require('path');
const fileName = path.resolve(`${__dirname}/node_modules/spectron/lib/api.js`);

fs.readFile(fileName, 'utf-8', (err, data) => {
    if (data.indexOf('eletronRequire') !== -1) {
        return;
    }
    const fixed = data.replace(/ require\(/, ' eletronRequire(');
    fs.writeFile(fileName, fixed);
});
console.log('spectron hacked');
