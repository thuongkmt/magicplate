using KonbiBrain.Common.Messages;
using KonbiBrain.Common.Messages.Payment;
using System;
using System.Collections.Generic;
using System.Text;

namespace Konbini.Messages.Payment
{
    public class NsqDisablePaymentCommand: NsqPaymentCommandBase
    {
        
        public NsqDisablePaymentCommand()
        {
            Command = UniversalCommandConstants.DisablePaymentCommand;
        }
        public NsqDisablePaymentCommand(string paymentType) : base(paymentType)
        {
            Command = UniversalCommandConstants.DisablePaymentCommand;
        }
        public NsqDisablePaymentCommand(string paymentType, Guid transactionId) : base(paymentType)
        {
            Command = UniversalCommandConstants.DisablePaymentCommand;
            TransactionId = transactionId;            
        }
    }
}
