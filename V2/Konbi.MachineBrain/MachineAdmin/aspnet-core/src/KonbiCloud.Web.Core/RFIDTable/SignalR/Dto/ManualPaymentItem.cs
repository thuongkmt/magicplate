using KonbiCloud.RFIDTable;
using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Web.RFIDTable.SignalR.Dto
{
    public class ManualPaymentDto
    {
        public string ItemName { get; set; }

        public decimal ItemAmount { get; set; }

        public PlateInfo Plate { get; set; }
    }
}
