using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Castle.Core.Logging;
using KonbiBrain.Common.Messages;
using KonbiBrain.Common.Messages.Payment;
using KonbiBrain.Messages;
using KonbiCloud.Common;
using KonbiCloud.Configuration;
using KonbiCloud.Enums;
using KonbiCloud.Services;
using Konbini.Messages.Commands;
using Konbini.Messages.Enums;
using Konbini.Messages.Payment;
using Newtonsoft.Json;
using NsqSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KonbiCloud.RFIDTable
{
    public class PaymentManager : IPaymentManager, IHandler
    {
        private readonly IDetailLogService _detailLogService;
        private readonly ILogger _logger;
        private readonly IMessageProducerService nsqProducerService;
        private readonly IRepository<Services.Service> _serviceRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly ISettingManager _settingManager;
        public PaymentTypes CurrentPaymentMode { get; set; }
        public ConcurrentDictionary<PaymentTypes, string> AcceptedPaymentMethods { get; set; }
        private readonly Consumer consumer;
        private CancellationTokenSource ctx;
        private TransactionInfo transaction;

        private IUniversalCommands Command { get; set; }
        private object commandLock = new object();


        public event EventHandler<CommandEventArgs> DeviceFeedBack;

        public PaymentManager(IMessageProducerService nsqProducerService, IDetailLogService detailLogService, ILogger logger,
            IUnitOfWorkManager unitOfWorkManager,
            ISettingManager settingManager,
             IRepository<Services.Service> serviceRepository)
        {
            this._logger = logger;
            this._detailLogService = detailLogService;
            this.nsqProducerService = nsqProducerService;
            _unitOfWorkManager = unitOfWorkManager;
            _serviceRepository = serviceRepository;
            _settingManager = settingManager;

            consumer = new Consumer(NsqTopics.PAYMENT_RESPONSE_TOPIC, NsqConstants.NsqDefaultChannel);
            consumer.AddHandler(this);

            // use ConnectToNsqd instead of using ConnectToNsqLookupd because  we use standalone nsq service not cluster one.
            consumer.ConnectToNsqd(NsqConstants.NsqUrlConsumer);
            //consumer.ConnectToNsqLookupd(NsqConstants.NsqUrlConsumer);

            AcceptedPaymentMethods = new ConcurrentDictionary<PaymentTypes, string>();

            ReloadAcceptedPayments();
        }

        public ConcurrentDictionary<PaymentTypes, string> ReloadAcceptedPayments()
        {

            var paymentAcceptStr = _settingManager.GetSettingValue(AppSettingNames.PaymentsAccept);
            AcceptedPaymentMethods.Clear();
            if (!string.IsNullOrEmpty(paymentAcceptStr))
            {

                paymentAcceptStr.Split(',').ToList().ForEach(m =>
                {
                    var mod = m.Trim();
                    if (Enum.TryParse<PaymentTypes>(mod, out PaymentTypes result))
                    {
                        var paymentCaption = result.ToString();
                        switch (result)
                        {
                            case PaymentTypes.IUC_CEPAS:
                                paymentCaption = "EZLink";
                                break;
                            case PaymentTypes.IUC_CONTACTLESS:
                                paymentCaption = "Contactless";
                                break;
                            case PaymentTypes.QR_DASH:
                                paymentCaption = "Dash QR";
                                break;
                            case PaymentTypes.KONBI_CREDITS:
                                paymentCaption = "Konbi Credits";
                                break;
                            case PaymentTypes.FACIAL_RECOGNITION:
                                paymentCaption = "FACIAL_RECOGNITION";
                                break;
                        }

                        AcceptedPaymentMethods[result] = paymentCaption;
                    }

                });
            }
            return AcceptedPaymentMethods;
        }

        /// <summary>
        /// Activate payment base one transaction info.
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public async Task<CommandState> ActivatePaymentAsync(TransactionInfo transaction)
        {
            this.transaction = transaction;
            var amountExcludedTAX = transaction.Amount / (1 + transaction.TaxPercentage / 100);
            var discountAmount = amountExcludedTAX * transaction.DiscountPercentage / 100;
            var taxAmount = amountExcludedTAX * transaction.TaxPercentage / 100;
            var subTotal = amountExcludedTAX - discountAmount;
            var actualAmount = subTotal + taxAmount;

            ctx?.Cancel();
            lock (commandLock)
            {
                Command = new NsqEnablePaymentCommand(
                    transaction.PaymentType.ToString(),
                     transaction.Id,
                     actualAmount
                 );
                CurrentPaymentMode = transaction.PaymentType;
                var cmdIsSent = false;

                if (transaction.PaymentType == PaymentTypes.IUC_CEPAS || transaction.PaymentType == PaymentTypes.IUC_CONTACTLESS)
                {
                    cmdIsSent = nsqProducerService.SendPaymentCommand(Command);
                }
                else if (transaction.PaymentType == PaymentTypes.KONBI_CREDITS)
                {
                    cmdIsSent = nsqProducerService.SendKonbiCreditPaymentCommand(Command);
                }
                else if (transaction.PaymentType == PaymentTypes.FACIAL_RECOGNITION)
                {
                    cmdIsSent = nsqProducerService.SendFacialRecognitionPaymentCommand(Command);
                }
                else if (transaction.PaymentType == PaymentTypes.QR_DASH)
                {
                    cmdIsSent = nsqProducerService.SendNsqCommand(NsqTopics.PAYMENT_QR_REQUEST_TOPIC, Command);
                }

                if (cmdIsSent)
                {
                    Command.CommandState = CommandState.SendSuccess;
                }
                else
                {
                    Command.CommandState = CommandState.Failed;
                }
            }

            // increase waiting time to get QR 
            if (transaction.PaymentType == PaymentTypes.QR_DASH)
                ctx = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            else
                ctx = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            try
            {

                while (Command.CommandState == CommandState.SendSuccess && Command.Command == UniversalCommandConstants.EnablePaymentCommand && !ctx.Token.IsCancellationRequested)
                {
                    await Task.Delay(50, ctx.Token);
                }
                if (ctx.Token.IsCancellationRequested)
                    Command.CommandState = CommandState.Cancelled;

            }
            catch (OperationCanceledException)
            {
                Command.CommandState = CommandState.TimeOut;

            }

            return Command.CommandState;

        }
        /// <summary>
        /// Deactivate payment device
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public async Task<CommandState> DeactivatePaymentAsync(TransactionInfo transaction)
        {
            ctx?.Cancel();
            lock (commandLock)
            {
                Command = new NsqDisablePaymentCommand(CurrentPaymentMode.ToString(), transaction.Id);
                bool cmdIsSent = false;
                if (transaction.PaymentType == PaymentTypes.IUC_CEPAS || transaction.PaymentType == PaymentTypes.IUC_CONTACTLESS || transaction.PaymentType == PaymentTypes.KONBI_CREDITS)
                {
                    cmdIsSent = nsqProducerService.SendPaymentCommand(Command);
                }
                else if (transaction.PaymentType == PaymentTypes.QR_DASH)
                {
                    cmdIsSent = nsqProducerService.SendNsqCommand(NsqTopics.PAYMENT_QR_REQUEST_TOPIC, Command);
                }

                if (cmdIsSent)
                {
                    Command.CommandState = CommandState.SendSuccess;
                }
                else
                {
                    Command.CommandState = CommandState.Failed;

                }

            }
            ctx = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            try
            {

                while (Command.CommandState == CommandState.SendSuccess && Command.Command == UniversalCommandConstants.DisablePaymentCommand && !ctx.Token.IsCancellationRequested)
                {
                    await Task.Delay(50, ctx.Token);
                }
                if (ctx.Token.IsCancellationRequested)
                    Command.CommandState = CommandState.Cancelled;

            }
            catch (OperationCanceledException)
            {
                Command.CommandState = CommandState.TimeOut;

            }

            return Command.CommandState;
        }

        public void HandleMessage(IMessage message)
        {
            try
            {
                // process for ACK message where  telling that the request  has been sent to device successfully.
                var msg = Encoding.UTF8.GetString(message.Body);
                var obj = JsonConvert.DeserializeObject<UniversalCommands>(msg);

                if (obj.Command == UniversalCommandConstants.PaymentACKCommand)
                {
                    if (Command != null && Command.CommandId == obj.CommandId)
                    {
                        Command.CommandState = CommandState.Received;
                    }
                }

                else if (obj.Command == UniversalCommandConstants.EnablePaymentCommand)
                {
                    OnDeviceFeedBack(new CommandEventArgs() { Command = JsonConvert.DeserializeObject<NsqEnablePaymentResponseCommand>(msg) });
                }

                else if (obj.Command == UniversalCommandConstants.DeviceInfo)
                {
                    var Command = JsonConvert.DeserializeObject<DeviceInfoCommand>(msg);
                    UpdateServiceStatus(Command.CommandObject);
                }

                else if (obj.Command == UniversalCommandConstants.PaymentDeviceResponse)
                {
                    var command = JsonConvert.DeserializeObject<NsqPaymentCallbackResponseCommand>(msg);

                    if (command != null)
                    {
                        _detailLogService.Log($"Payment Type: {CurrentPaymentMode} Payment Response:");
                        _detailLogService.Log(JsonConvert.SerializeObject(command));

                        switch (command.Response.State)
                        {
                            case PaymentState.Success:
                                {
                                    if (command.Response.ResponseObject != null)
                                    {
                                        var paymentInfo = new Transactions.CashlessDetail();
                                        paymentInfo.Aid = command.Response?.ResponseObject?.Aid;
                                        paymentInfo.Amount = command.Response?.ResponseObject?.Amount;
                                        paymentInfo.AppLabel = command.Response.ResponseObject.AppLabel ?? default(string);
                                        paymentInfo.ApproveCode = command.Response.ResponseObject.ApproveCode ?? default(string);
                                        paymentInfo.Batch = command.Response.ResponseObject.Batch ?? default(string);
                                        paymentInfo.CardLabel = command.Response.ResponseObject.CardLabel ?? default(string);
                                        paymentInfo.CardNumber = command.Response.ResponseObject.CardNumber ?? default(string);
                                        paymentInfo.EntryMode = command.Response.ResponseObject.EntryMode ?? default(string);
                                        paymentInfo.Invoice = command.Response.ResponseObject.Invoice ?? default(string);
                                        paymentInfo.Mid = command.Response.ResponseObject.Mid ?? default(string);
                                        paymentInfo.Rrn = command.Response.ResponseObject.Rrn ?? default(string);
                                        paymentInfo.Tc = command.Response.ResponseObject.Tc ?? default(string);
                                        paymentInfo.Tid = command.Response.ResponseObject.Tid ?? default(string);
                                        transaction.CashlessInfo = paymentInfo;

                                        if (transaction.PaymentType == PaymentTypes.KONBI_CREDITS || transaction.PaymentType ==PaymentTypes.FACIAL_RECOGNITION)
                                        {
                                            transaction.KonbiCreditBalance = command.Response?.ResponseObject?.KonbiCreditBalance;
                                            transaction.IsSufficientFund = command.Response?.ResponseObject?.IsSufficientFund;
                                        }
                                    }
                                }
                                break;
                        }
                    }
                    OnDeviceFeedBack(new CommandEventArgs() { Command = command });
                }
            }
            catch (Exception ex)
            {
                // dont catch unexpected message
                _logger?.Error(ex.Message, ex);
            }
        }

        private void UpdateServiceStatus(DeviceInfoCommandPayload payload)
        {
            using (var unitOfWork = _unitOfWorkManager.Begin())
            {
                try
                {
                    var service = _serviceRepository.FirstOrDefault(el => el.Type == payload.Type.Trim());
                    if (service != null)
                    {
                        service.Name = payload.Name.Trim();
                        service.IsError = payload.HasError;
                        service.ErrorMessage = payload.Errors.FirstOrDefault();
                    }
                    else
                    {
                        _serviceRepository.Insert(new Services.Service() { Name = payload.Name.Trim(), Type = payload.Type.Trim(), IsError = payload.HasError, ErrorMessage = payload.Errors.FirstOrDefault() });
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("ERROR: PaymentManger.UpdateServiceStatus");

                    _logger.Error(ex.Message, ex);
                }

                finally
                {
                    unitOfWork.Complete();
                }

            }
        }
        public void LogFailedMessage(IMessage message)
        {
            // Method intentionally left empty.
        }
        protected void OnDeviceFeedBack(CommandEventArgs e)
        {
            DeviceFeedBack?.Invoke(this, e);
        }
        private bool _isSendingPingCommand = false;
        public async Task<bool> PingAsync(string args = "")
        {
            if (_isSendingPingCommand)
            {
                var count = 0;
                while (_isSendingPingCommand && count < 100)
                {
                    await Task.Delay(100);
                    count++;
                }
            }
            _isSendingPingCommand = true;

            var cmd = new PingCommand();
            cmd.CommandId = Guid.NewGuid();
            Command = cmd;
            var result = await SendCommandAsync(args);
            _isSendingPingCommand = false;
            return result == CommandState.Received;

        }
        private async Task<CommandState> SendCommandAsync(string target = "PaymentController.Service")
        {
            ctx?.Cancel();
            lock (commandLock)
            {
                var isSent = false;
                if (target == ServiceTypeConstants.PAYMENT_CONTROLLER)
                    isSent = nsqProducerService.SendPaymentCommand(Command);
                else if (target == ServiceTypeConstants.PAYMENT_QR_CONTROLLER)
                    isSent = nsqProducerService.SendNsqCommand(NsqTopics.PAYMENT_QR_REQUEST_TOPIC, Command);
                else if (target == ServiceTypeConstants.KONBI_CREDITS_PAYMENT_CONTROLLER)
                    isSent = nsqProducerService.SendKonbiCreditPaymentCommand(Command);
                else if (target == ServiceTypeConstants.FACIAL_RECOGNITION_PAYMENT_CONTROLLER)
                    isSent = nsqProducerService.SendFacialRecognitionPaymentCommand(Command);

                if (isSent)
                {
                    Command.CommandState = CommandState.SendSuccess;
                }
                else
                {
                    Command.CommandState = CommandState.Failed;

                }

            }
            ctx = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            try
            {

                while (Command.CommandState == CommandState.SendSuccess && !ctx.Token.IsCancellationRequested)
                {
                    await Task.Delay(50, ctx.Token);
                }
                if (ctx.Token.IsCancellationRequested)
                    Command.CommandState = CommandState.Cancelled;

            }
            catch (OperationCanceledException)
            {
                Command.CommandState = CommandState.TimeOut;

            }

            return Command.CommandState;
        }
    }
    public class CommandEventArgs : EventArgs
    {
        public IUniversalCommands Command { get; set; }
        public string CommandStr { get; set; }
    }
}
