using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Transactions.Dtos
{   
    public class GetAllTransactionsForExcelInput 
    {
        public string SessionFilter { get; set; }

        public string StateFilter { get; set; }

        //type 0: error, type 1: success
        public int TransactionType { get; set; }

        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public Guid? MachineFilter { get; set; }
        public string Sorting { get; set; }
    }
}
