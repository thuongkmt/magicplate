using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Enums
{
    public enum PaymentType
    {
        MdbCashless,
        Cyklone,
        IucApi,
        Iuc_CEPAS = 103,
        Iuc_Contactless = 104,
        Cash = 105,
        QR_DASH = 106,
        OVERRIDE = 107,
    }
}
