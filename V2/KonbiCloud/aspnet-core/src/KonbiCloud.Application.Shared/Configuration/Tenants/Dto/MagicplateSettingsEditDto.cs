using System;
using System.Collections.Generic;
using System.Text;


namespace KonbiCloud.Configuration.Tenants.Dto
{
    public class MagicplateSettingsEditDto
    {       
        public TaxSettingsEditDto TaxSettings { get; set; }
    }
    public class TaxSettingsEditDto
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public double Percentage { get; set; }
    }
}
