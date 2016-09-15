import config from '../config';
const path = electronRequire('path');

const IS_DEBUG = MODE === 'DEVELOPMENT';

const dirname = (
        IS_DEBUG ? process.cwd()
                 : path.normalize(process.resourcesPath + '/app')
    )
    .replace(/%20/g, ' ');

export class ProcessHelper {
    public omnisharp(port: number): { command: string, directory: string } {
        let exePath = `"${dirname}/omnisharp/OmniSharp${!IS_LINUX ? '.exe' : ''}"`;
        let slnPath = IS_LINUX ? config.omnisharpProjectPath.replace(/\\/g, '/')
            : config.omnisharpProjectPath.replace(/\//g, '\\');
        let cmd = `${exePath} -s ${slnPath} -p ${port}`;
        return { command: cmd, directory: null };
    }

    public query(port: number): { command: string, directory: string } {
        let dir: string = null;
        let cmd: string = null;
        if (IS_DEBUG) {
            dir = `${dirname}`;
            cmd = `"${config.dotnetDebugPath}" run -p query/query/src/ReamQuery --server.urls http://localhost:${port}`;
        } else {
            dir = `${dirname}/query`;
            cmd = `"${dir}/ReamQuery${!IS_LINUX ? '.exe' : ''}"`;
        }
        return { command: cmd, directory: dir };
    }
}
