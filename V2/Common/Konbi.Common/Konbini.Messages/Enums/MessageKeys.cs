using System;
using System.Collections.Generic;
using System.Text;

namespace Konbini.Messages.Enums
{
    public enum MessageKeys
    {
        TestKey=1,
        Session=100,
        Tray=101,
        Plate=102,
        PlateCategory=103,
        MenuScheduler=104,
        Inventory=105,
        // TrungPQ add send message for auto sync takeAway.
        TakeAway = 106,
        ProductCategory = 107,
        Product = 108,
        Settings = 109
    }
}
