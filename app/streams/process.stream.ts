import { Http, Response } from '@angular/http';
import { Subject, Observable } from 'rxjs/Rx';
import { EventName, Message } from './api';
import config from '../config';
const child_process = electronRequire('child_process');
const ipc = electronRequire('electron').ipcRenderer;
const path = electronRequire('path');

const DEBUG = MODE === 'DEVELOPMENT';
type ProcessType = 'omnisharp' | 'query';

export class ProcessStream {
    public status: Subject<Message> = new Subject<Message>();
    private options: any;
    private command: string;
    private directory: string;
    private httpPort: number;
    private cancelCheck: boolean = false;
    private exitHandler = false;
    constructor(private http: Http) { }

    public start(processType: ProcessType, command: string, directory: string, httpPort: number) {
        this.command = command;
        this.httpPort = httpPort;
        this.options = processType === 'omnisharp' ? { } : { cwd: `${process.cwd()}\query` };
        this.status.next(new Message(EventName.ProcessStarting));
        let start = new Date().getTime();
        let statusSub = this.status.subscribe(msg => {
            if (msg.name === EventName.ProcessFailed) {
                this.cancelCheck = true;
            }
        });
        child_process.exec(this.command, {}, (error: string, stdout: string, stderr: string) => {
            if (!this.exitHandler) {
                // once we've seen the server response, we disable this, since it's unpredictable otherwise,
                // will return once stdout buffer fills, windows only?
                return;
            }
            if (stderr) { 
                this.status.next(new Message(EventName.ProcessFailed));
            } else {
                this.status.next(new Message(EventName.ProcessClosed));
            }
            setTimeout(() => {
                statusSub.unsubscribe();
            }, 100);
        });
        this.checkreadystatus();
    }

    public confirmedReady() {
        this.cancelCheck = true;
        this.exitHandler = false;
    }

    public close() {
        this.status.next(new Message(EventName.ProcessClosing));
        this.http.get(this.action('stopserver')).subscribe();
        setTimeout(() => this.checkstopstatus(), 500);
    }

    private checkreadystatus() {
        this.http.get(this.action('checkreadystatus'))
            .subscribe(ok => {
                this.cancelCheck = true;
                this.status.next(new Message(EventName.ProcessReady));
            }, error => {
                if (!this.cancelCheck) {
                    setTimeout(() => this.checkreadystatus(), 500);
                }
            });
    }

    private checkstopstatus() {
        this.http.get(this.action('checkreadystatus'))
            .subscribe(ok => {
                setTimeout(() => { this.checkstopstatus(); }, 500);
            }, error => {
                this.status.next(new Message(EventName.ProcessClosed));
            });
    }
    
    private action(name: string) {
        return `http://localhost:${this.httpPort}/${name}`;
    }
}
