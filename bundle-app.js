var path = require('path');
var Builder = require('systemjs-builder');
var fs = require('fs');
var UglifyJS = require('uglify-js');
var CleanCSS = require('clean-css');
// if building locally (without params), build right into electron folder
var output = (process.argv[2] || 'linq-editor-win32-x64/resources/app') + '/';

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
	console.log('systemjs bundled in', ((new Date().getTime() - start) / 1000).toFixed(0), 'seconds')
	// // include the other required stuff
	// var result = UglifyJS.minify([
    //     'node_modules/zone.js/dist/zone.js',
    //     'node_modules/reflect-metadata/Reflect.js',
	// 	jsOutput
	// ], {
	//     outSourceMap: jsOutput + '.map'
	// });
	// fs.writeFile(jsOutput + '.map', result.map, (err) => {
	// 	if (err) throw err;
	// 	fs.writeFile(jsOutput, result.code, (err) => {
	// 		if (err) throw err;
	// 		var end = new Date().getTime();
	// 	  	console.log(jsOutput, 'written in', ((end - start) / 1000).toFixed(0), 'seconds');
	// 	});
	// });

})
.catch(function(err) {
	console.log('JS build error');
	console.log(err);
});
