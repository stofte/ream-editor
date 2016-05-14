# if this is a local build, this is the assumed layout
$env:PACKAGE_BASE="build"
$env:ELECTRON_OUT="linq-editor-win32-x64"
$env:DOTNET_INSTALL_DIR="C:\Program Files\dotnet"
$env:OMNISHARP_ZIP="omnisharp-win-x64-netcoreapp1.0.zip"
.\build.ps1
