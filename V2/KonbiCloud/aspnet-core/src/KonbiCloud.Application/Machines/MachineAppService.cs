﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Runtime.Caching;
using Abp.Runtime.Session;
using Abp.UI;
using Castle.Core.Logging;
using KonbiCloud.Common;
using KonbiCloud.Common.Dtos;
using KonbiCloud.Configuration;
using KonbiCloud.Machines.Dtos;
using KonbiCloud.MultiTenancy;
using KonbiCloud.Products;
using KonbiCloud.RedisCache;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using KonbiCloud.Authorization;

namespace KonbiCloud.Machines
{
    //[AbpAuthorize(KonbiCloud.Authorization.AppPermissions.Pages_Machines)]

    public class MachineAppService : KonbiCloudAppServiceBase, IMachineAppService
    {
        private readonly IRepository<Machine, Guid> _machineRepository;
        private readonly IRepository<Product, Guid> _productRepository;
        private readonly IRepository<Tenant> _tenantRepository;
        private readonly IPlannedInventoryAppService _plannedInventoryAppService;
        //private readonly IRepository<ItemInventory, Guid> _itemInventoryRepository;
        //private readonly IRepository<CurrentMachineLoadout, Guid> _currentLoadoutRepository;
        //private readonly IRepository<LoadoutItemStatus, Guid> _loadoutItemStatusRepository;
        //private readonly IRedisService _redisService;
        //private readonly IAzureRedisService _azureRedisService;
        private readonly IRepository<MachineErrorSolution> _machineErrorSolutionRepository;
        private readonly ICacheManager _cacheManager;

        private readonly ILogger _logger;
        //migrate from old cloud: private readonly IEventBus _serviceBus;

        public MachineAppService(IRepository<Machine, Guid> machineRepository,
            IRepository<Tenant> tenantRepository,
            IPlannedInventoryAppService plannedInventoryAppService,
            //IRepository<ItemInventory, Guid> itemInventoryRepository,
            //IRepository<CurrentMachineLoadout, Guid> currentLoadoutRepository,
            IRepository<Product, Guid> productRepository,
            //IRepository<LoadoutItemStatus, Guid> loadoutItemStatusRepository,
            IRepository<MachineErrorSolution> machineErrorSolutionRepository,
            ILogger logger,
            //IRedisService redisService,
            //IAzureRedisService azureRedisService,
            ICacheManager cacheManager)
        {
            _machineRepository = machineRepository;
            _tenantRepository = tenantRepository;
            _plannedInventoryAppService = plannedInventoryAppService;
            //_itemInventoryRepository = itemInventoryRepository;
            //_currentLoadoutRepository = currentLoadoutRepository;
            _productRepository = productRepository;
            //_loadoutItemStatusRepository = loadoutItemStatusRepository;
            _logger = logger;
            //_redisService = redisService;
            //_azureRedisService = azureRedisService;
            _cacheManager = cacheManager;
            //migrate from old cloud: _serviceBus = serviceBus;
            _machineErrorSolutionRepository = machineErrorSolutionRepository;
        }
        public async Task<PageResultListDto<MachineListDto>> GetAll(MachineInputListDto input)
        {
            var allMachines = _machineRepository.GetAll();
            int totalCount = await allMachines.CountAsync();

            if (string.IsNullOrEmpty(input.Sorting) || input.Sorting == "undefined")
            {
                input.Sorting = "CreationTime Desc";
            }
            var machines = await allMachines
                .OrderBy(input.Sorting)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();

            var results = new PageResultListDto<MachineListDto>(machines.MapTo<List<MachineListDto>>(), totalCount);

            var tenants = await _tenantRepository.GetAllListAsync();
            foreach (var item in results.Items)
            {
                item.TenantName = tenants.FirstOrDefault(x => x.Id == item.TenantId)?.Name ?? "";
            }
            return results;
        }

        public async Task<PageResultListDto<MachineErrorSolution>> GetMachineErrorSolutionAll(MachineInputListDto input)
        {
            var allMachineErrorSolutions = await _machineErrorSolutionRepository.GetAllListAsync();
            int totalCount = allMachineErrorSolutions.Count();

            var machineErrorSolutions = allMachineErrorSolutions
                .OrderBy(e => e.Id)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToList();

            var results = new PageResultListDto<MachineErrorSolution>(machineErrorSolutions, totalCount);
            return results;
        }

        public async Task<ListResultDto<VendingMachineStatusCache>> GetAllMachineStatus()
        {

            var machinesFromDb = await _machineRepository
                .GetAllListAsync();
            machinesFromDb.OrderBy(x => x.Name).ToList();

            //var machinesFromCache = _azureRedisService.Get<List<VendingMachineStatusDto>>(_redisStorageString);
            //var machineOnlineOfflineCache = _azureRedisService.Get<MachineListCache>(MachineListCache.GetCacheName());
            var results = new List<VendingMachineStatusCache>();

            for (int i = 0; i < machinesFromDb.Count; i++)
            {
                var machineFromDb = machinesFromDb[i];
                //var machineFromCache = machineOnlineOfflineCache.Items.FirstOrDefault(x => x.MachineId == machineFromDb.Id);
                //if (machineFromCache != null)
                //{
                //    machineFromDb.IsOffline = machineFromCache.IsOffline;
                //    results.Add(new VendingMachineStatusCache
                //    {
                //        MachineId = machineFromDb.Id.ToString(),
                //        Name = machineFromDb.Name,
                //        IsOffline = machineFromDb.IsOffline,
                //        LastModified = machineFromDb.LastModificationTime,
                //    });
                //}
                //else
                {
                    results.Add(new VendingMachineStatusCache
                    {
                        MachineId = machineFromDb.Id.ToString(),
                        Name = machineFromDb.Name,
                        IsOffline = true,
                        LastModified = machineFromDb.LastModificationTime,
                    });
                }
            }
            return new ListResultDto<VendingMachineStatusCache>
            {
                Items = results
            };
        }

        public async Task<ListResultDto<MachineComboboxDto>> GetMachinesForComboBox()
        {

            var result = _cacheManager
                .GetCache("GetMachinesForComboBox")
                .Get("-1", () =>
                 {
                     return _machineRepository
                         .GetAll()
                         .Select(x => new
                         {
                             x.Id,
                             x.Name
                         })
                         .Distinct()
                         .ToList();
                 });


            return new ListResultDto<MachineComboboxDto>
            {
                Items = result.Select(x => new MachineComboboxDto
                {
                    Id = x.Id.ToString(),
                    Name = x.Name
                }).OrderBy(x => x.Name)
                    .ToList()
            };
        }

        //// ReSharper disable once UnusedMember.Global
        //public async Task<CurrentMachineLoadoutDto> GetLoadoutItemStatus(EntityDto<Guid> machineID)
        //{
        //    var currentMachineLoadout = await _currentLoadoutRepository
        //        .GetAll()
        //        .Where(x => x.MachineId == machineID.Id.ToString())
        //        .Include(m => m.ItemsStatus)
        //        .SingleOrDefaultAsync();
        //    if (currentMachineLoadout != null)
        //    {
        //        var result = currentMachineLoadout.MapTo<CurrentMachineLoadoutDto>();

        //        var filteredInvalidItems = new List<LoadoutItemStatus>();
        //        foreach (var itemStt in currentMachineLoadout.ItemsStatus)
        //        {
        //            var locationInt = int.Parse(itemStt.ItemLocation);
        //            var mode100 = locationInt % 100;
        //            if (mode100 <= 10)
        //            {
        //                filteredInvalidItems.Add(itemStt);
        //            }
        //        }
        //        currentMachineLoadout.ItemsStatus = filteredInvalidItems;

        //        result.ItemsStatusDtos = currentMachineLoadout.ItemsStatus.MapTo<ICollection<LoadoutItemStatusDto>>();
        //        return result;
        //    }
        //    return null;
        //}

        //public async Task<object[]> GetInventoryReport(string machineId)
        //{
        //    try
        //    {
        //        var machineLoadouts = await _currentLoadoutRepository
        //            .GetAll().Where(x => string.IsNullOrEmpty(machineId) || x.MachineId == machineId)
        //            .Include(m => m.ItemsStatus)
        //            .ToListAsync();
        //        var itemStatus = machineLoadouts
        //            .SelectMany(ml => ml.ItemsStatus).Where(it => (it.LocationCodeNumber % 100) <= 10).ToList();
        //        var productSKUs = itemStatus.Where(i => !string.IsNullOrEmpty(i.ProductSKU)).Select(i => i.ProductSKU).Distinct().ToList();
        //        var products = await _productRepository.GetAllListAsync(p => productSKUs.Contains(p.SKU));
        //        ICollection<LoadoutItemStatusAPDto> result = itemStatus.Select(iss => GetLoadoutItemStatusAPDto(iss, products)).ToList();

        //        var output = result.GroupBy(iv => new { iv.MachineId, iv.MachineName }).Select(g => new
        //        {
        //            g.Key.MachineId,
        //            machineLogicalId = g.Key.MachineName,
        //            Details = g.Select(i => new
        //            {
        //                i.ItemLocation,
        //                i.Quantity,
        //                i.ProductSKU,
        //                i.AmoutExpired,
        //                i.ProductName,
        //                i.Time
        //            })
        //        }).ToArray();
        //        return output;
        //    }
        //    catch (Exception ex)
        //    {
        //       Logger.Error(ex.Message,ex);
        //        return null;
        //    }
        //}

        //private LoadoutItemStatusAPDto GetLoadoutItemStatusAPDto(LoadoutItemStatus input, List<Product> products)
        //{
        //    var prod = string.IsNullOrEmpty(input.ProductSKU) ? null : products.FirstOrDefault(p => p.SKU == input.ProductSKU);
        //    return new LoadoutItemStatusAPDto
        //    {
        //        MachineId = input.CurrentMachineLoadout.MachineId,
        //        MachineName = input.CurrentMachineLoadout.MachineLogicalId,
        //        ItemLocation = input.ItemLocation,
        //        Quantity = input.Quantity,
        //        ProductSKU = input.ProductSKU,
        //        ProductName = prod != null ? prod.Name : "",
        //        AmoutExpired = input.NumberExpiredItem,
        //        Time = input.CurrentMachineLoadout.Time
        //    };
        //}

        public async Task<MachineEditDetailDto> GetDetail(EntityDto<Guid> input)
        {
            var machines = _machineRepository.GetAll()
                .Where(e => e.Id == input.Id);

            var machine = await machines.FirstOrDefaultAsync();

            if (machine == null)
            {
                throw new UserFriendlyException("Could not found the machine, maybe it's deleted..");
            }

            var output = new MachineEditDetailDto()
            {
                Id = machine.Id.ToString(),
                CashlessTerminalId = machine.CashlessTerminalId,
                IsOffline = machine.IsOffline,
                Name = machine.Name,
                RegisteredAzureIoT = machine.RegisteredAzureIoT,
                StopSalesAfter = machine.StopSalesAfter,
                TemperatureStopSales = machine.TemperatureStopSales,
                TenantId = machine.TenantId
            };
            output.Settings = new MachineSettingsDto();
            output.Settings.TaxSettings = await GetMachinettingsAsync();

            return output;
        }
        private async Task<TaxSettingsDto> GetMachinettingsAsync()
        {
            var percentStr = await SettingManager.GetSettingValueAsync(AppSettings.MagicplateSettings.TaxSettings.TaxPercentage);
            double.TryParse(percentStr, out double percent);
            return
                 new TaxSettingsDto()
                 {
                     Name = await SettingManager.GetSettingValueAsync(AppSettings.MagicplateSettings.TaxSettings.TaxName),
                     Type = await SettingManager.GetSettingValueAsync(AppSettings.MagicplateSettings.TaxSettings.TaxType),
                     Percentage = percent,
                 }
            ;
        }
        public async Task Create(CreateMachineInput input)
        {
            Guid.TryParse(input.Id, out var @id);
            if (@id == null)
                throw new UserFriendlyException("ID is incorrect!");
            var oldMachine = await _machineRepository.FirstOrDefaultAsync(e => e.Id == @id);

            if (oldMachine != null)
                throw new UserFriendlyException("The machine with ID =" + input.Id + " is taken.");

            var tenantId = AbpSession.TenantId;
            try
            {
                var machine = new Machine
                {
                    Id = @id,
                    TenantId = tenantId,
                    Name = input.Name,
                    CashlessTerminalId = input.CashlessTerminalId
                };
                //migrate from old cloud: await _serviceBus.CreateSubscription(input.Id);
                //await _plannedInventoryAppService.InitPlannedInventory(machine);
                await _machineRepository.InsertAsync(machine);
                await CurrentUnitOfWork.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }


        }




        public async Task<Machine> Update(MachineEditDetailDto input)
        {
            //CheckUpdatePermission();

            var machine = await _machineRepository.FirstOrDefaultAsync(e => e.Id == Guid.Parse(input.Id));


            machine.Name = input.Name;
            machine.CashlessTerminalId = input.CashlessTerminalId;
            machine.IsOffline = input.IsOffline;
            machine.StopSalesAfter = input.StopSalesAfter;
            machine.TemperatureStopSales = input.TemperatureStopSales;
            //migrate from old cloud: await _serviceBus.CreateSubscription(input.Id.ToString());

            await _machineRepository.UpdateAsync(machine);
            return machine;
        }

        public async Task<bool> UpdateMachineToServer(MachineSync input)
        {
            //CheckUpdatePermission();

            var machine = await _machineRepository.FirstOrDefaultAsync(e => e.Id == new Guid(input.id));

            machine.Name = input.name;
            //machine.CashlessTerminalId = input.CashlessTerminalId;

            //migrate from old cloud: await _serviceBus.CreateSubscription(input.Id.ToString());

            await _machineRepository.UpdateAsync(machine);
            return true;
        }

        public async Task<SendRemoteCommandOutput> SendCommandToMachine(SendRemoteCommandInput input)
        {
            //CheckUpdatePermission();
            // implenent send command to machine ID with Commandam and Command Argument
            var result = await Task.Run(() =>
            {
                try
                {

                    var cmdMessage = $"redis_command_{(int)input.Command}::{input.CommandArgs}";
                    //_redisService.PublishCommandToMachine(input.MachineID, cmdMessage);
                    return new SendRemoteCommandOutput
                    {
                        IsSuccess = true,
                        Message = "Success send to machine"
                    };
                }
                catch (Exception ex)
                {
                    _logger.Error("Error when send command to machine", ex);
                    return new SendRemoteCommandOutput
                    {
                        IsSuccess = false,
                        Message = "Failed send to machine"
                    };
                }

            });

            return result;
        }

        public async Task Delete(EntityDto<Guid> input)
        {
            var machine = await _machineRepository.FirstOrDefaultAsync(e => e.Id == input.Id);
            await _machineRepository.DeleteAsync(machine);
        }


        private void AddMachineToCache(MachineListCache machineListCache, Machine machine)
        {
            //var exisitng=machineList

        }
    }
}
