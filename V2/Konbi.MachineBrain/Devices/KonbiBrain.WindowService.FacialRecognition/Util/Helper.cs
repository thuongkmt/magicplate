using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.WindowService.FacialRecognition.Util
{
    public class Helper
    {
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
        public static void WriteToFile(string Message)
        {
            SeriLogService.LogInfo(Message);
            //string path = @"C:\Logs";

            //if (!Directory.Exists(path))
            //{
            //    Directory.CreateDirectory(path);
            //}

            //string filepath = @"C:\Logs\FacialRecognition_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";

            //DateTime saveNow = DateTime.Now;
            //DateTime localTime;
            //localTime = saveNow.ToLocalTime();

            //if (!File.Exists(filepath))
            //{
            //    using (StreamWriter sw = File.CreateText(filepath))
            //    {
            //        sw.WriteLine("[" + localTime + "] - " + Message);
            //    }
            //}

            //else
            //{
            //    using (StreamWriter sw = File.AppendText(filepath))
            //    {
            //        sw.WriteLine("[" + localTime + "] - " + Message);
            //    }
            //}
        }
    }
}
