using Abp.Application.Services;
using Abp.Configuration;
using Castle.Core.Logging;
using KonbiCloud.Common;
using KonbiCloud.Configuration;
using KonbiCloud.Machine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.CloudSync
{
    public class RestApiMachineSyncService : ApplicationService, IMachineSyncService, IApplicationService
    {
        private readonly ILogger _logger;
        private string serverUrl = null;
        private static HttpClient httpClient = new HttpClient();

        public RestApiMachineSyncService(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<MachineSync> GetMachineFromServer(string Id)
        {
            try
            {
                if (Id != null)
                {
                    serverUrl = SettingManager.GetSettingValue(AppSettingNames.SyncServerUrl);
                    var httpResponse = await httpClient.GetStringAsync($"{serverUrl}/api/services/app/Machine/GetDetail?Id={Id}");
                    var machineResponse = JsonConvert.DeserializeObject<SyncApiResponseNotList<MachineSync>>(httpResponse);
                    if (machineResponse != null && machineResponse.result!=null)
                    {
                       
                        return machineResponse.result;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }                
            }
            catch (Exception e)
            {
                _logger?.Error(e.Message);
                _logger?.Error(e.StackTrace);
                return null;
            }
        }

        public async Task<bool> UpdateMachineToServer(Machine.Machine input)
        {
            try
            {
                serverUrl = SettingManager.GetSettingValue(AppSettingNames.SyncServerUrl);
                input.Id = new Guid(input.id);
                string json = JsonConvert.SerializeObject(input, Formatting.Indented);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                using (var httpResponse = await httpClient.PutAsync($"{serverUrl}/api/services/app/Machine/UpdateMachineToServer", httpContent))
                {
                    return httpResponse.IsSuccessStatusCode;
                }
            }
            catch (Exception e)
            {
                _logger?.Error(e.Message);
                _logger?.Error(e.StackTrace);
                return false;
            }            
        }
    }
}
