using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Products.Dtos
{
    public class GetAllProductsInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }

        public string NameFilter { get; set; }

        public string BarcodeFilter { get; set; }

        public string SKUFilter { get; set; }

        public string CategoryNameFilter { get; set; }
    }
}
