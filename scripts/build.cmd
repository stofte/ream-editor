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
if %errorlevel% neq 0 exit /b %errorlevel%
rem https://github.com/npm/npm/issues/2938#issuecomment-11337463
call npm run-script gulp-build
if %errorlevel% neq 0 exit /b %errorlevel%
copy index.static.html %PACKAGE_BASE%\index.html
if %errorlevel% neq 0 exit /b %errorlevel%
copy bundle.js %PACKAGE_BASE%\bundle.js
if %errorlevel% neq 0 exit /b %errorlevel%
rem electron adds a LICSENSE as well
copy LICENSE %PACKAGE_BASE%\ream-editor-license.txt
if %errorlevel% neq 0 exit /b %errorlevel%
copy electron-main.js %PACKAGE_BASE%\electron-main.js
if %errorlevel% neq 0 exit /b %errorlevel%
copy omnisharp-setup.js %PACKAGE_BASE%\omnisharp-setup.js
if %errorlevel% neq 0 exit /b %errorlevel%
copy package.json %PACKAGE_BASE%\package.json
if %errorlevel% neq 0 exit /b %errorlevel%
xcopy resources %PACKAGE_BASE%\resources\ /S
if %errorlevel% neq 0 exit /b %errorlevel%
rem dotnet publish --configuration Release --output linq-editor-win32-x64\resources\app\query --runtime win7-x64 --framework netcoreapp1.0
dotnet publish query\query\src\ReamQuery\project.json --configuration Release --output %PACKAGE_BASE%\query --runtime win10-x64 --framework netcoreapp1.0
if %errorlevel% neq 0 exit /b %errorlevel%
copy query\query\NuGet.config %PACKAGE_BASE%\query\NuGet.config
if %errorlevel% neq 0 exit /b %errorlevel%
copy query\query\src\ReamQuery\project.json %PACKAGE_BASE%\query\project.json
if %errorlevel% neq 0 exit /b %errorlevel%
copy query\query\src\ReamQuery\project.lock.json %PACKAGE_BASE%\query\project.lock.json
if %errorlevel% neq 0 exit /b %errorlevel%
copy query\query\src\ReamQuery\nlog.config %PACKAGE_BASE%\query\nlog.config
if %errorlevel% neq 0 exit /b %errorlevel%
7z x %OMNISHARP_ZIP% -y -o"%PACKAGE_BASE%\omnisharp"
if %errorlevel% neq 0 exit /b %errorlevel%
call npm run-script package_electron_win32
endlocal
