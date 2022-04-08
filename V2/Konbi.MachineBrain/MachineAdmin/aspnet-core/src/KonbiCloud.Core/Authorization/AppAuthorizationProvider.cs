using Abp.Authorization;
using Abp.Configuration.Startup;
using Abp.Localization;
using Abp.MultiTenancy;

namespace KonbiCloud.Authorization
{
    /// <summary>
    /// Application's authorization provider.
    /// Defines permissions for the application.
    /// See <see cref="AppPermissions"/> for all permission names.
    /// </summary>
    public class AppAuthorizationProvider : AuthorizationProvider
    {
        private readonly bool _isMultiTenancyEnabled;

        public AppAuthorizationProvider(bool isMultiTenancyEnabled)
        {
            _isMultiTenancyEnabled = isMultiTenancyEnabled;
        }

        public AppAuthorizationProvider(IMultiTenancyConfig multiTenancyConfig)
        {
            _isMultiTenancyEnabled = multiTenancyConfig.IsEnabled;
        }

        public override void SetPermissions(IPermissionDefinitionContext context)
        {
            //COMMON PERMISSIONS (FOR BOTH OF TENANTS AND HOST)

            var pages = context.GetPermissionOrNull(AppPermissions.Pages) ?? context.CreatePermission(AppPermissions.Pages, L("Pages"));

            var sessions = pages.CreateChildPermission(AppPermissions.Pages_Sessions, L("Sessions"));
            sessions.CreateChildPermission(AppPermissions.Pages_Sessions_Create, L("CreateNewSession"));
            sessions.CreateChildPermission(AppPermissions.Pages_Sessions_Edit, L("EditSession"));
            sessions.CreateChildPermission(AppPermissions.Pages_Sessions_Delete, L("DeleteSession"));
            sessions.CreateChildPermission(AppPermissions.Pages_Sessions_Export, L("ExportSession"));
            sessions.CreateChildPermission(AppPermissions.Pages_Sessions_Sync, L("SyncSession"));

            var discs = pages.CreateChildPermission(AppPermissions.Pages_Discs, L("Discs"));
            discs.CreateChildPermission(AppPermissions.Pages_Discs_Create, L("CreateNewDisc"));
            discs.CreateChildPermission(AppPermissions.Pages_Discs_Edit, L("EditDisc"));
            discs.CreateChildPermission(AppPermissions.Pages_Discs_Delete, L("DeleteDisc"));
            discs.CreateChildPermission(AppPermissions.Pages_Discs_Sync, L("SyncDisc"));

            var plates = pages.CreateChildPermission(AppPermissions.Pages_Plates, L("Plates"));
            plates.CreateChildPermission(AppPermissions.Pages_Plates_Create, L("CreateNewPlate"));
            plates.CreateChildPermission(AppPermissions.Pages_Plates_Edit, L("EditPlate"));
            plates.CreateChildPermission(AppPermissions.Pages_Plates_Delete, L("DeletePlate"));
            plates.CreateChildPermission(AppPermissions.Pages_Plates_Inventory_Manager, L("InventoryManager"));
            plates.CreateChildPermission(AppPermissions.Pages_Plates_ImportCSV, L("ImportCSV"));
            plates.CreateChildPermission(AppPermissions.Pages_Plates_ExportCSV, L("ExportCSV"));
            plates.CreateChildPermission(AppPermissions.Pages_Plates_Sync, L("SyncPlate"));

            var deviceSetting = pages.CreateChildPermission(AppPermissions.Pages_DeviceSetting, L("DeviceSetting"));
            var systemSetting = pages.CreateChildPermission(AppPermissions.Pages_SystemSetting, L("SystemSetting"));

            var plateCategories = pages.CreateChildPermission(AppPermissions.Pages_PlateCategories, L("PlateCategories"));
            plateCategories.CreateChildPermission(AppPermissions.Pages_PlateCategories_Create, L("CreateNewPlateCategory"));
            plateCategories.CreateChildPermission(AppPermissions.Pages_PlateCategories_Edit, L("EditPlateCategory"));
            plateCategories.CreateChildPermission(AppPermissions.Pages_PlateCategories_Delete, L("DeletePlateCategory"));
            plateCategories.CreateChildPermission(AppPermissions.Pages_PlateCategories_Sync, L("SyncPlateCategory"));

            var Categories = pages.CreateChildPermission(AppPermissions.Pages_Categories, L("Categories"));
            Categories.CreateChildPermission(AppPermissions.Pages_Categories_Create, L("CreateNewCategory"));
            Categories.CreateChildPermission(AppPermissions.Pages_Categories_Edit, L("EditCategory"));
            Categories.CreateChildPermission(AppPermissions.Pages_Categories_Delete, L("DeleteCategory"));
            Categories.CreateChildPermission(AppPermissions.Pages_Categories_Sync, L("SyncCategory"));

            var Products = pages.CreateChildPermission(AppPermissions.Pages_Products, L("Products"));
            Products.CreateChildPermission(AppPermissions.Pages_Products_Create, L("CreateNewProduct"));
            Products.CreateChildPermission(AppPermissions.Pages_Products_Edit, L("EditProduct"));
            Products.CreateChildPermission(AppPermissions.Pages_Products_Delete, L("DeleteProduct"));
            Products.CreateChildPermission(AppPermissions.Pages_Products_Sync, L("SyncProduct"));

            var plateMenus = pages.CreateChildPermission(AppPermissions.Pages_PlateMenus, L("PlateMenus"));
            plateMenus.CreateChildPermission(AppPermissions.Pages_PlateMenus_Create, L("CreateNewPlateMenu"));
            plateMenus.CreateChildPermission(AppPermissions.Pages_PlateMenus_Edit, L("EditPlateMenu"));
            plateMenus.CreateChildPermission(AppPermissions.Pages_PlateMenus_Delete, L("DeletePlateMenu"));
            plateMenus.CreateChildPermission(AppPermissions.Pages_PlateMenus_Import, L("ImportPlateMenu"));
            plateMenus.CreateChildPermission(AppPermissions.Pages_PlateMenus_Export, L("ExportPlateMenu"));
            plateMenus.CreateChildPermission(AppPermissions.Pages_PlateMenus_Sync, L("SyncPlateMenu"));
            plateMenus.CreateChildPermission(AppPermissions.Pages_PlateMenus_Replicate, L("ReplicatePlateMenu"));
            plateMenus.CreateChildPermission(AppPermissions.Pages_PlateMenus_Generate, L("GeneratePlateMenu"));

            var transactions = pages.CreateChildPermission(AppPermissions.Pages_Transactions, L("Transactions"));

            var emps = pages.CreateChildPermission(AppPermissions.Pages_Employees, L("Employees"));

            var trays = pages.CreateChildPermission(AppPermissions.Pages_Trays, L("Trays"));
            trays.CreateChildPermission(AppPermissions.Pages_Trays_Create, L("CreateNewTray"));
            trays.CreateChildPermission(AppPermissions.Pages_Trays_Edit, L("EditTray"));
            trays.CreateChildPermission(AppPermissions.Pages_Trays_Delete, L("DeleteTray"));
            trays.CreateChildPermission(AppPermissions.Pages_Trays_Sync, L("SyncTray"));

            pages.CreateChildPermission(AppPermissions.Pages_DemoUiComponents, L("DemoUiComponents"));

            var administration = pages.CreateChildPermission(AppPermissions.Pages_Administration, L("Administration"));

            var roles = administration.CreateChildPermission(AppPermissions.Pages_Administration_Roles, L("Roles"));
            roles.CreateChildPermission(AppPermissions.Pages_Administration_Roles_Create, L("CreatingNewRole"));
            roles.CreateChildPermission(AppPermissions.Pages_Administration_Roles_Edit, L("EditingRole"));
            roles.CreateChildPermission(AppPermissions.Pages_Administration_Roles_Delete, L("DeletingRole"));

            var users = administration.CreateChildPermission(AppPermissions.Pages_Administration_Users, L("Users"));
            users.CreateChildPermission(AppPermissions.Pages_Administration_Users_Create, L("CreatingNewUser"));
            users.CreateChildPermission(AppPermissions.Pages_Administration_Users_Edit, L("EditingUser"));
            users.CreateChildPermission(AppPermissions.Pages_Administration_Users_Delete, L("DeletingUser"));
            users.CreateChildPermission(AppPermissions.Pages_Administration_Users_ChangePermissions, L("ChangingPermissions"));
            users.CreateChildPermission(AppPermissions.Pages_Administration_Users_Impersonation, L("LoginForUsers"));

            var languages = administration.CreateChildPermission(AppPermissions.Pages_Administration_Languages, L("Languages"));
            languages.CreateChildPermission(AppPermissions.Pages_Administration_Languages_Create, L("CreatingNewLanguage"));
            languages.CreateChildPermission(AppPermissions.Pages_Administration_Languages_Edit, L("EditingLanguage"));
            languages.CreateChildPermission(AppPermissions.Pages_Administration_Languages_Delete, L("DeletingLanguages"));
            languages.CreateChildPermission(AppPermissions.Pages_Administration_Languages_ChangeTexts, L("ChangingTexts"));

            administration.CreateChildPermission(AppPermissions.Pages_Administration_AuditLogs, L("AuditLogs"));

            var organizationUnits = administration.CreateChildPermission(AppPermissions.Pages_Administration_OrganizationUnits, L("OrganizationUnits"));
            organizationUnits.CreateChildPermission(AppPermissions.Pages_Administration_OrganizationUnits_ManageOrganizationTree, L("ManagingOrganizationTree"));
            organizationUnits.CreateChildPermission(AppPermissions.Pages_Administration_OrganizationUnits_ManageMembers, L("ManagingMembers"));

            administration.CreateChildPermission(AppPermissions.Pages_Administration_UiCustomization, L("VisualSettings"));
            administration.CreateChildPermission(AppPermissions.Pages_Administration_SyncSetting, L("SyncSettings"));

            //TENANT-SPECIFIC PERMISSIONS

            pages.CreateChildPermission(AppPermissions.Pages_Tenant_Dashboard, L("Dashboard"), multiTenancySides: MultiTenancySides.Tenant);

            administration.CreateChildPermission(AppPermissions.Pages_Administration_Tenant_Settings, L("Settings"), multiTenancySides: MultiTenancySides.Tenant);
            administration.CreateChildPermission(AppPermissions.Pages_Administration_Tenant_SubscriptionManagement, L("Subscription"), multiTenancySides: MultiTenancySides.Tenant);

            //HOST-SPECIFIC PERMISSIONS

            var editions = pages.CreateChildPermission(AppPermissions.Pages_Editions, L("Editions"), multiTenancySides: MultiTenancySides.Host);
            editions.CreateChildPermission(AppPermissions.Pages_Editions_Create, L("CreatingNewEdition"), multiTenancySides: MultiTenancySides.Host);
            editions.CreateChildPermission(AppPermissions.Pages_Editions_Edit, L("EditingEdition"), multiTenancySides: MultiTenancySides.Host);
            editions.CreateChildPermission(AppPermissions.Pages_Editions_Delete, L("DeletingEdition"), multiTenancySides: MultiTenancySides.Host);

            var tenants = pages.CreateChildPermission(AppPermissions.Pages_Tenants, L("Tenants"), multiTenancySides: MultiTenancySides.Host);
            tenants.CreateChildPermission(AppPermissions.Pages_Tenants_Create, L("CreatingNewTenant"), multiTenancySides: MultiTenancySides.Host);
            tenants.CreateChildPermission(AppPermissions.Pages_Tenants_Edit, L("EditingTenant"), multiTenancySides: MultiTenancySides.Host);
            tenants.CreateChildPermission(AppPermissions.Pages_Tenants_ChangeFeatures, L("ChangingFeatures"), multiTenancySides: MultiTenancySides.Host);
            tenants.CreateChildPermission(AppPermissions.Pages_Tenants_Delete, L("DeletingTenant"), multiTenancySides: MultiTenancySides.Host);
            tenants.CreateChildPermission(AppPermissions.Pages_Tenants_Impersonation, L("LoginForTenants"), multiTenancySides: MultiTenancySides.Host);

            administration.CreateChildPermission(AppPermissions.Pages_Administration_Host_Settings, L("Settings"), multiTenancySides: MultiTenancySides.Host);
            administration.CreateChildPermission(AppPermissions.Pages_Administration_Host_Maintenance, L("Maintenance"), multiTenancySides: _isMultiTenancyEnabled ? MultiTenancySides.Host : MultiTenancySides.Tenant);
            administration.CreateChildPermission(AppPermissions.Pages_Administration_HangfireDashboard, L("HangfireDashboard"), multiTenancySides: _isMultiTenancyEnabled ? MultiTenancySides.Host : MultiTenancySides.Tenant);
            administration.CreateChildPermission(AppPermissions.Pages_Administration_Host_Dashboard, L("Dashboard"), multiTenancySides: MultiTenancySides.Host);
        }

        private static ILocalizableString L(string name)
        {
            return new LocalizableString(name, KonbiCloudConsts.LocalizationSourceName);
        }
    }
}
