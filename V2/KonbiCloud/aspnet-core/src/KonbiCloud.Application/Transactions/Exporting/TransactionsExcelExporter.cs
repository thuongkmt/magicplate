using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using KonbiCloud.Configuration;
using KonbiCloud.DataExporting.Excel.NPOI;
using KonbiCloud.Dto;
using KonbiCloud.Storage;
using KonbiCloud.Transactions.Dtos;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace KonbiCloud.Transactions.Exporting
{
    public class TransactionsExcelExporter : NpoiExcelExporterBase, ITransactionsExcelExporter
    {
        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly IAbpSession _abpSession;

        public TransactionsExcelExporter(
            ITimeZoneConverter timeZoneConverter,
            IAbpSession abpSession,
            ITempFileCacheManager tempFileCacheManager) :
    base(tempFileCacheManager)
        {
            _timeZoneConverter = timeZoneConverter;
            _abpSession = abpSession;
           
        }

        public FileDto ExportToFile(List<TransactionDto> transactions)
        {
            return CreateExcelPackage(
                "Transactions.xlsx",
                excelPackage =>
                {

                    var sheet = excelPackage.CreateSheet(L("Transactions"));
                    var taxName = SettingManager.GetSettingValueAsync(AppSettings.MagicplateSettings.TaxSettings.TaxName).Result;
                    AddHeader(
                        sheet,
                        L("TransactionId"),
                        L("Machine"),
                        L("Session"),
                        L("Customer"),
                        L("Time"),
                        L("PaymentType"),
                        L("SalesAmount"),
                        taxName + " " + L("Amount"),
                        L("SalesAmountAfterXXX") + " " + taxName,
                        L("DiscountAmount")
                        );

                    AddObjects(
                        sheet, 2, transactions,
                           _ => _.TransactionId,                           // invoice no
                           _ => _.Machine,
                           _ => _.Session,
                           _ => _.Buyer,
                           _ => _timeZoneConverter.Convert(_.PaymentTime, _abpSession.TenantId, _abpSession.GetUserId()),        //Invoice date
                           _ => _.CardLabel,
                           _ => _.AmountBeforeTax,                                  // Sale Amount
                           _ => _.TaxAmount,                               // GST Amount
                           _ => _.TaxAmount + _.AmountBeforeTax,                          // Sale Amount included GST
                           _ => _.DiscountAmount                          // Sales Invoice Discount Amount
                        );
                    for (var i = 1; i <= transactions.Count; i++)
                    {
                        SetCellDataFormat(sheet.GetRow(i).Cells[4], "dd/MM/yyyy h:mm AM/PM");
                        SetCellDataFormat(sheet.GetRow(i).Cells[6], "0.00");
                        SetCellDataFormat(sheet.GetRow(i).Cells[7], "0.00");
                        SetCellDataFormat(sheet.GetRow(i).Cells[8], "0.00");
                        SetCellDataFormat(sheet.GetRow(i).Cells[9], "0.00");
                    }
                    sheet.AutoSizeColumn(0);
                    sheet.AutoSizeColumn(1);
                    sheet.AutoSizeColumn(2);
                    sheet.AutoSizeColumn(3);
                    sheet.AutoSizeColumn(4);
                    sheet.AutoSizeColumn(5);

                    //Transaction lines
                    var sheetItems = excelPackage.CreateSheet(L("Items"));

                    //preparing item list
                    var items = new List<dynamic>();
                    transactions.ForEach(el => {
                        var taxPercent = el.TaxPercentage;
                        var discountPercent = el.DiscountPercentage;
                        el.Products.ToList().ForEach(item => {
                            dynamic exportItem = new ExpandoObject();
                            exportItem.TransactionId = el.TransactionId;
                            exportItem.PaymentTime = _timeZoneConverter.Convert(el.PaymentTime, _abpSession.TenantId, _abpSession.GetUserId());
                            exportItem.Sku = item.Product != null ? item.Product.SKU : "";
                            exportItem.Name = item.Product != null ? item.Product.Name : "Custom Price";
                            exportItem.Quantity = 1;
                            exportItem.Amount = item.Amount;
                            exportItem.Discount = Math.Round(item.Amount*100/(100+taxPercent)*discountPercent/100,2);
                            exportItem.NetAmount = item.Amount * exportItem.Quantity;
                            exportItem.Category = item.Product?.Category?.Name;


                            items.Add(exportItem);
                        });
                    });
                    AddHeader(
                       sheetItems,
                       L("TransactionId"),
                        L("Time"),
                       L("ItemId"),
                        L("Category"),
                       L("ItemName"),
                       L("Quantity"),
                       L("UnitPrice"),
                       L("DiscountAmount"),
                       L("NetPrice")
                    
                       );

                    AddObjects(
                        sheetItems, 2, items,
                           _ => _.TransactionId,                            // invoice nno
                            _ => _.PaymentTime,        //Invoice date
                           _ => _.Sku,                                       //Item No.
                            _ => _.Category,                                     //Item category
                           _ => _.Name,                                     //Item Description
                           _ => _.Quantity,                                 // Quantity
                           _ => _.Amount,                    // Unit Price                   
                           _ => _.Discount,                  // Line Discount Amount
                           _ => _.NetAmount                 // Nett Price [(Quantity*Unit Price)-Line Discount Amount]
                        );
                    for (var i = 1; i <= items.Count; i++)
                    {
                        SetCellDataFormat(sheetItems.GetRow(i).Cells[4], "0");
                        SetCellDataFormat(sheetItems.GetRow(i).Cells[5], "0.00");
                        SetCellDataFormat(sheetItems.GetRow(i).Cells[6], "0.00");
                        SetCellDataFormat(sheetItems.GetRow(i).Cells[7], "0.00");
                        SetCellDataFormat(sheetItems.GetRow(i).Cells[8], "0.00");
                    }
                    sheetItems.AutoSizeColumn(0);
                    sheetItems.AutoSizeColumn(1);
                    sheetItems.AutoSizeColumn(2);
                    sheetItems.AutoSizeColumn(3);
                    sheetItems.AutoSizeColumn(4);
                    sheetItems.AutoSizeColumn(5);
                    sheetItems.AutoSizeColumn(6);
                    sheetItems.AutoSizeColumn(7);


                });
        }
    }
}
