using KonbiCloud.Dto;
using KonbiCloud.Products;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KonbiCloud.CloudSync
{
    public interface IProductCategorySyncService
    {
        Task<EntitySyncInputDto<CategorySyncDto>> Sync(Guid mId);
    }
}
