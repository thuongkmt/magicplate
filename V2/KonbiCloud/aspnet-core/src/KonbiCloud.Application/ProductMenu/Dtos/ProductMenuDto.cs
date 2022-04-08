using System;
using KonbiCloud.Machines;
using KonbiCloud.Products.Dtos;
using KonbiCloud.Sessions;

namespace KonbiCloud.ProductMenu.Dtos
{
    public class ProductMenuDto
    {
        public string Id { get; set; }
        public Plate.Plate Plate { get; set; }
        public string PlateCode { get; set; }
        public ProductDto Product { get; set; }
        public Guid? ProductId { get; set; }
        public string CategoryName { get; set; }
        public decimal? Price { get; set; }
        public DateTime? SelectedDate { get; set; }
        //public string PriceStrategyId { get; set; }
        public decimal? PriceStrategy { get; set; }
        public virtual Session Session { get; set; }
        public string SessionName { get; set; }
        public int? DisplayOrder { get; set; }
    }
}