using Caliburn.Micro;
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
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using System.Configuration;
using Konbi.Common.Interfaces;
using System.Threading;
using System.Diagnostics;
using System.Windows;
using Konbini.Messages.Commands;

namespace KonbiCreditsCardReaderListener.ViewModels
{
    public class ShellViewModel : Conductor<object>, IShell, IDisposable, INsqHandler
    {
        private NsqMessageProducerService nsqService;
        private NsqMessageConsumerService _nsqConsumer;
        public IKonbiBrainLogService LogService { get; set; }
        private readonly string paymentType = PaymentTypes.KONBI_CREDITS.ToString();
        KeyboardListener KListener = new KeyboardListener();
        System.Timers.Timer timer;
        System.Timers.Timer pingTimer;
        private readonly string SERVICE_NAME = "Konbi Credits Card Reader Listener App";
        private readonly string SERVICE_TYPE = "KonbiCreditsPaymentController.Service";
        private String appName = "KonbiCreditsCardReaderListener";
        private String cardID = "";
        private String magicPlateTxnID = "";
        private String cardReaderInput = "";
        private String paymentToken = "";
        private String paymentAmount = "";
        private String paymentTransactionID = "";
        private String konbiCreditBalance = "";
        private String APIServerAddress = "";
        private String APIClientID = "";
        private String APIClientSecret = "";
        private int isSufficientCredit = 1;
        private int isUserExist = 1;
        private bool isEnabled = false;
        private bool isListening = false;
        private decimal requestedAmount = 0;
        private WindowState WindowState;

        protected override void OnActivate()
        {
            LoadAPIInfo();
            InitNsq();
            try
            {
                base.OnActivate();
                KListener.KeyDown += new RawKeyEventHandler(KListener_KeyDown);
                timer = new System.Timers.Timer();
                timer.Interval = 1000;
                timer.Elapsed += KeyboardTimoutHandler;

                //pingTimer = new System.Timers.Timer();
                //pingTimer.Interval = 1800000;
                //pingTimer.Elapsed += PingAlive;
                //pingTimer.Start();
            }
            catch (Exception ex)
            {
                LogService.LogException($"{appName}: {ex}");
            }

            this.WindowState = WindowState.Minimized;
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            LogService.LogInfo($"{appName}: App Exited.");
            timer.Dispose();
            KListener.Dispose();
            App.Current.Shutdown();
            Process.GetCurrentProcess().Kill();
        }

        public void InitNsq()
        {
            // Init NSQ service
            nsqService = new NsqMessageProducerService();
            _nsqConsumer = new NsqMessageConsumerService(topic: NsqTopics.KONBI_CREDIT_PAYMENT_REQUEST_TOPIC, handler: this, ClientId: "KonbiCreditsPayment");

            LogService.LogInfo($"{appName}: Init NSQ completed.");
        }

        void KListener_KeyDown(object sender, RawKeyEventArgs args)
        {
            timer.Start();
            var key = args.Key;
            var inputChar = args.ToString();

            if (key == System.Windows.Input.Key.Enter || key == System.Windows.Input.Key.Return)
            {
                if (string.IsNullOrEmpty(cardReaderInput))
                {
                    return;
                }

                cardID = cardReaderInput;
                LogService.LogInfo($"{appName}: Card Detected: {cardID}");

                // Start payment if card id is detected and payment is enabled
                if (isEnabled)
                {
                    isEnabled = false;
                    isListening = false;
                    Payment();
                }

                cardReaderInput = "";
            }
            else
            {
                if (isListening)
                {
                    cardReaderInput += inputChar;
                }
            }
        }

        void KeyboardTimoutHandler(object sender, ElapsedEventArgs e)
        {
            cardReaderInput = string.Empty;
            timer.Stop();
        }

        void PingAlive(object sender, ElapsedEventArgs e)
        {
            LogService.LogInfo($"{appName}: Ping.");
            this.WindowState = WindowState.Minimized;
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
                LogService.LogInfo($"{appName}: Received EnablePaymentCommand.");
                var cmd = JsonConvert.DeserializeObject<NsqEnablePaymentCommand>(msg);
                Enum.TryParse<PaymentTypes>(cmd.PaymentType, out PaymentTypes pType);

                if (pType == PaymentTypes.KONBI_CREDITS)
                {
                    LogService.LogInfo($"{appName}: Enabled Payment.");
                    LogService.LogInfo($"{appName}: Message from NSQ: {msg}.");

                    CompleteReceivedAsync(cmd).Wait();
                    //Activate device
                    requestedAmount = cmd.Amount;
                    magicPlateTxnID = cmd.TransactionId.ToString();
                    var isActivated = false;

                    var command = new NsqEnablePaymentResponseCommand(paymentType);
                    command.TransactionId = cmd.TransactionId;
                    isActivated = true;
                    isListening = true;
                    command.Code = isActivated ? 0 : 1;
                    var isSent = nsqService.SendPaymentResponseCommand(command);
                    isEnabled = true;
                }
            }
            else if (obj.Command == UniversalCommandConstants.DisablePaymentCommand)
            {
                LogService.LogInfo($"{appName}: Received DisablePaymentCommand.");
                isEnabled = false;
            }
            else if (obj.Command == UniversalCommandConstants.Ping)
            {
                LogService.LogInfo($"{appName}: Received Ping.");
                PublishDeviceInfo(false);
                Task.Delay(500).Wait();
                CompleteReceivedAsync(new NsqPaymentACKResponseCommand(obj.CommandId, "None")).Wait();
            }
        }

        public void PublishDeviceInfo(bool hasError, string errorMessage = "")
        {
            // Publish service informative description
            var deviceInfoCmd = new DeviceInfoCommand();
            deviceInfoCmd.CommandObject.Name = this.SERVICE_NAME;
            deviceInfoCmd.CommandObject.Type = this.SERVICE_TYPE;
            deviceInfoCmd.CommandObject.HasError = hasError;

            if (!string.IsNullOrEmpty(errorMessage))
                deviceInfoCmd.CommandObject.Errors.Add(errorMessage);

            nsqService.SendPaymentResponseCommand(deviceInfoCmd);
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
            LogService.LogException($"{appName}: {message}");
        }

        private void Payment()
        {
            LogService.LogInfo($"{appName}: Start Payment.");

            // Request token from payment API
            if (RequestToken())
            {
                Thread.Sleep(100);
                LogService.LogInfo($"{appName}: Token requested success.");

                // Do payment with Token
                if (DoPayment())
                {
                    LogService.LogInfo($"{appName}: Payment made success.");
                    PaymentSucceed();
                }
                else
                {
                    PaymentFailed();
                }
            }
            else
            {
                GetTokenFailed();
            }

            konbiCreditBalance = "";
            isUserExist = 1;
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

            //if (!response.IsSuccessful)
            if (response.Content == "")
            {
                LogService.LogException($"{appName}: Request Token Failed.");

                return false;
            }
            else
            {
                LogService.LogInfo($"{appName}: Request Token API Response: {response.Content}.");

                // Convert to JSON object
                JObject responseJSON = JObject.Parse(response.Content);

                // Get Token Success
                if (responseJSON["access_token"] != null)
                {
                    paymentToken = responseJSON["access_token"].ToString();
                    LogService.LogInfo($"{appName}: Token: {paymentToken}.");

                    return true;
                }
            }
            return false;
        }

        // Deduct credit from Card ID through API
        private bool DoPayment()
        {
            paymentAmount = requestedAmount.ToString("#.##");

            LogService.LogInfo($"{appName}: Payment Amount: {paymentAmount}.");
            LogService.LogInfo($"{appName}: Payment Token: {paymentToken}.");
            LogService.LogInfo($"{appName}: Card ID: {cardID}.");

            var client = new RestClient(APIServerAddress + "/index.php/wp-json/wp/v2/wallet/pay");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("access_token", paymentToken);
            request.AddParameter("card_id", cardID);
            request.AddParameter("amount", paymentAmount);
            request.AddParameter("description", "MagicPlate Txn ID: " + magicPlateTxnID);
            IRestResponse response = client.Execute(request);

            if (response.Content == "")
            {
                LogService.LogException($"{appName}: Do Payment Failed.");

                return false;
            }
            else
            {
                LogService.LogInfo($"{appName}: Payment API Response: {response.Content}.");

                // Convert to JSON object
                JObject responseJSON = JObject.Parse(response.Content);

                // Payment Success
                if (responseJSON["transaction_id"] != null)
                {
                    paymentTransactionID = responseJSON["transaction_id"].ToString();
                    konbiCreditBalance = responseJSON["balance"].ToString();

                    LogService.LogInfo($"{appName}: Payment Transaction ID: {paymentTransactionID}.");

                    return true;
                }

                // Insufficient credit error
                if (responseJSON["error_code"] != null)
                {
                    if (responseJSON["message"].ToString() == "The amount is greater than balance!!!")
                    //if (responseJSON["error_code"].ToString() == "403" || responseJSON["error_code"].ToString() == "404")
                    {
                        LogService.LogInfo($"Insufficient balance.");
                        isSufficientCredit = 0;
                        konbiCreditBalance = responseJSON["balance"].ToString();
                        isUserExist = 1;
                    }
                    else if (responseJSON["message"].ToString() == "Can not find user!!!")
                    //else if (responseJSON["error_code"].ToString() == "401" || responseJSON["error_code"].ToString() == "402")
                    {
                        LogService.LogInfo($"{appName}: User not found.");
                        isSufficientCredit = -1;
                        konbiCreditBalance = "-1";
                        isUserExist = 0;
                    }
                    return false;
                }
            }
            return false;
        }

        // Respond success payment through NSQ
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
                        IsTokenRetrieved = 1,
                        KonbiCreditBalance = konbiCreditBalance,
                        IsSufficientFund = 1,
                        IsUserExist = 1
                    },
                    OtherInfo = null
                }
            };
            nsqService.SendPaymentResponseCommand(cmd);
            LogService.LogInfo($"{appName}: Success Payment Response Sent.");
        }

        // Respond failure payment through NSQ
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
                        IsTokenRetrieved = 1,
                        KonbiCreditBalance = konbiCreditBalance,
                        IsSufficientFund = isSufficientCredit,
                        IsUserExist = isUserExist
                    },
                    OtherInfo = null
                }
            };
            nsqService.SendPaymentResponseCommand(cmd);
            LogService.LogInfo($"{appName}: Fail Payment Response Sent.");
        }

        // Respond failure payment through NSQ (Because of failed to get token)
        private void GetTokenFailed()
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
                        IsTokenRetrieved = 0,
                        KonbiCreditBalance = -1,
                        IsSufficientFund = -1,
                        IsUserExist = -1
                    },
                    OtherInfo = null
                }
            };
            nsqService.SendPaymentResponseCommand(cmd);
            LogService.LogInfo($"{appName}: Fail Payment Response Sent.");
        }

        // Load API Info
        private void LoadAPIInfo()
        {
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = configFile.AppSettings.Settings;

            APIServerAddress = settings["api_server_address"].Value;
            APIClientID = settings["api_client_id"].Value;
            APIClientSecret = settings["api_client_secret"].Value;

            LogService.LogInfo($"{appName}: API Server Address: {APIServerAddress}.");
            LogService.LogInfo($"{appName}: API Client ID: {APIClientID}.");
            LogService.LogInfo($"{appName}: API Client Secret: {APIClientSecret}.");
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

        public void Dispose()
        {
            KListener.Dispose();
        }
    }
}
