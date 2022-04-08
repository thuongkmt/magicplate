import { AbpHttpInterceptor } from '@abp/abpHttpInterceptor';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { NgModule } from '@angular/core';
import * as ApiServiceProxies from './service-proxies';
import * as ApiProductServiceProxies from './productImage-proxies';
//migrate from old cloud
import * as MachineApiServiceProxies from './machine-service-proxies';
import * as PlateCategoriesServiceProxies from './plate-category-service-proxies';
import * as PlateServiceProxies from './plate-service-proxies';
import * as PlateMenuServiceProxies from './plate-menu-service-proxies';
import * as TransactionServiceProxy from './transaction-service-proxies';
import * as EmployeeServiceProxy from './employee-service-proxies';
import * as DashboardServiceProxy from './dashboard-service-proxies';
import * as SystemConfigServiceProxy from './system-config-service-proxies';
import * as GetStartedServiceProxy from './get-started-service-proxies';

@NgModule({
    providers: [
        ApiServiceProxies.SessionsServiceProxy,
        ApiServiceProxies.DiscsServiceProxy,
        PlateServiceProxies.PlateServiceProxy,
        PlateCategoriesServiceProxies.PlateCategoryServiceProxy,
        ApiServiceProxies.AuditLogServiceProxy,
        ApiServiceProxies.CachingServiceProxy,
        ApiServiceProxies.ChatServiceProxy,
        ApiServiceProxies.CommonLookupServiceProxy,
        ApiServiceProxies.EditionServiceProxy,
        ApiServiceProxies.FriendshipServiceProxy,
        ApiServiceProxies.HostSettingsServiceProxy,
        ApiServiceProxies.InstallServiceProxy,
        ApiServiceProxies.LanguageServiceProxy,
        ApiServiceProxies.NotificationServiceProxy,
        ApiServiceProxies.OrganizationUnitServiceProxy,
        ApiServiceProxies.PermissionServiceProxy,
        ApiServiceProxies.ProfileServiceProxy,
        ApiServiceProxies.RoleServiceProxy,
        ApiServiceProxies.SessionServiceProxy,
        ApiServiceProxies.TenantServiceProxy,
        ApiServiceProxies.TenantDashboardServiceProxy,
        ApiServiceProxies.TenantSettingsServiceProxy,
        ApiServiceProxies.TimingServiceProxy,
        ApiServiceProxies.UserServiceProxy,
        ApiServiceProxies.UserLinkServiceProxy,
        ApiServiceProxies.UserLoginServiceProxy,
        ApiServiceProxies.WebLogServiceProxy,
        ApiServiceProxies.AccountServiceProxy,
        ApiServiceProxies.TokenAuthServiceProxy,
        ApiServiceProxies.TenantRegistrationServiceProxy,
        ApiServiceProxies.HostDashboardServiceProxy,
        ApiServiceProxies.PaymentServiceProxy,
        ApiServiceProxies.DemoUiComponentsServiceProxy,
        ApiServiceProxies.InvoiceServiceProxy,
        ApiServiceProxies.SubscriptionServiceProxy,
        ApiServiceProxies.InstallServiceProxy,
        ApiServiceProxies.UiCustomizationSettingsServiceProxy,
        //ApiServiceProxies.TakeAwayTrayServiceProxy,
        { provide: HTTP_INTERCEPTORS, useClass: AbpHttpInterceptor, multi: true },
        //migrate from old cloud
        MachineApiServiceProxies.MachineServiceProxy,
        PlateMenuServiceProxies.PlateMenuServiceProxy,
        ApiServiceProxies.TransactionServiceProxy,
        EmployeeServiceProxy.EmployeeServiceProxy,
        DashboardServiceProxy.DashboardServiceProxy,
        SystemConfigServiceProxy.SystemConfigServiceProxy,
        GetStartedServiceProxy.GetStartedServiceProxy,
        ApiServiceProxies.CategoryServiceProxy,
        ApiServiceProxies.ProductServiceProxy,
        ApiServiceProxies.ProductMenusServiceProxy,
        ApiServiceProxies.PlatesServiceProxy,
        ApiProductServiceProxies.ProductImageServiceProxy
    ]
})
export class ServiceProxyModule { }
