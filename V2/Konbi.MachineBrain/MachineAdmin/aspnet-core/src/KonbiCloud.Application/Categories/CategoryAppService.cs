using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Caching;
using Abp.Runtime.Session;
using Abp.UI;
using KonbiCloud.Authorization;
using KonbiCloud.Categories.Dtos;
using KonbiCloud.CloudSync;
using KonbiCloud.Common;
using KonbiCloud.MultiTenancy;
using KonbiCloud.Products;
using KonbiCloud.RFIDTable;
using KonbiCloud.RFIDTable.Cache;
using Microsoft.EntityFrameworkCore;

namespace KonbiCloud.Categories
{
    public class CategoryAppService : KonbiCloudAppServiceBase, ICategoryAppService
    {
        private readonly IRepository<Category, Guid> _categoryRepository;
        private readonly IFileStorageService fileStorageService;
        private readonly IDetailLogService _detailLogService;
        private readonly IRfidTableSignalRMessageCommunicator signalRCommunicator;
        private readonly ISaveFromCloudService _saveFromCloudService;
        private readonly ICacheManager _cacheManager;

        public CategoryAppService(
            IRepository<Category, Guid> categoryRepository, 
            IFileStorageService fileStorageService, 
            IRepository<Tenant> tenantRepository, 
            IDetailLogService detailLog,
            IRfidTableSignalRMessageCommunicator signalRCommunicator,
            ISaveFromCloudService saveFromCloudService,
            ICacheManager cacheManager
            )
        {
            _categoryRepository = categoryRepository;
            this.fileStorageService = fileStorageService;
            _detailLogService = detailLog;
            _cacheManager = cacheManager;
            this.signalRCommunicator = signalRCommunicator;
            _saveFromCloudService = saveFromCloudService;
        }

        [AbpAllowAnonymous]
        public async Task<PagedResultDto<GetCategoryForView>> GetAll(GetCategoryListInput input)
        {
            try
            {

                var cates = _categoryRepository.GetAll();

                var filteredCategories = _categoryRepository.GetAllIncluding()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.Name.Contains(input.Filter) || e.Description.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.NameFilter), e => e.Name.ToLower().Trim().Contains(input.NameFilter.ToLower().Trim()))
                        .Include(x => x.Products);

                var query = (from o in filteredCategories
                             select new GetCategoryForView()
                             {
                                 Category = ObjectMapper.Map<CategoryDto>(o)
                             });

                var categories = query
                    .OrderBy(input.Sorting ?? "category.name ASC")
                    .PageBy(input)
                    .ToList();

                var totalCount = query.Count();

                return new PagedResultDto<GetCategoryForView>(
                    totalCount,
                    categories
                    );
            }
            catch(Exception ex)
            {
                Logger.Error(ex.Message,ex);
                return new PagedResultDto<GetCategoryForView>();
            }
            
        }

        [AbpAuthorize(AppPermissions.Pages_Categories)]
        public async Task<GetCategoryForEditOutput> GetCategoryForEdit(EntityDto<Guid> input)
        {
            try
            {
                var category = await _categoryRepository.FirstOrDefaultAsync(input.Id);

                var output = new GetCategoryForEditOutput { Category = ObjectMapper.Map<CreateOrEditCategoryDto>(category) };

                return output;
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
                return new GetCategoryForEditOutput();
            }
        }

        public async Task CreateOrEdit(CreateOrEditCategoryDto input)
        {
            if (input.Id == null)
            {
                await Create(input);
            }
            else
            {
                await Update(input);
            }
            await signalRCommunicator.NotifyProductChanges("Category");
        }

        [AbpAuthorize(AppPermissions.Pages_Categories_Create)]
        public async Task Create(CreateOrEditCategoryDto input)
        {
            try
            {
                var category = new Category();

                category.Name = input.Name;
                category.Description = input.Description;

                if (AbpSession.TenantId != null)
                {
                    category.TenantId = (int)AbpSession.TenantId;
                }
                await _categoryRepository.InsertAsync(category);
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Categories_Edit)]
        public async Task Update(CreateOrEditCategoryDto input)
        {
            try
            {
                var category = await _categoryRepository.FirstOrDefaultAsync((Guid)input.Id);

                category.Name = input.Name;
                category.Description = input.Description;

                await _categoryRepository.UpdateAsync(category);
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Categories_Delete)]
        public  async Task Delete(EntityDto<Guid> input)
        {
            try
            {
                await _categoryRepository.DeleteAsync(input.Id);
                await signalRCommunicator.NotifyProductChanges("Category");
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
            }
        }

        [AbpAllowAnonymous]
        public async Task<bool> SyncProductCategoryData()
        {            
            return await _saveFromCloudService.SyncProductCategoryData();
        }
    }
}
