using IucBrain;
using Konbi.Common.Interfaces;
using KonbiBrain.Common.Messages;
using KonbiBrain.Common.Messages.Payment;
using KonbiBrain.Common.Services;
using KonbiBrain.Messages;
using KonbiBrain.WindowServices.IUC.Interfaces;
using KonbiBrain.WindowServices.IUC.Iuc;
using Konbini.Messages.Enums;
using Konbini.Messages.Payment;
using Newtonsoft.Json;
using NsqSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.WindowServices.IUC
{
    public class IucHandler : IIucHandler, INsqHandler
    {
        private readonly IucSerialPortInterface _terminal1;
        private readonly IucSerialPortInterface _terminal2;
        private readonly IKonbiBrainLogService _logger;
        private readonly NsqMessageConsumerService _nsqConsumer;
        private readonly NsqMessageProducerService _nsqProducerService;

        private readonly string paymentType = PaymentTypes.IUC_API.ToString();



        public IucHandler(IKonbiBrainLogService logger)
        {

            if (_terminal1 == null)
            {
                _terminal1 = AppBootstrapper.Current.GetInstance(typeof(IIucDeviceService), null) as IucSerialPortInterface;
                _terminal1.OnSaleApproved = OnSaveApproved;
                _terminal1.OnSaleCancelled = OnSaleCancelled;
                _terminal1.OnSaleError = OnSaleError;
                _terminal1.OnTerminalCallback = OnTerminalCallback;
            }

            if (_terminal2 == null)
            {

                _terminal2 = AppBootstrapper.Current.GetInstance(typeof(IIucDeviceService), null) as IucSerialPortInterface;
                _terminal2.OnSaleApproved = OnSaveApproved;
                _terminal2.OnSaleCancelled = OnSaleCancelled;
                _terminal2.OnSaleError = OnSaleError;
                _terminal2.OnTerminalCallback = OnTerminalCallback;
            }

            _logger = logger;
            _nsqConsumer = new NsqMessageConsumerService(topic: NsqTopics.PAYMENT_REQUEST_TOPIC, handler: this);
            _nsqProducerService = new NsqMessageProducerService();

        }

        public void Handle()
        {
            _terminal1.Mode = string.IsNullOrEmpty(ConfigurationManager.AppSettings["terminal1.mode"]) ? IucPaymentMode.CONTACTLESS : (ConfigurationManager.AppSettings["terminal1.mode"].ToUpper() == "CEPAS" ? IucPaymentMode.CEPAS : IucPaymentMode.CONTACTLESS);
            _terminal1.ComportName = ConfigurationManager.AppSettings["terminal1.comport"];
            if (!string.IsNullOrEmpty(_terminal1.ComportName))
            {
                _terminal1.ConnectPort(_terminal1.ComportName, ((callbackMessage) =>
                {

                    _logger.LogInfo(callbackMessage);
                    var info = _terminal1.TerminalInfo();
                    foreach (var keyPair in info)
                    {
                        _logger.LogInfo($"{keyPair.Key}: {keyPair.Value}");
                    }
                }));
            }

            _terminal2.Mode = string.IsNullOrEmpty(ConfigurationManager.AppSettings["terminal2.mode"]) ? IucPaymentMode.CONTACTLESS : ConfigurationManager.AppSettings["terminal2.mode"].ToUpper() == "CEPAS" ? IucPaymentMode.CEPAS : IucPaymentMode.CONTACTLESS;
            _terminal2.ComportName = ConfigurationManager.AppSettings["terminal2.comport"];
            if (!string.IsNullOrEmpty(_terminal2.ComportName))
            {
                _terminal2.ConnectPort(_terminal2.ComportName, ((callbackMessage) =>
                {

                    _logger.LogInfo(callbackMessage);
                    var info = _terminal2.TerminalInfo();
                    foreach (var keyPair in info)
                    {
                        _logger.LogInfo($"{keyPair.Key}: {keyPair.Value}");
                    }
                }));
            }

        }

        public void HandleMessage(IMessage message)
        {
            var msg = Encoding.UTF8.GetString(message.Body);
            var obj = JsonConvert.DeserializeObject<UniversalCommands>(msg);

            _logger.LogInfo($"NSQ Request: {msg}");
            if (obj.Command == UniversalCommandConstants.EnablePaymentCommand)
            {
                var cmd = JsonConvert.DeserializeObject<NsqEnablePaymentCommand>(msg);
                CompleteReceivedAsync(cmd).Wait();
                if (obj.Command == UniversalCommandConstants.EnablePaymentCommand)
                {

                    //Activate device 
                    int priceInCents = (int)(cmd.Amount * 100);


                    var isActivated = EnablePayment(priceInCents);

                    var command = new NsqEnablePaymentResponseCommand(paymentType);

                    command.TransactionId = cmd.TransactionId;
                    command.Code = isActivated ? 0 : 1;
                    _nsqProducerService.SendPaymentResponseCommand(command);

                }
            }
            if (obj.Command == UniversalCommandConstants.DisablePaymentCommand)
            {
                var cmd = JsonConvert.DeserializeObject<NsqDisablePaymentCommand>(msg);
                CompleteReceivedAsync(cmd).Wait();
                DisablePayment();
            }
        }
        /// <summary>
        /// Activate payment. this will enable iuc devices.
        /// </summary>
        private bool EnablePayment(int cents)
        {
            var result1 = _terminal1.EnablePayment(cents);
            var result2 = _terminal2.EnablePayment(cents);
            return result1 || result2;
        }

        private bool DisablePayment(bool silent = false)
        {
            var result1 = _terminal1.DisablePayment(silent);
            var result2 = _terminal2.DisablePayment(silent);
            return result1 || result2;
        }

        public void LogFailedMessage(IMessage message)
        {
            // Method intentionally left empty.
        }
        private async Task CompleteReceivedAsync(NsqPaymentCommandBase obj)
        {
            await Task.Delay(10);
            var command = new NsqPaymentACKResponseCommand(obj.CommandId, obj.PaymentType);
            _nsqProducerService.SendPaymentResponseCommand(command);
        }
        #region Terminal Feedbacks
        private void OnSaveApproved(object sender, IucApprovedResponse response)
        {

            SendPaymentResponse(response, PaymentState.Success);
            var terminal = sender as IucSerialPortInterface;
            if (terminal != null)
                OtherTerminal(terminal).DisablePayment(true);
        }
        private void OnSaleError(object sender, SaleResponse response)
        {
            var terminal = sender as IucSerialPortInterface;
            if (terminal != null)
                OtherTerminal(terminal).DisablePayment(true);
            //override invalid card messages to make more sense. 
            var code = response.ResponseCode.ToUpper();
            if(code == "IC" || code == "IN" || code == "CO")
            {
                response.Message = $"Card type isn't valid, this terminal is for {terminal.Mode} card only";
            }
            if (code == "PL")
            {
                response.Message = response.Message + $" this terminal support {terminal.Mode} only";
            }
            
            var cmd = new NsqPaymentCallbackResponseCommand(paymentType)
            {
                Response = new PaymentResponseData()
                {
                    Message = response.Message,
                    State = PaymentState.InProgress,
                    ResponseObject = response,
                    OtherInfo = null
                }
            };
            _nsqProducerService.SendPaymentResponseCommand(cmd);
            Task.Run(async () =>
            {
                await Task.Delay(5000);
            }).ContinueWith(task => {
                // continute payment
                _logger.LogIucApi("Payment failed. Re-enable terminals to try again. ");
                EnablePayment(response.Amount);
            }); 
        }

        private IucSerialPortInterface OtherTerminal(IucSerialPortInterface terminal)
        {
            if (terminal.InstanceId == _terminal1.InstanceId)
            {
                return _terminal2;
            }
            return _terminal1;
        }

        private void OnSaleCancelled(object sender, SaleResponse response)
        {
            var terminal = sender as IucSerialPortInterface;
            if (terminal != null)
                OtherTerminal(terminal).DisablePayment(true);
            SendPaymentResponse(response, PaymentState.Cancelled);
        }

        private void OnTerminalCallback(object sender, ResponseBase response)
        {
            var cmd = new NsqPaymentCallbackResponseCommand(paymentType)
            {
                Response = new PaymentResponseData()
                {
                    Message = response.Message,
                    State = PaymentState.InProgress,
                    ResponseObject = response,
                    OtherInfo = null
                }
            };
            _nsqProducerService.SendPaymentResponseCommand(cmd);
        }
        private void SendPaymentResponse(SaleResponse response, PaymentState state)
        {
            var cmd = new NsqPaymentCallbackResponseCommand(paymentType)
            {
                Response = new PaymentResponseData()
                {
                    Message = response.Message,
                    State = state,
                    ResponseObject = response,
                    OtherInfo = null
                }
            };
            _nsqProducerService.SendPaymentResponseCommand(cmd);
        }
        #endregion
    }
}
