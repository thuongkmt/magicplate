using Abp.Application.Services.Dto;
using System;

namespace KonbiCloud.ProductMenu.Dtos
{
    public class PlateMenusInput : PagedAndSortedResultRequestDto
    {
		public string NameFilter { get; set; }

        public string CodeFilter { get; set; }

        public string CategoryFilter { get; set; }

		public string SKUFilter { get; set; }

		public DateTime? DateFilter { get; set; }

		public string SessionFilter { get; set; }

        public string Id { get; set; }
        public decimal Price { get; set; }

        public string PriceStrategyId { get; set; }
        public decimal PriceStrategy { get; set; }
        public Guid ProductId { get; set; }

        public int? DisplayOrder { get; set; }
    }
}