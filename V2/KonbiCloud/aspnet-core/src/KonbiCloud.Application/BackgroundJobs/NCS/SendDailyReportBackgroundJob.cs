using Abp.AspNetZeroCore.Net;
using Abp.Collections.Extensions;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Net.Mail;
using Abp.ObjectMapping;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using KonbiCloud.BackgroundJobs.NCS.Exporting;
using KonbiCloud.Configuration;
using KonbiCloud.Enums;
using KonbiCloud.Transactions;
using KonbiCloud.Transactions.Dtos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NCrontab;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.BackgroundJobs.NCS
{
    public class SendDailyReportBackgroundJob : PeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private CrontabSchedule _schedule = null;
        private DateTime _nextRun;
        public string Schedule = "58 23 * * *"; //Runs at 23:58 every day
        private string sendToEmailAddress;

        private readonly IConfigurationRoot _appConfiguration;
        private readonly IEmailSender _emailSender;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<DetailTransaction, long> _transactionRepository;
        private readonly ICsvExporter _csvExporter;

        public IObjectMapper ObjectMapper { get; set; }
        public SendDailyReportBackgroundJob(AbpTimer timer, IHostingEnvironment env, IEmailSender emailSender, ICsvExporter csvExporter,
            IUnitOfWorkManager unitOfWorkManager,
            IRepository<DetailTransaction, long> transactionRepository) : base(timer)
        {
            _emailSender = emailSender;
            _csvExporter = csvExporter;
            _unitOfWorkManager = unitOfWorkManager;
            _transactionRepository = transactionRepository;

            

            Timer.Period = 5 * 1000; 
            Timer.RunOnStart = true;
            _appConfiguration = env.GetAppConfiguration();
            Schedule = _appConfiguration["NCSReport:Time"];
            //for testing purpose
            //Schedule = "* * * * *";
            sendToEmailAddress = _appConfiguration["NCSReport:SendTo"];

            // setup schedule
            _schedule = CrontabSchedule.Parse(Schedule);
            _nextRun = _schedule.GetNextOccurrence(DateTime.Now);

        }
        protected override void DoWork()
        {
            var now = DateTime.Now;
            var nextrun = _schedule.GetNextOccurrence(now);
            if (now > _nextRun)
            {
                var task = DoWorkAsync();
                _nextRun = _schedule.GetNextOccurrence(DateTime.Now);
                task.Wait();
            }
        }
        private async Task DoWorkAsync()
        {
            //do something
            Logger.Info($"Schedule time: {Schedule}, Send To Email: {sendToEmailAddress}");
            try
            {
                
                using (var unitOfWork = _unitOfWorkManager.Begin())
                {
                    var transactions = await GetTransactionsAsync();
                    if (transactions != null)
                    {
                      
                      

                        var message = new MailMessage();
                        message.To.Add(sendToEmailAddress);
                        message.Subject = "Daily sales report";
                        message.Body = "this email is generated automatically and sent out regularly from magicplate server";

                        var fileDto= _csvExporter.ExportPOSSHToFile(transactions);
                        message.Attachments.Add(new Attachment(new MemoryStream(_csvExporter.GetFile(fileDto)), $"POSSH{DateTime.Now:yyMMdd}.csv", MimeTypeNames.TextCsv));
                     
                        //File.WriteAllBytes("c:\\POSSH.csv", _csvExporter.GetFile(fileDto));

                        fileDto = _csvExporter.ExportPOSSTToFile(transactions);
                        message.Attachments.Add(new Attachment(new MemoryStream(_csvExporter.GetFile(fileDto)), $"POSST{DateTime.Now:yyMMdd}.csv", MimeTypeNames.TextCsv));
                        //File.WriteAllBytes(@"c:\POSST.csv", _csvExporter.GetFile(fileDto));

                        fileDto = _csvExporter.ExportPOSSPToFile(transactions);
                        message.Attachments.Add(new Attachment(new MemoryStream(_csvExporter.GetFile(fileDto)), $"POSSP{DateTime.Now:yyMMdd}.csv", MimeTypeNames.TextCsv));
                        //File.WriteAllBytes(@"c:\POSSP.csv", _csvExporter.GetFile(fileDto));

                        fileDto = _csvExporter.ExportPOSSVToFile(transactions);
                        message.Attachments.Add(new Attachment(new MemoryStream(_csvExporter.GetFile(fileDto)), $"POSSV{DateTime.Now:yyMMdd}.csv", MimeTypeNames.TextCsv));
                        //File.WriteAllBytes(@"c:\POSSV.csv", _csvExporter.GetFile(fileDto));

                        await _emailSender.SendAsync(message);

                    }
                }

                 
            }
            catch (Exception ex)
            {

                Logger.Error(ex.Message, ex);
            }
           
         
        }

        private async Task<IList<TransactionDto>> GetTransactionsAsync()
        {
       
            try
            {
                // get only success txn for the day.                
                var transactions = _transactionRepository.GetAll().Where(el=> el.Status == TransactionStatus.Success && el.PaymentTime>= DateTime.Today);  

                transactions = transactions
                   .Include(x => x.Machine)
                   .Include(x => x.Session)
                   .Include("Products.Disc")
                   .Include("Products.Product")
                   .Include("Products.Product.Category")
                   .Include(x => x.CashlessDetail);

                foreach (var tran in transactions)
                {
                    tran.ProductCount = tran.Products == null ? 0 : tran.Products.Count;
                }

                var totalCount = await transactions.CountAsync();
                var tranLists = await  transactions.OrderByDescending(el => el.PaymentTime).ToListAsync();

              
                var list = new List<TransactionDto>();
                foreach (var x in tranLists)
                {
                    var newTran = new TransactionDto()
                    {
                        Id = x.Id,
                        TranCode = x.TranCode.ToString(),
                        Buyer = x.Buyer,
                        PaymentTime = x.PaymentTime,
                        PaymentType = x.PaymentType,
                        Amount = x.Amount,
                        TaxPercentage = x.TaxPercentage,
                        DiscountPercentage = x.DiscountPercentage,
                        PlatesQuantity = x.ProductCount,
                        States = x.Status.ToString(),
                        Machine = x.Machine == null ? null : x.Machine.Name,
                        Session = x.Session == null ? null : x.Session.Name,
                        TransactionId = x.Machine == null ? x.TransactionId : $"{ x.Machine.Name.Replace(" ", "")}-{x.TransactionId}",
                        BeginTranImage = x.BeginTranImage,
                        EndTranImage = x.EndTranImage,
                        CardLabel = x.CashlessDetail == null ? "" : x.CashlessDetail.CardLabel == null ? "" : x.CashlessDetail.CardLabel,
                        CardNumber = x.CashlessDetail == null ? "" : x.CashlessDetail.CardNumber == null ? "" : "XXXXXXXXXXXX" + x.CashlessDetail.CardNumber.Substring(x.CashlessDetail.CardNumber.Length - 4),
                        ApproveCode = x.CashlessDetail == null ? "" : x.CashlessDetail.ApproveCode == null ? "" : x.CashlessDetail.ApproveCode,                        
                        Products = ObjectMapper.Map<ICollection<ProductTransactionDto>>(x.Products)
                    };
                    if (x.CashlessDetail != null)
                        newTran.CashlessPaidAmount = x.CashlessDetail.Amount/100;
                    // use Card Label to denote payment mode.
                    if (x.PaymentType == PaymentType.Cash)
                    {
                        newTran.CardLabel = "Cash";
                    }
                    if (string.IsNullOrEmpty(newTran.CardLabel))
                    {
                        newTran.CardLabel = x.PaymentType.ToString();
                    }
                  
                    list.Add(newTran);
                }

                return list;
            }
            catch (Exception ex)
            {
                Logger.Error($"Get all Transactions {ex.Message}", ex);
                return null;
            }
        }
    }
}
