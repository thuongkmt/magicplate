using Abp.Application.Services;
using Abp.Configuration;
using Abp.UI;
using Castle.Core.Logging;
using KonbiCloud.Common;
using KonbiCloud.Configuration;
using KonbiCloud.Dto;
using KonbiCloud.Machines.Dtos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace KonbiCloud.CloudSync
{
    public class RestApiSessionSyncService : ApplicationService, ISessionSyncService, IApplicationService
    {
        private readonly ILogger _logger;
        private string serverUrl = null;
        private static HttpClient httpClient = new HttpClient();
        private readonly IDetailLogService _detailLogService;

        public RestApiSessionSyncService(IDetailLogService detailLogService)
        {
            _detailLogService = detailLogService;
        }

        public async Task<EntitySyncInputDto<SessionSyncDto>> Sync(Guid machineId)
        {
            try
            {
                _detailLogService.Log($"CallToServer: START call session API");
                serverUrl = SettingManager.GetSettingValue(AppSettingNames.SyncServerUrl);
                var lastSynced = await SettingManager.GetSettingValueForTenantAsync<long>(AppSettingNames.SessionEntityLastSynced, 1);
                var httpResponse = await httpClient.GetStringAsync($"{serverUrl}/api/services/app/Sessions/GetSessions?Id={machineId}&LastSyncedTimeStamp={lastSynced}");
                var responseObject = JsonConvert.DeserializeObject<SyncApiResponseNotList<EntitySyncInputDto<SessionSyncDto>>>(httpResponse);
                return responseObject.result;
            }
            catch (Exception e)
            {
                _detailLogService.Log($"CallToServer: END call session API with error -> {e.ToString()}");
                return null;
            }
        }
    }
}
