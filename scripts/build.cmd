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
if not "%CI%" == "" (call npm run bower-install)
if not "%CI%" == "" (call typings install)
rem stops tslint from linting typings ...
if not "%CI%" == "" (echo {} > typings\tslint.json)
if not "%CI%" == "" (dotnet restore query)
if not "%CI%" == "" (set DOTNET_RUNTIME=win81-x64)
mkdir %PACKAGE_BASE%
if %errorlevel% neq 0 goto errorexit
rem https://github.com/npm/npm/issues/2938#issuecomment-11337463
call npm run gulp-build
if %errorlevel% neq 0 goto errorexit
copy index.static.html %PACKAGE_BASE%\index.html
if %errorlevel% neq 0 goto errorexit
copy bundle.js %PACKAGE_BASE%\bundle.js
if %errorlevel% neq 0 goto errorexit
rem electron adds a LICSENSE as well
copy LICENSE %PACKAGE_BASE%\ream-editor-license.txt
if %errorlevel% neq 0 goto errorexit
copy electron-main.js %PACKAGE_BASE%\electron-main.js
if %errorlevel% neq 0 goto errorexit
copy package.json %PACKAGE_BASE%\package.json
if %errorlevel% neq 0 goto errorexit
xcopy resources %PACKAGE_BASE%\resources\ /S
if %errorlevel% neq 0 goto errorexit
dotnet build query\src\ReamQuery --runtime %DOTNET_RUNTIME%
dotnet publish query\src\ReamQuery --configuration Release --output %PACKAGE_BASE%\query --runtime %DOTNET_RUNTIME%
if %errorlevel% neq 0 goto errorexit
copy query\src\ReamQuery\nlog.config %PACKAGE_BASE%\query\nlog.config
if %errorlevel% neq 0 goto errorexit
7z x %OMNISHARP_ZIP% -y -o"%PACKAGE_BASE%\omnisharp"
if %errorlevel% neq 0 goto errorexit
echo Create a zip dist of reamquery that backends will run against
cd query
7z a %REAMQUERY_ZIP% NuGet.config global.json src\**\*.cs src\**\*.json -r
if %errorlevel% neq 0 goto errorexit
7z d %REAMQUERY_ZIP% bin\ -r
if %errorlevel% neq 0 goto errorexit
7z d %REAMQUERY_ZIP% obj\ -r
if %errorlevel% neq 0 goto errorexit
move %REAMQUERY_ZIP% ..\%PACKAGE_BASE%
if %errorlevel% neq 0 goto errorexit
echo Install startup dependencies
cd ..\scripts\startup
call npm install
if %errorlevel% neq 0 goto errorexit
xcopy node_modules ..\..\%PACKAGE_BASE%\node_modules\ /S /Y
if %errorlevel% neq 0 goto errorexit
copy omnisharp-setup.js ..\..\%PACKAGE_BASE%\omnisharp-setup.js
if %errorlevel% neq 0 goto errorexit
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
:errorexit
echo Build failed
exit /b %errorlevel
