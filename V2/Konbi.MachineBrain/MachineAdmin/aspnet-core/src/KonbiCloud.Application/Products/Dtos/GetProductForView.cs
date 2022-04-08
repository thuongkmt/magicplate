using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Products.Dtos
{
    public class GetProductForView
    {
        public ProductDto Product { get; set; }

        public string CategoryName { get; set; }
    }
}
