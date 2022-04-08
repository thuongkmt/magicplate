using Abp.Application.Services;
using Abp.Configuration;
using Castle.Core.Logging;
using KonbiCloud.Common;
using KonbiCloud.Configuration;
using KonbiCloud.Dto;
using KonbiCloud.Products;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.CloudSync
{
    public class RestApiProductCategorySyncService : ApplicationService, IProductCategorySyncService, IApplicationService
    {
        private readonly ILogger _logger;
        private string serverUrl = null;
        private static HttpClient httpClient = new HttpClient();
        private readonly IDetailLogService _detailLogService;

        public RestApiProductCategorySyncService(IDetailLogService detailLogService)
        {
            _detailLogService = detailLogService;
        }

        public async Task<EntitySyncInputDto<CategorySyncDto>> Sync(Guid mId)
        {
            try
            {
                _detailLogService.Log($"CallToServer: START call ProductCategory API");
                serverUrl = SettingManager.GetSettingValue(AppSettingNames.SyncServerUrl);
                var lastSynced = await SettingManager.GetSettingValueForTenantAsync<long>(AppSettingNames.CategoryEntityLastSynced, 1);
                var httpResponse = await httpClient.GetStringAsync($"{serverUrl}/api/services/app/Category/GetCategories?Id={mId}&LastSyncedTimeStamp={lastSynced}");
                var responseObject = JsonConvert.DeserializeObject<SyncApiResponseNotList<EntitySyncInputDto<CategorySyncDto>>>(httpResponse);
                return responseObject.result;
            }
            catch (Exception e)
            {
                _detailLogService.Log($"CallToServer: END call ProductCategory API with error -> {e.ToString()}");
                return null;
            }
        }
    }
}
