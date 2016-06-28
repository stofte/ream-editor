#!/usr/bin/env sh
if [ "$TRAVIS" = "" ];
then
    echo "LOCAL BUILD"
    export PACKAGE_BASE=build
    export ELECTRON_OUT=ream-editor-linux-x64
    export OMNISHARP_ZIP=omnisharp-ubuntu-x64-netcoreapp1.0.tar.gz
    rm -rf $PACKAGE_BASE
else
    npm install
    dotnet restore query
fi
mkdir -p $PACKAGE_BASE/omnisharp
mkdir -p $PACKAGE_BASE/resources/fonts/source-code-pro/WOFF2/TTF
mkdir -p $PACKAGE_BASE/resources/fonts/source-sans-pro/WOFF2/TTF
npm run-script gulp-build
npm run-script bundle
cp index.static.html $PACKAGE_BASE/index.html
# electron adds a LICSENSE as well
cp LICENSE $PACKAGE_BASE/ream-editor-license.txt
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
#dotnet publish --configuration Release --output ream-editor-ubuntu-x64/resources/app/query --runtime ubuntu.14.04-x64 --framework netcoreapp1.0
dotnet publish query/project.json --configuration Release --output $PACKAGE_BASE/query --runtime ubuntu.14.04-x64 --framework netcoreapp1.0
cp query/NuGet.config $PACKAGE_BASE/query/NuGet.config
cp query/project.json $PACKAGE_BASE/query/project.json
cp query/project.lock.json $PACKAGE_BASE/query/project.lock.json
tar -xzf $OMNISHARP_ZIP --directory $PACKAGE_BASE/omnisharp
npm run-script package_electron_linux

