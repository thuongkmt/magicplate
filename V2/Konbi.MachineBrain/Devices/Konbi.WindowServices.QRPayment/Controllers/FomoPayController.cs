using Autofac;
using Konbi.WindowServices.QRPayment.Services.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using static Konbi.WindowServices.QRPayment.DTO.WebApiResponse;

namespace Konbi.WindowServices.QRPayment.Controllers
{

    [Authorize]
    [ApiExplorerSettings(GroupName = "fomo")]
    [Route("api/fomo/")]

    public class FomoPayController : ControllerBase
    {
        private LogService LogService;
        private FomoService FomoService;
        public FomoPayController()
        {
            LogService = AutofacConfig.CurrentContainer.Resolve<LogService>();
            FomoService= AutofacConfig.CurrentContainer.Resolve<FomoService>();
        }

        [HttpGet]
        [Route("ping")]

        public IActionResult Ping()
        {
            return Ok();
        }
    }
}
