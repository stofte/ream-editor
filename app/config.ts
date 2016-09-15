const path = electronRequire('path');
// appveyor/travisci sets this var
const CI_MOD = process.env['CI'] ? 3 : 1;

const omnisharpPath = path.resolve((IS_LINUX ? `${process.env.HOME}/.ream-editor/` : 
    `${process.env.LOCALAPPDATA}\\ReamEditor\\`) + 'omnisharp');

// todo some of this is mirrored in int-helpers.js
const unitTestData = {
    backendTimeout: 10 * 1000 * CI_MOD
};

export default {
    unitTestData,
    omnisharpPort: 2000,
    queryEnginePort: 8111,
    omnisharpProjectPath: omnisharpPath,
    dotnetDebugPath: IS_LINUX ? path.normalize('/dotnetpreview2/dotnet')
        : path.normalize('C:/Program Files/dotnet/dotnet.exe')  
};
