if (-Not($env:CI)){
    $env:PACKAGE_BASE="build"
    $env:ELECTRON_OUT="linq-editor-win32-x64"
    $env:OMNISHARP_ZIP="omnisharp-win-x64-netcoreapp1.zip"
    remove-item $env:PACKAGE_BASE -recurse -Force | Out-Null
}
mkdir -Force $env:PACKAGE_BASE | Out-Null
npm install
npm run lint
npm run ts-build
npm run bundle $env:PACKAGE_BASE
copy index.static.html $env:PACKAGE_BASE\index.html
copy electron-main.js $env:PACKAGE_BASE\electron-main.js
copy omnisharp-setup.js $env:PACKAGE_BASE\omnisharp-setup.js
copy package.json $env:PACKAGE_BASE\package.json
# bootstrap fonts
mkdir -Force $env:PACKAGE_BASE\node_modules\bootstrap\dist\fonts | Out-Null
copy node_modules\bootstrap\dist\fonts\*.* $env:PACKAGE_BASE\node_modules\bootstrap\dist\fonts\
dotnet restore
dotnet publish --configuration Release --output $env:PACKAGE_BASE\query --runtime win7-x64 --framework netcoreapp1.0
# these files are used by omnisharp to simulate a project structure.
# these will be copied to a folder in APPDATA, which omnisharp is pointed at. 
copy NuGet.Config $env:PACKAGE_BASE\NuGet.Config
copy project.json $env:PACKAGE_BASE\project.json
copy project.lock.json $env:PACKAGE_BASE\project.lock.json
# include omnisharp in dist
7z x $env:OMNISHARP_ZIP -y -o"$env:PACKAGE_BASE\omnisharp"
# bundle everything in the package folder
npm run package_electron
