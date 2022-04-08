using System;
using System.Collections.Generic;
using System.Text;
using Konbini.Messages.Enums;

namespace Konbi.WindowServices.QRPayment.Services.Core.Fomo
{
    public class TransactionInfo
    {
        public int Amount { get; set; }
        public string Tid { get; set; }
        public string Mid { get; set; }
        public string OrderId { get; set; }
        public string Description { get; set; }
        public PaymentTypes PaymentType { get; internal set; }
    }
}
