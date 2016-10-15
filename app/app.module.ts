import { NgModule } from '@angular/core';
import { HttpModule } from '@angular/http';
import { BrowserModule  } from '@angular/platform-browser';
import { AppComponent } from './app.component';
import { TabListComponent, TabViewComponent, QuerySuiteComponent, ResultDisplayComponent,
    ResultListComponent, ExecuteQueryComponent, ConnectionSelectorComponent,
    ConnectionManagerComponent, StartPageComponent } from './components/index';
import { MdlModule } from 'angular2-mdl';

@NgModule({
    declarations: [
        AppComponent,
        TabListComponent
    ],
    imports: [
        BrowserModule,
        HttpModule,
        MdlModule
    ],
    bootstrap: [AppComponent],
})
export class AppModule {}
