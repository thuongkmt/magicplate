using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbi.RealsenseID.Util
{
    public class Helper
    {
        /**
        * Encode/decode into base64 type
        * **/
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
        /**
        * Write data into file by date
        * **/
        public static void WriteToFile(string Message)
        {
            string path = @"C:\Logs";

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string filepath = @"C:\Logs\RealsenseID_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";

            DateTime saveNow = DateTime.Now;
            DateTime localTime;
            localTime = saveNow.ToLocalTime();

            if (!File.Exists(filepath))
            {
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine("[" + localTime + "] - " + Message);
                }
            }

            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine("[" + localTime + "] - " + Message);
                }
            }
        }

        /**
         * Convert image into byte array and opposite
         * **/
        public static byte[] ImageToByteArray(Image imageIn)
        {
            using (var ms = new MemoryStream())
            {
                imageIn.Save(ms, imageIn.RawFormat);
                return ms.ToArray();
            }
        }
        public static Image byteArrayToImage(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            Image returnImage = Image.FromStream(ms);
            return returnImage;
        }
    }
}
