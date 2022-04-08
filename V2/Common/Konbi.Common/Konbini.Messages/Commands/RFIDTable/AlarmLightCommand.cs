using KonbiBrain.Common.Messages;
using Konbini.Messages.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Konbini.Messages.Commands.RFIDTable
{
    public class AlarmLightCommand : UniversalCommands<AlarmLightPayload>
    {
        public AlarmLightCommand() : base(UniversalCommandConstants.RfidTableAlarmLightControl)
        {
            CommandObject = new AlarmLightPayload();
        }       
        public bool Off { get; set; }
        public bool Green { get; set; }
        public bool Red { get; set; }
        public bool Beep { get; set; }
        public bool Blink { get; set; }
        public int Duration { get; set; }
        public string SoundIntruction { get; set; }
    }

    public class AlarmLightPayload
    {
        public string AlarmLightControl { get; set; }
    }
}
