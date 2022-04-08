using System;
using System.Collections.Generic;
using System.Linq;
using Abp.Application.Services.Dto;

namespace KonbiCloud.Products.Dtos
{
    public class ProductDto : FullAuditedEntityDto<Guid>
    {
        public ProductDto()
        {
            
        }
        public string SKU { get; set; }
        public string Name { get; set; }

        public int Status { get; set; }
        public string Unit { get; set; }
        public float Price { get; set; }
        public float? ContractorPrice { get; set; }
        public Guid? CategoryId { get; set; }
       
        public string ImageUrl { get; set; }        

        public int TenantId { get; set; }
        public string Barcode { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string ShortDesc1 { get; set; }
        public string ShortDesc2 { get; set; }
        public string ShortDesc3 { get; set; }
        public string Desc { get; set; }        

        public int? DisplayOrder { get; set; }
    }
}