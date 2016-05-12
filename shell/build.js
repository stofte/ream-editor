var path = require("path");
var Builder = require('systemjs-builder');
var fs = require('fs');
var UglifyJS = require("uglify-js2");
var CleanCSS = require('clean-css');

// optional constructor options
// sets the baseURL and loads the configuration file
var builder = new Builder('./', 'systemjs.config.js');
var start = new Date().getTime();
builder
.bundle('app/main.js', 'bundle.js')
.then(function() {
	var result = UglifyJS.minify([ 'bundle.js' ], {
	    outSourceMap: "bundle.js.map"
	});
	fs.writeFile('bundle.js.map', result.map, (err) => {
		if (err) throw err;
		fs.writeFile('bundle.js', result.code, (err) => {
			if (err) throw err;
			var end = new Date().getTime();
		  	console.log('JS finished in', ((end - start) / 1000 / 60).toFixed(3), 'minute(s)');
		});
	});

})
.catch(function(err) {
	console.log('Build error');
	console.log(err);
});


var css = new CleanCSS().minify([
	'node_modules/normalizecss/normalize.css',
	'node_modules/bootstrap/dist/css/bootstrap.css',
	'node_modules/codemirror/lib/codemirror.css',
	'node_modules/codemirror/addon/hint/show-hint.css',
	'styles.css'
]);
fs.writeFile('bundle.css.map', css.sourceMap, (err) => {
	if (err) throw err;
	fs.writeFile('bundle.css', css.styles, (err) => {
		if (err) throw err;
		var end = new Date().getTime();
	  	console.log('CSS finished in', ((end - start) / 1000 / 60).toFixed(3), 'minute(s)');
	});
});

