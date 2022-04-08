using System;
using System.Collections.Generic;
using System.Text;

namespace Konbi.WindowServices.QRPayment.Configuration
{
    public class AuthConfiguration
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string SigningKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
    }
}
