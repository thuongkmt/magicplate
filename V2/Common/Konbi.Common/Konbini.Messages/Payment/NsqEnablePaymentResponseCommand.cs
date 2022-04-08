using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.Common.Messages.Payment
{
    public class NsqEnablePaymentResponseCommand :NsqPaymentCommandBase
    {
        public int Code { get; set; }
        public dynamic CustomData { get; set; }
        public NsqEnablePaymentResponseCommand(string paymentType) : base(paymentType)
        {
            Command = UniversalCommandConstants.EnablePaymentCommand;
            CustomData = new ExpandoObject();


        }
    }
}
