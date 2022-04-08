using KonbiBrain.Common.Messages;
using KonbiBrain.Common.Messages.Payment;
using KonbiBrain.Common.Services;
using KonbiBrain.Messages;
using KonbiBrain.WindowService.FacialRecognition.Services;
using KonbiBrain.WindowService.FacialRecognition.Util;
using Konbini.Messages.Enums;
using MQTTnet.Extensions.ManagedClient;
using Newtonsoft.Json;
using NsqSharp;
using rsid;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KonbiBrain.WindowService.FacialRecognition
{
    public partial class FacialRecognitionService : ServiceBase, INsqHandler
    {
        private readonly NsqMessageProducerService _nsqProducer;
        private NsqMessageConsumerService _nsqConsumer;
        private readonly string paymentType = PaymentTypes.FACIAL_RECOGNITION.ToString();
        private RealSenseID _realsenseID;
        private RealsenseService _realsenseService;
        private MqttClient _mqttClient;

        private string apiServerAddress = "";
        private string apiClientID = "";
        private string apiClientSecret = "";
        private string comport = "";
        private int databaseVersion = 0;
        private int confidentThreadhold = 0;
        private string securityLevel = "";
        private string algoFlow = "";
        private string faceSelectionPolicy = "";
        private string cameraRotation = "";
        private string dumpMode = "";


        //mqtt
        private string mqttURI = "";
        private string mqttUser = "";
        private string mqttPassword = "";
        private int mqttPort = 0;

        private string paymentAmount = "";
        private string paymentToken = "";
        private decimal requestedAmount = 0;
        private string txnId = "";
        private double konbiCreditBalance = 0.0;
        private int isSufficientCredit = 1;
        private bool isEnabled = false;
        private string messageError = "";
        private bool isAutoDetect = false;



        public FacialRecognitionService()
        {
            InitializeComponent();


            SeriLogService.CreateLoggers();

            SeriLogService.LogInfo("FacialRecognitionService init");

            //initial nsq
            _nsqProducer = new NsqMessageProducerService();
            _nsqConsumer = new NsqMessageConsumerService(topic: NsqTopics.FACIAL_RECOGNITION_PAYMENT_REQUEST_TOPIC, handler: this, ClientId: "KonbiFacialRecognitionPayment");
            //load config string
            LoadConfig();

            _realsenseService = new RealsenseService(apiServerAddress);
            //_mqttClient = new MqttClient(mqttURI, mqttUser, mqttPassword, mqttPort);
        }
        /**
        * 
        * ServiceBase Implementation
        * 
        * **/
        protected override void OnStart(string[] args)
        {
            Helper.WriteToFile("Init NSQ Start.");
            _realsenseID = new RealSenseID(comport);
            _realsenseID.GetUserFaceprintFromDevice();

            //IMPLEMENT FACE_DETECTED EVENT
            _realsenseID.detectedFace += (sender, ev) =>
            {

                FaceprintEventArgs detectFace = (FaceprintEventArgs)ev;
                if (detectFace.status == AuthStatus.Success)//extract/detect faceprint ok -> then match it
                {
                    Helper.WriteToFile($"[EVENT-Message] Sub: face is detected");
                    Helper.WriteToFile($"[EVENT-Message] faceprints-confidence: {_realsenseID.faceprintsConfidence}");
                    if (_realsenseID.faceprintsSuccess == 1)//matched
                    {
                        Helper.WriteToFile($"[EVENT-Message] Matched");
                        Helper.WriteToFile($"[EVENT-Message] ConfidentThreadhold is {confidentThreadhold}");
                        if (_realsenseID.faceprintsConfidence > confidentThreadhold)
                        {
                            Helper.WriteToFile($"[EVENT-Message] Confidence {_realsenseID.faceprintsConfidence} is content the threshold {confidentThreadhold}");
                            string userid = _realsenseID.faceprintUserId;
                            DoPayment(userid);
                        }
                        else
                        {
                        FaceBelowThreshold();
                        _realsenseID.Standby();
                        _realsenseID.isCountDetect = 0;
                        Helper.WriteToFile($"[EVENT-Message] Standby");
                        Helper.WriteToFile($"[EVENT-Message] Confidence {_realsenseID.faceprintsConfidence} is less than ConfidentThreadhold is {confidentThreadhold}");
 
                    }
                    }
                    else
                    {

                    _realsenseID.Standby();
                    NoFaceDeteced();
                    _realsenseID.isCountDetect = 0;
                    Helper.WriteToFile($"[EVENT-Message] Standby");
                    Helper.WriteToFile($"[EVENT-Message] Not matched");
                        
                }
            }
                else if(detectFace.status == AuthStatus.Failure)
                {
                    _realsenseID.Standby();
                    Helper.WriteToFile($"[EVENT-Message] Standby");
                    ErrorDeviceConnection();
                   // PaymentFailed();
                    Helper.WriteToFile($"[EVENT-Message] No match found in the local device, stop camera");

                    _realsenseID.isCountDetect = 0;
                }
                else if (detectFace.status == AuthStatus.Forbidden) //not found face in the local device
                {
                    _realsenseID.Standby();
                    Helper.WriteToFile($"[EVENT-Message] Standby");
                    NoFaceDeteced();
                 //   PaymentFailed();
                    Helper.WriteToFile($"[EVENT-Message] No match found in the local device, stop camera");
                    _realsenseID.isCountDetect = 0;
                }
                else
                {
                }
                

            };
        }
        protected override void OnStop()
        {
            Helper.WriteToFile("Service is stopped.");
        }
        /**
         * 
         * NSQ implementation
         * 
         * **/
        public void HandleMessage(IMessage message)
        {
            try
            {
                var msg = Encoding.UTF8.GetString(message.Body);
                var obj = JsonConvert.DeserializeObject<UniversalCommands>(msg);

                //Check for message timeouts then do not make timeout messages.
                if (obj.IsTimeout())
                {
                    _realsenseID.Standby();
                    return;
                }

                if (obj.Command == UniversalCommandConstants.EnablePaymentCommand)
                {
                    Helper.WriteToFile("FacialPayment starting...");
                    Helper.WriteToFile($"NSQ message: {JsonConvert.SerializeObject(msg)}");
                    var cmd = JsonConvert.DeserializeObject<NsqEnablePaymentCommand>(msg);

                    Enum.TryParse(cmd.PaymentType, out PaymentTypes pType);

                    if (pType == PaymentTypes.FACIAL_RECOGNITION)
                    {
                        CompleteReceivedAsync(cmd).Wait();
                        //Activate device
                        requestedAmount = cmd.Amount;
                        Helper.WriteToFile($"requestedAmount: {requestedAmount}");
                        var isActivated = false;

                        var command = new NsqEnablePaymentResponseCommand(paymentType);
                        command.TransactionId = cmd.TransactionId;
                        isActivated = true;
                        command.Code = isActivated ? 0 : 1;
                        var isSent = _nsqProducer.SendPaymentResponseCommand(command);
                        isEnabled = true;
                        //start to payment by facial recognition
                            Helper.WriteToFile("=============START TO SCAN FACE=============");
                            PaymentProcessing();
                            Helper.WriteToFile($"^^^^^^^^^^^^END TO SCAN FACE^^^^^^^^^^^^");
                        
                    }
                }
                else if (obj.Command == UniversalCommandConstants.DisablePaymentCommand)
                {
                    _realsenseID.Standby();
                    isEnabled = false;
                }
            }
            catch (Exception ex)
            {
                SeriLogService.LogInfo(ex.ToString());
            }
        }
        private async Task CompleteReceivedAsync(NsqPaymentCommandBase obj)
        {
            await Task.Delay(10);
            var command = new NsqPaymentACKResponseCommand(obj.CommandId, obj.PaymentType);
            _nsqProducer.SendPaymentResponseCommand(command);
            return;
        }
        public void LogFailedMessage(IMessage message)
        {
            Helper.WriteToFile("NSQ Exception: " + message);
        }
        /**
         * 
         * Server processing: Payment/Deduct
         * 
         * **/
        private void PaymentProcessing()
        {
            _realsenseID.faceprintsScore = 0;
            _realsenseID.faceprintsConfidence = 0;
            _realsenseID.faceprintsSuccess = 0;
            _realsenseID.faceprintUserId = "";
            _realsenseID.AuthenticateLoopExtractFaceprints();
        }
        private void DoPayment(string user_id)
        {
            Helper.WriteToFile("Start Payment.");
            Thread.Sleep(100);
            if (Purchase(user_id))
            {
                Helper.WriteToFile("Start Purchase");
                PaymentSucceed();
            }
            else
            {
                PaymentFailed();
            }
            Helper.WriteToFile("End Payment.");
        }

        private bool Purchase(string user_id)
        {
            var response = _realsenseService.Purchase(user_id, requestedAmount);
            if (!response.IsSuccessful)
            {
                Helper.WriteToFile($"Do Payment Failed {response.ErrorMessage}");
            }
            Helper.WriteToFile(response.Content);
            var purchaseRes = JsonConvert.DeserializeObject<PurchaseResponse>(response.Content);
            if (purchaseRes.result == "success")
            {
                txnId = purchaseRes.txn_id.ToString();
                Helper.WriteToFile("Payment Transaction ID: " + txnId);
                konbiCreditBalance = purchaseRes.balance;
                return true;
            }
            else
            {
                messageError = purchaseRes.message;
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
                        CardLabel = "FACIAL RECOGNITION",
                        CardNumber = _realsenseID.faceprintUserId,
                        Rrn = txnId,
                        ApproveCode = "000002",
                        Amount = requestedAmount.ToString(),//paymentAmount
                        KonbiCreditBalance = konbiCreditBalance.ToString(),
                        IsSufficientFund = 0 //isSufficientCredit
                    },
                    OtherInfo = null
                }
            };
            _nsqProducer.SendPaymentResponseCommand(cmd);
            Helper.WriteToFile("[NSQ-lient] Success Payment Response Sent.");
        }
        private void PaymentFailed()
        {
            Helper.WriteToFile("[NSQ-lient] Payment failed");
            var cmd = new NsqPaymentCallbackResponseCommand(paymentType)
            {
                Response = new PaymentResponseData()
                {
                    Message = messageError,
                    State = PaymentState.Failure,
                    ResponseObject = new
                    {
                        CardLabel = "FACIAL RECOGNITION",
                        //CardNumber = cardID, ==> user_id or faceprint
                        ApproveCode = "000002",
                        Amount = requestedAmount.ToString(),//paymentAmount
                        KonbiCreditBalance = 0,//konbiCreditBalance
                        IsSufficientFund = 0 //isSufficientCredit
                    },
                    OtherInfo = null
                }
            };
            _nsqProducer.SendPaymentResponseCommand(cmd);
            Helper.WriteToFile("[NSQ-lient] Fail Payment Response Sent.");
        }
        private void NoFaceDeteced()
        {
            Helper.WriteToFile("[NSQ-lient] NoFaceDeteced");
            var cmd = new NsqPaymentCallbackResponseCommand(paymentType)
            {
                Response = new PaymentResponseData()
                {
                    Message = "Face not recognised",
                    State = PaymentState.Rejected,
                    ResponseObject = new
                    {
                        CardLabel = "FACIAL RECOGNITION",
                        //CardNumber = cardID, ==> user_id or faceprint
                        ApproveCode = "000002",
                        Amount = requestedAmount.ToString(),//paymentAmount
                        KonbiCreditBalance = 0,//konbiCreditBalance
                        IsSufficientFund = 0 //isSufficientCredit
                    },
                    OtherInfo = null
                }
            };
            _nsqProducer.SendPaymentResponseCommand(cmd);
            Helper.WriteToFile("[NSQ-lient] Fail Payment Response Sent.");
        }
        private void FaceBelowThreshold()
        {
            var cmd = new NsqPaymentCallbackResponseCommand(paymentType)
            {
                Response = new PaymentResponseData()
                {
                    Message = "Faceprint below threshold",
                    State = PaymentState.Rejected,
                    ResponseObject = new
                    {
                        CardLabel = "FACIAL RECOGNITION",
                        //CardNumber = cardID, ==> user_id or faceprint
                        ApproveCode = "000002",
                        Amount = requestedAmount.ToString(),//paymentAmount
                        KonbiCreditBalance = 0,//konbiCreditBalance
                        IsSufficientFund = 0 //isSufficientCredit
                    },
                    OtherInfo = null
                }
            };
            _nsqProducer.SendPaymentResponseCommand(cmd);
            Helper.WriteToFile("[NSQ-lient] Fail Payment Response Sent.");
        }
        private void ErrorDeviceConnection()
        {
            Helper.WriteToFile("[NSQ-lient] Connection Failed- Please check USB Cable!");
            var cmd = new NsqPaymentCallbackResponseCommand(paymentType)
            {
                Response = new PaymentResponseData()
                {
                    Message = "Connection Failed- Please check USB Cable!",
                    State = PaymentState.Failure,
                    ResponseObject = new
                    {
                        CardLabel = "FACIAL RECOGNITION",
                        //CardNumber = cardID, ==> user_id or faceprint
                        ApproveCode = "000002",
                        Amount = requestedAmount.ToString(),//paymentAmount
                        KonbiCreditBalance = 0,//konbiCreditBalance
                        IsSufficientFund = 0 //isSufficientCredit
                    },
                    OtherInfo = null
                }
            };
            _nsqProducer.SendPaymentResponseCommand(cmd);
            Helper.WriteToFile("[NSQ-lient] Fail Payment Response Sent.");
        }
        /**
         * 
         * System processing: Load API Info/Write log into file
         * 
         * **/
        private void LoadConfig()
        {
            try
            {

                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;

                isAutoDetect = bool.Parse(settings["AutoDetect"].Value);
                apiServerAddress = settings["api_server_address"].Value;
                apiClientID = settings["api_client_id"].Value;
                apiClientSecret = settings["api_client_secret"].Value;
                databaseVersion = int.Parse(settings["DatabaseVersion"].Value);
                confidentThreadhold = int.Parse(settings["ConfidentThreadhold"].Value);
                //securityLevel = settings["SecurityLevel"].Value;
                //algoFlow = settings["AlgoFlow"].Value;
                //faceSelectionPolicy = settings["FaceSelectionPolicy"].Value;
                //cameraRotation = settings["CameraRotation"].Value;
                //dumpMode = settings["DumpMode"].Value;

                //comport auto detect

                if (isAutoDetect)
                {
                    Helper.WriteToFile($"Start auto detecting port");
                    var enumerator = new DeviceEnumerator();
                    var enumeration = enumerator.Enumerate();

                    if (enumeration.Count == 0)
                    {
                        Helper.WriteToFile($"Could not detect device. Please reconnect the device and try again.");
                        throw new Exception("Connection Error");
                    }
                    else if (enumeration.Count > 1)
                    {
                        Helper.WriteToFile($"More than one device detected. Please make sure only one device is connected and try again.");
                        throw new Exception("Connection Error");
                    }

                    comport = enumeration[0].port;
                }
                else
                {
                    comport = settings["Comport"].Value; ;
                }

                //mqtt
                //mqtt config
                mqttURI = settings["mqtt_url"].Value;
                mqttUser = settings["mqtt_user"].Value;
                mqttPassword = settings["mqtt_password"].Value;
                if (string.IsNullOrEmpty(settings["mqtt_port"].Value))
                {
                    mqttPort = 1883;
                }
                else
                {
                    mqttPort = int.Parse(settings["mqtt_port"].Value);
                }

                //Log config info
                Helper.WriteToFile($"[Appsetting] - apiServerAddress: {apiServerAddress}");
                Helper.WriteToFile($"[Appsetting] - realsenseidComport: {comport}");
                Helper.WriteToFile($"[Appsetting] - databaseVersion: {databaseVersion}");
                Helper.WriteToFile($"[Appsetting] - confidentThreadhold: {confidentThreadhold}");
                Helper.WriteToFile($"[Appsetting] - confidentThreadhold: {confidentThreadhold}");
                Helper.WriteToFile($"[Appsetting] - securityLevel: {securityLevel}");
                Helper.WriteToFile($"[Appsetting] - confidentThreadhold: {confidentThreadhold}");
                Helper.WriteToFile($"[Appsetting] - AlgoFlow: {algoFlow}");
                Helper.WriteToFile($"[Appsetting] - FaceSelectionPolicy: {faceSelectionPolicy}");
                Helper.WriteToFile($"[Appsetting] - CameraRotation: {cameraRotation}");
                Helper.WriteToFile($"[Appsetting] - DumpMode: {dumpMode}");
                Helper.WriteToFile($"[Appsetting] - mqttURI: {mqttURI}");
                Helper.WriteToFile($"[Appsetting] - mqttUser: {mqttUser}");
                Helper.WriteToFile($"[Appsetting] - mqttPassword: {mqttPassword}");
                Helper.WriteToFile($"[Appsetting] - mqttPort: {mqttPort}");
       
            }
            catch (Exception ex)
            {
                Helper.WriteToFile($"Cannot initial config {ex.ToString()}");
            }
        }

        public void RunAsConsole(string[] args)
        {
            OnStart(args);
            Helper.WriteToFile("Run Service as console");
            ConsoleInput();
            OnStop();
        }

        private void ConsoleInput()
        {
            var isLopping = true;
            while (isLopping)
            {
                string action = "";
                Console.WriteLine("[Press A then enter to Authenticate] [Press DELETE to delete all faceprint] [Press PULL to pull all of faceprint on the server]");
                action = Console.ReadLine();
                switch (action)
                {
                    case "A":
                        _realsenseID.AuthenticateExtractFaceprints();
                        if (_realsenseID.onAuthResultStatus == AuthStatus.Success)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"Authenticated successful user is: {_realsenseID.onAuthResult}");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Authenticated falied: {_realsenseID.onAuthResultStatus}");
                            Console.ResetColor();
                        }
                        break;
                    case "DELETE":
                        var status = _realsenseID.RemoveAllUserInDevice();
                        if (status == Status.Ok)
                        {
                            //reaload faceprints on cache RAM
                            var faceprint = _realsenseID.GetUserFaceprintFromDevice();
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"Remove all faceprints out of device, current faceprint is {faceprint.Count}");
                            Console.ResetColor();
                        }
                        break;
                    case "PULL":
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine("Starting to pull data...");
                        Console.ResetColor();
                        GetFaceprintsServer();
                        break;
                }
            }
        }

        private void GetFaceprintsServer()
        {
            //1. remove all old faceprint on the local device
            var status = _realsenseID.RemoveAllUserInDevice();
            //2. start pull data from server
            List<UserDataResponse> serverData = new List<UserDataResponse>();
            List<UserDataResponse> faceprintDatas = new List<UserDataResponse>();
            var willBeImportedFaceprintArr = new List<(Faceprints, string)>();
            var wrongSyntaxFaceprintArr = new List<UserDataResponse>();
            if (status == Status.Ok)
            {
                string statusCallAPI = "";
                serverData = _realsenseService.GetListFaceprints(out statusCallAPI);
                if (statusCallAPI == "0")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Helper.WriteToFile($"Can not call to API because timeout");
                    Console.ResetColor();
                }
                else if (statusCallAPI == "Unauthorized")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Helper.WriteToFile($"Call api get error becasue Unauthorized");
                    Console.ResetColor();
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Helper.WriteToFile($"Can not clean data before importing");
                Console.ResetColor();
            }
            if (serverData.Count > 0)
            {
                //3. filter all durty data of faceprint
                foreach (var item in serverData)
                {
                    if (item.ccw_id2.Length > 20)
                    {
                        faceprintDatas.Add(item);
                    }
                    else
                    {
                        wrongSyntaxFaceprintArr.Add(item);
                    }
                }
                //4. import into the device
                foreach (var faceprintData in faceprintDatas)
                {
                    string faceprintEncode = faceprintData.ccw_id2; //ccw_id2 is faceprint
                    string faceprintDecode = Helper.Base64Decode(faceprintEncode);
                    Faceprints faceprint = JsonConvert.DeserializeObject<Faceprints>(faceprintDecode);
                    string userId = faceprintData.username;
                    willBeImportedFaceprintArr.Add((faceprint, userId));
                }
                var checkImport = _realsenseID.ImportFaceprintToDevice(willBeImportedFaceprintArr);
                if (checkImport)
                {
                    _realsenseID.GetUserFaceprintFromDevice();
                    //1.1. show number faceprint get from SERVER
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Number of faceprints get from server is {serverData.Count}");
                    Helper.WriteToFile($"Number of faceprints get from server is {serverData.Count}");
                    //1.2. show all wrong faceprints 
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Number of wrong faceprints is {wrongSyntaxFaceprintArr.Count}, detaily...");
                    Helper.WriteToFile($"Number of wrong faceprints is {wrongSyntaxFaceprintArr.Count}, detaily...");
                    Console.ResetColor();
                    foreach (var item in wrongSyntaxFaceprintArr)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkMagenta;
                        Helper.WriteToFile($"The user that faceprint is not right syntax: {item.username}");
                        Console.ResetColor();
                    }
                    //1.3.number of faceprint will be import into device
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Number of faceprints will be imported is {willBeImportedFaceprintArr.Count}");
                    Helper.WriteToFile($"Number of faceprints will be imported is {willBeImportedFaceprintArr.Count}");
                    Console.ResetColor();
                    //1.4.imported faceprint number
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Number of faceprints imported successfully: {willBeImportedFaceprintArr.Count}");
                    Helper.WriteToFile($"Number of faceprints imported successfully: {willBeImportedFaceprintArr.Count}");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Helper.WriteToFile($"Import finished");
                    Console.ResetColor();
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Helper.WriteToFile($"There was not faceprint from SERVER, stop importing");
                Console.ResetColor();
            }

        }
    }


}
