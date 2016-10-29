const http = require('http');
const fs = require('fs');
const host = process.argv[2];
const path = process.argv[3];

loadStylesheet(host, path, bodyText => {
    let txt = bodyText;
    const fnMatches = txt.match(/font-family: '([^']+)'/);
    const fontName = fnMatches[1];
    const nameCounts = {};
    const replacements = {};
    const encRegex = /\/\*\W([^\W]+)\W\*\//;
    const urlRegex = /url\(([^\)]+)\)/;
    const nameRegex = /\,\Wlocal\('([^')]+)'\)/;
    while(txt.indexOf('/*') !== -1) {
        const encs = txt.match(encRegex);
        const urls = txt.match(urlRegex);
        const names = txt.match(nameRegex);
        if (!nameCounts[names[1]]) {
            nameCounts[names[1]] = 0;
        }
        replacements[urls[1]] = `${names[1]}-${encs[1]}-${nameCounts[names[1]]++}.woff2`;
        txt = txt.substring(1);
        txt = txt.substring(txt.indexOf('/*') - 3);
    }

    const script = buildCurlScript(replacements);
    const css = mapStylesheet(bodyText, replacements);
    fs.writeFile(`curl-${fontName}.bat`, script);
    fs.writeFile(`${fontName}.css`, css);
    console.log(`Finished "${fnMatches[1]}"`);
});

function buildCurlScript(replacements) {
    const urls = Object.keys(replacements);
    const script = urls.map(url => `curl -o ${replacements[url]} ${url}`).join('\n');
    return script;
}

function mapStylesheet(bodyText, replacements) {
    const urls = Object.keys(replacements);
    let txt = bodyText;
    urls.forEach(url => {
        txt = txt.replace(url, replacements[url]);
    });
    return txt;
}

function loadStylesheet(host, path, cb) {
    return http.get({
        host, path, headers: {'user-agent': 'Mozilla/5.0 AppleWebKit/537.36 Chrome/54.0.2840.71'},
    }, (res) => {
        let str = '';
        res.on('data', chunk => str += chunk);
        res.on('end', () => {
            cb(str);
        });
    });
}
