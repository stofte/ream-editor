import { NgModule, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { HttpModule } from '@angular/http';
import { BrowserModule  } from '@angular/platform-browser';
import { AppComponent } from './app.component';
import { TabListComponent, TabViewComponent, QueryPanelComponent, ResultDisplayComponent,
    ExecuteQueryComponent, EditorComponent,
    ConnectionManagerComponent, StartPageComponent, ControlsComponent } from './components/index';
import { InputStream, OutputStream, StreamManager, EditorStream,
    ResultStream, QueryStream, OmnisharpStream } from './streams/index';
import { TabService, HotkeyService, ConnectionService } from './services/index';
import { EditorDirective } from './directives/editor.directive';
import { SortablejsModule } from 'angular-sortablejs';

@NgModule({
    declarations: [
        AppComponent,
        TabListComponent,
        QueryPanelComponent,
        ControlsComponent,
        ConnectionManagerComponent,
        EditorComponent,
        ResultDisplayComponent
    ],
    providers: [
        InputStream,
        OutputStream,
        StreamManager,
        EditorStream,
        ResultStream,
        QueryStream,
        OmnisharpStream,
        TabService,
        HotkeyService,
        ConnectionService
    ],
    imports: [
        BrowserModule,
        HttpModule,
        SortablejsModule
    ],
    bootstrap: [AppComponent],
    schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class AppModule {}
