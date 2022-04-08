using System;
using System.Collections.Generic;
using System.Text;

namespace Konbi.WindowServices.QRPayment.Services.Core
{
    public static class FomoResponseCodes
    {
        public static Dictionary<string, string> SALE_RESPONSES = new Dictionary<string, string>(new[] {
            new KeyValuePair<string, string>("00", "Request completed successfully"),
            new KeyValuePair<string, string>("03", "Invalid merchant"),
            new KeyValuePair<string, string>("05", "Do not honor"),
            new KeyValuePair<string, string>("06", "Unsupported condition code"),
            new KeyValuePair<string, string>("09", "Request in progress"),
            new KeyValuePair<string, string>("25", "Duplicate record"),
            new KeyValuePair<string, string>("30", "Format error"),
            new KeyValuePair<string, string>("96", "System malfunction"),
        });
        public static Dictionary<string, string> QUERY_RESPONSES = new Dictionary<string, string>(new[] {
             new KeyValuePair<string, string>("00", "Payment success"),
            new KeyValuePair<string, string>("03", "Invalid merchant"),
             new KeyValuePair<string, string>("05", "Payment error"),
            new KeyValuePair<string, string>("06", "Payment closed"),
             new KeyValuePair<string, string>("09", "Payment in progress"),
             new KeyValuePair<string, string>("12", "Invalid transaction"),
             new KeyValuePair<string, string>("21", "Payment void"),
             new KeyValuePair<string, string>("22", "Payment reversed"),
             new KeyValuePair<string, string>("23", "Payment cancelled"),
             new KeyValuePair<string, string>("30", "Format error"),
             new KeyValuePair<string, string>("96", "System malfunction")
        });
    }
}
