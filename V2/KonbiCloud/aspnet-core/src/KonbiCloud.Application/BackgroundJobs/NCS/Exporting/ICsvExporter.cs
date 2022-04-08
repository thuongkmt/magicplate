using KonbiCloud.Dto;
using KonbiCloud.Transactions.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.BackgroundJobs.NCS.Exporting
{
    public interface ICsvExporter
    {
        FileDto ExportPOSSHToFile(IList<TransactionDto> data);
        FileDto ExportPOSSPToFile(IList<TransactionDto> data);
        FileDto ExportPOSSTToFile(IList<TransactionDto> data);
        FileDto ExportPOSSVToFile(IList<TransactionDto> data);
        byte[] GetFile(FileDto file);
    }
}
