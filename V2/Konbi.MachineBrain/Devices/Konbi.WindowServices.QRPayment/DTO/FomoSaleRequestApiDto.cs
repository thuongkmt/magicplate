using Konbi.WindowServices.QRPayment.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using static Konbi.WindowServices.QRPayment.Services.Core.FomoService;

namespace Konbi.WindowServices.QRPayment.DTO
{
    public class FomoSaleRequestApiDto
    {
        public ConditionCodes conditionCode;
        public int amount;
        public string orderNumber;
        public string description;
    }
}
