@echo off
setlocal
set REAMQUERY_ZIP=reamquery.zip
rem CI builds must provide this file as well
set OMNISHARP_ZIP=omnisharp-win-x64-netcoreapp1.0.zip
if "%CI%" == "" (echo LOCAL BUILD)
if "%CI%" == "" (set PACKAGE_BASE=build)
if "%CI%" == "" (set ELECTRON_OUT=ream-editor-win32-x64)
if "%CI%" == "" (set DOTNET_RUNTIME=win10-x64)
if "%CI%" == "" (rmdir /s /q %ELECTRON_OUT%)
if "%CI%" == "" (rmdir /s /q %PACKAGE_BASE%)
if not "%CI%" == "" (call npm install -g gulp-cli)
if not "%CI%" == "" (call npm install -g typings)
if not "%CI%" == "" (call npm install)
if not "%CI%" == "" (call typings install)
rem stops tslint from linting typings ...
if not "%CI%" == "" (echo {} > typings\tslint.json)
if not "%CI%" == "" (dotnet restore query)
if not "%CI%" == "" (set DOTNET_RUNTIME=win81-x64)
mkdir %PACKAGE_BASE%
rem https://github.com/npm/npm/issues/2938#issuecomment-11337463
call npm run gulp-build
copy index.static.html %PACKAGE_BASE%\index.html
copy bundle.js %PACKAGE_BASE%\bundle.js
rem electron adds a LICSENSE as well
copy LICENSE %PACKAGE_BASE%\ream-editor-license.txt
copy electron-main.js %PACKAGE_BASE%\electron-main.js
copy package.json %PACKAGE_BASE%\package.json
xcopy resources %PACKAGE_BASE%\resources\ /S
dotnet build query\src\ReamQuery --runtime %DOTNET_RUNTIME%
dotnet publish query\src\ReamQuery --configuration Release --output %PACKAGE_BASE%\query --runtime %DOTNET_RUNTIME%
copy query\src\ReamQuery\nlog.config %PACKAGE_BASE%\query\nlog.config
7z x %OMNISHARP_ZIP% -y -o"%PACKAGE_BASE%\omnisharp"
echo Create a zip dist of reamquery that backends will run against
cd query
7z a %REAMQUERY_ZIP% NuGet.config global.json src\**\*.cs src\**\*.json -r
7z d %REAMQUERY_ZIP% bin\ -r
7z d %REAMQUERY_ZIP% obj\ -r
move %REAMQUERY_ZIP% ..\%PACKAGE_BASE%
echo Install startup dependencies
cd ..\scripts\startup
call npm install
xcopy node_modules ..\..\%PACKAGE_BASE%\node_modules\ /S /Y
copy omnisharp-setup.js ..\..\%PACKAGE_BASE%\omnisharp-setup.js
cd ..\..
call npm run-script package_electron_win32
echo starting unit tests
call npm run test
del reamquery-*.log
echo starting build test
call npm run int-test
dir %ELECTRON_OUT%
dir %ELECTRON_OUT%\resources
dir %ELECTRON_OUT%\resources\app
dir %ELECTRON_OUT%\resources\app\query
type %ELECTRON_OUT%\resources\app\query\reamquery-*.log
type reamquery-*.log
echo LOCALAPPDATA looks like
dir %LOCALAPPDATA%\ReamEditor\dist
endlocal
echo Build success
exit /b 0
