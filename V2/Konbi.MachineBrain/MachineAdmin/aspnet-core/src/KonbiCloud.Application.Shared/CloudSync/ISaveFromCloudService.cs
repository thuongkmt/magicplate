using System.Threading.Tasks;
using Abp.Dependency;

namespace KonbiCloud.CloudSync
{
    public interface ISaveFromCloudService:ITransientDependency
    {
        Task<bool> SyncSessionData();
        //Task<bool> SyncTrayData();
        Task<bool> SyncPlateCategoryData();
        Task<bool> SyncPlateData();
        Task<bool> SyncDiscData();
        Task<bool> SyncProductMenuData();
        Task<bool> SyncProductCategoryData();
        Task<bool> SyncProductData();
        Task<bool> SyncSettingsData();
        /// <summary>
        /// This will sync all data from server partially
        /// </summary>
        /// <returns></returns>
        Task<bool> PartiallySyncAllData();
        /// <summary>
        /// the method will delete all master data and sync it fully from server
        /// </summary>
        /// <returns></returns>
        Task<bool> FullySyncAllData();

    }
}
