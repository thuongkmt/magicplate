using System.Collections.Generic;
using KonbiBrain.Common.Messages;

namespace Konbini.Messages.Commands
{
    public class PongCommand : UniversalCommands<PongCommandPayload>
    {
        private const string command = UniversalCommandConstants.Pong;
        public PongCommand():base(command)
        {
            CommandObject = new PongCommandPayload();            
        }
    }
    public class PongCommandPayload
    {
        public bool HasError { get; set; }
        public string Message { get; set; }
    }
}
