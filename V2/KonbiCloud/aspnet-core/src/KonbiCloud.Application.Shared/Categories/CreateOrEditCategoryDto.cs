using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace KonbiCloud.Categories
{
    public class CreateOrEditCategoryDto : EntityDto<Guid?>
    {
        [Required]
        public string Name { get; set; }


        public string Description { get; set; }
    }
}
