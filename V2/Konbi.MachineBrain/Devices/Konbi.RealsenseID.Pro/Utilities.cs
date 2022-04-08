﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace Konbi.RealsenseID.Pro
{
    public class DeviceEnumerator
    {
        public List<rsid.SerialConfig> Enumerate()
        {
            var results = new List<rsid.SerialConfig>();

            using (var searcher = new ManagementObjectSearcher("Select * From Win32_SerialPort"))
            {
                foreach (ManagementObject query in searcher.Get())
                {
                    try
                    {
                        var deviceId = query[PnpDeviceIdField].ToString();

                        var vidIndex = deviceId.IndexOf(VidField);
                        var pidIndex = deviceId.IndexOf(PidField);

                        if (vidIndex == -1 || pidIndex == -1)
                            continue;

                        var vid = string.Empty;
                        var pid = string.Empty;

                        // extract com port
                        var serialPort = query[DeviceIdField].ToString();

                        var vidStart = deviceId.Substring(vidIndex + VidField.Length);
                        vid = vidStart.Substring(0, VidLength);

                        string pidStart = deviceId.Substring(pidIndex + PidField.Length);
                        pid = pidStart.Substring(0, PidLength);

                        // use vid and pid to decide if it is connected to device's usb or device's uart 
                        foreach (var id in DeviceIds)
                        {
                            if (vid == id.vid && pid == id.pid)
                                results.Add(new rsid.SerialConfig { port = serialPort });
                        }
                    }
                    catch (ManagementException)
                    {
                    }
                }
            }

            return results;
        }

        private struct AnnotatedSerialPort
        {
            public string vid;
            public string pid;
        };

        private static readonly string VidField = "VID_";
        private static readonly string PidField = "PID_";
        private static readonly string DeviceIdField = "DeviceID";
        private static readonly string PnpDeviceIdField = "PNPDeviceID";
        private static readonly int VidLength = 4;
        private static readonly int PidLength = 4;
        private static readonly List<AnnotatedSerialPort> DeviceIds = new List<AnnotatedSerialPort> {
                new AnnotatedSerialPort{ vid = "04D8", pid = "00DD" },
                new AnnotatedSerialPort{ vid = "2AAD", pid = "6373" },
            };
    }
}
