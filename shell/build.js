var path = require("path");
var Builder = require('systemjs-builder');
var fs = require('fs');
var UglifyJS = require("uglify-js");
var CleanCSS = require('clean-css');
var output = process.argv[2] + '/';

try {
	fs.accessSync(output);
} catch (e) {
	fs.mkdirSync(output);
}

// optional constructor options
// sets the baseURL and loads the configuration file
var builder = new Builder('./', 'systemjs.config.js');
var start = new Date().getTime();
builder
.bundle('app/main.js', output + 'bundle.js')
.then(function() {
	var result = UglifyJS.minify([ output + 'bundle.js' ], {
	    outSourceMap: output + 'bundle.js.map'
	});
	fs.writeFile(output + 'bundle.js.map', result.map, (err) => {
		if (err) throw err;
		fs.writeFile(output + 'bundle.js', result.code, (err) => {
			if (err) throw err;
			var end = new Date().getTime();
		  	console.log('JS finished in', ((end - start) / 1000).toFixed(0), 'seconds');
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
fs.writeFile(output + 'bundle.css.map', css.sourceMap, (err) => {
	if (err) throw err;
	fs.writeFile(output + 'bundle.css', css.styles, (err) => {
		if (err) throw err;
		var end = new Date().getTime();
	  	console.log('CSS finished in', ((end - start) / 1000).toFixed(0), 'seconds');
	});
});

