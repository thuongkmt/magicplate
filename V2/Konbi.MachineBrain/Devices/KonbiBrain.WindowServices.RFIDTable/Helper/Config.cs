using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.WindowServices.RFIDTable.Helper
{
    public class Config
    {
        /// <summary>
        /// COM串口名称
        /// </summary>
        /// <returns></returns>
        public static string COM()
        {
            try
            {
                return ConfigurationSettings.AppSettings["COM"].ToString();
            }
            catch
            {
                return "";
            }
        }



    }
}
