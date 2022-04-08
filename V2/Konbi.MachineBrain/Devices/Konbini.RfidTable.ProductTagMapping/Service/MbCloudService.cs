using Konbini.RfidFridge.TagManagement.Data;
using Konbini.RfidFridge.TagManagement.DTO;
using Konbini.RfidFridge.TagManagement.Enums;
using Konbini.RfidFridge.TagManagement.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using static Konbini.RfidFridge.TagManagement.DTO.TrayResult;

namespace Konbini.RfidFridge.TagManagement.Service
{
    public class MbCloudService : IMbCloudService
    {
        public string BASE_URL { get; set; }
        public string USER_NAME { get; set; }
        public string PASSWORD { get; set; }
        public string Token { get; set; }

        public MbCloudService()
        {
            try
            {
                using (var context = new KDbContext())
                {
                    BASE_URL = context.Settings.SingleOrDefault(x => x.Key == SettingKey.CloudUrl)?.Value;
                    USER_NAME = context.Settings.SingleOrDefault(x => x.Key == SettingKey.UserName)?.Value;
                    PASSWORD = context.Settings.SingleOrDefault(x => x.Key == SettingKey.Password)?.Value;
                }
            }
            catch (Exception ex)
            {
                SeriLogService.LogError(ex.ToString());
            }
        }
        public async Task<List<PlateCategoryDTO.PlateCategory>> GetPlateCategories()
        {
            var data = await GetAsync("/api/services/app/PlateCategories/GetAll?MaxResultCount=10000&SkipCount=0");
            SeriLogService.LogInfo($"GetPlateCategories result: {Convert.ToString(data)}");

            if (data == null)
            {
                return new List<PlateCategoryDTO.PlateCategory>();
            }
            PlateCategoryDTO.Data returnData = JsonConvert.DeserializeObject<PlateCategoryDTO.Data>(Convert.ToString(data));
            var list = returnData.Result.Items.Select(x => x.Category).ToList();
            return list;
        }
        public async Task<List<GetPlateForView>> GetAllTray()
        {
            var data = await GetAsync("/api/services/app/Plates/GetAll?MaxResultCount=10000&SkipCount=0");
            SeriLogService.LogInfo($"Get Trays result: {Convert.ToString(data)}");

            if (data == null)
            {
                return new List<GetPlateForView>();
            }

            var returnData = JsonConvert.DeserializeObject<TrayResult.Data>(Convert.ToString(data));
            return returnData.Result.Items;
        }

        private async Task<dynamic> GetAsync(string api)
        {
            var client = new HttpClient { BaseAddress = new Uri(BASE_URL) };
            if (string.IsNullOrEmpty(Token))
            {
                Token = await GetToken();

                if (string.IsNullOrEmpty(Token))
                {
                    return null;
                }
            }

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + Token);

            var response = client.GetAsync(api).Result;
            dynamic res;
            using (var content = response.Content)
            {
                var result = content.ReadAsStringAsync();
                res = result.Result;
            }
            return string.IsNullOrEmpty(res) ? null : JObject.Parse(res);
        }

        public async Task<string> GetToken()
        {
            try
            {
                HttpClient client = new HttpClient
                {
                    BaseAddress = new Uri(BASE_URL)
                };
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Abp.TenantId", "1");
                var login = "{\"userNameOrEmailAddress\": \"" + USER_NAME + "\",\"password\": \"" + PASSWORD + "\"}";
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/api/TokenAuth/Authenticate")
                {
                    Content = new StringContent(login, Encoding.UTF8, "application/json")
                };

                dynamic res;
                var response = await client.SendAsync(request);
                using (var content = response.Content)
                {
                    var result = await content.ReadAsStringAsync();
                    res = result;
                }

                var returnData = string.IsNullOrEmpty(res) ? null : JObject.Parse(res);
                if (!string.IsNullOrEmpty(res))
                {
                    return returnData.result.accessToken;
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}