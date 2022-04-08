using KonbiCloud.Products;
using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.RFIDTable
{
    public class MenuItemInfo
    {
        public string Name { get; set; }

        public string ImageUrl { get; set; }

        public string Desc { get; set; }

        public string Code { get; set; }       

        public string Color { get; set; }

        public decimal Price { get;  set; }

        public decimal PriceContractor { get; set; }

        public Guid PlateId { get; set; }

        public Guid ProductId { get; set; }

        public string ProductName { get; set; }

        public PlateInfo Plate { get; set; }

        public Product Product { get; set; }

        public Guid UId { get; set; }

        public string BarCode { get; set; }
    }
}
