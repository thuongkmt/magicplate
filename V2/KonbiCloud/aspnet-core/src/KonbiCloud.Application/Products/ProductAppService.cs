using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Caching;
using Abp.Runtime.Session;
using Abp.UI;
using KonbiCloud.Authorization;
using KonbiCloud.Common;
using KonbiCloud.Common.Dtos;
using KonbiCloud.Configuration;
using KonbiCloud.Machines;
using KonbiCloud.Machines.Dtos;
using KonbiCloud.Messaging;
using KonbiCloud.Messaging.Events;
using KonbiCloud.Products.Dtos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Linq.Dynamic.Core;
using Konbini.Messages;
using Konbini.Messages.Enums;
using Abp.Domain.Uow;
using KonbiCloud.PlateMenus.Dtos;
using KonbiCloud.Dto;
using System.Text.RegularExpressions;
using KonbiCloud.Products.Helpers;
using Newtonsoft.Json;

namespace KonbiCloud.Products
{
    [AbpAuthorize(AppPermissions.Pages_Products)]
    public class ProductAppService : KonbiCloudAppServiceBase, IProductAppService
    {
        private readonly IRepository<Product, Guid> _productRepository;
        private readonly IRepository<Plate.Plate, Guid> _plateRepository;
        
        private readonly IRepository<Category, Guid> _categoriesRepository;
        private readonly IConfigurationRoot _appConfiguration;
        private readonly string ServerUrl;
        private readonly IDetailLogService _detailLogService;

        private readonly IFileStorageService _fileStorageService;
        private readonly ICacheManager _cacheManager;
        private readonly ISendMessageToMachineService _sendMessageToMachineService;
        private readonly IRepository<Machine, Guid> _machineRepository;
        private readonly ISKUHelper _skuHelper;
        public ProductAppService(
            IRepository<Plate.Plate, Guid> plateRepository,
            IRepository<Product, Guid> productRepository,
            IRepository<Category, Guid> categoriesRepository,
            IHostingEnvironment env,
            IDetailLogService detailLog,
            IFileStorageService fileStorageService,
            ICacheManager cacheManager,
            ISendMessageToMachineService sendMessageToMachineService,
            IRepository<Machine, Guid> machineRepository, ISKUHelper skuHelper)
        {
            _plateRepository = plateRepository;
            _productRepository = productRepository;
            _categoriesRepository = categoriesRepository;
            _appConfiguration = env.GetAppConfiguration();
            _detailLogService = detailLog;
            _cacheManager = cacheManager;
            _fileStorageService = fileStorageService;
            ServerUrl = _appConfiguration["App:ServerRootAddress"] ?? "";
            _sendMessageToMachineService = sendMessageToMachineService;
            _machineRepository = machineRepository;
            _skuHelper = skuHelper;
            
        }

        public async Task<PagedResultDto<GetProductForView>> GetAll(GetAllProductsInput input)
        {
            try
            {
                var query = _productRepository.GetAllIncluding(x => x.Category)
                                    .WhereIf(!string.IsNullOrEmpty(input.Filter), e => false || e.Name.Contains(input.Filter.Trim(), StringComparison.OrdinalIgnoreCase) || e.ImageUrl.Contains(input.Filter, StringComparison.OrdinalIgnoreCase) || e.Desc.Contains(input.Filter.Trim(), StringComparison.OrdinalIgnoreCase) || e.Barcode.Contains(input.Filter.Trim(), StringComparison.OrdinalIgnoreCase) || e.SKU.Contains(input.Filter.Trim(), StringComparison.OrdinalIgnoreCase))
                                    .WhereIf(!string.IsNullOrEmpty(input.NameFilter), e => e.Name.Contains(input.NameFilter.Trim(), StringComparison.OrdinalIgnoreCase))
                                    .WhereIf(!string.IsNullOrEmpty(input.BarcodeFilter), e => e.Barcode.Contains(input.BarcodeFilter.Trim(), StringComparison.OrdinalIgnoreCase))
                                    .WhereIf(!string.IsNullOrEmpty(input.SKUFilter), e => e.SKU.Contains(input.SKUFilter, StringComparison.OrdinalIgnoreCase))
                                    .WhereIf(!string.IsNullOrEmpty(input.CategoryNameFilter), e => e.Category.Name.Equals(input.CategoryNameFilter.Trim(), StringComparison.OrdinalIgnoreCase))

                                    .Select(el=> new GetProductForView()
                                    {
                                        Product = ObjectMapper.Map<ProductDto>(el),
                                        CategoryName =el.Category.Name,
                                       
                                    });
               
                var totalCount = await query.CountAsync();

                var products = await query
                    .OrderBy(input.Sorting ?? "product.name")
                    .PageBy(input).ToListAsync();

                var pathImage = Path.Combine(ServerUrl, Const.ImageFolder, _appConfiguration[AppSettingNames.ProductImageFolder]);
                foreach (var p in products)
                {
                    if (p.Product != null && p.Product.ImageUrl != null)
                    {
                        if (p.Product.ImageUrl.Contains(Const.NoImage)) continue;

                        var arrImage = p.Product.ImageUrl.Split("|");
                        var images = "";
                        for (int index = 0; index < arrImage.Length; index++)
                        {
                            if (index == arrImage.Length - 1)
                            {
                                images += Path.Combine(pathImage, arrImage[index]);
                            }
                            else
                            {
                                images += Path.Combine(pathImage, arrImage[index]) + '|';
                            }
                        }
                        p.Product.ImageUrl = images;
                    }
                }
                return new PagedResultDto<GetProductForView>(
                    totalCount,
                    products
                );
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
                return new PagedResultDto<GetProductForView>(0, new List<GetProductForView>());
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Products_Edit)]
        public async Task<GetProductForEditOutput> GetProductForEdit(EntityDto<Guid> input)
        {
            try
            {
                var product = await _productRepository.FirstOrDefaultAsync(input.Id);
                // Product = ObjectMapper.Map<CreateOrEditProductDto>(product)
                var output = new GetProductForEditOutput {
                    Product = new CreateOrEditProductDto
                    {
                        Id = input.Id,
                        Name = product.Name,
                        ImageUrl = product.ImageUrl,
                        Desc = product.Desc,
                        CategoryId = product.CategoryId,
                        CategoryName = "",                     
                        Price = product.Price,
                        ContractorPrice = product.ContractorPrice,
                        Barcode = product.Barcode,
                        SKU = product.SKU,
                        DisplayOrder = product.DisplayOrder
                    },
                    CategoryName = ""
                };

                if (output.Product.CategoryId != null)
                {
                    var category = await _categoriesRepository.FirstOrDefaultAsync((Guid)output.Product.CategoryId);
                    if (category != null)
                    {
                        output.CategoryName = category.Name;
                    }

                }

                if (output.Product.ImageUrl != null)
                {
                    var pathImage = Path.Combine(ServerUrl, Const.ImageFolder, _appConfiguration[AppSettingNames.ProductImageFolder]);

                    var arrImage = output.Product.ImageUrl.Split("|");
                    var images = "";
                    for (int index = 0; index < arrImage.Length; index++)
                    {
                        if (index == arrImage.Length - 1)
                        {
                            images += Path.Combine(pathImage, arrImage[index]);
                        }
                        else
                        {
                            images += Path.Combine(pathImage, arrImage[index]) + '|';
                        }
                    }
                    output.Product.ImageUrl = images;
                }

                return output;
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
                return new GetProductForEditOutput();
            }
        }

        public async Task<ProductMessage> CreateOrEdit(GetProductForEditOutput input)
        {
            try
            {
                _detailLogService.Log($"Product: START checking");
                // Get all product for check exists.
                var productAll = input.Product.Id == null ? _productRepository.GetAll() : _productRepository.GetAll().Where(x => x.Id != input.Product.Id);
                // Check exists barcode.
                if (!string.IsNullOrWhiteSpace(input.Product.Barcode))
                {

                    bool exists = productAll.Any(x => x.Barcode == input.Product.Barcode);
                    if (exists)
                    {
                        return new ProductMessage { Message = $"Barcode {input.Product.Barcode} already exists." };
                    }
                }

                // Check exists Product Name.
                if (!string.IsNullOrWhiteSpace(input.Product.Name))
                {

                    bool exists = productAll.Any(x => x.Name == input.Product.Name);
                    if (exists)
                    {
                        return new ProductMessage { Message = $"Product Name {input.Product.Name} already exists." };
                    }
                }

                if (input.Product.Id == null)
                {
                    if(!input.Product.AutoGenerateSKU&& string.IsNullOrEmpty(input.Product.SKU))
                        return new ProductMessage { Message = $"SKU is required" };
                    
                    if (input.Product.AutoGenerateSKU && string.IsNullOrEmpty(input.Product.SKU))
                    {
                        input.Product.SKU = GetNewSKU();
                    }
                    if (string.IsNullOrEmpty(input.Product.SKU))
                    {
                        return new ProductMessage { Message = $"SKU is required" };
                    }
                    else
                    {
                        input.Product.SKU = input.Product.SKU.ToUpper();
                    }
                    if (!_skuHelper.Validate(input.Product.SKU))
                    {
                        return new ProductMessage { Message = $"SKU is invalid. SKU should not contain whitespace nor special characters" };
                    }
                    if (CheckSKUExisting(input.Product))
                    {
                        return new ProductMessage { Message = $"SKU is existing" };
                    }
                    await Create(input);
                }
                else
                {
                    if (!input.Product.AutoGenerateSKU && string.IsNullOrEmpty(input.Product.SKU))
                        return new ProductMessage { Message = $"SKU is required" };

                    if (input.Product.AutoGenerateSKU && string.IsNullOrEmpty(input.Product.SKU))
                    {
                        input.Product.SKU = GetNewSKU();
                    }
                    if (string.IsNullOrEmpty(input.Product.SKU))
                    {
                        return new ProductMessage { Message = $"SKU is required" };
                    }
                    else
                    {
                        input.Product.SKU = input.Product.SKU.ToUpper();
                    }
                    if (!_skuHelper.Validate(input.Product.SKU))
                    {
                        return new ProductMessage { Message = $"SKU is invalid. SKU should not contain whitespace nor special characters" };
                    }
                    if (CheckSKUExisting(input.Product))
                    {
                        return new ProductMessage { Message = $"SKU is existing" };
                    }
                    await Update(input);
                }
                

                return new ProductMessage { Message = null };
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
                return new ProductMessage { Message = "An error occurred, please try again." };
            }
        }

        private bool CheckSKUExisting(CreateOrEditProductDto product)
        {
            return _productRepository.GetAll().WhereIf(product.Id!=null&& product.Id!= Guid.Empty,el=> el.Id !=product.Id).Any(el => el.SKU == product.SKU);
        }

        private string  GetNewSKU(int length=12)
        {
            var sku = _skuHelper.Generate(length);
            while(_productRepository.GetAll().Any(el=> el.SKU == sku))
            {
                sku = _skuHelper.Generate(length);
            }
            return sku;
        }

        [AbpAuthorize(AppPermissions.Pages_Products_Create)]
        private async Task Create(GetProductForEditOutput input)
        {
            try
            {
                _detailLogService.Log($"Product: START Update");
                var product = new Product();
                product.Name = input.Product.Name;
                product.Desc = input.Product.Desc;
                product.ImageUrl = input.Product.ImageUrl;
                product.CategoryId = input.Product.CategoryId;
                product.SKU = input.Product.SKU;
                product.Barcode = input.Product.Barcode;
                product.Price = input.Product.Price;              
                product.ContractorPrice = input.Product.ContractorPrice;
                product.DisplayOrder = input.Product.DisplayOrder;
                if (AbpSession.TenantId != null)
                {
                    product.TenantId = AbpSession.TenantId;
                }

                var productId = await _productRepository.InsertAsync(product);
                await CurrentUnitOfWork.SaveChangesAsync();
                bool checkSendToRabbitMQ = await _sendMessageToMachineService.SendNotificationAsync(new KeyValueMessage() { Key = MessageKeys.Product });
                _detailLogService.Log($"Product: END Create, then send changes to rabbitmq is -> " + checkSendToRabbitMQ);
            }
            catch (Exception ex)
            {
                _detailLogService.Log("Product: END Create with error -> " + ex.ToString());
            }

        }

        [AbpAuthorize(AppPermissions.Pages_Products_Edit)]
        private async Task Update(GetProductForEditOutput input)
        {
            try
            {
                _detailLogService.Log($"Product: START Update");
                if (input.Product.ImageUrl != null)
                {
                    var newProductImage = "";
                    var arrImage = input.Product.ImageUrl.Split("|");
                    for (int index = 0; index < arrImage.Length; index++)
                    {
                        if (index == arrImage.Length - 1)
                        {
                            newProductImage += Path.GetFileName(arrImage[index]);
                        }
                        else
                        {
                            newProductImage += (Path.GetFileName(arrImage[index]) + '|');
                        }
                    }
                    input.Product.ImageUrl = newProductImage;
                }

                var product = await _productRepository.FirstOrDefaultAsync((Guid)input.Product.Id);

                product.Name = input.Product.Name;
                product.Desc = input.Product.Desc;
                product.ImageUrl = input.Product.ImageUrl;
                product.CategoryId = input.Product.CategoryId;
                if(string.IsNullOrEmpty(product.SKU)&&!string.IsNullOrEmpty(input.Product.SKU))
                    product.SKU = input.Product.SKU;
                product.Barcode = input.Product.Barcode;
                product.Price = input.Product.Price;
                product.ContractorPrice = input.Product.ContractorPrice;             
                product.DisplayOrder = input.Product.DisplayOrder;

                await _productRepository.UpdateAsync(product);
                bool checkSendToRabbitMQ = await _sendMessageToMachineService.SendNotificationAsync(new KeyValueMessage() { Key = MessageKeys.Product });
                _detailLogService.Log($"Product: END Update, then send changes to rabbitmq is -> " + checkSendToRabbitMQ);
            }
            catch (Exception ex)
            {
                _detailLogService.Log("Product: END Update with error -> " + ex.ToString());
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Products_Delete)]
        public async Task Delete(EntityDto<Guid> input)
        {
            try
            {
                _detailLogService.Log($"Product: START Delete");
                var product = await _productRepository.FirstOrDefaultAsync(input.Id);
                if (product != null)
                {
                    await _productRepository.DeleteAsync(input.Id);
                    bool checkSendToRabbitMQ = await _sendMessageToMachineService.SendNotificationAsync(new KeyValueMessage() { Key = MessageKeys.Product });
                    _detailLogService.Log($"Product: END Delete, then send changes to rabbitmq is -> " + checkSendToRabbitMQ);
                }
            }
            catch (Exception ex)
            {
                _detailLogService.Log("Product: END Delete with error -> " + ex.ToString());
            }

        }

        [AbpAllowAnonymous]
        public async Task<EntitySyncOutputDto<ProductSyncDto>> GetProducts(EntitySyncInputDto<Guid> machineSyncInput)
        {
            _detailLogService.Log($"Product: START get products from server to sync to slave, request -> {JsonConvert.SerializeObject(machineSyncInput)}");
            try
            {
                var unsyncedEntities = new List<Product>();
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MustHaveTenant, AbpDataFilters.MayHaveTenant, AbpDataFilters.SoftDelete))
                {
                    var machine = await _machineRepository.FirstOrDefaultAsync(x => x.Id == machineSyncInput.Id);
                    if (machine == null)
                    {
                        _detailLogService.Log($"Product: MachineId: {machineSyncInput.Id} does not exist");
                        return null;
                    }
                    else if (machine.IsDeleted)
                    {
                        _detailLogService.Log($"Product: Machine with id: {machineSyncInput.Id} is deleted");
                        return null;
                    }

                    //products = await _productRepository.GetAllListAsync(x => x.TenantId == machine.TenantId);
                    unsyncedEntities = await _productRepository.GetAllIncluding(el=> el.Category).Where(x => x.TenantId == machine.TenantId &&
                                                                  // get data with last synced timestamp
                                                                  (x.CreationTime > machineSyncInput.LastSynced ||
                                                                      x.LastModificationTime > machineSyncInput.LastSynced ||
                                                                      x.DeletionTime > machineSyncInput.LastSynced)).ToListAsync();

                }
                var output = new EntitySyncOutputDto<ProductSyncDto>();
                foreach (var entity in unsyncedEntities)
                {
                    if (entity.IsDeleted)
                    {
                        output.DeletionEntities.Add(new ProductSyncDto()
                        {
                            Id = entity.Id
                        });
                    }
                    else
                    {
                        var modification = new ProductSyncDto()
                        {
                            Id = entity.Id,
                            Name = entity.Name,
                            Desc = entity.Desc,
                            ImageUrl = entity.ImageUrl,
                            Barcode = entity.Barcode,
                            ContractorPrice = entity.ContractorPrice,
                            DisplayOrder = entity.DisplayOrder,
                            FromDate    = entity.FromDate,
                            ImageChecksum = entity.ImageChecksum,                            
                            Price = entity.Price,
                            ShortDesc1 = entity.ShortDesc1,
                            ShortDesc2 = entity.ShortDesc2,
                            ShortDesc3 = entity.ShortDesc3,
                            SKU = entity.SKU,
                            Status = entity.Status,
                            ToDate = entity.ToDate,
                            Unit = entity.Unit,
                            CategoryId = entity.CategoryId
                        };

                        output.ModificationEntities.Add(modification);
                    }
                    // calculate latest sync time, so that client can keep track
                    //var recordLastUpdated = new[] { cat.CreationTime, cat.LastModificationTime, cat.DeletionTime }.Max();

                    // ignore this because LastSyncedTimeStamp is set to Datetime.Now currently.
                    //if (output.LastSyncedTimeStamp < recordLastUpdated?.ToUnixTime())
                    //{
                    //    output.LastSyncedTimeStamp = recordLastUpdated.GetValueOrDefault().ToUnixTime();
                    //}
                }
                _detailLogService.Log($"Product: END get products from server to sync to slave -> " + JsonConvert.SerializeObject(output));
                return output;

            }
           
        
            catch (Exception ex)
            {
                _detailLogService.Log("Product: get products from server to sync to slave ->" + ex.ToString());
                return null;
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Products_Edit)]
        public async Task<bool> UpdatePrice(ProductDto input)
        {
            try
            {
                _detailLogService.Log($"Product: START updatePrice");
                if (input.Id != Guid.Empty)
                {
                    var product = await _productRepository.FirstOrDefaultAsync(input.Id);
                    product.Price = input.Price;
                    bool checkSendToRabbitMQ = await _sendMessageToMachineService.SendNotificationAsync(new KeyValueMessage() { Key = MessageKeys.Product });
                    _detailLogService.Log($"Product: END updatePrice, then send changes to rabbitmq is -> " + checkSendToRabbitMQ);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _detailLogService.Log("Product: END updatePrice with error -> " + ex.ToString());
                return false;
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Products_Edit)]
        public async Task<bool> UpdateDisplayOrder(ProductDto input)
        {
            try
            {
                _detailLogService.Log($"Product: START UpdateDisplayOrder");
                if (input.Id != Guid.Empty)
                {
                    var product = await _productRepository.FirstOrDefaultAsync(input.Id);
                    product.DisplayOrder = input.DisplayOrder;
                    bool checkSendToRabbitMQ = await _sendMessageToMachineService.SendNotificationAsync(new KeyValueMessage() { Key = MessageKeys.Product });
                    _detailLogService.Log($"Product: END UpdateDisplayOrder, then send changes to rabbitmq is -> " + checkSendToRabbitMQ);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _detailLogService.Log("Product: END UpdateDisplayOrder with error -> " + ex.ToString());
                return false;
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Products_Edit)]
        public async Task<bool> UpdateContractorPrice(ProductDto input)
        {
            try
            {
                _detailLogService.Log($"Product: START UpdateContractorPrice");
                if (input.Id != Guid.Empty)
                {
                    var product = await _productRepository.FirstOrDefaultAsync(input.Id);
                    product.ContractorPrice = input.ContractorPrice;
                    bool checkSendToRabbitMQ = await _sendMessageToMachineService.SendNotificationAsync(new KeyValueMessage() { Key = MessageKeys.Product });
                    _detailLogService.Log($"Product: END UpdateDisplayOrder, then send changes to rabbitmq is -> " + checkSendToRabbitMQ);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _detailLogService.Log("Product: END UpdateDisplayOrder with error -> " + ex.ToString());
                return false;
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Products_Import)]
        public async Task<ImportResult> ImportProduct(List<CreateOrEditProductDto> input)
        {
            try
            {
                var listError = new List<string>();
                var listSuccess = new List<CreateOrEditProductDto>();
                var products = await _productRepository.GetAllListAsync();
                var plates = await _plateRepository.GetAllListAsync();
                var categories = await _categoriesRepository.GetAllListAsync();

                for (int i = 0; i < input.Count; i++)
                {
                    var inputProduct = input[i];
                    if (!string.IsNullOrEmpty(inputProduct.Name))
                    {
                        inputProduct.Name = inputProduct.Name.Trim();
                    }

                    // Check required product name.
                    if (!string.IsNullOrEmpty(inputProduct.Name))
                    {
                        // Format product for import.
                        inputProduct.Barcode = !string.IsNullOrEmpty(inputProduct.Barcode) ? Regex.Match(inputProduct.Barcode.Trim(), @"\d+").Value : "";                      
                        var product = FormatImportProduct(inputProduct, plates, categories);
                        
                        var p = CheckExistProduct(product, products);
                        
                        if (p == null)
                        {
                            var isError = await VerifyImportProduct(product, products, inputProduct, listError, i);

                            if (!isError)
                            {
                                await _productRepository.InsertAsync(product);
                                listSuccess.Add(inputProduct);
                            }
                        }
                        else
                        {
                            p.CategoryId = product.CategoryId;
                            if (!string.IsNullOrEmpty(product.SKU))
                                p.SKU = product.SKU;
                            p.Name = product.Name;                            
                            p.Barcode = product.Barcode;
                            p.ImageUrl = product.ImageUrl;
                            p.Desc = product.Desc;
                            p.Price = product.Price;
                            p.DisplayOrder = product.DisplayOrder;

                            await _productRepository.UpdateAsync(p);
                            listSuccess.Add(inputProduct);
                        }
                    }
                    else
                    {
                        listError.Add("Row " + (i + 2) + "- Name is null");
                    }
                }
                if (listSuccess.Any())
                {
                    await CurrentUnitOfWork.SaveChangesAsync();
                    await _sendMessageToMachineService.SendNotificationAsync(new KeyValueMessage() { Key = MessageKeys.Product });
                }
                var result = new ImportResult
                {
                    ErrorCount = listError.Count,
                    SuccessCount = listSuccess.Count,
                    ErrorList = string.Join(", ", listError.Take(100).Select(x => x.ToString()).ToArray())
                };
                if (listError.Count > 100)
                {
                    result.ErrorList = string.Join(", ", listError.Take(100).Select(x => x.ToString()).ToArray()) + "...";
                }
                else
                {
                    result.ErrorList = string.Join(", ", listError.Take(100).Select(x => x.ToString()).ToArray());
                }
                return result;
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
                return new ImportResult
                {
                    ErrorCount = 0,
                    SuccessCount = 0,
                    ErrorList = "Error"
                };
            }
        }

        /// <summary>
        /// Verify Import Product.
        /// </summary>
        /// <param name="product"></param>
        /// <param name="products"></param>
        /// <param name="inputProduct"></param>
        /// <param name="listError"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private async Task<bool> VerifyImportProduct(Product product, List<Product> products, CreateOrEditProductDto inputProduct, List<string> listError, int i)
        {
            bool isError = false;
            if (product.CategoryId != null)
            {
                var pc = await _categoriesRepository.FirstOrDefaultAsync(x => x.Id.Equals(product.CategoryId));
                if (pc == null)
                {
                    isError = true;
                    listError.Add("Row " + (i + 1) + "- Category Name " + inputProduct.CategoryName + " is available");
                    var errMess = "Import error: Category Name " + inputProduct.CategoryName + " is available";
                    Logger.Error(errMess);
                    return isError;
                }
            }
            // Check duplicate Barcode.
            if (products.Any(x => x.Barcode == product.Barcode && !string.IsNullOrEmpty(product.Barcode)))
            {
                isError = true;
                listError.Add("Row " + (i + 1) + "- Barcode " + inputProduct.Barcode + " is available");
                var errMess = "Import error: Barcode " + inputProduct.Barcode + " is available";
                Logger.Error(errMess);
                return isError;
            }
            // Check duplicate sku.
            if (!string.IsNullOrEmpty(product.SKU) &&products.Any(x => x.SKU == product.SKU))
            {
                isError = true;
                listError.Add("Row " + (i + 1) + "- SKU " + inputProduct.Barcode + " is available");
                var errMess = "Import error: SKU " + inputProduct.Barcode + " is available";
                Logger.Error(errMess);
                return isError;
            }

            if (products.Any(x => x.Name == product.Name))
            {
                isError = true;
                listError.Add("Row " + (i + 1) + "- Product Name " + inputProduct.Name + " is available");
                var errMess = "Import error: Product Name " + inputProduct.Name + " is available";
                Logger.Error(errMess);
                return isError;
            }

            return isError;
        }

        /// <summary>
        /// Format Import Product.
        /// </summary>
        /// <param name="inputProduct"></param>
        /// <param name="plates"></param>
        /// <param name="categories"></param>
        /// <returns></returns>
        private Product FormatImportProduct(CreateOrEditProductDto inputProduct, List<Plate.Plate> plates, List<Category> categories)
        {            
            var category = categories.FirstOrDefault(x => x.Name == inputProduct.CategoryName.Trim());

            var product = new Product();
            product.Name = inputProduct.Name.Trim();
            product.Id = Guid.NewGuid();
           
            if (category != null)
            {
                product.CategoryId = category.Id;
            }
            product.Barcode = !string.IsNullOrEmpty(inputProduct.Barcode) ? Regex.Match(inputProduct.Barcode, @"\d+").Value : "";
            //TODO: check SKU unique
            product.SKU = inputProduct.SKU;
            product.Price = inputProduct.Price;
            product.Desc = inputProduct.Desc;
            if (inputProduct.ImageUrl != null)
            {
                var newProductImage = "";
                var arrImage = inputProduct.ImageUrl.Split("|");
                for (int index = 0; index < arrImage.Length; index++)
                {
                    if (index == arrImage.Length - 1)
                    {
                        newProductImage += Path.GetFileName(arrImage[index]);
                    }
                    else
                    {
                        newProductImage += (Path.GetFileName(arrImage[index]) + '|');
                    }
                }
                product.ImageUrl = newProductImage;
                product.DisplayOrder = inputProduct.DisplayOrder;
            }

            if (AbpSession.TenantId != null)
            {
                product.TenantId = (int?)AbpSession.TenantId;
            }

            return product;
        }

        private Product CheckExistProduct(Product product, List<Product> products)
        {
            Product result = new Product();
            if (!string.IsNullOrEmpty(product.SKU))
            {
                result = products.FirstOrDefault(x => x.SKU == product.SKU);
                if(result!=null)
                    return result;
            }

            if (!String.IsNullOrEmpty(product.Barcode))
            {
                result = products.FirstOrDefault(x => x.Barcode == product.Barcode);
                if (result != null)
                    return result;
            }
            if (String.IsNullOrEmpty(product.Barcode))
            {
                result = products.FirstOrDefault(x => x.Name.Trim() == product.Name);
                if (result != null)
                    return result;
            }

          

            else
            {
                result = products.FirstOrDefault(x => x.Name.Trim() == product.Name );
                if (result != null)
                    return result;
            }
            return null;

        }
    }
}
