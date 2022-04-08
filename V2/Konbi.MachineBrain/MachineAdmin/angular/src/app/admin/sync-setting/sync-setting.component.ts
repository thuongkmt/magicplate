import { Component, OnInit, Injector, ViewEncapsulation, ViewChild, ElementRef } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Http } from '@angular/http';
import { PlateServiceProxy } from '@shared/service-proxies/plate-service-proxies';
import { SessionsServiceProxy } from '@shared/service-proxies/service-proxies';
import { ProductMenuServiceProxy } from '@shared/service-proxies/plate-menu-service-proxies';
import { NotifyService } from '@abp/notify/notify.service';
import { AppComponentBase } from '@shared/common/app-component-base';
import { TokenAuthServiceProxy } from '@shared/service-proxies/service-proxies';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import * as _ from 'lodash';
import { DiscsServiceProxy, DiscDto } from '@shared/service-proxies/service-proxies';
import { PlateCategoryServiceProxy, PlateCategoryDto } from '@shared/service-proxies/plate-category-service-proxies';
import { CommonServiceProxy } from '@shared/service-proxies/common-service-proxies';
import { Ng4LoadingSpinnerService } from 'ng4-loading-spinner';

@Component({
    selector: 'app-sync-setting',
    templateUrl: './sync-setting.component.html',
    styleUrls: ['./sync-setting.component.css'],
    encapsulation: ViewEncapsulation.None,
    animations: [appModuleAnimation()]
})
export class SyncSettingComponent extends AppComponentBase {
    categoriesLastSynced = 'Never';
    productsLastSynced = 'Never';
    plateCategoriesLastSynced = 'Never';
    platesLastSynced = 'Never';
    inventoryLastSynced = 'Never';
    menuSchedulerLastSynced = 'Never';
    sessionLastSynced = 'Never';
    constructor(
        injector: Injector,
        private _plateMenusServiceProxy: ProductMenuServiceProxy,
        private _sessionsServiceProxy: SessionsServiceProxy,
        private _platesServiceProxy: PlateServiceProxy,
        private _discsServiceProxy: DiscsServiceProxy,
        private _plateCategoriesServiceProxy: PlateCategoryServiceProxy,
        private spinnerService: Ng4LoadingSpinnerService,
        private _commonServiceProxy: CommonServiceProxy
    ) {
        super(injector);
    }

    ngOnInit() {
        this.getLastSyncedDatetimes();
    }

    syncDataPartially() {
        abp.ui.setBusy();
        this._sessionsServiceProxy.syncDataPartially()

            .subscribe(() => {
                abp.ui.clearBusy();
                this.notify.success('Data synced successfully');
                this.getLastSyncedDatetimes();
            });
    }

    syncDataFully() {
        abp.ui.setBusy();
        this._sessionsServiceProxy.syncDataFully()
            .finally(() => {
                abp.ui.clearBusy();
            })
            .subscribe(result => {
                console.log(result);
                abp.ui.clearBusy();
                this.notify.success('Data synced successfully');
                this.getLastSyncedDatetimes();
            });
    }
    getLastSyncedDatetimes() {
        this._commonServiceProxy.getSetting('CategoryEntityLastSynced')
            .subscribe((data) => {
                let seconds = parseInt(data);
                if (seconds > 0) {
                    let d = new Date(0);
                    d.setUTCSeconds(seconds);
                    this.categoriesLastSynced = d.toLocaleString();
                }
            });
        this._commonServiceProxy.getSetting('ProductEntityLastSynced')
            .subscribe((data) => {
                let seconds = parseInt(data);
                if (seconds > 0) {
                    let d = new Date(0);
                    d.setUTCSeconds(seconds);
                    this.productsLastSynced = d.toLocaleString();
                }
            });
        this._commonServiceProxy.getSetting('PlateCategoryEntityLastSynced')
            .subscribe((data) => {
                let seconds = parseInt(data);
                if (seconds > 0) {
                    let d = new Date(0);
                    d.setUTCSeconds(seconds);
                    this.plateCategoriesLastSynced = d.toLocaleString();
                }
            });
        this._commonServiceProxy.getSetting('PlateEntityLastSynced')
            .subscribe((data) => {
                let seconds = parseInt(data);
                if (seconds > 0) {
                    let d = new Date(0);
                    d.setUTCSeconds(seconds);
                    this.platesLastSynced = d.toLocaleString();
                }
            });
        this._commonServiceProxy.getSetting('InventoryEntityLastSynced')
            .subscribe((data) => {
                let seconds = parseInt(data);
                if (seconds > 0) {
                    let d = new Date(0);
                    d.setUTCSeconds(seconds);
                    this.inventoryLastSynced = d.toLocaleString();
                }
            });
        this._commonServiceProxy.getSetting('ProductMenuLastSynced')
            .subscribe((data) => {
                let seconds = parseInt(data);
                if (seconds > 0) {
                    let d = new Date(0);
                    d.setUTCSeconds(seconds);
                    this.menuSchedulerLastSynced = d.toLocaleString();
                }
            });
        this._commonServiceProxy.getSetting('SessionEntityLastSynced')
            .subscribe((data) => {
                let seconds = parseInt(data);
                if (seconds > 0) {
                    let d = new Date(0);
                    d.setUTCSeconds(seconds);
                    this.sessionLastSynced = d.toLocaleString();
                }
            });
    }
}
