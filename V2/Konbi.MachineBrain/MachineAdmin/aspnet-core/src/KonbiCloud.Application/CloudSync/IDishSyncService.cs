using KonbiCloud.Dto;
using KonbiCloud.Plate;
using KonbiCloud.Plate.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KonbiCloud.CloudSync
{
    public interface IDishSyncService : ICloudSyncService<Disc>
    {
        Task<EntitySyncInputDto<DiscSyncDto>> Sync(Guid machineId);
        Task<bool> UpdateSyncStatus(SyncedItemData<Guid> input);
        Task<bool> PushToServer(SyncedItemData<Disc> dishes);
    }
}
