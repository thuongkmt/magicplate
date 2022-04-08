using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using KonbiCloud.Authorization;
using KonbiCloud.Categories.Dtos;
using KonbiCloud.Common;
using KonbiCloud.Dto;
using KonbiCloud.Machines;
using KonbiCloud.Machines.Dtos;
using KonbiCloud.Messaging;
using KonbiCloud.MultiTenancy;
using KonbiCloud.Products;
using Konbini.Messages;
using Konbini.Messages.Enums;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace KonbiCloud.Categories
{
    public class CategoryAppService : KonbiCloudAppServiceBase, ICategoryAppService
    {
        private readonly IRepository<Category, Guid> _categoryRepository;
        private readonly IFileStorageService fileStorageService;
        private readonly IDetailLogService _detailLogService;
        private readonly ISendMessageToMachineService _sendMessageToMachineService;
        private readonly IRepository<Machine, Guid> _machineRepository;

        public CategoryAppService(
            IRepository<Category, Guid> categoryRepository,
            IFileStorageService fileStorageService,
            IRepository<Tenant> tenantRepository,
            IDetailLogService detailLog,
            ISendMessageToMachineService sendMessageToMachineService,
            IRepository<Machine, Guid> machineRepository
            )
        {
            _categoryRepository = categoryRepository;
            this.fileStorageService = fileStorageService;
            _detailLogService = detailLog;
            _sendMessageToMachineService = sendMessageToMachineService;
            _machineRepository = machineRepository;
        }

        [AbpAuthorize(AppPermissions.Pages_Categories)]
        public async Task<PagedResultDto<GetCategoryForView>> GetAll(GetCategoryListInput input)
        {
            try
            {
                var filteredCategories = _categoryRepository.GetAllIncluding()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.Name.Contains(input.Filter) || e.Description.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.NameFilter), e => e.Name.ToLower().Contains(input.NameFilter.ToLower().Trim()))
                        .Include(x => x.Products);

                var query = (from o in filteredCategories
                             select new GetCategoryForView()
                             {
                                 Category = ObjectMapper.Map<CategoryDto>(o)
                             });

                var categories = query
                    .OrderBy(input.Sorting ?? "category.name")
                    .PageBy(input)
                    .ToList();

                var totalCount = query.Count();

                return new PagedResultDto<GetCategoryForView>(
                    totalCount,
                    categories
                    );
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
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

            // Send Queued Msg To Machines.
            //await _sendMessageToMachineService.SendNotificationAsync(new KeyValueMessage() { Key = MessageKeys.ProductCategory });
        }

        [AbpAuthorize(AppPermissions.Pages_Categories_Create)]
        public async Task Create(CreateOrEditCategoryDto input)
        {
            try
            {
                _detailLogService.Log($"Category: START create");
                var category = new Category();

                category.Name = input.Name.Trim();
                category.Description = input.Description;

                if (AbpSession.TenantId != null)
                {
                    category.TenantId = (int)AbpSession.TenantId;
                }
                await _categoryRepository.InsertAsync(category);
                bool checkSendToRabbitMQ = await _sendMessageToMachineService.SendNotificationAsync(new KeyValueMessage() { Key = MessageKeys.ProductCategory });
                _detailLogService.Log($"Category: END create, then send changes to rabbitmq is -> " + checkSendToRabbitMQ);
            }
            catch (Exception ex)
            {
                _detailLogService.Log("Category: END create with error -> " + ex.ToString());
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Categories_Edit)]
        public async Task Update(CreateOrEditCategoryDto input)
        {
            _detailLogService.Log($"Category: START update");
            try
            {
                var category = await _categoryRepository.FirstOrDefaultAsync((Guid)input.Id);

                category.Name = input.Name.Trim();
                category.Description = input.Description;

                await _categoryRepository.UpdateAsync(category);
                bool checkSendToRabbitMQ = await _sendMessageToMachineService.SendNotificationAsync(new KeyValueMessage() { Key = MessageKeys.ProductCategory });
                _detailLogService.Log($"Category: END update, then send changes to rabbitmq is -> " + checkSendToRabbitMQ);
            }
            catch (Exception ex)
            {
                _detailLogService.Log("Category: END update with error -> " + ex.ToString());
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Categories_Delete)]
        public async Task Delete(EntityDto<Guid> input)
        {
            _detailLogService.Log($"Category: START delete");
            try
            {
                await _categoryRepository.DeleteAsync(input.Id);

                // Send Queued Msg To Machines.
                bool checkSendToRabbitMQ = await _sendMessageToMachineService.SendNotificationAsync(new KeyValueMessage() { Key = MessageKeys.ProductCategory });
                _detailLogService.Log($"Category: END delete, then send changes to rabbitmq is -> " + checkSendToRabbitMQ);
            }
            catch (Exception ex)
            {
                _detailLogService.Log("Category: END with error -> " + ex.Message);
            }
        }
        /// <summary>
        /// Sync data between Server and Slave, To be Called from Slave side to this API
        /// </summary>
        /// <param name="machineSyncInput"></param>
        /// <returns></returns>
        [AbpAllowAnonymous]
        public async Task<EntitySyncOutputDto<CategorySyncDto>> GetCategories(EntitySyncInputDto<Guid> machineSyncInput)
        {
            try
            {
                _detailLogService.Log($"Category: START get category from server to sync to slave, request -> {JsonConvert.SerializeObject(machineSyncInput)}");
                var unsyncedEntities = new List<Category>();
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MustHaveTenant, AbpDataFilters.MayHaveTenant, AbpDataFilters.SoftDelete))
                {
                    var machine = await _machineRepository.FirstOrDefaultAsync(x => x.Id == machineSyncInput.Id);
                    if (machine == null)
                    {
                        _detailLogService.Log($"Category: MachineId is {machineSyncInput.Id} does not exist");
                        return null;
                    }
                    else if (machine.IsDeleted)
                    {
                        _detailLogService.Log($"Category: MachineId is {machineSyncInput.Id} was deleted");
                        return null;
                    }


                    unsyncedEntities = await _categoryRepository.GetAllListAsync(x => x.TenantId == machine.TenantId &&
                                                                      // get data with last synced timestamp
                                                                      (x.CreationTime > machineSyncInput.LastSynced ||
                                                                          x.LastModificationTime > machineSyncInput.LastSynced ||
                                                                          x.DeletionTime > machineSyncInput.LastSynced
                                                                      ));
                }
                var output = new EntitySyncOutputDto<CategorySyncDto>();
                foreach (var entity in unsyncedEntities)
                {
                    if (entity.IsDeleted)
                    {
                        output.DeletionEntities.Add(new CategorySyncDto()
                        {
                            Id = entity.Id
                        });
                    }
                    else
                    {
                        output.ModificationEntities.Add(new CategorySyncDto()
                        {
                            Id = entity.Id,
                            Name = entity.Name,
                            Description = entity.Description,
                            ImageUrl = entity.ImageUrl

                        });
                    }
                    // calculate latest sync time, so that client can keep track
                    //var recordLastUpdated = new[] { cat.CreationTime, cat.LastModificationTime, cat.DeletionTime }.Max();

                    // ignore this because LastSyncedTimeStamp is set to Datetime.Now currently.
                    //if (output.LastSyncedTimeStamp < recordLastUpdated?.ToUnixTime())
                    //{
                    //    output.LastSyncedTimeStamp = recordLastUpdated.GetValueOrDefault().ToUnixTime();
                    //}
                }
                _detailLogService.Log($"Category: END get category from server to sync to slave, data -> " + JsonConvert.SerializeObject(output));
                return output;
            }
            catch (Exception ex)
            {
                _detailLogService.Log($"Category: END get category from server to sync to slave, error -> " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Send Queued Msg To Machines.
        /// </summary>
       
    }
}
