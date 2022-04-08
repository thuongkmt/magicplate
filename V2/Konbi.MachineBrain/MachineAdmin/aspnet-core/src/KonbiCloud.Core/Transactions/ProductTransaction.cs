using Abp.Domain.Entities.Auditing;
using KonbiCloud.Plate;
using KonbiCloud.Products;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace KonbiCloud.Transactions
{
    public class ProductTransaction : CreationAuditedEntity
    {
        public virtual Product Product { get; set; }
        public virtual DetailTransaction Transaction { get; set; }
        public decimal Amount { get; set; }
        public decimal DiscountAmount { get; set; }

        public Guid? DiscId { get; set; }
        [ForeignKey("DiscId")]
        public virtual Disc Disc { get; set; }
    }
}