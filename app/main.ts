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
import {
        MonitorService,
        OverlayService,
        StorageService,
        ConnectionService,
        TabService,
        EditorService,
        QueryService,
        OmnisharpService,
        LogService,
        MirrorChangeStream,
        HotkeyService,
        UserService
} from './services/index';

import {
    UserStream,
    EditorStream
} from './streams/index';

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
    MirrorChangeStream,
    HotkeyService,
    UserService,
    UserStream,
    EditorStream,
    HTTP_PROVIDERS
]);
