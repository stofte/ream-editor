@echo off
if "%CI%" == "" (
    set PACKAGE_BASE="build"
    set ELECTRON_OUT="linq-editor-win32-x64"
    set OMNISHARP_ZIP="omnisharp-win-x64-netcoreapp1.zip"
    rem https://github.com/npm/npm/issues/2938#issuecomment-11337463
    rmdir /q /s %PACKAGE_BASE%
)
mkdir %PACKAGE_BASE%
call npm install
call npm run-script lint
call npm run-script ts-build
call npm run-script bundle %PACKAGE_BASE%
copy index.static.html %PACKAGE_BASE%\index.html
copy electron-main.js %PACKAGE_BASE%\electron-main.js
copy omnisharp-setup.js %PACKAGE_BASE%\omnisharp-setup.js
copy package.json %PACKAGE_BASE%\package.json
mkdir %PACKAGE_BASE%\node_modules\bootstrap\dist\fonts
copy node_modules\bootstrap\dist\fonts\*.* %PACKAGE_BASE%\node_modules\bootstrap\dist\fonts\
dotnet restore
dotnet publish --configuration Release --output %PACKAGE_BASE%\query --runtime win7-x64 --framework netcoreapp1.0
copy NuGet.Config %PACKAGE_BASE%\NuGet.Config
copy project.json %PACKAGE_BASE%\project.json
copy project.lock.json %PACKAGE_BASE%\project.lock.json
copy NuGet.Config %PACKAGE_BASE%\query\NuGet.Config
copy project.json %PACKAGE_BASE%\query\project.json
copy project.lock.json %PACKAGE_BASE%\query\project.lock.json
7z x %OMNISHARP_ZIP% -y -o"%PACKAGE_BASE%\omnisharp"
call npm run-script package_electron
