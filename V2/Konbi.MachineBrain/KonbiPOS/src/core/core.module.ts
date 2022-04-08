import { NgModule } from '@angular/core';
import * as PosApiService from './api-services/api-service-proxies';
import { SignalRService } from './signalr/signalr-service';

@NgModule({
    providers: [
        PosApiService.ProductServiceProxy,
        PosApiService.CategoryServiceProxy,
        PosApiService.ProductMenusServiceProxy,
        PosApiService.SessionsServiceProxy,
        SignalRService
    ]
})
export class CoreModule { }
