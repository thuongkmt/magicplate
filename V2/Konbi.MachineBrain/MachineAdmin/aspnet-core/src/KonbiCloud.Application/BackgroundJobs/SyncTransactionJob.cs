using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using KonbiCloud.CloudSync;
using KonbiCloud.Common;
using KonbiCloud.Configuration;
using KonbiCloud.Transactions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace KonbiCloud.BackgroundJobs
{
    public class SyncTransactionJob : PeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly IRepository<DetailTransaction, long> _transactionRepository;
        private readonly ITransactionSyncService _transactionSyncService;
        private readonly ITransactionAppService _transactionService;
        private readonly IConfigurationRoot _appConfiguration;
        private bool isRunning;
        private readonly IDetailLogService detailLogService;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public SyncTransactionJob(AbpTimer timer,
                                  IRepository<DetailTransaction, long> transactionRepository,
                                  ITransactionSyncService transactionSyncService,
                                  ITransactionAppService transactionService,
                                  IHostingEnvironment env,
                                  IUnitOfWorkManager unitOfWorkManager,
                                  IDetailLogService detailLog) : base(timer)
        {
            Timer.Period = 10 * 60 * 1000; //10 mins
            Timer.RunOnStart = true;
            //Timer.Period = 10000; //10 s
            _unitOfWorkManager = unitOfWorkManager;
            _transactionRepository = transactionRepository;
            _transactionSyncService = transactionSyncService;
            _transactionService = transactionService;
            this.detailLogService = detailLog;
            _appConfiguration = env.GetAppConfiguration();
        }

     
        protected async override void DoWork()
        {
            using (var unitOfWork = _unitOfWorkManager.Begin())
            {
                if (SettingManager == null)
                {
                    detailLogService.Log($"Sync Transaction: SettingManager is null");
                    return;
                }
                if (isRunning) return;
                isRunning = true;

                try
                {
                    var mId = Guid.Empty;
                    Guid.TryParse(SettingManager.GetSettingValue(AppSettingNames.MachineId), out mId);
                    if (mId == Guid.Empty)
                    {
                        detailLogService.Log($"Sync Transaction: Machine Id is null");
                        isRunning = false;
                        return;
                    }
                    var machineName = SettingManager.GetSettingValue(AppSettingNames.MachineName);

                    using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant, AbpDataFilters.MustHaveTenant))
                    {
                        var query = _transactionRepository.GetAllIncluding(el => el.CashDetail, el => el.CashlessDetail)
                            .Include(x => x.Session)
                            .Include(x => x.Products)
                            .Include("Products.Disc")
                            .Include("Products.Product")
                            .WhereIf(true, x => !x.IsSynced);//.OrderByDescending(el=> el.CreationTime).Take(100).ToListAsync();
                        var numberOfTxToBeSynced = await query.CountAsync();

                        if (numberOfTxToBeSynced > 0)
                        {
                            detailLogService.Log($"Start pushing {query.Count()} transactions to server");
                            var retry_times = 0;
                            do
                            {
                                // only send every 100 records at a time.

                                var result = await _transactionSyncService.PushTransactionsToServer(query.Take(100).ToList(), _appConfiguration[AppSettingNames.TransactionImageFolder]);
                                detailLogService.Log($"Sync transaction result: {result.success}");
                                if (result.result != null && result.result.Any())
                                {
                                    await _transactionService.UpdateSyncStatus(result.result);
                                    retry_times = 0;
                                }
                                else
                                {
                                    retry_times++;
                                }
                                numberOfTxToBeSynced = await query.CountAsync();
                            }
                            while (numberOfTxToBeSynced > 0 && retry_times < 10);
                        }

                        isRunning = false;
                    }
                }
                catch (Exception ex)
                {
                    isRunning = false;
                    Logger.Error($"Push Transactions result: {ex.Message}", ex);
                }
                finally
                {
                    isRunning = false;
                }
                unitOfWork.Complete();
            }
            

        }
    }
}
