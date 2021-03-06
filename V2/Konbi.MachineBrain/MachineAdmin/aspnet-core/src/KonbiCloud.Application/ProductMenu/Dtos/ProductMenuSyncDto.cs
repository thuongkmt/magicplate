using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.ProductMenu.Dtos
{
    public class ProductMenuSyncDto
    {
        public Guid Id { get; set; }

        public Guid ProductId { get; set; }
        public Guid? PlateId { get; set; }
        public decimal Price { get; set; }
        public DateTime SelectedDate { get; set; }

        public decimal PriceStrategy { get; set; }
        public Guid SessionId { get; set; }

        public int DisplayOrder { get; set; }
    }
}
