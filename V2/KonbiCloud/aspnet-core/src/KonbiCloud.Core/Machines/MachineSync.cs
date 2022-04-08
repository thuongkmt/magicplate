using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace KonbiCloud.Machines
{
    public class MachineSync : FullAuditedEntity<Guid>, IMayHaveTenant
    {

        public int? TenantId { get; set; }
        public string id { get; set; }
        public string name { get; set; }

    }

}
