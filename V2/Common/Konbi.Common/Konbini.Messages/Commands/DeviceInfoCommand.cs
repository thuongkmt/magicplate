using System.Collections.Generic;
using KonbiBrain.Common.Messages;

namespace Konbini.Messages.Commands
{
    public class DeviceInfoCommand : UniversalCommands<DeviceInfoCommandPayload>
    {
        private const string command = UniversalCommandConstants.DeviceInfo;
        public DeviceInfoCommand():base(command)
        {
            CommandObject = new DeviceInfoCommandPayload();
            
        }
    }
    public class DeviceInfoCommandPayload
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool HasError { get; set; }
        public List<string> Errors { get; set; }
        public DeviceInfoCommandPayload()
        {
            Errors = new List<string>();
        }
    }

}
