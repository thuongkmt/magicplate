using KonbiCloud.RFIDTable.Cache;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.RFIDTable
{
    public interface ITableAppService
    {
        Task<SaleSessionCacheItem> GetSaleSessionInternalAsync();
        Task<TaxSettingsCacheItem> GetTaxSettingsInternalAsync();
        Task<long> GenerateTransactionAsync(TransactionInfo transactionInfo);
        string Validate(IEnumerable<PlateReadingInput> plates, SaleSessionCacheItem cacheItem);
        void UpdateTransactionImage(string transactionId, string beginImage, string endImage);
    }
}
