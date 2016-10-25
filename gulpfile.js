const gulp = require('gulp');
const watch = require('gulp-watch');
const concat = require('gulp-concat')
const postcss = require('gulp-postcss');
const sourcemaps = require('gulp-sourcemaps');
const ts = require('gulp-typescript');
const tslint = require('gulp-tslint');
const filter = require('gulp-filter');
const tsProject = ts.createProject('tsconfig.json');
const incrProject = ts.createProject('tsconfig.json');
const inlineb64 = require('postcss-inline-base64');
const cssnano = require('cssnano');
const urlrewrite = require('postcss-urlrewrite');

const DEBUG = !process.env.PACKAGE_BASE;
const output = process.env.PACKAGE_BASE || '.';
 
const cssFiles = [
    'node_modules/normalizecss/normalize.css',
    'node_modules/bootstrap/dist/css/bootstrap.css',
    'node_modules/codemirror/lib/codemirror.css',
    'node_modules/codemirror/addon/lint/lint.css',
    'resources/fonts/source-code-pro/source-code-pro.css',
    'resources/fonts/source-sans-pro/source-sans-pro.css',
    'app/styles/codemirror-hint.css',
    'app/styles/main.css'
];

const tsFiles = 'app/**/*.ts';
const jsFiles = [
    'node_modules/zone.js/dist/zone.js',
    'node_modules/reflect-metadata/Reflect.js',
    'app/**/*.js'
];

const urlRewrites = {
    properties: ['src'],
    rules: [
        { from: /^\.\.\/fonts\/glyphicons/, to: DEBUG ? 'node_modules/bootstrap/dist/fonts/glyphicons' : 'resources/fonts/glyphicons' },
        { from: /^(.*)\/SourceCodePro/, to: 'resources/fonts/source-code-pro/$1/SourceCodePro' },
        { from: /^(.*)\/SourceSansPro/, to: 'resources/fonts/source-sans-pro/$1/SourceSansPro' }
    ]
};

gulp.task('ts:lint', () => {
    // this file contains lots of copy-pastaed data, so we ignore it all
    const f = filter(file => !(/editor-testdata.ts$/.test(file.path)));
    return tsProject.src()
        .pipe(f)
        .pipe(tslint({ formatter: 'prose' }))
        .pipe(tslint.report({ emitError: false }))
        ;
});

gulp.task('ts', () => {
    return tsProject.src()
        .pipe(incrProject())
        .js.pipe(gulp.dest('app'))
        ;
});

gulp.task('css', () => {
    return gulp
        .src(cssFiles)
        .pipe(sourcemaps.init())
        .pipe(concat('bundle.css'))
        .pipe(postcss([
            inlineb64(),
            urlrewrite(urlRewrites),
            // cssnano(),
        ]))
        .pipe(sourcemaps.write('.'))
        .pipe(gulp.dest(output))
        ;
});

gulp.task('watch', () => {
    gulp.watch(cssFiles, ['css']);
    gulp.watch(tsFiles, ['ts:lint', 'ts']);
});

gulp.task('watch:ts', () => {
    gulp.watch(tsFiles, ['ts']);
});

gulp.task('watch:ts:lint', ['ts:lint'], () => {
    gulp.watch(tsFiles, ['ts:lint']);
});

const mainTasks = ['css', 'ts', 'ts:lint'];
gulp.task('build', mainTasks);
gulp.task('default', ['watch', ...mainTasks]);
