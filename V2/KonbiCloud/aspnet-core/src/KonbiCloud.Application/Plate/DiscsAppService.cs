using KonbiCloud.Plate;

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
using KonbiCloud.SignalR;
using Microsoft.EntityFrameworkCore;
using Abp.Domain.Uow;
using KonbiCloud.CloudSync;
using KonbiCloud.Machines;
using KonbiCloud.Common;
using KonbiCloud.Messaging;
using Konbini.Messages;
using Konbini.Messages.Enums;
using Newtonsoft.Json;

namespace KonbiCloud.Plate
{
    [AbpAllowAnonymous]
    public class DiscsAppService : KonbiCloudAppServiceBase, IDiscsAppService
    {
        private readonly IRepository<Disc, Guid> _discRepository;
        private readonly IDiscsExcelExporter _discsExcelExporter;
        private readonly IRepository<Plate, Guid> _plateRepository;
        private readonly IMessageCommunicator messageCommunicator;
        private readonly IRepository<DishMachineSyncStatus, Guid> _dishMachineSyncStatusRepository;
        private readonly IRepository<Machine, Guid> _machineRepository;
        private readonly IDetailLogService _detailLogService;
        private readonly ISendMessageToMachineService _sendMessageToMachineService;

        public DiscsAppService(IRepository<Disc, Guid> discRepository, IDiscsExcelExporter discsExcelExporter,
                               IRepository<Plate, Guid> plateRepository, IMessageCommunicator messageCommunicator,
                               IRepository<DishMachineSyncStatus, Guid> dishMachineSyncStatusRepository,
                               IRepository<Machine, Guid> machineRepository, IDetailLogService detailLog, ISendMessageToMachineService sendMessageToMachineService)
        {
            _discRepository = discRepository;
            _discsExcelExporter = discsExcelExporter;
            _plateRepository = plateRepository;
            this.messageCommunicator = messageCommunicator;
            _dishMachineSyncStatusRepository = dishMachineSyncStatusRepository;
            _machineRepository = machineRepository;
            _detailLogService = detailLog;
            _sendMessageToMachineService = sendMessageToMachineService;
        }

        public async Task<PagedResultDto<GetDiscForView>> GetAll(GetAllDiscsInput input)
        {
            try
            {
                var filteredDiscs = _discRepository.GetAllIncluding()
                             .Include(x => x.Plate)
                             .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.Uid.Contains(input.Filter) || e.Code.Contains(input.Filter)
                                                                                            || e.Plate.Name.Contains(input.Filter))
                             .WhereIf(!string.IsNullOrWhiteSpace(input.UidFilter), e => e.Uid.ToLower().Contains(input.UidFilter.ToLower().Trim()))
                             .WhereIf(!string.IsNullOrWhiteSpace(input.CodeFilter), e => e.Code.ToLower().Contains(input.CodeFilter.ToLower().Trim()))
                             .WhereIf(!string.IsNullOrWhiteSpace(input.PlateIdFilter), e => e.PlateId == new Guid(input.PlateIdFilter))
                             .WhereIf(!string.IsNullOrWhiteSpace(input.PlateNameFilter), e => e.Plate.Name.Contains(input.PlateNameFilter));

                var totalCount = await filteredDiscs.CountAsync();

                var discs = await filteredDiscs
                    .OrderBy(input.Sorting ?? "Plate.Name asc")
                    .PageBy(input)
                    .Select(x => new GetDiscForView
                    {
                        Disc = ObjectMapper.Map<DiscDto>(x),
                        PlateName = x.Plate == null ? "" : x.Plate.Name
                    })
                    .ToListAsync();

                return new PagedResultDto<GetDiscForView>(
                    totalCount,
                    discs
                );
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
                return new PagedResultDto<GetDiscForView>(0, new List<GetDiscForView>());
            }

        }

        [AbpAuthorize(AppPermissions.Pages_Discs_Edit)]
        public async Task<GetDiscForEditOutput> GetDiscForEdit(DiscDto input)
        {
            try
            {
                var disc = await _discRepository.FirstOrDefaultAsync(input.Id);

                var output = new GetDiscForEditOutput { Disc = ObjectMapper.Map<CreateOrEditDiscDto>(disc) };

                if (output.Disc.PlateId != null)
                {
                    var plate = await _plateRepository.FirstOrDefaultAsync((Guid)output.Disc.PlateId);
                    output.PlateName = plate.Name;
                }

                return output;
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
                return new GetDiscForEditOutput();
            }
        }

        public async Task CreateOrEdit(List<CreateOrEditDiscDto> input)
        {
            await Create(input);
        }

        [AbpAuthorize(AppPermissions.Pages_Discs_Create)]
        private async Task Create(List<CreateOrEditDiscDto> input)
        {
            _detailLogService.Log($"Disc: START create");
            try
            {
                //TODO: need to find bulk insert DB
                foreach (var entity in input)
                {
                    var disc = ObjectMapper.Map<Disc>(entity);
                    if (AbpSession.TenantId != null)
                    {
                        disc.TenantId = (int?)AbpSession.TenantId;
                    }
                    await _discRepository.InsertAsync(disc);
                }
                // Send Queued Msg To Machines.
                bool checkSendToRabbitMQ = await _sendMessageToMachineService.SendNotificationAsync(new KeyValueMessage() { Key = MessageKeys.Inventory });
                _detailLogService.Log($"Disc: END create, then send changes to rabbitmq is -> " + checkSendToRabbitMQ);
            }
            catch (Exception ex)
            {
                _detailLogService.Log("Disc: END create with error -> " + ex.ToString());
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Discs_Edit)]
        private async Task Update(CreateOrEditDiscDto input)
        {
            try
            {
                var disc = await _discRepository.FirstOrDefaultAsync(input.Id.Value);
                ObjectMapper.Map(input, disc);
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Discs_Delete)]
        public async Task Delete(DiscDto input)
        {
            _detailLogService.Log($"Disc: START delete");
            try
            {
                await _discRepository.DeleteAsync(input.Id);
                bool checkSendToRabbitMQ = await _sendMessageToMachineService.SendNotificationAsync(new KeyValueMessage(){Key = MessageKeys.Inventory});
                _detailLogService.Log($"Disc: END delete, then send changes to rabbitmq is -> " + checkSendToRabbitMQ);
            }
            catch (Exception ex)
            {
                _detailLogService.Log("Disc: END delete with error -> " + ex.ToString());
            }
        }

        public async Task<FileDto> GetDiscsToExcel(GetAllDiscsForExcelInput input)
        {

            var filteredDiscs = _discRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.Uid.Contains(input.Filter) || e.Code.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.UidFilter), e => e.Uid.ToLower() == input.UidFilter.ToLower().Trim())
                        .WhereIf(!string.IsNullOrWhiteSpace(input.CodeFilter), e => e.Code.ToLower() == input.CodeFilter.ToLower().Trim());


            var query = (from o in filteredDiscs
                         join o1 in _plateRepository.GetAll() on o.PlateId equals o1.Id into j1
                         from s1 in j1.DefaultIfEmpty()

                         select new GetDiscForView()
                         {
                             Disc = ObjectMapper.Map<DiscDto>(o),
                             PlateName = s1 == null ? "" : s1.Name
                         })
                        .WhereIf(!string.IsNullOrWhiteSpace(input.PlateNameFilter), e => e.PlateName.ToLower() == input.PlateNameFilter.ToLower().Trim());


            var discListDtos = await query.ToListAsync();

            return _discsExcelExporter.ExportToFile(discListDtos);
        }


        [AbpAuthorize(AppPermissions.Pages_Discs)]
        public async Task<PagedResultDto<PlateLookupTableDto>> GetAllPlateForLookupTable(GetAllForLookupTableInput input)
        {
            try
            {
                var query = _plateRepository.GetAll()
                            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => e.Name.Contains(input.Filter))
                            .WhereIf(true, x => x.Type == Enums.PlateType.Plate);

                var totalCount = await query.CountAsync();

                var plateList = await query
                    .PageBy(input)
                    .ToListAsync();

                var lookupTableDtoList = new List<PlateLookupTableDto>();
                foreach (var plate in plateList)
                {
                    lookupTableDtoList.Add(new PlateLookupTableDto
                    {
                        Id = plate.Id.ToString(),
                        DisplayName = plate.Name,
                        Code = plate.Code
                    });
                }

                return new PagedResultDto<PlateLookupTableDto>(
                    totalCount,
                    lookupTableDtoList
                );
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
                return new PagedResultDto<PlateLookupTableDto>(0, new List<PlateLookupTableDto>());
            }
        }


        public async Task TestSendSignalRMessage(List<CreateOrEditDiscDto> input)
        {
            var mess = "{\"type\":\"RFIDTable_DetectedDisc\",\"data\":[";
            int i = 0;
            foreach (var entity in input)
            {
                if (i == (input.Count - 1))
                {
                    mess += "{\"PlateCode\":\"" + entity.Code + "\",\"DiscUID\":\"" + entity.Uid + "\"}";
                }
                else
                {
                    mess += "{\"PlateCode\":\"" + entity.Code + "\",\"DiscUID\":\"" + entity.Uid + "\"},";
                }
                i++;
            }
            mess += "]}";
            await messageCommunicator.SendTestMessageToAllClient(new GeneralMessage { Message = mess });
            // await messageCommunicator.SendTestMessageToAllClient(new GeneralMessage { Message = "{\"type\":\"RFIDTable_DetectedDisc\",\"data\":[{\"code\":\"010203\",\"uid\":\"12345\"},{\"code\":\"010203\",\"uid\":\"98765\"}]}" });
        }

        //Reveive Dish from Machine
        [AbpAllowAnonymous]
        public async Task<bool> SyncDishData(SyncedItemData<Disc> syncData)
        {
            try
            {
                var existDishes = new List<Disc>();
                var plates = new List<Plate>();
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MustHaveTenant, AbpDataFilters.MayHaveTenant, AbpDataFilters.SoftDelete))
                {
                    var machine = await _machineRepository.FirstOrDefaultAsync(x => x.Id == syncData.MachineId);
                    if (machine == null)
                    {
                        Logger.Error($"Sync Dish: MachineId: {syncData.MachineId} does not exist");
                        return false;
                    }

                    existDishes = await _discRepository.GetAllListAsync(x => x.TenantId == machine.TenantId);
                    plates = await _plateRepository.GetAllListAsync(x => x.TenantId == machine.TenantId);

                    var syncedList = new SyncedItemData<Guid> { MachineId = syncData.MachineId, SyncedItems = new List<Guid>() };
                    foreach (var d in syncData.SyncedItems)
                    {
                        if (d.PlateId != Guid.Empty)
                        {
                            var plate = plates.FirstOrDefault(x => x.Id == d.PlateId);
                            if (plate == null)
                                continue;
                        }
                        syncedList.SyncedItems.Add(d.Id);
                        if (d.Id != Guid.Empty)
                        {
                            var dish = await _discRepository.FirstOrDefaultAsync(x => x.Id.Equals(d.Id));
                            if (dish == null || dish.TenantId != machine.TenantId)
                            {
                                d.TenantId = AbpSession.TenantId;
                                await _discRepository.InsertAsync(d);
                            }
                            else
                            {
                                dish.IsDeleted = d.IsDeleted;
                                dish.TenantId = AbpSession.TenantId;
                                dish.Uid = d.Uid;
                                dish.Code = d.Code;
                                dish.PlateId = d.PlateId;
                            }
                        }
                        else
                        {
                            d.TenantId = AbpSession.TenantId;
                            await _discRepository.InsertAsync(d);
                        }
                    }

                    await CurrentUnitOfWork.SaveChangesAsync();
                    await UpdateSyncStatus(syncedList);

                    // Send messageto machine service for sync inventory.
                    await _sendMessageToMachineService.SendNotificationAsync(new KeyValueMessage() { Key = MessageKeys.Inventory });

                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return false;
            }
        }

        /// <summary>
        /// Sync data between Server and Slave, To be Called from Slave side to this API
        /// </summary>
        /// <param name="machineSyncInput"></param>
        /// <returns></returns>
        [AbpAllowAnonymous]
        public async Task<EntitySyncOutputDto<DiscSyncDto>> GetDishes(EntitySyncInputDto<Guid> machineSyncInput)
        {
            try
            {
                _detailLogService.Log($"Discs: START get discs from server to sync to slave, request -> {JsonConvert.SerializeObject(machineSyncInput)}");
                var unsyncedEntities = new List<Disc>();
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MustHaveTenant, AbpDataFilters.MayHaveTenant, AbpDataFilters.SoftDelete))
                {
                    var machine = await _machineRepository.FirstOrDefaultAsync(x => x.Id == machineSyncInput.Id);
                    if (machine == null)
                    {
                        _detailLogService.Log($"Discs: MachineId is {machineSyncInput.Id} does not exist");
                        return null;
                    }
                    else if (machine.IsDeleted)
                    {
                        _detailLogService.Log($"Discs: MachineId is {machineSyncInput.Id} was deleted");
                        return null;
                    }

                    unsyncedEntities = await _discRepository.GetAllListAsync(x => x.TenantId == machine.TenantId &&
                                                                   // get data with last synced timestamp
                                                                   (x.CreationTime > machineSyncInput.LastSynced ||
                                                                       x.LastModificationTime > machineSyncInput.LastSynced ||
                                                                       x.DeletionTime > machineSyncInput.LastSynced
                                                                   ));
                }
                var output = new EntitySyncOutputDto<DiscSyncDto>();
                foreach (var entity in unsyncedEntities)
                {
                    if (entity.IsDeleted)
                    {
                        output.DeletionEntities.Add(new DiscSyncDto()
                        {
                            Id = entity.Id
                        });
                    }
                    else
                    {
                        output.ModificationEntities.Add(new DiscSyncDto()
                        {
                            Id = entity.Id,
                            Code = entity.Code,
                            Uid = entity.Uid,
                           PlateId = entity.PlateId
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
                _detailLogService.Log($"Discs: END get discs from server to sync to slave, data -> " + JsonConvert.SerializeObject(output));
                return output;
            }
            catch (Exception ex)
            {
                _detailLogService.Log($"Discs: END get discs from server to sync to slave, error -> " + ex.ToString());
                return null;
            }
        }
        [AbpAllowAnonymous]
        public async Task<bool> UpdateSyncStatus(SyncedItemData<Guid> syncData)
        {
            try
            {
                _detailLogService.Log($"Discs: START updateStatus from server to sync to slave");
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MustHaveTenant, AbpDataFilters.MayHaveTenant, AbpDataFilters.SoftDelete))
                {
                    var machine = await _machineRepository.FirstOrDefaultAsync(x => x.Id == syncData.MachineId);
                    if (machine == null)
                    {
                        _detailLogService.Log($"Discs: MachineId is {syncData.MachineId} does not exist");
                        return false;
                    }
                    else if(machine.IsDeleted)
                    {
                        _detailLogService.Log($"Discs: MachineId is {syncData.MachineId} was deleted");
                        return false;
                    }

                    var allPMs = await _dishMachineSyncStatusRepository.GetAllListAsync(x => x.MachineId == syncData.MachineId && x.TenantId == machine.TenantId);
                    foreach (var item in syncData.SyncedItems)
                    {
                        var pm = allPMs.FirstOrDefault(x => x.DiscId == item);
                        if (pm == null)
                        {
                            await _dishMachineSyncStatusRepository.InsertAsync(
                                new DishMachineSyncStatus
                                {
                                    Id = Guid.NewGuid(),
                                    DiscId = item,
                                    MachineId = syncData.MachineId,
                                    IsSynced = true,
                                    SyncDate = DateTime.Now
                                });
                            continue;
                        }
                        pm.SyncDate = DateTime.Now;
                    }
                }
                await CurrentUnitOfWork.SaveChangesAsync();
                _detailLogService.Log($"Discs: END updateStatus");
                return true;
            }
            catch (Exception ex)
            {
                _detailLogService.Log($"Discs: END updateStatus with error -> " + ex.ToString());
                return false;
            }
        }
    }
}