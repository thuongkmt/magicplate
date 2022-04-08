using Abp.Domain.Repositories;
using KonbiCloud.Common;
using KonbiCloud.GetStarted.Dtos;
using KonbiCloud.Machines;
using KonbiCloud.Plate;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KonbiCloud.Dashboard
{
    public class GetStartedAppService : KonbiCloudAppServiceBase, IGetStartedAppService
    {
        private readonly IRepository<PlateCategory> _plateCategoryRepository;
        private readonly IRepository<Session, Guid> _sessionRepository;
        private readonly IRepository<Plate.Plate, Guid> _plateRepository;
        private readonly IRepository<MenuSchedule.ProductMenu, Guid> _plateMenuRepository;

        public GetStartedAppService(
            IRepository<PlateCategory> plateCategoryRepository,
            IRepository<Session, Guid> sessionRepository,
            IRepository<Plate.Plate, Guid> plateRepository,
            IRepository<MenuSchedule.ProductMenu, Guid> plateMenuRepository)
        {
            _plateCategoryRepository = plateCategoryRepository;
            _sessionRepository = sessionRepository;
            _plateRepository = plateRepository;
            _plateMenuRepository = plateMenuRepository;
        }

        public async Task<List<GetStartedDataOutput>> getGetStartedStatus()
        {
            var listResult = new List<GetStartedDataOutput>();

            var plateCategories = _plateCategoryRepository.GetAll();
            var totalPlateCategories = await plateCategories.CountAsync();
            listResult.Add(new GetStartedDataOutput() { StepId = 1, StepName = "PlateCategory", StepTitle = "Add Plate Category", StepSubTitle = "Click Create to navigate Category manager screen", StepActionUrl = "/app/main/plate/plateCategories", StepDoneFlg = totalPlateCategories });

            var session = _sessionRepository.GetAll();
            var totalSession = await session.CountAsync();
            listResult.Add(new GetStartedDataOutput() { StepId = 2, StepName = "Session", StepTitle = "Add Session", StepSubTitle = "Click Create to navigate Session manager screen", StepActionUrl = "/app/main/machines/sessions", StepDoneFlg = totalSession });

            var plateModel = _plateRepository.GetAll();
            var totalPlateModel = await plateModel.CountAsync();
            listResult.Add(new GetStartedDataOutput() { StepId = 3, StepName = "PlateModel", StepTitle = "Import Plate Models", StepSubTitle = "Bulk set Plate Model, Name, Category, Image to manage Plates", StepActionUrl = "/app/main/plate/plates", StepDoneFlg = totalPlateModel });

            var plateMenu = _plateMenuRepository.GetAll();
            var totalPlateMenu = await plateMenu.CountAsync();
            listResult.Add(new GetStartedDataOutput() { StepId = 4, StepName = "PlateMenu", StepTitle = "Import Plate Menu", StepSubTitle = "Bulk set Plate Prices", StepActionUrl = "/app/main/plate/plateMenus", StepDoneFlg = totalPlateMenu });

            var step6Count = 0;
            if(totalPlateCategories > 0 && totalSession > 0 && totalPlateModel > 0 && totalPlateMenu > 0)
            {
                step6Count = 1;
            }

            listResult.Add(new GetStartedDataOutput() { StepId = 6, StepName = "SyncDataFromServerMachine", StepTitle = "<div>1. Sync initial data from Server to 2 machines</div><div>2. Scan all plates at machine 1 to manage inventory</div><div>3. Sync Inventory from machine to server database</div>", StepSubTitle = "", StepActionUrl = "", StepDoneFlg = step6Count });

            return listResult;
        }


    }
}
