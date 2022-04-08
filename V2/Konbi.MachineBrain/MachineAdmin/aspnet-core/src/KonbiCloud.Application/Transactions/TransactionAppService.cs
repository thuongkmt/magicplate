using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using KonbiCloud.Authorization;
using KonbiCloud.CloudSync;
using KonbiCloud.Configuration;
using KonbiCloud.Transactions.Dtos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Domain.Uow;
using KonbiCloud.Enums;
using System.IO;
using Abp.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using KonbiCloud.Common;
using KonbiCloud.RFIDTable;

namespace KonbiCloud.Transactions
{
    [AbpAuthorize(AppPermissions.Pages_Transactions)]
    public class TransactionAppService : KonbiCloudAppServiceBase, ITransactionAppService
    {
        private readonly IRepository<DetailTransaction, long> _transactionRepository;
        private readonly ITransactionSyncService transactionSyncService;
        private readonly IConfigurationRoot _appConfiguration;
        private readonly IDetailLogService detailLogService;
        private const string defaultImage = "assets/common/images/ic_nophoto.jpg";
        private const string ServerRootAddress = "App:ServerRootAddress";

        private readonly ITableManager _tableManager;


        public TransactionAppService(IRepository<DetailTransaction, long> transactionRepository,
                                     ITransactionSyncService transactionSyncService,
                                     IHostingEnvironment env,
                                     ITableManager tableManager,
                                     IDetailLogService detailLog)
        {
            _transactionRepository = transactionRepository;
            this.transactionSyncService = transactionSyncService;
            _appConfiguration = env.GetAppConfiguration();
            this.detailLogService = detailLog;
            _tableManager = tableManager;
        }

        [AbpAuthorize(AppPermissions.Pages_Transactions)]
        public async Task<PagedResultDto<TransactionDto>> GetAllTransactions(TransactionInput input)
        {
            try
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

                var transactions = _transactionRepository.GetAllIncluding()
                    .WhereIf(!string.IsNullOrWhiteSpace(input.SessionFilter), e => e.SessionId.ToString().Equals(input.SessionFilter))
                    .WhereIf(!string.IsNullOrEmpty(input.FromDate), e => e.PaymentTime.Date >= fromDate)
                    .WhereIf(!string.IsNullOrEmpty(input.ToDate), e => e.PaymentTime.Date <= toDate);

                if (input.TransactionType == 1)
                {
                    transactions = transactions.Where(e => e.Status == TransactionStatus.Success);
                }
                else
                {
                    transactions = transactions.Where(e => e.Status != TransactionStatus.Success);
                    transactions = transactions.WhereIf(!string.IsNullOrWhiteSpace(input.StateFilter), e => e.Status.ToString().Equals(input.StateFilter));
                }

                transactions = transactions
                   .Include(x => x.Session)
                   .Include(x => x.Products)
                   .Include("Products.Disc")
                   .Include("Products.Product")
                   .Include(e => e.CashlessDetail);

                var totalCount = await transactions.CountAsync();

                var tranList = await transactions.OrderBy(input.Sorting ?? "PaymentTime desc")
                    .PageBy(input)
                    .ToListAsync();

                var machineName = await SettingManager.GetSettingValueAsync(AppSettingNames.MachineName);
                var pathImage = Path.Combine(_appConfiguration[ServerRootAddress], Const.ImageFolder, _appConfiguration[AppSettingNames.TransactionImageFolder]);
                var list = new List<TransactionDto>();
                foreach (var x in tranList)
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
                        Session = x.Session?.Name,
                        TransactionId = string.IsNullOrEmpty(machineName) ? x.Id.ToString() : $"{machineName}_{x.Id}",
                        BeginTranImage = x.BeginTranImage,
                        EndTranImage = x.EndTranImage,
                        CardLabel = x.CashlessDetail == null ? "" : x.CashlessDetail.CardLabel == null ? "" : x.CashlessDetail.CardLabel,
                        CardNumber = x.CashlessDetail == null ? "" : x.CashlessDetail.CardNumber == null ? "" : "XXXXXXXXXXXX" + x.CashlessDetail.CardNumber.Substring(x.CashlessDetail.CardNumber.Length - 4),
                        ApproveCode = x.CashlessDetail == null ? "" : x.CashlessDetail.ApproveCode == null ? "" : x.CashlessDetail.ApproveCode,
                        Products = ObjectMapper.Map<ICollection<ProductTransactionDto>>(x.Products),
                        IsSynced = x.IsSynced
                    };
                    // use Card Label to denote payment mode.
                    if(x.PaymentType == Konbini.Messages.Enums.PaymentTypes.CASH)
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
                detailLogService.Log($"Get All Transactions returns : {list.Count}");
                return new PagedResultDto<TransactionDto>(totalCount, list);
            }
            catch (Exception ex)
            {
                Logger.Error($"Get all Transactions {ex.Message}", ex);
                return new PagedResultDto<TransactionDto>(0, new List<TransactionDto>());
            }
        }

        //Update Sync status after bg job is done
        [AbpAllowAnonymous]
        public async Task UpdateSyncStatus(IList<long> tranIds)
        {
            try
            {
                var existTrans = new List<DetailTransaction>();
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MustHaveTenant, AbpDataFilters.MayHaveTenant))
                {
                    existTrans = await _transactionRepository.GetAllListAsync();
                }

                foreach (var tranId in tranIds)
                {
                    var tran = existTrans.FirstOrDefault(x => x.Id == tranId);
                    if (tran != null)
                    {
                        tran.IsSynced = true;
                        tran.SyncDate = DateTime.Now;
                    }
                }
                await CurrentUnitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Logger.Error($"Update transaction sync status {ex.Message}", ex);
            }
        }

    }
}