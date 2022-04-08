using KonbiBrain.Common;
using KonbiCloud.Transactions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KonbiCloud.CloudSync
{
    public interface ITransactionSyncService
    {
        Task<RestApiGenericResult<long>> PushTransactionsToServer(IList<DetailTransaction> trans, string transImageFolder);
    }
}
