const gulp = require('gulp');
const watch = require('gulp-watch');
const sass = require('gulp-sass');
const concat = require('gulp-concat')
const postcss = require('gulp-postcss');
const sourcemaps = require('gulp-sourcemaps');
const systemjsBuilder = require('gulp-systemjs-builder');
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
    'node_modules/material-design-lite/material.min.css',
    'node_modules/material-design-lite/dist/material.light_blue-yellow.min.css',
    'app/**/*.css'
];

const tsFiles = 'app/**/*.ts';
const sassFiles = 'app/**/*.scss';
const jsFiles = [
    'node_modules/zone.js/dist/zone.js',
    'node_modules/reflect-metadata/Reflect.js',
    'app/**/*.js'
];

const urlRewrites = {
    properties: ['src'],
    rules: [
        { from: /^(.*)\/(resources\/materialicons\/MaterialIcons-Regular.woff2?)/, to: '$2'}
    ]
};

gulp.task('ts', () => {
    return tsProject.src()
        .pipe(incrProject())
        .js.pipe(gulp.dest('app'))
        ;
});

gulp.task('ts:lint', ['ts'], () => {
    // this file contains lots of copy-pastaed data, so we ignore it all
    const f = filter(file => !/editor-testdata.ts$/.test(file.path) &&
                             !/bower_components/.test(file.path));
    return tsProject.src()
        .pipe(f)
        .pipe(tslint({ formatter: 'prose' }))
        .pipe(tslint.report({ emitError: false }))
        ;
});

gulp.task('ts:bundle', ['ts'], () => {
    const builder = systemjsBuilder();
    builder.loadConfigSync('./systemjs.config.js')
    return builder.buildStatic('app/main.js', 'bundle.js', {
            minify: false,
            mangle: false
        })
        .pipe(gulp.dest('./'));
});

gulp.task('sass', function () {
    return gulp.src(sassFiles)
        .pipe(sass().on('error', sass.logError))
        .pipe(gulp.dest('app'));
});

gulp.task('css', ['sass'], () => {
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
    gulp.watch(sassFiles, ['sass', 'css']);
    gulp.watch(tsFiles, ['ts:lint', 'ts']);
});

const mainTasks = ['sass', 'css', 'ts', 'ts:lint'];
const buildOnlyTasks = ['ts:bundle'];
gulp.task('build', [...mainTasks, ...buildOnlyTasks]);
gulp.task('default', [...mainTasks, 'watch']);
