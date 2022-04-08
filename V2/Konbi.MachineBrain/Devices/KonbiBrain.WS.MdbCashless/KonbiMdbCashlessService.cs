using KonbiBrain.Common.Services;
using KonbiBrain.Messages;
using MdbCashlessBrain;
using Microsoft.Extensions.Configuration;
using NsqSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KonbiBrain.WS.MdbCashless
{
    public partial class KonbiMdbCashlessService : ServiceBase
    {
        private MdbProcessingService mdbProcessingService;
        private LogService logSvc = new LogService();
        public KonbiMdbCashlessService()
        {
            InitializeComponent();
            logSvc.LogMdbInfo("Starting mdb...");
        }

        protected async override void OnStart(string[] args)
        {
            await Start();
        }

        public async Task Start()
        {
            try
            {
                var builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

                IConfigurationRoot configuration = builder.Build();

                //Console.WriteLine(configuration.GetConnectionString("Storage"));
               
                var machineRestSvc = new MachineAdminRestService(configuration);
                //var task = machineRestSvc.GetPort();
                //task.Wait();
                //var selectedPort = task.Result;
                var selectedPort = await machineRestSvc.GetPort();

                mdbProcessingService = new MdbProcessingService();

                var consumer = new Consumer(NsqTopics.PAYMENT_REQUEST_TOPIC, NsqConstants.NsqDefaultChannel);
                consumer.AddHandler(new PaymentRequestMessageHandler(mdbProcessingService));
                Console.WriteLine(Messages.NsqConstants.NsqUrlConsumer);
                // use ConnectToNsqd instead of using ConnectToNsqLookupd because  we use standalone nsq service not cluster one.
                consumer.ConnectToNsqd(NsqConstants.NsqUrlConsumer);
                //consumer.ConnectToNsqLookupd(NsqConstants.NsqUrlConsumer);
                mdbProcessingService.NsqMessageProducerService = new NsqMessageProducerService();

                mdbProcessingService.LogService = logSvc;
                mdbProcessingService.SetPort(selectedPort);
                mdbProcessingService.StartBackgroundWorker();
                Thread.Sleep(300);
                Task.Factory.StartNew(() => mdbProcessingService.InitMdb());
                logSvc.LogMdbInfo($"MdbCashless starting at {selectedPort}");
            }
            catch (Exception ex)
            {

                logSvc.LogMdbError(ex);
            }
            
        }

        protected override void OnStop()
        {
            mdbProcessingService?.Stop();
        }                  
    }
}
