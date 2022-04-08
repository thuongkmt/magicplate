using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Plate.Dtos
{
    public class DiscSyncDto
    {
        public Guid Id { get; set; }

        public string Code { get; set; }
        public string Uid { get; set; }

        public Guid PlateId { get; set; }

    }
}
