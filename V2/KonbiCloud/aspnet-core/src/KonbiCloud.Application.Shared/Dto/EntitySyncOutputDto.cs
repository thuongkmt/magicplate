using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Dto
{
    public class EntitySyncOutputDto<T>
    {
        public List<T> ModificationEntities { get; set; }
        public List<T> DeletionEntities { get; set; }
        public long LastSyncedTimeStamp { get; set; }
        public EntitySyncOutputDto()
        {
            ModificationEntities = new List<T>();
            DeletionEntities = new List<T>();
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            LastSyncedTimeStamp = Convert.ToInt64((DateTime.Now.ToUniversalTime() - epoch).TotalSeconds);            
        }   
    }
}
