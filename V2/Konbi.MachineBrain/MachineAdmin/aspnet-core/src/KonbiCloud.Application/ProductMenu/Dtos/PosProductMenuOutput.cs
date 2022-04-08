using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.ProductMenu.Dtos
{
    public class PosProductMenuOutput
    {
        public Guid? ProductId { get; set; }

        public string PlateCode { get; set; }

        public string ProductName { get; set; }

        public decimal? Price { get; set; }
    }
}
