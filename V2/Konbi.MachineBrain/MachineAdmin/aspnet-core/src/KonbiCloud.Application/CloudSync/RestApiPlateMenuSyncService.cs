using Abp.Application.Services;
using Abp.Configuration;
using Castle.Core.Logging;
using KonbiCloud.Common;
using KonbiCloud.Configuration;
using KonbiCloud.Dto;
using KonbiCloud.ProductMenu.Dtos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.CloudSync
{
    public class RestApiPlateMenuSyncService : ApplicationService, IPlateMenuSyncService, IApplicationService
    {
        private readonly ILogger _logger;
        private string serverUrl = null;
        private static HttpClient httpClient = new HttpClient();
        private readonly IDetailLogService _detailLogService;
        public RestApiPlateMenuSyncService(ILogger logger, IDetailLogService detailLogService)
        {
            _detailLogService = detailLogService;
        }

        public async Task<EntitySyncInputDto<ProductMenuSyncDto>> Sync(Guid machineId)
        {
            try
            {
                _detailLogService.Log($"CallToServer: START call ProductMenu API");
                serverUrl = SettingManager.GetSettingValue(AppSettingNames.SyncServerUrl);
                var lastSynced = await SettingManager.GetSettingValueForTenantAsync<long>(AppSettingNames.ProductMenuEntityLastSynced,1);
                var httpResponse = await httpClient.GetStringAsync($"{serverUrl}/api/services/app/ProductMenus/GetProductMenus?Id={machineId}&LastSyncedTimeStamp={lastSynced}");
                var responseObject = JsonConvert.DeserializeObject<SyncApiResponseNotList<EntitySyncInputDto<ProductMenuSyncDto>>>(httpResponse);
                return responseObject.result;
            }
            catch (Exception e)
            {
                _detailLogService.Log($"CallToServer: END call ProductMenu API with error -> {e.ToString()}");
                return null;
            }
        }

        //public async Task<bool> UpdateSyncStatus(SyncedItemData<Guid> input)
        //{
        //    try
        //    {
        //        serverUrl = SettingManager.GetSettingValue(AppSettingNames.SyncServerUrl);
        //        string json = JsonConvert.SerializeObject(input, Formatting.Indented);
        //        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        //        using (var httpResponse = await httpClient.PutAsync($"{serverUrl}/api/services/app/ProductMenus/UpdateSyncStatus", httpContent))
        //        {
        //            return httpResponse.IsSuccessStatusCode;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        _logger?.Error(e.Message);
        //        _logger?.Error(e.StackTrace);
        //        return false;
        //    }
        //}
    }
}
