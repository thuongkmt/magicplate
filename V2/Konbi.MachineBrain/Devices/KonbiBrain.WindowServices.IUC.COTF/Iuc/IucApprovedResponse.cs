using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.WindowServices.IUC.COTF.Iuc
{
    public class IucApprovedResponse : SaleResponse
    {
        public string Tid { get; set; }
        public string Mid { get; set; }
        public string DateTime { get; set; }
        public string Invoice { get; set; }
        public string Batch { get; set; }
        public string CardLabel { get; set; }
        public string CardNumber { get; set; }
        public string CardExpiryDate { get; set; }
        public string Rrn { get; set; }
        public string ApproveCode { get; set; }
        public string EntryMode { get; set; }
        public string AppLabel { get; set; }
        public string Aid { get; set; }

        public string Tc { get; set; }
        public new decimal Amount { get; set; }
        public string TransactionId { get; set; }
        public string FormattedAmount { get; set; }
        public string TransactionType { get; set; }
        public string Trace { get; set; }
        public string Timespan { get; set; }
    }
}
