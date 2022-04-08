using System;

namespace KonbiCloud.Plate.Dtos
{
    public class CheckInventoryDto
    {
		public string UID { get; set;}
    }
    public class PlateCheckInventoryDto
    {
        public Guid PlateId { get; set; }
        public string PlateName { get; set; }

        public string PlateCode { get; set; }
    }
}