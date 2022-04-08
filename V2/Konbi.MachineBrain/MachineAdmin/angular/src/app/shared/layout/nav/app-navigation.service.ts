import { PermissionCheckerService } from '@abp/auth/permission-checker.service';
import { AppSessionService } from '@shared/common/session/app-session.service';
import { FeatureCheckerService } from '@abp/features/feature-checker.service';
import { Injector, Injectable } from '@angular/core';
import { AppMenu } from './app-menu';
import { AppMenuItem } from './app-menu-item';

@Injectable()
export class AppNavigationService {

    feature: FeatureCheckerService;

    constructor(
        injector: Injector,
        private _permissionCheckerService: PermissionCheckerService,
        private _appSessionService: AppSessionService
    ) {
        this.feature = injector.get(FeatureCheckerService);
    }

    getMenu(): AppMenu {
        var appMenu = new AppMenu('MainMenu', 'MainMenu', [

            new AppMenuItem('Dashboard', 'Pages.Administration.Host.Dashboard', 'flaticon-line-graph', '/app/admin/hostDashboard'),
            new AppMenuItem('Dashboard', 'Pages.Tenant.Dashboard', 'flaticon-line-graph', '/app/main/dashboard'),
            new AppMenuItem('Tenants', 'Pages.Tenants', 'flaticon-list-3', '/app/admin/tenants'),
            new AppMenuItem('Editions', 'Pages.Editions', 'flaticon-app', '/app/admin/editions')
            // new AppMenuItem('DemoUiComponents', 'Pages.DemoUiComponents', 'flaticon-shapes', '/app/admin/demo-ui-components')
        ]);

        appMenu.items.push(
            new AppMenuItem('Plate Model Manager', 'Pages.Plates', 'flaticon-interface-8', '', [
                new AppMenuItem('Categories', 'Pages.PlateCategories', 'flaticon-pie-chart', '/app/main/plate/plateCategories'),
                new AppMenuItem('Plate Model', 'Pages.Plates', 'flaticon-car', '/app/main/plate/plates'),
                //new AppMenuItem('Plate Inventory Manager', 'Pages.Discs', 'flaticon-menu-1', '/app/main/plate/dishes'),
                // new AppMenuItem('PlateMenus', 'Pages.PlateMenus', '	flaticon-calendar-2', '/app/main/plate/plateMenus'),
                // new AppMenuItem('TrayManagement', 'Pages.Trays', 'flaticon-menu-1', '/app/main/plate/trays'),
            ])
        );

        // TrungPQ add products.
        appMenu.items.push(
            new AppMenuItem('Product Manager', '', 'flaticon-background', '', [
                new AppMenuItem('Categories', '', 'flaticon-pie-chart', '/app/main/products/productCategories'),
                new AppMenuItem('Products', '', 'flaticon-car', '/app/main/products/products'),
            ])
        );

        appMenu.items.push(
            new AppMenuItem('Plate Inventory Manager', 'Pages.Discs', 'flaticon-menu-1', '', [
                new AppMenuItem('Register Inventory', 'Pages.Discs', 'flaticon-menu-1', '/app/main/plate/dishes'),
                new AppMenuItem('Check Inventory', 'Pages.Discs', 'flaticon-menu-1', '/app/main/plate/check-dishes'),
            ])

        )
        appMenu.items.push(
            new AppMenuItem('Menu Scheduler', 'Pages.PlateMenus', 'flaticon-calendar-2', '', [
                new AppMenuItem('Sessions', 'Pages.Sessions', 'flaticon-time', '/app/main/machines/sessions'),
                new AppMenuItem('Menu Scheduler', 'Pages.PlateMenus', 'flaticon-calendar-2', '/app/main/plate/plateMenus'),
                new AppMenuItem('Menu Calendar', 'Pages.PlateMenus', 'flaticon-calendar-2', '/app/main/plate/plateMenuCalendar'),
            ])
        )

        appMenu.items.push(
            new AppMenuItem('User Access Manager', 'Pages.Trays', 'flaticon-menu-1', '', [
                new AppMenuItem('Trays', 'Pages.Trays', 'flaticon-interface-3', '/app/main/plate/trays')
                // new AppMenuItem('Take Away', 'Pages.Trays', 'flaticon-interface-3', '/app/main/plate/take-away-trays')
            ])
        )

        appMenu.items.push(
            new AppMenuItem('Device Settings', 'Pages.DeviceSetting', 'flaticon-settings', '', [
                new AppMenuItem('RFID table settings', 'Pages.DeviceSetting', 'flaticon-interface-3', '/app/main/devicesetting/rfid-table'),
                // new AppMenuItem('MDBCashless Diagnostic', 'Pages.DeviceSetting', 'flaticon-interface-3', '/app/main/devicesetting/mdbcashless-diagnostic'),
                new AppMenuItem('Service status', '', 'flaticon-line-graph', '/app/main/service-status')
            ])
        )

        appMenu.items.push(
            new AppMenuItem('System Setting', 'Pages.SystemSetting', 'flaticon-laptop', '/app/main/system-settings')
        )

        appMenu.items.push(
            new AppMenuItem('Transactions', 'Pages.Transactions', 'flaticon-signs-1', '', [
                new AppMenuItem('Success', 'Pages.Transactions', 'flaticon-signs-1', '/app/main/transactions-success'),
                new AppMenuItem('Error', 'Pages.Transactions', 'flaticon-signs-1', '/app/main/transactions-error')
            ])
        )
        // appMenu.items.push(
        //     new AppMenuItem('Transactions', 'Pages.Transactions', 'flaticon-signs-1', '/app/main/transactions')
        // )

        // appMenu.items.push(
        //     new AppMenuItem('Transactions Error', 'Pages.Transactions', 'flaticon-signs-1', '/app/main/transactions-error')
        // )

        appMenu.items.push(new AppMenuItem('Administration', '', 'flaticon-interface-8', '', [
            new AppMenuItem('OrganizationUnits', 'Pages.Administration.OrganizationUnits', 'flaticon-map', '/app/admin/organization-units'),
            new AppMenuItem('Roles', 'Pages.Administration.Roles', 'flaticon-suitcase', '/app/admin/roles'),
            new AppMenuItem('Users', 'Pages.Administration.Users', 'flaticon-users', '/app/admin/users'),
            new AppMenuItem('Languages', 'Pages.Administration.Languages', 'flaticon-tabs', '/app/admin/languages'),
            new AppMenuItem('AuditLogs', 'Pages.Administration.AuditLogs', 'flaticon-folder-1', '/app/admin/auditLogs'),
            new AppMenuItem('Maintenance', 'Pages.Administration.Host.Maintenance', 'flaticon-lock', '/app/admin/maintenance'),
            new AppMenuItem('Subscription', 'Pages.Administration.Tenant.SubscriptionManagement', 'flaticon-refresh', '/app/admin/subscription-management'),
            new AppMenuItem('VisualSettings', 'Pages.Administration.UiCustomization', 'flaticon-medical', '/app/admin/ui-customization'),
            new AppMenuItem('Settings', 'Pages.Administration.Host.Settings', 'flaticon-settings', '/app/admin/hostSettings'),
            new AppMenuItem('Settings', 'Pages.Administration.Tenant.Settings', 'flaticon-settings', '/app/admin/tenantSettings'),
            new AppMenuItem('SyncSettings', 'Pages.Administration.SyncSetting', 'flaticon-settings', '/app/admin/syncsetting'),
        ]));

        return appMenu;
    }

    checkChildMenuItemPermission(menuItem): boolean {

        for (let i = 0; i < menuItem.items.length; i++) {
            let subMenuItem = menuItem.items[i];

            if (subMenuItem.permissionName) {
                return this._permissionCheckerService.isGranted(subMenuItem.permissionName);
            } else if (subMenuItem.items && subMenuItem.items.length) {
                return this.checkChildMenuItemPermission(subMenuItem);
            }
            return true;
        }

        return false;
    }

    showMenuItem(menuItem: AppMenuItem): boolean {
        if (menuItem.permissionName === 'Pages.Administration.Tenant.SubscriptionManagement' && this._appSessionService.tenant && !this._appSessionService.tenant.edition) {
            return false;
        }

        let hideMenuItem = false;

        if (menuItem.requiresAuthentication && !this._appSessionService.user) {
            hideMenuItem = true;
        }

        if (menuItem.permissionName && !this._permissionCheckerService.isGranted(menuItem.permissionName)) {
            hideMenuItem = true;
        }

        if (menuItem.hasFeatureDependency() && !menuItem.featureDependencySatisfied()) {
            hideMenuItem = true;
        }

        if (!hideMenuItem && menuItem.items && menuItem.items.length) {
            return this.checkChildMenuItemPermission(menuItem);
        }

        return !hideMenuItem;
    }

    getRfidTableEabled(): boolean {
        return (!this._appSessionService.tenantId || this.feature.isEnabled('App.RFIDTableFeature'));
    }
}
