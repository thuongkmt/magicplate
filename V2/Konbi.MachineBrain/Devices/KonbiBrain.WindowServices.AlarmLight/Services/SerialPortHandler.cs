using Konbi.Common.Interfaces;
using KonbiBrain.Common.Messages;
using KonbiBrain.Common.Services;
using KonbiBrain.Messages;
using Konbini.Common.Messages;
using Konbini.Messages.Commands;
using Konbini.Messages.Commands.RFIDTable;
using Konbini.Messages.Enums;
using Konbini.Messages.Payment;
using Newtonsoft.Json;
using NsqSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KonbiBrain.WindowServices.AlarmLight.Services
{
    public class SerialPortHandler : INsqHandler
    {
        private NsqMessageConsumerService consumer { get; set; }
        private readonly NsqMessageProducerService _nsqProducerService;
        public IKonbiBrainLogService Logger { get; set; }
        private bool IsOffCurrentBlink = false;

        private CancellationTokenSource tokenSource;
        private SerialPort port;
        private AlarmState alarmState = new AlarmState();
        private string lastAlertSound;
        private DateTime lastAlertSoundReceived = DateTime.Now;
        protected string LastAlertSound
        {
            get
            {
                if (DateTime.Now.Subtract(lastAlertSoundReceived).TotalSeconds > 10)
                    lastAlertSound = string.Empty;
                return lastAlertSound;
            }
            set
            {
                if(value!= lastAlertSound)
                {
                    lastAlertSoundReceived = DateTime.Now;
                    lastAlertSound = value;
                }
            }
        }
        //public string ComPort;

        public SerialPortHandler()
        {
            consumer = new NsqMessageConsumerService(topic: NsqTopics.ALARM_LIGHT_REQUEST_TOPIC, handler: this);
            _nsqProducerService = new NsqMessageProducerService();
            int.TryParse(ConfigurationManager.AppSettings["blinktime.default"], out int blinkDuration);
            alarmState.BlinkDuration = blinkDuration;

            var comPort = ConfigurationManager.AppSettings["alarmlight.comport"];
            
            if (string.IsNullOrEmpty(comPort))
                comPort = "COM30";

            port = new SerialPort(comPort, 9600, Parity.None, 8, StopBits.One);
           
            port.DtrEnable = true;
            try
            {
                port.Open();
                Logger?.LogInfo("Open serial port success");
            }
            catch (Exception ex)
            {

                Logger?.LogInfo(ex.Message);
            }
            if (port.IsOpen)
            {
                PublishDeviceInfo(false);
            }
            else
            {
                PublishDeviceInfo(true,"Cannot connect to traffic light device");
            }
        }

        public void HandleMessage(IMessage message)
        {
            var msg = Encoding.UTF8.GetString(message.Body);
            var obj = JsonConvert.DeserializeObject<UniversalCommands>(msg);
            //Ignore expired messages.
            if (obj.IsTimeout())
                return;

            Logger.LogInfo($"Alarm Light NSQ Received: {msg}");
            if (obj.Command == UniversalCommandConstants.RfidTableAlarmLightControl)
            {
                var cmd = JsonConvert.DeserializeObject<AlarmLightCommand>(msg);
                ExecuteAlarm(cmd);               
            }
            else if (obj.Command == UniversalCommandConstants.Ping)
            {
                if (port.IsOpen)
                    PublishDeviceInfo(false);
                else
                {
                    PublishDeviceInfo(true, "Cannot connect to traffic light device");
                }
                Task.Delay(500).Wait();
                CompleteReceivedAsync(obj).Wait();
            }
        }
        private async Task<bool> CompleteReceivedAsync(IUniversalCommands obj)
        {
            return await Task.Run(() =>
            {
                var command = new UniversalACKResponseCommand(obj.CommandId);
              
                return _nsqProducerService.SendAlarmLightResponseCommand(command);
            });
        }
        private void ExecuteAlarm(AlarmLightCommand cmd)
        {
            alarmState.Off = cmd.Off;
            alarmState.Blink = cmd.Blink;
            alarmState.Beep = cmd.Beep;
            alarmState.Duration = cmd.Duration;
            alarmState.Green = cmd.Green;
            alarmState.Red = cmd.Red;
            alarmState.SoundIntruction = cmd.SoundIntruction;
          
            HandleAlarmAsync();
        }
      
        private async void HandleAlarmAsync()
        {
            tokenSource?.Cancel();
           
            tokenSource = new CancellationTokenSource();

            if (alarmState.SoundIntruction != null && alarmState.SoundIntruction != "")
            {
                PlaySoundIntruction(alarmState.SoundIntruction);
            }
           
            try
            {             
                await WriteAlarm();
             
                if (alarmState.Blink && alarmState.BlinkDuration > 0)
                {
                    while (alarmState.Duration > 0 && !tokenSource.IsCancellationRequested)
                    {
                        await Task.Delay(alarmState.BlinkDuration,tokenSource.Token);
                        await WriteAlarmData(AlarmLightControl.Off);
                        await Task.Delay(alarmState.BlinkDuration, tokenSource.Token);
                        alarmState.Duration -= alarmState.BlinkDuration * 2;
                        await WriteAlarm();
                    }
                    if (!tokenSource.IsCancellationRequested)
                    {
                        await WriteAlarmData(AlarmLightControl.Off);
                        
                    }
                }
                else
                {
                    await Task.Delay(alarmState.Duration,tokenSource.Token);
                    if (!tokenSource.IsCancellationRequested)
                    {
                        await WriteAlarmData(AlarmLightControl.Off);
                    }
                }
               
            }
            catch (TaskCanceledException)
            {
                if (alarmState.Blink && alarmState.BlinkDuration > 0)
                    await WriteAlarmData(AlarmLightControl.Off);
                Console.WriteLine("Cancelled previous comamnd to execute new command.");
              
            }
        }

        private async Task WriteAlarm()
        {
            if (alarmState.Off)
            {
                await WriteAlarmData(AlarmLightControl.Off);
                return;
            }

            if (alarmState.Red && alarmState.Green && alarmState.Beep) await WriteAlarmData(AlarmLightControl.Red_Green_Beep);
            else if (alarmState.Green && alarmState.Beep) await WriteAlarmData(AlarmLightControl.Green_Beep);
            else if (alarmState.Red && alarmState.Beep) await WriteAlarmData(AlarmLightControl.Red_Beep);
            else if (alarmState.Red && alarmState.Green) await WriteAlarmData(AlarmLightControl.Red_Green);
            else if (alarmState.Red) await WriteAlarmData(AlarmLightControl.Red);
            else if (alarmState.Green) await WriteAlarmData(AlarmLightControl.Green);
            else if (alarmState.Beep) await WriteAlarmData(AlarmLightControl.Beep);
        }

        public void LogFailedMessage(IMessage message)
        {
            // Method intentionally left empty.
        }

        public Task PlaySoundIntruction(string soundPath)
        {
            return Task.Run(() => {
                if (!string.IsNullOrEmpty(alarmState.SoundIntruction) && alarmState.SoundIntruction != this.LastAlertSound)
                {
                    LastAlertSound = alarmState.SoundIntruction;
                }
                else
                {
                    return;
                }
                try
                {
                    var basePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, @"Assets");
                    //If input sound file is not existing then skip. .. 

                    if (!File.Exists(Path.Combine(basePath, soundPath)))
                    {
                        Logger.LogInfo($"{Path.Combine(basePath, soundPath)} not found");
                        return;
                    }
                    SoundPlayer player = new SoundPlayer();

                    player.SoundLocation = Path.Combine(basePath, soundPath);
                    player.PlaySync();

                }
                catch (Exception e)
                {
                    Logger.LogInfo(e.Message);
                }
            });
            
          
        }
        private DateTime lastRetried = DateTime.Now.AddDays(-1);
        private readonly object writeSerialDataLock = new object();
        private readonly string SERVICE_NAME = "Traffic Light Controller";
        private readonly string SERVICE_TYPE = "LightAlarm.Service";

        public Task<bool> WriteAlarmData(string command)
        {
            return Task.Run(() => {
                lock (writeSerialDataLock)
                {
                    Logger.LogInfo($"WriteAlarmData: {command}");
                    if (!port.IsOpen)
                    {
                        port.PortName = ConfigurationManager.AppSettings["alarmlight.comport"];

                        var retry = 0;
                        while (!port.IsOpen && retry < 1 && lastRetried.AddSeconds(15) < DateTime.Now)
                        {
                            try
                            {
                                lastRetried = DateTime.Now;
                                retry++;
                                port.Open();
                            }
                            catch (Exception ex)
                            {

                                Logger.LogInfo($"Exception: {ex}");
                                Logger.LogInfo($"Retrying to connect to serial port {port.PortName}");
                            }
                        }
                    }

                    if (port.IsOpen)
                    {
                        byte[] newMsg = HexToByte(command);
                        try
                        {
                            port.Write(newMsg, 0, newMsg.Length);
                        }
                        catch (Exception ex)
                        {

                            Logger.LogInfo($"WriteAlarmData: Failed to write");
                            return false;
                        }
                      
                        Logger.LogInfo($"WriteAlarmData: Writen success");
                        return true;

                    }

                    Logger.LogInfo($"WriteAlarmData: Failed to write");
                    return false;
                }
            });
        }


        private byte[] HexToByte(string msg)
        {
            //remove any spaces from the string
            msg = msg.Replace(" ", "");
            //create a byte array the length of the
            //divided by 2 (Hex is 2 characters in length)
            byte[] comBuffer = new byte[msg.Length / 2];
            //loop through the length of the provided string
            for (int i = 0; i < msg.Length; i += 2)
                //convert each set of 2 characters to a byte
                //and add to the array
                comBuffer[i / 2] = (byte)Convert.ToByte(msg.Substring(i, 2), 16);
            //return the array
            return comBuffer;
        }
        public void PublishDeviceInfo(bool hasError, string errorMessage = "")
        {
            //publish service informative description
            var deviceInfoCmd = new DeviceInfoCommand();
            deviceInfoCmd.CommandObject.Name = this.SERVICE_NAME;
            deviceInfoCmd.CommandObject.Type = this.SERVICE_TYPE;
            deviceInfoCmd.CommandObject.HasError = hasError;
            if (!string.IsNullOrEmpty(errorMessage))
                deviceInfoCmd.CommandObject.Errors.Add(errorMessage);
            _nsqProducerService.SendAlarmLightResponseCommand(deviceInfoCmd);
        }

    }
}
