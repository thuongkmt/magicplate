using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.WindowService.FacialRecognition.Services
{
    public class UserDataResponse
    {
        public string username = "";//this is default
        public string email = "";//not use
        public string ccw_id1 = "";// not use
        public string ccw_id2 = ""; //this is faceprint is encrypted by base64, need to decaode to get the raw data
    }
}
