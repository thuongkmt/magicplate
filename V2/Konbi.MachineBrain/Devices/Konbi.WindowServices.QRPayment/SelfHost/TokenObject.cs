using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Konbi.WindowServices.QRPayment.SelfHost
{
    public class TokenObject
    {
        [JsonProperty("access_token")]
        public string Token { get; set; }
        [JsonProperty("token_type")]
        public string TokenType { get; set; }
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
