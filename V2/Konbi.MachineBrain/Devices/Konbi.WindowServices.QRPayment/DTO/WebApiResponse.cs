using System;
using System.Collections.Generic;
using System.Text;

namespace Konbi.WindowServices.QRPayment.DTO
{
    public class WebApiResponse
    {

        public class Error
        {
            public string error { get; set; }
            public string detail { get; set; }
        }

        public class Rejected
        {
            public string rejected { get; set; }
        }

        public class WebApiTransCompletionResult
        {
            public int? transactionId { get; set; }
            public int quantum { get; set; }
            public int collected { get; set; }
            public int balance { get; set; }
            public bool isCancelled { get; set; }
        }

        public class WebApiCashlessTransCompletionResult : WebApiTransCompletionResult
        {
            public object cashlessInfomation { get; set; }
        }

        public class WebApiDepositResult
        {
            public int id { get; set; }
            public int? transactionId { get; set; }
            public int target { get; set; }
            public int collected { get; set; }
            public int denomination { get; set; }
        }

        public class WebApiBoolResult
        {
            public WebApiBoolResult(bool value)
            {
                result = value;
            }

            public bool result { get; set; }
        }

        public class WebApiResponseResult
        {
            public bool success = false;
            public List<ResponseError> errors = new List<ResponseError>();
            public Dictionary<string, object> successProps = new Dictionary<string, object>();

            public WebApiResponseResult(bool success)
            {
                this.success = success;
            }

            public void addSuccessProp(string key, object o = null)
            {
                successProps.Add(key, o);
            }

            public void addError(string error, string errorDescription = "")
            {
                errors.Add(new ResponseError(error, errorDescription));
            }

            public class ResponseError
            {
                public string error;
                public string errorDescription;

                public ResponseError(string error, string errorDescription)
                {
                    this.error = error;
                    this.errorDescription = errorDescription;
                }
            }
        }

        public class CashlessResponseResult
        {
            public int id { get; set; }
            public int? transactionId { get; set; }
            public int type { get; set; }
            public string code { get; set; }
            public dynamic message { get; set; }
        }

        public class CashlessApprovedResponse
        {
            public string Tid { get; set; }
            public string Mid { get; set; }
            public string DateTime { get; set; }
            public string Invoice { get; set; }
            public string Batch { get; set; }
            public string CardLabel { get; set; }
            public string CardNumber { get; set; }
            public string Rrn { get; set; }
            public string ApproveCode { get; set; }
            public string EntryMode { get; set; }
            public string AppLabel { get; set; }
            public string Aid { get; set; }
            //public string TvrTsi { get; set; }
            public string Tc { get; set; }
            public decimal Amount { get; set; }
        }
    }
}
