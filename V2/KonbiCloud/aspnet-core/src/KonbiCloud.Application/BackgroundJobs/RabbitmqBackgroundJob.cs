using Abp.Configuration;
using Abp.Dependency;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Castle.Core.Logging;
using KonbiCloud.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.BackgroundJobs
{
    public class RabbitmqBackgroundJob : PeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly IConnectToRabbitMqService _connector;
        private readonly ILogger _logger;
        public RabbitmqBackgroundJob(AbpTimer timer, ILogger logger, IConnectToRabbitMqService connectToRabbitMqService) : base(timer)
        {
            Timer.Period = 5*60*1000; //check connection every 5 minutes!
            Timer.RunOnStart = true;
            _connector = connectToRabbitMqService;
            _logger = logger;
        }
        protected override void DoWork()
        {
            //ensure rabbitmq is connected.
            if (_connector.IsConnected == false)
            {
                _connector.Connect();
            }
        }
    }
}
