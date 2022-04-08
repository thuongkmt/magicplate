using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using KonbiCloud.Messaging;
using Konbini.Messages;
using Konbini.Messages.Enums;

namespace KonbiCloud.Common
{
    public class TestAppService: KonbiCloudAppServiceBase
    {
        private ISendMessageToMachineService _sendMessageToMachineService;

        public TestAppService(ISendMessageToMachineService sendMessageToMachineService)
        {
            _sendMessageToMachineService = sendMessageToMachineService;
        }

        public async Task<object> TestRabbitMq()
        {
            var obj = new KeyValueMessage()
            {
                Key = MessageKeys.TestKey,
                Value = "Hello"
            };
            await _sendMessageToMachineService.SendNotificationAsync(obj);
            return obj;
        }


        public async Task<object> TestRabbitMqWithKey(MessageKeys key)
        {
            var obj = new KeyValueMessage()
            {
                Key = key,
                Value = key.ToString()
            };
            await _sendMessageToMachineService.SendNotificationAsync(obj);
            return obj;
        }
    }
}
