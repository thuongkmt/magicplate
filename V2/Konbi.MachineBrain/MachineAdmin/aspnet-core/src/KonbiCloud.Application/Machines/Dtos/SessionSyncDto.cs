using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Machines.Dtos
{
    public class SessionSyncDto
    {
        public Guid Id { get; set; }
        public virtual string Name { get; set; }

        public virtual string FromHrs { get; set; }

        public virtual string ToHrs { get; set; }

        public bool ActiveFlg { get; set; }
    }
}
