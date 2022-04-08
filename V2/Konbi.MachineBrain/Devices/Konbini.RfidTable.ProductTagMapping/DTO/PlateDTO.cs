using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using static Konbini.RfidFridge.TagManagement.DTO.TrayResult;

namespace Konbini.RfidFridge.TagManagement.DTO
{
    public class PlateDTO
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonIgnore]
        public string DisplayText
        {
            get
            {
                return $"{Name}-{Code}";
            }
        }
    }

    public class TrayResult : BaseAbpDTO<GetPlateForView>
    {
        public class GetPlateForView
        {
            [JsonProperty("plate")]
            public PlateDTO Plate { get; set; }

            [JsonProperty("plateCategoryName")]
            public string PlateCategoryName { get; set; }
        }
    }
}
