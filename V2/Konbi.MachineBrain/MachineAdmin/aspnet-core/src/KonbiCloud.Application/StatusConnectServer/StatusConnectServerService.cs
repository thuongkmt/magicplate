using Abp.Authorization;
using Abp.Configuration;
using KonbiCloud.Common;
using KonbiCloud.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.StatusConnectServer
{
    public class StatusConnectServerService : KonbiCloudAppServiceBase, IStatusConnectServerService
    {
        private readonly IConfigurationRoot _appConfiguration;
        private string _serverUrl;
        private readonly IDetailLogService _detailLogService;
        private static HttpClient httpClient = new HttpClient();

        public StatusConnectServerService(
            IHostingEnvironment env,
            IDetailLogService detailLog)
        {
            _appConfiguration = env.GetAppConfiguration();
            _detailLogService = detailLog;
        }

        [AbpAllowAnonymous]
        public async Task<bool> GetStatusConnectServer()
        {
            try
            {
                bool result = false;

                _serverUrl = SettingManager.GetSettingValue(AppSettingNames.SyncServerUrl);
                var httpResponse = await httpClient.GetAsync($"{_serverUrl}/api/services/app/StatusConnectClientService/GetStatusConnectClient");
                if (httpResponse.IsSuccessStatusCode)
                {
                    result = true;
                }

                return result;
            }
            catch (Exception ex)
            {
                _detailLogService.Log($"Error when check status connection to server: {ex.Message}");
                return false;
            }
        }
    }
}
