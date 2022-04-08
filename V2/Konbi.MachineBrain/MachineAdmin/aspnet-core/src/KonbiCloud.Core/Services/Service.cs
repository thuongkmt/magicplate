using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace KonbiCloud.Services
{
    [Table("Services")]
    public class Service : FullAuditedEntity<int>, IMayHaveTenant
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool IsArchived { get; set; }
        public bool IsError { get; set; }
        public DateTime? ErrotAt { get; set; }
        public string ErrorMessage { get; set; }

        public int? TenantId { get; set; }
    }
}
