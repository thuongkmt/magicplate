using KonbiBrain.Common.Messages;
using KonbiBrain.Messages;
using KonbiCloud.Common;
using Konbini.Messages.Enums;
using Newtonsoft.Json;
using NsqSharp;
using NsqSharp.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.RFIDTable
{
    public class BarcodeScannerMessageHandler : IBarcodeScannerMessageHandler, IHandler
    {
        private readonly IMessageProducerService messageProducerService;
        private readonly IDetailLogService detailLogService;
        private readonly Consumer consumer;
        private IUniversalCommands Command { get; set; }
        public event EventHandler<CommandEventArgs> DeviceFeedBack;

        public BarcodeScannerMessageHandler(IMessageProducerService messageProducerService, IDetailLogService detailLogService)
        {
            this.messageProducerService = messageProducerService;
            this.detailLogService = detailLogService;

            consumer = new Consumer(NsqTopics.BARCODE_SCANNER_TOPIC, NsqConstants.NsqDefaultChannel, new Config() { ClientID = Guid.NewGuid().ToString() });
            consumer.AddHandler(this);
            consumer.ConnectToNsqd(NsqConstants.NsqUrlConsumer);
        }

        public Task<CommandState> GetServiceInfosAsync()
        {
            throw new NotImplementedException();
        }

        protected void OnDeviceFeedBack(CommandEventArgs e)
        {
            DeviceFeedBack?.Invoke(this, e);
        }

        public void HandleMessage(IMessage message)
        {
            try
            {
                var msg = Encoding.UTF8.GetString(message.Body);
                var obj = JsonConvert.DeserializeObject<UniversalCommands>(msg);
                if (obj.IsTimeout())
                    return;
                OnDeviceFeedBack(new CommandEventArgs() { CommandStr = msg, Command = JsonConvert.DeserializeObject<UniversalCommands<object>>(msg) });
            }
            catch
            {
                // dont catch unexpected message
            }
        }

        public void LogFailedMessage(IMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
