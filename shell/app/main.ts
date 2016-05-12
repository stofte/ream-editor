import { provide } from '@angular/core';
import { bootstrap }    from '@angular/platform-browser-dynamic';
import { HTTP_PROVIDERS } from '@angular/http';
import { ROUTER_PROVIDERS } from '@angular/router-deprecated';
import { LocationStrategy, HashLocationStrategy } from '@angular/common';
import { AppComponent } from './app.component';
import { MonitorService } from './services/monitor.service';
import { OverlayUiStateService } from './services/overlay-ui-state.service';
import { StorageService } from './services/storage.service';
import { ConnectionService } from './services/connection.service';
import { TabService } from './services/tab.service';
import { EditorService } from './services/editor.service';
import { QueryService } from './services/query.service';
import { OmnisharpService } from './services/omnisharp.service';

bootstrap(AppComponent, [
    MonitorService,
    OverlayUiStateService,
    StorageService,
    ConnectionService,
    TabService,
    EditorService,
    QueryService,
    OmnisharpService,
    HTTP_PROVIDERS,
    ROUTER_PROVIDERS,
    provide(LocationStrategy, { useClass: HashLocationStrategy }),
]);
