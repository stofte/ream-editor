# if this is a local build, this is the assumed layout
if (-Not($env:PACKAGE_BASE)) { $env:PACKAGE_BASE="build" }
if (-Not($env:ELECTRON_OUT)) { $env:ELECTRON_OUT="linq-editor-win32-x64" }
if (-Not($env:DOTNET_INSTALL_DIR)) { $env:DOTNET_INSTALL_DIR="C:\Program Files\dotnet" }
if (-Not($env:OMNISHARP_ZIP)) { $env:OMNISHARP_ZIP="omnisharp-win-x64-netcoreapp1.0.zip" }
mkdir -Force $env:PACKAGE_BASE | Out-Null
copy index.html $env:PACKAGE_BASE
copy package.json $env:PACKAGE_BASE 
copy systemjs.config.js $env:PACKAGE_BASE
copy electron-main.js $env:PACKAGE_BASE 
npm run ts-build
npm run bundle $env:PACKAGE_BASE
# dotnet restore
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
7z a $env:ELECTRON_OUT.zip $env:ELECTRON_OUT
