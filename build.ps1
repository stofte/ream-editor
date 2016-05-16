mkdir -Force $env:PACKAGE_BASE | Out-Null
if (-Not($env:CI)){
    $env:PACKAGE_BASE="build"
    $env:ELECTRON_OUT="linq-editor-win32-x64"
    $env:OMNISHARP_ZIP="omnisharp-win-x64-netcoreapp1.0.zip"
    remove-item $env:PACKAGE_BASE -recurse -Force | Out-Null
    remove-item $env:ELECTRON_OUT -recurse -Force | Out-Null
}
npm install
npm run lint
npm run ts-build
npm run bundle $env:PACKAGE_BASE
copy index.static.html $env:PACKAGE_BASE\index.html
copy electron-main.js $env:PACKAGE_BASE\electron-main.js
copy omnisharp-setup.js $env:PACKAGE_BASE\omnisharp-setup.js
copy package.json $env:PACKAGE_BASE\package.json
dotnet restore
dotnet publish --configuration Release --output $env:PACKAGE_BASE\query
# these files are used by omnisharp to simulate a project structure.
# these will be copied to a folder in APPDATA, which omnisharp is pointed at. 
copy NuGet.Config $env:PACKAGE_BASE\query\NuGet.Config
copy project.json $env:PACKAGE_BASE\query\project.json
copy project.lock.json $env:PACKAGE_BASE\query\project.lock.json
# include omnisharp in dist
7z x $env:OMNISHARP_ZIP -y -o"$env:PACKAGE_BASE\omnisharp"
# bundle everything in the package folder
npm run package_electron
