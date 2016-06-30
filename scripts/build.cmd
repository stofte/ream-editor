@echo off
setlocal
if "%CI%" == "" (echo LOCAL BUILD)
if "%CI%" == "" (set PACKAGE_BASE=build)
if "%CI%" == "" (set ELECTRON_OUT=ream-editor-win32-x64)
if "%CI%" == "" (set OMNISHARP_ZIP=omnisharp-win-x64-netcoreapp1.0.zip)
if "%CI%" == "" (rmdir /s /q %ELECTRON_OUT%)
if "%CI%" == "" (rmdir /s /q %PACKAGE_BASE%)
if not "%CI%" == "" (call npm install)
if not "%CI%" == "" (dotnet restore query)
mkdir %PACKAGE_BASE%
rem https://github.com/npm/npm/issues/2938#issuecomment-11337463
call npm run-script gulp-build
call npm run-script bundle
copy index.static.html %PACKAGE_BASE%\index.html
rem electron adds a LICSENSE as well
copy LICENSE %PACKAGE_BASE%\ream-editor-license.txt
copy electron-main.js %PACKAGE_BASE%\electron-main.js
copy omnisharp-setup.js %PACKAGE_BASE%\omnisharp-setup.js
copy package.json %PACKAGE_BASE%\package.json
copy node_modules\zone.js\dist\zone.js %PACKAGE_BASE%\zone.js
copy node_modules\reflect-metadata\Reflect.js %PACKAGE_BASE%\Reflect.js
mkdir %PACKAGE_BASE%\resources\fonts
rem glyphicons
xcopy node_modules\bootstrap\dist\fonts %PACKAGE_BASE%\resources\fonts /R
rem adobe source code/source sans fonts
rem chrome only seems to load 
xcopy resources\fonts\source-code-pro\WOFF2\TTF %PACKAGE_BASE%\resources\fonts\source-code-pro\WOFF2\TTF /S /I /R
xcopy resources\fonts\source-sans-pro\WOFF2\TTF %PACKAGE_BASE%\resources\fonts\source-sans-pro\WOFF2\TTF /S /I /R
rem dotnet publish --configuration Release --output linq-editor-win32-x64\resources\app\query --runtime win7-x64 --framework netcoreapp1.0
dotnet publish query\src\QueryEngine\project.json --configuration Release --output %PACKAGE_BASE%\query --runtime win7-x64 --framework netcoreapp1.0
copy query\NuGet.config %PACKAGE_BASE%\query\NuGet.config
copy query\src\QueryEngine\project.json %PACKAGE_BASE%\query\project.json
copy query\src\QueryEngine\project.lock.json %PACKAGE_BASE%\query\project.lock.json
copy query\src\QueryEngine\appsettings.json %PACKAGE_BASE%\query\appsettings.json
7z x %OMNISHARP_ZIP% -y -o"%PACKAGE_BASE%\omnisharp"
call npm run-script package_electron_win32
endlocal