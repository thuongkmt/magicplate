using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.EntityFrameworkCore.Uow;
using Abp.Linq.Extensions;
using Abp.MultiTenancy;
using Abp.Runtime.Caching;
using Abp.UI;
using KonbiCloud.Authorization;
using KonbiCloud.CloudSync;
using KonbiCloud.Common;
using KonbiCloud.Configuration;
using KonbiCloud.EntityFrameworkCore;
using KonbiCloud.Features.Custom;
using KonbiCloud.Plate;
using KonbiCloud.ProductMenu.Dtos;
using KonbiCloud.Prices;
using KonbiCloud.Products;
using KonbiCloud.RFIDTable.Cache;
using KonbiCloud.Sessions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using System.Transactions;
using KonbiCloud.Products.Dtos;
using KonbiCloud.RFIDTable;
using KonbiCloud.Enums;
using KonbiCloud.RFIDTable.Dtos;

namespace KonbiCloud.ProductMenu
{
    [AbpAuthorize(AppPermissions.Pages_PlateMenus)]
    public class ProductMenusAppService : KonbiCloudAppServiceBase, IProductMenusAppService
    {
        private readonly IRepository<ProductMenu, Guid> _productMenuRepository;
        private readonly IRepository<Plate.Plate, Guid> _plateRepository;
        private readonly IRepository<Session, Guid> _sessionRepository;
        private readonly IRepository<Product, Guid> _productRepository;
        private readonly IRepository<PriceStrategyCode, int> _priceStrategyCodeRepository;
        private readonly IRepository<PriceStrategy, Guid> _priceStrategyRepository;
        private readonly IRfidTableFeatureChecker rfidTableFeatureChecker;
        private readonly ICacheManager _cacheManager;
        private readonly IPlateMenuSyncService _plateMenuSyncService;
        private readonly IConfigurationRoot _appConfiguration;
        private IIocResolver _iocResolver;
        private readonly IDetailLogService detailLogService;
        private readonly ISaveFromCloudService _saveFromCloudService;
        private readonly IRfidTableSignalRMessageCommunicator signalRCommunicator;
        private const string noImage = "assets/common/images";
        private string _serverUrl;
        public string ServerUrl
        {
            get
            {
                if (SettingManager != null && string.IsNullOrEmpty(_serverUrl))
                {
                    _serverUrl = SettingManager.GetSettingValue(AppSettingNames.SyncServerUrl);
                    if (_serverUrl == null)
                    {
                        _serverUrl = "";
                    }
                }
                return _serverUrl;
            }
        }

        public ProductMenusAppService(IRepository<ProductMenu, Guid> plateMenuRepository,
                                    IRepository<Plate.Plate, Guid> plateRepository,
                                    IRepository<Session, Guid> sessionRepository,
                                    IRepository<Product, Guid> productRepository,
                                    IRepository<PriceStrategyCode, int> priceStrategyCodeRepository,
                                    IRepository<PriceStrategy, Guid> priceStrategyRepository,
                                    IRfidTableFeatureChecker rfidTableFeatureChecker,
                                    ICacheManager cacheManager,
                                    IPlateMenuSyncService plateMenuSyncService,
                                    IHostingEnvironment env,
                                    IIocResolver iocResolver,
                                    IDetailLogService detailLog,
                                    ISaveFromCloudService saveFromCloudService,
                                    IRfidTableSignalRMessageCommunicator signalRCommunicator)
        {
            _productMenuRepository = plateMenuRepository;
            _plateRepository = plateRepository;
            _sessionRepository = sessionRepository;
            _productRepository = productRepository;
            _priceStrategyCodeRepository = priceStrategyCodeRepository;
            _priceStrategyRepository = priceStrategyRepository;
            this.rfidTableFeatureChecker = rfidTableFeatureChecker;
            _cacheManager = cacheManager;
            _plateMenuSyncService = plateMenuSyncService;
            _appConfiguration = env.GetAppConfiguration();
            _iocResolver = iocResolver;
            this.detailLogService = detailLog;
            _saveFromCloudService = saveFromCloudService;
            this.signalRCommunicator = signalRCommunicator;
        }


        [AbpAllowAnonymous]
        public async Task<PagedResultDto<ProductMenuDto>> GetAllProductMenus(PlateMenusInput input)
        {
            try
            {
                Guid.TryParse(input.CategoryFilter, out Guid catId);
                Guid.TryParse(input.SessionFilter, out Guid sessionId);
                var productMenus = _productMenuRepository.GetAll()
                    //.Include(pm => pm.Product)
                    //.ThenInclude(p => p.Category)
                    .WhereIf(!string.IsNullOrWhiteSpace(input.NameFilter), pm => pm.Product.Name.Contains(input.NameFilter.Trim(), StringComparison.OrdinalIgnoreCase))
                    .WhereIf(catId!=null&& catId!= Guid.Empty, pm => pm.Product.Category.Id == catId)
                    .WhereIf(!string.IsNullOrWhiteSpace(input.SKUFilter), pm => pm.Product.SKU.Contains(input.SKUFilter.Trim(), StringComparison.OrdinalIgnoreCase))
                    .WhereIf(input.DateFilter != null, e => e.SelectedDate == input.DateFilter.Value)
                    .WhereIf(!string.IsNullOrWhiteSpace(input.CodeFilter), e => e.Plate.Code.Contains(input.CodeFilter.Trim(), StringComparison.OrdinalIgnoreCase))
                    .WhereIf(sessionId!=null&& sessionId!=Guid.Empty, e => e.SessionId == sessionId);


                var totalCount = await productMenus.Select(el => el.Product.IsDeleted == false).CountAsync();
                var menuItems = await productMenus
                    .Select(m => new ProductMenuDto()
                    {
                        Id = m.Id.ToString(),
                        Plate = m.Plate,
                        PlateCode = m.Plate != null ? m.Plate.Code : string.Empty,
                        Product = m.Product != null ? new ProductDto
                        {
                            Id = m.Product.Id,
                            Name = m.Product.Name,
                            SKU = m.Product.SKU,
                            Barcode = m.Product.Barcode,
                            ImageUrl = m.Product.ImageUrl,
                            Price = m.Product.Price,
                            ContractorPrice = m.Product.ContractorPrice
                        } : null,
                        Price = m.Price,
                        CategoryName = m.Product.Category.Name,
                        PriceStrategy = m.ContractorPrice,
                        Session = m.Session,
                        ProductId = m.ProductId,
                        DisplayOrder = m.DisplayOrder
                    })
                    .OrderBy(input.Sorting ?? "Product.name")
                    .PageBy(input)

                    .ToListAsync();


                var pathImage = $"{ServerUrl.TrimEnd('/')}/{Const.ImageFolder}/{_appConfiguration[AppSettingNames.ProductImageFolder].TrimEnd('/')}";
                foreach (var d in menuItems)
                {
                    if (d.Product != null && !string.IsNullOrEmpty(d.Product.ImageUrl))
                    {

                        var arrImage = d.Product.ImageUrl.Split("|");
                        var images = "";
                        for (int index = 0; index < arrImage.Length; index++)
                        {
                            if (index == arrImage.Length - 1)
                            {
                                images += $"{pathImage}/{arrImage[index]}";
                            }
                            else
                            {
                                images += $"{pathImage}/{arrImage[index]}" + '|';
                            }
                        }
                        d.Product.ImageUrl = images;
                    }
                    else if (string.IsNullOrEmpty(d.Product.ImageUrl))
                    {
                        d.Product.ImageUrl = $"{ServerUrl}/{noImage}/ic_nophoto.jpg";
                    }


                    if (d.Plate != null)
                    {
                        if (string.IsNullOrEmpty(d.Plate.ImageUrl))
                        {
                            d.Plate.ImageUrl = $"{ServerUrl}/{noImage}/ic_nophoto.jpg";
                        }
                        else
                        {
                            var arrImage = d.Plate.ImageUrl.Split("|");
                            var images = "";
                            for (int index = 0; index < arrImage.Length; index++)
                            {
                                if (index == arrImage.Length - 1)
                                {
                                    images += $"{pathImage}/{arrImage[index]}";
                                }
                                else
                                {
                                    images += $"{pathImage}/{arrImage[index]}" + '|';
                                }
                            }
                            d.Plate.ImageUrl = images;
                        }

                    }
                }
                detailLogService.Log($"Get all plate menus: {menuItems.Count}");
                return new PagedResultDto<ProductMenuDto>(totalCount, menuItems);

            }
            catch (Exception ex)
            {
                Logger.Error($"Get plate menu: {ex.Message}", ex);
                throw new UserFriendlyException(ex.Message, ex);
            }
        }

        [AbpAllowAnonymous]
        public async Task<PagedResultDto<ProductMenuDto>> GetPOSProductMenuList(PlateMenusInput input)
        {
            try
            {
                var productMenus = _productMenuRepository.GetAll()
                    .WhereIf(!string.IsNullOrEmpty(input.NameFilter), pm => pm.Product.Name.Contains(input.NameFilter.Trim(), StringComparison.OrdinalIgnoreCase))
                    .WhereIf(!string.IsNullOrEmpty(input.CodeFilter), e => e.Plate.Code.Contains(input.CodeFilter.Trim(), StringComparison.OrdinalIgnoreCase))
                    .WhereIf(!string.IsNullOrEmpty(input.CategoryFilter) && Guid.Parse(input.CategoryFilter) != Guid.Empty, pm => pm.Product.Category.Id == Guid.Parse(input.CategoryFilter))
                    .WhereIf(!string.IsNullOrEmpty(input.SKUFilter), pm => pm.Product.SKU.Contains(input.SKUFilter.Trim(), StringComparison.OrdinalIgnoreCase))
                    .WhereIf(input.DateFilter != null, e => e.SelectedDate == input.DateFilter.Value.Date)                    
                    .WhereIf(!string.IsNullOrEmpty(input.SessionFilter) && !"0".Equals(input.SessionFilter), e => e.SessionId == Guid.Parse(input.SessionFilter));
                //.Where(x => x.Product.IsDeleted == false);

                var totalCount = await productMenus.CountAsync();
                var menuItems = await productMenus
                   .Select(m => new ProductMenuDto()
                   {
                       Id = m.Id.ToString(),
                       Plate = m.Plate,
                       PlateCode = m.Plate != null ? m.Plate.Code : string.Empty,
                       Product = m.Product != null ? new ProductDto
                       {
                           Id = m.Product.Id,
                           Name = m.Product.Name,
                           SKU = m.Product.SKU,
                           Barcode = m.Product.Barcode,
                           ImageUrl = m.Product.ImageUrl,
                           Price = m.Product.Price,
                           ContractorPrice = m.Product.ContractorPrice
                       } : null,
                       Price = m.Price,
                       CategoryName = m.Product.Category.Name,
                       PriceStrategy = m.ContractorPrice,
                       Session = m.Session,
                       ProductId = m.ProductId,
                       DisplayOrder = m.DisplayOrder
                   })
                   .OrderBy(input.Sorting ?? "displayOrder asc")
                   .PageBy(input)

                   .ToListAsync();

                var pathImage = $"{ServerUrl.TrimEnd('/')}/{Const.ImageFolder}/{_appConfiguration[AppSettingNames.ProductImageFolder].TrimEnd('/')}";
                foreach (var d in menuItems)
                {
                    if (d.Product != null && !string.IsNullOrEmpty(d.Product.ImageUrl))
                    {

                        var arrImage = d.Product.ImageUrl.Split("|");
                        var images = "";
                        for (int index = 0; index < arrImage.Length; index++)
                        {
                            if (index == arrImage.Length - 1)
                            {
                                images += $"{pathImage}/{arrImage[index]}";
                            }
                            else
                            {
                                images += $"{pathImage}/{arrImage[index]}" + '|';
                            }
                        }
                        d.Product.ImageUrl = images;
                    }
                    else if (string.IsNullOrEmpty(d.Product.ImageUrl))
                    {
                        d.Product.ImageUrl = $"{ServerUrl}/{noImage}/ic_nophoto.jpg";
                    }


                    if (d.Plate != null)
                    {
                        if (string.IsNullOrEmpty(d.Plate.ImageUrl))
                        {
                            d.Plate.ImageUrl = $"{ServerUrl}/{noImage}/ic_nophoto.jpg";
                        }
                        else
                        {
                            var arrImage = d.Plate.ImageUrl.Split("|");
                            var images = "";
                            for (int index = 0; index < arrImage.Length; index++)
                            {
                                if (index == arrImage.Length - 1)
                                {
                                    images += $"{pathImage}/{arrImage[index]}";
                                }
                                else
                                {
                                    images += $"{pathImage}/{arrImage[index]}" + '|';
                                }
                            }
                            d.Plate.ImageUrl = images;
                        }

                    }
                }
                detailLogService.Log($"Get all plate menus: {menuItems.Count}");
                return new PagedResultDto<ProductMenuDto>(totalCount, menuItems);
            }
            catch (Exception ex)
            {
                Logger.Error($"Get plate menu: {ex.Message}", ex);
                return new PagedResultDto<ProductMenuDto>(0, new List<ProductMenuDto>());
            }
        }

        [AbpAllowAnonymous]
        public async Task<ListResultDto<PosProductMenuOutput>> GetPOSMenu(PosProductMenuInput input)
        {
            try
            {
                var taxSettings = await GetTaxSettingsInternalAsync();              
                var result = _productMenuRepository.GetAll()
                    .Where(m => m.Product.Category.Id == input.CategoryId)
                    .Where(m => m.SessionId == input.SessionId)
                    .Where(m => m.SelectedDate == DateTime.Now.Date)                    
                    .OrderBy("DisplayOrder asc")
                    .Select(m => new PosProductMenuOutput()
                    {
                        ProductId = m.ProductId,
                        ProductName = m.Product.Name,
                        PlateCode = m.Plate == null ? "" : m.Plate.Code,
                   
                        Price = m.Price <= 0?0 : (taxSettings.TaxSettings.Type == TaxType.Inclusive ? m.Price : m.Price * (100 + taxSettings.TaxSettings.Percentage) / 100),
                    })                    
                    .ToList();

                return new ListResultDto<PosProductMenuOutput>(result);
            }
            catch (Exception ex)
            {
                return new ListResultDto<PosProductMenuOutput>(new List<PosProductMenuOutput>());
            }
        }

        [AbpAuthorize(AppPermissions.Pages_PlateMenus_Generate)]
        public async Task<bool> GenerateProductMenu(ReplicateInput input)
        
         {
            try
            {
                input.DateFilter = input.DateFilter.ToLocalTime();
                var sessions = await _sessionRepository.GetAllListAsync();
                var plates = await _plateRepository.GetAllListAsync();
                var products = await _productRepository.GetAllListAsync();
                var pmDate = await _productMenuRepository
                            //.GetAllIncluding(x => x.PriceStrategies)
                            .GetAll().Where(x => x.SelectedDate.Date == input.DateFilter.Date).ToListAsync();
                pmDate.ForEach(el => { el.IsDeleted = true; });

                foreach (var session in sessions)
                {
                    //Get platemenu by session and date
                    var productMenusForSessionAndDate = pmDate.Where(x => x.SessionId == session.Id);

                    foreach (var product in products)
                    {
                        //Check if menu schedule exist
                        var productMenuItem = productMenusForSessionAndDate.FirstOrDefault(x => x.ProductId == product.Id);
                        if (productMenuItem != null)
                        {
                            //productMenuItem.Price = (decimal)product.Price;                           
                            //productMenuItem.DisplayOrder = product.DisplayOrder;
                            productMenuItem.IsDeleted = false;
                            continue;
                        }

                        //Generate data
                        var newPm = new ProductMenu
                        {
                            TenantId = AbpSession.TenantId,
                            ProductId = product.Id,
                            Price = (decimal)product.Price,
                            ContractorPrice = 0,
                            SessionId = session.Id,
                            SelectedDate = input.DateFilter,
                            DisplayOrder = product.DisplayOrder
                        };

                        await _productMenuRepository.InsertAsync(newPm);
                    }
                }
                await CurrentUnitOfWork.SaveChangesAsync();
                detailLogService.Log($"Generate plate menus done");
                await signalRCommunicator.NotifyProductChanges("Product");
                await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Generate Plate Menu{ex.Message}", ex);
                throw new UserFriendlyException("Generate Plate Menus failed");
            }
        }

        [AbpAuthorize(AppPermissions.Pages_PlateMenus_Edit)]
        public async Task<bool> UpdatePrice(PlateMenusInput input)
        {
            try
            {
                if (Guid.TryParse(input.Id, out Guid id))
                {
                    var plate = await _productMenuRepository.FirstOrDefaultAsync(id);
                    plate.Price = input.Price;
                    await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return false;
            }
        }
        [AbpAuthorize(AppPermissions.Pages_PlateMenus_Edit)]
        public async Task<bool> UpdatePlate(AssignPlateModelInput input)
        {
            try
            {

                var productMenu = await _productMenuRepository.FirstOrDefaultAsync(input.Id);

                if (productMenu != null)
                {
                    var plate = await _plateRepository.FirstOrDefaultAsync(input.PlateId);
                    if (plate != null)
                    {
                        var p = await _productMenuRepository.GetAllIncluding(el => el.Product).FirstOrDefaultAsync(el => el.Id != input.Id && el.SessionId == productMenu.SessionId && el.SelectedDate == productMenu.SelectedDate && el.PlateId == input.PlateId);

                        if (p != null)
                        {
                            throw new UserFriendlyException($"Plate {plate.Code} is already assigned to product \"{p.Product.Name}\"");
                        }

                        productMenu.PlateId = plate.Id;

                    }
                    else
                    {
                        productMenu.PlateId = null;
                    }
                    await _productMenuRepository.UpdateAsync(productMenu);
                    await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();
                    
                    return true;
                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                throw ex;
            }
            return false;
        }
        [AbpAuthorize(AppPermissions.Pages_PlateMenus_Edit)]
        public async Task<bool> UpdateDisplayOrder(PlateMenusInput input)
        {
            try
            {
                if (Guid.TryParse(input.Id, out Guid id))
                {
                    var plate = await _productMenuRepository.FirstOrDefaultAsync(id);
                    plate.DisplayOrder = input.DisplayOrder;
                    await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();
                    await _productMenuRepository.UpdateAsync(plate);
                    signalRCommunicator.NotifyProductChanges("Product");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return false;
            }
        }

        [AbpAuthorize(AppPermissions.Pages_PlateMenus_Edit)]
        public async Task<bool> UpdateProduct(PlateMenusInput input)
        {
            try
            {
                if (Guid.TryParse(input.Id, out Guid id))
                {
                    var plateMenu = await _productMenuRepository.FirstOrDefaultAsync(id);
                    plateMenu.ProductId = input.ProductId;
                    await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();
                    await _productMenuRepository.UpdateAsync(plateMenu);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return false;
            }
        }

        [AbpAuthorize(AppPermissions.Pages_PlateMenus_Edit)]
        public async Task<bool> UpdatePriceStrategy(PlateMenusInput input)
        {
            try
            {
                if (Guid.TryParse(input.Id, out Guid id))
                {
                    var productMenu = await _productMenuRepository.FirstOrDefaultAsync(id);
                    if (productMenu != null)
                    {
                        productMenu.ContractorPrice = input.PriceStrategy;
                        await _productMenuRepository.UpdateAsync(productMenu);
                        await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();
                    }
                  
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return false;
            }
        }

        public async Task<ImportResult> ImportProductMenu(List<ImportData> input)
        {
            try
            {
                var listError = new List<string>();
                var updatingList = new List<ProductMenu>();
                var products = await _productRepository.GetAllListAsync();
                var sessions = await _sessionRepository.GetAllListAsync();
                var plates = await _plateRepository.GetAllListAsync();


                for (int i = 0; i < input.Count; i++)
                {
                    var dto = input[i];

                    if (string.IsNullOrEmpty(dto.SessionName) || string.IsNullOrEmpty(dto.SelectedDate))
                    {
                        listError.Add($"&#8226; Row {i + 2}- Date, Session and Product Name are mandatory");
                        continue;
                    }

                    //Check session exist
                    var session = sessions.FirstOrDefault(x => x.Name.ToLower().Equals(dto.SessionName.Trim().ToLower()));
                    if (session == null)
                    {
                        listError.Add($"&#8226; Row {i + 2}- Cannot find Session with Name: {dto.SessionName}");
                        continue;
                    }

                    var date = DateTime.Now;
                    try
                    {
                        date = DateTime.Parse(dto.SelectedDate, new CultureInfo("en-SG"));
                    }
                    catch
                    {
                        listError.Add($"&#8226; Row {i + 2}- Wrong datetime");
                        continue;
                    }
                    if (string.IsNullOrEmpty(dto.ProductName) && string.IsNullOrEmpty(dto.SKU))
                    {
                        listError.Add($"&#8226; Row {i + 2}- Product Name or Product SKU is required");
                        continue;
                    }



                    //Check product existing
                    Product product = null;
                    if (!string.IsNullOrEmpty(dto.SKU))
                        product = products.FirstOrDefault(x => x.SKU == dto.SKU.Trim());
                    if (product == null)
                    {
                        product = products.FirstOrDefault(x => x.Name.ToLower() == dto.ProductName.Trim().ToLower());

                    }
                    if (product == null)
                    {
                        listError.Add($"&#8226; Row {i + 2}- Cannot find Product with Name: {dto.ProductName}");
                        continue;
                    }

                    // Check Plate Model existing
                    Plate.Plate plate = plates.FirstOrDefault(x => x.Code == dto.PlateCode.Trim());


                    //Check duplicate product

                    if (updatingList.Any(x => x.ProductId == product.Id))
                    {
                        listError.Add($"&#8226; Row {i + 2}- Duplicate Product \"{product.Name}\"");
                        continue;
                    }

                    //Check duplicate plate model

                    if (plate != null && updatingList.Any(x => x.PlateId == plate.Id))
                    {
                        listError.Add($"&#8226; Row {i + 2}- Duplicate Plate \"{plate.Code}\"");
                        continue;
                    }



                    //Check exist
                    var productMenu = _productMenuRepository.GetAll()
                                                        .Where(x => x.Product.Id == product.Id && x.SessionId == session.Id && x.SelectedDate.Date == date.Date)
                                                        .FirstOrDefault();



                    decimal price = 0;
                    decimal.TryParse(dto.Price, out price);
                    decimal contractorPrice = 0;
                    decimal.TryParse(dto.ContractorPrice, out contractorPrice);

                    if (productMenu == null)
                    {
                        var newProductMenu = new ProductMenu
                        {
                            TenantId = AbpSession.TenantId,
                            ProductId = product.Id,
                            SessionId = session.Id,
                            SelectedDate = date,
                            Price = price,
                            ContractorPrice = contractorPrice
                        };
                        if (plate != null)
                            newProductMenu.PlateId = plate.Id;
                        await _productMenuRepository.InsertAsync(newProductMenu);
                        updatingList.Add(newProductMenu);
                    }
                    else
                    {
                        productMenu.Price = price;
                        productMenu.ContractorPrice = contractorPrice;
                        if (plate != null)
                            productMenu.PlateId = plate.Id;
                        updatingList.Add(productMenu);
                    }

                }
                if (updatingList.Any())
                {
                    await CurrentUnitOfWork.SaveChangesAsync();
                }
                var result = new ImportResult
                {
                    ErrorCount = listError.Count,
                    SuccessCount = updatingList.Count
                };
                if (listError.Count > 100)
                {
                    result.ErrorList = string.Join("<br/>", listError.Take(100).Select(x => x.ToString()).ToArray()) + "...";
                }
                else
                {
                    result.ErrorList = string.Join("<br/>", listError.Take(100).Select(x => x.ToString()).ToArray());
                }

                await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error($"Import Plate Menu Error: {ex.Message}", ex);
                return new ImportResult
                {
                    ErrorCount = 0,
                    SuccessCount = 0,
                    ErrorList = "Error"
                };
            }
        }

        [AbpAllowAnonymous]
        public async Task<bool> SyncProductMenuData()
        {
            //// Sync Plate Category.
            //bool result = await _saveFromCloudService.SyncPlateCategoryData();
            //if (!result)
            //{
            //    return false;
            //}

            // Sync Plate.
            var result = await _saveFromCloudService.SyncPlateData();
            if (!result)
            {
                return false;
            }

            //Sync Product Category.
            //result = await _saveFromCloudService.SyncProductCategoryData();
            //if (!result)
            //{
            //    return false;
            //}

            // Sync Product.
            result = await _saveFromCloudService.SyncProductData();
            if (!result)
            {
                return false;
            }

            // Sync Inventory(Disc).
            //result = await _saveFromCloudService.SyncDiscData();
            //if (!result)
            //{
            //    return false;
            //}

            // Sync Session.
            result = await _saveFromCloudService.SyncSessionData();
            if (!result)
            {
                return false;
            }

            // Sync Product Menu.
            result = await _saveFromCloudService.SyncProductMenuData();
            if (!result)
            {
                return false;
            }

            return result;
        }

        [AbpAuthorize(AppPermissions.Pages_PlateMenus_Replicate)]
        public async Task ReplicateProductMenu(ReplicateInput input)
        {
            try
            {
                var pmToClone = await _productMenuRepository.GetAll()
                                .Where(x => x.SelectedDate == input.DateFilter.Date)
                                .ToListAsync();
                if (!pmToClone.Any()) return;

                var futurePm = await _productMenuRepository.GetAll()
                                .Where(x => x.SelectedDate >= input.DateFilter.Date.AddDays(1)
                                            && x.SelectedDate <= input.DateFilter.Date.AddDays(input.Days))
                                .ToListAsync();

                //var addList = new List<MenuSchedule.ProductMenu>();
                //var updateList = new List<MenuSchedule.ProductMenu>();

                for (int i = 1; i <= input.Days; i++)
                {
                    var cloneDate = input.DateFilter.AddDays(i);
                    var futurePmAtCloneDate = futurePm.Where(x => x.SelectedDate == cloneDate.Date);
                    var hasData = futurePmAtCloneDate.Any();
                    foreach (var pm in pmToClone)
                    {
                        if (!hasData)
                        {
                            var newPm = AddNewPlateMenu(pm, cloneDate);
                            //addList.Add(newPm);
                            await _productMenuRepository.InsertAsync(newPm);
                            continue;
                        }

                        var futurePmAtCloneDateAndSession = futurePmAtCloneDate.Where(x => x.SessionId == pm.SessionId);
                        if (!futurePmAtCloneDateAndSession.Any())
                        {
                            var newPm = AddNewPlateMenu(pm, cloneDate);
                            //addList.Add(newPm);
                            await _productMenuRepository.InsertAsync(newPm);
                            continue;
                        }

                        var existPm = futurePmAtCloneDateAndSession.FirstOrDefault(x => x.ProductId == pm.ProductId);
                        if (existPm == null)
                        {
                            var newPm = AddNewPlateMenu(pm, cloneDate);
                            //addList.Add(newPm);
                            await _productMenuRepository.InsertAsync(newPm);
                        }
                        else
                        {
                            existPm.Price = pm.Price;
                            existPm.ContractorPrice = pm.ContractorPrice;
                            existPm.TenantId = pm.TenantId;
                            existPm.ProductId = pm.ProductId;
                            existPm.SessionId = pm.SessionId;
                            existPm.PlateId = pm.PlateId;
                            existPm.DisplayOrder = pm.DisplayOrder;

                            //updateList.Add(existPm);
                        }

                    }
                }
                // Clear cache.
                await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();

               
            }
            catch (Exception ex)
            {
                Logger.Error($"Replicate PM: {ex.Message}", ex);
            }
        }

        private ProductMenu AddNewPlateMenu(ProductMenu pm, DateTime cloneDate)
        {
            var newPm = new ProductMenu
            {
                TenantId = pm.TenantId,
                ProductId = pm.ProductId,
                Price = pm.Price,
                DisplayOrder = pm.DisplayOrder,
                SessionId = pm.SessionId,
                ContractorPrice = pm.ContractorPrice,
                SelectedDate = cloneDate,
                PlateId = pm.PlateId
            };

            return newPm;
        }

        public async Task<List<PlateMenuDayResult>> GetDayPlateMenusDetail(DateTime inputDate)
        {
            try
            {
                var plateMenus = await _productMenuRepository.GetAllIncluding(e => e.Product, e => e.Session)
                    .Where(e => e.SelectedDate == inputDate.Date)
                    //.Where(e => e.Product.IsDeleted == false)
                    .GroupBy(e => e.Session)
                    .ToListAsync();

                plateMenus = plateMenus.FindAll(x => x.Key != null).OrderBy(x => x.Key.FromHrs).ToList();

                var listResult = new List<PlateMenuDayResult>();

                for (int i = 0; i < plateMenus.Count; i++)
                {
                    var sessionMenu = plateMenus[i].ToList();
                    var plateMenu = new PlateMenuDayResult
                    {
                        SessionId = plateMenus[i].Key.Id.ToString(),
                        SessionName = plateMenus[i].Key.Name,
                        TotalSetPrice = sessionMenu.FindAll(x => x.Price != 0).Count,
                        TotalNoPrice = sessionMenu.FindAll(x => x.Price == 0).Count,
                        ActiveFlg = plateMenus[i].Key.ActiveFlg
                    };
                    listResult.Add(plateMenu);
                }
                return listResult;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return new List<PlateMenuDayResult>();
            }
        }

        public async Task<List<string>> GetWeekPlateMenus()
        {
            var currentDay = DateTime.Now;
            var endNext7Day = currentDay.AddDays(7);
            try
            {
                var result = new List<string>();

                var plateMenus = await _productMenuRepository.GetAllIncluding(e => e.Product)
                    .Where(e => e.SelectedDate >= currentDay.Date && e.SelectedDate <= endNext7Day.Date)
                    //.Where(e => e.Product.IsDeleted == false)
                    .GroupBy(e => e.SelectedDate)
                    .ToListAsync();

                for (int i = 0; i < 7; i++)
                {
                    if (!plateMenus.Any(x => x.Key.Date == currentDay.Date))
                    {
                        result.Add(currentDay.Date.ToString("dd/MM/yyyy"));
                    }
                    currentDay = currentDay.AddDays(1);
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return new List<string>();
            }
        }

        public async Task<List<ProductMenuMonthResult>> GetMonthPlateMenus(DateTime inputDate)
        {
            var firstDayOfMonth = new DateTime(inputDate.Year, inputDate.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            try
            {
                var plateMenus = await _productMenuRepository.GetAllIncluding(e => e.Product)
                    .Where(e => e.SelectedDate >= firstDayOfMonth.Date && e.SelectedDate <= lastDayOfMonth.Date)
                    //.Where(e => e.Product.IsDeleted == false)
                    .GroupBy(e => e.SelectedDate)
                    .ToListAsync();

                var listResult = new List<ProductMenuMonthResult>();

                for (int i = 0; i < plateMenus.Count; i++)
                {
                    var dailyMenu = plateMenus[i].ToList();
                    var plateMenu = new ProductMenuMonthResult
                    {
                        Day = plateMenus[i].Key.ToString("MM-dd-yyy"),
                        TotalSetPrice = dailyMenu.FindAll(x => x.Price != 0).Count,
                        TotalNoPrice = dailyMenu.FindAll(x => x.Price == 0).Count
                    };
                    listResult.Add(plateMenu);
                }
                return listResult;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return new List<ProductMenuMonthResult>();
            }
      
        }
        private async Task<TaxSettingsCacheItem> GetTaxSettingsInternalAsync()
        {
            try
            {
                var percentStr = await SettingManager.GetSettingValueAsync(AppSettings.MagicplateSettings.TaxSettings.TaxPercentage);
                decimal.TryParse(percentStr, out decimal percent);
                var type = await SettingManager.GetSettingValueAsync(AppSettings.MagicplateSettings.TaxSettings.TaxType);
                if (!TaxType.TryParse(type, true, out TaxType taxType))
                    taxType = TaxType.Exclusive;

                var output = new TaxSettingsDto()
                {
                    Name = await SettingManager.GetSettingValueAsync(AppSettings.MagicplateSettings.TaxSettings.TaxName),
                    Type = taxType,
                    Percentage = percent,

                };
                return new TaxSettingsCacheItem() { TaxSettings = output };


            }
            catch (Exception ex)
            {
                detailLogService.Log(ex.Message);
            }
            return null;
        }
    }
}