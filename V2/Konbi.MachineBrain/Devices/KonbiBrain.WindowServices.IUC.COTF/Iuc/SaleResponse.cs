using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.WindowServices.IUC.COTF.Iuc
{
    public class SaleResponse : ResponseBase
    {
        public int Amount { get; set; }

        public override string ToString()
        {
            return $"[{ResponseCode}] {Message}";
        }
    }
}
