using Abp.Application.Services;
using Abp.Configuration;
using Castle.Core.Logging;
using KonbiBrain.Common;
using KonbiCloud.Common;
using KonbiCloud.Configuration;
using KonbiCloud.Transactions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.CloudSync
{
    public class RestApiTransactionSyncService : ApplicationService, ITransactionSyncService, IApplicationService
    {
        private readonly ILogger _logger;
        private readonly IConfigurationRoot _appConfiguration;
        private readonly IDetailLogService _detailLogService;
        private string serverUrl = null;
        private static HttpClient httpClient = new HttpClient();

        public RestApiTransactionSyncService(ILogger logger, IHostingEnvironment env, IDetailLogService detailLogService)
        {
            _logger = logger;
            _appConfiguration = env.GetAppConfiguration();
            _detailLogService = detailLogService;
        }

        public async Task<RestApiGenericResult<long>> PushTransactionsToServer(IList<DetailTransaction> trans, string transImageFolder)
        {          

            var tranImgFolderPath = Path.Combine(Directory.GetCurrentDirectory(), Const.ImageFolder, transImageFolder);
            var syncingTransactions = new List<DetailTransaction>();

            foreach (var transaction in trans)
            {            
                var tranSync = new DetailTransaction()
                {
                    Id = transaction.Id,
                    TranCode = transaction.TranCode,
                    Amount = transaction.Amount,
                    DiscountPercentage = transaction.DiscountPercentage,
                    TaxPercentage = transaction.TaxPercentage,
                    CashlessDetail = transaction.CashlessDetail,
                    CashDetail = transaction.CashDetail,
                    PaymentState = transaction.PaymentState,
                    PaymentTime = transaction.PaymentTime,
                    PaymentType = transaction.PaymentType,
                    StartTime = transaction.StartTime,
                    Status = transaction.Status,
                    Buyer = transaction.Buyer,
                    SessionId = transaction.SessionId,
                    MachineId = transaction.MachineId,
                    MachineName = transaction.MachineName,
                    BeginTranImage = transaction.BeginTranImage,
                    EndTranImage = transaction.EndTranImage,
                    Products = transaction.Products
                };

                var mId = Guid.Empty;
                Guid.TryParse(await SettingManager.GetSettingValueAsync(AppSettingNames.MachineId), out mId);
                if (mId != Guid.Empty)
                {
                    tranSync.MachineId = mId;
                }
                tranSync.MachineName = await SettingManager.GetSettingValueAsync(AppSettingNames.MachineName);

                //compress and generate image in byte array
                Common.Common.CompressImage(tranSync, tranImgFolderPath, _detailLogService);
                Common.Common.CompressImage(tranSync, tranImgFolderPath, _detailLogService, false);
                syncingTransactions.Add(tranSync);

            }
            var result = new RestApiGenericResult<long>();
            if (SettingManager == null)
            {
                _logger?.Error($"Transaction Sync Service: SettingManager is null");
                return result;
            }
            try
            {
                serverUrl = SettingManager.GetSettingValue(AppSettingNames.SyncServerUrl);
                var stringPayload = await Task.Run(() => JsonConvert.SerializeObject(syncingTransactions, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }));
                var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");
                using (var httpResponse = await httpClient.PostAsync($"{serverUrl}/api/services/app/Transaction/AddTransactions", httpContent))
                {
                    if (httpResponse.Content != null && httpResponse.StatusCode == HttpStatusCode.OK)
                    {
                        var responseContent = await httpResponse.Content.ReadAsStringAsync();
                        result = JsonConvert.DeserializeObject<RestApiGenericResult<long>>(responseContent);
                        return result;
                    }
                    return result;
                }
            }
            catch (Exception e)
            {
                _logger?.Error(e.Message);
                _logger?.Error(e.StackTrace);
                return result;
            }
        }
    }
}
