using System;
using System.Collections.Generic;
using System.Text;

namespace Konbi.Hardware.NFCReader
{
    public interface INFCReader
    {
        /// <summary>
        /// Event when card is inserted. card number is returned in string
        /// </summary>
        Action<string> OnInsertedCard { get; set; }

        /// <summary>
        /// Get connected readers
        /// </summary>
        /// <returns></returns>
        string[] GetReaders();
        
    }
}
