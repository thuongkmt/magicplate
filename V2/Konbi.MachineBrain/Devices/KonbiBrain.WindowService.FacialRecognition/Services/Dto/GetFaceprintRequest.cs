using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.WindowService.FacialRecognition.Services
{
    public class GetFaceprintRequest
    {
        string card_id = "";

        public GetFaceprintRequest(string card_id)
        {
            this.card_id = card_id;
        }
    }
}
