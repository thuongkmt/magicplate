using KonbiCloud.Dto;
using KonbiCloud.Products;
using KonbiCloud.Products.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.CloudSync
{
    public interface IProductSyncService
    {
        Task<EntitySyncInputDto<ProductSyncDto>> Sync(Guid machineId);
    }
}
