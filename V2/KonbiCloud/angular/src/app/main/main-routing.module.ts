import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { SessionsComponent } from './machines/sessions/sessions.component';
import { DiscsComponent } from './plate/discs/discs.component';
import { PlatesComponent } from './plate/plates/plates.component';
import { PlateCategoriesComponent } from './plate/plateCategories/plateCategories.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { PlateMenusComponent } from './plate/plate-menus/plate-menus.component';
import { PlateMenuCalendarComponent } from './plate/plate-menu-calendar/plate-menu-calendar.component';
import { TransactionsComponent } from './transactions/transactions.component';
import { AbpSettingComponent } from './system-setting/abp-setting.component';
import { GetStartedComponent } from './get-started/get-started.component';
import { ProductsComponent } from './products/products/products.component';
import { ProductsCategoriesComponent } from './products/products-categories/products-categories.component';

@NgModule({
    imports: [
        RouterModule.forChild([
            {
                path: '',
                children: [
                    { path: 'machines/sessions', component: SessionsComponent, data: { permission: 'Pages.Sessions' }  },
                    { path: 'plate/dishes', component: DiscsComponent, data: { permission: 'Pages.Discs' }  },
                    { path: 'plate/dishes/:plate_id', component: DiscsComponent, data: { permission: 'Pages.Discs' }  },
                    { path: 'plate/plates', component: PlatesComponent, data: { permission: 'Pages.Plates' }  },
                    { path: 'plate/plateCategories', component: PlateCategoriesComponent, data: { permission: 'Pages.PlateCategories' }  },
                    { path: 'plate/plateMenus', component: PlateMenusComponent, data: { permission: 'Pages.PlateMenus' }  },
                    { path: 'plate/plateMenus/:datefilter', component: PlateMenusComponent, data: { permission: 'Pages.PlateMenus' }  },
                    { path: 'plate/plateMenus/:datefilter/:sessionfilter', component: PlateMenusComponent, data: { permission: 'Pages.PlateMenus' }  },
                    { path: 'plate/plateMenuCalendar', component: PlateMenuCalendarComponent, data: { permission: 'Pages.PlateMenus' }  },
                    { path: 'plate/trays', component: PlatesComponent, data: { permission: 'Pages.Plates' }  },
                    { path: 'dashboard', component: DashboardComponent, data: { permission: 'Pages.Tenant.Dashboard' } },
                    { path: 'getting-started', component: GetStartedComponent, data: { permission: 'Pages.Tenant.Dashboard' } },
                    { path: 'transactions-success', component: TransactionsComponent, data: { permission: 'Pages.Transactions' } },
                    { path: 'transactions-error', component: TransactionsComponent, data: { permission: 'Pages.Transactions' } },
                    { path: 'system-settings', component: AbpSettingComponent, data: { permission: 'Pages.SystemSetting' } },
                    { path: 'products/products', component: ProductsComponent, data: { permission: 'Pages.Products' } },
                    { path: 'products/productCategories', component: ProductsCategoriesComponent, data: { permission: 'Pages.Categories' } },
                ]
            }
        ])
    ],
    exports: [
        RouterModule
    ]
})
export class MainRoutingModule { }
