import { Injectable } from '@angular/core';
import { Http } from '@angular/http';
import { Observable } from 'rxjs/Rx';
import config from '../config';

const child_process = electronRequire('child_process');
const ipc = electronRequire('electron').ipcRenderer;

@Injectable()
export class MonitorService {
    public omnisharpReady: Promise<boolean>;
    public queryReady: Promise<boolean>;
    
    private omnisharpResolver;
    private queryResolver;
    private checker;
        
    constructor(private http: Http) {
        ipc.on('application-event', this.applicationEventHandler.bind(this));
        this.omnisharpReady = new Promise((res, err) => {
            this.omnisharpResolver = (arg) => {
                // some buffer time
                setTimeout(() => { res(arg); }, 500); 
            };
        });
        
        this.queryReady = new Promise((res, err) => {
            this.queryResolver = res;
        });
    }
    
    private applicationEventHandler(event : any, msg : string) {
        if (msg === 'close') {
            let queryCb = () => ipc.send('application-event', 'close-query-engine');
            let omniCb = () => ipc.send('application-event', 'close-omnisharp');
            this.http.get(this.action(config.omnisharpPort, 'stopserver')).subscribe(omniCb, omniCb);
            this.http.get(this.action(config.queryEnginePort, 'stopserver')).subscribe(queryCb, queryCb);
        }
    }
        
    public start() {
        let slnPath = 'C:/src/frank/project';
        let queryCmd = '"C:/Program Files/dotnet/dotnet.exe" run';
        let omnisharpCmd = `C:/src/frank/omnisharp/Omnisharp.exe -s ${slnPath} -p ${config.omnisharpPort}`;
        let dotnetCwd = 'C:/src/frank/query-engine';
        
        this.http.get(this.action(config.omnisharpPort, 'checkreadystatus'))
            .subscribe(
                ok => {
                    console.log(`omnisharp already running on ${config.omnisharpPort}`);
                    this.omnisharpResolver(true);
                },
                error => {
                    this.checkBackends(config.omnisharpPort);
                    this.startProcess(omnisharpCmd, { });
                }
            );

        this.http.get(this.action(config.queryEnginePort, 'checkreadystatus'))
            .subscribe(
                ok =>{
                    console.log(`query-engine already running on ${config.queryEnginePort}`);
                    this.queryResolver(true);
                }, error => {
                    this.checkBackends(config.queryEnginePort);
                    this.startProcess(queryCmd, { cwd: dotnetCwd });
                }
            );
    }
    
    private pollingInternalMs;
    private checkBackends(port: number) {
        this.http.get(this.action(port, 'checkreadystatus'))
            .subscribe(ok => {
                if (port === config.omnisharpPort) this.omnisharpResolver(true);
                if (port === config.queryEnginePort) this.queryResolver(true);
            }, error => {
                let lbl = port === config.omnisharpPort ? 'omnisharp' : 'query-engine';
                console.log('checkBackends failed for', lbl);
                setTimeout(() => this.checkBackends(port), 250);
            });
    }
    
    private startProcess(cmd : string, options : any) {
        
        console.log('starting process');
        child_process.exec(cmd, options, (error: string, stdout: string, stderr: string) => {
            console.log(`stdout: ${stdout}`);
            console.log(`stderr: ${stderr}`);            
            if (error !== null) {
                console.log(`exec error: ${error}`);
            }
        });
    }
    
    private action(port : number, name : string) {
        return `http://localhost:${port}/${name}`;
    }
}
