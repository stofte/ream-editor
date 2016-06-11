import { Injectable } from '@angular/core';
const ipc = electronRequire('electron').ipcRenderer;
const fs = electronRequire('fs');
const path = electronRequire('path');
const isProduction = MODE !== 'DEVELOPMENT';
const dirname = isProduction ?  path.dirname(process.resourcesPath) : path.dirname(path.dirname(__dirname));
const logFile = path.normalize(`${dirname}/linq-editor${isProduction ? '' : '.dev'}.log`);

@Injectable()
export class LogService {
    private logs: string[] = [];
    
    constructor() {
        ipc.on('application-event', this.applicationEventHandler.bind(this));
        this.log('log.service', `mode=${MODE}`);
    }
    
    public log(stamp: string, msg: string) {
        const str = `${stamp} => \n${msg}`;
        this.logs.push(str);
    }
    
    private applicationEventHandler(event: any, msg: string) {
        if (msg === 'close') {
            // allow shit to flush to the log
            setTimeout(() => {
                const logOut = this.logs.join('\n\n- LOG --------------------------------------------------------------\n\n');
                fs.writeFile(logFile, logOut, function(err) {
                    if (err) {
                        console.error(`writeFile.err ${err}`);
                    }
                    ipc.send('application-event', 'close-log');
                });                
            }, 5000);
        }
    }
}
