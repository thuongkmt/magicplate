using KonbiBrain.Messages;
using NsqSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.Common.Services
{

    public class NsqMessageConsumerService
    {
        public LogService LogService { get; set; }
        private readonly Consumer consumer;
        public NsqMessageConsumerService(string topic, INsqHandler handler)
        {
            consumer = new Consumer(topic, NsqConstants.NsqDefaultChannel);
            consumer.AddHandler(handler);
            // use ConnectToNsqd instead of using ConnectToNsqLookupd because  we use standalone nsq service not cluster one.
            consumer.ConnectToNsqd(NsqConstants.NsqUrlConsumer);
            //consumer.ConnectToNsqLookupd(NsqConstants.NsqUrlConsumer);

        }
        public NsqMessageConsumerService(string topic, INsqHandler handler, string ClientId)
        {
            try
            {
                consumer = new Consumer(topic, NsqConstants.NsqDefaultChannel, new Config() { ClientID = ClientId });
                consumer.AddHandler(handler);
                // use ConnectToNsqd instead of using ConnectToNsqLookupd because  we use standalone nsq service not cluster one.
                consumer.ConnectToNsqd(NsqConstants.NsqUrlConsumer);
                //consumer.ConnectToNsqLookupd(NsqConstants.NsqUrlConsumer);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }

    }
    public interface INsqHandler : IHandler { }
}
