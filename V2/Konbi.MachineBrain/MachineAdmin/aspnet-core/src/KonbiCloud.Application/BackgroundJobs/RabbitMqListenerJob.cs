using System;
using Abp.Dependency;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using KonbiCloud.Common;
using Konbini.Messages;
using MessagePack;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using KonbiCloud.Configuration;
using System.Threading.Tasks;
using Abp.Configuration;
using KonbiCloud.CloudSync;
using KonbiCloud.Messaging;
using Konbini.Messages.Enums;
using Castle.Core.Logging;
using System.Collections.Generic;
using Abp.Runtime.Caching;
using KonbiCloud.RFIDTable.Cache;
using KonbiCloud.RFIDTable;
using Newtonsoft.Json;
using KonbiCloud.Services;
using KonbiCloud.Services.Dto;

namespace KonbiCloud.BackgroundJobs
{
    public class RabbitMqListenerJob : PeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private const string EXCHANGE_NAME = "IOT_EXCHANGE";
        //define constants for device routing_key
        private const string ROUTING_KEY_CLOUD_PREFIX = "c";        // c for cloud
        private const string ROUTING_KEY_DEVICE_MSG = "magicplate_notification";
        private EventingBasicConsumer _notificationConsumer;
        private readonly ILogger _logger;
        private readonly IConnectToRabbitMqService _connector;        
        private readonly ISaveFromCloudService _saveFromCloudService;
        private IModel _listeningChannel;
        private readonly string _machineId;
        private int _jobTicks = 0;
        private readonly IDetailLogService _detailLogService;
        private readonly ICacheManager _cacheManager;
        private readonly IServiceStatusAppService _serviceStatusAppService;
        private readonly IRfidTableSignalRMessageCommunicator _signalRCommunicator;

        public RabbitMqListenerJob(AbpTimer timer,  
            IConnectToRabbitMqService connectToRabbitMqService,
            ISettingManager settingManager, 
            ISaveFromCloudService saveFromCloudService,
            ICacheManager cacheManager,
            IServiceStatusAppService serviceStatusAppService,
            IRfidTableSignalRMessageCommunicator signalRCommunicator,
            IDetailLogService detailLogService) : base(timer)
        {
            Timer.Period = 60*1000; //check connection every 1 minutes!
            Timer.RunOnStart = true;

            _machineId = settingManager.GetSettingValue(AppSettingNames.MachineId);
            _connector = connectToRabbitMqService;
            _connector.OnBrokerConnectionStateChanged += _connector_OnBrokerConnectionStateChanged;
            _saveFromCloudService = saveFromCloudService;
            _detailLogService = detailLogService;
            _cacheManager = cacheManager;
            _serviceStatusAppService = serviceStatusAppService;
            _signalRCommunicator = signalRCommunicator;
        }

        private void _connector_OnBrokerConnectionStateChanged(ConnectionStateEventArgs args)
        {
            try
            {
                _detailLogService.Log($"RabbitMQ: START conssumer listen");
                if (args.IsConnected)
                {
                    _listeningChannel = args.Connector.EnsureListeningChannel();
                    if (_listeningChannel != null)
                    {
                        try
                        {
                            _listeningChannel.ExchangeDeclare(EXCHANGE_NAME, ExchangeType.Topic, true, false);

                            var queueName = $"Magicplate_machine_{_machineId}-notification-listener";
                            _detailLogService.Log($"RabbitMQ: queueName is {queueName}");
                            // set to auto delete message in queue after 60s 
                            var queueArgs = new Dictionary<string, object>();
                            queueArgs.Add("x-message-ttl", 60000);

                            _listeningChannel.QueueDeclare(queue: queueName,
                                durable: false,
                                exclusive: true,
                                autoDelete: true,
                                arguments: queueArgs);
                            ;
                            _listeningChannel.QueueBind(queueName, EXCHANGE_NAME, $"#.{ROUTING_KEY_DEVICE_MSG}");
                            _notificationConsumer = new EventingBasicConsumer(_listeningChannel);
                            _notificationConsumer.Received += (model, ea) =>
                            {
                                var body = ea.Body;
                                var message = MessagePackSerializer.Deserialize<KeyValueMessage>(body);

                                var json = MessagePackSerializer.ToJson(body);

                                _detailLogService.Log($"RabbitMQ: queued data Received - > {json}");

                                var successProceedTask = ProcessIncomingMessage(message);

                                successProceedTask.Wait();


                            };
                            _listeningChannel.BasicConsume(queue: queueName,
                                autoAck: true,
                                consumer: _notificationConsumer);
                        }
                        catch(Exception ex)
                        {
                            _detailLogService.Log($"RabbitMQ: END with error -> {ex.ToString()}");
                        }
                    }
                    _saveFromCloudService.PartiallySyncAllData();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("_connector_OnBrokerConnectionStateChanged",ex);
            }
                   
            
        }

        protected override void DoWork()
        {
            _jobTicks++;
            //ensure jobticks is not exceed.
            if(_jobTicks>= int.MaxValue - 100)
            {
                _jobTicks = 0;
            }
            //ensure rabbitmq is connected.
            if (_connector.IsConnected == false)
            {
                _connector.Connect();
            }

            // doing timers with job ticks.
            //TODO

            //nnthuong: call to check Session 
            CheckCurrentSession().Wait();

            //nnthuong: check service status
            _serviceStatusAppService.CheckServiceStatus().Wait();
        }
        ////////////////////////////////////////
        /// Description: update realtime Session
        /// Author: nnthuong
        /// Date: 2021/01/24
        /// /////////////////////////////////////
        private async Task CheckCurrentSession()
        {
            try
            {
                var currentTime = Convert.ToInt32(string.Format("{0}{1}", DateTime.Now.Hour, DateTime.Now.Minute));
                var isCachedDataIsClear = false;

                //initial or check data in order to cache
                var cacheItem = await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).Get(SaleSessionCacheItem.CacheName, async () =>
                {
                    var tableAppService = IocManager.Instance.Resolve<ITableAppService>();
                    isCachedDataIsClear = true;
                    return await tableAppService.GetSaleSessionInternalAsync();
                });
                _detailLogService.Log("RabbitMqListenerJob.CheckCurrentSession: current Session --> " + JsonConvert.SerializeObject(cacheItem.SessionInfo));
                //verify expired data
                bool expriredCachingData = (cacheItem.SessionInfo != null && Convert.ToInt32(currentTime) > Convert.ToInt32(cacheItem.SessionInfo.ToHrs.Replace(":", ""))) ? true : false;
                if (cacheItem.SessionInfo == null || expriredCachingData)
                {
                    _cacheManager.GetCache(SaleSessionCacheItem.CacheName).Clear();

                    cacheItem = await _cacheManager.GetCache(SaleSessionCacheItem.CacheName).Get(SaleSessionCacheItem.CacheName, async () =>
                    {
                        var tableAppService = IocManager.Instance.Resolve<ITableAppService>();
                        isCachedDataIsClear = true;
                        return await tableAppService.GetSaleSessionInternalAsync();
                    });

                    _detailLogService.Log("RabbitMqListenerJob.CheckCurrentSession: change to new Session --> " + JsonConvert.SerializeObject(cacheItem.SessionInfo));
                }

                //update UI through signalR
                if (isCachedDataIsClear)
                {
                    await _signalRCommunicator.UpdateSessionInfo(cacheItem.SessionInfo);
                }
            }
            catch (Exception ex)
            {
                _detailLogService.Log("RabbitMqListenerJob.CheckCurrentSession: gets exception error --> " + ex.ToString());
                throw ex;
            }
        }
        private async Task<bool> ProcessIncomingMessage(KeyValueMessage keyValueMessage)
        {
            try
            {
                var key = keyValueMessage.Key;
                switch (key)
                {
                    case MessageKeys.TestKey:
                        break;
                    case MessageKeys.Session:
                        await _saveFromCloudService.SyncSessionData();
                        break;
                    case MessageKeys.Tray:
                        await _saveFromCloudService.SyncPlateData();
                        break;
                    case MessageKeys.Plate:
                        await _saveFromCloudService.SyncPlateData();
                        break;
                    case MessageKeys.PlateCategory:
                        await _saveFromCloudService.SyncPlateCategoryData();
                        break;
                    case MessageKeys.MenuScheduler:
                        await _saveFromCloudService.SyncProductMenuData();
                        break;
                    case MessageKeys.Inventory:
                        await _saveFromCloudService.SyncDiscData();
                        break;
                    case MessageKeys.TakeAway:
                        //await _saveFromCloudService.SyncTakeAwayTrayData();
                        break;
                    case MessageKeys.ProductCategory:
                        await _saveFromCloudService.SyncProductCategoryData();
                        break;
                    case MessageKeys.Product:
                        await _saveFromCloudService.SyncProductData();
                        break;
                    case MessageKeys.Settings:
                        await _saveFromCloudService.SyncSettingsData();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                return true;
            }
            catch (Exception e)
            {
                Logger.Error("ProcessIncomingMessage",e);
                return false;
            }
            
        }

      
    }
}
