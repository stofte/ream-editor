const gulp = require('gulp');
const watch = require('gulp-watch');
const concat = require('gulp-concat')
const postcss = require('gulp-postcss');
const sourcemaps = require('gulp-sourcemaps');
const inlineb64 = require('postcss-inline-base64');
const cssnano = require('cssnano');
const urlrewrite = require('postcss-urlrewrite');

const DEBUG = process.env.NODE_ENV !== 'PRODUCTION';

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

const urlRewrites = {
    properties: ['src'],
    rules: [
        {
            from: /^\.\.\/fonts\/glyphicons/,
            to: DEBUG ? 'node_modules/bootstrap/dist/fonts/glyphicons' : 'fonts/glyphicons'
        },
        {
            from: /^(.*)\/SourceCodePro/,
            to: DEBUG ? 'resources/fonts/source-code-pro/$1/SourceCodePro' : 'fonts/source-code-pro/$1/SourceCodePro'
        },
        {
            from: /^(.*)\/SourceSansPro/,
            to: DEBUG ? 'resources/fonts/source-sans-pro/$1/SourceSansPro' : 'fonts/source-sans-pro/$1/SourceSansPro'
        }
    ]
};

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
        .pipe(gulp.dest('.'))
});

gulp.task('watch', () => {
    gulp.watch(cssFiles, ['css']);
});

gulp.task('build', ['css']);

gulp.task('default', ['watch', 'css']);
