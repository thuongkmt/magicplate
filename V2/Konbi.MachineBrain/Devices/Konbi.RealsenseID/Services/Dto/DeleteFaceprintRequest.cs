using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbi.RealsenseID.Services.Dto
{
    public class DeleteFaceprintRequest
    {
        public string user_id = "";
        public string user_id_type = "username";// this value can be: username, email or ccw_id1
        public DeleteFaceprintRequest(string user_id)
        {
            this.user_id = user_id;
        }
    }
}
