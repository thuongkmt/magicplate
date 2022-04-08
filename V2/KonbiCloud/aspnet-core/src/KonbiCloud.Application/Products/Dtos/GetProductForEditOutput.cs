using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Products.Dtos
{
    public class GetProductForEditOutput
    {
        public CreateOrEditProductDto Product { get; set; }

        public string CategoryName { get; set; }
    }
}
