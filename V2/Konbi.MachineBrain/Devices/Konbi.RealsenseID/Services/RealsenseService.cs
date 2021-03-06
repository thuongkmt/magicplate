using Konbi.RealsenseID.Services.Dto;
using Konbi.RealsenseID.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbi.RealsenseID
{
    public class RealsenseService
    {
        string apiServerAddress = "";
        public RealsenseService(string apiServerAddress)
        {
            this.apiServerAddress = apiServerAddress;
        }

        public List<UserDataResponse> GetListFaceprints(out string status)
        {
            var client = new RestClient(apiServerAddress + "/wp-json/wp/v2/magicplate-web/get-faceprint-data-list");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            Helper.WriteToFile($"Start to call GetListFaceprints API");

            IRestResponse response = client.Execute(request);
            if (!response.IsSuccessful)
            {
                Helper.WriteToFile($"Call GetListFaceprints API Failed {response.ToString()}");
                status = response.StatusCode.ToString();
                return null;
            }
            //WriteToFile(response.Content);
            var userData = JsonConvert.DeserializeObject<List<UserDataResponse>>(response.Content);
            Helper.WriteToFile($"End to call GetListFaceprints API");
            if (userData.Count > 0)
            {
                //WriteToFile("userData" + JsonConvert.SerializeObject(userData));
                status = "";
                return userData;
            }
            else
            {
                status = "";
                return null;
            }
            
        }

        public bool EnrollFaceprint(EnrollFaceprintRequest enrollRequest, out string status)
        {
            var client = new RestClient(apiServerAddress + "/wp-json/wp/v2/magicplate-web/faceprint-enrollment");
            client.Timeout = 40 * 1000;
            var request = new RestRequest(Method.POST);

            JObject enrollReq = new JObject();
            enrollReq.Add("user_id", enrollRequest.user_id);
            enrollReq.Add("user_id_type", enrollRequest.user_id_type);
            enrollReq.Add("faceprint", enrollRequest.faceprint);// this is need to be encoded befor pass into
            enrollReq.Add("is_override_old_faceprint", enrollRequest.is_override_old_faceprint);//allow override the old value if faceprint is existed

            Helper.WriteToFile($"EnrollFaceprint - request: {enrollReq}");
            string enrollReqJson = JsonConvert.SerializeObject(enrollReq);
            request.AddParameter("application/json; charset=utf-8", enrollReqJson, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            if (!response.IsSuccessful)
            {
                Helper.WriteToFile($"EnrollFaceprint - responsse: Failed {response.ToString()}");
                Helper.WriteToFile($"EnrollFaceprint - response.StatusCode: Failed {response.StatusCode.ToString()}");
                status = response.StatusCode.ToString();
                return false;
            }
            Helper.WriteToFile($"EnrollFaceprint - responsse: content {response.Content}");
            var enrollRes = JsonConvert.DeserializeObject<CommonResponse>(response.Content);
            if (enrollRes.result == "success")
            {
                status = "";
                return true;
            }
            status = "";
            return false;
        }

        public bool DeleteFaceprint(DeleteFaceprintRequest deleteRequest)
        {
            var client = new RestClient(apiServerAddress + "/wp-json/wp/v2/magicplate-web/faceprint-delete");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);

            JObject deleteReq = new JObject();
            deleteReq.Add("user_id", deleteRequest.user_id);
            deleteReq.Add("user_id_type", deleteRequest.user_id_type);
            Helper.WriteToFile($"DeleteFaceprint - request: {deleteReq}");

            string deleteReqJson = JsonConvert.SerializeObject(deleteReq);
            request.AddParameter("application/json; charset=utf-8", deleteReqJson, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            if (!response.IsSuccessful)
            {
                Helper.WriteToFile($"DeleteFaceprint - responsse: Failed {response}");
                return false;
            }
            Helper.WriteToFile($"DeleteFaceprint - responsse: content {response.Content}");
            var deleteRes = JsonConvert.DeserializeObject<CommonResponse>(response.Content);
            if (deleteRes.result == "success")
            {
                return true;
            }
            return false;
        }
    }
}
