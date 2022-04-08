﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.Common.Attributes
{
    public class CloudFieldAttribute:Attribute
    {
        public CloudFieldAttribute()
        {
            
        }

        public CloudFieldAttribute(string fieldName)
        {
            FieldName = fieldName;
        }
        public string FieldName { get; set; }
    }
}
