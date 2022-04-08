using KonbiCloud.RFIDTable.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.RFIDTable.Cache
{
    public class TaxSettingsCacheItem
    {
        public static readonly string CacheName = "TaxSettingsCacheItem";

       public TaxSettingsDto TaxSettings { get; set; }
    }
}
