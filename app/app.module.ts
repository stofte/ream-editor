import { NgModule } from '@angular/core';
import { HttpModule } from '@angular/http';
import { BrowserModule  } from '@angular/platform-browser';
import { AppComponent } from './app.component';
import { TabListComponent, TabViewComponent, QuerySuiteComponent, ResultDisplayComponent,
    ResultListComponent, ExecuteQueryComponent, ConnectionSelectorComponent,
    ConnectionManagerComponent, StartPageComponent } from './components/index';

@NgModule({
    declarations: [
        AppComponent,
        // TabListComponent,
        // TabViewComponent,
        // QuerySuiteComponent,
        // ResultDisplayComponent,
        // ResultListComponent,
        // ExecuteQueryComponent,
        // ConnectionSelectorComponent,
        // ConnectionManagerComponent,
        // StartPageComponent
    ],
    imports: [
        BrowserModule,
        HttpModule
    ],
    bootstrap: [AppComponent],
})
export class AppModule {}
