import { enableProdMode } from '@angular/core';

if (MODE === 'PRODUCTION') {
    enableProdMode();
} else {
    console.log(MODE);
}

import { platformBrowserDynamic  }    from '@angular/platform-browser-dynamic';
import { AppModule } from './app.module';

platformBrowserDynamic().bootstrapModule(AppModule);

// import {
//         MonitorService,
//         OverlayService,
//         StorageService,
//         ConnectionService,
//         TabService,
//         EditorService,
//         QueryService,
//         OmnisharpService,
//         LogService,
//         MirrorChangeStream,
//         HotkeyService,
//         UserService
// } from './services/index';

// import {
//     UserStream,
//     EditorStream
// } from './streams/index';

// bootstrap(AppComponent, [
//     MonitorService,
//     OverlayService,
//     StorageService,
//     ConnectionService,
//     TabService,
//     EditorService,
//     QueryService,
//     OmnisharpService,
//     LogService,
//     MirrorChangeStream,
//     HotkeyService,
//     UserService,
//     UserStream,
//     EditorStream,
//     HTTP_PROVIDERS
// ]);
