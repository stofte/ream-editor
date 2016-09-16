import { Injectable } from '@angular/core';
import { Http } from '@angular/http';
import { Subject } from 'rxjs/Rx';
import { ProcessMessage } from '../messages/index';
const child_process = electronRequire('child_process');
const ipc = electronRequire('electron').ipcRenderer;

@Injectable()
export class ProcessStream {
    public status: Subject<ProcessMessage> = new Subject<ProcessMessage>();
    private options: any;
    private command: string;
    private directory: string;
    private httpPort: number;
    private cancelCheck: boolean = false;
    constructor(private http: Http) { }

    public start(command: string, directory: string, httpPort: number) {
        this.command = command;
        this.directory = directory;
        this.httpPort = httpPort;
        this.options = directory ? { cwd: directory } : { };
        this.status.next(new ProcessMessage('starting'));
        let start = new Date().getTime();
        let statusSub = this.status.subscribe(msg => {
            if (msg.type === 'failed') {
                this.cancelCheck = true;
            }
        });
        child_process.exec(this.command, this.options, (error: string, stdout: string, stderr: string) => {
            if (error) { // expected when starting command fails, otherwise only stdout/err should be filled
                console.log('error', error);
                this.status.next(new ProcessMessage('failed', error));
            } else {
                this.status.next(new ProcessMessage('closed'));
            }
            statusSub.unsubscribe();
        });
        this.checkreadystatus();
    }

    public close() {
        this.status.next(new ProcessMessage('closing'));
        this.http.get(this.action('stopserver')).subscribe();
    }

    private checkreadystatus() {
        this.http.get(this.action('checkreadystatus'))
            .subscribe(ok => {
                // console.log('checkreadystatus ok response', ok.text());
                this.cancelCheck = true;
                this.status.next(new ProcessMessage('ready'));
            }, error => {
                // console.log('checkreadystatus timeouted')
                if (!this.cancelCheck) {
                    setTimeout(() => { this.checkreadystatus(); }, 500);
                }
            });
    }
    
    private action(name: string) {
        return `http://localhost:${this.httpPort}/${name}`;
    }
}
