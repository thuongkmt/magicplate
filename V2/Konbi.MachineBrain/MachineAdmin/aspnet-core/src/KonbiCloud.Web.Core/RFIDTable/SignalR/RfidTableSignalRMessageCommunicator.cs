using Abp.Dependency;
using Abp.ObjectMapping;
using Castle.Core.Logging;
using KonbiCloud.RFIDTable;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;
using Konbini.Messages.Commands.RFIDTable;
using Konbini.Messages.Enums;
using KonbiCloud.Services.Dto;

namespace KonbiCloud.Web.RFIDTable.SignalR
{

    public class RfidTableSignalRMessageCommunicator : IRfidTableSignalRMessageCommunicator, ITransientDependency
    {
        /// <summary>
        /// Reference to the logger.
        /// </summary>
        public ILogger Logger { get; set; }

        private readonly IObjectMapper _objectMapper;


        private readonly IHubContext<RFIDTableHub> _messageRfidHub;

        public RfidTableSignalRMessageCommunicator(
            IObjectMapper objectMapper,
           IHubContext<RFIDTableHub> messageRfidHub)
        {
            _objectMapper = objectMapper;

            _messageRfidHub = messageRfidHub;

            Logger = NullLogger.Instance;
        }

        public Task UpdateTransactionInfo(TransactionInfo transaction)
        {
            return _messageRfidHub.Clients.Group("CustomerUI").SendCoreAsync("updateTransactionInfo", new[] { transaction });
        }

        public Task SendAdminDetectedPlates(IEnumerable<Konbini.Messages.Commands.RFIDTable.PlateInfo> plates)
        {
            return _messageRfidHub.Clients.Group(TableManager.TableDeviceSettingGroup).SendCoreAsync("detectedPlates", new[] { plates });
        }

        public Task UpdateTableSettings(string selectedPort, List<string> availablePorts, bool isServiceRunning)
        {
            return _messageRfidHub.Clients.Group(TableManager.TableDeviceSettingGroup).SendCoreAsync("updateTableSettings", new[] { new { selectedPort, availablePorts, isServiceRunning } });
        }

        public Task UpdateSessionInfo(SessionInfo sessionInfo)
        {
            //return _messageRfidHub.Clients.Group(TableManager.CustomerUIGroup).SendCoreAsync("updateSessionInfo", new[] { sessionInfo });
            return _messageRfidHub.Clients.All.SendCoreAsync("updateSessionInfo", new[] { sessionInfo });
        }

        public Task UpdatePaymentSate(PaymentState paymentState)
        {
            return _messageRfidHub.Clients.Group(TableManager.CustomerUIGroup).SendCoreAsync("updatePaymentSate", new[] { new { paymentState } });
        }

        public Task UpdatePosModeStatus(bool isPosModeOn)
        {
            return _messageRfidHub.Clients.Group(TableManager.CustomerUIGroup).SendCoreAsync("updatePosModeStatus", new[] { new { isPosModeOn } });
        }

        public Task NotifyCashPayment()
        {
            return _messageRfidHub.Clients.Group(TableManager.CustomerUIGroup).SendCoreAsync("notifyCashPayment", new[] { new { } });
        }

        public Task NotifyProductChanges(string type)
        {
            return _messageRfidHub.Clients.Group(TableManager.CustomerUIGroup).SendCoreAsync("notifyProductChanges", new[] { new { type } });
        }

        public Task NotifyBarcodeValue(string barcode)
        {
            return _messageRfidHub.Clients.Group(TableManager.CustomerUIGroup).SendCoreAsync("notifyBarcode", new[] { new { barcode } });
        }


        public Task ShowCustomerCountDown(int countTime, bool isOff)
        {
            return _messageRfidHub.Clients.Group(TableManager.CustomerUIGroup).SendCoreAsync("showCustomerCountDown", new[] { new { countTime, isOff } });
        }

        public Task CheckServiceStatus(List<ServiceStatusForCheckingOnUIDto> serviceStatusResult)
        {
            return _messageRfidHub.Clients.All.SendCoreAsync("checkServiceStatus", new[] { serviceStatusResult });
        }
    }
}
