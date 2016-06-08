const path = electronRequire('path');

export default {
    omnisharpPort: 2000,
    queryEnginePort: 8111,
    dotnetDebugPath: IS_LINUX ? path.normalize('/usr/bin/dotnet')
        : path.normalize('C:/Program Files/dotnet/dotnet.exe')  
};
