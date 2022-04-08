using IucBrain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.WindowServices.IUC.Iuc
{
    public class ResponseBase
    {
        public ResponseBase(IucPaymentMode mode)
        {
            PaymentMode = mode;
        }
        IucPaymentMode PaymentMode { get; set; }
        public string ResponseCode { get; set; }
        public string Message { get; set; }
    }
}
