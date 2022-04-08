using Abp.Dependency;
using KonbiBrain.Common.Messages.Payment;
using KonbiCloud.Enums;
using Konbini.Messages.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.RFIDTable
{
    public interface IPaymentManager : IDeviceManager, ISingletonDependency
    {
        ConcurrentDictionary<PaymentTypes, string> AcceptedPaymentMethods { get; set; }
        PaymentTypes CurrentPaymentMode { get;  set; }
        event EventHandler<CommandEventArgs> DeviceFeedBack;
        Task<CommandState> ActivatePaymentAsync(TransactionInfo transaction);
        Task<CommandState> DeactivatePaymentAsync(TransactionInfo transaction);
        ConcurrentDictionary<PaymentTypes, string> ReloadAcceptedPayments();
    }
}
