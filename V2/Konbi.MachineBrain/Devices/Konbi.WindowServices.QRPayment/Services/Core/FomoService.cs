using Konbi.WindowServices.QRPayment.Configuration;
using Konbi.WindowServices.QRPayment.Util;
using KonbiBrain.Common.Messages;
using KonbiBrain.Common.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NsqSharp;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Timers;
using Konbi.WindowServices.QRPayment.Enums;
using Konbini.Messages.Commands;
using KonbiBrain.Interfaces;
using System.Threading.Tasks;
using Konbini.Common.Messages;
using KonbiBrain.Messages;
using KonbiBrain.Common.Messages.Payment;
using Konbini.Messages.Payment;
using Konbini.Messages.Enums;
using Konbi.WindowServices.QRPayment.Services.Core.Fomo;
using Microsoft.Extensions.Options;

namespace Konbi.WindowServices.QRPayment.Services.Core
{


    public class FomoService : IDisposable, INsqHandler
    {
        #region Services
        private LogService LogService;
        private IMessageProducerService _nsqMessageProducerService { get; set; }
        private NsqMessageConsumerService _nsqConsumer;

        #endregion

        #region Properties
        private string Stan { get; set; }
        private FomoConfiguration FomoConfig;
        private Timer _pollingTimer = new Timer();

        private readonly HttpClient httpClient = new HttpClient();

        private readonly string SERVICE_NAME = "QR Payment Controller";
        private readonly string SERVICE_TYPE = "QRPaymentController.Service";
        private readonly string AppLabel = "QR Payment";
        private readonly double Polling_Interval = 1000; //miliseconds
        private PaymentTypes CurrentPaymentType = PaymentTypes.QR_DASH;
        private TransactionInfo CurrentTransaction { get; set; }

        #endregion


        public FomoService(LogService logService, IMessageProducerService nsqMessageProducerService)
        {
            LogService = logService;
            _nsqMessageProducerService = nsqMessageProducerService;



        }
        /// <summary>
        /// this is init for Application
        /// </summary>
        /// <param name="config"></param>
        public void Init(IConfigurationRoot config)
        {

            _nsqConsumer = new NsqMessageConsumerService(topic: NsqTopics.PAYMENT_QR_REQUEST_TOPIC, handler: this, ClientId: "QRPayment");
            LoadSettingsAndKeys(config);
            CurrentTransaction = new TransactionInfo() { Mid = FomoConfig.MID, Tid = FomoConfig.TID };
            _pollingTimer.Interval = Polling_Interval;
            _pollingTimer.Elapsed += _pollingTimer_Elapsed;

            PublishDeviceInfo(false, "");
        }
        /// <summary>
        /// This is init for Background Job only because of no need of nsqConsumer
        /// </summary>
        /// <param name="config"></param>
        public void Init(FomoConfiguration config)
        {
            try
            {
                FomoConfig = config;

                FomoConfig.FomoPublicKey = System.IO.File.ReadAllText(EnvironmentVariables.CurrentPath + "\\FomoKeys\\" + config.FomoPublicKey);
                FomoConfig.VendorPrivateKey = System.IO.File.ReadAllText(EnvironmentVariables.CurrentPath + "\\FomoKeys\\" + config.VendorPrivateKey);
                CurrentTransaction = new TransactionInfo() { Mid = FomoConfig.MID, Tid = FomoConfig.TID };
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="conditionCode"></param>
        /// <param name="amount"> amount in cent</param>
        /// <param name="orderNumber"> order id</param>
        /// <param name="description"> order description</param>
        /// <returns> 
        /// IsSuccess,Error Code, QR code or Error message
        /// </returns>
        private async Task<Tuple<bool, string, string>> SaleRequestAsync(ConditionCodes conditionCode, int amount, string orderNumber, string description)
        {
            var isSuccess = false;
            var errorCode = "9999";
            var qrCodeOrMessage = "";
            StopPollingRequest();

            LogService.LogInfo("Start Sale Request");

            if (conditionCode == ConditionCodes.SingtelDashQr)
                CurrentTransaction.PaymentType = PaymentTypes.QR_DASH;
            CurrentTransaction.Amount = amount;
            CurrentTransaction.OrderId = orderNumber;
            CurrentTransaction.Description = description;

            var timestamp = GetTimestamp().ToString();
            var s_timestamp = DateTime.Now.ToString("MMddHHmmss");
            var s_time = DateTime.Now.ToString("HHmmss");
            var s_date = DateTime.Now.ToString("MMdd");
            var sAmount = amount.ToString().PadLeft(12, '0');
            var payload = "{\"0\": \"0200\",\"1\": \"a238408000c080040000010001000000\",\"3\": \"000000\",\"7\": \"" + s_timestamp + "\"," +
                "\"11\": \"" + Stan + "\",\"12\": \"" + s_time + "\",\"13\": \"" + s_date + "\",\"18\": \"" + "5812" + "\"," +
                "\"25\": \"" + (int)conditionCode + "\",\"41\": \"" + FomoConfig.TID + "\",\"42\": \"" + FomoConfig.MID + "\"," +
                "\"49\": \"" + "SGD" + "\",\"88\": \"" + sAmount + "\",\"62\": \"" + orderNumber + "\",\"104\": \"" + description + "\"}";

            var nonce = GenerateNonce();
            var sign = SignData(payload, timestamp, nonce);

            using (var request = new HttpRequestMessage(new HttpMethod("POST"), FomoConfig.Url))
            {
                try
                {
                    request.Headers.TryAddWithoutValidation("X-Authentication-KeyId", FomoConfig.KeyId);
                    request.Headers.TryAddWithoutValidation("X-Authentication-Method", "SHA256WithRSA");
                    request.Headers.TryAddWithoutValidation("X-Authentication-Nonce", nonce);
                    request.Headers.TryAddWithoutValidation("X-Authentication-Sign", sign);
                    request.Headers.TryAddWithoutValidation("X-Authentication-Timestamp", timestamp);
                    request.Headers.TryAddWithoutValidation("X-Authentication-Version", "1.1");

                    request.Content = new StringContent(payload);
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    var response = await httpClient.SendAsync(request);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                        result.TryGetValue("39", out string responseCode);
                        errorCode = responseCode;
                        if (responseCode == "00")
                        {
                            // QR Code
                            isSuccess = true;
                            result.TryGetValue("63", out qrCodeOrMessage);
                            if (string.IsNullOrEmpty(qrCodeOrMessage))
                            {
                                isSuccess = false;
                                qrCodeOrMessage = "Server returned empty Payment QR code";
                            }
                            LogService.LogInfo("QR:" + qrCodeOrMessage);
                            if (isSuccess)
                                StartPollingRequest();
                        }
                        else
                        {
                            //fomo return error with response code                          
                            if (!FomoResponseCodes.SALE_RESPONSES.TryGetValue(responseCode, out qrCodeOrMessage))
                                qrCodeOrMessage = "Unknown response code";
                            LogService.LogError($"SaleRequest: {conditionCode.ToString()}, Error Code: {errorCode}, Message: {qrCodeOrMessage}");
                        }
                    }
                    else
                    {
                        qrCodeOrMessage = $"Http Error code: {response.StatusCode}";
                        LogService.LogInfo($"Http Error code: {response.StatusCode}");
                        LogService.LogInfo($"Content: {response.Content.ReadAsStringAsync()}");

                    }
                }
                catch (Exception ex)
                {
                    LogService.LogInfo("ERROR  - SaleRequest");
                    LogService.LogError(ex);
                    qrCodeOrMessage = ex.Message;
                }
            }
            return new Tuple<bool, string, string>(isSuccess, errorCode, qrCodeOrMessage);




        }

        private async Task<bool> QueryRequestAsync()
        {
            bool ret = false;
            LogService.LogInfo("Start Querry Request");

            var timestamp = GetTimestamp().ToString();
            var s_timestamp = DateTime.Now.ToString("MMddHHmmss");
            var payload = "{\"0\": \"0100\",\"1\": \"2220000000c00000\"," +
                "\"3\": \"300000\",\"7\": \"" + (s_timestamp) + "\"," +
                "\"11\": \"" + Stan + "\"," +
                "\"41\": \"" + FomoConfig.TID + "\"," +
                "\"42\": \"" + FomoConfig.MID + "\"}";
            var nonce = GenerateNonce();
            var sign = SignData(payload, timestamp, nonce);


            using (var request = new HttpRequestMessage(new HttpMethod("POST"), FomoConfig.Url))
            {
                try
                {
                    request.Headers.TryAddWithoutValidation("X-Authentication-KeyId", FomoConfig.KeyId);
                    request.Headers.TryAddWithoutValidation("X-Authentication-Method", "SHA256WithRSA");
                    request.Headers.TryAddWithoutValidation("X-Authentication-Nonce", nonce);
                    request.Headers.TryAddWithoutValidation("X-Authentication-Sign", sign);
                    request.Headers.TryAddWithoutValidation("X-Authentication-Timestamp", timestamp);
                    request.Headers.TryAddWithoutValidation("X-Authentication-Version", "1.1");

                    request.Content = new StringContent(payload);
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    var response = await httpClient.SendAsync(request);

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        ret = true;
                        var json = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                        var responseCode = result["39"];
                        LogService.LogInfo("Querry reponse code: " + responseCode);
                        // Payment success
                        if (responseCode == "00")
                        {
                            var cmd = new NsqPaymentCallbackResponseCommand(CurrentPaymentType.ToString())
                            {
                                Response = new PaymentResponseData()
                                {
                                    Message = $"Payment Success. Code = {responseCode}",
                                    State = PaymentState.Success,
                                    ResponseObject = new
                                    {
                                        Amount = CurrentTransaction.Amount,
                                        AppLabel = AppLabel,
                                        CardLabel = CurrentTransaction.PaymentType.ToString(),
                                        Mid = CurrentTransaction.Mid,
                                        Tid = CurrentTransaction.Tid
                                    },
                                    OtherInfo = null
                                }
                            };
                            _nsqMessageProducerService.SendPaymentResponseCommand(cmd);
                            // Finished
                            StopPollingRequest();
                        }
                        else if (responseCode == "09")
                        {
                            LogService.LogInfo("Waiting for payment. Keep polling..");
                        }
                        // Inprocess or failed
                        else
                        {
                            if (!FomoResponseCodes.QUERY_RESPONSES.TryGetValue(responseCode, out string msg))
                                msg = "Unknown response code";
                            LogService.LogInfo($"Transaction Error: {responseCode}, Message: {msg}");
                            var cmd = new NsqPaymentCallbackResponseCommand(CurrentPaymentType.ToString())
                            {
                                Response = new PaymentResponseData()
                                {
                                    Message = $"Payment Failed. Code = {responseCode}",
                                    State = PaymentState.Failure,
                                    ResponseObject = new
                                    {
                                        Amount = CurrentTransaction.Amount,
                                        AppLabel = AppLabel,
                                        CardLabel = CurrentTransaction.PaymentType.ToString(),
                                        Mid = CurrentTransaction.Mid,
                                        Tid = CurrentTransaction.Tid
                                    },
                                    OtherInfo = null
                                }
                            };
                            _nsqMessageProducerService.SendPaymentResponseCommand(cmd);
                            // Finished
                            StopPollingRequest();
                        }
                    }
                    else
                    {
                        LogService.LogInfo("ERROR  - QueryRequest");
                        LogService.LogInfo($"Http Error code: {response.StatusCode}");
                        LogService.LogInfo($"Content: {response.Content.ReadAsStringAsync()}");
                    }
                }
                catch (Exception ex)
                {
                    LogService.LogInfo("ERROR  - QueryRequest");
                    LogService.LogError(ex);

                }
            }
            return ret;

        }

        private void StartPollingRequest()
        {
            LogService.LogInfo("Start polling");
            _pollingTimer.Enabled = true;
            _pollingTimer.Start();
        }

        private void StopPollingRequest()
        {
            LogService.LogInfo("Stop polling");
            _pollingTimer.Enabled = false;
            _pollingTimer.Stop();
        }
        private Task<bool> queryTask;
        private void _pollingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (queryTask == null || queryTask.IsCompleted)
                queryTask = QueryRequestAsync();
            else
                LogService.LogInfo("QueryRequestAsync is not finished");


        }

        public async Task<Tuple<bool, string, string>> GenerateQRAsync(ConditionCodes conditionCode, int amount, string orderNumber, string description)
        {
            RandomStan();
            return await SaleRequestAsync(conditionCode, amount, orderNumber, description);
        }
        public async Task<bool> BatchSubmitAsync()
        {
            try
            {
                LogService.LogInfo("Run BatchSubmitc");
                var timestamp = GetTimestamp().ToString();
                var s_timestamp = DateTime.Now.ToString("MMddHHmmss");
                var payload = "{\"0\": \"0500\",\"1\": \"2200000000c00000\",\"3\": \"000000\",\"7\": \"" + s_timestamp +
                    "\",\"41\": \"" + FomoConfig.TID +
                    "\",\"42\": \"" + FomoConfig.MID + "\"}";
                var nonce = GenerateNonce();
                var sign = SignData(payload, timestamp, nonce);
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), FomoConfig.Url))
                {
                    request.Headers.TryAddWithoutValidation("X-Authentication-KeyId", FomoConfig.KeyId);
                    request.Headers.TryAddWithoutValidation("X-Authentication-Method", "SHA256WithRSA");
                    request.Headers.TryAddWithoutValidation("X-Authentication-Nonce", nonce);
                    request.Headers.TryAddWithoutValidation("X-Authentication-Sign", sign);
                    request.Headers.TryAddWithoutValidation("X-Authentication-Timestamp", timestamp);
                    request.Headers.TryAddWithoutValidation("X-Authentication-Version", "1.1");

                    request.Content = new StringContent(payload);
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    var response = await httpClient.SendAsync(request);

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {

                        var json = response.Content.ReadAsStringAsync().Result;
                        var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                        var responseCode = result["39"];
                        LogService.LogInfo("Querry reponse code: " + responseCode);
                        if (responseCode == "00")
                        {
                            LogService.LogInfo("Batch submitted success");
                            return true;
                        }
                        else
                        {
                            LogService.LogError("Batch submitted FAILED");
                        }
                    }
                    else
                    {
                        LogService.LogError($"Batch submitted FAILED. Request Error; HttpCode: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);

            }
            return false;
        }
        public async Task CancelRequestAsync(string stan = null)
        {
            try
            {

                if (stan == null)
                    stan = Stan;

                LogService.LogInfo("Cancel request | Stan: " + stan);
                var timestamp = GetTimestamp().ToString();
                var s_timestamp = DateTime.Now.ToString("MMddHHmmss");
                var payload = "{\"0\": \"0460\"," +
                    "\"1\": \"2220000000c00000\"," +
                    "\"3\": \"000000\"," +
                    "\"7\": \"" + s_timestamp + "\"," +
                    "\"11\": \"" + stan + "\"," +
                    "\"41\": \"" + FomoConfig.TID + "\"," +
                    "\"42\": \"" + FomoConfig.MID + "\"}";
                var nonce = GenerateNonce();
                var sign = SignData(payload, timestamp, nonce);


                using (var request = new HttpRequestMessage(new HttpMethod("POST"), FomoConfig.Url))
                {
                    request.Headers.TryAddWithoutValidation("X-Authentication-KeyId", FomoConfig.KeyId);
                    request.Headers.TryAddWithoutValidation("X-Authentication-Method", "SHA256WithRSA");
                    request.Headers.TryAddWithoutValidation("X-Authentication-Nonce", nonce);
                    request.Headers.TryAddWithoutValidation("X-Authentication-Sign", sign);
                    request.Headers.TryAddWithoutValidation("X-Authentication-Timestamp", timestamp);
                    request.Headers.TryAddWithoutValidation("X-Authentication-Version", "1.1");

                    request.Content = new StringContent(payload);
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    var response = await httpClient.SendAsync(request);

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {

                        var json = response.Content.ReadAsStringAsync().Result;
                        var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                        var responseCode = result["39"];
                        LogService.LogInfo("Querry reponse code: " + responseCode);
                        if (responseCode == "00")
                        {
                            LogService.LogInfo("Cancelled transaction");
                        }
                        else
                        {
                            LogService.LogInfo($"Falied to cancel transaction, code: {responseCode}");
                        }
                    }
                    else
                    {
                        LogService.LogInfo("Cancel request failed: " + response.ToString());
                    }
                }

            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
            }
        }



        #region Utils
        private void RandomStan()
        {
            var r = new Random();
            var n = r.Next(1, 999999);
            Stan = n.ToString().PadLeft(6, '0');
        }

        private string GenerateNonce()
        {
            var s = RandomString(256);
            string result = SHA512(s).ToLower();
            return result;
        }

        private int GetTimestamp()
        {
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            return unixTimestamp;
        }

        private string SignData(string payload, string timestamp, string nonce)
        {
            try
            {
                var pKey = ImportPrivateKey(FomoConfig.VendorPrivateKey);
                var data = payload + timestamp + nonce;
                byte[] bytes = Encoding.ASCII.GetBytes(data);
                var sign = pKey.SignData(bytes, CryptoConfig.MapNameToOID("SHA256"));
                var hex = BitConverter.ToString(sign).Replace("-", string.Empty).ToLower();
                return hex;
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                throw ex;
            }

        }

        private void LoadSettingsAndKeys(IConfigurationRoot config)
        {
            try
            {
                FomoConfig = new FomoConfiguration()
                {
                    KeyId = config["Fomo:KeyId"],
                    MID = config["Fomo:MID"],
                    TID = config["Fomo:TID"],
                    Url = config["Fomo:Url"],
                };

                FomoConfig.FomoPublicKey = System.IO.File.ReadAllText(EnvironmentVariables.CurrentPath + "\\FomoKeys\\" + config["Fomo:FomoPublicKey"]);
                FomoConfig.VendorPrivateKey = System.IO.File.ReadAllText(EnvironmentVariables.CurrentPath + "\\FomoKeys\\" + config["Fomo:VendorPrivateKey"]);
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
            }
        }

        private string SHA512(string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            using (var hash = System.Security.Cryptography.SHA512.Create())
            {
                var hashedInputBytes = hash.ComputeHash(bytes);

                var hashedInputStringBuilder = new System.Text.StringBuilder(128);
                foreach (var b in hashedInputBytes)
                    hashedInputStringBuilder.Append(b.ToString("X2"));
                return hashedInputStringBuilder.ToString();
            }
        }

        private string RandomString(int size, bool lowerCase = true)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            if (lowerCase)
                return builder.ToString().ToLower();
            return builder.ToString();
        }

        private static RSACryptoServiceProvider ImportPrivateKey(string pem)
        {
            PemReader pr = new PemReader(new StringReader(pem));
            AsymmetricCipherKeyPair KeyPair = (AsymmetricCipherKeyPair)pr.ReadObject();
            RSAParameters rsaParams = DotNetUtilities.ToRSAParameters((RsaPrivateCrtKeyParameters)KeyPair.Private);

            RSACryptoServiceProvider csp = new RSACryptoServiceProvider();// cspParams);
            csp.ImportParameters(rsaParams);
            return csp;
        }

        public void Dispose()
        {
            _pollingTimer.Enabled = false;
            _pollingTimer.Stop();
        }

        public void HandleMessage(IMessage message)
        {
            var msg = Encoding.UTF8.GetString(message.Body);
            var obj = JsonConvert.DeserializeObject<UniversalCommands>(msg);
            LogService.LogInfo($"NSQ Request: {msg}");
            if (obj.IsTimeout())
            {
                LogService.LogInfo("NSQ message is timedout");
                return;
            }

            if (obj.Command == UniversalCommandConstants.EnablePaymentCommand)
            {
                var cmd = JsonConvert.DeserializeObject<NsqEnablePaymentCommand>(msg);

                CompleteReceivedAsync(cmd).Wait();
                if (obj.Command == UniversalCommandConstants.EnablePaymentCommand)
                {

                    //Activate device 
                    int priceInCents = (int)(cmd.Amount * 100);
                    var isActivated = false;
                    if (Enum.TryParse<PaymentTypes>(cmd.PaymentType, out PaymentTypes pType))
                    {
                        if (pType == PaymentTypes.QR_DASH)
                        {

                            GenerateQRAsync(ConditionCodes.SingtelDashQr, priceInCents, cmd.TransactionId.ToString(), $"TxId: {cmd.TransactionId.ToString()}, Amount: {priceInCents}c")
                                .ContinueWith(result =>
                                {
                                    if (result.IsCompleted && !result.IsFaulted && !result.IsCanceled)
                                    {
                                        var command = new NsqEnablePaymentResponseCommand(cmd.PaymentType);
                                        command.TransactionId = cmd.TransactionId;

                                        isActivated = result.Result.Item1;
                                        command.CustomData.QR = result.Result.Item3;
                                        if (!isActivated)
                                        {
                                            command.CustomData.ErrorMessage = result.Result.Item3;
                                            command.CustomData.ErrorCode = result.Result.Item2;
                                        }

                                        command.Code = isActivated ? 0 : 1;
                                        var isSent = _nsqMessageProducerService.SendPaymentResponseCommand(command);
                                        LogService.LogInfo($"Send NSQ PaymentResponse: {(isActivated ? "Activated" : "Failed to Activate")}, isSent:{isSent}");
                                    }
                                    else
                                    {
                                        LogService.LogInfo("Error:" + result.Exception?.Message);
                                        var command = new NsqEnablePaymentResponseCommand(cmd.PaymentType);
                                        command.TransactionId = cmd.TransactionId;
                                        command.Code = isActivated ? 0 : 1;
                                        _nsqMessageProducerService.SendPaymentResponseCommand(command);
                                    }
                                });
                        }
                        else
                        {

                            var command = new NsqEnablePaymentResponseCommand(cmd.PaymentType);
                            command.TransactionId = cmd.TransactionId;
                            command.CustomData.ErrorMessage = "Payment method is not supported";
                            command.Code = isActivated ? 0 : 1;
                            _nsqMessageProducerService.SendPaymentResponseCommand(command);
                        }
                    }



                }
            }
            else if (obj.Command == UniversalCommandConstants.DisablePaymentCommand)
            {
                var cmd = JsonConvert.DeserializeObject<NsqDisablePaymentCommand>(msg);
                CompleteReceivedAsync(cmd).Wait();
                CancelRequestAsync();
                StopPollingRequest();
            }
            if (obj.Command == UniversalCommandConstants.Ping)
            {
                //TODO: check service is running properly
                if (true)//util.IsConnected)
                    PublishDeviceInfo(false);
                else
                {
                    PublishDeviceInfo(true, "Something went wrong");
                }
                CompleteReceivedAsync(new NsqPaymentACKResponseCommand(obj.CommandId, "None")).Wait();
            }
        }

        public void LogFailedMessage(IMessage message)
        {
            throw new NotImplementedException();
        }
        private void PublishDeviceInfo(bool hasError, string errorMessage = "")
        {
            //publish service informative description
            var deviceInfoCmd = new DeviceInfoCommand();
            deviceInfoCmd.CommandObject.Name = this.SERVICE_NAME;
            deviceInfoCmd.CommandObject.Type = this.SERVICE_TYPE;
            deviceInfoCmd.CommandObject.HasError = hasError;
            if (!string.IsNullOrEmpty(errorMessage))
                deviceInfoCmd.CommandObject.Errors.Add(errorMessage);
            _nsqMessageProducerService.SendNsqCommand(NsqTopics.PAYMENT_RESPONSE_TOPIC, deviceInfoCmd);
        }
        private async Task CompleteReceivedAsync(NsqPaymentCommandBase obj)
        {
            await Task.Delay(10);
            var command = new NsqPaymentACKResponseCommand(obj.CommandId, obj.PaymentType);
            _nsqMessageProducerService.SendPaymentResponseCommand(command);
        }
        #endregion
    }
}
