using KonbiBrain.Common.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Konbini.Messages.Commands
{
    public class BarcodeScannerCommand: UniversalCommands<BarcodeScannerCommandPayload>
    {
        private const string command = UniversalCommandConstants.BarcodeScannerCommand;
        public BarcodeScannerCommand() : base(command)
        {
            CommandObject = new BarcodeScannerCommandPayload() { BarcodeValue = string.Empty };
        }
    }

    public class BarcodeScannerCommandPayload
    {
        public string BarcodeValue { get; set; }
    }
}
