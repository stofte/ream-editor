#!/usr/bin/env sh
if [ $TRAVIS -neq "true"  ]; then
    export PACKAGE_BASE=build
    export ELECTRON_OUT=linq-editor-ubuntu-x64
    export OMNISHARP_ZIP=omnisharp-ubuntu-x64-netcoreapp1.0.tar.gz
    rm -rf $PACKAGE_BASE
fi
// todo must be an argument
mkdir $PACKAGE_BASE
mkdir $PACKAGE_BASE/omnisharp
mkdir $PACKAGE_BASE/resources
mkdir $PACKAGE_BASE/resources/fonts
mkdir $PACKAGE_BASE/resources/fonts/source-code-pro
mkdir $PACKAGE_BASE/resources/fonts/source-code-pro/WOFF2
mkdir $PACKAGE_BASE/resources/fonts/source-code-pro/WOFF2/TTF
mkdir $PACKAGE_BASE/resources/fonts/source-sans-pro
mkdir $PACKAGE_BASE/resources/fonts/source-sans-pro/WOFF2
mkdir $PACKAGE_BASE/resources/fonts/source-sans-pro/WOFF2/TTF
npm install
npm run-script gulp-build
npm run-script bundle
cp index.static.html $PACKAGE_BASE/index.html
# electron adds a LICSENSE as well
cp LICENSE $PACKAGE_BASE/linq-editor-license.txt
cp electron-main.js $PACKAGE_BASE/electron-main.js
cp omnisharp-setup.js $PACKAGE_BASE/omnisharp-setup.js
cp package.json $PACKAGE_BASE/package.json
cp node_modules/zone.js/dist/zone.js $PACKAGE_BASE/zone.js
cp node_modules/reflect-metadata/Reflect.js $PACKAGE_BASE/Reflect.js
# glyphicons
cp node_modules/bootstrap/dist/fonts/* $PACKAGE_BASE/resources/fonts
# adobe source code/source sans fonts
# chrome only seems to load 
cp -r resources/fonts/source-code-pro/WOFF2/TTF/* $PACKAGE_BASE/resources/fonts/source-code-pro/WOFF2/TTF
cp -r resources/fonts/source-sans-pro/WOFF2/TTF/* $PACKAGE_BASE/resources/fonts/source-sans-pro/WOFF2/TTF
dotnet restore
dotnet publish --configuration Release --output $PACKAGE_BASE/query --runtime ubuntu.14.04-x64 --framework netcoreapp1.0
#dotnet publish --configuration Release --output linq-editor-ubuntu-x64/resources/app/query --runtime ubuntu.14.04-x64 --framework netcoreapp1.0
cp project.json $PACKAGE_BASE/project.json
cp project.lock.json $PACKAGE_BASE/project.lock.json
cp project.json $PACKAGE_BASE/query/project.json
cp project.lock.json $PACKAGE_BASE/query/project.lock.json
tar -xzf $OMNISHARP_ZIP --directory $PACKAGE_BASE/omnisharp
npm run-script package_electron_linux
