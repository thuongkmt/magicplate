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
using KonbiCloud.RFIDTable.Cache;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Linq.Dynamic.Core;
using KonbiCloud.CloudSync;
using KonbiCloud.Products.Helpers;

namespace KonbiCloud.Products
{
    [AbpAuthorize(AppPermissions.Pages_Products)]
    public class ProductAppService : KonbiCloudAppServiceBase, IProductAppService
    {
        private readonly IRepository<Product, Guid> _productRepository;
        
        private readonly IRepository<Category, Guid> _categoriesRepository;
        private readonly IConfigurationRoot _appConfiguration;
        private string _serverUrl;
        private readonly IDetailLogService _detailLogService;
        private readonly ISaveFromCloudService _saveFromCloudService;
        private readonly IFileStorageService _fileStorageService;
        private readonly ICacheManager _cacheManager;
        private readonly ISKUHelper _skuHelper;

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

        public ProductAppService(
            IRepository<Product, Guid> productRepository,
            
            IRepository<Category, Guid> categoriesRepository,
            IHostingEnvironment env,
            IDetailLogService detailLog,
            IFileStorageService fileStorageService,
            ICacheManager cacheManager,
            ISaveFromCloudService saveFromCloudService, ISKUHelper skuHelper)
        {
            _productRepository = productRepository;            
            _categoriesRepository = categoriesRepository;
            _appConfiguration = env.GetAppConfiguration();
            _detailLogService = detailLog;
            _cacheManager = cacheManager;
            _fileStorageService = fileStorageService;
            _saveFromCloudService = saveFromCloudService;
            _skuHelper = skuHelper;
        }

        public async Task<PagedResultDto<GetProductForView>> GetAll(GetAllProductsInput input)
        {
            try
            {
                var query = _productRepository.GetAllIncluding(x => x.Category)
                                    .WhereIf(!string.IsNullOrEmpty(input.Filter), e => e.Name.Contains(input.Filter.Trim(), StringComparison.OrdinalIgnoreCase) 
                                                                                        || (!string.IsNullOrEmpty(e.Desc)&& e.Desc.Contains(input.Filter.Trim(), StringComparison.OrdinalIgnoreCase)) 
                                                                                        || (!string.IsNullOrEmpty(e.Barcode) && e.Barcode.Contains(input.Filter.Trim(), StringComparison.OrdinalIgnoreCase)) 
                                                                                        || (!string.IsNullOrEmpty(e.SKU) && e.SKU.Contains(input.Filter.Trim(), StringComparison.OrdinalIgnoreCase)))
                                    .WhereIf(!string.IsNullOrEmpty(input.NameFilter), e => e.Name.Contains(input.NameFilter.Trim(), StringComparison.OrdinalIgnoreCase))
                                    .WhereIf(!string.IsNullOrEmpty(input.BarcodeFilter), e => e.Barcode.Contains(input.BarcodeFilter.Trim(), StringComparison.OrdinalIgnoreCase))
                                    .WhereIf(!string.IsNullOrEmpty(input.SKUFilter), e => e.SKU.Contains(input.SKUFilter, StringComparison.OrdinalIgnoreCase))
                                    .WhereIf(!string.IsNullOrEmpty(input.CategoryNameFilter), e => e.Category.Name.Equals(input.CategoryNameFilter.Trim(), StringComparison.OrdinalIgnoreCase))

                                    .Select(el => new GetProductForView()
                                    {
                                        Product = ObjectMapper.Map<ProductDto>(el),
                                        CategoryName = el.Category!=null?el.Category.Name: string.Empty,

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
                var output = new GetProductForEditOutput
                {
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

                if (input.Product.Id == null)
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

                await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();

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
            return _productRepository.GetAll().WhereIf(product.Id != null && product.Id != Guid.Empty, el => el.Id != product.Id).Any(el => el.SKU == product.SKU);
        }
        private string GetNewSKU(int length = 12)
        {
            var sku = _skuHelper.Generate(length);
            while (_productRepository.GetAll().Any(el => el.SKU == sku))
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

                await _productRepository.InsertAsync(product);

                await CurrentUnitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Products_Edit)]
        private async Task Update(GetProductForEditOutput input)
        {
            try
            {
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
                product.SKU = input.Product.SKU;
                product.Barcode = input.Product.Barcode;
                product.Price = input.Product.Price;
                product.ContractorPrice = input.Product.ContractorPrice;
                if (string.IsNullOrEmpty(product.SKU) && !string.IsNullOrEmpty(input.Product.SKU))
                    product.SKU = input.Product.SKU;
                product.DisplayOrder = input.Product.DisplayOrder;

                await _productRepository.UpdateAsync(product);

               

            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Products_Delete)]
        public async Task Delete(EntityDto<Guid> input)
        {
            try
            {
                await _productRepository.DeleteAsync(input.Id);
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
            }

        }

        [AbpAuthorize(AppPermissions.Pages_Products_Edit)]
        public async Task<bool> UpdatePrice(ProductDto input)
        {
            try
            {
                if (input.Id != Guid.Empty)
                {
                    var product = await _productRepository.FirstOrDefaultAsync(input.Id);
                    product.Price = input.Price;
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

        [AbpAuthorize(AppPermissions.Pages_Products_Edit)]
        public async Task<bool> UpdateDisplayOrder(ProductDto input)
        {
            try
            {
                if (input.Id != Guid.Empty)
                {
                    var product = await _productRepository.FirstOrDefaultAsync(input.Id);
                    product.DisplayOrder = input.DisplayOrder;
                    await _productRepository.UpdateAsync(product);
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

        [AbpAuthorize(AppPermissions.Pages_Products_Edit)]
        public async Task<bool> UpdateContractorPrice(ProductDto input)
        {
            try
            {
                if (input.Id != Guid.Empty)
                {
                    var product = await _productRepository.FirstOrDefaultAsync(input.Id);
                    product.ContractorPrice = input.ContractorPrice;
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

        [AbpAllowAnonymous]
        public async Task<bool> SyncProductData()
        {
            //// Sync Plate Category.
            //bool result = await _saveFromCloudService.SyncPlateCategoryData();
            //if (!result)
            //{
            //    return false;
            //}

            //// Sync Plate.
            //result = await _saveFromCloudService.SyncPlateData();
            //if (!result)
            //{
            //    return false;
            //}

            //// Sync Product Category.
            //result = await _saveFromCloudService.SyncProductCategoryData();
            //if (!result)
            //{
            //    return false;
            //}

            // Sync Product.
            return await _saveFromCloudService.SyncProductData();
        }
    }
}
