using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.EntityFrameworkCore.Uow;
using Abp.MultiTenancy;
using Abp.Runtime.Caching;
using Abp.UI;
using Castle.Core.Logging;
using KonbiCloud.Common;
using KonbiCloud.Configuration;
using KonbiCloud.EntityFrameworkCore;
using KonbiCloud.Plate;
using KonbiCloud.Products;
using KonbiCloud.RFIDTable;
using KonbiCloud.RFIDTable.Cache;
using KonbiCloud.Sessions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace KonbiCloud.CloudSync
{
    public class SaveFromCloudService : ISaveFromCloudService
    {
        private readonly ISessionSyncService _sessionSyncService;
        private readonly IRepository<Session, Guid> _sessionRepository;
        private readonly ISettingManager _settingManager;
        private readonly ILogger _logger;
        private readonly IRepository<Plate.Plate, Guid> _plateRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly ICacheManager _cacheManager;
        private readonly IPlateSyncService _plateSyncService;
        private readonly IRepository<PlateCategory> _plateCategoryRepository;
        private readonly IRepository<Category, Guid> _productCategoryRepository;
        private readonly IPlateCategorySyncService _plateCategorySyncService;
        private readonly IProductSyncService _productSyncService;
        private readonly IProductCategorySyncService _productCategorySyncService;
        private readonly IRepository<Disc, Guid> _discRepository;
        private readonly IDishSyncService _dishSyncService;
        private readonly IMachineSyncService _machineSyncService;
        private readonly IRepository<ProductMenu.ProductMenu, Guid> _productMenuRepository;
        private readonly IPlateMenuSyncService _plateMenuSyncService;
        private readonly IRepository<Product, Guid> _productRepository;
        private readonly IRepository<Prices.PriceStrategy, Guid> _priceStrategyRepository;
        private readonly IRepository<Prices.PriceStrategyCode, int> _priceStrategyCodeRepository;
        private readonly IRepository<Category, Guid> _categoriesRepository;
        private readonly IRfidTableSignalRMessageCommunicator _signalRCommunicator;
        private readonly IDetailLogService _detailLogService;
        public ISettingManager SettingManager { get; set; }


        public SaveFromCloudService(ISessionSyncService sessionSyncService,
            IRepository<Session, Guid> sessionRepository,
            ISettingManager settingManager,
            ILogger logger,
            IUnitOfWorkManager unitOfWorkManager,
            IRepository<PlateCategory> plateCategoryRepository,
            IRepository<Category, Guid> productCategoryRepository,
            IPlateCategorySyncService plateCategorySyncService,
            IProductCategorySyncService productCategorySyncService,
            IProductSyncService productSyncService,
            ICacheManager cacheManager,
            IRepository<Disc, Guid> discRepository,
            IDishSyncService dishSyncService,
            IPlateSyncService plateSyncService,
            IRepository<Plate.Plate, Guid> plateRepository,
            IRepository<ProductMenu.ProductMenu, Guid> plateMenuRepository,
            IPlateMenuSyncService plateMenuSyncService,
            IRepository<Category, Guid> categoriesRepository,
            
            IRepository<Prices.PriceStrategy, Guid> priceStrategyRepository,
            IRepository<Prices.PriceStrategyCode, int> priceStrategyCodeRepository,
            IMachineSyncService machineSyncService,
            IRepository<Product, Guid> productRepository,
            IRfidTableSignalRMessageCommunicator signalRCommunicator,
            IDetailLogService detailLogService)
        {
            _sessionSyncService = sessionSyncService;
            _sessionRepository = sessionRepository;
            _settingManager = settingManager;
            _logger = logger;
            _unitOfWorkManager = unitOfWorkManager;
            _plateCategoryRepository = plateCategoryRepository;
            _productCategoryRepository = productCategoryRepository;
            _plateCategorySyncService = plateCategorySyncService;
            _productCategorySyncService = productCategorySyncService;
            _productSyncService = productSyncService;
            _cacheManager = cacheManager;
            _discRepository = discRepository;
            _dishSyncService = dishSyncService;
            _plateSyncService = plateSyncService;
            _plateRepository = plateRepository;
            _productMenuRepository = plateMenuRepository;
            _plateMenuSyncService = plateMenuSyncService;
            _productRepository = productRepository;
            _categoriesRepository = categoriesRepository;            
            _priceStrategyCodeRepository = priceStrategyCodeRepository;
            _priceStrategyRepository = priceStrategyRepository;
            _machineSyncService = machineSyncService;
            _signalRCommunicator = signalRCommunicator;
            _detailLogService = detailLogService;
        }


        public async Task<bool> SyncSessionData()
        {
            _detailLogService.Log($"SyncFromCloud: START sync session");
            var mId = Guid.Empty;
            Guid.TryParse(await _settingManager.GetSettingValueAsync(AppSettingNames.MachineId), out mId);
            if (mId == Guid.Empty)
            {
                _detailLogService.Log($"SyncFromCloud: MachineId is {mId} not existed");
                throw new UserFriendlyException("Machine configuration error");

            }

            var data = await _sessionSyncService.Sync(mId);
            if (data == null)
            {
                _detailLogService.Log($"SyncFromCloud: Data of session is null");
                throw new UserFriendlyException("Cannot get Sessions from server");
            }
            _detailLogService.Log($"SyncFromCloud: Data of session -> {JsonConvert.SerializeObject(data)}");
            var hasUpdates = data.DeletionEntities.Any() || data.ModificationEntities.Any();

            using (var uow = _unitOfWorkManager.Begin())
            {
                var existingEntities = new List<Session>();
                using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
                {
                    existingEntities = await _sessionRepository.GetAllListAsync();
                }
                try
                {
                    SessionInfo sessionInfo = null;
                    bool changeSession = false;
                    foreach (var modification in data.ModificationEntities)
                    {

                        var existingEntity = existingEntities.FirstOrDefault(el => el.Id == modification.Id);

                        if (existingEntity != null)
                        {
                            existingEntity.Name = modification.Name;
                            existingEntity.ActiveFlg = modification.ActiveFlg;
                            existingEntity.TenantId = 1;
                            existingEntity.IsDeleted = false;
                            existingEntity.FromHrs = modification.FromHrs;
                            existingEntity.ToHrs = modification.ToHrs;                            

                        }
                        else
                        {
                            var adding = new Session()
                            {

                                Id = modification.Id,
                                Name = modification.Name,
                                TenantId = 1,
                                ToHrs = modification.ToHrs,
                                FromHrs = modification.FromHrs,
                                ActiveFlg = modification.ActiveFlg,
                                

                            };

                            await _sessionRepository.InsertAsync(adding);
                        }
                        //start: update realtime Session when change from Server side on Customer UI
                        var currentTime = Convert.ToInt32(string.Format("{0:00}{1:00}", DateTime.Now.Hour, DateTime.Now.Minute));
                        
                        if (modification != null 
                            && Convert.ToInt32(currentTime) < Convert.ToInt32(modification.ToHrs.Replace(":", "")) 
                            && Convert.ToInt32(currentTime) > Convert.ToInt32(modification.FromHrs.Replace(":", "")))
                        {
                            sessionInfo = new SessionInfo {
                                Id = modification.Id,
                                Name = modification.Name,
                                FromHrs = modification.FromHrs,
                                ToHrs = modification.ToHrs
                            };
                            changeSession = true;
                        }
                        //end: update realtime Session when change from Server side on Customer UI
                    }
                    //Delete entities
                    foreach (var deletion in data.DeletionEntities)
                    {
                        await _sessionRepository.DeleteAsync(deletion.Id);
                    }

                    await _settingManager.ChangeSettingForTenantAsync(1, AppSettingNames.SessionEntityLastSynced, data.LastSyncedTimeStamp.ToString());
                    if (hasUpdates)
                    {
                        await _unitOfWorkManager.Current.SaveChangesAsync();
                        await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();
                    }
                    await uow.CompleteAsync();
                    _detailLogService.Log($"Synced sessions sucessfully. {data.DeletionEntities.Count} deletions, {data.ModificationEntities.Count} modifications, lastSynced: {data.LastSyncedTimeStamp.FromUnixTime().ToLocalTime()}.");
                    // TrungPQ: Clear cache after sync.   

                    //start: update realtime Session when change from Server side on Customer UI
                    if (changeSession)
                    {
                        await _signalRCommunicator.UpdateSessionInfo(sessionInfo);
                    }
                    //end: update realtime Session when change from Server side on Customer UI

                    _detailLogService.Log($"SyncFromCloud: END session");
                    return true;
                }
                catch (Exception ex)
                {
                    _detailLogService.Log($"SyncFromCloud: END session with error -> {ex.ToString()}");
                    throw new UserFriendlyException("Sync sessions failed");
                }
            }           
        }

        public async Task<bool> SyncProductCategoryData()
        {

            _detailLogService.Log($"SyncFromCloud: START sync ProductCategory");
            var mId = Guid.Empty;
            Guid.TryParse(await _settingManager.GetSettingValueAsync(AppSettingNames.MachineId), out mId);
            if (mId == Guid.Empty)
            {
                _detailLogService.Log($"SyncFromCloud: MachineId is {mId} not existed");
                throw new UserFriendlyException("Machine configuration error");
            }

            var data = await _productCategorySyncService.Sync(mId);
            if (data == null)
            {
                _detailLogService.Log($"SyncFromCloud: Data of ProductCategory is null");
                throw new UserFriendlyException("Cannot get Product Category from server");
            }
            _detailLogService.Log($"SyncFromCloud: Data of ProductCategory -> {JsonConvert.SerializeObject(data)}");
            var hasUpdates = data.DeletionEntities.Count > 0 || data.ModificationEntities.Count > 0;
            using (var uow = _unitOfWorkManager.Begin())
            {
                var existingEntities = new List<Category>();
                using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
                {
                    existingEntities = await _categoriesRepository.GetAllListAsync();
                }
                try
                {
                    foreach (var modification in data.ModificationEntities)
                    {

                        var existingEntity = existingEntities.FirstOrDefault(el => el.Id == modification.Id);

                        if (existingEntity != null)
                        {
                            existingEntity.Name = modification.Name;
                            existingEntity.Description = modification.Description;
                            existingEntity.TenantId = 1;
                            existingEntity.IsDeleted = false;

                        }
                        else
                        {
                            var adding = new Category()
                            {

                                Id = modification.Id,
                                Name = modification.Name,
                                Description = modification.Description,
                                ImageUrl = modification.ImageUrl,
                                TenantId = 1,

                            };

                            await _categoriesRepository.InsertAsync(adding);
                        }
                    }
                    //Delete entities
                    foreach (var deletion in data.DeletionEntities)
                    {
                        await _categoriesRepository.DeleteAsync(deletion.Id);
                    }

                    await _settingManager.ChangeSettingForTenantAsync(1, AppSettingNames.CategoryEntityLastSynced, data.LastSyncedTimeStamp.ToString());
                    if (hasUpdates)
                    {
                        await _unitOfWorkManager.Current.SaveChangesAsync();
                        await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();
                    }
                    await uow.CompleteAsync();
                    _detailLogService.Log($"SyncFromCloud: END ProductCategory sucessfully. {data.DeletionEntities.Count} deletions, {data.ModificationEntities.Count} modifications, lastSynced: {data.LastSyncedTimeStamp.FromUnixTime().ToLocalTime()}.");
                    // TrungPQ: Clear cache after sync.                    

                    return true;
                }
                catch (Exception ex)
                {
                    _detailLogService.Log($"SyncFromCloud: END session with error -> {ex.ToString()}");
                    throw new UserFriendlyException("Sync product categories failed");
                }
            }
        }

        public async Task<bool> SyncProductData()
        {
            _detailLogService.Log($"SyncFromCloud: START sync Product");
            var mId = Guid.Empty;
            Guid.TryParse(await _settingManager.GetSettingValueAsync(AppSettingNames.MachineId), out mId);
            if (mId == Guid.Empty)
            {
                _detailLogService.Log($"SyncFromCloud: MachineId is {mId} not existed");
                throw new UserFriendlyException("Machine configuration error");
            }

            var data = await _productSyncService.Sync(mId);
            if (data == null)
            {
                _detailLogService.Log($"SyncFromCloud: Data of Product is null");
                throw new UserFriendlyException("Cannot get Products from server");
            }
            _detailLogService.Log($"SyncFromCloud: Data of Product -> {JsonConvert.SerializeObject(data)}");
            var hasUpdates = data.DeletionEntities.Any() || data.ModificationEntities.Any();
            using (var uow = _unitOfWorkManager.Begin())
            {
                //var syncedList = new SyncedItemData<Guid> { MachineId = mId, SyncedItems = new List<Guid>() };
                var categories = new List<Category>();        
                var existingEntities = new List<Product>();
                using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MustHaveTenant,
                    AbpDataFilters.MayHaveTenant, AbpDataFilters.SoftDelete))
                {
                    categories = await _productCategoryRepository.GetAllListAsync();
                   
                    existingEntities = await _productRepository.GetAllIncluding(x => x.Category).ToListAsync();
                }
                try
                {
                    foreach (var modification in data.ModificationEntities)
                    {

                        var existingEntity = existingEntities.FirstOrDefault(el => el.Id == modification.Id);

                        if (existingEntity != null)
                        {
                            existingEntity.IsDeleted = false;
                            existingEntity.Name = modification.Name;
                            existingEntity.ImageUrl = modification.ImageUrl;
                            existingEntity.Desc = modification.Desc;
                            existingEntity.DisplayOrder = modification.DisplayOrder;
                            existingEntity.Barcode = modification.Barcode;
                            existingEntity.ContractorPrice = modification.ContractorPrice;
                            existingEntity.FromDate = modification.FromDate;
                            existingEntity.ToDate = modification.ToDate;
                            existingEntity.Unit = modification.Unit;
                            existingEntity.SKU = modification.SKU;
                            existingEntity.ShortDesc3 = modification.ShortDesc3;
                            existingEntity.ShortDesc2 = modification.ShortDesc2;
                            existingEntity.ShortDesc1 = modification.ShortDesc1;
                            existingEntity.Price = modification.Price;
                            //
                            
                            existingEntity.ImageChecksum = modification.ImageChecksum;
                            existingEntity.Id = modification.Id;

                            //category

                            existingEntity.TenantId = 1;

                            if (modification.CategoryId.HasValue)
                            {
                                var cat = categories.FirstOrDefault(el => el.Id == modification.CategoryId);
                                if (cat == null)
                                {
                                    cat = new Category() { Id = modification.CategoryId.Value, TenantId = 1, IsDeleted = true };
                                    await _categoriesRepository.InsertAsync(cat);
                                    categories.Add(cat);
                                }
                                existingEntity.Category = cat;
                                existingEntity.CategoryId = cat.Id;
                            }
                            else
                            {
                                existingEntity.Category = null;
                                existingEntity.CategoryId = null;
                            }

                        }
                        else
                        {
                            var adding = new Product
                            {

                                IsDeleted = false,
                                Name = modification.Name,
                                ImageUrl = modification.ImageUrl,
                                Desc = modification.Desc,
                                DisplayOrder = modification.DisplayOrder,
                                Barcode = modification.Barcode,
                                ContractorPrice = modification.ContractorPrice,
                                FromDate = modification.FromDate,
                                ToDate = modification.ToDate,
                                Unit = modification.Unit,
                                SKU = modification.SKU,
                                ShortDesc3 = modification.ShortDesc3,
                                ShortDesc2 = modification.ShortDesc2,
                                ShortDesc1 = modification.ShortDesc1,
                                Price = modification.Price,
                                
                                ImageChecksum = modification.ImageChecksum,
                                Id = modification.Id,
                            };
                            if (modification.CategoryId.HasValue)
                            {
                                var cat = categories.FirstOrDefault(el => el.Id == modification.CategoryId);
                                if (cat == null)
                                {
                                    cat = new Category() { Id = modification.CategoryId.Value, TenantId = 1, IsDeleted = true };
                                    await _categoriesRepository.InsertAsync(cat);
                                    categories.Add(cat);
                                }
                                adding.Category = cat;
                                adding.CategoryId = cat.Id;
                            }
                            else
                            {
                                adding.Category = null;
                                adding.CategoryId = null;
                            }

                            await _productRepository.InsertAsync(adding);
                        }
                    }
                    //Delete entities
                    foreach (var deletion in data.DeletionEntities)
                    {
                        await _productRepository.DeleteAsync(deletion.Id);
                    }

                    await _settingManager.ChangeSettingForTenantAsync(1, AppSettingNames.ProductEntityLastSynced, data.LastSyncedTimeStamp.ToString());
                    if (hasUpdates)
                    {
                        await _unitOfWorkManager.Current.SaveChangesAsync();
                        await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();
                    }
                    await uow.CompleteAsync();
                    _logger.Info($"SyncFromCloud: products sucessfully. {data.DeletionEntities.Count} deletions, {data.ModificationEntities.Count} modifications, lastSynced: {data.LastSyncedTimeStamp.FromUnixTime().ToLocalTime()}.");
                    // TrungPQ: Clear cache after sync.                    

                    return true;
                }
                catch (Exception ex)
                {
                    _detailLogService.Log($"SyncFromCloud: END Product with error -> {ex.ToString()}");
                    throw new UserFriendlyException("Sync products failed");
                }
            }

        }

        public async Task<bool> SyncPlateCategoryData()
        {

            _detailLogService.Log($"SyncFromCloud: START sync PlateCategory");
            var mId = Guid.Empty;

            Guid.TryParse(await _settingManager.GetSettingValueAsync(AppSettingNames.MachineId), out mId);
            if (mId == Guid.Empty)
            {
                _detailLogService.Log($"SyncFromCloud: MachineId is {mId} not existed");
                throw new UserFriendlyException("Machine configuration error");
            }

            var data = await _plateCategorySyncService.Sync(mId);
            if (data == null)
            {
                _detailLogService.Log($"SyncFromCloud: Data of PlateCategory is null");
                throw new UserFriendlyException("Cannot get Plate Category from server");
            }
            _detailLogService.Log($"SyncFromCloud: Data of PlateCategory -> {JsonConvert.SerializeObject(data)}");
            var hasUpdates = data.DeletionEntities.Count > 0 || data.ModificationEntities.Count > 0;
            using (var uow = _unitOfWorkManager.Begin())
            {
                var existingEntities = new List<PlateCategory>();
                using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
                {
                    existingEntities = await _plateCategoryRepository.GetAllListAsync();
                }
                try
                {
                    foreach (var modification in data.ModificationEntities)
                    {

                        var existingEntity = existingEntities.FirstOrDefault(el => el.Id == modification.Id);

                        if (existingEntity != null)
                        {
                            existingEntity.Name = modification.Name;
                            existingEntity.Desc = modification.Desc;
                            existingEntity.TenantId = 1;
                            existingEntity.IsDeleted = false;

                        }
                        else
                        {
                            var adding = new PlateCategory()
                            {

                                Id = modification.Id,
                                Name = modification.Name,
                                Desc = modification.Desc,
                                TenantId = 1
                            };

                            await _plateCategoryRepository.InsertAsync(adding);
                        }
                    }
                    //Delete entities
                    foreach (var deletion in data.DeletionEntities)
                    {
                        await _plateCategoryRepository.DeleteAsync(deletion.Id);
                    }

                    await _settingManager.ChangeSettingForTenantAsync(1, AppSettingNames.PlateCategoryEntityLastSynced, data.LastSyncedTimeStamp.ToString());
                    if (hasUpdates)
                    {
                        await _unitOfWorkManager.Current.SaveChangesAsync();
                        await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();
                    }
                    await uow.CompleteAsync();
                    _detailLogService.Log($"SyncFromCloud: plateCategories sucessfully. {data.DeletionEntities.Count} deletions, {data.ModificationEntities.Count} modifications, lastSynced: {data.LastSyncedTimeStamp.FromUnixTime().ToLocalTime()}.");
                    // TrungPQ: Clear cache after sync.                    

                    return true;
                }
                catch (Exception ex)
                {
                    _detailLogService.Log($"SyncFromCloud: END plateCategories with error -> {ex.ToString()}");
                    throw new UserFriendlyException("Sync plate categories failed");
                }
            }
        }

        public async Task<bool> SyncPlateData()
        {

            _detailLogService.Log($"SyncFromCloud: START sync Plate");
            var mId = Guid.Empty;

            Guid.TryParse(await _settingManager.GetSettingValueAsync(AppSettingNames.MachineId), out mId);
            if (mId == Guid.Empty)
            {
                _detailLogService.Log($"SyncFromCloud: MachineId is {mId} not existed");
                throw new UserFriendlyException("Machine configuration error");
            }

            var data = await _plateSyncService.Sync(mId);
            if (data == null)
            {
                _detailLogService.Log($"SyncFromCloud: Data of Plate is null");
                throw new UserFriendlyException("Cannot get Plates from server");
            }
            _detailLogService.Log($"SyncFromCloud: Data of Plate -> {JsonConvert.SerializeObject(data)}");
            var hasUpdates = data.DeletionEntities.Count > 0 || data.ModificationEntities.Count > 0;
            using (var uow = _unitOfWorkManager.Begin())
            {
                var existingEntities = new List<Plate.Plate>();
                var plateCategories = new List<PlateCategory>();
                using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
                {
                    existingEntities = await _plateRepository.GetAllIncluding(el=> el.PlateCategory).ToListAsync();
                    plateCategories = await _plateCategoryRepository.GetAllListAsync();
                }
                try
                {
                    foreach (var modification in data.ModificationEntities)
                    {

                        var existingEntity = existingEntities.FirstOrDefault(el => el.Id == modification.Id);

                        if (existingEntity != null)
                        {
                            existingEntity.IsDeleted = false;
                            existingEntity.Name = modification.Name;
                            existingEntity.ImageUrl = modification.ImageUrl;
                            existingEntity.Desc = modification.Desc;
                            existingEntity.Code = modification.Code;
                            existingEntity.Avaiable = modification.Avaiable;
                            existingEntity.Color = modification.Color;
                            existingEntity.TenantId = 1;
                            if (modification.PlateCategoryId.HasValue)
                            {
                                var category = plateCategories.FirstOrDefault(el=> el.Id == modification.PlateCategoryId.Value);
                                if(category ==null)
                                {
                                    category = new PlateCategory()
                                    {
                                        Id = modification.PlateCategoryId.Value,
                                        TenantId = 1,
                                        Name = "NotSyncedYet",
                                        IsDeleted = true
                                    };
                                    await _plateCategoryRepository.InsertAsync(category);
                                    plateCategories.Add(category);
                                }
                                existingEntity.PlateCategory =category;
                            }
                            else
                            {
                                existingEntity.PlateCategory = null;
                                existingEntity.PlateCategoryId = null;
                            }

                            existingEntity.Type = modification.Type;

                        }
                        else
                        {
                            var adding = new Plate.Plate()
                            {

                                Id = modification.Id,
                                IsDeleted = false,
                                Name = modification.Name,
                                ImageUrl = modification.ImageUrl,
                                Desc = modification.Desc,
                                Code = modification.Code,
                                Avaiable = modification.Avaiable,
                                Color = modification.Color,
                                TenantId = 1,
                                Type = modification.Type
                            };
                            if (modification.PlateCategoryId.HasValue)
                            {
                                var category = plateCategories.FirstOrDefault(el => el.Id == modification.PlateCategoryId.Value);
                                if (category == null)
                                {
                                    //create temporarily plate category to get Plate syncing goes through
                                    category = new PlateCategory()
                                    {
                                        Id = modification.PlateCategoryId.Value,
                                        TenantId = 1,
                                        Name = "NotSyncedYet",
                                        IsDeleted = true
                                    };
                                    plateCategories.Add(category);
                                }
                                adding.PlateCategory = category;
                            }

                            await _plateRepository.InsertAsync(adding);
                        }
                    }
                    //Delete entities
                    foreach (var deletion in data.DeletionEntities)
                    {
                        await _plateRepository.DeleteAsync(deletion.Id);
                    }

                    await _settingManager.ChangeSettingForTenantAsync(1, AppSettingNames.PlateEntityLastSynced, data.LastSyncedTimeStamp.ToString());
                    if (hasUpdates)
                    {
                        await _unitOfWorkManager.Current.SaveChangesAsync();
                        await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();
                    }
                    await uow.CompleteAsync();
                    _detailLogService.Log($"SyncFromCloud: Plate sucessfully. {data.DeletionEntities.Count} deletions, {data.ModificationEntities.Count} modifications, lastSynced: {data.LastSyncedTimeStamp.FromUnixTime().ToLocalTime()}.");
                    // TrungPQ: Clear cache after sync.                    

                    return true;
                }
                catch (Exception ex)
                {
                    _detailLogService.Log($"SyncFromCloud: END Plate with error -> {ex.ToString()}");
                    throw new UserFriendlyException("Sync plate model failed");
                }
            }
        }

        public async Task<bool> SyncDiscData()
        {
            _detailLogService.Log($"SyncFromCloud: START sync Discs");
            var mId = Guid.Empty;
            Guid.TryParse(await _settingManager.GetSettingValueAsync(AppSettingNames.MachineId), out mId);
            if (mId == Guid.Empty)
            {
                _detailLogService.Log($"SyncFromCloud: MachineId is {mId} not existed");
                throw new UserFriendlyException("Machine configuration error");
            }

            var data = await _dishSyncService.Sync(mId);
            if (data == null)
            {
                _detailLogService.Log($"SyncFromCloud: Data of Discs is null");
                throw new UserFriendlyException("Cannot get inventory from server");
            }
            _detailLogService.Log($"SyncFromCloud: Data of Discs -> {JsonConvert.SerializeObject(data)}");
            var hasUpdates = data.DeletionEntities.Any() || data.ModificationEntities.Any();
            using (var uow = _unitOfWorkManager.Begin())
            {
                var plates = new List<Plate.Plate>();
                var existingEntities = new List<Disc>();

                var priceStrategyContractor = await _priceStrategyCodeRepository.FirstOrDefaultAsync(x => x.Code.Equals(PlateConsts.Contractor));
                if (priceStrategyContractor == null)
                {
                    priceStrategyContractor = new Prices.PriceStrategyCode() { Code = PlateConsts.Contractor };
                    await _priceStrategyCodeRepository.InsertAsync(priceStrategyContractor);
                }
                using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
                {
                    plates = await _plateRepository.GetAllListAsync();

                    existingEntities = await _discRepository.GetAllIncluding(el => el.Plate).ToListAsync();
                }

                try
                {
                    foreach (var modification in data.ModificationEntities)
                    {
                        var existingEntity = existingEntities.FirstOrDefault(el => el.Id == modification.Id);
                        if (existingEntity != null)
                        {
                            if (modification.PlateId != null && modification.PlateId != Guid.Empty)
                            {
                                var plate = plates.FirstOrDefault(el => el.Id == modification.PlateId);
                                if (plate == null)
                                {
                                    plate = new Plate.Plate() { Id = modification.Id, TenantId = 1, IsDeleted = true, Code = modification.Code };
                                    plates.Add(plate);
                                }
                                existingEntity.Plate = plate;


                            }
                            else
                            {
                                existingEntity.Plate = null;
                            }
                            existingEntity.Code = modification.Code;
                            existingEntity.Uid = modification.Uid;
                            existingEntity.TenantId = 1;
                            existingEntity.IsDeleted = false;
                            existingEntity.IsSynced = true;

                        }
                        else
                        {
                            var adding = new Disc
                            {                                
                                TenantId = 1,
                                Id = modification.Id,
                                Code = modification.Code,
                                Uid = modification.Uid,
                                IsSynced = true,
                                
                            };
                            if (modification.PlateId!=null && modification.PlateId!=Guid.Empty)
                            {

                                var plate = plates.FirstOrDefault(el => el.Id == modification.PlateId);
                                if(plate==null)
                                {
                                    plate = new Plate.Plate() { Id =modification.PlateId, TenantId = 1, IsDeleted = true, Code = modification.Code };
                                    plates.Add(plate);
                                }
                                adding.Plate = plate;
                            }
                            await _discRepository.InsertAsync(adding);
                        }
                    }
                    //Delete entities
                    foreach (var deletion in data.DeletionEntities)
                    {
                        await _discRepository.DeleteAsync(deletion.Id);
                    }
                    await _settingManager.ChangeSettingForTenantAsync(1, AppSettingNames.InventoryEntityLastSynced, data.LastSyncedTimeStamp.ToString());
                    if (hasUpdates)
                    {
                        await _unitOfWorkManager.Current.SaveChangesAsync();
                        // TrungPQ: Clear cache after sync.
                        await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();
                    }
                    await uow.CompleteAsync();
                    _detailLogService.Log($"SyncFromCloud: Discs sucessfully. {data.DeletionEntities.Count} deletions, {data.ModificationEntities.Count} modifications, lastSynced: {data.LastSyncedTimeStamp.FromUnixTime().ToLocalTime()}.");

                    return true;
                }
                catch (Exception ex)
                {
                    _detailLogService.Log($"SyncFromCloud: END Discs with error -> {ex.ToString()}");
                    throw new UserFriendlyException("Sync inventory failed");
                }
            }
           
        }
        
        public async Task<bool> SyncProductMenuData()
        {
            _detailLogService.Log($"SyncFromCloud: START sync ProductMenu");
            var machineId = Guid.Empty;
            Guid.TryParse(await _settingManager.GetSettingValueAsync(AppSettingNames.MachineId), out machineId);
            if (machineId == Guid.Empty)
            {
                _detailLogService.Log($"SyncFromCloud: MachineId is {machineId} not existed");
                throw new UserFriendlyException("Machine configuration error");
            }

            var data = await _plateMenuSyncService.Sync(machineId);
            if (data == null)
            {
                _detailLogService.Log($"SyncFromCloud: Data of ProductMenu is null");
                throw new UserFriendlyException("Cannot get Product Menus from server");
            }
            _detailLogService.Log($"SyncFromCloud: Data of ProductMenu -> {JsonConvert.SerializeObject(data)}");
            var hasUpdates = data.DeletionEntities.Count > 0 || data.ModificationEntities.Count > 0;
            using (var uow = _unitOfWorkManager.Begin())
            {
                var products = new List<Product>();
                var sessions = new List<Session>();
                var plates = new List<Plate.Plate>();
                var existingEntities = new List<ProductMenu.ProductMenu>();

                using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
                {
                    products = await _productRepository.GetAllListAsync();
                    sessions = await _sessionRepository.GetAllListAsync();
                    plates = await _plateRepository.GetAllListAsync();
                    existingEntities = await _productMenuRepository.GetAll()
                         .Where(x => x.SelectedDate >= DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc).Date)
                         .ToListAsync();
                }
                int coutSync = 0;
                try
                {
                    foreach (var modification in data.ModificationEntities)
                    {

                        if (modification.ProductId != null && !products.Any(x => x.Id == modification.ProductId))
                        {
                            // if product is not synced yet then create a temporarily deleted product to make connection
                            var newProduct = new Product() { Id = modification.ProductId, TenantId = 1, IsDeleted = true };
                            await _productRepository.InsertAsync(newProduct);
                            products.Add(newProduct);
                        }
                        if (modification.SessionId != null && !sessions.Any(x => x.Id == modification.SessionId))
                        {
                            // if session is not synced yet then create a temporarily deleted session to make connection
                            var newSession = new Session { Id = modification.SessionId, TenantId = 1, Name="NotSyncedYet", IsDeleted = true };
                            await _sessionRepository.InsertAsync(newSession);
                            sessions.Add(newSession);
                            
                        }
                        if (modification.PlateId.HasValue && !plates.Any(x => x.Id == modification.PlateId))
                        {
                            // if session is not synced yet then create a temporarily deleted session to make connection
                            var newPlate = new Plate.Plate { Id = modification.PlateId.Value, TenantId = 1, Name = "NotSyncedYet", IsDeleted = true };
                            await _plateRepository.InsertAsync(newPlate);
                            plates.Add(newPlate);

                        }

                        var existingEntity = existingEntities.FirstOrDefault(el => el.Id == modification.Id);

                        if (existingEntity != null)
                        {
                            existingEntity.ContractorPrice = modification.PriceStrategy;
                            existingEntity.SessionId = modification.SessionId;
                            existingEntity.SelectedDate = modification.SelectedDate;
                            existingEntity.ProductId = modification.ProductId;
                            existingEntity.Id = modification.Id;
                            existingEntity.DisplayOrder = modification.DisplayOrder;
                            existingEntity.Price = modification.Price;
                            existingEntity.IsDeleted = false;
                            existingEntity.TenantId = 1;
                            existingEntity.PlateId = modification.PlateId;
                            coutSync++;
                        }
                        else
                        {
                            var adding = new ProductMenu.ProductMenu()
                            {
                                ContractorPrice = modification.PriceStrategy,
                                SessionId = modification.SessionId,
                                SelectedDate = modification.SelectedDate,
                                ProductId = modification.ProductId,   
                                PlateId = modification.PlateId,
                                Id = modification.Id,
                                DisplayOrder = modification.DisplayOrder,
                                Price = modification.Price,
                                TenantId = 1
                            };
                          

                            await _productMenuRepository.InsertAsync(adding);
                            coutSync++;
                        }
                    }
                    //Delete entities
                    foreach (var deletion in data.DeletionEntities)
                    {
                        await _productMenuRepository.DeleteAsync(deletion.Id);
                        coutSync++;
                    }
                   
                    if (hasUpdates)
                    {
                        await _unitOfWorkManager.Current.SaveChangesAsync();
                        // TrungPQ: Clear cache after sync.
                        await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();
                    }
                    await uow.CompleteAsync();
                    if (coutSync > 0)
                    {
                        await _settingManager.ChangeSettingForTenantAsync(1, AppSettingNames.ProductMenuEntityLastSynced, data.LastSyncedTimeStamp.ToString());
                    }
                    _detailLogService.Log($"SyncFromCloud: productMenus sucessfully. {data.DeletionEntities.Count} deletions, {data.ModificationEntities.Count} modifications, lastSynced: {data.LastSyncedTimeStamp.FromUnixTime().ToLocalTime()}.");

                    return true;
                }
                catch (Exception ex)
                {
                    _detailLogService.Log($"SyncFromCloud: END productMenus with error -> {ex.ToString()}");
                    throw new UserFriendlyException("Sync Product Menus failed");
                }
            }
        }
        public async Task<bool> SyncSettingsData()
        {
            var mId = Guid.Empty;
            Guid.TryParse(await _settingManager.GetSettingValueAsync(AppSettingNames.MachineId), out mId);
            if (mId == Guid.Empty)
            {
                throw new UserFriendlyException("Machine configuration error");
            }

            var machine = await _machineSyncService.GetMachineFromServer(mId.ToString());
            if (machine!=null &&machine.Settings != null)
            {
                if (machine.Settings.TaxSettings != null)
                {
                    await SettingManager.ChangeSettingForTenantAsync(machine.tenantId, AppSettings.MagicplateSettings.TaxSettings.TaxName, machine.Settings.TaxSettings.Name);
                    await SettingManager.ChangeSettingForTenantAsync(machine.tenantId, AppSettings.MagicplateSettings.TaxSettings.TaxType, machine.Settings.TaxSettings.Type);
                    await SettingManager.ChangeSettingForTenantAsync(machine.tenantId, AppSettings.MagicplateSettings.TaxSettings.TaxPercentage, machine.Settings.TaxSettings.Percentage.ToString());
                    //Clear caching objects
                    await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();
                    await _cacheManager.GetCache(TaxSettingsCacheItem.CacheName).ClearAsync();
                    return true;
                }
            }
            return false;

        }
        private static bool _isSyncingFullData = false;
        public async Task<bool> FullySyncAllData()
        {
            if (_isSyncingFullData)
            {
                throw new UserFriendlyException("Fullly data synchronisation is running");
            }
            try
            {
                _isSyncingFullData = true;
                //FIRST: clear old meta data
                using (var uow = _unitOfWorkManager.Begin())
                {
                    var dbContext = _unitOfWorkManager.Current.GetDbContext<KonbiCloudDbContext>(MultiTenancySides.Tenant);
                    dbContext.Database.ExecuteSqlCommand(new RawSqlString("update platecategories set isdeleted =1"));
                    dbContext.Database.ExecuteSqlCommand(new RawSqlString("update categories set isdeleted =1"));
                    dbContext.Database.ExecuteSqlCommand(new RawSqlString("update discs set isdeleted =1"));
                    dbContext.Database.ExecuteSqlCommand(new RawSqlString("update pricestrategies set isdeleted =1"));
                    dbContext.Database.ExecuteSqlCommand(new RawSqlString("update plates set isdeleted =1"));
                    
                    dbContext.Database.ExecuteSqlCommand(new RawSqlString("update productmenus set isdeleted =1"));
                    dbContext.Database.ExecuteSqlCommand(new RawSqlString("update products set isdeleted =1"));
                    dbContext.Database.ExecuteSqlCommand(new RawSqlString("update sessions set isdeleted =1"));
                  
                    await uow.CompleteAsync();
                }
                //SECOND: reset last synced success timestamp


                await _settingManager.ChangeSettingForTenantAsync(1, AppSettingNames.ProductMenuEntityLastSynced, "0");
                await _settingManager.ChangeSettingForTenantAsync(1, AppSettingNames.PlateCategoryEntityLastSynced, "0");
                await _settingManager.ChangeSettingForTenantAsync(1, AppSettingNames.PlateEntityLastSynced, "0");
                await _settingManager.ChangeSettingForTenantAsync(1, AppSettingNames.CategoryEntityLastSynced, "0");
                await _settingManager.ChangeSettingForTenantAsync(1, AppSettingNames.ProductEntityLastSynced, "0");
                await _settingManager.ChangeSettingForTenantAsync(1, AppSettingNames.SessionEntityLastSynced, "0");
                await _settingManager.ChangeSettingForTenantAsync(1, AppSettingNames.InventoryEntityLastSynced, "0");
            }
            catch (Exception ex)
            {
               
                _logger.Error("FullySyncAllData", ex);
            }
            finally
            {
                _isSyncingFullData = false;
            }
           
            return await PartiallySyncAllData();
        }
        private static bool _isSyncingPartialData = false;
        public async Task<bool> PartiallySyncAllData()
        {
            _detailLogService.Log($"SyncFromCloud: START PartiallySyncAllData");
            if (_isSyncingPartialData)
            {
                throw new UserFriendlyException("Partially data synchronisation is running");
            }
            _isSyncingPartialData = true;
            bool isSyncedSuccess = true;
            try
            {
                bool result = await SyncPlateCategoryData();
            }
            catch (UserFriendlyException e)
            {
                _logger.Info(e.Message);
                isSyncedSuccess = false;
            }

            try
            {
                bool result = await SyncPlateData();
            }
            catch (UserFriendlyException e)
            {
                isSyncedSuccess = false;
                _logger.Info(e.Message);
            }

            try
            {
                bool result = await SyncProductCategoryData();
            }
            catch (UserFriendlyException e)
            {
                isSyncedSuccess = false;
                _logger.Info(e.Message);
            }
            try
            {
                bool result = await SyncProductData();
            }
            catch (UserFriendlyException e)
            {
                isSyncedSuccess = false;
                _logger.Info(e.Message);
            }
            try
            {
                bool result = await SyncDiscData();
            }
            catch (UserFriendlyException e)
            {
                isSyncedSuccess = false;
                _logger.Info(e.Message);
            }
            try
            {
                bool result = await SyncSessionData();
            }
            catch (UserFriendlyException e)
            {
                isSyncedSuccess = false;
                _logger.Info(e.Message);
            }

            try
            {
                bool result = await SyncProductMenuData();
            }
            catch (UserFriendlyException e)
            {
                isSyncedSuccess = false;
                _logger.Info(e.Message);
            }
            try
            {
                bool result = await SyncSettingsData();
            }
            catch (UserFriendlyException e)
            {
                isSyncedSuccess = false;
                _logger.Info(e.Message);
            }
            _isSyncingPartialData = false;

            _detailLogService.Log($"SyncFromCloud: END PartiallySyncAllData");
            return isSyncedSuccess;
        }
    }
}
