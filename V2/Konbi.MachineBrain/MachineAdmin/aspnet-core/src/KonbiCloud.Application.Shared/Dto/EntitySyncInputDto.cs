using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Dto
{
    public class EntitySyncInputDto<T>
    {
        public List<T> ModificationEntities { get; set; }
        public List<T> DeletionEntities { get; set; }
        public long LastSyncedTimeStamp { get; set; }
        public EntitySyncInputDto()
        {
            ModificationEntities = new List<T>();
            DeletionEntities = new List<T>();
        }
    }
}
