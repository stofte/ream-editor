import { Component } from '@angular/core';
// import { BUTTON_DIRECTIVES } from 'ng2-bootstrap/ng2-bootstrap';
import { EditorService } from '../services/editor.service';
import { MonitorService } from '../services/monitor.service';
import { ConnectionService } from '../services/connection.service';
import { TabService } from '../services/tab.service';
import { HotkeyService } from '../services/hotkey.service';
import { Tab } from '../models/tab';
import { Connection } from '../models/connection';

@Component({
    selector: 'f-execute-query',
    template: `
<button class="btn btn-primary form-control int-test-execute-btn"
    type="button"
    (click)="run()" 
    disabled="{{ isEnabled ? '' : 'disabled' }}"
    >
    <span class="glyphicon glyphicon-play"></span>
</button>
`
})
export class ExecuteQueryComponent {
    private isDisabled = true;
    private isExecuting = false;
    constructor(
        private mirrors,
        monitorService: MonitorService,
        hotkeys: HotkeyService
    ) {
        monitorService.queryReady.then(() => {
            this.isDisabled = false;
        });
        hotkeys.executeQuery
            .subscribe(() => this.run());
    }
    
    private get isEnabled(): boolean {
        return !this.isDisabled && !this.isExecuting;
    }
    
    private run(): void {
        if (!this.isEnabled) {
            return;
        } else {
            this.isExecuting = true;
            this.mirrors.execute();
            setTimeout(() => {
                this.isExecuting = false;
            }, 1000); // disable spamming
        }
    }
}
