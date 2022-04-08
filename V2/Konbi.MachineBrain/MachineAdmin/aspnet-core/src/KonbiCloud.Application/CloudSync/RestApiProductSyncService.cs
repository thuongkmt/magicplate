using Abp.Application.Services;
using Abp.Configuration;
using Castle.Core.Logging;
using KonbiCloud.Common;
using KonbiCloud.Configuration;
using KonbiCloud.Dto;
using KonbiCloud.Products;
using KonbiCloud.Products.Dtos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.CloudSync
{
    public class RestApiProductSyncService : ApplicationService, IProductSyncService, IApplicationService
    {
        private readonly ILogger _logger;
        private string serverUrl = null;
        private static HttpClient httpClient = new HttpClient();
        private readonly IDetailLogService _detailLogService;

        public RestApiProductSyncService(IDetailLogService detailLogService)
        {
            _detailLogService = detailLogService;
        }

        public async Task<EntitySyncInputDto<ProductSyncDto>> Sync(Guid machineId)
        {
            try
            {
                _detailLogService.Log($"CallToServer: START call Product API");
                serverUrl = SettingManager.GetSettingValue(AppSettingNames.SyncServerUrl);
                var lastSynced = await SettingManager.GetSettingValueForTenantAsync<long>(AppSettingNames.ProductEntityLastSynced, 1);
                var httpResponse = await httpClient.GetStringAsync($"{serverUrl}/api/services/app/Product/GetProducts?Id={machineId}&LastSyncedTimeStamp={lastSynced}");
                var responseObject = JsonConvert.DeserializeObject<SyncApiResponseNotList<EntitySyncInputDto<ProductSyncDto>>>(httpResponse);
                return responseObject.result;
            }
            catch (Exception e)
            {
                _detailLogService.Log($"CallToServer: END call Product API with error -> {e.ToString()}");
                return null;
            }
        }
    }
}
