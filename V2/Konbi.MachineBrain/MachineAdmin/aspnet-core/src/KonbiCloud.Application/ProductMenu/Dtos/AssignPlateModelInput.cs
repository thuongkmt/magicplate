using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.ProductMenu.Dtos
{
    public class AssignPlateModelInput : EntityDto<Guid>
    {
        public Guid PlateId { get; set; }
    }
}
