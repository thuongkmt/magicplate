using Konbi.Hardware.NFCReader.ACSModel;
using KonbiBrain.Common.Messages;
using KonbiBrain.Common.Messages.Payment;
using KonbiBrain.Common.Services;
using KonbiBrain.Messages;
using Konbini.Messages.Enums;
using Konbini.Messages.Payment;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NsqSharp;
using RestSharp;
using System;
using System.IO;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
using Konbi.Common.Interfaces;

namespace KonbiBrain.WindowServices.KonbiCredits
{
    public partial class KonbiCreditsService : ServiceBase, INsqHandler
    {
        private readonly NsqMessageProducerService nsqService;
        private NsqMessageConsumerService _nsqConsumer;
        private readonly string paymentType = PaymentTypes.KONBI_CREDITS.ToString();
        private String cardID = "";
        private String paymentToken = "";
        private String paymentAmount = "";
        private String paymentTransactionID = "";
        private String konbiCreditBalance = "";
        private String APIServerAddress = "";
        private String APIClientID = "";
        private String APIClientSecret = "";
        private int isSufficientCredit = 1;
        private bool isEnabled = false;
        private decimal requestedAmount = 0;

        public KonbiCreditsService()
        {
            InitializeComponent();

            // Init NSQ service
            nsqService = new NsqMessageProducerService();
            _nsqConsumer = new NsqMessageConsumerService(topic: NsqTopics.KONBI_CREDIT_PAYMENT_REQUEST_TOPIC, handler: this, ClientId: "KonbiCreditsPayment");

            WriteToFile("Init NSQ completed.");

            // Init the card reader and related event handler
            InitCardReader();

            // Load API Info
            LoadAPIInfo();
        }

        protected override void OnStart(string[] args)
        {
        }

        protected override void OnStop()
        {
            WriteToFile("Konbi Credits Service Exited.");
        }

        public void HandleMessage(IMessage message)
        {
            var msg = Encoding.UTF8.GetString(message.Body);
            var obj = JsonConvert.DeserializeObject<UniversalCommands>(msg);

            //Check for message timeouts then do not make timeout messages.
            if (obj.IsTimeout())
            {
                return;
            }

            if (obj.Command == UniversalCommandConstants.EnablePaymentCommand)
            {
                WriteToFile("Enable Payment.");

                var cmd = JsonConvert.DeserializeObject<NsqEnablePaymentCommand>(msg);
                Enum.TryParse<PaymentTypes>(cmd.PaymentType, out PaymentTypes pType);

                if (pType == PaymentTypes.KONBI_CREDITS)
                {
                    CompleteReceivedAsync(cmd).Wait();
                    //Activate device
                    requestedAmount = cmd.Amount;
                    var isActivated = false;

                    var command = new NsqEnablePaymentResponseCommand(paymentType);
                    command.TransactionId = cmd.TransactionId;
                    isActivated = true;
                    command.Code = isActivated ? 0 : 1;
                    var isSent = nsqService.SendPaymentResponseCommand(command);
                    isEnabled = true;
                }
            }
            else if (obj.Command == UniversalCommandConstants.DisablePaymentCommand)
            {
                isEnabled = false;
            }
            //else if (obj.Command == UniversalCommandConstants.Ping)
            //{
            //    CompleteReceivedAsync(cmd).Wait();
            //}
        }

        private async Task CompleteReceivedAsync(NsqPaymentCommandBase obj)
        {
            await Task.Delay(10);
            var command = new NsqPaymentACKResponseCommand(obj.CommandId, obj.PaymentType);
            nsqService.SendPaymentResponseCommand(command);
            return;
        }

        public void LogFailedMessage(IMessage message)
        {
            WriteToFile("Exception: " + message);
        }

        private void Payment()
        {
            WriteToFile("Start Payment.");
            // Request token from payment API
            if (RequestToken())
            {
                Thread.Sleep(100);
                WriteToFile("Request Token.");

                // Do payment with Token
                if (DoPayment())
                {
                    WriteToFile("Do Payment.");
                    PaymentSucceed();
                }
                else
                {
                    PaymentFailed();
                }
            }
            else
            {
                PaymentFailed();
            }
        }

        // Request an access token from API
        private bool RequestToken()
        {
            var client = new RestClient(APIServerAddress + "/?oauth=token");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("grant_type", "client_credentials");
            request.AddParameter("client_id", APIClientID);
            request.AddParameter("client_secret", APIClientSecret);
            IRestResponse response = client.Execute(request);

            if (!response.IsSuccessful)
            {
                WriteToFile("Request Token Failed");
            }

            WriteToFile(response.Content);

            // Convert to JSON object
            JObject responseJSON = JObject.Parse(response.Content);

            // Get Token Success
            if (responseJSON["access_token"] != null)
            {
                paymentToken = responseJSON["access_token"].ToString();
                WriteToFile("Token: " + paymentToken);

                return true;
            }

            return false;
        }

        // Deduct credit from Card ID through API
        private bool DoPayment()
        {
            paymentAmount = requestedAmount.ToString("#.##");

            WriteToFile("{paymentAmount: " + paymentAmount + "}");
            WriteToFile("{paymentToken: " + paymentToken + "}");
            WriteToFile("{cardID: " + cardID + "}");

            var client = new RestClient(APIServerAddress + "/index.php/wp-json/wp/v2/wallet/pay");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("access_token", paymentToken);
            request.AddParameter("card_id", cardID);
            request.AddParameter("amount", paymentAmount);
            request.AddParameter("description", "Konbi Credits Payment Amount " + paymentAmount);
            IRestResponse response = client.Execute(request);

            if (!response.IsSuccessful)
            {
                WriteToFile("Do Payment Failed");
            }

            WriteToFile(response.Content);

            // Convert to JSON object
            JObject responseJSON = JObject.Parse(response.Content);

            // Payment Success
            if (responseJSON["transaction_id"] != null)
            {
                paymentTransactionID = responseJSON["transaction_id"].ToString();
                konbiCreditBalance = responseJSON["balance"].ToString();

                WriteToFile("Payment Transaction ID: " + paymentTransactionID);

                return true;
            }

            // Insufficient credit error
            if (responseJSON["error_code"] != null)
            {
                if (responseJSON["message"].ToString() == "The amount is greater than balance!!!")
                {
                    isSufficientCredit = 0;
                    konbiCreditBalance = responseJSON["balance"].ToString();
                }
            }

            return false;
        }

        private void PaymentSucceed()
        {
            var cmd = new NsqPaymentCallbackResponseCommand(paymentType)
            {
                Response = new PaymentResponseData()
                {
                    Message = "Payment approved",
                    State = PaymentState.Success,
                    ResponseObject = new
                    {
                        CardLabel = "KONBI CREDITS",
                        CardNumber = cardID,
                        Rrn = paymentTransactionID,
                        ApproveCode = "000001",
                        Amount = paymentAmount,
                        KonbiCreditBalance = konbiCreditBalance,
                        IsSufficientFund = isSufficientCredit
                    },
                    OtherInfo = null
                }
            };
            nsqService.SendPaymentResponseCommand(cmd);
            WriteToFile("Success Payment Response Sent.");
        }

        private void PaymentFailed()
        {
            var cmd = new NsqPaymentCallbackResponseCommand(paymentType)
            {
                Response = new PaymentResponseData()
                {
                    Message = "Cannot validate your membership info",
                    State = PaymentState.Failure,
                    ResponseObject = new
                    {
                        CardLabel = "KONBI CREDITS",
                        CardNumber = cardID,
                        ApproveCode = "000001",
                        Amount = paymentAmount,
                        KonbiCreditBalance = konbiCreditBalance,
                        IsSufficientFund = isSufficientCredit
                    },
                    OtherInfo = null
                }
            };
            nsqService.SendPaymentResponseCommand(cmd);
            WriteToFile("Fail Payment Response Sent.");
        }

        private void InitCardReader()
        {
            var ascReaderService = new ASCReader();

            // Initialize card reader lisener
            ascReaderService.Init();

            // Listen to insert card event
            ascReaderService.OnInsertedCard = cardNumber =>
            {
                cardID = cardNumber;
                WriteToFile("Card Detected: " + cardNumber);

                if (isEnabled)
                {
                    isEnabled = false;
                    Payment();
                }
            };

            // When NFC card is removed from reader
            ascReaderService.OnRemovedCard = () =>
            {
            };

            // listen on reader attachment
            ascReaderService.OnConnectedReader = isConnected =>
            {
                if (isConnected)
                {
                    // Reader is connected
                    var readers = ascReaderService.GetReaders();

                    foreach (var reader in readers)
                    {
                        Console.WriteLine($"{reader}");
                        WriteToFile("Connected: " + reader);
                    }
                }
            };
        }

        // Load API Info
        private void LoadAPIInfo()
        {
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = configFile.AppSettings.Settings;

            APIServerAddress = settings["api_server_address"].Value;
            APIClientID = settings["api_client_id"].Value;
            APIClientSecret = settings["api_client_secret"].Value;

            WriteToFile("APIServerAddress: " + APIServerAddress);
            WriteToFile("APIClientID: " + APIClientID);
            WriteToFile("APIClientSecret: " + APIClientSecret);
        }

        // Write Log
        public void WriteToFile(string Message)
        {
            string path = @"C:\Logs";

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string filepath = @"C:\Logs\KonbiCreditsServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";

            if (!File.Exists(filepath))
            {
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }

            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
        }
    }
}
