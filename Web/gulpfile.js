/// <binding ProjectOpened='.build, watch' />
'use strict';

const config = {
    types: {
        js: 'js',
        less: 'less',
        scss: 'scss',
        svg: 'svg',
        cshtml:'cshtml'
    }
};

const gulp = require("gulp"),
    concat = require("gulp-concat"),
    uglify = require("gulp-uglify"),
    merge = require("merge-stream"),
    babel = require('gulp-babel'),    
    clean = require('gulp-clean'),
    less = require('gulp-less'),
    replaceExt = require('replace-ext'),
    cleanCSS = require('gulp-clean-css'),
    sass = require('gulp-sass'),
    cleanhtml = require('gulp-cleanhtml'),
    htmlmin = require('gulp-htmlmin'),
    bundleconfig = require("./bundleconfig.json");

//_________________________________________
//builds
//_________________________________________
gulp.task(".clean", ['clean:js', 'clean:less', 'clean:scss']);
gulp.task(".build", ['build:js', 'build:less', 'build:scss', 'min:cshtml']);

gulp.task('watch:js', function () {
    var inputFiles = [];
    for (var bundle of bundleconfig[config.types.js]) {
        for (var inp of bundle.inputFiles) {
            inputFiles.push(inp);
        }
    }
    gulp.watch(inputFiles,['min:js']);
});

gulp.task('watch:less', function () {
    var inputFiles = [];
    for (var bundle of bundleconfig[config.types.less]) {
        for (var inp of bundle.inputFiles) {
            inputFiles.push(inp);
        }
    }
    gulp.watch(inputFiles, ['min:less']);
});

gulp.task('watch:scss', function () {
    var inputFiles = [];
    for (var bundle of bundleconfig[config.types.scss]) {
        for (var inp of bundle.inputFiles) {
            inputFiles.push(inp);
        }
    }
    gulp.watch(inputFiles, ['min:scss']);
});

gulp.task('watch:cshtml', function () {
    var inputFiles = [];
    for (var bundle of bundleconfig[config.types.cshtml]) {
        for (var inp of bundle.inputFiles) {
            inputFiles.push(inp);
        }
    }
    gulp.watch(inputFiles,['min:cshtml']);
});

gulp.task('watch', ['watch:js', 'watch:scss','watch:less', 'watch:cshtml']);

//_________________________________________
//JS
//_________________________________________
gulp.task("min:js", function () {
    var tasks = bundleconfig[config.types.js].map(function (bundle) {
        //return gulp.src(bundle.inputFiles, { base: "." })
        //    .pipe(babel({
        //        presets: [
        //            ["es2015", { "modules": false }]
        //        ]
        //    }))
        //    .pipe(concat(bundle.outputFileName))
        //    .pipe(uglify())
        //    .pipe(gulp.dest('.'));

        return gulp.src(bundle.inputFiles, { base: "." })
            .pipe(babel({
                presets: [
                    ["es2015", { "modules": false }]
                ]
            }))
            .pipe(concat(bundle.outputFileName))
            .pipe(gulp.dest('.'))
            //min
            .pipe(uglify())
            .pipe(concat(replaceExt(bundle.outputFileName, '.min.js')))
            .pipe(gulp.dest('.'));

    });
    return merge(tasks);
});

gulp.task("clean:js", function () {
    var tasks = bundleconfig[config.types.js].map(function (bundle) {
        return gulp.src(bundle.outputFileName)
            .pipe(clean());
    });
    return merge(tasks);
});

gulp.task('build:js', ['clean:js', 'min:js']);

//_________________________________________
//LESS
//_________________________________________
gulp.task("min:less", function () {
    var tasks = bundleconfig[config.types.less].map(function (bundle) {
        return gulp.src(bundle.inputFiles, { base: "." })
            .pipe(less())
            .pipe(concat(bundle.outputFileName))
            .pipe(gulp.dest('.'))
            //min
            .pipe(cleanCSS())
            .pipe(concat(replaceExt(bundle.outputFileName, '.min.css')))
            .pipe(gulp.dest('.'));
    });
    return merge(tasks);
});

gulp.task("clean:less", function () {
    var tasks = bundleconfig[config.types.less].map(function (bundle) {
        var css = gulp.src(bundle.outputFileName)
            .pipe(clean());
        var min = gulp.src(replaceExt(bundle.outputFileName, '.min.css'))
            .pipe(clean());
        return merge([css, min]);
    });
    return merge(tasks);
});

gulp.task('build:less', ['clean:less', 'min:less']);

//_________________________________________
//SCSS
//_________________________________________
gulp.task("min:scss", function () {
    var tasks = bundleconfig[config.types.scss].map(function (bundle) {
        return gulp.src(bundle.inputFiles, { base: "." })
            .pipe(sass())
            .pipe(concat(bundle.outputFileName))
            .pipe(gulp.dest('.'))
            //min
            .pipe(cleanCSS())
            .pipe(concat(replaceExt(bundle.outputFileName, '.min.css')))
            .pipe(gulp.dest('.'));
    });
    return merge(tasks);
});

gulp.task("clean:scss", function () {
    var tasks = bundleconfig[config.types.scss].map(function (bundle) {
        var css = gulp.src(bundle.outputFileName)
            .pipe(clean());
        var min = gulp.src(replaceExt(bundle.outputFileName, '.min.css'))
            .pipe(clean());
        return merge([css, min]);
    });
    return merge(tasks);
});

gulp.task('build:scss', ['clean:scss', 'min:scss']);


//_________________________________________
//SVG
//_________________________________________

gulp.task('min:svg', function () {
    var tasks = bundleconfig[config.types.svg].map(function (bundle) {
        return gulp.src(bundle.inputFiles, { base: "." })
            .pipe(cleanhtml())
            .pipe(concat(bundle.outputFileName))
            .pipe(gulp.dest('.'));
    });
    return merge(tasks);
});

//_________________________________________
//cshtml
//_________________________________________

gulp.task('min:cshtml', function () {
    var tasks = bundleconfig[config.types.cshtml].map(function (bundle) {
        return gulp.src(bundle.inputFiles, { base: "." })
            .pipe(htmlmin({collapseWhitespace: true}))
            .pipe(concat(bundle.outputFileName))
            .pipe(gulp.dest('.'));
    });
    return merge(tasks);
});