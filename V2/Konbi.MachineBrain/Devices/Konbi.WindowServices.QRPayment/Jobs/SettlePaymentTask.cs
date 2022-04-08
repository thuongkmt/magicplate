using Konbi.WindowServices.QRPayment.Configuration;
using Konbi.WindowServices.QRPayment.Services.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NCrontab;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Konbi.WindowServices.QRPayment.Jobs
{
    public class SettlePaymentTask : BackgroundService
    {
        private  LogService _logger { get; set; }
        private FomoService _fomoService { get; set; }
        private int count;
        private CrontabSchedule _schedule =null;
        private DateTime _nextRun;
        public string Schedule = "58 23 * * *"; //Runs at 23:50 every day
        public SettlePaymentTask(LogService logService, FomoService fomoService, IHostingEnvironment environment, IServiceProvider serviceProvider, IOptions<FomoConfiguration> options)
        {
            _logger = logService;
            _logger.Init(environment.ContentRootPath);
            var service = serviceProvider.GetService(typeof(IOptions<FomoConfiguration>));
            _fomoService = fomoService;
           _fomoService.Init(options.Value);
            _schedule = CrontabSchedule.Parse(Schedule);
            _nextRun = _schedule.GetNextOccurrence(DateTime.Now);
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
           
           
            do
            {
                var now = DateTime.Now;
                var nextrun = _schedule.GetNextOccurrence(now);
                if (now > _nextRun)
                {
                    await DoWorkAsync(stoppingToken);
                    _nextRun = _schedule.GetNextOccurrence(DateTime.Now);
                }
                await Task.Delay(5000, stoppingToken); //5 seconds delay
            }
            while (!stoppingToken.IsCancellationRequested);

        }
        private async Task DoWorkAsync(CancellationToken stoppingToken)
        {

            count++;
            _logger.LogInfo($"Begin BatchSubmit: {DateTime.Now}");
            var result = await _fomoService.BatchSubmitAsync();
            _logger.LogInfo($"End BatchSubmit: IsSuccess =  {result}");
          
        }
    }
}
