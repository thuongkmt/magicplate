using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.WindowService.FacialRecognition.Services
{
    public class EnrollFaceprintRequest
    {
        public string user_id = "";
        public string user_id_type = "username";// this value can be: username, email or ccw_id1
        public string faceprint = ""; //this is faceprint is encrypted by base64
        public bool is_override_old_faceprint = false; //if the old exist, need to override this old value then set it to true

        public EnrollFaceprintRequest(string user_id, string faceprint, bool is_override_old_faceprint)
        {
            this.user_id = user_id;
            this.faceprint = faceprint;
            this.is_override_old_faceprint = is_override_old_faceprint;
        }
    }
}
