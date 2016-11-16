(function(global) {

    // map tells the System loader where to look for things
    var map = {
        'app': 'app', // 'dist',

        '@angular/core' : 'node_modules/@angular/core/bundles/core.umd.js',
        '@angular/common' : 'node_modules/@angular/common/bundles/common.umd.js',
        '@angular/compiler' : 'node_modules/@angular/compiler/bundles/compiler.umd.js',
        '@angular/platform-browser' : 'node_modules/@angular/platform-browser/bundles/platform-browser.umd.js',
        '@angular/platform-browser-dynamic' : 'node_modules/@angular/platform-browser-dynamic/bundles/platform-browser-dynamic.umd.js',
        '@angular/http' : 'node_modules/@angular/http/bundles/http.umd.js',
        '@angular/router' : 'node_modules/@angular/router/bundles/router.umd.js',
        '@angular/forms' : 'node_modules/@angular/forms/bundles/forms.umd.js',

        'rxjs': 'node_modules/rxjs',
        '@angular': 'node_modules/@angular',
        'moment': 'node_modules/moment',
        'codemirror': 'node_modules/codemirror',
        'lodash': 'node_modules/lodash',
        'node-uuid': 'node_modules/node-uuid',
        'angular-sortablejs': 'node_modules/angular-sortablejs',
        'sortablejs': 'node_modules/sortablejs/Sortable.js',
        // hypergrid deps:
        'fin-hypergrid': 'node_modules/fin-hypergrid',
        'finbars': 'node_modules/finbars',
        'fincanvas': 'node_modules/fincanvas',
        'rectangular': 'node_modules/rectangular',
        'object-iterators': 'node_modules/object-iterators',
        'lru-cache': 'node_modules/lru-cache',
        'sparse-boolean-array': 'node_modules/sparse-boolean-array',
        'css-injector': 'node_modules/css-injector',
        'automat': 'node_modules/automat',
        'extend-me': 'node_modules/extend-me',
        'mustache': 'node_modules/mustache',
        'pop-menu': 'node_modules/pop-menu',
        'hyper-analytics': 'node_modules/hyper-analytics',
        'filter-tree': 'node_modules/filter-tree',
        'list-dragon': 'node_modules/list-dragon',
        'tabz': 'node_modules/tabz',
        'templex': 'node_modules/templex',
        'unstrungify': 'node_modules/unstrungify',
        'regexp-like': 'node_modules/regexp-like'
    };

    // packages tells the System loader how to load when no filename and/or no extension
    var packages = {
        'app': { main: 'main.js',  defaultExtension: 'js' },
        'rxjs': { defaultExtension: 'js' },
        'moment': { main: 'moment.js', defaultExtension: 'js' },
        'codemirror': { main: 'lib/codemirror.js', defaultExtension: 'js' },
        'lodash': { main: 'lodash.js', defaultExtension: 'js' },
        'node-uuid': { main: 'uuid.js', defaultExtension: 'js' },
        'angular-sortablejs': { main: 'index.js', defaultExtension: 'js' },
        'fin-hypergrid': {
            main: './src/Hypergrid.js',
            format: 'cjs',
            map: {
                './src/behaviors': './src/behaviors/index.js',
                './src/dialogs': './src/dialogs/index.js',
                './src/features': './src/features/index.js',
                './src/cellRenderers': './src/cellRenderers/index.js',
                './src/cellEditors': './src/cellEditors/index.js',
                './images': './images/index.js',
                './css': './css/index.js',
                './html': './html/index.js',
            }
        },
        'finbars': { main: 'index.js' },
        'fincanvas': { main: 'index.js' },
        'rectangular': { main: 'index.js' },
        'object-iterators': { main: 'index.js' },
        'lru-cache': { main: 'lib/lru-cache.js' },
        'sparse-boolean-array': { main: 'index.js' },
        'css-injector': { main: 'index.js' },
        'automat': { main: 'index.js' },
        'extend-me': { main: 'index.js' },
        'mustache': { main: 'mustache.js' },
        'pop-menu': { main: 'index.js' },
        'hyper-analytics': { main: 'index.js' },
        'filter-tree': {
            main: './index.js',
            map: {
                './html': './html/index.js'
            }
        },
        'list-dragon': { main: 'index.js' },
        'tabz': { main: 'index.js' },
        'templex': { main: 'index.js' },
        'unstrungify': { main: 'index.js' },
        'regexp-like': { main: 'index.js' }
    };

    var config = {
        map: map,
        packages: packages
    }

    System.config(config);

})(this);
