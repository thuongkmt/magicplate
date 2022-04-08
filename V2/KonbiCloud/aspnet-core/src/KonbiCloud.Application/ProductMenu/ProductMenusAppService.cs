using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.EntityFrameworkCore;
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
using KonbiCloud.Machines;
using KonbiCloud.MenuSchedule;
using KonbiCloud.Plate;
using KonbiCloud.ProductMenu.Dtos;
using KonbiCloud.PlateMenus;
using KonbiCloud.PlateMenus.Dtos;
using KonbiCloud.Prices;
using KonbiCloud.Products;
using KonbiCloud.RFIDTable.Cache;
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
using KonbiCloud.Messaging;
using Konbini.Messages;
using Konbini.Messages.Enums;
using KonbiCloud.Products.Dtos;
using KonbiCloud.Dto;
using ServiceStack;
using Abp.Timing;
using Newtonsoft.Json;

namespace KonbiCloud.ProductMenu
{
    [AbpAuthorize(AppPermissions.Pages_PlateMenus)]
    public class ProductMenusAppService : KonbiCloudAppServiceBase, IProductMenusAppService
    {
        private readonly IRepository<MenuSchedule.ProductMenu, Guid> _productMenuRepository;
        private readonly IRepository<Plate.Plate, Guid> _plateRepository;
        private readonly IRepository<Session, Guid> _sessionRepository;
        private readonly IRepository<Product, Guid> _productRepository;
        private readonly IRepository<PriceStrategyCode, int> _priceStrategyCodeRepository;
        private readonly IRepository<PriceStrategy, Guid> _priceStrategyRepository;
        private readonly IRfidTableFeatureChecker rfidTableFeatureChecker;
        private readonly ICacheManager _cacheManager;
        private const string noImage = "assets/common/images";
        private readonly string serverUrl;
        private readonly IConfigurationRoot _appConfiguration;
        private readonly IRepository<Machine, Guid> _machineRepository;

        private IIocResolver _iocResolver;
        private readonly IDetailLogService _detailLogService;
        private readonly ISendMessageToMachineService _sendMessageToMachineService;
        private readonly IRepository<Category, Guid> _categoryRepository;

        public ProductMenusAppService(IRepository<MenuSchedule.ProductMenu, Guid> plateMenuRepository,
                                    IRepository<Plate.Plate, Guid> plateRepository,
                                    IRepository<Session, Guid> sessionRepository,
                                    IRepository<Product, Guid> productRepository,
                                    IRepository<PriceStrategyCode, int> priceStrategyCodeRepository,
                                    IRepository<PriceStrategy, Guid> priceStrategyRepository,
                                    IRfidTableFeatureChecker rfidTableFeatureChecker,
                                    ICacheManager cacheManager,
                                    IHostingEnvironment env,
                                    IRepository<Machine, Guid> machineRepository,
                                    IIocResolver iocResolver,
                                    IDetailLogService detailLog, ISendMessageToMachineService sendMessageToMachineService,
                                    IRepository<Category, Guid> categoryRepository)
        {
            _productMenuRepository = plateMenuRepository;
            _plateRepository = plateRepository;
            _sessionRepository = sessionRepository;
            _productRepository = productRepository;
            _priceStrategyCodeRepository = priceStrategyCodeRepository;
            _priceStrategyRepository = priceStrategyRepository;
            this.rfidTableFeatureChecker = rfidTableFeatureChecker;
            _cacheManager = cacheManager;
            _appConfiguration = env.GetAppConfiguration();
            serverUrl = _appConfiguration["App:ServerRootAddress"];
            if (serverUrl == null)
            {
                serverUrl = "";
            }
            _machineRepository = machineRepository;
            _iocResolver = iocResolver;
            _detailLogService = detailLog;
            _sendMessageToMachineService = sendMessageToMachineService;
            _categoryRepository = categoryRepository;
        }
        [AbpAllowAnonymous]
        
        public async Task<PagedResultDto<ProductMenuDto>> GetAllProductMenus(PlateMenusInput input)
        {
            try
            {

                var productMenus = _productMenuRepository.GetAll()
                    //.Include(pm => pm.Product)
                    //    .ThenInclude(p => p.Category)
                    .WhereIf(!string.IsNullOrWhiteSpace(input.NameFilter), pm => pm.Product.Name.Contains(input.NameFilter.Trim(), StringComparison.OrdinalIgnoreCase))
                    .WhereIf(!string.IsNullOrEmpty(input.CategoryFilter) && Guid.Parse(input.CategoryFilter) != Guid.Empty, pm => pm.Product.Category.Id == Guid.Parse(input.CategoryFilter))
                    .WhereIf(!string.IsNullOrWhiteSpace(input.SKUFilter), pm => pm.Product.SKU.Contains(input.SKUFilter.Trim(), StringComparison.OrdinalIgnoreCase))
                    .WhereIf(input.DateFilter != null, e => e.SelectedDate == input.DateFilter.Value)
                    .WhereIf(!string.IsNullOrWhiteSpace(input.CodeFilter), e => e.Plate.Code.Contains(input.CodeFilter.Trim(), StringComparison.OrdinalIgnoreCase))
                    .WhereIf(!string.IsNullOrWhiteSpace(input.SessionFilter) && !"0".Equals(input.SessionFilter), e => e.SessionId == Guid.Parse(input.SessionFilter));
                    

                var totalCount = await productMenus.Select(el=>el.Product.IsDeleted==false).CountAsync();
                var menuItems = await productMenus
                    .Select(m => new ProductMenuDto()
                    {
                        Id = m.Id.ToString(),
                        Plate = m.Plate,
                        PlateCode = m.Plate!=null? m.Plate.Code: string.Empty,
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

             
                var pathImage = $"{serverUrl.TrimEnd('/')}/{Const.ImageFolder}/{_appConfiguration[AppSettingNames.ProductImageFolder].TrimEnd('/')}";
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
                    }else if (string.IsNullOrEmpty(d.Product.ImageUrl))
                    {
                        d.Product.ImageUrl = $"{serverUrl}/{noImage}/ic_nophoto.jpg";
                    }

               
                    if (d.Plate != null)
                    {
                        if (string.IsNullOrEmpty(d.Plate.ImageUrl))
                        {
                            d.Plate.ImageUrl = $"{serverUrl}/{noImage}/ic_nophoto.jpg";
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
                _detailLogService.Log($"Get all plate menus: {menuItems.Count}");
                return new PagedResultDto<ProductMenuDto>(totalCount, menuItems);

            }
            catch (Exception ex)
            {
                Logger.Error($"Get plate menu: {ex.Message}", ex);
                throw new UserFriendlyException(ex.Message, ex);
            }
        }

       

        [AbpAuthorize(AppPermissions.Pages_PlateMenus_Generate)]
        
        public async Task<bool> GenerateProductMenu(ReplicateInput input)
        {
            try
            {
                _detailLogService.Log($"ProductMenu: START generate");
                input.DateFilter = input.DateFilter.ToLocalTime();
                var sessions = await _sessionRepository.GetAllListAsync(x => x.ActiveFlg == true);
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
                        var newPm = new MenuSchedule.ProductMenu
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
                bool checkSendToRabbitMQ = await _sendMessageToMachineService.SendNotificationAsync(new KeyValueMessage() { Key = MessageKeys.MenuScheduler });
                _detailLogService.Log($"ProductMenu: END generate, then send changes to rabbitmq is -> " + checkSendToRabbitMQ);
                return true;
            }
            catch (Exception ex)
            {
                _detailLogService.Log("ProductMenu: END generate with error -> " + ex.ToString());
                throw new UserFriendlyException("Generate Plate Menus failed");
            }
        }

        [AbpAuthorize(AppPermissions.Pages_PlateMenus_Edit)]
        public async Task<bool> UpdatePrice(PlateMenusInput input)
        {
            try
            {
                _detailLogService.Log($"ProductMenu: START updatePrice");
                if (Guid.TryParse(input.Id, out Guid id))
                {
                    var productMenu = await _productMenuRepository.FirstOrDefaultAsync(id);
                    if (productMenu != null)
                    {
                        productMenu.Price = input.Price;
                        await _productMenuRepository.UpdateAsync(productMenu);
                        await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();
                    }
                    bool checkSendToRabbitMQ = await _sendMessageToMachineService.SendNotificationAsync(new KeyValueMessage() { Key = MessageKeys.MenuScheduler });
                    _detailLogService.Log($"ProductMenu: END updatePrice, then send changes to rabbitmq is -> " + checkSendToRabbitMQ);
                    return true;
                }
                _detailLogService.Log($"ProductMenu: END updatePrice failed");
                return false;
            }
            catch (Exception ex)
            {
                _detailLogService.Log("ProductMenu: END updatePrice with error -> " + ex.ToString());
                return false;
            }
        }
        [AbpAuthorize(AppPermissions.Pages_PlateMenus_Edit)]
        public async Task<bool> UpdatePlate(AssignPlateModelInput input)
        {
            try
            {
                _detailLogService.Log($"ProductMenu: START updatePlate");
                var productMenu = await _productMenuRepository.FirstOrDefaultAsync(input.Id);
                
                if (productMenu != null)
                {
                    var plate = await _plateRepository.FirstOrDefaultAsync(input.PlateId);
                    if (plate != null)
                    {
                        var p = await _productMenuRepository.GetAllIncluding(el=> el.Product).FirstOrDefaultAsync(el => el.Id != input.Id && el.SessionId == productMenu.SessionId && el.SelectedDate == productMenu.SelectedDate && el.PlateId == input.PlateId);

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
                    bool checkSendToRabbitMQ =  await _sendMessageToMachineService.SendNotificationAsync(new KeyValueMessage() { Key = MessageKeys.MenuScheduler });
                    _detailLogService.Log($"ProductMenu: END updatePlate, then send changes to rabbitmq is -> " + checkSendToRabbitMQ);
                    return true;
                }

            }
            catch (Exception ex)
            {
                _detailLogService.Log("ProductMenu: END updatePlate with error -> " + ex.ToString());
                throw ex;
            }
            return false;
        }

        [AbpAuthorize(AppPermissions.Pages_PlateMenus_Edit)]
        public async Task<bool> UpdateDisplayOrder(PlateMenusInput input)
        {
            try
            {
                _detailLogService.Log($"ProductMenu: START updateDisplayOrder");
                if (Guid.TryParse(input.Id, out Guid id))
                {
                    var productMenu = await _productMenuRepository.FirstOrDefaultAsync(id);
                    if (productMenu != null)
                    {
                        productMenu.DisplayOrder = input.DisplayOrder;
                        await _productMenuRepository.UpdateAsync(productMenu);
                        await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();
                    }
                    bool checkSendToRabbitMQ = await _sendMessageToMachineService.SendNotificationAsync(new KeyValueMessage() { Key = MessageKeys.MenuScheduler });
                    _detailLogService.Log($"ProductMenu: END updateDisplayOrder, then send changes to rabbitmq is -> " + checkSendToRabbitMQ);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _detailLogService.Log("ProductMenu: END updateDisplayOrder with error -> " + ex.ToString());
                return false;
            }
        }

        [AbpAuthorize(AppPermissions.Pages_PlateMenus_Edit)]
        public async Task<bool> UpdateProduct(PlateMenusInput input)
        {
            try
            {
                _detailLogService.Log($"ProductMenu: START updateProduct");
                if (Guid.TryParse(input.Id, out Guid id))
                {
                    var productMenu = await _productMenuRepository.FirstOrDefaultAsync(id);
                    if (productMenu != null)
                    {
                        productMenu.ProductId = input.ProductId;
                        await _productMenuRepository.UpdateAsync(productMenu);
                        await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();
                    }
                    bool checkSendToRabbitMQ = await _sendMessageToMachineService.SendNotificationAsync(new KeyValueMessage() { Key = MessageKeys.MenuScheduler });
                    _detailLogService.Log($"ProductMenu: END updateProduct, then send changes to rabbitmq is -> " + checkSendToRabbitMQ);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _detailLogService.Log("ProductMenu: END updateProduct with error -> " + ex.ToString());
                return false;
            }
        }

        [AbpAuthorize(AppPermissions.Pages_PlateMenus_Edit)]
        public async Task<bool> UpdatePriceStrategy(PlateMenusInput input)
        {
            try
            {
                _detailLogService.Log($"ProductMenu: START updatePriceStrategy");
                if (Guid.TryParse(input.Id, out Guid id))
                {
                    var productMenu = await _productMenuRepository.FirstOrDefaultAsync(id);
                    if (productMenu != null)
                    {
                        productMenu.ContractorPrice = input.PriceStrategy;
                        await _productMenuRepository.UpdateAsync(productMenu);
                        await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();
                    }
                    bool checkSendToRabbitMQ = await _sendMessageToMachineService.SendNotificationAsync(new KeyValueMessage() { Key = MessageKeys.MenuScheduler });
                    _detailLogService.Log($"ProductMenu: END updatePriceStrategy, then send changes to rabbitmq is -> " + checkSendToRabbitMQ);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _detailLogService.Log("ProductMenu: END updatePriceStrategy with error -> " + ex.ToString());
                return false;
            }

        }

        [AbpAuthorize(AppPermissions.Pages_PlateMenus_Import)]
        public async Task<ImportResult> ImportPlateMenu(List<ImportData> input)
        {
            try
            {
                var listError = new List<string>();
                var updatingList = new List<MenuSchedule.ProductMenu>();
                var products = await _productRepository.GetAllListAsync();
                var sessions = await _sessionRepository.GetAllListAsync();
                var plates = await _plateRepository.GetAllListAsync(); 
            
                
                for (int i = 0; i < input.Count; i++)
                {
                    var dto = input[i];

                    if ( string.IsNullOrEmpty(dto.SessionName) || string.IsNullOrEmpty(dto.SelectedDate))
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
                    if(!string.IsNullOrEmpty(dto.SKU))
                        product = products.FirstOrDefault(x => x.SKU ==dto.SKU.Trim());
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

                    if (plate!=null &&updatingList.Any(x => x.PlateId == plate.Id))
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
                        var newProductMenu = new MenuSchedule.ProductMenu
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

                await _sendMessageToMachineService.SendNotificationAsync(new KeyValueMessage() { Key = MessageKeys.MenuScheduler });
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error("Import Plate Menu Error", ex);
                return new ImportResult
                {
                    ErrorCount = 0,
                    SuccessCount = 0,
                    ErrorList = "Error"
                };
            }
        }
        /// <summary>
        /// Sync data between Server and Slave, To be Called from Slave side to this API
        /// </summary>
        /// <param name="machineSyncInput"></param>
        /// <returns></returns>
        [AbpAllowAnonymous]
        public async Task<EntitySyncOutputDto<ProductMenuSyncDto>> GetProductMenus(EntitySyncInputDto<Guid> machineSyncInput)
        {
            try
            {
                _detailLogService.Log($"ProductMenu: START get productMenu from server to sync to slave, request -> " + JsonConvert.SerializeObject(machineSyncInput));
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MustHaveTenant, AbpDataFilters.MayHaveTenant, AbpDataFilters.SoftDelete))
                {
                    var machine = await _machineRepository.FirstOrDefaultAsync(x => x.Id == machineSyncInput.Id);
                    if (machine == null)
                    {
                        _detailLogService.Log($"ProductMenu: MachineId is {machineSyncInput.Id} does not exist");
                        return null;
                    }
                    else if (machine.IsDeleted)
                    {
                        _detailLogService.Log($"ProductMenu: MachineId is {machineSyncInput.Id} was deleted");
                        return null;
                    }
                    ;
                    _detailLogService.Log($"ProductMenu: DateTime.Today.AddDays(-1) -> {DateTime.Today.AddDays(-1)}");
                    var unsyncedEntities = await _productMenuRepository
                                                            .GetAll()
                                                            .Where(x => x.TenantId == machine.TenantId &&
                                                                    x.SelectedDate > DateTime.Today.AddDays(-1) && // only sync records from  a day old .
                                                                        (x.CreationTime > machineSyncInput.LastSynced ||
                                                                            x.LastModificationTime > machineSyncInput.LastSynced ||
                                                                            x.DeletionTime > machineSyncInput.LastSynced
                                                                        )
                                                                    )
                                                            .ToListAsync();
                    _detailLogService.Log($"ProductMenu: get productMenu from server to sync to slave, unsyncedEntities -> " + JsonConvert.SerializeObject(unsyncedEntities));
                    var output = new EntitySyncOutputDto<ProductMenuSyncDto>();
                    foreach (var entity in unsyncedEntities)
                    {
                        if (entity.IsDeleted)
                        {
                            output.DeletionEntities.Add(new ProductMenuSyncDto()
                            {
                                Id = entity.Id
                            });
                        }
                        else
                        {
                            output.ModificationEntities.Add(new ProductMenuSyncDto()
                            {
                                Id = entity.Id,
                                DisplayOrder = entity.DisplayOrder.GetValueOrDefault(),
                                Price = entity.Price,
                                PriceStrategy = entity.ContractorPrice,// entity.PriceStrategies.Any() ? entity.PriceStrategies.First().Value.GetValueOrDefault() : 0,
                                ProductId = entity.ProductId,
                                PlateId = entity.PlateId,
                                SelectedDate = entity.SelectedDate,
                                SessionId = entity.SessionId
                            });
                        }
                        // calculate latest sync time, so that client can keep track
                        // ignore this because LastSyncedTimeStamp is set to Datetime.Now currently.
                        //if (output.LastSyncedTimeStamp < recordLastUpdated?.ToUnixTime())
                        //{
                        //    output.LastSyncedTimeStamp = recordLastUpdated.GetValueOrDefault().ToUnixTime();
                        //}
                    }
                    _detailLogService.Log($"ProductMenu: END get productMenu from server to sync to slave, data -> " + JsonConvert.SerializeObject(output));
                    return output;
                }
            }
            catch(Exception ex)
            {
                _detailLogService.Log($"ProductMenu: END get productMenu from server to sync to slave, error -> " + ex.ToString());
                return null;
            }
            
        }
        /// <summary>
        /// obsoleted
        /// </summary>
        /// <param name="syncData"></param>
        /// <returns></returns>
        [AbpAllowAnonymous]
        public async Task<bool> UpdateSyncStatus(SyncedItemData<Guid> syncData)
        {
            try
            {
                _detailLogService.Log($"ProductMenu: START UpdateSyncStatus, request -> {JsonConvert.SerializeObject(syncData)}");
                bool hasUpdate = false;
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MustHaveTenant, AbpDataFilters.MayHaveTenant, AbpDataFilters.SoftDelete))
                {
                    var machine = await _machineRepository.FirstOrDefaultAsync(x => x.Id == syncData.MachineId);
                    if (machine == null)
                    {
                        _detailLogService.Log($"ProductMenu: MachineId is {syncData.MachineId} does not exist");
                        return false;
                    }
                    else if (machine.IsDeleted)
                    {
                        _detailLogService.Log($"ProductMenu: MachineIs is {syncData.MachineId} was deleted");
                        return false;
                    }

                    var allPlateMenus = _productMenuRepository.GetAllIncluding()
                                                            .Where(x => x.TenantId == machine.TenantId &&
                                                                   x.SelectedDate.Date == DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc).Date)
                                                            .Include(x => x.PlateMenuMachineSyncStatus);

                    foreach (var item in syncData.SyncedItems)
                    {
                        var p = await allPlateMenus.FirstOrDefaultAsync(x => x.Id == item);
                        if (p == null) continue;

                        var pm = p.PlateMenuMachineSyncStatus.FirstOrDefault(x => x.MachineId.Equals(syncData.MachineId));
                        hasUpdate = true;
                        if (pm == null)
                        {
                            p.PlateMenuMachineSyncStatus.Add(
                                new PlateMenuMachineSyncStatus
                                {
                                    Id = Guid.NewGuid(),
                                    PlateMenuId = p.Id,
                                    MachineId = syncData.MachineId,
                                    IsSynced = true,
                                    SyncDate = DateTime.Now
                                });
                            continue;
                        }
                        pm.SyncDate = DateTime.Now;
                    }
                }
                if (hasUpdate)
                {
                    await CurrentUnitOfWork.SaveChangesAsync();
                    _detailLogService.Log($"ProductMenu: END UpdateSyncStatus");
                }
                return true;
            }
            catch (Exception ex)
            {
                _detailLogService.Log($"ProductMenu: END UpdateSyncStatus with error -> " + ex.ToString());
                return false;
            }
        }

        [AbpAuthorize(AppPermissions.Pages_PlateMenus_Replicate)]
        public async Task ReplicatePlateMenu(ReplicateInput input)
        {
            try
            {
                _detailLogService.Log($"ProductMenu: START ReplicatePlateMenu");
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
                await CurrentUnitOfWork.SaveChangesAsync();
                bool checkSendToRabbitMQ = await _sendMessageToMachineService.SendNotificationAsync(new KeyValueMessage() { Key = MessageKeys.MenuScheduler });
                _detailLogService.Log($"ProductMenu: END ReplicatePlateMenu, then send changes to rabbitmq is -> " + checkSendToRabbitMQ);
            }
            catch (Exception ex)
            {
                _detailLogService.Log($"ProductMenu: END ReplicatePlateMenu with error -> " + ex.ToString());
            }
        }

        private MenuSchedule.ProductMenu AddNewPlateMenu(MenuSchedule.ProductMenu pm, DateTime cloneDate)
        {
            var newPm = new MenuSchedule.ProductMenu
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
                var plateMenus = await _productMenuRepository.GetAllIncluding(x => x.Product, y => y.Session)
                    .Where(e => e.SelectedDate == inputDate.Date)
                    .Where(x => x.Product.IsDeleted == false)
                    .GroupBy(x => x.Session)
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

                var plateMenus = await _productMenuRepository.GetAllIncluding()
                    .Where(e => e.SelectedDate >= currentDay.Date && e.SelectedDate <= endNext7Day.Date)
                    .Include(x => x.Product)
                    //.Where(x => x.Product.IsDeleted == false)
                    .GroupBy(e => e.SelectedDate)
                    .ToListAsync();

                result.AddRange(plateMenus.Select(el => el.Key.Date.ToString("dd/MM/yyyy")));
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return new List<string>();
            }
        }

        public async Task<List<PlateMenuMonthResult>> GetMonthPlateMenus(DateTime inputDate)
        {
            // DateTime inputDate = DateTime.Now;
            var firstDayOfMonth = new DateTime(inputDate.Year, inputDate.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            try
            {
                var plateMenus = await _productMenuRepository.GetAllIncluding()
                    .Where(e => e.SelectedDate >= firstDayOfMonth.Date && e.SelectedDate <= lastDayOfMonth.Date)
                    .Include(x => x.Product)
                    //.Where(x => x.Product.IsDeleted == false)
                    .GroupBy(e => e.SelectedDate)
                    .ToListAsync();

                var listResult = new List<PlateMenuMonthResult>();

                for (int i = 0; i < plateMenus.Count; i++)
                {
                    var dailyMenu = plateMenus[i].ToList();
                    var plateMenu = new PlateMenuMonthResult
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
                return new List<PlateMenuMonthResult>();
            }
        }

    }
}