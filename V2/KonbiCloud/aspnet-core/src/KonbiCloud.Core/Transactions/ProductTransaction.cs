using Abp.Domain.Entities.Auditing;
using KonbiCloud.Plate;
using KonbiCloud.Products;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace KonbiCloud.Transactions
{
    public class ProductTransaction : CreationAuditedEntity
    {
        public Guid? ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
        public virtual DetailTransaction Transaction { get; set; }
        public decimal Amount { get; set; }
       
        public Guid? DiscId { get; set; }
        [ForeignKey("DiscId")]
        public virtual Disc Disc { get; set; }
    }
}