using Abp.Dependency;
using Konbini.Messages.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.RFIDTable
{
    public interface ITableManager: IDeviceManager, ISingletonDependency
    {
        HashSet<ClientInfo> Clients { get;  }

        bool OnSale { get; }

        TransactionInfo Transaction { get; set; }

        Task<SessionInfo> GetSessionInfoAsync();

        bool GetPOSModeStatusAsync();

        void ChangeProductUpdateStatus(bool isUpdating);
        void ChangePosModeStatus();

        Task<TransactionInfo> ProcessTransactionAsync(IEnumerable<PlateReadingInput> plates);

        void NotifyOnTransactionChanged();

        void NotifyPosModeStatusChanged();

        Task GenerateSaleTransactionAsync();

        Task GetTableDeviceSettingsAsync();

        Task CancelTransactionAsync();

        void RemoveOrderItem(Guid uid);

        Task ExecuteBarcodeScanningLog(string barcodeValue);

        Task<TransactionInfo> ExecuteManualTransactionAsync(ManualPaymentInput input);

        void ResetTransaction();

        Task<bool> ActivatePayment(string paymentMode);

        Task PayCash();
        ConcurrentDictionary<PaymentTypes,string> GetAcceptedPaymentMethods();

        void PrintReceipt(List<TransactionInfo> lstTransaction);
        Task<bool> OverrideSuccessPaymentAsync();
    }
}
