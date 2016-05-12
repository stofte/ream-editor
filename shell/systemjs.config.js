(function(global) {

    // map tells the System loader where to look for things
    var map = {
        'app': 'app', // 'dist',
        'rxjs': 'node_modules/rxjs',
        'angular2-in-memory-web-api': 'node_modules/angular2-in-memory-web-api',
        '@angular': 'node_modules/@angular',
        'ng2-bootstrap': 'node_modules/ng2-bootstrap',
        'moment': 'node_modules/moment',
        'codemirror': 'node_modules/codemirror',
        'lodash': 'node_modules/lodash',
        'node-uuid': 'node_modules/node-uuid',
    };

    // packages tells the System loader how to load when no filename and/or no extension
    var packages = {
        'app': { main: 'main.js',  defaultExtension: 'js' },
        'rxjs': { defaultExtension: 'js' },
        'angular2-in-memory-web-api': { defaultExtension: 'js' },
        'ng2-bootstrap': { main: 'ng2-bootstrap', defaultExtension: 'js' },
        'moment': { main: 'moment.js', defaultExtension: 'js' },
        'codemirror': { main: 'lib/codemirror.js', defaultExtension: 'js' },
        'lodash': { main: 'lodash.js', defaultExtension: 'js' },
        'node-uuid': { main: 'uuid.js', defaultExtension: 'js' },
    };

    var packageNames = [
        '@angular/common',
        '@angular/compiler',
        '@angular/core',
        '@angular/http',
        '@angular/platform-browser',
        '@angular/platform-browser-dynamic',
        '@angular/router-deprecated',
        '@angular/testing',
        '@angular/upgrade',
    ];

    // add package entries for angular packages in the form '@angular/common': { main: 'index.js', defaultExtension: 'js' }
    packageNames.forEach(function(pkgName) {
        packages[pkgName] = { main: 'index.js', defaultExtension: 'js' };
    });

    var config = {
        map: map,
        packages: packages
    }

    System.config(config);

})(this);
