using KonbiBrain.Interfaces;
using KonbiBrain.WindowServices.RFIDTable.Helper;
using KonbiBrain.WindowServices.RFIDTable.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Konbini.Messages.Commands.RFIDTable;
using KonbiBrain.Common.Services;
using KonbiBrain.Messages;
using NsqSharp;
using KonbiBrain.Common.Messages;
using System.Text;
using Newtonsoft.Json;
using Konbini.Messages.Enums;
using Konbini.Common.Messages;
using System;
using System.IO.Ports;
using System.Diagnostics;
using Konbi.Common.Interfaces;

namespace KonbiBrain.WindowServices.RFIDTable.Services
{
    public class SerialPortHandler : INotifyPropertyChanged, INsqHandler
    {
        private int connectionId;
        private string lastSentCommand = "";
        private string plateUIDusedforCheckingHeartbeat;

        public int ConnectionId
        {
            get
            {
                return connectionId;
            }
            set
            {
                if (connectionId != value)
                {
                    connectionId = value;
                    NotifyOfPropertyChange("ConnectionId");
                }
            }
        }



        public string ComPort { get; set; }
        #region  services
        public IKonbiBrainLogService Logger { get; set; }
        public IMessageProducerService NsqMessageProducerService { get; set; }
        private NsqMessageConsumerService consumer { get; set; }
        #endregion
        public SerialPortHandler()
        {
            this.PropertyChanged += SerialPortHandler_PropertyChanged;
            consumer = new NsqMessageConsumerService(topic: NsqTopics.RFID_TABLE_REQUEST_TOPIC, handler: this);

        }
        private DateTime lastDisconnectedTime = DateTime.Now;
        private void SerialPortHandler_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ConnectionId")
            {
                if (ConnectionId > 0)
                {
                    var msg = $"Open {ComPort} successfully";
                    // Logger.LogWindowEvent(msg, System.Diagnostics.EventLogEntryType.Information);
                    // Logger.SendToSlackAlert(msg);
                    Logger.LogRfIdTableInfo(msg);

                    new Thread(new ThreadStart(ListenOnDevice)).Start();
                }
                else
                {
                    lastDisconnectedTime = DateTime.Now;
                    var msg = $"Connection is closed or Couldn't open {ComPort}. trying to connect in 10s";
                    Logger.LogRfIdTableInfo(msg);
                    new Thread(new ThreadStart(() =>
                    {
                        while (connectionId <= 0)
                        {
                            Thread.Sleep(50);
                            if (DateTime.Now.Subtract(lastDisconnectedTime).TotalSeconds > 10)
                            {
                                lastDisconnectedTime = DateTime.Now;
                                Open("");
                            }


                        }
                    })).Start();
                }
            }
        }

        object deviceLock = new object();
        private void ListenOnDevice()
        {
            lock (deviceLock)
            {
                while (ConnectionId > 0)
                {
                    List<DishData> dishes = uid.ReadData().OrderBy(x => x.UID).ToList();
                    //List<DishData> data = uid.ReadData().OrderBy(x => x.UID).ToList();

                    // logic to detect communication problem.
                    if (!string.IsNullOrEmpty(plateUIDusedforCheckingHeartbeat))
                    {
                        if (!dishes.Any(el => el.UID == plateUIDusedforCheckingHeartbeat))
                        {
                            Logger.LogRfIdTableInfo($"Couldn't detect Heartbeat TAG UID: {plateUIDusedforCheckingHeartbeat}. it could be the Tag UID is not set correctly or there is a connection problem");
                            Logger.LogRfIdTableInfo("Trying to initialize connection again ..");
                            Close();
                        }
                        else
                        {
                            dishes.Remove(dishes.First(el => el.UID == plateUIDusedforCheckingHeartbeat));
                        }

                    }

                    var sendingCommandSignature = string.Join("-", dishes.Select(el => el.UType.Trim() + el.UID.Trim()));
                    if (sendingCommandSignature != lastSentCommand)
                    {
                        Logger.LogRfIdTableInfo("-------------------------------------------------------------------");
                        dishes.ForEach(el => { Logger.LogRfIdTableInfo($"Plate: Type:{el.UType}    UID: {el.UID}     Data:  {el.UData}" ); });

                        lastSentCommand = sendingCommandSignature;
                        var command = new DetectPlatesCommand();
                        command.CommandObject.Plates = dishes.Select(el => new PlateInfo() { UID = el.UID.Trim(), UType = el.UType.Trim(), UData =el.UData });
                        NsqMessageProducerService.SendRfidTableResponseCommand(command);
                    }

                    // signalRUpdateDishesAsync(dishes).Wait();
                    Thread.Sleep(50);
                }
            }
        }

        public void HandleMessage(IMessage message)
        {
            var msg = Encoding.UTF8.GetString(message.Body);
            var obj = JsonConvert.DeserializeObject<UniversalCommands>(msg);
            Logger.LogRfIdTableInfo($"NSQ Request: {msg}");
            if (obj.Command == UniversalCommandConstants.RfidTableConfiguration)
            {
                var cmd = JsonConvert.DeserializeObject<ConfigCommand>(msg);
                CompleteReceivedAsync(obj).Wait();
                if (cmd.CommandObject.Action != null)
                    switch (cmd.CommandObject.Action.ToLower())
                    {
                        case "getsettings":
                            {
                                GetCurrentSettings();
                            }
                            break;
                        case "forcetoreadplates":
                            {
                                lastSentCommand = Guid.NewGuid().ToString();
                            }
                            break;
                    }
            }

        }



        public void LogFailedMessage(IMessage message)
        {
            //throw new System.NotImplementedException();
        }
        private void GetCurrentSettings()
        {
            Task.Run(async () =>
            {
                await Task.Delay(1000);
                var cmd = new ConfigCommand();
                cmd.CommandObject.Action = "settingsResult";
                cmd.CommandObject.ComPortAvaliable = SerialPort.GetPortNames().ToList();
                cmd.CommandObject.selectedComPort = ConfigurationManager.AppSettings.Get("table.comport");
                cmd.CommandObject.IsServiceRunning = true;
                Debug.WriteLine(JsonConvert.SerializeObject(cmd));
                NsqMessageProducerService.SendRfidTableResponseCommand(cmd);
            });
        }
        private async Task<bool> CompleteReceivedAsync(IUniversalCommands obj)
        {
            return await Task.Run(() =>
            {
                var command = new UniversalACKResponseCommand(obj.CommandId);
                Debug.WriteLine(JsonConvert.SerializeObject(command));
                return NsqMessageProducerService.SendRfidTableResponseCommand(command);
            });
        }

        private readonly UID uid = new UID();


        #region Serial interactions


        public int Open(string port)
        {
            uid.Port = ConfigurationManager.AppSettings["table.comport"];
            plateUIDusedforCheckingHeartbeat = ConfigurationManager.AppSettings["table.detect.UID"];

            ComPort = uid.Port;
            ConnectionId = uid.Open();
            return ConnectionId;

        }
        public void Close(bool isShuttingDown = false)
        {
            uid.Close();
            if (!isShuttingDown)
                ConnectionId = 0;
        }
        #endregion

        #region  property changed events 


        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void NotifyOfPropertyChange(string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }


        #endregion



    }
}
