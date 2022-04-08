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
using Abp.Authorization;
using KonbiCloud.SignalR;
using Microsoft.EntityFrameworkCore;
using KonbiCloud.CloudSync;
using Abp.Domain.Uow;
using Abp.UI;
using Abp.Configuration;
using KonbiCloud.Configuration;
using KonbiCloud.RFIDTable.Cache;
using Abp.Runtime.Caching;
using KonbiCloud.Common;

namespace KonbiCloud.Plate
{
    [AbpAuthorize(AppPermissions.Pages_Discs)]
    public class DiscsAppService : KonbiCloudAppServiceBase, IDiscsAppService
    {
        private readonly IRepository<Disc, Guid> _discRepository;
        private readonly IDiscsExcelExporter _discsExcelExporter;
        private readonly IRepository<Plate, Guid> _plateRepository;
        private readonly IMessageCommunicator messageCommunicator;
        private readonly IDishSyncService _dishSyncService;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly ICacheManager _cacheManager;
        private readonly IDetailLogService _detailLogService;
        private readonly ISaveFromCloudService _saveFromCloudService;

        public DiscsAppService(IRepository<Disc, Guid> discRepository, IDiscsExcelExporter discsExcelExporter,
                               IRepository<Plate, Guid> plateRepository, IMessageCommunicator messageCommunicator,
                               ICacheManager cacheManager,
                               IDishSyncService dishSyncService, IUnitOfWorkManager unitOfWorkManager, IDetailLogService detailLog, ISaveFromCloudService saveFromCloudService)
        {
            _cacheManager = cacheManager;
            _discRepository = discRepository;
            _discsExcelExporter = discsExcelExporter;
            _plateRepository = plateRepository;
            this.messageCommunicator = messageCommunicator;
            this._dishSyncService = dishSyncService;
            _unitOfWorkManager = unitOfWorkManager;
            _detailLogService = detailLog;
            _saveFromCloudService = saveFromCloudService;
        }

        public async Task<PagedResultDto<GetDiscForView>> GetAll(GetAllDiscsInput input)
        {
            try
            {
                // disabled SoftDelete to fix bug that Count() is not equal to Query.ToList().Count because of Parent entity is messed up with including
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
                {
                    var filteredDiscs = _discRepository.GetAllIncluding()
                                .Include(x => x.Plate)
                                .Where(el=> el.IsDeleted == false && el.Plate.IsDeleted ==false)
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
                   
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
                return new PagedResultDto<GetDiscForView>(0, new List<GetDiscForView>());
            }
        }

        public async Task<PagedResultDto<CheckInventoryDto>> GetAllDish()
        {
            try
            {
                var inventories = await _discRepository.GetAll().Select(x => new CheckInventoryDto
                                                            {
                                                                UID = x.Uid
                                                            }).ToListAsync();

                return new PagedResultDto<CheckInventoryDto>(
                    inventories.Count(),
                    inventories
                );
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
                return new PagedResultDto<CheckInventoryDto>(0, new List<CheckInventoryDto>());
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
            _detailLogService.Log($"Start create Dish: {DateTime.Now}");

            await Create(input);

            _detailLogService.Log($"Mid create Dish: {DateTime.Now}");

            await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();

            _detailLogService.Log($"End create Dish: {DateTime.Now}");
        }

        [AbpAuthorize(AppPermissions.Pages_Discs_Create)]
        private async Task Create(List<CreateOrEditDiscDto> input)
        {
            try
            {
                //TODO: need to find bulk insert DB
                var listDishes = new List<Disc>();
                foreach (var entity in input)
                {
                    var disc = ObjectMapper.Map<Disc>(entity);
                    listDishes.Add(disc);
                    if (AbpSession.TenantId != null)
                    {
                        disc.TenantId = AbpSession.TenantId;
                    }

                    await _discRepository.InsertAsync(disc);
                }

                await CurrentUnitOfWork.SaveChangesAsync();

                var allowSyncToServer = await SettingManager.GetSettingValueAsync<bool>(AppSettingNames.AllowPushDishToServer);
                if (allowSyncToServer)
                {
                    // Create new task sync to cloud and update disc to machine admin.
                    var taskSyncUpdate = Task.Run(() => {
                        try
                        {
                            var mId = Guid.Empty;
                            Guid.TryParse(SettingManager.GetSettingValueAsync(AppSettingNames.MachineId).Result, out mId);
                            if (mId == Guid.Empty)
                            {
                                Logger.Info($"Push dish: Machine Id is null");
                            }
                            else
                            {
                                var syncItem = new SyncedItemData<Disc>
                                {
                                    MachineId = mId,
                                    SyncedItems = listDishes
                                };
                                _detailLogService.Log($"Create dish: sync {listDishes.Count} to server");
                                var success = _dishSyncService.PushToServer(syncItem).Result;
                                if (success)
                                {
                                    var dishes = _discRepository.GetAllListAsync(x => !x.IsSynced).Result;
                                    foreach (var d in listDishes)
                                    {
                                        var dish = dishes.FirstOrDefault(x => x.Id == d.Id);
                                        if (dish != null)
                                        {
                                            dish.IsSynced = true;
                                            dish.SyncDate = DateTime.Now;
                                            _detailLogService.Log($"Create dish: update sync status {dish.Id}");
                                        }
                                    }
                                    CurrentUnitOfWork.SaveChangesAsync();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex.Message, ex);
                        }
                    });                    
                }
                await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Discs_Edit)]
        private async Task Update(CreateOrEditDiscDto input)
        {
            try
            {
                var disc = await _discRepository.FirstOrDefaultAsync(input.Id.Value);
                if (disc != null)
                {
                    ObjectMapper.Map(input, disc);
                }
                await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Discs_Delete)]
        public async Task Delete(DiscDto input)
        {
            try
            {
                var disc = await _discRepository.FirstOrDefaultAsync(input.Id);
                if (disc != null)
                {
                    //await _discRepository.DeleteAsync(disc);
                    //await CurrentUnitOfWork.SaveChangesAsync();
                    await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();
                    var allowSyncToServer = await SettingManager.GetSettingValueAsync<bool>(AppSettingNames.AllowPushDishToServer);
                    if (allowSyncToServer)
                    {
                        var mId = Guid.Empty;
                        Guid.TryParse(await SettingManager.GetSettingValueAsync(AppSettingNames.MachineId), out mId);
                        if (mId == Guid.Empty)
                        {
                            Logger.Info($"Push dish: Machine Id is null");
                        }
                        else
                        {
                            disc.IsDeleted = true;
                            var syncItem = new SyncedItemData<Disc>
                            {
                                MachineId = mId,
                                SyncedItems = new List<Disc>
                            {
                                disc
                            }
                            };
                            _detailLogService.Log($"Delete dish: sync {disc.Code} - {disc.Uid} to server");
                            await _dishSyncService.PushToServer(syncItem);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _detailLogService.Log($"Delete Dish error: {ex.Message}");
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Discs_Delete)]
        public async Task PostDeleteDishes(List<DiscDto> input)
        {
            try
            {
                var listDishes = new List<Disc>();
                foreach (var entity in input)
                {
                    var discItem = await _discRepository.FirstOrDefaultAsync(entity.Id);
                    discItem.IsDeleted = true;
                    _detailLogService.Log($"Delete dish: {discItem.Code} - {discItem.Uid} ");
                    listDishes.Add(discItem);
                }
                await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).ClearAsync();
                var allowSyncToServer = await SettingManager.GetSettingValueAsync<bool>(AppSettingNames.AllowPushDishToServer);
                if (allowSyncToServer)
                {
                    var mId = Guid.Empty;
                    Guid.TryParse(await SettingManager.GetSettingValueAsync(AppSettingNames.MachineId), out mId);
                    if (mId == Guid.Empty)
                    {
                        Logger.Info($"Push dish: Machine Id is null");
                    }
                    else
                    {
                        var syncItem = new SyncedItemData<Disc>
                        {
                            MachineId = mId,
                            SyncedItems = listDishes
                        };
                        await _dishSyncService.PushToServer(syncItem);
                    }
                }
            }
            catch (Exception ex)
            {
                _detailLogService.Log($"Delete Dishes error: {ex.Message}");
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
                            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => e.Name.Contains(input.Filter) || e.Code.Contains(input.Filter))
                            .WhereIf(true, x => x.Type == Enums.PlateType.Plate);

                var totalCount = await query.CountAsync();

                var plateList = await query.Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Code
                })
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
           // "{"type":"RFIDTable_DetectedDisc","data":{"Plates":[{"UID":"E0040150900F4B0A","UType":"0709"}]}}"

            var mess = "{\"type\":\"RFIDTable_DetectedDisc\",\"data\":{\"Plates\":[";
            int i = 0;
            foreach (var entity in input)
            {
                if (i == (input.Count - 1))
                {
                    mess += "{\"UType\":\"" + entity.Code + "\",\"UID\":\"" + entity.Uid + "\"}";
                }
                else
                {
                    mess += "{\"UType\":\"" + entity.Code + "\",\"UID\":\"" + entity.Uid + "\"},";
                }
                i++;
            }
            mess += "]}}";
            await messageCommunicator.SendRfidTableMessageToAllClient(new GeneralMessage { Message = mess });
            // await messageCommunicator.SendTestMessageToAllClient(new GeneralMessage { Message = "{\"type\":\"RFIDTable_DetectedDisc\",\"data\":[{\"code\":\"010203\",\"uid\":\"12345\"},{\"code\":\"010203\",\"uid\":\"98765\"}]}" });
        }

        [AbpAuthorize(AppPermissions.Pages_Discs_Sync)]
        public async Task<bool> SyncPlateDataFromServer()
        {
            //// Sync Plate Category.
            //bool result = await _saveFromCloudService.SyncPlateCategoryData();
            //if (!result)
            //{
            //    return false;
            //}

            //// Sync Plate.
            var result = await _saveFromCloudService.SyncPlateData();
            if (!result)
            {
                return false;
            }

            // Sync Inventory(Disc).
            result = await _saveFromCloudService.SyncDiscData();
            if (!result)
            {
                return false;
            }

            return result;
        }

        [AbpAllowAnonymous]
        public async Task UpdateSyncStatus(IEnumerable<DiscDto> dishes)
        {
            try
            {
                var existDishes = new List<Disc>();
                using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MustHaveTenant, AbpDataFilters.MayHaveTenant, AbpDataFilters.SoftDelete))
                {
                    existDishes = await _discRepository.GetAllListAsync();
                }

                foreach (var d in dishes)
                {
                    var existDish = existDishes.FirstOrDefault(x => x.Id == d.Id);
                    if (existDish != null)
                    {
                        existDish.IsSynced = true;
                        existDish.SyncDate = DateTime.Now;
                    }
                }
                await CurrentUnitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }
        }
    }
}