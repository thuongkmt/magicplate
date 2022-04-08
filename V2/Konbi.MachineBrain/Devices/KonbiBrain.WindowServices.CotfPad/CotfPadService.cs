using KonbiBrain.Common.Messages;
using KonbiBrain.Common.Services;
using KonbiBrain.Interfaces;
using KonbiBrain.Messages;
using KonbiBrain.WindowServices.CotfPad.Hardware;
using Konbini.Common.Messages;
using Konbini.Messages.Commands;
using Konbini.Messages.Commands.RFIDTable;
using Newtonsoft.Json;
using NsqSharp;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.WindowServices.CotfPad
{
    public partial class CotfPadService : ServiceBase, INsqHandler
    {
        private HardwareUtil util;
        private readonly string SERVICE_NAME = "Tag Reader V2";
        private readonly string SERVICE_TYPE = "TagReader.Service";

        private IMessageProducerService nsqMessageProducerService { get; set; }
        private NsqMessageConsumerService consumer { get; set; }
        public CotfPadService()
        {
            InitializeComponent();
            consumer = new NsqMessageConsumerService(topic: NsqTopics.RFID_TABLE_REQUEST_TOPIC, handler: this);
            nsqMessageProducerService = new NsqMessageProducerService();
        }

        public void Start()
        {
            OnStart(null);
        }
        protected override void OnStart(string[] args)
        {

            PublishDeviceInfo(false);
            //initialize hardware
            util = new HardwareUtil();
            util.Connect();
            util.TagDetected = TagDetected;
            util.TagsDetected = TagsDetected;
        }

        protected override void OnStop()
        {
            util.Disconnect();
        }

        private void TagsDetected(List<TagDto> tags)
        {
            var command = new DetectPlatesCommand();
            command.CommandObject.Plates = tags.Select(x => new PlateInfo
            {
                UID = x.TagId,
                UType = x.Model,
                UData = x.CustomData
            });

            nsqMessageProducerService.SendRfidTableResponseCommand(command);
        }


        private void TagDetected(TagDto tag)
        {
            Console.WriteLine($"Detected tag {tag.TagId} model {tag.Model} price {tag.CustomData}");
            Log.Information($"Detected tag {tag.TagId} model {tag.Model} price {tag.CustomData}");
        }

        public void HandleMessage(IMessage message)
        {
            try
            {
                var msg = Encoding.UTF8.GetString(message.Body);
                var obj = JsonConvert.DeserializeObject<UniversalCommands>(msg);
                Log.Information($"NSQ Request: {msg}");
                //CompleteReceivedAsync(obj).Wait();

                if (obj.Command == UniversalCommandConstants.CotfResetCustomPrice)
                {
                    CompleteReceivedAsync(obj).Wait();
                    Log.Information($"Received NSQ Reset Price message: {msg}");

                    //Reset custom price
                    util.WriteTagsData();
                }

                else if( obj.Command == UniversalCommandConstants.Ping)
                {
                    if (util.IsConnected)
                    {
                        PublishDeviceInfo(false);
                    }
                    else
                    {
                        PublishDeviceInfo(true, "Cannot connect to tag reader device");
                    }
                    CompleteReceivedAsync(obj).Wait();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot connect to RFID please check your RFID reader cable...");
                Log.Error(ex.ToString(), "Cannot connect to RFID please check cable connection");
            }      
        }

        public void LogFailedMessage(IMessage message)
        {
            
        }

        public void PublishDeviceInfo(bool hasError, string errorMessage= "")
        {
            //publish service informative description
            var deviceInfoCmd = new DeviceInfoCommand();
            deviceInfoCmd.CommandObject.Name = this.SERVICE_NAME;
            deviceInfoCmd.CommandObject.Type = this.SERVICE_TYPE;
            deviceInfoCmd.CommandObject.HasError = hasError;
            if (!string.IsNullOrEmpty(errorMessage))
                deviceInfoCmd.CommandObject.Errors.Add(errorMessage);
            nsqMessageProducerService.SendRfidTableResponseCommand(deviceInfoCmd);
        }

        private async Task<bool> CompleteReceivedAsync(IUniversalCommands obj)
        {
            return await Task.Run(() =>
            {
                var command = new UniversalACKResponseCommand(obj.CommandId);
                Debug.WriteLine(JsonConvert.SerializeObject(command));
                return nsqMessageProducerService.SendRfidTableResponseCommand(command);
            });
        }
    }
}
