
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.Runtime.Caching;
using Abp.UI;
using KonbiCloud.Authorization;
using KonbiCloud.CloudSync;
using KonbiCloud.Common;
using KonbiCloud.Configuration;
using KonbiCloud.Dto;
using KonbiCloud.Enums;
using KonbiCloud.Plate.Dtos;
using KonbiCloud.Plate.Exporting;
using KonbiCloud.ProductMenu.Dtos;
using KonbiCloud.RFIDTable.Cache;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace KonbiCloud.Plate
{
    [AbpAuthorize(AppPermissions.Pages_Plates)]
    public class PlatesAppService : KonbiCloudAppServiceBase, IPlatesAppService
    {
        private readonly IRepository<Plate, Guid> _plateRepository;
        private readonly IPlatesExcelExporter _platesExcelExporter;
        private readonly IRepository<PlateCategory, int> _plateCategoryRepository;
        private readonly IRepository<Disc, Guid> _discRepository;
        private readonly IConfigurationRoot _appConfiguration;
        private readonly IDetailLogService _detailLogService;
        private readonly ISaveFromCloudService _saveFromCloudService;
        private readonly ICacheManager _cacheManager;

        private string _serverUrl;
        public string ServerUrl
        {
            get
            {
                if(SettingManager != null && string.IsNullOrEmpty(_serverUrl))
                {
                    _serverUrl = SettingManager.GetSettingValue(AppSettingNames.SyncServerUrl);
                    if(_serverUrl == null)
                    {
                        _serverUrl = "";
                    }
                }
                return _serverUrl;
            }
        }

        public PlatesAppService(IRepository<Plate, Guid> plateRepository, IRepository<Disc, Guid> discRepository,
            IPlatesExcelExporter platesExcelExporter, IRepository<PlateCategory, int> plateCategoryRepository,
            IHostingEnvironment env, IDetailLogService detailLog, ISaveFromCloudService saveFromCloudService, ICacheManager cacheManager)
        {
            _plateRepository = plateRepository;
            _platesExcelExporter = platesExcelExporter;
            _plateCategoryRepository = plateCategoryRepository;
            _discRepository = discRepository;
            _appConfiguration = env.GetAppConfiguration();
            _detailLogService = detailLog;
            _cacheManager = cacheManager;
            _saveFromCloudService = saveFromCloudService;
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

                var pathImage = Path.Combine(ServerUrl, Const.ImageFolder, _appConfiguration[AppSettingNames.PlateImageFolder]);
                foreach (var p in plates)
                {
                    if (p.Plate != null && p.Plate.ImageUrl != null)
                    {
                        if (p.Plate.ImageUrl.Contains(Const.NoImage)) continue;

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

        public async Task<GetPlateForEditOutput> GetPlateForView(EntityDto<Guid> input)
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
                    var pathImage = Path.Combine(ServerUrl, Const.ImageFolder, _appConfiguration[AppSettingNames.PlateImageFolder]);

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
                    var pathImage = Path.Combine(ServerUrl, Const.ImageFolder, _appConfiguration[AppSettingNames.PlateImageFolder]);

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
                if (input.Id == null)
                {
                    var p = await _plateRepository.FirstOrDefaultAsync(x => x.Code.ToLower().Equals(input.Code.Trim().ToLower()));
                    if (p != null)
                    {
                        return new PlateMessage { Message = $"Plate code {input.Code} already existed, please use another code." };
                    }
                }
                else
                {
                    var p = await _plateRepository.FirstOrDefaultAsync(x => x.Id != input.Id.Value && x.Code.ToLower().Equals(input.Code.Trim().ToLower()));
                    if (p != null)
                    {
                        return new PlateMessage { Message = $"Plate code {input.Code} already existed, please use another code." };
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
                return new PlateMessage { Message = null };
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
                return new PlateMessage { Message = $"An error occurred, please try again." };
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Plates_Create)]
        private async Task Create(CreateOrEditPlateDto input)
        {
            var plate = ObjectMapper.Map<Plate>(input);

            if (AbpSession.TenantId != null)
            {
                plate.TenantId = AbpSession.TenantId;
            }

            await _plateRepository.InsertAsync(plate);
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        [AbpAuthorize(AppPermissions.Pages_Plates_Edit)]
        private async Task Update(CreateOrEditPlateDto input)
        {
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
            ObjectMapper.Map(input, plate);
        }

        [AbpAuthorize(AppPermissions.Pages_Plates_Delete)]
        public async Task Delete(EntityDto<Guid> input)
        {
            try
            {
                await _plateRepository.DeleteAsync(input.Id);
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
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
            return _platesExcelExporter.ExportToFile(plateListDtos);
        }

        public async Task<PagedResultDto<PlateCheckInventoryDto>> GetAllPlates ()
        {
            try
            {
                var plates = await _plateRepository.GetAll().Select(x => new PlateCheckInventoryDto
                {
                    PlateId = x.Id,
                    PlateCode = x.Code,
                    PlateName = x.Name
                }).ToListAsync();

                return new PagedResultDto<PlateCheckInventoryDto>(
                    plates.Count(),
                    plates
                );
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
                return new PagedResultDto<PlateCheckInventoryDto>(0, new List<PlateCheckInventoryDto>());
            }
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
                    if (inputPlate.Code != null)
                    {
                        var type = inputPlate.IsPlate ? PlateType.Plate : PlateType.Tray;
                        var p = plates.FirstOrDefault(x => x.Code.Equals(inputPlate.Code.Trim()));
                        if (p == null)
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
                                    var pc = await _plateCategoryRepository.FirstOrDefaultAsync(x => x.Name.Equals(inputPlate.PlateCategoryName));
                                    if (pc == null)
                                    {
                                        listError.Add("Row " + (i + 2) + "- category: " + inputPlate.PlateCategoryName + " is not exist");
                                        var errMess = "Import error: category: " + inputPlate.PlateCategoryName + " is  not exist";
                                        Logger.Error(errMess);
                                    }
                                    else
                                    {
                                        plate.PlateCategoryId = pc.Id;
                                        await _plateRepository.InsertAsync(plate);
                                        listSuccess.Add(inputPlate);
                                    }
                                }
                                else
                                {
                                    listError.Add("Row " + (i + 2) + "- category name is null");
                                }
                            }
                            else
                            {
                                await _plateRepository.InsertAsync(plate);
                                listSuccess.Add(inputPlate);
                            }
                        }
                        else
                        {
                            listError.Add("Row " + (i + 2) + "- code " + inputPlate.Code + " is available");
                            var errMess = "Import error: code " + inputPlate.Code + " is available";
                            Logger.Error(errMess);
                        }
                    }
                    else
                    {
                        listError.Add("Row " + (i + 2) + "- code is null");
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

        [AbpAllowAnonymous]
        public async Task<bool> SyncPlateData()
        {
            //// Sync Plate Category.
            bool result = await _saveFromCloudService.SyncPlateCategoryData();
            if (!result)
            {
                return false;
            }
            result= await _saveFromCloudService.SyncPlateData();

            return result;
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