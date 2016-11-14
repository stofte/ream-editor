#!/usr/bin/env sh
if [ "$TRAVIS" = "" ];
then
    echo "LOCAL BUILD"
    export PACKAGE_BASE=$TRAVIS_BUILD_DIR/build
    export ELECTRON_OUT=ream-editor-linux-x64
    export OMNISHARP_ZIP=omnisharp-ubuntu-x64-netcoreapp1.0.tar.gz
    rm -rf $PACKAGE_BASE
else
    npm install -g gulp-cli
    npm install -g typings
    npm install
    npm run bower-install
    typings install
    echo {} > typings/tslint.json
    dotnet restore query
    export DOTNET_RUNTIME=ubuntu.14.04-x64
fi
export REAMQUERY_ZIP=reamquery.zip
mkdir -p $PACKAGE_BASE
npm run gulp-build
cp index.static.html $PACKAGE_BASE/index.html
cp bundle.js $PACKAGE_BASE/bundle.js
cp LICENSE $PACKAGE_BASE/ream-editor-license.txt
cp electron-main.js $PACKAGE_BASE/electron-main.js
cp package.json $PACKAGE_BASE/package.json
cp -a resources/. $PACKAGE_BASE/resources/
cp query/src/ReamQuery/nlog.config $PACKAGE_BASE/query/nlog.config
tar -xzf $OMNISHARP_ZIP --directory $PACKAGE_BASE/omnisharp
cd query
zip -r $REAMQUERY_ZIP .
cd ..
dotnet build query/src/ReamQuery --runtime $DOTNET_RUNTIME
dotnet publish query/src/ReamQuery --configuration Release --output $PACKAGE_BASE\query --runtime $DOTNET_RUNTIME
# startup deps
cd scripts/startup
npm install
cp -a node_modules/. $PACKAGE_BASE/node_modules/
cp omnisharp-setup.js $PACKAGE_BASE/omnisharp-setup.js
cd ../..
npm run-script package_electron_linux
npm test
cat reamquery-*.log
npm run int-test
