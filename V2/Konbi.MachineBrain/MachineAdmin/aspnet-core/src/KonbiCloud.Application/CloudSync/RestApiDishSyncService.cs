using Abp.Application.Services;
using Abp.Configuration;
using Castle.Core.Logging;
using KonbiBrain.Common;
using KonbiCloud.Common;
using KonbiCloud.Configuration;
using KonbiCloud.Dto;
using KonbiCloud.Plate;
using KonbiCloud.Plate.Dtos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.CloudSync
{
    public class RestApiDishSyncService : ApplicationService, IDishSyncService, IApplicationService
    {
        private readonly ILogger _logger;
        private string serverUrl = null;
        private static HttpClient httpClient = new HttpClient();
        private readonly IDetailLogService _detailLogService;

        public RestApiDishSyncService(IDetailLogService detailLogService)
        {
            _detailLogService = detailLogService;
        }

        public async Task<bool> PushToServer(SyncedItemData<Disc> dishes)
        {
            try
            {
                _detailLogService.Log($"CallToServer: START PushToServer Discs ");
                serverUrl = SettingManager.GetSettingValue(AppSettingNames.SyncServerUrl);
                var stringPayload = await Task.Run(() => JsonConvert.SerializeObject(dishes));
                var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");
                using (var httpResponse = await httpClient.PostAsync($"{serverUrl}/api/services/app/Discs/SyncDishData", httpContent))
                {
                    // If the response contains content we want to read it!
                    if (httpResponse.Content != null && httpResponse.StatusCode == HttpStatusCode.OK)
                    {
                        var responseContent = await httpResponse.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<RestApiResult>(responseContent);
                        if (result.success && result.result)
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }
            catch (Exception e)
            {
                _detailLogService.Log($"CallToServer: END PushToServer Discs with error -> {e.ToString()}");
                return false;
            }
        }


        public async Task<EntitySyncInputDto<DiscSyncDto>> Sync(Guid machineId)
        {
            try
            {
                _detailLogService.Log($"CallToServer: START call Discs API");
                serverUrl = SettingManager.GetSettingValue(AppSettingNames.SyncServerUrl);
                var lastSynced = await SettingManager.GetSettingValueForTenantAsync<long>(AppSettingNames.InventoryEntityLastSynced, 1);
                var httpResponse = await httpClient.GetStringAsync($"{serverUrl}/api/services/app/Discs/GetDishes?Id={machineId}&LastSyncedTimeStamp={lastSynced}");
                var responseObject = JsonConvert.DeserializeObject<SyncApiResponseNotList<EntitySyncInputDto<DiscSyncDto>>>(httpResponse);
                return responseObject.result;
            }
            catch (Exception e)
            {
                _detailLogService.Log($"CallToServer: END call Discs API with error -> {e.ToString()}");
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

                using (var httpResponse = await httpClient.PutAsync($"{serverUrl}/api/services/app/Discs/UpdateSyncStatus", httpContent))
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
