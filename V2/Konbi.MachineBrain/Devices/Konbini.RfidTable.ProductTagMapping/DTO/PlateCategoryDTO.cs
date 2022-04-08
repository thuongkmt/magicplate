using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Konbini.RfidFridge.TagManagement.DTO
{
    public class PlateCategoryDTO : BaseAbpDTO<PlateCategoryDTO.Item>
    {
        public class Item
        {
            [JsonProperty("plateCategory")]
            public PlateCategory Category { get; set; }
        }

        public class PlateCategory
        {
            [JsonProperty("id")]
            public long Id { get; set; }
            [JsonProperty("name")]
            public string Name { get; set; }
            [JsonProperty("plates")]
            public IList<PlateDTO> Plates { get; set; }
        }

    }


   
}
