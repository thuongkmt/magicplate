
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using Abp.Linq.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using KonbiCloud.Plate.Exporting;
using KonbiCloud.Plate.Dtos;
using KonbiCloud.Dto;
using Abp.Application.Services.Dto;
using KonbiCloud.Authorization;
using Abp.Extensions;
using Abp.Authorization;
using Microsoft.EntityFrameworkCore;
using Abp.Domain.Uow;
using KonbiCloud.Machines;
using KonbiCloud.Common;
using KonbiCloud.Messaging;
using Konbini.Messages;
using Konbini.Messages.Enums;
using Newtonsoft.Json;

namespace KonbiCloud.Plate
{
    [AbpAllowAnonymous]
    public class PlateCategoriesAppService : KonbiCloudAppServiceBase, IPlateCategoriesAppService
    {
        private readonly IRepository<PlateCategory> _plateCategoryRepository;
        private readonly IRepository<Plate, Guid> _plateRepository;
        private readonly IPlateCategoriesExcelExporter _plateCategoriesExcelExporter;
        private readonly IRepository<Machine, Guid> _machineRepository;
        private readonly IDetailLogService _detailLogService;
        private readonly ISendMessageToMachineService _sendMessageToMachineService;

        public PlateCategoriesAppService(IRepository<PlateCategory> plateCategoryRepository,
            IPlateCategoriesExcelExporter plateCategoriesExcelExporter,
            IRepository<Plate, Guid> plateRepository,
            IRepository<Machine, Guid> machineRepository, IDetailLogService detailLog, ISendMessageToMachineService sendMessageToMachineService)
        {
            _plateCategoryRepository = plateCategoryRepository;
            _plateCategoriesExcelExporter = plateCategoriesExcelExporter;
            _plateRepository = plateRepository;
            _machineRepository = machineRepository;
            _detailLogService = detailLog;
            _sendMessageToMachineService = sendMessageToMachineService;
        }

        public async Task<PagedResultDto<GetPlateCategoryForView>> GetAll(GetAllPlateCategoriesInput input)
        {
            try
            {
                var filteredPlateCategories = _plateCategoryRepository.GetAllIncluding()
                            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.Name.Contains(input.Filter) || e.Desc.Contains(input.Filter))
                            .WhereIf(!string.IsNullOrWhiteSpace(input.NameFilter), e => e.Name.ToLower().Contains(input.NameFilter.ToLower().Trim()))
                            .Include(x => x.Plates);


                var query = (from o in filteredPlateCategories
                             select new GetPlateCategoryForView()
                             {
                                 PlateCategory = ObjectMapper.Map<PlateCategoryDto>(o)
                             });

                var totalCount = await query.CountAsync();

                var plateCategories = await query
                    .OrderBy(input.Sorting ?? "plateCategory.name")
                    .PageBy(input)
                    .ToListAsync();

                return new PagedResultDto<GetPlateCategoryForView>(
                    totalCount,
                    plateCategories
                );
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
                return new PagedResultDto<GetPlateCategoryForView>(0, new List<GetPlateCategoryForView>());
            }
        }

        [AbpAuthorize(AppPermissions.Pages_PlateCategories_Edit)]
        public async Task<GetPlateCategoryForEditOutput> GetPlateCategoryForEdit(EntityDto input)
        {
            try
            {
                var plateCategory = await _plateCategoryRepository.FirstOrDefaultAsync(input.Id);

                var output = new GetPlateCategoryForEditOutput { PlateCategory = ObjectMapper.Map<CreateOrEditPlateCategoryDto>(plateCategory) };

                return output;
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
                return new GetPlateCategoryForEditOutput();
            }
        }

        public async Task CreateOrEdit(CreateOrEditPlateCategoryDto input)
        {
            if (input.Id == null)
            {
                await Create(input);
            }
            else
            {
                await Update(input);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_PlateCategories_Create)]
        private async Task Create(CreateOrEditPlateCategoryDto input)
        {
            try
            {
                _detailLogService.Log($"PlateCategory: START create");
                var plateCategory = ObjectMapper.Map<PlateCategory>(input);
                plateCategory.Name = input.Name.Trim();

                if (AbpSession.TenantId != null)
                {
                    plateCategory.TenantId = (int?)AbpSession.TenantId;
                }
                await _plateCategoryRepository.InsertAsync(plateCategory);
                bool checkSendToRabbitMQ = await _sendMessageToMachineService.SendNotificationAsync(new KeyValueMessage() { Key = MessageKeys.PlateCategory });
                _detailLogService.Log($"PlateCategory: END create, then send changes to rabbitmq is -> " + checkSendToRabbitMQ);
            }
            catch (Exception ex)
            {
                _detailLogService.Log("PlateCategory: END create with error -> " + ex.ToString());
            }
        }

        [AbpAuthorize(AppPermissions.Pages_PlateCategories_Edit)]
        private async Task Update(CreateOrEditPlateCategoryDto input)
        {
            try
            {
                _detailLogService.Log($"PlateCategory: START update");
                var plateCategory = await _plateCategoryRepository.FirstOrDefaultAsync((int)input.Id);
                ObjectMapper.Map(input, plateCategory);
                plateCategory.Name = input.Name.Trim();
                bool checkSendToRabbitMQ = await _sendMessageToMachineService.SendNotificationAsync(new KeyValueMessage() { Key = MessageKeys.PlateCategory });
                _detailLogService.Log($"PlateCategory: END update, then send changes to rabbitmq is -> " + checkSendToRabbitMQ);
            }
            catch (Exception ex)
            {
                _detailLogService.Log("PlateCategory: END update with error -> " + ex.ToString());
            }
        }

        [AbpAuthorize(AppPermissions.Pages_PlateCategories_Delete)]
        public async Task Delete(EntityDto input)
        {
            //check category has plate
            //var totalPlate = await _plateRepository.GetAll().Where(e => e.PlateCategoryId == input.Id).CountAsync();
            //if(totalPlate > 0)
            //{
            //    return "Can not delete category has plate";

            //}
            try
            {
                _detailLogService.Log($"PlateCategory: START delete");
                await _plateCategoryRepository.DeleteAsync(input.Id);
                bool checkSendToRabbitMQ = await _sendMessageToMachineService.SendNotificationAsync(new KeyValueMessage() { Key = MessageKeys.PlateCategory });
                _detailLogService.Log($"PlateCategory: END delete, then send changes to rabbitmq is -> " + checkSendToRabbitMQ);
            }
            catch (Exception ex)
            {
                _detailLogService.Log("PlateCategory: END delete with error -> " + ex.ToString());
            }
            //return "Delete success !";
        }

        public async Task<FileDto> GetPlateCategoriesToExcel(GetAllPlateCategoriesForExcelInput input)
        {

            var filteredPlateCategories = _plateCategoryRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.Name.Contains(input.Filter) || e.Desc.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.NameFilter), e => e.Name.ToLower() == input.NameFilter.ToLower().Trim())
                        .WhereIf(!string.IsNullOrWhiteSpace(input.DescFilter), e => e.Desc.ToLower() == input.DescFilter.ToLower().Trim());


            var query = (from o in filteredPlateCategories
                         select new GetPlateCategoryForView()
                         {
                             PlateCategory = ObjectMapper.Map<PlateCategoryDto>(o)
                         });


            var plateCategoryListDtos = await query.ToListAsync();

            return _plateCategoriesExcelExporter.ExportToFile(plateCategoryListDtos);
        }
        /// <summary>
        /// Sync data between Server and Slave, To be Called from Slave side to this API
        /// </summary>
        /// <param name="machineSyncInput"></param>
        /// <returns></returns>
        [AbpAllowAnonymous]
        public async Task<EntitySyncOutputDto<PlateCategorySyncDto>> GetCategories(EntitySyncInputDto<Guid> machineSyncInput)
        {
            try
            {
                _detailLogService.Log($"PlateCategory: START get plateCategory from server to sync to slave, request -> {JsonConvert.SerializeObject(machineSyncInput)}");
                var unsyncedEntities = new List<PlateCategory>();
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MustHaveTenant, AbpDataFilters.MayHaveTenant, AbpDataFilters.SoftDelete))
                {
                    var machine = await _machineRepository.FirstOrDefaultAsync(x => x.Id == machineSyncInput.Id);
                    if (machine == null)
                    {
                        _detailLogService.Log($"PlateCategory: MachineId is {machineSyncInput.Id} does not exist");
                        return null;
                    }
                    else if (machine.IsDeleted)
                    {
                        _detailLogService.Log($"PlateCategory: MachineId is {machineSyncInput.Id} was deleted");
                        return null;
                    }

                    unsyncedEntities = await _plateCategoryRepository.GetAllListAsync(x => x.TenantId == machine.TenantId &&
                                                                        // get data with last synced timestamp
                                                                        (x.CreationTime > machineSyncInput.LastSynced ||
                                                                            x.LastModificationTime > machineSyncInput.LastSynced ||
                                                                            x.DeletionTime > machineSyncInput.LastSynced
                                                                        ));
                }
                var output = new EntitySyncOutputDto<PlateCategorySyncDto>();
                foreach (var entity in unsyncedEntities)
                {
                    if (entity.IsDeleted)
                    {
                        output.DeletionEntities.Add(new PlateCategorySyncDto()
                        {
                            Id = entity.Id
                        });
                    }
                    else
                    {
                        output.ModificationEntities.Add(new PlateCategorySyncDto()
                        {
                            Id = entity.Id,
                            Name = entity.Name,
                            Desc = entity.Desc
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
                _detailLogService.Log($"PlateCategory: END get plateCategory from server to sync to slave, data -> " + JsonConvert.SerializeObject(output));
                return output;
            }
            catch(Exception ex)
            {
                _detailLogService.Log($"PlateCategory: END get plateCategory from server to sync to slave, error -> " + ex.ToString());
                return null;
            }
        }
    }
}