using System;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using Konbi.Common.Interfaces;
using NsqSharp;
using System.Threading.Tasks;
using System.Windows;
using KonbiBrain.Interfaces;
using KonbiBrain.Messages;
//using StompSharp;
//using StompSharp.Messages;
using System.Configuration;
using System.Text;
using System.Windows.Forms;
using KonbiBrain.Common.Messages.Payment;
using System.Threading;
using Konbini.Messages.Enums;
using Newtonsoft.Json;
using KonbiBrain.Common.Messages;
using KonbiBrain.Common.Services;

namespace RawInputBrain.ViewModels
{


    public class ShellViewModel : Conductor<object>, IShell, INsqHandler, IDisposable
    {
        //private StompClient client;
        #region Member Data
        private readonly string paymentType = PaymentTypes.Mdb_CASHLESS.ToString();
        private IDisposable webApiServer;
        private decimal price;

        private Consumer consumer;

        private ObservableCollection<string> notificationList;

        private string textInput = "";

        #endregion

        public ShellViewModel()
        {
            NotificationList = new ObservableCollection<string>();
            //client= new StompClient(ConfigurationManager.AppSettings["RabbitMqServerIp"], 61613);
            nsqConsumerService = new NsqMessageConsumerService(NsqTopics.PAYMENT_REQUEST_TOPIC, this);

        }

        #region Properties

        private readonly NsqMessageConsumerService nsqConsumerService;

        public IMessageProducerService NsqMessageProducerService { get; set; }

        public IKonbiBrainLogService KonbiBrainLogService { get; set; }

        public Window CurrentWindow { get; set; }
        public RawInputInterface RawInputDevice { get; set; }

        private string _rawInputSelectedDevice;

        private ObservableCollection<RawInputDevice> _device;

        private RawInputDevice _selectedDevice;

        public string RawInputSelectedDevice
        {
            get
            {
                return _rawInputSelectedDevice;
            }
            set
            {
                if (_rawInputSelectedDevice != value)
                {
                    _rawInputSelectedDevice = value;
                    NotifyOfPropertyChange(() => RawInputSelectedDevice);

                    var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    config.AppSettings.Settings["DeviceFriendlyName"].Value = SelectedDevice.FriendlyName;
                    config.Save(ConfigurationSaveMode.Modified);

                    ConfigurationManager.RefreshSection("appSettings");
                }
            }
        }

        public ObservableCollection<string> NotificationList
        {
            get
            {
                return notificationList;
            }
            set
            {
                notificationList = value;
            }
        }

        public ObservableCollection<RawInputDevice> Devices
        {
            get
            {
                return this._device;
            }
            set
            {
                if (Equals(value, this._device)) return;
                this._device = value;
                this.NotifyOfPropertyChange(() => this.Devices);
            }
        }

        public RawInputDevice SelectedDevice
        {
            get
            {
                return this._selectedDevice;
            }
            set
            {
                if (Equals(value, this._selectedDevice)) return;
                this._selectedDevice = value;
                this.NotifyOfPropertyChange(() => this.SelectedDevice);
            }
        }

        public void AppendNotification(string msg)
        {
            Execute.OnUIThread(
            () =>
            {
                if (notificationList.Count == 100) notificationList.RemoveAt(0);
                NotificationList.Add(msg);
            });
        }


        public void SendRabbitMq()
        {
           
        }
        #endregion



        protected override void OnActivate()
        {
            try
            {
                base.OnActivate();

                var location = System.Reflection.Assembly.GetExecutingAssembly().Location;

                consumer = new Consumer(NsqTopics.MDB_MESSAGE_TOPIC, NsqConstants.NsqDefaultChannel);
                consumer.AddHandler(new MessageHandler(this));
                Console.WriteLine(NsqConstants.NsqUrlConsumer);
                consumer.ConnectToNsqLookupd(NsqConstants.NsqUrlConsumer);
                this.NotificationList = new ObservableCollection<string>() { "Raw Input v1.10" };
                Task.Factory.StartNew(() => this.Start());
                RawInputDevice.OnKeyPress = OnKeyPress;
            }
            catch (Exception ex)
            {
                KonbiBrainLogService.LogException(ex);
            }
        }

        private string _number { get; set; }
        public void OnKeyPress(System.Windows.Forms.Keys key)
        {
            
            if (key == Keys.Enter)
            {
                try
                {
                    AppendNotification($"Number: {_number} | {DateTime.Now}");
                    SendMessage(_number).Wait();
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                }

                _number = "";
                textInput = string.Empty;
            }
            else
            {
                _number += key.ToString().Replace("D", string.Empty);
                textInput += key.ToString();
            }
        }

    

        protected override void OnDeactivate(bool close)
        {
            consumer?.Stop();
            webApiServer?.Dispose();
            base.OnDeactivate(close);
        }

        public void DisposeObjects()
        {
            consumer.Stop();
        }

        public void RegisterDevice()
        {

            RawInputDevice.RegisterDevice(SelectedDevice, this.CurrentWindow,
                () =>
                    {
                        AppendNotification($"Register device {this._selectedDevice.FriendlyName} success");
                    },
                () =>
                    {
                        AppendNotification($"Register device {this._selectedDevice.FriendlyName} failed");
                    });

            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["DeviceFriendlyName"].Value = SelectedDevice.FriendlyName;
            config.Save(ConfigurationSaveMode.Modified);

            ConfigurationManager.RefreshSection("appSettings");
        }

        public void Start()
        {

            try
            {
                Devices = new ObservableCollection<RawInputDevice>(RawInputDevice.GetDevicesList());
                var selectedDevice = ConfigurationManager.AppSettings["DeviceFriendlyName"];
                SelectedDevice = Devices.FirstOrDefault(x => x.FriendlyName == selectedDevice);
                if (SelectedDevice != null)
                {
                    RegisterDevice();
                }
                else
                {
                    AppendNotification($"Device {selectedDevice} not found.");
                }
            }
            catch (Exception e)
            {
                KonbiBrainLogService.LogException(e);
            }

        }

        private async Task CompleteReceivedAsync(NsqPaymentCommandBase obj)
        {
            await Task.Delay(10);
            var command = new NsqPaymentACKResponseCommand(obj.CommandId, obj.PaymentType);
            this.NsqMessageProducerService.SendPaymentResponseCommand(command);
        }

        public void LogFailedMessage(IMessage message)
        {

        }

        public void HandleMessage(IMessage message)
        {
            //if (message is string) AppendNotification(DateTime.Now + " " + (string)message);
            //else
            //{
                var msg = Encoding.UTF8.GetString(message.Body);
                var obj = JsonConvert.DeserializeObject<NsqPaymentCommandBase>(msg);
                if (obj == null)// || obj.PaymentType != paymentType)
                    return;

                // Complete Received 
                CompleteReceivedAsync(obj).Wait();
                if (obj.Command == UniversalCommandConstants.EnablePaymentCommand)
                {
                    var cmd = JsonConvert.DeserializeObject<NsqEnablePaymentCommand>(msg);
                    this.price = cmd.Amount * 100;
                    //Activate device 
                    SimulateEnablePaymentAsync(true, obj.TransactionId).Wait();

                }
            //}

        }

        private void SendResultToMain(string number)
        {
            throw new NotImplementedException();
            //var cmd = new CommonCommands.SubmitCardNumber { CardNumber = number };
            //NsqMessageProducerService.SendToCustomerUICommand(cmd);
        }

        public void Dispose()
        {
            //client?.Dispose();
        }


        private async  Task SendMessage(string message)
        {
            //var destination = client.GetDestination("/topic/demo", client.SubscriptionBehaviors.AutoAcknowledge);
            //IReceiptBehavior receiptBehavior =
            //    new ReceiptBehavior(destination.Destination, client.Transport.IncommingMessages);
            //receiptBehavior = NoReceiptBehavior.Default;
            //var bodyOutgoingMessage =
            //    (new BodyOutgoingMessage(Encoding.ASCII.GetBytes(message))).WithPersistence();
            //await destination.SendAsync(bodyOutgoingMessage, receiptBehavior);


            //demo for rfid table payment
            Thread.Sleep(1000);
            var cards = ConfigurationManager.AppSettings["CardIds"];
            var idList = cards.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (idList.Any(x => x.Equals(message)))
            {
                //SimulateEnablePaymentAsync(true, Guid.NewGuid()).Wait();
                var cmd = new NsqPaymentCallbackResponseCommand(paymentType)
                {
                    Response = new PaymentResponseData()
                    {
                        Message = "Payment success",
                        State = Konbini.Messages.Enums.PaymentState.Success,
                        ResponseObject = new
                        {
                            Tid = "60002643",
                            Mid = "600054000000085",
                            DateTime = "07/03 13:05:07",
                            Invoice = "000138",
                            Batch = "000003",
                            CardLabel = "NFC",
                            CardNumber = message,
                            Rrn = "0307130507",
                            ApproveCode = "000001",
                            Amount = price
                        },
                        OtherInfo = null
                    }
                };
                NsqMessageProducerService.SendPaymentResponseCommand(cmd);

            }
            else
            {
                var cmd = new NsqPaymentCallbackResponseCommand(paymentType)
                {
                    Response = new PaymentResponseData()
                    {
                        Message = "Payment Error",
                        State = Konbini.Messages.Enums.PaymentState.Failure,
                        ResponseObject = null,
                        OtherInfo = null
                    }
                };
                NsqMessageProducerService.SendPaymentResponseCommand(cmd);
            }

        }


        private async Task SimulateEnablePaymentAsync(bool success, Guid transactionId)
        {
            var command = new NsqEnablePaymentResponseCommand(paymentType);

            command.TransactionId = transactionId;
            command.Code = success ? 0 : 1;
            NsqMessageProducerService.SendPaymentResponseCommand(command);

            await Task.Delay(300);
            if (success)
            {
                var cmd = new NsqPaymentCallbackResponseCommand(paymentType)
                {
                    Response = new PaymentResponseData()
                    {
                        Message = "PLEASE TAP CARD",
                        State = Konbini.Messages.Enums.PaymentState.InProgress,
                        ResponseObject = null,
                        OtherInfo = null
                    }
                };
                NsqMessageProducerService.SendPaymentResponseCommand(cmd);
            }
        }

    }
}