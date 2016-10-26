import { NgModule, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { HttpModule } from '@angular/http';
import { BrowserModule  } from '@angular/platform-browser';
import { AppComponent } from './app.component';
import { TabListComponent, TabViewComponent, QueryPanelComponent, ResultDisplayComponent,
    ResultListComponent, ExecuteQueryComponent,
    ConnectionManagerComponent, StartPageComponent, ControlsComponent } from './components/index';
import { InputStream, OutputStream, StreamManager, EditorStream,
    ResultStream, QueryStream, OmnisharpStream } from './streams/index';
import { TabService } from './services/index';
import { MdlModule } from 'angular2-mdl';
import { PolymerElement } from '@vaadin/angular2-polymer';

@NgModule({
    declarations: [
        AppComponent,
        TabListComponent,
        QueryPanelComponent,
        ControlsComponent,
        ConnectionManagerComponent,
        PolymerElement('paper-button'),
        PolymerElement('paper-dialog'),
        PolymerElement('paper-dialog-scrollable'),
        PolymerElement('paper-item'),
        PolymerElement('paper-listbox'),
        PolymerElement('paper-dropdown-menu')
    ],
    providers: [
        InputStream,
        OutputStream,
        StreamManager,
        EditorStream,
        ResultStream,
        QueryStream,
        OmnisharpStream,
        TabService
    ],
    imports: [
        BrowserModule,
        HttpModule,
        MdlModule
    ],
    bootstrap: [AppComponent],
    schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class AppModule {}
