import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { NgModule} from '@angular/core';
import { HttpClientModule } from '@angular/common/http';
import { ModalModule } from 'ngx-bootstrap';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { VirtualKeyboardComponent, TruncatePipe } from './components';
import { CoreModule} from '../core/core.module';
import { NgxSpinnerModule } from "ngx-spinner";
import { POS_API_BASE_URL } from '../core/api-services/api-service-proxies';

@NgModule({
  declarations: [
    AppComponent,
    VirtualKeyboardComponent,
    TruncatePipe
  ],
  imports: [
    BrowserModule,
    FormsModule,
    AppRoutingModule,
    HttpClientModule,
    CoreModule,
    NgxSpinnerModule,
    ModalModule.forRoot()
  ],
  providers: [
    { provide: POS_API_BASE_URL, useValue: 'http://localhost:22742'},
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
