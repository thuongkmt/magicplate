using System;
using System.Threading.Tasks;
using Abp.Configuration;
using Castle.Core.Logging;
using KonbiCloud.Common;
using KonbiCloud.Configuration;
using Konbini.Messages;
using Konbini.Messages.Enums;
using MessagePack;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace KonbiCloud.Messaging
{
    public class RabbitMqSendMessageToMachineService : ISendMessageToMachineService
    {
        private const string EXCHANGE_NAME = "IOT_EXCHANGE";
        //define constants for device routing_key
        private const string ROUTING_KEY_CLOUD_PREFIX = "c";        // c for cloud
        private const string ROUTING_KEY_DEVICE_MSG = "magicplate_notification";
        //private IConnection _connection;
        //private IModel _queuedChannel;
        //private IModel _noQueueChannel;
        private ILogger _logger;

        private readonly IConfigurationRoot _configurationRoot;
        private readonly IDetailLogService _detailLogService;
        
        private readonly IConnectToRabbitMqService _connectToRabbitMqService;

        public RabbitMqSendMessageToMachineService(IAppConfigurationAccessor configurationRoot,
            IDetailLogService detailLogService,ILogger logger,ISettingManager settingManager, IConnectToRabbitMqService connectToRabbitMqService)
        {
          
            _configurationRoot = configurationRoot.Configuration;
            _detailLogService = detailLogService;
            _logger = logger;
            _connectToRabbitMqService = connectToRabbitMqService;
            _connectToRabbitMqService.OnBrokerConnectionStateChanged += _connectToRabbitMqService_OnBrokerConnectionStateChanged;
            //start connection
            _connectToRabbitMqService.Connect();

        }

        private void _connectToRabbitMqService_OnBrokerConnectionStateChanged(ConnectionStateEventArgs args)
        {
            if (args.IsConnected)
            {
                var channel = args.Connector.EnsurePublishingChannel();
                if(channel!=null)
                    channel.ExchangeDeclare(EXCHANGE_NAME, ExchangeType.Topic, true, false);
            }
        }

        public bool SendNotification(KeyValueMessage message)
        {
            try
            {
                _detailLogService.Log($"RabbitMQ: START SendNotification, request -> {JsonConvert.SerializeObject(message)}");
                var channel = _connectToRabbitMqService.EnsurePublishingChannel(); if (channel == null)
                {
                    _detailLogService.Log($"RabbitMQ: SendNotification -> Failed because publishing channel is null");
                    _logger?.Warn($"RabbitMQ - SendNotification: Failed because publishing channel is null");
                    return false;
                }
                var bodyMsg = MessagePackSerializer.Serialize(message);
                _detailLogService.Log($"RabbitMQ: SendNotification, bodyMsg -> {bodyMsg}");
                var properties = channel.CreateBasicProperties();
                

                // NOTICE: waiting for publish confirm for each message is not a good ideal. this one can be improved in the future.
                channel.ConfirmSelect();
                    channel.BasicPublish(exchange: EXCHANGE_NAME,
                                                routingKey: $"{_connectToRabbitMqService.ClientId}.{ROUTING_KEY_CLOUD_PREFIX}.all.all.{ROUTING_KEY_DEVICE_MSG}",
                                                // routingKey: $"{device.Type.ToString()}.{ROUTING_KEY_DEVICE_PREFIX}.{device.Id}.{ROUTING_KEY_DEVICE_CMD}",
                                                basicProperties: properties,
                                                body: bodyMsg);
                    channel.WaitForConfirmsOrDie();
                _detailLogService.Log($"RabbitMQ: END SendNotification");
            }
            catch (OperationInterruptedException oex)
            {
                _detailLogService.Log($"RabbitMQ: END SendNotification with OperationInterruptedException -> {oex.ToString()}");
                _logger?.Error( oex.Message);
                return false;
            }
            catch (Exception ex)
            {
                _detailLogService.Log($"RabbitMQ: END SendNotification with error -> {ex.ToString()}");
                _logger?.Error( ex.Message);
                return false;
            }
            return true;


        }
        public async Task<bool> SendNotificationAsync(KeyValueMessage message)
        {
            return await Task.Run(() => {
                return SendNotification(message);
            });
        }
    }
}
