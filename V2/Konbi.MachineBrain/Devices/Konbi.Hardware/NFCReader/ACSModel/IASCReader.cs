using System;
using System.Collections.Generic;
using System.Text;

namespace Konbi.Hardware.NFCReader.ACSModel
{
    public interface IASCReader: INFCReader
    {
        Action<string> OnInsertedCard { get; set; }
        Action OnRemovedCard { get; set; }
        Action OnScanningCard { get; set; }
        Action OnScannedCard { get; set; }
        Action OnError { get; set; }
        Action<bool> OnConnectedReader { get; set; }
    }
}
