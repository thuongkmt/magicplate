using KonbiCloud.Dto;
using KonbiCloud.Plate;
using KonbiCloud.Plate.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KonbiCloud.CloudSync
{
    public interface IPlateCategorySyncService
    {
        Task<EntitySyncInputDto<PlateCategorySyncDto>> Sync(Guid mId);
    }
}
