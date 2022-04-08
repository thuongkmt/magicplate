using KonbiCloud.Dto;
using KonbiCloud.ProductMenu.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KonbiCloud.CloudSync
{
    public interface IPlateMenuSyncService
    {
        Task<EntitySyncInputDto<ProductMenuSyncDto>> Sync(Guid machineId);
        //Task<bool> UpdateSyncStatus(SyncedItemData<Guid> input);
    }
}
