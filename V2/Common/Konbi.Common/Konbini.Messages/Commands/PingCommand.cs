using System.Collections.Generic;
using KonbiBrain.Common.Messages;

namespace Konbini.Messages.Commands
{
    public class PingCommand : UniversalCommands<bool>
    {
        private const string command = UniversalCommandConstants.Ping;
        public PingCommand():base(command)
        {
            CommandObject = true;            
        }
    }
}
