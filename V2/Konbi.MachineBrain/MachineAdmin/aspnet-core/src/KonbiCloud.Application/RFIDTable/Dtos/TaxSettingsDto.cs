using KonbiCloud.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.RFIDTable.Dtos
{
   public  class TaxSettingsDto
    {
        public string Name { get; set; }
        public TaxType Type { get; set; }
        public decimal Percentage { get; set; }
    }
}
