using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.ProductMenu.Dtos
{
    public class PosProductMenuInput
    {
        public Guid CategoryId { get; set; }

        public Guid SessionId { get; set; }
    }
}
