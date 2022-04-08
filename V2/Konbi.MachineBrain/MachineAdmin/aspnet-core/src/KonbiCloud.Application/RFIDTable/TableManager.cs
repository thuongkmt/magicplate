using Abp.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KonbiCloud.RFIDTable.Cache;
using Abp.Runtime.Caching;
using KonbiCloud.Sessions;
using Abp.Dependency;

using KonbiBrain.Common.Messages.Payment;
using KonbiCloud.Common;
using System.Threading;
using KonbiBrain.Common.Messages;
using Konbini.Messages.Enums;
using Konbini.Messages.Commands.RFIDTable;
using KonbiCloud.SignalR;
using System.Diagnostics;
using Newtonsoft.Json;
using Abp.Configuration;
using KonbiCloud.Configuration;
using NsqSharp;
using KonbiBrain.Messages;
using KonbiBrain.Common.Messages.Camera;

using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.Timers;
using Konbini.Messages;
using Microsoft.Extensions.Configuration;
using KonbiCloud.Plate;
using Microsoft.EntityFrameworkCore;
using Konbini.Messages.Commands;
using KonbiCloud.ProductMenu;
using Abp.Domain.Uow;
using KonbiCloud.ProductMenu.Dtos;
using KonbiCloud.Products.Dtos;
using System.Dynamic;
using System.Collections.Concurrent;
using KonbiCloud.RFIDTable.Dtos;

namespace KonbiCloud.RFIDTable
{
    public class TableManager : ITableManager, IHandler
    {
        public static readonly string TableDeviceSettingGroup = "TableDeviceSettingGroup";
        public static readonly string PaymentDeviceGroup = "PaymentDevice";
        public static readonly string CustomerUIGroup = "CustomerUI";
        private static readonly string BUYER_STAFF_NAME = "Staff";
        private static readonly string BUYER_CONTRACTOR_NAME = "Contractor";

        private static readonly int DEFAULT_CUSTOMER_COUNTDOWN = 60;

        private readonly IDetailLogService _detailLogService;
        private readonly IRepository<Session, Guid> _sessionRepository;
        private readonly IRepository<ProductMenu.ProductMenu, Guid> _prodMenuRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<Services.Service> _serviceRepository;
        private readonly ICacheManager cacheManager;
        private readonly IRfidTableSignalRMessageCommunicator signalRCommunicator;
        private readonly ISettingManager _settingManager;
        private readonly int scanningStableAfterMiliseconds;
        private readonly IMessageCommunicator messageCommunicator;
        private readonly ITableSettingsManager tableSettingsManager;
        private readonly IBarcodeScannerMessageHandler barcodeScannerHandler;
        private readonly IPaymentManager paymentManager;
        private readonly IAlarmLightManager alarmLightManager;
        private CancellationTokenSource cancelTokenResetTransactionState;
        private string lastCustomerMessage = "";

        public ISettingManager SettingManager { get; set; }


        private IUniversalCommands Command { get; set; }

        private HashSet<ClientInfo> clients = new HashSet<ClientInfo>();
        public HashSet<ClientInfo> Clients
        {
            get { return clients; }
        }

        private TransactionInfo transaction;

        // Create list transaction info for print receipt.
        public List<TransactionInfo> lstTransaction = new List<TransactionInfo>();

        public bool IsPosModeOn { get; set; }
        public bool IsProductUpdating { get; set; }

        public bool OnSale
        {
            get
            {
                return !Clients.Any(el => el.Group == TableManager.TableDeviceSettingGroup);
            }
        }

        public TransactionInfo Transaction
        {
            get { return transaction; }
            set
            {
                lock (transactionLock)
                {
                    transaction = value;
                }
            }
        }

        private object transactionLock = new object();
        private readonly IMessageProducerService nsqProducerService;
        private readonly Consumer consumer;

        //check payment timeout
        private System.Timers.Timer timer;
        private bool isGeneratingTransaction = false;
        private int count = 0;
        private int PaymentErrorRetries = 0;

        public TableManager(
            IRfidTableSignalRMessageCommunicator signalRCommunicator,
            ICacheManager cacheManager,
            ITableSettingsManager tableSettingsManager,
            IBarcodeScannerMessageHandler barcodeScannerHandler,
            IPaymentManager paymentManager,
            IAlarmLightManager alarmLightManager,
            IMessageCommunicator messageCommunicator,
            ISettingManager settingManager,
            IMessageProducerService nsqProducerService,
            IDetailLogService detailLog,
            IUnitOfWorkManager unitOfWorkManager,
            IRepository<ProductMenu.ProductMenu, Guid> prodMenuRepository,
            IRepository<Services.Service> serviceRepository,
            IRepository<Session, Guid> sessionRepository)
        {
            _unitOfWorkManager = unitOfWorkManager;
            _prodMenuRepository = prodMenuRepository;
            this._sessionRepository = sessionRepository;
            this.tableSettingsManager = tableSettingsManager;
            this.barcodeScannerHandler = barcodeScannerHandler;
            this.barcodeScannerHandler.DeviceFeedBack += BarcodeScannerHandler_DeviceFeedBackAsync;
            this.tableSettingsManager.DeviceFeedBack += TableSettingsManager_DeviceFeedBackAsync;
            this.cacheManager = cacheManager;
            this.signalRCommunicator = signalRCommunicator;
            this.paymentManager = paymentManager;
            this.alarmLightManager = alarmLightManager;
            this.paymentManager.DeviceFeedBack += PaymentManager_DeviceFeedBack;
            this.messageCommunicator = messageCommunicator;
            this._settingManager = settingManager;
            this.nsqProducerService = nsqProducerService;
            this._detailLogService = detailLog;
            _serviceRepository = serviceRepository;
            consumer = new Consumer(NsqTopics.CAMERA_RESPONSE_TOPIC, NsqConstants.NsqDefaultChannel);
            consumer.AddHandler(this);
            // use ConnectToNsqd instead of using ConnectToNsqLookupd because  we use standalone nsq service not cluster one.
            consumer.ConnectToNsqd(NsqConstants.NsqUrlConsumer);
            //consumer.ConnectToNsqLookupd(NsqConstants.NsqUrlConsumer);

            scanningStableAfterMiliseconds = _settingManager.GetSettingValue<int>(AppSettingNames.ScanningStableAfterMiliseconds);
            if (scanningStableAfterMiliseconds <= 0)
            {
                scanningStableAfterMiliseconds = 2000;
            }
            // retrive tax value.
            var tax = GetTaxSettingsAsync().Result;
            Transaction = new TransactionInfo() { TaxPercentage = tax.Percentage, TaxName = tax.Name, MenuItems = new List<MenuItemInfo>()};
        }

        private async Task<TaxSettingsDto> GetTaxSettingsAsync()
        {
           
            try
            {

                var cacheItem = await cacheManager.GetCache(TaxSettingsCacheItem.CacheName).Get(TaxSettingsCacheItem.CacheName, async () =>
                {
                    var tableAppService = IocManager.Instance.Resolve<ITableAppService>();                
                    return await tableAppService.GetTaxSettingsInternalAsync();
                });
                if (cacheItem != null)
                    return cacheItem.TaxSettings;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
        }

        private void BarcodeScannerHandler_DeviceFeedBackAsync(object sender, CommandEventArgs e)
        {
            try
            {
                var cmd = JsonConvert.DeserializeObject<UniversalCommands<BarcodeScannerCommandPayload>>(e.CommandStr);
                var barcodeValue = cmd.CommandObject.BarcodeValue;
                _detailLogService.Log($"Machine Admin, Barcode Value: {barcodeValue}");
                ExecuteBarcodeScanningTransaction(barcodeValue);
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
            }
        }

        private void TableSettingsManager_DeviceFeedBackAsync(object sender, CommandEventArgs e)
        {
            // TODO: Confirm with team.
            if (e.Command.Command == UniversalCommandConstants.RfidTableConfiguration)
            {
                //var cmd = e.Command as ConfigCommand;
                //if (cmd != null)
                //{
                //    if(cmd.CommandObject.Action == "settingsResult")
                //    {
                //        signalRCommunicator.UpdateTableSettings(cmd.CommandObject.selectedComPort, cmd.CommandObject.ComPortAvaliable, cmd.CommandObject.IsServiceRunning);
                //    }

                //}
            }
            else if (e.Command.Command == UniversalCommandConstants.RfidTableDetectPlates)
            {
                var cmd = JsonConvert.DeserializeObject<UniversalCommands<DetectPlatesCommandPayload>>(e.CommandStr);
                var mess = "{\"type\":\"RFIDTable_DetectedDisc\",\"data\":";
                mess += JsonConvert.SerializeObject(cmd.CommandObject);
                mess += "}";
                messageCommunicator.SendRfidTableMessageToAllClient(new GeneralMessage { Message = mess });

                if (OnSale)
                {
                    try
                    {
                        cmd = JsonConvert.DeserializeObject<UniversalCommands<DetectPlatesCommandPayload>>(e.CommandStr);
                        var plates = cmd.CommandObject.Plates.Select(el => new PlateReadingInput() { UID = el.UID, UType = el.UType, UData = el.UData });
                        // processing transaction and notify customer UI
                        ProcessTransactionAsync(plates).ContinueWith(taskResult =>
                        {
                            if (taskResult.IsCompleted && taskResult.IsFaulted == false && taskResult.IsCanceled == false)
                                signalRCommunicator.UpdateTransactionInfo(taskResult.Result);
                        });
                    }
                    catch (Exception ex)
                    {
                        _detailLogService.Log($"Exception: {ex.Message}");
                    }
                }
                else
                {
                    // notify Admin about detecting dishes
                    signalRCommunicator.SendAdminDetectedPlates(cmd.CommandObject.Plates);
                }
            }
        }

        private void PaymentManager_DeviceFeedBack(object sender, CommandEventArgs e)
        {
            if (e.Command.Command == UniversalCommandConstants.EnablePaymentCommand)
            {
                var cmd = e.Command as NsqEnablePaymentResponseCommand;
                if (cmd.TransactionId == Transaction.Id && (transaction.PaymentState == PaymentState.ActivatingPayment || transaction.PaymentState == PaymentState.Init || transaction.PaymentState == PaymentState.ReadyToPay))
                {

                    Transaction.PaymentState = cmd.Code == 0 ? PaymentState.ActivatedPaymentSuccess : PaymentState.ActivatedPaymentError;
                    if (Transaction.PaymentState == PaymentState.ActivatedPaymentSuccess)
                    {
                        // Send message when detected dish.
                        SendMessageToCamera(Transaction.Id.ToString(), true);

                        var msg = MessageConstants.PAYMENT_MOD_INVALID;
                        if (Enum.TryParse<PaymentTypes>(cmd.PaymentType, out PaymentTypes pType)){
                            switch (pType)
                            {
                                case PaymentTypes.IUC_CEPAS:
                                case PaymentTypes.IUC_CONTACTLESS:
                                case PaymentTypes.KONBI_CREDITS:
                                    msg = MessageConstants.PAYMENT_PLEASE_TAP_CARD;
                                    break;
                                case PaymentTypes.FACIAL_RECOGNITION:
                                    msg = MessageConstants.PAYMENT_PLEASE_SCAN_YOUR_FACE;
                                    break;
                                case PaymentTypes.QR_DASH:
                                    if (Transaction.CustomData == null)
                                        Transaction.CustomData = new ExpandoObject();
                                    Transaction.CustomData.QR = cmd.CustomData.QR;
                                    msg = MessageConstants.PAYMENT_PLEASE_SCAN_QR_CODE;
                                    break;
                            }
                        }
                        Transaction.CustomerMessage = msg;
                        _detailLogService.Log("PaymentManager_DeviceFeedBack: ActivatedPaymentSuccess");
                    }
                    else if (Transaction.PaymentState == PaymentState.ActivatedPaymentError)
                    {
                        //Alarm active payment failed : Critical level
                        Transaction.CustomerMessage = MessageConstants.ACTIVATE_PAYMENT_FAILED;
                        _detailLogService.Log("PaymentManager_DeviceFeedBack: ActivatedPaymentError");
                    }

                    NotifyOnTransactionChanged();
                }
            }

            if (e.Command.Command == UniversalCommandConstants.PaymentDeviceResponse)
            {
                var cmd = e.Command as NsqPaymentCallbackResponseCommand;
                if (cmd != null)
                {
                    switch (cmd.Response.State)
                    {
                        case PaymentState.InProgress:
                            {
                                _detailLogService.Log("PaymentState.InProgress");
                                Transaction.CustomerMessage = cmd.Response.Message;
                                Transaction.PaymentState = PaymentState.InProgress;
                                signalRCommunicator.ShowCustomerCountDown(DEFAULT_CUSTOMER_COUNTDOWN, false).Wait();

                                try
                                {
                                    if (cmd.Response.ResponseObject != null)
                                    {
                                        _detailLogService.Log("PaymentState.InProgress cmd.Response.ResponseObject != null");
                                        // commented out because the detecting card type is not in used any more.
                                        //if (cmd.Response.ResponseObject.ResponseCode == "CO" || cmd.Response.ResponseObject.ResponseCode == "PL") //Invalid Card type
                                        //{
                                        //    //Alarm invalid card type : Level Medium (Red solid & Sound instruction)
                                        //    alarmLightManager.Switch(false, true, false, false, 60 * 1000, "incorrect_card_detected_please_check_the_card_type.wav");
                                        //    _detailLogService.Log("PaymentManager_DeviceFeedBack: Payment InProgress Invalid Card Type");
                                        //}

                                        if (cmd.Response.ResponseObject.ResponseCode == "06") //PLEASE TAP CARD
                                        {
                                            _detailLogService.Log("PaymentState.InProgress cmd.Response.ResponseObject == 06");
                                            // Send message when detected dish.
                                            SendMessageToCamera(Transaction.Id.ToString(), true);
                                            Transaction.PaymentState = PaymentState.ActivatedPaymentSuccess;
                                            signalRCommunicator.ShowCustomerCountDown(DEFAULT_CUSTOMER_COUNTDOWN, false);

                                            Enum.TryParse(cmd.PaymentType, out PaymentTypes pType);

                                            if (pType == PaymentTypes.FACIAL_RECOGNITION)
                                            {
                                                alarmLightManager.Switch(true, false, false, true, 60 * 1000, "facial_detect.wav");
                                            }
                                            else
                                            {
                                                alarmLightManager.Switch(true, false, false, true, 60 * 1000, "please_scan_your_card.wav");
                                            }
                                            
                                            _detailLogService.Log("PaymentManager_DeviceFeedBack: Payment InProgress PLEASE TAP CARD");
                                        }

                                        else if (cmd.Response.ResponseObject.ResponseCode == "03") //REMOVE CARD
                                        {
                                            _detailLogService.Log("PaymentState.InProgress cmd.Response.ResponseObject == 03");
                                            //Alarm processing payment: Red/Green
                                            alarmLightManager.Switch(true, true, false, false, 60 * 1000, "processing_payment.wav");
                                            _detailLogService.Log("PaymentManager_DeviceFeedBack: Payment InProgress Processing");
                                        }
                                    }                                   

                                }
                                catch (Exception ex)
                                {
                                    _detailLogService.Log(ex.Message);
                                }
                            }
                            break;

                        case PaymentState.Success:
                            {
                                _detailLogService.Log("PaymentState.Success");
                                // Reset payment count error every success.
                                PaymentErrorRetries = 0;
                                nsqProducerService.SendRfidTableCommand(new UniversalCommands(UniversalCommandConstants.CotfResetCustomPrice));
                                isGeneratingTransaction = true;
                                Transaction.PaymentState = PaymentState.Success;
                                Transaction.CustomerMessage = MessageConstants.PAYMENT_PAID;
                                Transaction.Buyer = cmd.Response.ResponseObject.CardNumber;
                                //Send message to take end transaction picture
                                SendMessageToCamera(Transaction.Id.ToString());
                                signalRCommunicator.ShowCustomerCountDown(0, true);

                                _detailLogService.Log("PaymentManager_DeviceFeedBack: Payment Success");

                                // DO NOT STOP Transactions if camera app stops, sync empty data for image with default image.
                                // Save Transaction
                                var task = GenerateSaleTransactionAsync();
                                task.Wait();

                                //var anyScanningItems = Transaction.MenuItems.Any(p => p.UId == Guid.Empty);
                                //if(!anyScanningItems)
                                //{
                                //    CreateNewTransactionInfo(Guid.Empty);
                                //}
                            }
                            break;

                        case PaymentState.Failure:
                            {
                                PaymentErrorRetries++;
                                _detailLogService.Log("PaymentManager_DeviceFeedBack: Payment Failure");
                                alarmLightManager.Switch(true, true, true, true, 60 * 1000, "payment_failed_please_try_again_or_contact_staff.wav");

                                if (Transaction.PaymentType == PaymentTypes.KONBI_CREDITS)
                                {
                                    Transaction.KonbiCreditBalance = cmd.Response?.ResponseObject?.KonbiCreditBalance;
                                    Transaction.IsSufficientFund = cmd.Response?.ResponseObject?.IsSufficientFund;
                                    Transaction.IsTokenRetrieved = cmd.Response?.ResponseObject?.IsTokenRetrieved;
                                    Transaction.IsUserExist = cmd.Response?.ResponseObject?.IsUserExist;
                                }

                                if (Transaction.PaymentType == PaymentTypes.FACIAL_RECOGNITION)
                                {
                                    isGeneratingTransaction = true;
                                    Transaction.PaymentState = PaymentState.Failure;
                                    Transaction.CustomerMessage = cmd.Response.Message;
                                    // Send message to take end transaction picture
                                    SendMessageToCamera(Transaction.Id.ToString());
                                    // Alarm Failure state : red solid => back to scanning state
                                    signalRCommunicator.ShowCustomerCountDown(0, true);

                                    NotifyOnTransactionChanged();
                                    Task.Delay(3000).Wait();

                                    // Save Transaction
                                    // var task = GenerateSaleTransactionAsync();
                                    // task.Wait();

                                    PaymentErrorRetries = 0;
                                    paymentManager.DeactivatePaymentAsync(Transaction);

                                    ResetTransaction();
                                }

                                if (PaymentErrorRetries >= 3)
                                {
                                    _detailLogService.Log("PaymentState.Failure PaymentErrorRetries >= 3");
                                    isGeneratingTransaction = true;
                                    Transaction.PaymentState = PaymentState.Rejected;
                                    Transaction.CustomerMessage = cmd.Response.Message;

                                    // Send message to take end transaction picture
                                    SendMessageToCamera(Transaction.Id.ToString());
                                    // Alarm Failure state : red solid => back to scanning state
                                    signalRCommunicator.ShowCustomerCountDown(0, true);

                                    NotifyOnTransactionChanged();
                                    Task.Delay(3000).Wait();

                                    // Save Transaction
                                    // var task = GenerateSaleTransactionAsync();
                                    // task.Wait();

                                    PaymentErrorRetries = 0;

                                    Transaction.CustomerMessage = MessageConstants.PAYMENT_FAILED + ". Please pay by cash";
                                    paymentManager.DeactivatePaymentAsync(Transaction);

                                    ResetTransaction();
                                }

                                else if (PaymentErrorRetries < 3)
                                {
                                    _detailLogService.Log("PaymentState.Failure PaymentErrorRetries < 3");
                                    // Payment failed. Re-enable terminals to try again.
                                    Transaction.PaymentState = PaymentState.Failure;
                                    Transaction.CustomerMessage = MessageConstants.PAYMENT_FAILED + ". Please try again";
                                    NotifyOnTransactionChanged();
                                    Task.Delay(2000).Wait();

                                    if (Transaction.MenuItems.Any())
                                    {
                                        Transaction.PaymentState = PaymentState.ReadyToPay;
                                        Transaction.CustomerMessage = MessageConstants.PLEASE_PAY;
                                        signalRCommunicator.ShowCustomerCountDown(DEFAULT_CUSTOMER_COUNTDOWN, false);
                                    }
                                    else
                                    {
                                        ResetTransaction();
                                        //Transaction.PaymentState = PaymentState.Init;
                                        //Transaction.CustomerMessage = "";
                                    }

                                    NotifyOnTransactionChanged();
                                }
                            }
                            break;

                        case PaymentState.Cancelled:
                            {
                                _detailLogService.Log("PaymentState.Cancelled");
                                Transaction.PaymentState = PaymentState.Cancelled;

                                if (cmd.Response.Message.ToUpper() == MessageConstants.TIMED_OUT)
                                {
                                    // Alarm payment timeout: high level
                                    Transaction.CustomerMessage = MessageConstants.TRANSACTION_TIMED_OUT;
                                    alarmLightManager.Switch(true, true, false, true, 60 * 1000, "transaction_timed_out_please_try_again.wav");
                                    _detailLogService.Log("PaymentManager_DeviceFeedBack: Transaction TIMEOUT");
                                }
                                else
                                {
                                    Transaction.CustomerMessage = cmd.Response.Message;
                                    // Alarm payment cancel: red solid => back to scanning state
                                    alarmLightManager.Switch(false, true, false, false, 60 * 1000, "");
                                    _detailLogService.Log("PaymentManager_DeviceFeedBack: Payment Cancelled");
                                }
                                signalRCommunicator.ShowCustomerCountDown(0, true);
                            }
                            break;
                        case PaymentState.Rejected:
                            if (Transaction.PaymentType == PaymentTypes.FACIAL_RECOGNITION)
                            {
                                isGeneratingTransaction = true;
                                Transaction.PaymentState = PaymentState.Rejected;
                                Transaction.CustomerMessage = cmd.Response.Message;
                                // Send message to take end transaction picture
                                SendMessageToCamera(Transaction.Id.ToString());
                                // Alarm Failure state : red solid => back to scanning state
                                signalRCommunicator.ShowCustomerCountDown(0, true);

                                NotifyOnTransactionChanged();
                                Task.Delay(3000).Wait();

                                // Save Transaction
                                // var task = GenerateSaleTransactionAsync();
                                // task.Wait();

                                PaymentErrorRetries = 0;

                                Transaction.CustomerMessage = MessageConstants.FACE_NOT_RECOGNISED;
                                paymentManager.DeactivatePaymentAsync(Transaction);

                                ResetTransaction();
                            }
                            break;
                    }
                    NotifyOnTransactionChanged();
                }
            }
        }

        public void CheckPaymentTimeout()
        {
            isGeneratingTransaction = false;
            count = 0;
            //
            timer = new System.Timers.Timer();
            timer.Interval = 1000;
            timer.Elapsed += CheckPaymentTimeoutTick;
            timer.Start();
        }

        private void CheckPaymentTimeoutTick(object sender, ElapsedEventArgs e)
        {
            if (isGeneratingTransaction)
            {
                timer.Stop();
                count = 0;
            }
            else if (count >= 60)
            {
                //Alarm scanning state : red solid
                alarmLightManager.Switch(false, true, false, false, 5 * 1000, "");
                timer.Stop();
                count = 0;
            }
            else
            {
                count++;
            }
        }

        //public void SendMessageToAlarmLight(string _AlarmLightControl)
        //{
        //    var cmd = new NsqAlarmLightCommand()
        //    {
        //        AlarmLightControl = _AlarmLightControl
        //    };
        //    nsqProducerService.SendNsqCommand(NsqTopics.ALARM_LIGHT_REQUEST_TOPIC, cmd);
        //}

        private void SendMessageToCamera(string transactionId, bool isPaymentStart = false)
        {
            var cmd = new NsqCameraRequestCommand()
            {
                IsPaymentStart = isPaymentStart,
                TransactionId = transactionId
            };
            nsqProducerService.SendNsqCommand(NsqTopics.CAMERA_REQUEST_TOPIC, cmd);
        }

        public async Task<SessionInfo> GetSessionInfoAsync()
        {
            var cacheItem = await GetSaleSessionCacheItemAsync();
            return cacheItem.SessionInfo ?? null;

        }

        public bool GetPOSModeStatusAsync()
        {
            return IsPosModeOn;
        }

        public async Task<bool> OverrideSuccessPaymentAsync()
        {
            try
            {
                if (Transaction.Amount <= 0)
                    return false;
                // reset Discount
                if (Transaction.DiscountPercentage > 0)
                    Transaction.DiscountPercentage = 0;
                nsqProducerService.SendRfidTableCommand(new UniversalCommands(UniversalCommandConstants.CotfResetCustomPrice));
                // Send message when detected dish.
                SendMessageToCamera(Transaction.Id.ToString(), true);

                //deactive payment device
                //await paymentManager.DeactivatePaymentAsync(Transaction);    
                Transaction.PaymentType = PaymentTypes.OVERRIDE;
                Transaction.PaymentState = PaymentState.Success;
                Transaction.CustomerMessage = "Payment is successful.";
                NotifyOnTransactionChanged();
                await GenerateSaleTransactionAsync();              
                signalRCommunicator.ShowCustomerCountDown(0, true);//reset count down.
                _detailLogService.Log("OverrideSuccessPaymentAsync: Payment Override successful");
                SendMessageToCamera(Transaction.Id.ToString());
                return true;
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);               
                return false;
            }
        }

        CancellationTokenSource ts = new CancellationTokenSource();

        /// <summary>
        /// Click button Pay in screen 7Inch.
        /// </summary>
        /// <returns></returns>
        public async Task PayCash()
        {
            try
            {
                // reset Discount
                if (Transaction.DiscountPercentage > 0)
                    Transaction.DiscountPercentage = 0;
                nsqProducerService.SendRfidTableCommand(new UniversalCommands(UniversalCommandConstants.CotfResetCustomPrice));
                // Send message when detected dish.
                SendMessageToCamera(Transaction.Id.ToString(), true);

                //deactive payment device
                //await paymentManager.DeactivatePaymentAsync(Transaction);    
                Transaction.PaymentType = PaymentTypes.CASH;
                Transaction.PaymentState = PaymentState.Success;
                Transaction.CustomerMessage = "The cash payment is successful.";
                NotifyOnTransactionChanged();
                await GenerateSaleTransactionAsync();
                signalRCommunicator.NotifyCashPayment();
                signalRCommunicator.ShowCustomerCountDown(0, true);//reset count down.
                _detailLogService.Log("PaymentManager_DeviceFeedBack: Payment Cash Success");
                SendMessageToCamera(Transaction.Id.ToString());
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
                signalRCommunicator.NotifyCashPayment();
            }
        }

        public async Task<bool> ActivatePayment(string paymentMode)
        {
            if (Transaction == null)
            {
                return false;
            }
            if (Transaction.PaymentState != PaymentState.ReadyToPay && Transaction.PaymentState != PaymentState.Failure && Transaction.PaymentState != PaymentState.ActivatedPaymentError)
                return false;

            PaymentTypes p;
            var isAPaymentType = Enum.TryParse(paymentMode, true, out p);
            if (!isAPaymentType)
            {
                return false;
            }
            if (!paymentManager.AcceptedPaymentMethods.ContainsKey(p))
            {
                return false;
            }
            if(p == PaymentTypes.QR_DASH)
            {
                Transaction.DiscountPercentage = 5; // 5% discount
            }
            else
            {
                transaction.DiscountPercentage = 0;
            }
            // Activate Payment device
            Transaction.CustomerMessage = MessageConstants.PLEASE_WAIT;
            Transaction.PaymentType = p;
            Transaction.PaymentState = PaymentState.ActivatingPayment;
            NotifyOnTransactionChanged();
            var result = await paymentManager.ActivatePaymentAsync(Transaction);


            switch (result)
            {
                
                case CommandState.Received:
                    return true;

                case CommandState.TimeOut:
                    Transaction.PaymentState = PaymentState.ActivatedPaymentError;
                    Transaction.CustomerMessage = MessageConstants.ACTIVE_PAYMENT_TIME_OUT;

                    _detailLogService.Log("ProcessTransactionAsync: Activate payment device timeout");
                    break;

                default:
                    break;
            }

            NotifyOnTransactionChanged();

            return false;
        }

        public async Task CancelTransactionAsync()
        {
            var currentState = Transaction.PaymentState;
            Transaction.CustomerMessage = MessageConstants.CANCELLING_TRANSACTION;

            if (Transaction.PaymentState == PaymentState.ReadyToPay)
            {
                Transaction.PaymentState = PaymentState.Init;
            }

            NotifyOnTransactionChanged();

            if (currentState == PaymentState.ActivatedPaymentSuccess || currentState == PaymentState.InProgress)
            {
                var result = await paymentManager.DeactivatePaymentAsync(Transaction);
                _detailLogService.Log($"DeactivatePaymentAsync: {result}");

                if (result == CommandState.Failed || result == CommandState.Cancelled || result == CommandState.TimeOut)
                {
                    Transaction.CustomerMessage = MessageConstants.CAN_NOT_DEACTIVE_PAYMENT_DEVICE;
                    transaction.PaymentState = PaymentState.Failure;
                    NotifyOnTransactionChanged();
                    await Task.Delay(1000);
                }
            }

            CreateNewTransactionInfo(this.currentSesionId);
            //Alarm turn off light
            alarmLightManager.Off();
            _detailLogService.Log("CancelTransactionAsync: Cancel transaction");
            NotifyOnTransactionChanged();
        }

        public void RemoveOrderItem(Guid uId)
        {
            if (uId == null || uId == Guid.Empty) return;

            if (Transaction.PaymentState != PaymentState.ActivatedPaymentSuccess && Transaction.PaymentState != PaymentState.InProgress && Transaction.PaymentState != PaymentState.ActivatingPayment)
            {
                var item = Transaction.MenuItems.Where(x => x.UId == uId).FirstOrDefault();
                if (item != null)
                    Transaction.MenuItems.Remove(item);
                if (Transaction.MenuItems.Count() == 0)
                {
                    ResetTransaction();                 

                }
                NotifyOnTransactionChanged();
            }

        }

        private DateTime lastReading = DateTime.MinValue;

        private int readingStage = 0;
        public async Task<bool> IsStablizeReadingAsync(CancellationToken token)
        {
            lastReading = DateTime.Now;
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(10, token);
                    if (DateTime.Now.Subtract(lastReading).TotalMilliseconds > scanningStableAfterMiliseconds)
                        break;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
           
            return !token.IsCancellationRequested;
        }

        private string ValidateReading(IEnumerable<PlateReadingInput> plates, SaleSessionCacheItem cacheItem)
        {
            var tableAppService = IocManager.Instance.Resolve<ITableAppService>();
            return tableAppService.Validate(plates, cacheItem);
        }

        private async Task<SaleSessionCacheItem> GetSaleSessionCacheItemAsync()
        {
            try
            {
                var currentTime = Convert.ToInt32(string.Format("{0:00}{1:00}", DateTime.Now.Hour, DateTime.Now.Minute));
                var isCachedDataIsClear = false;

                var cacheItem = await cacheManager.GetCache(SaleSessionCacheItem.CacheName).Get(SaleSessionCacheItem.CacheName, async () =>
                {
                    var tableAppService = IocManager.Instance.Resolve<ITableAppService>();
                    isCachedDataIsClear = true;
                    return await tableAppService.GetSaleSessionInternalAsync();
                });

                if (cacheItem.SessionInfo == null || (cacheItem.SessionInfo != null && Convert.ToInt32(currentTime) > Convert.ToInt32(cacheItem.SessionInfo.ToHrs.Replace(":", ""))))
                {                    
                    cacheManager.GetCache(SaleSessionCacheItem.CacheName).Clear();
                }
                cacheItem = await cacheManager.GetCache(SaleSessionCacheItem.CacheName).Get(SaleSessionCacheItem.CacheName, async () =>
                {
                    var tableAppService = IocManager.Instance.Resolve<ITableAppService>();
                    isCachedDataIsClear = true;
                    return await tableAppService.GetSaleSessionInternalAsync();
                });

                if (isCachedDataIsClear)
                {
                    signalRCommunicator.UpdateSessionInfo(cacheItem.SessionInfo);
                }

                return cacheItem;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void ChangePosModeStatus()
        {
            IsPosModeOn = !IsPosModeOn;
            NotifyPosModeStatusChanged();
        }

        public void ChangeProductUpdateStatus(bool isUpdating)
        {
            IsProductUpdating = isUpdating;
        }

        public void NotifyPaymentState()
        {
            signalRCommunicator.UpdatePaymentSate(Transaction.PaymentState);
        }

        public void NotifyPosModeStatusChanged()
        {
            signalRCommunicator.UpdatePosModeStatus(IsPosModeOn);
        }

        public void NotifyOnTransactionChanged()
        {
            signalRCommunicator.UpdateTransactionInfo(Transaction);
        }

        public async Task GenerateSaleTransactionAsync()
        {
            // Store transaction to database.
            // Prevent generating transaction if Amount = 0 or PlatCount = 0.
            if (Transaction.Amount == 0 || Transaction.PlateCount == 0)
            {
                _detailLogService.Log($"GenerateSaleTransactionAsync. Warning: Transaction.Amount = {Transaction.Amount}, Transaction.PlateCount = {Transaction.PlateCount} ");
                return;
            }
                
            var tableAppService = IocManager.Instance.Resolve<ITableAppService>();
            _detailLogService.Log("Start Generating Sale Transaction.");

            // Add transaction to list transaction for print receipt.
            lstTransaction.Add(transaction);

            await tableAppService.GenerateTransactionAsync(Transaction);

            _detailLogService.Log("Generate Sale Transaction Async successfully.");

            // Processing print receipt.
            Session session = await _sessionRepository.FirstOrDefaultAsync(el => el.Id == transaction.SessionId);
            this._session = session;

            if (transaction.PaymentState != PaymentState.Failure)
            {
                // PrintReceipt(lstTransaction);
            }
        }

        private Font _printFont;
        private Font _printFontBold;
        private Font _printFontBoldSize;
        private Session _session;
        private TransactionInfo _transaction;
        private Guid currentSesionId;
        private CancellationTokenSource sendCameraCommandCtx;
        private object cmdCamreaLock = new object();

        /// <summary>
        /// Print receipt.
        /// </summary>
        public void PrintReceipt(List<TransactionInfo> lstTransaction)
        {
            // Get config on/off print in appsettings.json
            bool statusPrint = false;
            statusPrint = _settingManager.GetSettingValue<bool>(AppSettingNames.PrintIsEnabled);
            // Check turn on/off print.
            if (statusPrint)
            {
                // Check list transaction empty.
                bool isEmpty = !lstTransaction.Any();
                if (isEmpty)
                {
                    return;
                }

                foreach (var item in lstTransaction)
                {
                    _transaction = item;
                    _detailLogService.Log($"Printing receipt for transaction: {item}");
                    // Printing.
                    try
                    {
                        // Declare font.
                        _printFont = new Font("Arial", 10);
                        _printFontBold = new Font("Arial", 10, FontStyle.Bold);
                        _printFontBoldSize = new Font("Arial", 16, FontStyle.Bold);
                        // Declare print document.
                        PrintDocument pd = new PrintDocument();
                        pd.PrinterSettings.DefaultPageSettings.PaperSize = new System.Drawing.Printing.PaperSize("A6", 80, 100);
                        // Genarate content print document.
                        pd.PrintPage += new PrintPageEventHandler(pd_PrintPageReceipt);
                        // Print the document.
                        pd.Print();
                        // Remove item in list transaction.
                        //lstTransaction.Remove(item);
                    }
                    catch (Exception ex)
                    {
                        _detailLogService.Log(ex.Message);
                        return;
                    }
                }
            }
            else
            {
                _detailLogService.Log("Printer configuration is false.");
            }
        }

        /// <summary>
        /// The PrintPage event is raised for each page to be printed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ev"></param>
        private void pd_PrintPageReceipt(object sender, PrintPageEventArgs ev)
        {
            float leftMargin = ev.MarginBounds.Left;
            float topMargin = ev.MarginBounds.Top;

            // Iterate over the file, printing each line.
            var logo = new RectangleF(60, 0, 150, 50);
            ev.Graphics.DrawImage(Image.FromFile("Images/logo.png"), logo);

            ev.Graphics.DrawString(
                "Receipt Info",
                _printFontBoldSize,
                Brushes.Black,
                new RectangleF(0, 65, 302, 20), AlignmentRectangle(1));

            DrawLinesPoint(ev, 0, 85, 302);

            ev.Graphics.DrawString("Date:		  " + DateTime.Now.ToString("dd-MM-yyyy hh:mm"),
                _printFont,
                Brushes.Black,
                new RectangleF(0, 105, 302, 20),
                AlignmentRectangle(0));
            // TrungPQ check session null when using POS mode.
            string sessionName = _session == null ? "" : _session.Name;
            ev.Graphics.DrawString("Session: 	  " + sessionName,
                _printFont,
                Brushes.Black,
                new RectangleF(0, 125, 302, 20),
                AlignmentRectangle(0));
            ev.Graphics.DrawString("Payment: " + _transaction.PaymentType.ToString(),
                _printFont,
                Brushes.Black,
                new RectangleF(0, 145, 302, 20),
                AlignmentRectangle(0));
            ev.Graphics.DrawString("Machine: " + _settingManager.GetSettingValue(AppSettingNames.MachineName),
                _printFont,
                Brushes.Black,
                new RectangleF(0, 165, 302, 20),
                AlignmentRectangle(0));

            ev.Graphics.DrawString("Plate name",
                _printFontBold,
                Brushes.Black,
                new RectangleF(0, 185, 152, 20),
                AlignmentRectangle(0));
            ev.Graphics.DrawString("Qty",
                _printFontBold,
                Brushes.Black,
                new RectangleF(152, 185, 50, 20),
                AlignmentRectangle(0));
            ev.Graphics.DrawString("Sub Total",
                _printFontBold,
                Brushes.Black,
                new RectangleF(202, 185, 100, 20),
                AlignmentRectangle(0));

            DrawLinesPoint(ev, 0, 205, 302);

            int offset = 205;
            if (!IsPosModeOn)
            {
                var plates = _transaction.MenuItems.GroupBy(x => x.Code).ToList();

                for (var i = 0; i < plates.Count(); i++)
                {
                    var plate = plates[i];
                    offset = offset + 20;
                    ev.Graphics.DrawString(plate.First().Name, _printFont, Brushes.Black, new RectangleF(0, offset, 152, 20), AlignmentRectangle(0));
                    ev.Graphics.DrawString(plate.Count().ToString(), _printFont, Brushes.Black, new RectangleF(152, offset, 50, 20), AlignmentRectangle(2));
                    ev.Graphics.DrawString(
                        (plate.First().Price * plate.Count()).ToString("C", CultureInfo.CurrentCulture),
                        _printFont,
                        Brushes.Black,
                        new RectangleF(202, offset, 100, 20),
                        AlignmentRectangle(2));
                }
            }
            else
            {
                var plates = _transaction.MenuItems.ToList();

                for (var i = 0; i < plates.Count(); i++)
                {
                    var plate = plates[i];
                    offset = offset + 20;
                    ev.Graphics.DrawString(plate.Name, _printFont, Brushes.Black, new RectangleF(0, offset, 152, 20), AlignmentRectangle(0));
                    ev.Graphics.DrawString("1", _printFont, Brushes.Black, new RectangleF(152, offset, 50, 20), AlignmentRectangle(2));
                    ev.Graphics.DrawString(
                        (plate.Price * 1).ToString("C", CultureInfo.CurrentCulture),
                        _printFont,
                        Brushes.Black,
                        new RectangleF(202, offset, 100, 20),
                        AlignmentRectangle(2));
                }
            }

            DrawLinesPoint(ev, 0, offset + 20, 302);

            ev.Graphics.DrawString("Total " + _transaction.Amount.ToString("C", CultureInfo.CurrentCulture),
                _printFontBoldSize,
                Brushes.Black,
                new RectangleF(0, offset + 40, 302, 20),
                AlignmentRectangle(2));
        }

        /// <summary>
        /// Draw line with point.
        /// </summary>
        /// <param name="ev"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void DrawLinesPoint(PrintPageEventArgs ev, int x, int y, int z)
        {
            // Create pen.
            Pen pen = new Pen(Color.Black, 1);

            // Create coordinates of points that define line.
            int x1 = x;
            int y1 = y;
            int x2 = z;
            int y2 = y;

            //Draw lines to screen.
            ev.Graphics.DrawLine(pen, x1, y1, x2, y2);
        }

        /// <summary>
        /// Set StringFormat
        /// </summary>
        /// <param name="align"></param>
        /// <returns>StringFormat</returns>
        private StringFormat AlignmentRectangle(int align)
        {
            StringFormat stringFormat = new StringFormat();
            switch (align)
            {
                case 0:
                    stringFormat.Alignment = StringAlignment.Near;
                    break;
                case 1:
                    stringFormat.Alignment = StringAlignment.Center;
                    break;
                default:
                    stringFormat.Alignment = StringAlignment.Far;
                    break;
            }
            stringFormat.LineAlignment = StringAlignment.Center;

            return stringFormat;
        }

        public async Task GetTableDeviceSettingsAsync()
        {
            startWatchingOnProcessForServiceappearance();
            await tableSettingsManager.GetSettingsAsync();

        }

        private void startWatchingOnProcessForServiceappearance()
        {
            new Thread(new ThreadStart(watchOnThread)).Start();
        }

        private void watchOnThread()
        {
            while (!OnSale)
            {

                var tableProcess = Process.GetProcessesByName(tableSettingsManager.TableProcessName);
                if (tableProcess != null && tableProcess.Length > 0)
                {
                    tableSettingsManager.IsServiceRunning = true;
                }
                else
                {
                    tableSettingsManager.IsServiceRunning = false;
                }
                Thread.Sleep(500);

            }
        }

        public void HandleMessage(IMessage message)
        {
            var msg = Encoding.UTF8.GetString(message.Body);
            var obj = JsonConvert.DeserializeObject<NsqCameraResponseCommand>(msg);
            if ((obj.Command == UniversalCommandConstants.CameraResponse) && !obj.IsTimeout())
            {
                _detailLogService.Log($"Update transaction {obj.TransactionId} return from camera.");
                if (obj.TransactionId != null)
                {
                    var tableAppService = IocManager.Instance.Resolve<ITableAppService>();
                    tableAppService.UpdateTransactionImage(obj.TransactionId, obj.BeginImage, obj.EndImage);
                }
            }
            else if (obj.Command == UniversalCommandConstants.ACKResponse)
            {
                var ackCommand = JsonConvert.DeserializeObject<UniversalCommands>(msg);
                if (Command != null && Command.CommandId == ackCommand.CommandId)
                {
                    Command.CommandState = CommandState.Received;
                }
            }

            else if (obj.Command == UniversalCommandConstants.DeviceInfo)
            {
                var Command = JsonConvert.DeserializeObject<DeviceInfoCommand>(msg);
                UpdateCameraServiceStatus(Command.CommandObject);


            }
        }

        private void UpdateCameraServiceStatus(DeviceInfoCommandPayload payload)
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
                    _detailLogService.Log("ERROR: Tablemanager.UpdateCameraServiceStatus");

                    _detailLogService.Log(ex.Message);
                }

                finally
                {
                    unitOfWork.Complete();
                }

            }
        }

        public void LogFailedMessage(IMessage message)
        {

        }

        public void ResetTransaction()
        {
            var tax = GetTaxSettingsAsync().Result;
            if (Transaction == null)
            {
                Transaction = new TransactionInfo()
                {
                    Id = Guid.NewGuid(),
                    PaymentState = PaymentState.Init,
                    MenuItems = new List<MenuItemInfo>(),
                    PaymentType = paymentManager.CurrentPaymentMode,
                    SessionId = currentSesionId,
                    TaxPercentage = tax.Percentage,
                    TaxName = tax.Name,
                };
            }
            else
            {
                Transaction.Id = Guid.NewGuid();
                Transaction.PaymentState = PaymentState.Init;
                Transaction.MenuItems = new List<MenuItemInfo>();
                Transaction.PaymentType = paymentManager.CurrentPaymentMode;
                Transaction.SessionId = currentSesionId;
                transaction.TaxName = tax.Name;
                if (transaction.TaxPercentage != tax.Percentage)
                    transaction.TaxPercentage = tax.Percentage;
            }
            Transaction.DiscountPercentage = 0;
            Transaction.CustomerMessage = string.Empty;
            Transaction.BeginTranImage = string.Empty;
            Transaction.EndTranImage = string.Empty;
            Transaction.Buyer = BUYER_STAFF_NAME;
            //alarmLightManager.Off();
            //NotifyOnTransactionChanged();
        }
        public async Task<bool> PingAsync(string args = "")
        {
            var cmd = new PingCommand();
            cmd.CommandId = Guid.NewGuid();
            Command = cmd;
            var result = await SendCameraCommandAsync();
            return result == CommandState.Received;
        }

        private async Task<CommandState> SendCameraCommandAsync()
        {

            sendCameraCommandCtx?.Cancel();
            lock (cmdCamreaLock)
            {

                var result = nsqProducerService.SendNsqCommand(NsqTopics.CAMERA_REQUEST_TOPIC, Command);
                if (result)
                {
                    Command.CommandState = CommandState.SendSuccess;
                }
                else
                {
                    Command.CommandState = CommandState.Failed;

                }

            }
            sendCameraCommandCtx = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            try
            {

                while (Command.CommandState == CommandState.SendSuccess && !sendCameraCommandCtx.Token.IsCancellationRequested)
                {
                    await Task.Delay(50, sendCameraCommandCtx.Token);
                }
                if (sendCameraCommandCtx.Token.IsCancellationRequested)
                    Command.CommandState = CommandState.Cancelled;

            }
            catch (OperationCanceledException)
            {
                Command.CommandState = CommandState.TimeOut;
            }

            return Command.CommandState;
        }
        #region Generate Transaction on COTF
        /// <summary>
        /// Create New TransactionInfo.
        /// </summary>
        private void CreateNewTransactionInfo(Guid sessionId)
        {
            ResetTransaction();
            Transaction.SessionId = sessionId;
        }

        #region Generate Transaction from RFID Table
        /// <summary>
        /// Compare Old Transactions and New Transactions when scaning from RFID Table.
        /// </summary>
        /// <param name="plates"></param>
        /// <param name="cacheItem"></param>
        /// <returns></returns>
        public bool CompareTransactionsPlate(IEnumerable<PlateReadingInput> plates, SaleSessionCacheItem cacheItem)
        {
            var currentMenuItem = string.Join(",", Transaction.MenuItems.Where(x => x.UId == Guid.Empty).OrderBy(el => el.Code).Select(x => x.Code));
            var tmlPlates = plates.Where(el => (!cacheItem.PlateInfos.Any(x => x.Code == el.UType && x.Type != Enums.PlateType.Plate)));
            var newMenuItem = string.Join(",", tmlPlates.OrderBy(el => el.UType).Select(x => x.UType));
            _detailLogService.Log($"CompareTransactionsPlate: {newMenuItem} - {currentMenuItem}");
            if (currentMenuItem != newMenuItem)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check Transaction locked from RFID Table after Activated Payment Success.
        /// </summary>
        /// <param name="plates"></param>
        /// <param name="cacheItem"></param>
        /// <returns></returns>
        private bool CheckTransactionPlateLocked(IEnumerable<PlateReadingInput> plates, SaleSessionCacheItem cacheItem)
        {
            bool isLocked = false;

            if (Transaction != null && Transaction.MenuItems != null && Transaction.MenuItems.Count > 0 && (Transaction.PaymentState == PaymentState.ActivatedPaymentSuccess || Transaction.PaymentState == PaymentState.InProgress))
            {
                // Check change menu item on transaction.
                bool isChange = CompareTransactionsPlate(plates, cacheItem);

                if (isChange)
                {
                    // Alarm critical RED & GREEN BLINKING + Buzzer INTERRUPTED
                    alarmLightManager.Switch(true, true, true, true, 60 * 1000, "do_not_remove_your_tray.wav");
                    if (Transaction.CustomerMessage != MessageConstants.DO_NOT_REMOVE_TRAY)
                    {
                        lastCustomerMessage = Transaction.CustomerMessage;
                    }

                    Transaction.CustomerMessage = MessageConstants.DO_NOT_REMOVE_TRAY;
                    _detailLogService.Log("ProcessTransactionAsync: transaction locked, tray change detected");
                }
                else
                {
                    Transaction.CustomerMessage = lastCustomerMessage;
                    if(Transaction.PaymentState == PaymentState.ActivatedPaymentSuccess)
                    {
                        // Green blinking
                        alarmLightManager.Switch(true, false, false, true, 60 * 1000, "");
                    }
                    if (Transaction.PaymentState == PaymentState.InProgress)
                    {
                        // Red green solid
                        alarmLightManager.Switch(true, true, false, false, 60 * 1000, "");
                    }

                    _detailLogService.Log("ProcessTransactionAsync: transaction locked, tray stable");
                }

                NotifyOnTransactionChanged();
                isLocked = true;
            }

            return isLocked;
        }

        /// <summary>
        /// Set Value Transaction From RFID Table.
        /// </summary>
        /// <param name="plates"></param>
        /// <param name="cacheItem"></param>
        private void SetValueTransactionFromPlate(IEnumerable<PlateReadingInput> plates, SaleSessionCacheItem cacheItem)
        {
            if (cacheItem.MenuItems != null)
            {
                var isContractor = false;
                var contractorTrays = cacheItem.PlateInfos.Where(el => el.Type == Enums.PlateType.Tray);
                if (contractorTrays != null)
                {
                    isContractor = contractorTrays.Any(el => plates.Any(cond => cond.UType == el.Code));
                }
                Transaction.Buyer = isContractor ? BUYER_CONTRACTOR_NAME : BUYER_STAFF_NAME;

                // take out custom items from pos.
                var customItems = Transaction.MenuItems.Where(x => x.UId != Guid.Empty).ToList();
                Transaction.MenuItems.Clear();
                // Add item scanning.
                foreach (var inputPlate in plates)
                {
                    // Ignore contractor tray
                    if (contractorTrays != null && contractorTrays.Any(el => el.Code == inputPlate.UType))
                        continue;
                    _detailLogService.Log($"Input item {JsonConvert.SerializeObject(inputPlate)}");
                    var disc = cacheItem.PlateInfos.FirstOrDefault(x => x.Uid == inputPlate.UID);
                    //Get custom price
                    decimal customPrice = 0;
                    decimal.TryParse(inputPlate.UData, out customPrice);
                    if (customPrice > 0)
                    {
                        customPrice = Math.Round(customPrice / 100, 2);
                    }

                    //Add Transaction menu item
                    var menuItemInfo = new MenuItemInfo()
                    {
                        Code = disc?.Code,
                        Plate = new PlateInfo() { Code = disc?.Code, Uid = inputPlate.UID }
                    };

                    if (disc != null)
                    {
                        menuItemInfo.PlateId = disc.PlateId;
                    }

                    var itemInMenu = cacheItem.MenuItems.FirstOrDefault(x => x.Code == inputPlate.UType);

                    _detailLogService.Log($"Item in Menu {JsonConvert.SerializeObject(itemInMenu)}");

                    if (itemInMenu != null)
                    {
                        menuItemInfo.ProductId = itemInMenu.ProductId;
                        menuItemInfo.Desc = itemInMenu.Desc;
                        menuItemInfo.Color = itemInMenu.Color;
                        menuItemInfo.Name = itemInMenu.Name;
                        menuItemInfo.ImageUrl = itemInMenu.ImageUrl;

                    }
                    if (customPrice > 0)
                    {
                        menuItemInfo.Price = customPrice;
                        menuItemInfo.ProductName = "Custom Price";
                        menuItemInfo.Code = inputPlate.UType;
                    }
                    else
                    {
                        if (itemInMenu != null)
                        {
                            menuItemInfo.Price = isContractor ? itemInMenu.PriceContractor : itemInMenu.Price;
                            menuItemInfo.ProductName = itemInMenu.ProductName;
                        }
                    }
                    _detailLogService.Log($"Transaction MenuItem info: {JsonConvert.SerializeObject(menuItemInfo)}");
                    Transaction.MenuItems.Add(menuItemInfo);
                }
                // append custom items to exsiting transaction items.
                if (customItems != null && customItems.Count() > 0)
                {
                    transaction.MenuItems.AddRange(customItems);
                }
            }
        }

        /// <summary>
        /// Generate transaction due to  plates of reading from RFID Table.
        /// </summary>
        /// <param name="plates"></param>
        /// <returns></returns>
        public async Task<TransactionInfo> ProcessTransactionAsync(IEnumerable<PlateReadingInput> plates)
        {
            try
            {
                bool activedCountDown = false;
                // Get Cache SaleSessionCacheItem.
                var cacheItem = await GetSaleSessionCacheItemAsync();
                this.currentSesionId = cacheItem.SessionInfo != null ? cacheItem.SessionInfo.Id : Guid.Empty;
                // Check Transaction locked after Activated Payment Success.
                bool isLocked = CheckTransactionPlateLocked(plates, cacheItem);
                if (isLocked)
                {
                    return Transaction;
                }
                else
                {
                    activedCountDown = true;
                }
                //Create new transaction info.
                if (Transaction == null || (Transaction != null && !Transaction.MenuItems.Any())
                    || Transaction.PaymentState == PaymentState.Success
                    || Transaction.PaymentState == PaymentState.Rejected
                    || Transaction.PaymentState == PaymentState.Cancelled
                    || Transaction.PaymentState == PaymentState.Failure)
                {
                    activedCountDown = true;
                    CreateNewTransactionInfo(this.currentSesionId);
                }
                if (plates.Any())
                {
                    //Alarm scanning state: red solid A1
                    alarmLightManager.Switch(false, true, false, false, 60 * 1000, "please_place_tray_in_the_box.wav");
                    _detailLogService.Log("ProcessTransactionAsync: scanning state");
                }
                // Send message scanning.
                Transaction.CustomerMessage = MessageConstants.SCANNING;
                signalRCommunicator.ShowCustomerCountDown(0, true);
                if (Transaction.PaymentState == PaymentState.ReadyToPay)
                    Transaction.PaymentState = PaymentState.Init;
                NotifyOnTransactionChanged();
                ts.Cancel();
                ts = new CancellationTokenSource();
                var isStatble = await IsStablizeReadingAsync(ts.Token);
                if (!isStatble)
                {                    
                    return Transaction;
                }
                    
                Transaction.CustomerMessage = string.Empty;
                EnsureTransactionInfoHasEventHandlers();          

                // Check product assignments with plate.
                if (Transaction.MenuItems.Any(x => string.IsNullOrEmpty(x.ProductName)))
                {
                    // Remove unassigned product items.
                    Transaction.MenuItems = Transaction.MenuItems.Where(x => x.UId != Guid.Empty || !string.IsNullOrEmpty(x.ProductName)).ToList();
                    Transaction.CustomerMessage = MessageConstants.PRODUCT_NOT_ASSIGNED;
                }

                NotifyOnTransactionChanged();

                //Validate transaction.
                var errorMessage = ValidateReading(plates, cacheItem);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    Transaction.PaymentState = PaymentState.Rejected;
                    Transaction.CustomerMessage = errorMessage;
                    _detailLogService.Log("ProcessTransactionAsync: ValidateReading Error " + errorMessage);
                    return Transaction;
                }
                //Set value for transaction from RFID Table.
                SetValueTransactionFromPlate(plates, cacheItem);

                if (Transaction.Amount > 0)
                {
                    if (Transaction.PaymentState == PaymentState.Init)
                    {
                        //TrungPQ: Add new flow of Cotf.    
                        Transaction.PaymentState = PaymentState.ReadyToPay;
                        Transaction.CustomerMessage = MessageConstants.PLEASE_PAY;
                        NotifyOnTransactionChanged();
                        _detailLogService.Log("=====>Show message PLEASE_PAY");

                        Transaction.CustomerMessage = MessageConstants.SELECT_PAYMENT_METHOD;
                        NotifyOnTransactionChanged();
                        _detailLogService.Log("=====>Show message SELECT_PAYMENT_METHOD");

                        if (activedCountDown)
                        {
                            signalRCommunicator.ShowCustomerCountDown(DEFAULT_CUSTOMER_COUNTDOWN, false);
                        }
                    }

                }
                else
                {
                    if (Transaction.PaymentState == PaymentState.Init)
                    {
                        if (Transaction.MenuItems.Count > 0)
                        {
                            Transaction.CustomerMessage = MessageConstants.PRICE_NOT_DEFINE;
                            Transaction.PaymentState = PaymentState.Rejected;
                        }
                        else
                        {
                            Transaction.CustomerMessage = Transaction.CustomerMessage == MessageConstants.PRODUCT_NOT_ASSIGNED ? MessageConstants.PRODUCT_NOT_ASSIGNED : string.Empty;
                            if (!string.IsNullOrEmpty(Transaction.CustomerMessage))
                            {
                                Transaction.PaymentState = PaymentState.Rejected;
                            }
                        }
                    }
                    else if (Transaction.PaymentState == PaymentState.ReadyToPay)
                    {
                        ResetTransaction();
                    }

                }

                return Transaction;
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
                return new TransactionInfo();
            }
        }

        private void EnsureTransactionInfoHasEventHandlers()
        {
            if (Transaction == null) { 
                CreateNewTransactionInfo(currentSesionId);
              
            }
            Transaction.PropertyChanged -= Transaction_PropertyChanged;
            Transaction.PropertyChanged += Transaction_PropertyChanged;

        }

        private void Transaction_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.ToLower() == "paymentstate")
            {
                switch (Transaction.PaymentState)
                {
                    case PaymentState.Init:
                        alarmLightManager.Off();
                        break;

                    case PaymentState.ReadyToPay:
                        {
                            //Alarm scanning state: red solid
                            alarmLightManager.Switch(false, true, false, false, 60 * 1000, "");
                        }

                        break;

                    case PaymentState.ActivatingPayment:
                        break;

                    case PaymentState.ActivatedPaymentSuccess:
                        {
                            if(Transaction.PaymentType == PaymentTypes.IUC_CEPAS || 
                                Transaction.PaymentType == PaymentTypes.IUC_CONTACTLESS || 
                                Transaction.PaymentType == PaymentTypes.KONBI_CREDITS)
                            {
                                signalRCommunicator.ShowCustomerCountDown(DEFAULT_CUSTOMER_COUNTDOWN, false).Wait();

                                // Green blinking 60s
                                alarmLightManager.Switch(true, false, false, true, 60 * 1000, "please_scan_your_card.wav");
                                _detailLogService.Log("PaymentManager_DeviceFeedBack: Payment InProgress PLEASE TAP CARD");
                            }
                            else if(Transaction.PaymentType == PaymentTypes.FACIAL_RECOGNITION)
                            {
                                signalRCommunicator.ShowCustomerCountDown(DEFAULT_CUSTOMER_COUNTDOWN, false).Wait();

                                alarmLightManager.Switch(true, false, false, true, 60 * 1000, "facial_detect.wav");
                                _detailLogService.Log("PaymentManager_DeviceFeedBack: Payment InProgress PLEASE CHECK YOUR FACE");
                            }
                            else if(Transaction.PaymentType == PaymentTypes.QR_DASH)
                            {
                                signalRCommunicator.ShowCustomerCountDown(90, false).Wait();

                                // Green blinking 60s
                                alarmLightManager.Switch(true, false, false, true, 60 * 1000, "please_scan_the_qr.wav");
                                _detailLogService.Log("PaymentManager_DeviceFeedBack: Payment InProgress PLEASE SCAN QR");
                            }
                           
                        }
                        break;

                    case PaymentState.ActivatedPaymentError:
                        {
                            // Red green buzzer blinking
                            alarmLightManager.Switch(true, true, true, true, 60 * 1000, "");
                        }
                        break;

                    case PaymentState.Cancelled:
                        break;

                    case PaymentState.InProgress:
                        break;

                    case PaymentState.Success:
                        {
                            //Alarm paid state : green solid/audio thank you
                            alarmLightManager.Switch(true, false, false, false, 60 * 1000, "payment_successful_thank_you.wav");
                        }
                        break;

                    case PaymentState.Failure:
                        break;

                    case PaymentState.Rejected:
                        {
                            //Alarm state validate : Hight level (red + green + blink)
                            alarmLightManager.Switch(true, true, false, true, 60 * 1000, "");
                        }
                        break;
                }
                _detailLogService.Log("Transaction.State Changed: " + Transaction.PaymentState + " PaymentType: " + Transaction.PaymentType.ToString());

                //  going back to empty transaction after payment success, rejected or cannot activate payment device.
                cancelTokenResetTransactionState?.Cancel();
                cancelTokenResetTransactionState = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                ResetTransactionStateAsync(cancelTokenResetTransactionState.Token);


            }
        }
        /// <summary>
        /// Reset transaction to empty transaction if last state is Cancelled, Rejected, Activate Payment Failed or Success .
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task ResetTransactionStateAsync(CancellationToken token)
        {
            if (Transaction.PaymentState != PaymentState.ActivatedPaymentError
                && Transaction.PaymentState != PaymentState.Cancelled
                && Transaction.PaymentState != PaymentState.Rejected
                && Transaction.PaymentState != PaymentState.Success)
            {
                return;
                
            }
            try
            {
                await Task.Delay(3000, token); // reduce delayed time to reset into init transaction from 5s to 3s.
                if (Transaction.PaymentState == PaymentState.ActivatedPaymentError
                || Transaction.PaymentState == PaymentState.Cancelled
                || Transaction.PaymentState == PaymentState.Rejected
                || Transaction.PaymentState == PaymentState.Success)
                {
                    ResetTransaction();
                    NotifyOnTransactionChanged();
                }
            }
            catch (Exception ex)
            {
                //Ignore log for this exception, it would be OperationCancelledException because of timed out.
            }

        }

        /// <summary>
        /// Check Transaction locked from POS after Activated Payment Success.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="cacheItem"></param>
        /// <returns></returns>
        private bool CheckTransactionPosLocked()
        {
            bool isLocked = false;

            if (Transaction != null && Transaction.MenuItems != null && Transaction.MenuItems.Count > 0 && (Transaction.PaymentState == PaymentState.ActivatedPaymentSuccess || Transaction.PaymentState == PaymentState.InProgress))
            {
                isLocked = true;
            }

            return isLocked;
        }

        /// <summary>
        /// Set Value Transaction From Pos.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="cacheItem"></param>
        private void SetValueTransactionFromPos(ManualPaymentInput input, SaleSessionCacheItem cacheItem)
        {
            Transaction.MenuItems.Add(new MenuItemInfo()
            {
                UId = Guid.NewGuid(),
                Code = null,
                Desc = "",
                Price = input.Price,
                Name = "",
                ProductName = string.IsNullOrEmpty(input.ProductName) ? "Custom Price" : input.ProductName,
                ProductId = (input.ProductId != null && input.ProductId != Guid.Empty) ? input.ProductId.Value : Guid.Empty,
                Plate = new PlateInfo(),
            });
        }

        private void SetValueTransactionFromBarcodeScanning(MenuItemInfo input)
        {
            Transaction.MenuItems.Add(new MenuItemInfo()
            {
                UId = Guid.NewGuid(),
                Code = null,
                Desc = input.Desc,
                Price = input.Price,
                PriceContractor = input.PriceContractor,
                Name = input.Name,
                ProductName = input.ProductName,
                ProductId = input.ProductId,
                Plate = new PlateInfo(),
            });
        }

        private async void ExecuteBarcodeScanningTransaction(string barcodeValue)
        {
            try
            {
                var cacheItem = await GetSaleSessionCacheItemAsync();
                var prodMenu = cacheItem.MenuItems.Where(m => string.Compare(barcodeValue, m.BarCode) == 0).FirstOrDefault();
                _detailLogService.Log($"Prod menu: {(prodMenu == null ? "null" : "has value")}, Barcode: {barcodeValue}");
                if (prodMenu == null)
                {
                    return;
                }

                EnsureTransactionInfoHasEventHandlers();
                bool activedCountDown = true;
                this.currentSesionId = cacheItem.SessionInfo != null ? cacheItem.SessionInfo.Id : Guid.Empty;
                bool isLocked = CheckTransactionPosLocked();
                if (isLocked)
                {
                    return ;
                }

                if (Transaction == null || (Transaction != null && !Transaction.MenuItems.Any())
                    || Transaction.PaymentState == PaymentState.Success
                    || Transaction.PaymentState == PaymentState.Rejected
                    || Transaction.PaymentState == PaymentState.Cancelled)
                {
                    CreateNewTransactionInfo(this.currentSesionId);
                }

                SetValueTransactionFromBarcodeScanning(prodMenu);
                NotifyOnTransactionChanged();

                if (Transaction.PaymentState == PaymentState.Init || Transaction.PaymentState == PaymentState.ReadyToPay)
                {
                    if (Transaction.Amount > 0)
                    {
                        Transaction.CustomerMessage = MessageConstants.PLEASE_PAY;
                        Transaction.PaymentState = PaymentState.ReadyToPay;
                        NotifyOnTransactionChanged();
                        _detailLogService.Log("=====>Show message PLEASE_PAY");

                        if (Transaction.MenuItems.Any(e => e.UId == null))
                        {
                            Transaction.CustomerMessage = MessageConstants.DO_NOT_REMOVE_TRAY;
                            NotifyOnTransactionChanged();
                            _detailLogService.Log("=====>Show message DO_NOT_REMOVE_TRAY");
                        }
                        if (activedCountDown)
                        {
                            signalRCommunicator.ShowCustomerCountDown(DEFAULT_CUSTOMER_COUNTDOWN, false);
                        }
                    }
                    else
                    {
                        Transaction.PaymentState = PaymentState.Init;
                        if (Transaction.MenuItems.Count > 0)
                        {
                            Transaction.CustomerMessage = MessageConstants.PRICE_NOT_DEFINE;
                        }
                        else
                        {
                            Transaction.CustomerMessage = string.Empty;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
            }
        }

        /// <summary>
        /// Generate transaction due to  plates of reading from POS.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<TransactionInfo> ExecuteManualTransactionAsync(ManualPaymentInput input)
        {
            try
            {
                EnsureTransactionInfoHasEventHandlers();
                bool activedCountDown = true;
                // Get Cache SaleSessionCacheItem.
                var cacheItem = await GetSaleSessionCacheItemAsync();
                this.currentSesionId = cacheItem.SessionInfo != null ? cacheItem.SessionInfo.Id : Guid.Empty;
                // Check Transaction locked after Activated Payment Success.
                bool isLocked = CheckTransactionPosLocked();
                if (isLocked)
                {
                    return Transaction;
                }

                //Create new transaction processing.
                if (Transaction == null || (Transaction != null && !Transaction.MenuItems.Any())
                    || Transaction.PaymentState == PaymentState.Success
                    || Transaction.PaymentState == PaymentState.Rejected
                    || Transaction.PaymentState == PaymentState.Cancelled)
                {
                    // activedCountDown = true;
                    CreateNewTransactionInfo(this.currentSesionId);
                }

                //Set value for transaction from POS.
                SetValueTransactionFromPos(input, cacheItem);
                NotifyOnTransactionChanged();

                if (Transaction.PaymentState == PaymentState.Init || Transaction.PaymentState == PaymentState.ReadyToPay)
                {
                    if (Transaction.Amount > 0)
                    {
                        //TrungPQ: Add new flow of Cotf.
                        Transaction.CustomerMessage = MessageConstants.PLEASE_PAY;
                        Transaction.PaymentState = PaymentState.ReadyToPay;
                        NotifyOnTransactionChanged();
                        _detailLogService.Log("=====>Show message PLEASE_PAY");

                        if (Transaction.MenuItems.Any(e => e.UId == null))
                        {
                            Transaction.CustomerMessage = MessageConstants.DO_NOT_REMOVE_TRAY;
                            NotifyOnTransactionChanged();
                            _detailLogService.Log("=====>Show message DO_NOT_REMOVE_TRAY");
                        }
                        if (activedCountDown)
                        {
                            signalRCommunicator.ShowCustomerCountDown(DEFAULT_CUSTOMER_COUNTDOWN, false);
                        }
                    }
                    else
                    {
                        Transaction.PaymentState = PaymentState.Init;
                        if (Transaction.MenuItems.Count > 0)
                        {
                            Transaction.CustomerMessage = MessageConstants.PRICE_NOT_DEFINE;
                        }
                        else
                        {
                            ResetTransaction();
                        }
                    }
                }

                return Transaction;
            }
            catch (Exception ex)
            {
                _detailLogService.Log(ex.Message);
                return new TransactionInfo();
            }
        }

        public ConcurrentDictionary<PaymentTypes, string> GetAcceptedPaymentMethods()
        {
            paymentManager.ReloadAcceptedPayments();
            return paymentManager.AcceptedPaymentMethods;
        }

        //TODO(quyen): Do not remove. This is backup solution and will be implemented in case the Barcode Reader Listener works not good.
        public async Task ExecuteBarcodeScanningLog(string barcodeValue)
        {
            _detailLogService.Log($"Barcode listened from POS: {barcodeValue}");
        }
        #endregion Generate Transaction from POS
        #endregion Generate Transaction on COTF
    }
    public class ClientInfo
    {
        public string ConnectionId { get; set; }
        public string Group { get; set; }
    }
}
