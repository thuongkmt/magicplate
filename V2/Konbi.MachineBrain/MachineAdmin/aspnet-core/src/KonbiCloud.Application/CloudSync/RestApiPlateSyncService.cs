using Abp.Application.Services;
using Abp.Configuration;
using Castle.Core.Logging;
using KonbiCloud.Common;
using KonbiCloud.Configuration;
using KonbiCloud.Dto;
using KonbiCloud.Plate.Dtos;
using KonbiCloud.ProductMenu.Dtos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.CloudSync
{
    public class RestApiPlateSyncService : ApplicationService, IPlateSyncService, IApplicationService
    {
        private readonly ILogger _logger;
        private string serverUrl = null;
        private static HttpClient httpClient = new HttpClient();
        private readonly IDetailLogService _detailLogService;
        public RestApiPlateSyncService(IDetailLogService detailLogService)
        {
            _detailLogService = detailLogService;
        }

        public async Task<EntitySyncInputDto<PlateSyncDto>> Sync(Guid machineId)
        {
            try
            {
                _detailLogService.Log($"CallToServer: START call Plate API");
                serverUrl = SettingManager.GetSettingValue(AppSettingNames.SyncServerUrl);
                var lastSynced = await SettingManager.GetSettingValueForTenantAsync<long>(AppSettingNames.PlateEntityLastSynced,1);
                var httpResponse = await httpClient.GetStringAsync($"{serverUrl}/api/services/app/Plates/GetPlates?Id={machineId}&LastSyncedTimeStamp={lastSynced}");
                var responseObject = JsonConvert.DeserializeObject<SyncApiResponseNotList<EntitySyncInputDto<PlateSyncDto>>>(httpResponse);
                return responseObject.result;
            }
            catch (Exception e)
            {
                _detailLogService.Log($"CallToServer: END call Plate API with error -> {e.ToString()}");
                return null;
            }
        }

        public async Task<bool> UpdateSyncStatus(SyncedItemData<Guid> input)
        {
            try
            {
                _detailLogService.Log($"CallToServer: START call UpdateSyncStatus API");
                serverUrl = SettingManager.GetSettingValue(AppSettingNames.SyncServerUrl);
                string json = JsonConvert.SerializeObject(input, Formatting.Indented);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                using (var httpResponse = await httpClient.PutAsync($"{serverUrl}/api/services/app/Plates/UpdateSyncStatus", httpContent))
                {
                    return httpResponse.IsSuccessStatusCode;
                }
            }
            catch (Exception e)
            {
                _detailLogService.Log($"CallToServer: END call UpdateSyncStatus API with error -> {e.ToString()}");
                return false;
            }
        }
    }
}
