using Abp.Application.Services.Dto;
using KonbiCloud.Products.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Categories.Dtos
{
    public class CategoryDto : EntityDto<Guid>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public ICollection<ProductDto> Products { get; set; }
    }
}
