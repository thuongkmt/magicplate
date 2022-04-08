using KonbiCloud.Dto;
using KonbiCloud.Transactions.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Transactions.Exporting
{
    public interface ITransactionsExcelExporter
    {
        FileDto ExportToFile(List<TransactionDto> customers);
    }
}
