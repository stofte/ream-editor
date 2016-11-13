const path = electronRequire('path');
// appveyor/travisci sets this var
const CI_MOD = process.env['CI'] ? 3 : 1;

let omnisharpPath = path.resolve((IS_LINUX ? `${process.env.HOME}/.ream-editor/` : 
    `${process.env.LOCALAPPDATA}\\ReamEditor\\`) + 'dist');

if (MODE === 'DEVELOPMENT')
{
    omnisharpPath = path.resolve(`${process.cwd()}/query/src/ReamQuery`);
}

const omnisharpFilePath = MODE === 'DEVELOPMENT' ? omnisharpPath :
    `${omnisharpPath}/src/ReamQuery`;

// todo some of this is mirrored in int-helpers.js
const unitTestData = {
    backendTimeout: 10 * 1000 * CI_MOD,
    sqliteWorlddbConnectionString: `Data Source=${path.normalize(path.join(process.cwd(), 'query/sql/world.sqlite'))}`
};

export default {
    unitTestData,
    omnisharpPort: 2000,
    queryEnginePort: 8111,
    omnisharpProjectPath: omnisharpPath,
    omnisharpSubPath: omnisharpFilePath,
    dotnetDebugPath: IS_LINUX ? path.normalize('/usr/bin/dotnet')
        : path.normalize('C:/Program Files/dotnet/dotnet.exe')  
};
