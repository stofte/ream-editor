import { Component } from '@angular/core';
import { BUTTON_DIRECTIVES } from 'ng2-bootstrap/ng2-bootstrap';
import { EditorService } from '../services/editor.service';
import { MirrorChangeStream } from '../services/mirror-change.stream';
import { MonitorService } from '../services/monitor.service';
import { ConnectionService } from '../services/connection.service';
import { QueryService } from '../services/query.service';
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
    disabled="{{ isDisabled ? 'disabled' : '' }}"
    >
    <span class="glyphicon glyphicon-play"></span>
</button>
`
})
export class ExecuteQueryComponent {
    private isDisabled = true;
    constructor(
        private mirrors: MirrorChangeStream,
        monitorService: MonitorService,
        hotkeys: HotkeyService
    ) {
        monitorService.queryReady.then(() => {
            this.isDisabled = false;
        });
        hotkeys.executeQuery
            .subscribe(() => this.run());
    }
    
    private run(): void {
        this.mirrors.execute();
    }
}
