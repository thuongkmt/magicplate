using KonbiCloud.Plate;
using KonbiCloud.Products;
using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.RFIDTable.Cache
{
    public class SaleSessionCacheItem
    {
        public static readonly string CacheName = "SaleSessionCacheItem";
        
        public SessionInfo SessionInfo { get; set; }

        public List<MenuItemInfo> MenuItems { get;  set; }

        public List<PlateInfo> PlateInfos { get; set; }

        public List<TrayInfo> Trays { get; set; }
        // obsoleted - unused
        //public List<Disc> Discs { get; set; }
    }
}
