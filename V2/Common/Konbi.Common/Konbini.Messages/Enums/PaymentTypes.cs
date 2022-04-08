using System;
using System.Collections.Generic;
using System.Text;

namespace Konbini.Messages.Enums
{
    public enum PaymentTypes
    {
        Mdb_CASHLESS = 100,
        Cyklone = 101,
        IUC_API = 102,
        IUC_CEPAS = 103,
        IUC_CONTACTLESS = 104,
        CASH = 105,
        QR_DASH = 106,
        OVERRIDE = 107,
        KONBI_CREDITS = 108,
        FACIAL_RECOGNITION = 109
    }
}
