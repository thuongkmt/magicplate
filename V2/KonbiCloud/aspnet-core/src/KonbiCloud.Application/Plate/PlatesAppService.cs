using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using KonbiCloud.Authorization;
using KonbiCloud.CloudSync;
using KonbiCloud.Common;
using KonbiCloud.Configuration;
using KonbiCloud.Dto;
using KonbiCloud.Machines;
using KonbiCloud.Plate.Dtos;
using KonbiCloud.Plate.Exporting;
using KonbiCloud.PlateMenus.Dtos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using KonbiCloud.Messaging;
using Konbini.Messages;
using Konbini.Messages.Enums;
using KonbiCloud.Enums;
using Newtonsoft.Json;

namespace KonbiCloud.Plate
{
    [AbpAllowAnonymous]
    public class PlatesAppService : KonbiCloudAppServiceBase, IPlatesAppService
    {
        private readonly IRepository<Plate, Guid> _plateRepository;
        private readonly IPlatesExcelExporter _platesExcelExporter;
        private readonly IRepository<PlateCategory, int> _plateCategoryRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IRepository<Disc, Guid> _discRepository;
        private readonly IConfigurationRoot _appConfiguration;
        private readonly string serverUrl;
        private const string noImage = "assets/common/images";
        private readonly IRepository<PlateMachineSyncStatus, Guid> _plateMachineSyncStatusRepository;
        private readonly IRepository<Machine, Guid> _machineRepository;
        private readonly IDetailLogService _detailLogService;
        private readonly ISendMessageToMachineService _sendMessageToMachineService;

        public PlatesAppService(IRepository<Plate, Guid> plateRepository, IRepository<Disc, Guid> discRepository,
            IPlatesExcelExporter platesExcelExporter, IRepository<PlateCategory, int> plateCategoryRepository,
            IFileStorageService fileStorageService, IHostingEnvironment env,
            IRepository<PlateMachineSyncStatus, Guid> plateMachineSyncStatusRepository,
            IRepository<Machine, Guid> machineRepository, IDetailLogService detailLog, ISendMessageToMachineService sendMessageToMachineService)
        {
            _plateRepository = plateRepository;
            _platesExcelExporter = platesExcelExporter;
            _plateCategoryRepository = plateCategoryRepository;
            _fileStorageService = fileStorageService;
            _discRepository = discRepository;
            _appConfiguration = env.GetAppConfiguration();
            serverUrl = _appConfiguration["App:ServerRootAddress"] ?? "";

            _plateMachineSyncStatusRepository = plateMachineSyncStatusRepository;
            _machineRepository = machineRepository;
            _detailLogService = detailLog;
            _sendMessageToMachineService = sendMessageToMachineService;
        }

        public async Task<PagedResultDto<GetPlateForView>> GetAll(GetAllPlatesInput input)
        {
            try
            {
                var filteredPlates = _plateRepository.GetAllIncluding()
                                .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.Name.Contains(input.Filter) || e.ImageUrl.Contains(input.Filter) || e.Desc.Contains(input.Filter) || e.Code.Contains(input.Filter) || e.Color.Contains(input.Filter))
                                .WhereIf(!string.IsNullOrWhiteSpace(input.NameFilter), e => e.Name.ToLower().Contains(input.NameFilter.ToLower().Trim()))
                                .WhereIf(!string.IsNullOrWhiteSpace(input.ImageUrlFilter), e => e.ImageUrl.ToLower().Contains(input.ImageUrlFilter.ToLower().Trim()))
                                .WhereIf(!string.IsNullOrWhiteSpace(input.DescFilter), e => e.Desc.ToLower().Contains(input.DescFilter.ToLower().Trim()))
                                .WhereIf(!string.IsNullOrWhiteSpace(input.CodeFilter), e => e.Code.ToLower().Contains(input.CodeFilter.ToLower().Trim()))
                                .WhereIf(input.MinAvaiableFilter != null, e => e.Avaiable >= input.MinAvaiableFilter)
                                .WhereIf(input.MaxAvaiableFilter != null, e => e.Avaiable <= input.MaxAvaiableFilter)
                                .WhereIf(!string.IsNullOrWhiteSpace(input.ColorFilter), e => e.Color.ToLower().Contains(input.ColorFilter.ToLower().Trim()))
                                .WhereIf(true, e => input.IsPlate == true ? e.Type == Enums.PlateType.Plate : e.Type == Enums.PlateType.Tray)
                                .Include(x => x.Discs);

                var query = (from o in filteredPlates
                             join o1 in _plateCategoryRepository.GetAll() on o.PlateCategoryId equals o1.Id into j1
                             from s1 in j1.DefaultIfEmpty()

                             select new GetPlateForView()
                             {
                                 Plate = ObjectMapper.Map<PlateDto>(o),
                                 PlateCategoryName = s1 == null ? "" : s1.Name
                             })
                            .WhereIf(!string.IsNullOrWhiteSpace(input.PlateCategoryNameFilter), e => e.PlateCategoryName.ToLower() == input.PlateCategoryNameFilter.ToLower().Trim());

                var totalCount = await query.CountAsync();

                var plates = await query
                    .OrderBy(input.Sorting ?? "plate.name")
                    .PageBy(input)
                    .ToListAsync();

                var pathImage = Path.Combine(serverUrl, Const.ImageFolder, _appConfiguration[AppSettingNames.PlateImageFolder]);
                foreach (var p in plates)
                {
                    if (p.Plate != null && p.Plate.ImageUrl != null)
                    {
                        if (p.Plate.ImageUrl.Contains(noImage)) continue;

                        var arrImage = p.Plate.ImageUrl.Split("|");
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
                        p.Plate.ImageUrl = images;
                    }
                }
                return new PagedResultDto<GetPlateForView>(
                    totalCount,
                    plates
                );
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
                return new PagedResultDto<GetPlateForView>(0, new List<GetPlateForView>());
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Plates_Edit)]
        public async Task<GetPlateForEditOutput> GetPlateForEdit(EntityDto<Guid> input)
        {
            try
            {
                var plate = await _plateRepository.FirstOrDefaultAsync(input.Id);

                var output = new GetPlateForEditOutput { Plate = ObjectMapper.Map<CreateOrEditPlateDto>(plate) };

                if (output.Plate.PlateCategoryId != null)
                {
                    var plateCategory = await _plateCategoryRepository.FirstOrDefaultAsync((int)output.Plate.PlateCategoryId);
                    if (plateCategory != null)
                    {
                        output.PlateCategoryName = plateCategory.Name;
                    }
                }

                var dishes = _discRepository.GetAll()
                            .Where(e => e.PlateId == output.Plate.Id);
                output.Plate.Avaiable = await dishes.CountAsync();

                if (output.Plate.ImageUrl != null)
                {
                    var pathImage = Path.Combine(serverUrl, Const.ImageFolder, _appConfiguration[AppSettingNames.PlateImageFolder]);

                    var arrImage = output.Plate.ImageUrl.Split("|");
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
                    output.Plate.ImageUrl = images;
                }

                return output;
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
                return new GetPlateForEditOutput();
            }
        }

        public async Task<PlateMessage> CreateOrEdit(CreateOrEditPlateDto input)
        {
            try
            {
                _detailLogService.Log($"Plate: START check logic");
                if (input.Id == null)
                {
                    var p = await _plateRepository.FirstOrDefaultAsync(x => x.Code.ToLower().Equals(input.Code.Trim().ToLower()));
                    if (p != null)
                    {
                        return new PlateMessage { Message = $"Plate Code {input.Code} already existed, please use another code." };
                    }

                    p = await _plateRepository.FirstOrDefaultAsync(x => x.Name.ToLower().Equals(input.Name.Trim().ToLower()));
                    if (p != null)
                    {
                        return new PlateMessage { Message = $"Plate Name {input.Name} already existed, please use another code." };
                    }
                }
                else
                {
                    var p = await _plateRepository.FirstOrDefaultAsync(x => x.Id != input.Id.Value && x.Code.ToLower().Equals(input.Code.Trim().ToLower()));
                    if (p != null)
                    {
                        return new PlateMessage { Message = $"Plate Code {input.Code} already existed, please use another code." };
                    }

                    p = await _plateRepository.FirstOrDefaultAsync(x => x.Id != input.Id.Value && x.Name.ToLower().Equals(input.Name.Trim().ToLower()));
                    if (p != null)
                    {
                        return new PlateMessage { Message = $"Plate Name {input.Name} already existed, please use another code." };
                    }
                }

                if (input.Id == null)
                {
                    await Create(input);
                }
                else
                {
                    await Update(input);
                }

                bool checkSendToRabbitMQ = await _sendMessageToMachineService.SendNotificationAsync(new KeyValueMessage() { Key = MessageKeys.Plate });
                _detailLogService.Log($"Plate: END create, then send changes to rabbitmq is -> " + checkSendToRabbitMQ);

                return new PlateMessage { Message = null };
            }
            catch (Exception ex)
            {
                _detailLogService.Log("Plate: END create with error -> " + ex.ToString());
                return new PlateMessage { Message = $"An error occurred, please try again." };
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Plates_Create)]
        private async Task Create(CreateOrEditPlateDto input)
        {
            try
            {
                _detailLogService.Log($"Plate: START create");
                var plate = ObjectMapper.Map<Plate>(input);
                plate.Name = input.Name.Trim();

                if (AbpSession.TenantId != null)
                {
                    plate.TenantId = AbpSession.TenantId;
                }

                await _plateRepository.InsertAsync(plate);
                await CurrentUnitOfWork.SaveChangesAsync();
                _detailLogService.Log($"Plate: END create");
            }
            catch(Exception ex)
            {
                _detailLogService.Log($"Plate: END create with error -> " + ex.ToString());
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Plates_Edit)]
        private async Task Update(CreateOrEditPlateDto input)
        {
            try
            {
                _detailLogService.Log($"Plate: START update");
                if (input.ImageUrl != null)
                {
                    var newPlateImage = "";
                    var arrImage = input.ImageUrl.Split("|");
                    for (int index = 0; index < arrImage.Length; index++)
                    {
                        if (index == arrImage.Length - 1)
                        {
                            newPlateImage += Path.GetFileName(arrImage[index]);
                        }
                        else
                        {
                            newPlateImage += (Path.GetFileName(arrImage[index]) + '|');
                        }
                    }
                    input.ImageUrl = newPlateImage;
                }

                var plate = await _plateRepository.FirstOrDefaultAsync((Guid)input.Id);
                plate.Name = input.Name.Trim();
                ObjectMapper.Map(input, plate);
                _detailLogService.Log($"Plate: END update");
            }
            catch(Exception ex)
            {
                _detailLogService.Log("Plate: END update with error -> " + ex.ToString());
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Plates_Delete)]
        public async Task Delete(EntityDto<Guid> input)
        {
            try
            {
                _detailLogService.Log($"Plate: START delete");
                await _plateRepository.DeleteAsync(input.Id);
                bool checkSendToRabbitMQ = await _sendMessageToMachineService.SendNotificationAsync(new KeyValueMessage() { Key = MessageKeys.Plate });
                _detailLogService.Log($"Plate: END delete, then send changes to rabbitmq is -> " + checkSendToRabbitMQ);
            }
            catch (Exception ex)
            {
                _detailLogService.Log("Plate: END delete with error -> " + ex.ToString());
            }
        }

        public async Task<FileDto> GetPlatesToExcel(GetAllPlatesForExcelInput input)
        {
            var filteredPlates = _plateRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.Name.Contains(input.Filter) || e.ImageUrl.Contains(input.Filter) || e.Desc.Contains(input.Filter) || e.Code.Contains(input.Filter) || e.Color.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.NameFilter), e => e.Name.ToLower().Contains(input.NameFilter.ToLower().Trim()))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.ImageUrlFilter), e => e.ImageUrl.ToLower().Contains(input.ImageUrlFilter.ToLower().Trim()))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.DescFilter), e => e.Desc.ToLower().Contains(input.DescFilter.ToLower().Trim()))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.CodeFilter), e => e.Code.ToLower().Contains(input.CodeFilter.ToLower().Trim()))
                        .WhereIf(input.MinAvaiableFilter != null, e => e.Avaiable >= input.MinAvaiableFilter)
                        .WhereIf(input.MaxAvaiableFilter != null, e => e.Avaiable <= input.MaxAvaiableFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.ColorFilter), e => e.Color.ToLower().Contains(input.ColorFilter.ToLower().Trim()))
                        .WhereIf(true, e => input.IsPlate == true ? e.Type == Enums.PlateType.Plate : e.Type == Enums.PlateType.Tray);

            var query = (from o in filteredPlates
                         join o1 in _plateCategoryRepository.GetAll() on o.PlateCategoryId equals o1.Id into j1
                         from s1 in j1.DefaultIfEmpty()

                         select new GetPlateForView()
                         {
                             Plate = ObjectMapper.Map<PlateDto>(o),
                             PlateCategoryName = s1 == null ? "" : s1.Name
                         })
                        .WhereIf(!string.IsNullOrWhiteSpace(input.PlateCategoryNameFilter), e => e.PlateCategoryName.ToLower() == input.PlateCategoryNameFilter.ToLower().Trim());


            var plateListDtos = await query.ToListAsync();
            //foreach (var p in plateListDtos)
            //{
            //    if (p.Plate == null) continue;
            //    if (p.Plate.ImageUrl.Contains(oldImageUrl) || p.Plate.ImageUrl.Contains(noImage)) continue;
            //    p.Plate.ImageUrl = Path.Combine(serverUrl, p.Plate.ImageUrl);
            //}
            return _platesExcelExporter.ExportToFile(plateListDtos);
        }

        [AbpAuthorize(AppPermissions.Pages_Plates)]
        public async Task<PagedResultDto<PlateCategoryLookupTableDto>> GetAllPlateCategoryForLookupTable(GetAllForLookupTableInput input)
        {
            try
            {
                var query = _plateCategoryRepository.GetAll().WhereIf(
                   !string.IsNullOrWhiteSpace(input.Filter),
                  e => e.Name.Contains(input.Filter)
               );

                var totalCount = await query.CountAsync();

                var plateCategoryList = await query
                    .PageBy(input)
                    .ToListAsync();

                var lookupTableDtoList = new List<PlateCategoryLookupTableDto>();
                foreach (var plateCategory in plateCategoryList)
                {
                    lookupTableDtoList.Add(new PlateCategoryLookupTableDto
                    {
                        Id = plateCategory.Id,
                        DisplayName = plateCategory.Name
                    });
                }

                return new PagedResultDto<PlateCategoryLookupTableDto>(
                    totalCount,
                    lookupTableDtoList
                );
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
                return new PagedResultDto<PlateCategoryLookupTableDto>(0, new List<PlateCategoryLookupTableDto>());
            }
        }

        public async Task<ImportResult> ImportPlate(List<CreateOrEditPlateDto> input)
        {
            try
            {
                var listError = new List<string>();
                var listSuccess = new List<CreateOrEditPlateDto>();
                var plates = await _plateRepository.GetAllListAsync(x => input.Any(y => y.Code.Equals(x.Code)));
                for (int i = 0; i < input.Count; i++)
                {
                    var inputPlate = input[i];
                    if (!String.IsNullOrEmpty(inputPlate.Code) && !String.IsNullOrEmpty(inputPlate.Name))
                    {
                        var type = inputPlate.IsPlate ? PlateType.Plate : PlateType.Tray;
                        var p = plates.FirstOrDefault(x => x.Code.Equals(inputPlate.Code.Trim()) && x.Name.Equals(inputPlate.Name.Trim()));
                        var plate = await FormatImportPlate(inputPlate, type, listError, i);
                        var isError = await VerifyImportPlate(inputPlate, type, listError, i);

                        if (isError)
                        {
                            continue;
                        }

                        if (p == null)
                        {
                            await _plateRepository.InsertAsync(plate);
                            listSuccess.Add(inputPlate);
                        }
                        else
                        {
                            p.Name = plate.Name;
                            p.ImageUrl = plate.ImageUrl;
                            p.Desc = plate.Desc;
                            p.Code = plate.Code;
                            p.Color = plate.Color;
                            p.PlateCategoryId = plate.PlateCategoryId;
                            await _plateRepository.UpdateAsync(p);
                            listSuccess.Add(inputPlate);
                        }
                    }
                    else
                    {
                        listError.Add("Row " + (i + 2) + "- Plate code or Plate Name is Empty.");
                    }
                }
                if (listSuccess.Any())
                {
                    await CurrentUnitOfWork.SaveChangesAsync();
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
                    result.ErrorList = string.Join(", ", listError.Select(x => x.ToString()).ToArray());
                }
                await _sendMessageToMachineService.SendNotificationAsync(new KeyValueMessage() { Key = MessageKeys.Plate });
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error("Import Plate Error", ex);
                return new ImportResult
                {
                    ErrorCount = 0,
                    SuccessCount = 0,
                    ErrorList = "Error"
                };
            }
        }

        private async Task<Plate> FormatImportPlate(CreateOrEditPlateDto inputPlate, PlateType type, List<string> listError, int i)
        {
            var plate = ObjectMapper.Map<Plate>(inputPlate);
            plate.Type = type;
            if (plate.ImageUrl != null)
            {
                var newPlateImage = "";
                var arrImage = plate.ImageUrl.Split("|");
                for (int index = 0; index < arrImage.Length; index++)
                {
                    if (index == arrImage.Length - 1)
                    {
                        newPlateImage += Path.GetFileName(arrImage[index]);
                    }
                    else
                    {
                        newPlateImage += (Path.GetFileName(arrImage[index]) + '|');
                    }
                }
                plate.ImageUrl = newPlateImage;
            }

            if (AbpSession.TenantId != null)
            {
                plate.TenantId = (int?)AbpSession.TenantId;
            }

            if (inputPlate.IsPlate)
            {
                if (!string.IsNullOrEmpty(inputPlate.PlateCategoryName))
                {
                    var pc = await _plateCategoryRepository.FirstOrDefaultAsync(x => x.Name.Equals(inputPlate.PlateCategoryName.Trim()));
                    if (pc != null)
                    {
                        plate.PlateCategoryId = pc.Id;
                    }
                }
            }

            plate.Name = inputPlate.Name.Trim();
            plate.Code = inputPlate.Code.Trim();

            return plate;
        }

        private async Task<bool> VerifyImportPlate(CreateOrEditPlateDto inputPlate, PlateType type, List<string> listError, int i)
        {
            bool isError = false;

            if (inputPlate.IsPlate)
            {
                // Check exists PlateCategoryName.
                if (!string.IsNullOrEmpty(inputPlate.PlateCategoryName))
                {
                    var pc = await _plateCategoryRepository.FirstOrDefaultAsync(x => x.Name.Equals(inputPlate.PlateCategoryName));
                    if (pc == null)
                    {
                        isError = true;
                        listError.Add("Row " + (i + 2) + "- category: " + inputPlate.PlateCategoryName + " is not exist");
                        var errMess = "Import error: category: " + inputPlate.PlateCategoryName + " is  not exist";
                        Logger.Error(errMess);
                        return isError;
                    }
                }

                // Check exists Plate Code.
                if (!string.IsNullOrEmpty(inputPlate.Code))
                {
                    var p = await _plateRepository.FirstOrDefaultAsync(x => x.Code.Equals(inputPlate.Code.Trim()) && x.Name.Equals(inputPlate.Name.Trim()));

                    if (p == null)
                    {
                        p = await _plateRepository.FirstOrDefaultAsync(x => x.Code.Equals(inputPlate.Code.Trim()));
                        if (p != null)
                        {
                            isError = true;
                            listError.Add("Row " + (i + 2) + "- Plate Code: " + inputPlate.Code + " is exist");
                            var errMess = "Import error: Plate Code: " + inputPlate.Code + " is exist";
                            Logger.Error(errMess);
                            return isError;
                        }

                        p = await _plateRepository.FirstOrDefaultAsync(x => x.Name.Equals(inputPlate.Name.Trim()));
                        if (p != null)
                        {
                            isError = true;
                            listError.Add("Row " + (i + 2) + "- Plate Name: " + inputPlate.Name + " is exist");
                            var errMess = "Import error: Plate Name: " + inputPlate.Name + " is exist";
                            Logger.Error(errMess);
                            return isError;
                        }
                    }
                }
            }

            return isError;
        }

        /// <summary>
        /// Sync data between Server and Slave, To be Called from Slave side to this API
        /// </summary>
        /// <param name="machineSyncInput"></param>
        /// <returns></returns>
        [AbpAllowAnonymous]
        public async Task<EntitySyncOutputDto<PlateSyncDto>> GetPlates(EntitySyncInputDto<Guid> machineSyncInput)
        {
            try
            {
                _detailLogService.Log($"Plate: START get plate from server to sync to slave, request -> {JsonConvert.SerializeObject(machineSyncInput)}");
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MustHaveTenant, AbpDataFilters.MayHaveTenant, AbpDataFilters.SoftDelete))
                {
                    var machine = await _machineRepository.FirstOrDefaultAsync(x => x.Id == machineSyncInput.Id);
                    if (machine == null)
                    {
                        _detailLogService.Log($"Plate: MachineId is {machineSyncInput.Id} does not exist");
                        return null;
                    }
                    else if (machine.IsDeleted)
                    {
                        _detailLogService.Log($"Plate: MachineIs is {machineSyncInput.Id} was deleted");
                        return null;
                    }

                    var unsyncedEntities = _plateRepository.GetAllIncluding()
                                    .Where(x => x.TenantId == machine.TenantId && // only sync records from  a day old .
                                                                    (x.CreationTime > machineSyncInput.LastSynced ||
                                                                        x.LastModificationTime > machineSyncInput.LastSynced ||
                                                                        x.DeletionTime > machineSyncInput.LastSynced
                                                                    ))
                                    //.Include(x => x.PlateCategory)
                                    //.Include(x => x.PlateMachineSyncStatus)
                                    ;
                    var output = new EntitySyncOutputDto<PlateSyncDto>();
                    foreach (var entity in unsyncedEntities)
                    {
                        if (entity.IsDeleted)
                        {
                            output.DeletionEntities.Add(new PlateSyncDto()
                            {
                                Id = entity.Id
                            });
                        }
                        else
                        {
                            output.ModificationEntities.Add(new PlateSyncDto()
                            {
                                Id = entity.Id,
                                Name = entity.Name,
                                ImageUrl = entity.ImageUrl,
                                Desc = entity.Desc,
                                Code = entity.Code,
                                Avaiable = entity.Avaiable.GetValueOrDefault(),
                                Color = entity.Color,
                                PlateCategoryId = entity.PlateCategoryId,
                                Type = entity.Type.GetValueOrDefault()

                            });
                        }
                        // calculate latest sync time, so that client can keep track
                        // ignore this because LastSyncedTimeStamp is set to Datetime.Now currently.
                        //if (output.LastSyncedTimeStamp < recordLastUpdated?.ToUnixTime())
                        //{
                        //    output.LastSyncedTimeStamp = recordLastUpdated.GetValueOrDefault().ToUnixTime();
                        //}
                    }
                    _detailLogService.Log($"Plate: END get plate from server to sync to slave, data -> " + JsonConvert.SerializeObject(output));
                    return output;

                }
            }
            catch (Exception ex)
            {
                _detailLogService.Log($"Plate: END get plate from server to sync to slave, error -> " + ex.ToString());
                return null;
            }
        }

        [AbpAllowAnonymous]
        public async Task<bool> UpdateSyncStatus(SyncedItemData<Guid> syncData)
        {
            try
            {
                _detailLogService.Log($"Plate: START updateSyncStatus from server to sync to slave");
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MustHaveTenant, AbpDataFilters.MayHaveTenant, AbpDataFilters.SoftDelete))
                {
                    var machine = await _machineRepository.FirstOrDefaultAsync(x => x.Id == syncData.MachineId);
                    if (machine == null)
                    {
                        _detailLogService.Log($"Plate: MachineId is {syncData.MachineId} does not exist");
                        return false;
                    }
                    else if (machine.IsDeleted)
                    {
                        _detailLogService.Log($"Plate: MachineId is {syncData.MachineId} was deleted");
                        return false;
                    }

                    var allPMs = await _plateMachineSyncStatusRepository.GetAllListAsync(x => x.TenantId == machine.TenantId &&
                                                                                  x.MachineId == syncData.MachineId);
                    foreach (var item in syncData.SyncedItems)
                    {
                        var pm = allPMs.FirstOrDefault(x => x.PlateId == item);
                        if (pm == null)
                        {
                            await _plateMachineSyncStatusRepository.InsertAsync(
                                new PlateMachineSyncStatus
                                {
                                    Id = Guid.NewGuid(),
                                    PlateId = item,
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
                _detailLogService.Log($"Plate: END updateSyncStatus from server to sync to slave");
                return true;
            }
            catch (Exception ex)
            {
                _detailLogService.Log($"Plate: START updateSyncStatus with error -> " + ex.ToString());
                return false;
            }
        }
        private Random random = new Random();
        public async Task<PlateDto> GeneratePlateCode()
        {
            const string chars = "0123456789";
            var dto = new PlateDto();
            try
            {
                var newPlateCode = new string(Enumerable.Repeat(chars, 4).Select(s => s[random.Next(s.Length)]).ToArray());

                var exist = true;
                while (exist)
                {
                    var plate = await _plateRepository.FirstOrDefaultAsync(x => x.Code.Equals(newPlateCode));
                    exist = plate != null;
                }
                dto.Code = newPlateCode;
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
                return dto;
            }

            return dto;
        }
    }
}