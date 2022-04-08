using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.EntityFrameworkCore.Uow;
using Abp.Linq.Extensions;
using Abp.MultiTenancy;
using Abp.UI;
using KonbiCloud.Authorization;
using KonbiCloud.CloudSync;
using KonbiCloud.Common;
using KonbiCloud.Configuration;
using KonbiCloud.Dto;
using KonbiCloud.EntityFrameworkCore;
using KonbiCloud.Enums;
using KonbiCloud.Machines;
using KonbiCloud.Plate;
using KonbiCloud.Transactions.Dtos;
using KonbiCloud.Transactions.Exporting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using System.Transactions;

namespace KonbiCloud.Transactions
{
    [AbpAuthorize(AppPermissions.Pages_Transactions)]
    public class TransactionAppService : KonbiCloudAppServiceBase, ITransactionAppService
    {
        private readonly IRepository<DetailTransaction, long> _transactionRepository;
        private readonly IRepository<Machine, Guid> _machineRepository;
        private readonly IRepository<Session, Guid> _sessionRepository;
        private readonly IRepository<Employees.Employee, Guid> _employeeRepository;
        private readonly IRepository<Disc, Guid> _discRepository;
        private readonly IConfigurationRoot _appConfiguration;
        private readonly IDetailLogService _detailLogService;
        private IIocResolver _iocResolver;
        private readonly ITransactionsExcelExporter _transactionsExcelExporter;
        private const string defaultImage = "assets/common/images/ic_nophoto.jpg";
        private const string ServerRootAddress = "App:ServerRootAddress";

        public TransactionAppService(IRepository<DetailTransaction, long> transactionRepository,
                                     IRepository<Machine, Guid> machineRepository,
                                     IRepository<Session, Guid> sessionRepository,
                                     IRepository<Employees.Employee, Guid> employeeRepository,
                                     IRepository<Disc, Guid> discRepository,
                                     IHostingEnvironment env,
                                     IDetailLogService detailLog,
                                     ITransactionsExcelExporter transactionsExcelExporter,
                                     IIocResolver iocResolver)
        {
            _transactionRepository = transactionRepository;
            _machineRepository = machineRepository;
            _sessionRepository = sessionRepository;
            _employeeRepository = employeeRepository;
            _discRepository = discRepository;
            _appConfiguration = env.GetAppConfiguration();
            _detailLogService = detailLog;
            _iocResolver = iocResolver;
            _transactionsExcelExporter = transactionsExcelExporter;
        }

        [AbpAuthorize(AppPermissions.Pages_Transactions)]
        public async Task<PagedResultDto<TransactionDto>> GetAllTransactions(TransactionInput input)
        {
            try
            {
                DateTime? fromDate = null;
                if(!string.IsNullOrEmpty(input.FromDate))
                {
                    fromDate = DateTime.ParseExact(input.FromDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                }

                DateTime? toDate = null;
                if (!string.IsNullOrEmpty(input.ToDate))
                {
                    toDate = DateTime.ParseExact(input.ToDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                }

                var transactions = _transactionRepository.GetAllIncluding()
                    .WhereIf(!string.IsNullOrWhiteSpace(input.SessionFilter), e => e.SessionId.ToString().Equals(input.SessionFilter))
                    .WhereIf(!string.IsNullOrEmpty(input.FromDate), e => e.PaymentTime.Date >= fromDate)
                    .WhereIf(!string.IsNullOrEmpty(input.ToDate), e => e.PaymentTime.Date <= toDate)
                    .WhereIf(input.MachineFilter.HasValue, e => e.MachineId.Equals(input.MachineFilter));

                if (input.TransactionType == 1)
                {
                    transactions = transactions.Where(e => e.Status == Enums.TransactionStatus.Success);
                }
                else
                {
                    transactions = transactions.Where(e => e.Status != Enums.TransactionStatus.Success);
                    transactions = transactions.WhereIf(!string.IsNullOrWhiteSpace(input.StateFilter), e => e.Status.ToString().Equals(input.StateFilter));
                }

                transactions = transactions
                   .Include(x => x.Machine)
                   .Include(x => x.Session)
                   .Include("Products.Disc")
                   .Include("Products.Product")
                   .Include("Products.Product.Category")
                   .Include(x => x.CashlessDetail);

                //foreach (var tran in transactions)
                //{
                //    tran.ProductCount = tran.Products == null ? 0 : tran.Products.Count;
                //}

                var totalCount = await transactions.CountAsync();
                var tranLists= await transactions.OrderBy(input.Sorting ?? "PaymentTime desc")
                    .PageBy(input)
                    .ToListAsync();

                var pathImage = Path.Combine(_appConfiguration[ServerRootAddress], Const.ImageFolder, _appConfiguration[AppSettingNames.TransactionImageFolder]);
                var list = new List<TransactionDto>();
                foreach (var x in tranLists)
                {
                    var newTran = new TransactionDto()
                    {
                        Id = x.Id,
                        TranCode = x.TranCode.ToString(),
                        Buyer = x.Buyer,
                        PaymentTime = x.PaymentTime,
                        Amount = x.Amount,
                        TaxPercentage = x.TaxPercentage,
                        DiscountPercentage = x.DiscountPercentage,
                        PlatesQuantity = x.ProductCount,
                        States = x.Status.ToString(),
                        Machine = x.Machine == null ? null :  x.Machine.Name,
                        Session = x.Session == null ? null : x.Session.Name,
                        TransactionId = x.Machine == null ? x.TransactionId : $"{ x.Machine.Name.Replace(" ","")}-{x.TransactionId}",
                        BeginTranImage = x.BeginTranImage,
                        EndTranImage = x.EndTranImage,
                        CardLabel = x.CashlessDetail == null ? "" : x.CashlessDetail.CardLabel == null ? "" : x.CashlessDetail.CardLabel,
                        CardNumber = x.CashlessDetail == null ? "" : x.CashlessDetail.CardNumber == null ? "" : "XXXXXXXXXXXX" + x.CashlessDetail.CardNumber.Substring(x.CashlessDetail.CardNumber.Length - 4),
                        ApproveCode = x.CashlessDetail == null ? "" : x.CashlessDetail.ApproveCode == null ? "" : x.CashlessDetail.ApproveCode,
                        Products = ObjectMapper.Map<ICollection<ProductTransactionDto>>(x.Products)
                    };
                    // use Card Label to denote payment mode.
                    if (x.PaymentType == PaymentType.Cash)
                    {
                        newTran.CardLabel = "Cash";
                    }
                    if (string.IsNullOrEmpty(newTran.CardLabel))
                    {
                        newTran.CardLabel = x.PaymentType.ToString();
                    }
                    if (string.IsNullOrEmpty(x.BeginTranImage))
                    {
                        newTran.BeginTranImage = defaultImage;
                    }
                    else
                    {
                        newTran.BeginTranImage = Path.Combine(pathImage, x.BeginTranImage);
                    }
                    if (string.IsNullOrEmpty(x.EndTranImage))
                    {
                        newTran.EndTranImage = defaultImage;
                    }
                    else
                    {
                        newTran.EndTranImage = Path.Combine(pathImage, x.EndTranImage);
                    }
                    list.Add(newTran);
                }
                _detailLogService.Log($"Get All Transactions returns : {list.Count}");
                return new PagedResultDto<TransactionDto>(totalCount, list);
            }
            catch (Exception ex)
            {
                Logger.Error($"Get all Transactions {ex.Message}", ex);
                return new PagedResultDto<TransactionDto>(0, new List<TransactionDto>());
            }
        }
        [AbpAuthorize(AppPermissions.Pages_Transactions)]
        public async Task<FileDto> GetAllTransactionsForExcel(GetAllTransactionsForExcelInput input)
        {
            
                DateTime? fromDate = null;
                if (!string.IsNullOrEmpty(input.FromDate))
                {
                    fromDate = DateTime.ParseExact(input.FromDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                }

                DateTime? toDate = null;
                if (!string.IsNullOrEmpty(input.ToDate))
                {
                    toDate = DateTime.ParseExact(input.ToDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                }

                var transactionQuery = _transactionRepository.GetAllIncluding()
                    .WhereIf(!string.IsNullOrWhiteSpace(input.SessionFilter), e => e.SessionId.ToString().Equals(input.SessionFilter))
                    .WhereIf(fromDate.HasValue, e => e.PaymentTime >= fromDate)
                    .WhereIf(toDate.HasValue, e => e.PaymentTime < toDate.Value.AddDays(1))
                    .WhereIf(input.MachineFilter.HasValue, e => e.MachineId.Equals(input.MachineFilter));

                if (input.TransactionType == 1)
                {
                    transactionQuery = transactionQuery.Where(e => e.Status == Enums.TransactionStatus.Success);
                }
                else
                {
                    transactionQuery = transactionQuery.Where(e => e.Status != Enums.TransactionStatus.Success);
                    transactionQuery = transactionQuery.WhereIf(!string.IsNullOrWhiteSpace(input.StateFilter), e => e.Status.ToString().Equals(input.StateFilter));
                }

                transactionQuery = transactionQuery
                   .Include(x => x.Machine)
                   .Include(x => x.Session)
                   .Include("Products.Disc")
                   .Include("Products.Product")
                   .Include("Products.Product.Category")
                   .Include(x => x.CashlessDetail);

              
             
                var tranLists = await transactionQuery.OrderBy(input.Sorting ?? "PaymentTime desc")
                    .ToListAsync();

                var pathImage = Path.Combine(_appConfiguration[ServerRootAddress], Const.ImageFolder, _appConfiguration[AppSettingNames.TransactionImageFolder]);
                var list = new List<TransactionDto>();
                foreach (var x in tranLists)
                {
                    var newTran = new TransactionDto()
                    {
                        Id = x.Id,
                        TranCode = x.TranCode.ToString(),
                        Buyer = x.Buyer,
                        PaymentTime = x.PaymentTime,
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
                    // use Card Label to denote payment mode.
                    if (x.PaymentType == PaymentType.Cash)
                    {
                        newTran.CardLabel = "Cash";
                    }
                    if (string.IsNullOrEmpty(newTran.CardLabel))
                    {
                        newTran.CardLabel = x.PaymentType.ToString();
                    }
                    if (string.IsNullOrEmpty(x.BeginTranImage))
                    {
                        newTran.BeginTranImage = defaultImage;
                    }
                    else
                    {
                        newTran.BeginTranImage = Path.Combine(pathImage, x.BeginTranImage);
                    }
                    if (string.IsNullOrEmpty(x.EndTranImage))
                    {
                        newTran.EndTranImage = defaultImage;
                    }
                    else
                    {
                        newTran.EndTranImage = Path.Combine(pathImage, x.EndTranImage);
                    }
                    list.Add(newTran);
                }
                  return _transactionsExcelExporter.ExportToFile(list);


        }

        [AbpAllowAnonymous]
        public async Task<List<long>> AddTransactions(IList<DetailTransaction> trans)
        {
            var originalTransactionList = new List<DetailTransaction>(trans);
            try
            {
                
                if (trans.Count() == 0)
                {
                    return new List<long>();
                }
                var successTrans = new List<long>();
                IQueryable<Machine> machinesQuery;
                IQueryable<Session> sessionsQuery;
                IQueryable<Disc> dishesQuery;
                IQueryable<DetailTransaction> existTransQuery;
                Machine machine = null;
                var addList = new List<DetailTransaction>();
                var updateList = new List<DetailTransaction>();
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MustHaveTenant, AbpDataFilters.MayHaveTenant))
                {
                    var mId = trans[0].MachineId;
                    machine = await _machineRepository.FirstOrDefaultAsync(x => x.Id == mId);
                    if (machine == null)
                    {
                        Logger.Error($"Sync Transaction: MachineId: {mId} does not exist");
                        return null;
                    }
                    else if (machine.IsDeleted)
                    {
                        Logger.Error($"Sync Transaction: {machine.Name}-{mId} is deleted");
                        return null;
                    }

                    machinesQuery = _machineRepository.GetAll();
                    sessionsQuery = _sessionRepository.GetAll();
                    dishesQuery =  _discRepository.GetAll();                  
                    existTransQuery = _transactionRepository.GetAll();

                    var tranImgFolderPath = Path.Combine(Directory.GetCurrentDirectory(), Const.ImageFolder, _appConfiguration[AppSettingNames.TransactionImageFolder]);
                    
                    foreach (var tran in trans)
                    {
                        _detailLogService.Log($"{tran.MachineName}-Sync Transaction: {tran.ToString()}");
                        tran.TransactionId = tran.Id.ToString();
                        var oldId = tran.Id;
                        tran.Id = 0;

                        //transaction existed
                        if (await existTransQuery.AnyAsync(x => x.TranCode == tran.TranCode && x.MachineId == tran.MachineId))
                        {
                            successTrans.Add(oldId);
                            continue;
                        }

                        if (tran.MachineId != null)
                        {
                            if (!(await machinesQuery.AnyAsync(x => x.Id == tran.MachineId)))
                            {
                                tran.MachineId = null;
                            }
                        }
                        if (tran.SessionId != null)
                        {
                            if (!(await sessionsQuery.AnyAsync(x => x.Id == tran.SessionId)))
                            {
                                tran.SessionId = null;
                            }
                        }

                        if (tran.CashlessDetail != null)
                        {
                            tran.CashlessDetail.Id = 0;
                        }

                        foreach (var item in tran.Products)
                        {
                            item.Transaction = null;
                            item.Id = 0;
                            item.Disc = null;
                            if (item.Product != null)
                            {
                                item.ProductId = item.Product.Id;
                                item.Product = null;
                            }
                            
                        }

                        //Save image
                        ExtractTransactionImage(tran.BeginTranImageByte, tranImgFolderPath);
                        ExtractTransactionImage(tran.EndTranImageByte, tranImgFolderPath);

                        tran.TenantId = machine?.TenantId;
                        addList.Add(tran);                     
                        _detailLogService.Log($"{tran.MachineName}-Sync Transaction-{tran.Id} added.");
                    }
                }

                using (var uowManager = _iocResolver.ResolveAsDisposable<IUnitOfWorkManager>())
                {
                    using (var uow = uowManager.Object.Begin(TransactionScopeOption.Suppress))
                    {
                        var dbContext = uowManager.Object.Current.GetDbContext<KonbiCloudDbContext>(MultiTenancySides.Tenant);
                        await dbContext.AddRangeAsync(addList);
                        dbContext.UpdateRange(updateList);
                        uow.Complete();
                        _detailLogService.Log($"Sync Transaction: Done,Uploaded: {trans.Count} Added: {addList.Count}, Ignore existing: {successTrans.Count}");
                    }
                }
                return successTrans;
            }
            catch (Exception ex)
            {
                Logger.Error($"Original transaction list: {JsonConvert.SerializeObject(trans, Formatting.Indented, new JsonSerializerSettings {PreserveReferencesHandling = PreserveReferencesHandling.Objects})}");
                Logger.Error($"Add Transactions {ex.Message}", ex);                
                return null;
            }
        }
        private void ExtractTransactionImage(byte[] imgBytes, string tranImgFolderPath)
        {
            try
            {
                if (imgBytes != null)
                {
                    using (var zipStream = new MemoryStream(imgBytes))
                    {
                        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Read, true))
                        {
                            archive.ExtractToDirectory(tranImgFolderPath, true);
                            _detailLogService.Log($"Extract transaction image: {tranImgFolderPath}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error when decompress transaction images: {ex.Message}", ex);
            }
        }
    }
}