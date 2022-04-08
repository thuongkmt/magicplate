using Abp.Application.Services;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using KonbiCloud.CloudSync;
using KonbiCloud.Common;
using KonbiCloud.Configuration;
using KonbiCloud.Plate;
using KonbiCloud.ProductMenu;
using KonbiCloud.Products;
using KonbiCloud.Sessions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Abp.Runtime.Session;

namespace KonbiCloud.BackgroundJobs
{
    public class GetPlateMenuJob : PeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly ISaveFromCloudService _saveFromCloudService;     

        private bool isSyncRunning;

        public GetPlateMenuJob(AbpTimer timer,
                           ISaveFromCloudService saveFromCloudService,
                           IDetailLogService detailLog,
                           IRepository<Product, Guid> productRepository,
                           IRepository<Session, Guid> sessionRepository,
                           IRepository<ProductMenu.ProductMenu, Guid> plateMenuRepository,
                           IRepository<Plate.Plate, Guid> plateRepository,
                           IPlateMenuSyncService plateMenuSyncService) : base(timer)
        {
            Timer.Period = 60 * 60 * 1000; //1 hour
            Timer.RunOnStart = true;
            

            _saveFromCloudService = saveFromCloudService;
       

        }

        [UnitOfWork]
        protected async override void DoWork()
        {
            if (isSyncRunning) return;
            isSyncRunning = true;

            try
            {
                await _saveFromCloudService.SyncProductMenuData();
            }
            catch (Exception ex)
            {
                isSyncRunning = false;
                Logger.Error($"Sync Product Menu Job: {ex.Message}", ex);
            }
            finally
            {
                isSyncRunning = false;
            }
        }
    }
}
