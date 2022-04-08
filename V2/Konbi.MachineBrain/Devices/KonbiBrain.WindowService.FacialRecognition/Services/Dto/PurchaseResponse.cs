using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.WindowService.FacialRecognition.Services
{
    public class PurchaseResponse
    {
        public string result = "";
        public string message = "";
        public string txn_description = "";
        public long txn_id = 0;
        public double balance = 0.0;
    }
}
