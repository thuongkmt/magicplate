using System;
using System.Collections.Generic;
using System.Text;

namespace Konbini.Messages.Enums
{
    public class AlarmLightControl
    {
        public static string Off = "00";
        public static string Red = "11";
        public static string Green = "14";
        public static string Red_Green = "15";
        public static string Beep = "18";
        public static string Red_Beep = "19";
        public static string Green_Beep = "1C";
        public static string Red_Green_Beep = "1D";
    }
    public class SoundIntructionType
    {
        public static string Sample = "sample.wav";
    }
}
