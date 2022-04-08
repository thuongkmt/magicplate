using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Dto
{
    public class EntitySyncInputDto<T>: EntityDto<T>
    {
        public long LastSyncedTimeStamp { get; set; }
        public DateTime LastSynced
        {
            get
            {
                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                return epoch.AddSeconds(LastSyncedTimeStamp).ToLocalTime();
            }
        }
    }
}
