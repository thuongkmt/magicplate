using Abp.AspNetCore.SignalR.Hubs;
using Abp.Auditing;
using Abp.RealTime;
using KonbiCloud.RFIDTable;
using KonbiCloud.Web.RFIDTable.SignalR.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace KonbiCloud.Web.RFIDTable.SignalR
{
    public class RFIDTableHub : OnlineClientHubBase
    {

        public static int TableDeviceState; // 0 disconnected, 1 - connected and working, 2 - connected but comport is not ready.
        private readonly ITableManager tableManager;
        private readonly ITableSettingsManager tableSettingsManager;
        public RFIDTableHub(IOnlineClientManager onlineClientManager, IClientInfoProvider clientInfoProvider, ITableManager tableManager, ITableSettingsManager tableSettingsManager) : base(onlineClientManager, clientInfoProvider)
        {
            this.tableManager = tableManager;
            this.tableSettingsManager = tableSettingsManager;
        }

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {

            var disconnectingClient = tableManager.Clients.FirstOrDefault(el => el.ConnectionId == Context.ConnectionId);
            if (disconnectingClient != null)
            {
                if (!string.IsNullOrEmpty(disconnectingClient.Group))
                    Groups.RemoveFromGroupAsync(disconnectingClient.ConnectionId, disconnectingClient.Group);
                tableManager.Clients.RemoveWhere(el => el.ConnectionId == disconnectingClient.ConnectionId);

            }

            return base.OnDisconnectedAsync(exception);
        }

        public async Task<bool> JoinGroup(string groupName)
        {
            try
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                tableManager.Clients.Add(new ClientInfo() { ConnectionId = Context.ConnectionId, Group = groupName });
                if (groupName == TableManager.CustomerUIGroup)
                {
                    await Clients.Caller.SendCoreAsync("updateTransactionInfo", new[] { tableManager.Transaction });
                    await Clients.Caller.SendCoreAsync("updatePaymentMethods", new[] { tableManager.GetAcceptedPaymentMethods() });
                }
                else if (groupName == TableManager.TableDeviceSettingGroup)
                {
                    await tableManager.GetTableDeviceSettingsAsync();

                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return false;
            }
            return true;
          


        }
        public Task LeaveGroup(string groupName)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task CancelTransaction()
        {
            await tableManager.CancelTransactionAsync();
        }

        public void RemoveOrderItem(Guid uId)
        {
            tableManager.RemoveOrderItem(uId);
        }

        public async Task<bool> OnClickPay(string paymentMode)
        {
            return await tableManager.ActivatePayment(paymentMode);
        }

        public void PrintReceipt(string lstTransaction)
        {
            TransactionInfo transaction = JsonConvert.DeserializeObject<TransactionInfo>(lstTransaction);
            List<TransactionInfo> lst = new List<TransactionInfo>();
            lst.Add(transaction);
            tableManager.PrintReceipt(lst);
        }

        public async Task PayCash()
        {
            await tableManager.PayCash();
        }
        public async Task<bool> OverrideSuccessPayment()
        {
            return await tableManager.OverrideSuccessPaymentAsync();
        }
        public async Task ExecuteBarcodeScanningTransaction(string barcodeValue)
        {
            await tableManager.ExecuteBarcodeScanningLog(barcodeValue);
        }

        public async Task ExecuteManualTransaction(ManualPaymentInput input)
        {
            await tableManager.ExecuteManualTransactionAsync(input);
        }

        public void ResetTransaction()
        {
            tableManager.ResetTransaction();
            tableManager.NotifyOnTransactionChanged();
        }

        //public async Task UpdateDishes(IEnumerable<DishInfo> dishes)
        //{

        //    var tran = tableManager.ProcessTransaction(dishes.Select(el => new PlateReadingInput() { UID = el.UID, UType = el.UType }));

        //    // update customer with transaction info.
        //    await Clients.Group("CustomerUI").SendCoreAsync("updateTransactionInfo", new[] { tran });


        //    await Clients.Group("AdminUI").SendCoreAsync("detectedPlates", new[] { dishes });
        //}
        #region CustomerUI 
        public async Task<SessionInfo> GetSessionInfo()
        {
            return await tableManager.GetSessionInfoAsync();
        }

        public bool GetPOSModeStatus()
        {
            return tableManager.GetPOSModeStatusAsync();
        }

        public void ChangeProductUpdateStatus(bool isUpdating)
        {
            tableManager.ChangeProductUpdateStatus(isUpdating);
        }

        public void ChangePosModeStatus()
        {
            tableManager.ChangePosModeStatus();
        }

        #endregion

        #region Table Settings
        public async Task<bool> StopTableService()
        {
            return tableSettingsManager.StopService();
        }
        public async Task<bool> StartTableService(string port)
        {
            return tableSettingsManager.StartService(port);
        }
        public async Task<bool> ReadPlates()
        {
            return await tableSettingsManager.ForceToReadPlates();
        }
        #endregion
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
