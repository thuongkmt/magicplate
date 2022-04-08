using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Products.Dtos
{
    public class CreateOrEditProductDto : EntityDto<Guid?>
    {
        public string Name { get; set; }

        public string ImageUrl { get; set; }

        public string Desc { get; set; }

        public Guid? CategoryId { get; set; }

       

        public string CategoryName { get; set; }

       

        public float Price { get; set; }

        public float? ContractorPrice { get; set; }

        public string Barcode { get; set; }

        public string SKU { get; set; }
        public bool AutoGenerateSKU { get; set; }

        public int? DisplayOrder { get; set; }
    }
}
