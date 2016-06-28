const path = electronRequire('path');

const omnisharpPath = path.resolve((IS_LINUX ? `${process.env.HOME}/.ream-editor/` : 
    `${process.env.LOCALAPPDATA}\\ReamEditor\\`) + 'omnisharp');

export default {
    omnisharpPort: 2000,
    queryEnginePort: 8111,
    omnisharpProjectPath: omnisharpPath,
    dotnetDebugPath: IS_LINUX ? path.normalize('/usr/bin/dotnet')
        : path.normalize('C:/Program Files/dotnet/dotnet.exe')  
};
