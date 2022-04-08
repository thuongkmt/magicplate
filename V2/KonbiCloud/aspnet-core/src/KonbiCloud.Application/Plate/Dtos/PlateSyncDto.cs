using KonbiCloud.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Plate.Dtos
{
    public class PlateSyncDto
    {
        public Guid Id { get; set; }
        public  string Name { get; set; }

        public  string ImageUrl { get; set; }

        public  string Desc { get; set; }

        
        public  string Code { get; set; }

        public  int Avaiable { get; set; }

        public  string Color { get; set; }


        public  int? PlateCategoryId { get; set; }
        public PlateType Type { get; set; }
    }
}
