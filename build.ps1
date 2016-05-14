if (-Not($env:CI)){
    $env:PACKAGE_BASE="build"
    $env:ELECTRON_OUT="linq-editor-win32-x64"
    $env:DOTNET_INSTALL_DIR="C:\Program Files\dotnet"
    $env:OMNISHARP_ZIP="omnisharp-win-x64-netcoreapp1.0.zip"
}
mkdir -Force $env:PACKAGE_BASE | Out-Null
remove-item $env:PACKAGE_BASE\* -recurse
npm install
npm run ts-build
npm run bundle $env:PACKAGE_BASE
copy index.static.html $env:PACKAGE_BASE\index.html
copy electron-main.js $env:PACKAGE_BASE\index.js
copy systemjs.config.js $env:PACKAGE_BASE\systemjs.config.js
# other web resources
copy node_modules\es6-shim\es6-shim.min.js $env:PACKAGE_BASE\es6-shim.min.js
copy node_modules\zone.js\dist\zone.js $env:PACKAGE_BASE\zone.js
copy node_modules\reflect-metadata\Reflect.js $env:PACKAGE_BASE\Reflect.js
copy node_modules\systemjs\dist\system.src.js $env:PACKAGE_BASE\system.src.js
dotnet restore
dotnet publish --configuration Release --output $env:PACKAGE_BASE\query
# windows dotnet cant make exe files, so need to include dotnet.exe for bootstraping query-engine
copy "$env:DOTNET_INSTALL_DIR\dotnet.exe" $env:PACKAGE_BASE\query\dotnet.exe
copy "$env:DOTNET_INSTALL_DIR\hostfxr.dll" $env:PACKAGE_BASE\query\hostfxr.dll
xcopy "$env:DOTNET_INSTALL_DIR\shared" "$env:PACKAGE_BASE\query\shared\" /y /s
# need project.json to allow resolving references at runtime
copy NuGet.Config $env:PACKAGE_BASE\query\NuGet.Config
copy project.json $env:PACKAGE_BASE\query\project.json
copy project.lock.json $env:PACKAGE_BASE\query\project.lock.json
# include omnisharp in dist
7z x $env:OMNISHARP_ZIP -y -o"$env:PACKAGE_BASE\omnisharp"
# bundle everything in the package folder
npm run package_electron
7z a app.zip $env:ELECTRON_OUT\
