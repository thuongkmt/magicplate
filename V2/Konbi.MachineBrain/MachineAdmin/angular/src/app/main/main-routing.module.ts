import { Product } from './../../shared/service-proxies/service-proxies';
import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { SessionsComponent } from './machines/sessions/sessions.component';
import { DiscsComponent } from './plate/discs/discs.component';
import { DiscCheckComponent } from './plate/disc-check/disc-check.component';
import { PlatesComponent } from './plate/plates/plates.component';
import { PlateCategoriesComponent } from './plate/plateCategories/plateCategories.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { PlateMenusComponent } from './plate/plate-menus/plate-menus.component';
import { PlateMenuCalendarComponent } from './plate/plate-menu-calendar/plate-menu-calendar.component';
import { TransactionsComponent } from './transactions/transactions.component';
import { RfidTableComponent } from 'customer/rfid-table/rfid-table.component';
import { RfidTableSettingComponent } from './device-setting/rfid-table-setting/rfid-table-setting.component';
import { AbpSettingComponent } from './system-setting/abp-setting.component';
import { MdbcashlessDiagnosticComponent } from './device-setting/mdbcashless-diagnostic/mdbcashless-diagnostic.component';
import { ProductsComponent } from './products/products/products.component';
import { ProductsCategoriesComponent } from './products/products-categories/products-categories.component';
import { ServiceStatusComponent } from './service-status/service-status.component';

@NgModule({
    imports: [
        RouterModule.forChild([
            {
                path: '',
                children: [
                    { path: 'machines/sessions', component: SessionsComponent, data: { permission: 'Pages.Sessions' } },
                    { path: 'plate/dishes', component: DiscsComponent, data: { permission: 'Pages.Discs' } },
                    { path: 'plate/check-dishes', component: DiscCheckComponent, data: { permission: 'Pages.Discs' } },
                    { path: 'plate/dishes/:plate_id', component: DiscsComponent, data: { permission: 'Pages.Discs' } },
                    { path: 'plate/plates', component: PlatesComponent, data: { permission: 'Pages.Plates' } },
                    { path: 'plate/plateCategories', component: PlateCategoriesComponent, data: { permission: 'Pages.PlateCategories' } },
                    { path: 'plate/plateMenus', component: PlateMenusComponent, data: { permission: 'Pages.PlateMenus' } },
                    { path: 'plate/plateMenus/:datefilter', component: PlateMenusComponent, data: { permission: 'Pages.PlateMenus' } },
                    { path: 'plate/plateMenus/:datefilter/:sessionfilter', component: PlateMenusComponent, data: { permission: 'Pages.PlateMenus' } },
                    { path: 'plate/plateMenuCalendar', component: PlateMenuCalendarComponent, data: { permission: 'Pages.PlateMenus' } },
                    { path: 'plate/trays', component: PlatesComponent, data: { permission: 'Pages.Plates' } },
                    { path: 'dashboard', component: DashboardComponent, data: { permission: 'Pages.Tenant.Dashboard' } },
                    { path: 'transactions-success', component: TransactionsComponent, data: { permission: 'Pages.Transactions' } },
                    { path: 'transactions-error', component: TransactionsComponent, data: { permission: 'Pages.Transactions' } },
                    { path: 'devicesetting/rfid-table', component: RfidTableSettingComponent, data: { permission: 'Pages.DeviceSetting' } },
                    { path: 'devicesetting/mdbcashless-diagnostic', component: MdbcashlessDiagnosticComponent, data: { permission: 'Pages.DeviceSetting' } },
                    { path: 'system-settings', component: AbpSettingComponent, data: { permission: 'Pages.SystemSetting' } },
                    { path: 'products/products', component: ProductsComponent, data: { permission: 'Pages.Tenant.Dashboard' } },
                    { path: 'products/productCategories', component: ProductsCategoriesComponent, data: { permission: 'Pages.Tenant.Dashboard' } },
                    { path: 'service-status', component: ServiceStatusComponent }
                ]
            }
        ]),
    ],
    exports: [
        RouterModule
    ]
})
export class MainRoutingModule { }
