using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.RFIDTable
{
    public class PlateReadingInput
    {
        public string UID;

        public string UType;

        public string UData { get; set; }
        public int CustomPrice { get {
                int price = -1;
                if(!string.IsNullOrEmpty(UData))
                    int.TryParse(UData, out price);
                return price;
            }
        }
        public bool HasCustomPrice
        {
            get
            {
                return CustomPrice >0;
            }
        }

       
    }

    public class ManualPaymentInput
    {
        public string PlateCode { get; set; }

        public Guid? ProductId { get; set; }

        public string ProductName { get; set; }

        public decimal Price { get; set; }
    }
}
