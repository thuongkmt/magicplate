using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace KonbiCloud.Machines
{
    public class Machine : FullAuditedEntity<Guid>, IMayHaveTenant
    {
     
        public string Name { get; set; }
        public int? TenantId { get; set; }
        public bool IsOffline { get; set; }


        public string CashlessTerminalId  { get; set; }
        public bool RegisteredAzureIoT { get; set; }
        public int StopSalesAfter { get; set; }
        public int TemperatureStopSales { get; set; }

    }

}
