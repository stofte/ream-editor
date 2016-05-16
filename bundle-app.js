var path = require('path');
var Builder = require('systemjs-builder');
var fs = require('fs');
var UglifyJS = require('uglify-js');
var CleanCSS = require('clean-css');
var output = (process.argv[2] || 'build') + '/';

try {
	fs.accessSync(output);
} catch (e) {
	fs.mkdirSync(output);
}

const jsOutput = output + 'bundle.js';
const cssOutput = output + 'bundle.css';

// optional constructor options
// sets the baseURL and loads the configuration file
var builder = new Builder('./', 'systemjs.config.js');
var start = new Date().getTime();
builder
.buildStatic('app/main.js', jsOutput)
.then(function() {
	console.log('systemjs bundle in', ((new Date().getTime() - start) / 1000).toFixed(0), 'seconds')
	// include the other required stuff
	var result = UglifyJS.minify([ 
        'node_modules/es6-shim/es6-shim.min.js',
        'node_modules/zone.js/dist/zone.js',
        'node_modules/reflect-metadata/Reflect.js',
		jsOutput 
	], {
	    outSourceMap: jsOutput + '.map'
	});
	fs.writeFile(jsOutput + '.map', result.map, (err) => {
		if (err) throw err;
		fs.writeFile(jsOutput, result.code, (err) => {
			if (err) throw err;
			var end = new Date().getTime();
		  	console.log(jsOutput, 'written in', ((end - start) / 1000).toFixed(0), 'seconds');
		});
	});

})
.catch(function(err) {
	console.log('JS build error');
	console.log(err);
});

var css = new CleanCSS().minify([
	'node_modules/normalizecss/normalize.css',
	'node_modules/bootstrap/dist/css/bootstrap.css',
	'node_modules/codemirror/lib/codemirror.css',
	'node_modules/codemirror/addon/hint/show-hint.css',
	'styles.css'
]);
fs.writeFile(cssOutput + '.map', css.sourceMap, (err) => {
	if (err) throw err;
	fs.writeFile(cssOutput, css.styles, (err) => {
		if (err) throw err;
		var end = new Date().getTime();
	  	console.log(cssOutput, 'written in', ((end - start) / 1000).toFixed(0), 'seconds');
	});
});

