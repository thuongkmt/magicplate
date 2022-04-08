using Konbi.Common.Interfaces;
using KonbiBrain.Common.Messages;
using KonbiBrain.Common.Messages.Payment;
using KonbiBrain.Common.Services;
using KonbiBrain.Messages;
using KonbiBrain.WindowServices.IUC.COTF.Interfaces;
using KonbiBrain.WindowServices.IUC.COTF.Iuc;
using Konbini.Messages.Commands;
using Konbini.Messages.Enums;
using Konbini.Messages.Payment;
using Newtonsoft.Json;
using NsqSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KonbiBrain.WindowServices.IUC.COTF
{
    public class IucHandler : IIucHandler, INsqHandler
    {
        private readonly IucSerialPortInterface _terminal;
        private readonly IKonbiBrainLogService _logger;
        private readonly NsqMessageConsumerService _nsqConsumer;
        private readonly NsqMessageProducerService _nsqProducerService;
        private readonly string paymentType = PaymentTypes.IUC_API.ToString();
        private readonly string SERVICE_NAME ="IUC Terminal Controller";
        private readonly string SERVICE_TYPE = "PaymentController.Service";
        private CardReaderInterface _cardReader;

        public IucHandler(IKonbiBrainLogService logger)
        {
            _cardReader = new CardReaderInterface
            {

            };
            _cardReader.Connect();
            _cardReader.Initialize(true);
            _cardReader.TurnOffAllLeds();
            _cardReader.LatchAutoLock(false);
            _cardReader.OpenLatch(true);

            if (_terminal == null)
            {
                _terminal = AppBootstrapper.Current.GetInstance(typeof(IIucDeviceService), null) as IucSerialPortInterface;
                _terminal.OnSaleApproved = OnSaveApproved;
                _terminal.OnSaleCancelled = OnSaleCancelled;
                _terminal.OnSaleError = OnSaleError;
                _terminal.OnTerminalCallback = OnTerminalCallback;
            }
            
            _logger = logger;
            _nsqConsumer = new NsqMessageConsumerService(topic: NsqTopics.PAYMENT_REQUEST_TOPIC, handler: this,ClientId: "Iuc");
            
            _nsqProducerService = new NsqMessageProducerService();
        }

        public void Handle()
        {
            _terminal.ComportName = ConfigurationManager.AppSettings["terminal.comport"];
            if (!string.IsNullOrEmpty(_terminal.ComportName))
            {
                _terminal.ConnectPort(_terminal.ComportName, ((callbackMessage) =>
                {

                    _logger.LogInfo(callbackMessage);
                    var info = _terminal.TerminalInfo();
                    foreach (var keyPair in info)
                    {
                        _logger.LogInfo($"{keyPair.Key}: {keyPair.Value}");
                    }
                }));
            }
            if(_terminal.IsTerminalRunning)
            PublishDeviceInfo(false);
            else
                PublishDeviceInfo(true, "Cannot connect to IUC terminal");

        }

        public void HandleMessage(IMessage message)
        {
            var msg = Encoding.UTF8.GetString(message.Body);
            var obj = JsonConvert.DeserializeObject<UniversalCommands>(msg);
            //Check for message timeouts then do not make timeout messages.
            if (obj.IsTimeout())
                return;

            _logger.LogInfo($"NSQ Request: {msg}");

            if (obj.Command == UniversalCommandConstants.EnablePaymentCommand)
            {
                var cmd = JsonConvert.DeserializeObject<NsqEnablePaymentCommand>(msg);

                CompleteReceivedAsync(cmd).Wait();

                if (obj.Command == UniversalCommandConstants.EnablePaymentCommand)
                {
                   
                    // Activate device 
                    int priceInCents = (int)(cmd.Amount * 100);

                    var isActivated = EnablePayment(priceInCents, cmd.PaymentType);

                    var command = new NsqEnablePaymentResponseCommand(cmd.PaymentType);

                    command.TransactionId = cmd.TransactionId;
                    command.Code = isActivated ? 0 : 1;
                    _nsqProducerService.SendPaymentResponseCommand(command);

                }
            }            
            else if (obj.Command == UniversalCommandConstants.DisablePaymentCommand)
            {
                var cmd = JsonConvert.DeserializeObject<NsqDisablePaymentCommand>(msg);
                CompleteReceivedAsync(cmd).Wait();
                DisablePayment();
            }
            else if (obj.Command == UniversalCommandConstants.Ping)
            {
                if (_terminal.IsTerminalRunning)
                    PublishDeviceInfo(false);
                else
                {
                    PublishDeviceInfo(true, "Cannot connect to IUC terminal");
                }
                Task.Delay(500).Wait();
                CompleteReceivedAsync(new NsqPaymentACKResponseCommand(obj.CommandId, "None")).Wait();
            }
        }

        // Activate payment. this will enable iuc devices.
        private bool EnablePayment(int cents)
        {
            var result = _terminal.EnablePayment(cents);
            return result;
        }

        private  bool EnablePayment(int cents,string paymentType)
        {
            PaymentTypes p;
            if(Enum.TryParse<PaymentTypes>(paymentType,true, out p))
            {
                _terminal.PaymentType = p;
                var result = _terminal.EnablePayment(cents);
                return result;
            }
            _logger.LogIucApi($"Payment type is incorrect. {paymentType}");
            return false;
        }

        private bool DisablePayment(bool silent = false)
        {
            var result = _terminal.DisablePayment(silent);
            return result;
        }

        public void LogFailedMessage(IMessage message)
        {
            // Method intentionally left empty.
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
            _nsqProducerService.SendPaymentResponseCommand(deviceInfoCmd);
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
            ////override invalid card messages to make more sense. 
            //var code = response.ResponseCode.ToUpper();
            //if (code == "IC" || code == "IN" || code == "CO")
            //{
            //    response.Message = $"Card type isn't valid, this terminal is for {terminal.Mode} card only";
            //}
            //if (code == "PL")
            //{
            //    response.Message = response.Message + $" this terminal support {terminal.Mode} only";
            //}

            var cmd = new NsqPaymentCallbackResponseCommand(paymentType)
            {
                Response = new PaymentResponseData()
                {
                    Message = response.Message,
                    State = PaymentState.Failure,
                    ResponseObject = response,
                    OtherInfo = null
                }
            };
            _nsqProducerService.SendPaymentResponseCommand(cmd);
            //Task.Run(async () =>
            //{
            //    await Task.Delay(5000);
            //}).ContinueWith(task => {
            //    // continute payment
            //    _logger.LogIucApi("Payment failed. Re-enable terminals to try again. ");
            //    EnablePayment(response.Amount);
            //});
        }

        private IucSerialPortInterface OtherTerminal(IucSerialPortInterface terminal)
        {
            return _terminal;
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
