using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Machine
{
    public class Machine : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public string id { get; set; }
        public string name { get; set; }
    }
    public class TaxSettingsDto
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public double Percentage { get; set; }
    }
    public class MachineSettingsDto
    {
        public TaxSettingsDto TaxSettings { get; set; }
    }
    public class MachineSync
    {
        public string name { get; set; }
        public int tenantId { get; set; }
        public bool isOffline { get; set; }

        public string cashlessTerminalId { get; set; }
        public bool registeredAzureIoT { get; set; }
        public int stopSalesAfter { get; set; }
        public int temperatureStopSales { get; set; }

        public bool isDeleted { get; set; }
        public int? deleterUserId { get; set; }
        public DateTime? deletionTime { get; set; }
        public DateTime? lastModificationTime { get; set; }
        public int? lastModifierUserId { get; set; }
        public DateTime creationTime { get; set; }
        public int creatorUserId { get; set; }
        public string id { get; set; }
        public MachineSettingsDto Settings { get; set; }
    }
}
