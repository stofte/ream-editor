
import { enableProdMode } from '@angular/core';

if (MODE === 'PRODUCTION') {
    enableProdMode();
} else {
    console.log(MODE);
}

import { provide } from '@angular/core';
import { bootstrap }    from '@angular/platform-browser-dynamic';
import { HTTP_PROVIDERS } from '@angular/http';
import { ROUTER_PROVIDERS } from '@angular/router-deprecated';
import { LocationStrategy, HashLocationStrategy } from '@angular/common';
import { AppComponent } from './app.component';
import { MonitorService } from './services/monitor.service';
import { OverlayService } from './services/overlay.service';
import { StorageService } from './services/storage.service';
import { ConnectionService } from './services/connection.service';
import { TabService } from './services/tab.service';
import { EditorService } from './services/editor.service';
import { QueryService } from './services/query.service';
import { OmnisharpService } from './services/omnisharp.service';
import { LogService } from './services/log.service';
import { BufferNameService } from './services/buffer-name.service';
import { MirrorChangeStream } from './services/mirror-change.stream';

bootstrap(AppComponent, [
    MonitorService,
    OverlayService,
    StorageService,
    ConnectionService,
    TabService,
    EditorService,
    QueryService,
    OmnisharpService,
    LogService,
    BufferNameService,
    MirrorChangeStream,
    TabService,
    HTTP_PROVIDERS
    // ROUTER_PROVIDERS,
    // provide(LocationStrategy, { useClass: HashLocationStrategy })
]);
