using KonbiCloud.Services.Dto;
using Konbini.Messages.Commands.RFIDTable;
using Konbini.Messages.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KonbiCloud.RFIDTable
{
    public interface IRfidTableSignalRMessageCommunicator
    {
        Task UpdateTransactionInfo(TransactionInfo transaction);
        Task SendAdminDetectedPlates(IEnumerable<Konbini.Messages.Commands.RFIDTable.PlateInfo> plates);
        Task UpdateTableSettings(string selectedPort, List<string> availablePorts, bool isServiceRunning);
        Task UpdateSessionInfo(SessionInfo sessionInfo);
        Task UpdatePosModeStatus(bool isPosModeOn);
        Task UpdatePaymentSate(PaymentState paymentState);
        Task ShowCustomerCountDown(int countTime, bool isOff);
        Task NotifyCashPayment();
        Task NotifyBarcodeValue(string barcode);
        Task NotifyProductChanges(string type);
        Task CheckServiceStatus(List<ServiceStatusForCheckingOnUIDto> serviceStatusResult);

    }
}
