using Abp.Application.Services;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Services;
using Abp.Domain.Uow;
using KonbiBrain.Common;
using KonbiCloud.CloudSync;
using KonbiCloud.Common;
using KonbiCloud.Configuration;
using KonbiCloud.Enums;
using KonbiCloud.Plate;
using KonbiCloud.Products;
using KonbiCloud.RFIDTable.Cache;
using KonbiCloud.RFIDTable.Dtos;
using KonbiCloud.Sessions;
using KonbiCloud.Transactions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KonbiCloud.RFIDTable
{
    [RemoteService(IsEnabled = false)]
    public class TableAppService : ApplicationService, ITableAppService
    {
        private readonly IRepository<Session, Guid> sessionRepository;
        private readonly IRepository<ProductMenu.ProductMenu, Guid> _productMenuRepository;

        private readonly IRepository<DetailTransaction, long> transactionRepository;
        private readonly IRepository<Disc, Guid> discRepository;
        private readonly IRepository<Plate.Plate, Guid> plateRepository;
        private readonly IRepository<Product, Guid> _productRepository;


        private readonly ISettingManager _settingManager;
        private readonly ITransactionSyncService transactionSyncService;
        private readonly IConfigurationRoot _appConfiguration;
        private readonly IDetailLogService detailLogService;
        private readonly IUnitOfWorkManager unitOfWorkManager;

        protected bool PreventSellingSamePlate => _settingManager.GetSettingValue<bool>(AppSettingNames.PreventSellingSamePlateInASession);

        public TableAppService(ISettingManager settingManager,
                               IRepository<Session, Guid> sessionRepository,
                               IRepository<ProductMenu.ProductMenu, Guid> productMenuRepository,
                               IRepository<DetailTransaction, long> transactionRepository,
                               IRepository<Disc, Guid> discRepository,
                               IRepository<Plate.Plate, Guid> plateRepository,
                               IRepository<Product, Guid> productRepository,
                               ITransactionSyncService transactionSyncService,
                               IHostingEnvironment env,
                               IDetailLogService detailLog,
                               IUnitOfWorkManager unitOfWorkManager)
        {
            this.sessionRepository = sessionRepository;
            this._productMenuRepository = productMenuRepository;
            this.transactionRepository = transactionRepository;
            this.discRepository = discRepository;
            this.plateRepository = plateRepository;
            this._productRepository = productRepository;
            this.transactionSyncService = transactionSyncService;
            this.unitOfWorkManager = unitOfWorkManager;

            _settingManager = settingManager;
            _appConfiguration = env.GetAppConfiguration();
            this.detailLogService = detailLog;
        }

        public async Task<long> GenerateTransactionAsync(TransactionInfo transactionInfo)
        {
            try
            {
                this.detailLogService.Log("Start change image of transaction.");

                // Create new detail transaction for save to local database.
                var newTran = new DetailTransaction()
                {
                    TranCode = transactionInfo.Id,
                    Amount = transactionInfo.Amount,
                    TaxPercentage = transactionInfo.TaxPercentage,
                    DiscountPercentage = transactionInfo.DiscountPercentage,

                    CashlessDetail = transactionInfo.CashlessInfo != null && transactionInfo.PaymentType != Konbini.Messages.Enums.PaymentTypes.CASH ? new CashlessDetail()
                    {
                        Aid = transactionInfo.CashlessInfo.Aid,
                        Amount = transactionInfo.CashlessInfo.Amount,
                        AppLabel = transactionInfo.CashlessInfo.AppLabel,
                        ApproveCode = transactionInfo.CashlessInfo.ApproveCode,
                        Batch = transactionInfo.CashlessInfo.Batch,
                        CardLabel = transactionInfo.CashlessInfo.CardLabel,
                        CardNumber = transactionInfo.CashlessInfo.CardNumber,
                        EntryMode = transactionInfo.CashlessInfo.EntryMode,
                        Invoice = transactionInfo.CashlessInfo.Invoice,
                        Mid = transactionInfo.CashlessInfo.Mid,
                        Rrn = transactionInfo.CashlessInfo.Rrn,
                        Tc = transactionInfo.CashlessInfo.Tc,
                        Tid = transactionInfo.CashlessInfo.Tid
                    } : null,

                    PaymentState = transactionInfo.PaymentState,
                    PaymentTime = DateTime.Now,
                    PaymentType = transactionInfo.PaymentType,
                    StartTime = DateTime.Now,
                    Status = transactionInfo.PaymentState == Konbini.Messages.Enums.PaymentState.Success ? Enums.TransactionStatus.Success : Enums.TransactionStatus.Error,
                    Buyer = transactionInfo.Buyer,
                    BeginTranImage = transactionInfo.BeginTranImage,
                    EndTranImage = transactionInfo.EndTranImage
                };

                if (transactionInfo.SessionId != Guid.Empty)
                    newTran.SessionId = transactionInfo.SessionId;

                var mId = Guid.Empty;
                Guid.TryParse(await SettingManager.GetSettingValueAsync(AppSettingNames.MachineId), out mId);

                if (mId != Guid.Empty)
                {
                    newTran.MachineId = mId;
                }

                newTran.MachineName = await SettingManager.GetSettingValueAsync(AppSettingNames.MachineName);

                newTran.Products = new List<ProductTransaction>();

                if (transactionInfo.MenuItems != null && transactionInfo.MenuItems.Any())
                {
                    foreach (var mn in transactionInfo.MenuItems)
                    {
                        var disc = discRepository.FirstOrDefault(x => x.Code.Equals(mn.Plate.Code) && x.Uid.Equals(mn.Plate.Uid));
                        var product = _productRepository.FirstOrDefault(x => x.Id.Equals(mn.ProductId));
                        newTran.Products.Add(new ProductTransaction
                        {
                            Amount = mn.Price,
                            DiscountAmount = newTran.DiscountPercentage >0? mn.Price * newTran.DiscountPercentage/100: 0,
                            DiscId = disc?.Id,
                            Product = product
                        });
                    }
                }

                // Log trnsaction
                string newTranJson = JsonConvert.SerializeObject(newTran, Formatting.Indented);
                Console.WriteLine(newTranJson);
                this.detailLogService.Log("Saving transaction details");
                this.detailLogService.Log(newTranJson);

                // Save new transaction to local database.
                this.detailLogService.Log("Start save new transaction to local database.");
                var localId = await transactionRepository.InsertAndGetIdAsync(newTran);

                return localId;
            }
            catch (Exception ex)
            {
                Logger?.Error(ex.Message, ex);
                return 0;
            }
        }

        /// <summary>
        /// Update image for transaction when camera return.
        /// </summary>
        /// <param name="transactionId">Transaction id.</param>
        /// <param name="beginImage">Path begin images.</param>
        /// <param name="endImage">Path end images.</param>
        public void UpdateTransactionImage(string transactionId, string beginImage, string endImage)
        {
            var source = new CancellationTokenSource();
            var token = source.Token;
            source.CancelAfter(10 * 1000);

            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        //get transaction by transaction code.
                        using (var unitOfWork = unitOfWorkManager.Begin())
                        {
                            var transaction = transactionRepository.GetAllIncluding(el => el.CashDetail, el => el.CashlessDetail, el => el.Products)
                                .Include("Products.Disc")
                                .Include("Products.Product").FirstOrDefault(x => x.TranCode == Guid.Parse(transactionId));
                            var hasUpdated = false;
                            //wait for transaction stored into database
                            if (transaction != null)
                            {
                                var tranImgFolderPath = Path.Combine(Directory.GetCurrentDirectory(), Const.ImageFolder, _appConfiguration[AppSettingNames.TransactionImageFolder]);

                                var beginImgName = $"{transactionId}.{Const.Begin}.jpg";
                                var beginImgPath = Path.Combine(tranImgFolderPath, beginImgName);

                                if (File.Exists(beginImage))
                                {
                                    File.Copy(beginImage, beginImgPath);
                                    if (File.Exists(beginImgPath))
                                    {
                                        File.Delete(beginImage);
                                    }
                                    transaction.BeginTranImage = beginImgName;
                                    hasUpdated = true;
                                }

                                var endImgName = $"{transactionId}.{Const.End}.jpg";
                                var endImgPath = Path.Combine(tranImgFolderPath, endImgName);

                                if (File.Exists(endImage))
                                {
                                    File.Copy(endImage, endImgPath);
                                    if (File.Exists(endImgPath))
                                    {
                                        File.Delete(endImage);
                                    }
                                    transaction.EndTranImage = endImgName;
                                    hasUpdated = true;
                                }
                                // Update image.
                                if (hasUpdated)
                                {
                                    await transactionRepository.UpdateAsync(transaction);

                                    var result = await transactionSyncService.PushTransactionsToServer(new List<DetailTransaction> { transaction }, _appConfiguration[AppSettingNames.TransactionImageFolder]);
                                    if (result.result != null && result.result.Any())
                                    {
                                        var syncedTran = await transactionRepository.FirstOrDefaultAsync(transaction.Id);
                                        if (syncedTran != null)
                                        {
                                            syncedTran.IsSynced = true;
                                            syncedTran.SyncDate = DateTime.Now;
                                            await transactionRepository.UpdateAsync(syncedTran);
                                        }
                                    }
                                    await unitOfWork.CompleteAsync();
                                }
                                // stop loop
                                break;
                            }

                        }
                    }

                    catch (Exception ex)
                    {

                        Logger?.Error(ex.Message, ex);
                    }

                    await Task.Delay(100);
                }
            }, token);

        }

        public async Task<SaleSessionCacheItem> GetSaleSessionInternalAsync()
        {
            try
            {
                SaleSessionCacheItem cacheItem = new SaleSessionCacheItem();
                var currentTime = Convert.ToInt32(string.Format("{0:00}{1:00}", DateTime.Now.Hour, DateTime.Now.Minute));
                var s = await sessionRepository.GetAll().Where(session => session.IsDeleted == false && session.ActiveFlg == true && currentTime > Convert.ToInt32(session.FromHrs.Replace(":", "")) && currentTime <= Convert.ToInt32(session.ToHrs.Replace(":", ""))).OrderBy(session => session.FromHrs.Replace(":", ""))
                  .FirstOrDefaultAsync();
                if (s != null)
                {
                    var taxSettings = await GetTaxSettingsInternalAsync();
                    cacheItem.SessionInfo = new SessionInfo() { Id = s.Id, Name = s.Name, FromHrs = s.FromHrs.Replace(":", ""), ToHrs = s.ToHrs.Replace(":", "") };

                    var productMenus = _productMenuRepository.GetAllIncluding(el=> el.Product, el=> el.Plate)
                        .Where(el => el.IsDeleted == false && el.SelectedDate == DateTime.Today && el.SessionId == s.Id)                       
                        .Where(el => el.Product.IsDeleted == false)
                        .OrderBy(el => el.Plate.Name).ToList();

                    cacheItem.MenuItems = new List<MenuItemInfo>();
                    foreach (var el in productMenus)
                    {
                        var menuItemInfo = new MenuItemInfo()
                        {
                            Name = el.Product?.Name,
                            ImageUrl = el.Product?.ImageUrl,
                            Desc = el.Product?.Desc,
                            Code = el.Plate != null ? el.Plate.Code : "",
                            Color = el.Plate != null ? el.Plate.Color : "",
                            Price =  taxSettings.TaxSettings.Type == TaxType.Inclusive? el.Price: el.Price*(100+taxSettings.TaxSettings.Percentage)/100,
                            PriceContractor = taxSettings.TaxSettings.Type == TaxType.Inclusive ? el.ContractorPrice : el.ContractorPrice * (100 + taxSettings.TaxSettings.Percentage) / 100,
                            PlateId = el.PlateId.HasValue? el.PlateId.Value : Guid.Empty,
                            ProductId = (Guid)el.ProductId,
                            ProductName = el.Product?.Name,
                            BarCode = el.Product.Barcode
                        };
                     
                        cacheItem.MenuItems.Add(menuItemInfo);
                    }

                    cacheItem.PlateInfos = await discRepository.GetAll().Where(el => el.IsDeleted == false).Select(el =>
                       new PlateInfo()
                       {
                           Code = el.Code,
                           PlateId = el.PlateId,
                           Uid = el.Uid,
                           Type = el.Plate.Type
                       }
                    ).ToListAsync();

                    //cacheItem.Discs = discRepository.GetAll().Include(el => el.Plate).ToList();
                }
                return cacheItem;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<TaxSettingsCacheItem> GetTaxSettingsInternalAsync()
        {
            try
            {
                var percentStr = await SettingManager.GetSettingValueAsync(AppSettings.MagicplateSettings.TaxSettings.TaxPercentage);
                decimal.TryParse(percentStr, out decimal percent);
                var type = await SettingManager.GetSettingValueAsync(AppSettings.MagicplateSettings.TaxSettings.TaxType);
                if (!TaxType.TryParse(type, true, out TaxType taxType))
                    taxType = TaxType.Exclusive;

                var output = new TaxSettingsDto()
                {
                    Name = await SettingManager.GetSettingValueAsync(AppSettings.MagicplateSettings.TaxSettings.TaxName),
                    Type = taxType,
                    Percentage = percent,

                };
                return new TaxSettingsCacheItem() { TaxSettings = output };

               
            }
            catch (Exception ex)
            {
                detailLogService.Log(ex.Message);
            }
            return null;
        }
        public string Validate(IEnumerable<PlateReadingInput> plates, SaleSessionCacheItem cacheItem)
        {

            if (cacheItem.SessionInfo == null)
                return Konbini.Messages.MessageConstants.CAN_NOT_FIND_SESSION;

            var normalPlates = plates.Where(e => !e.HasCustomPrice);

            //validating for normal plates
            if (normalPlates.Count() > 0)
            {
                if (cacheItem.PlateInfos == null || cacheItem.PlateInfos.Count == 0)
                    return Konbini.Messages.MessageConstants.NO_PLATE_REGISTERED;
                if (cacheItem.MenuItems == null || cacheItem.MenuItems.Count == 0)
                    return Konbini.Messages.MessageConstants.NO_MENU_SESSION;

                //check all plates input availabe on session

                // menu items with assigned plate model list
                var menuItemWithPlateModels = cacheItem.MenuItems.Where(el => !string.IsNullOrEmpty(el.Code));
                foreach (var item in normalPlates.Where(el => (!cacheItem.PlateInfos.Any(x => x.Code == el.UType && x.Type != Enums.PlateType.Plate)))) //exclude Trays and takeaway tags
                {
                    if(!menuItemWithPlateModels.Any(el=> el.Code == item.UType))
                    {                        
                        return $"({item.UType})-{Konbini.Messages.MessageConstants.UNREGISTED_PLATEMODEL_IN_SESSION}";
                    }
                    if (!cacheItem.PlateInfos.Any(x => x.Code == item.UType && x.Uid == item.UID))
                    {
                        return $"({item.UType}:{item.UID})-({Konbini.Messages.MessageConstants.UNREGISTED_PLATE_IN_SESSION})";
                    }
                }
            }
            var currentSessionId = cacheItem.SessionInfo != null ? cacheItem.SessionInfo.Id : Guid.Empty;


            if (PreventSellingSamePlate)
            {
                // validate to ensure the plates have not sold for the session
                var foundSoldPlate = transactionRepository.GetAll().Any(el =>
                el.Status == Enums.TransactionStatus.Success
                && el.PaymentTime.Year == DateTime.Today.Year
                && el.PaymentTime.Month == DateTime.Today.Month
                && el.PaymentTime.Day == DateTime.Today.Day
                && el.Session.Id == currentSessionId);
                if (foundSoldPlate)
                    return Konbini.Messages.MessageConstants.SOLD_PLATE_DETECTED;
            }
            return string.Empty;
        }
    }
}
