import { NgModule } from '@angular/core';
import { HttpModule } from '@angular/http';
import { BrowserModule  } from '@angular/platform-browser';
import { AppComponent } from './app.component';
import { TabListComponent, TabViewComponent, QueryPanelComponent, ResultDisplayComponent,
    ResultListComponent, ExecuteQueryComponent, ContextSelectorComponent,
    ConnectionManagerComponent, StartPageComponent } from './components/index';
import { InputStream, OutputStream, StreamManager, EditorStream,
    ResultStream, QueryStream, OmnisharpStream } from './streams/index';
import { TabService } from './services/index';
import { MdlModule } from 'angular2-mdl';

@NgModule({
    declarations: [
        AppComponent,
        TabListComponent,
        QueryPanelComponent,
        ContextSelectorComponent
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
})
export class AppModule {}
