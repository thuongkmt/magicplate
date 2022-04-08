using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.WindowServices.AlarmLight.Services
{
    public class AlarmState
    {
        public bool Off { get; set; }
        public bool Green { get; set; }
        public bool Red { get; set; }
        public bool Beep { get; set; }
        public bool Blink { get; set; }
        public int BlinkDuration { get; set; }
        public int Duration { get; set; }
        public string SoundIntruction { get; set; }
    }
}
