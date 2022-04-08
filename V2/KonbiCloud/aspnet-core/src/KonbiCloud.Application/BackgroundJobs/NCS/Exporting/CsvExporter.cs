using Abp.AspNetZeroCore.Net;
using Abp.Collections.Extensions;
using Abp.Dependency;
using KonbiCloud.Dto;
using KonbiCloud.Storage;
using KonbiCloud.Transactions.Dtos;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;

namespace KonbiCloud.BackgroundJobs.NCS.Exporting
{
    public class CsvExporter: ITransientDependency, ICsvExporter
    {
        private readonly ITempFileCacheManager _tempFileCacheManager;
        private IList<string> _headers;
        private IList<string[]> _data;
        private char _delimiter = '|'; 
        public CsvExporter(ITempFileCacheManager tempFileCacheManager)
        {
            _tempFileCacheManager = tempFileCacheManager;
            _headers = new List<string>();
            _data = new List<string[]>();
        }
        protected FileDto CreateCsvPackage(string fileName, Action<CsvExporter> creator)
        {
            var file = new FileDto(fileName, MimeTypeNames.TextCsv);
            _headers = new List<string>();
            _data = new List<string[]>();
            creator(this);
            Save(file);

            return file;

        }
        protected void AddHeader(params string[] headerTexts)
        {
            if (headerTexts.IsNullOrEmpty())
            {
                return;
            }
            for (var i = 0; i < headerTexts.Length; i++)
            {
                _headers.Add(headerTexts[i]);               
            }
        }

        protected void AddObjects<T>(IList<T> items, params Func<T, object>[] propertySelectors)
        {
            if (items.IsNullOrEmpty() || propertySelectors.IsNullOrEmpty())
            {
                return;
            }

            for (var i = 1; i <= items.Count; i++)
            {
                var row = new List<string>();
                for (var j = 0; j < propertySelectors.Length; j++)
                {
                
                    var value = propertySelectors[j](items[i - 1]);
                    if (value != null)
                    {
                        row.Add(value.ToString());
                    }
                }
                _data.Add(row.ToArray());
            }
        }

        protected void Save(FileDto file)
        {
            using (var stream = new MemoryStream())
            {
                using(var streamWriter = new StreamWriter(stream))
                {
                    if (_headers.Count > 0) {
                        streamWriter.WriteLine(string.Join(_delimiter, _headers));
                    }
                    if (_data.Count > 0)
                    {
                        foreach(var row in _data)
                        {
                            if (row.Length > 0)
                            {
                                streamWriter.WriteLine(string.Join(_delimiter, row));
                            }
                        }
                    }
                }
               
                _tempFileCacheManager.SetFile(file.FileToken, stream.ToArray());
            }
        }
        public FileDto ExportPOSSHToFile(IList<TransactionDto> data)
        {
            return CreateCsvPackage("POSSH", generator => {
                generator.AddObjects(data,
                   _ => _.TransactionId,                           // invoice nno
                   _ => _.PaymentTime.ToString("yyyyMMdd"),        //Invoice date
                   _ => "NCS",                                        //Outlet No.
                   _ => "DI",                                        // Ordering Mode
                   _ => "",                                        // Reference No
                   _ => _.AmountBeforeTax.ToString("N2"),                                  // Sale Amount
                   _ => _.TaxAmount.ToString("N2"),                               // GST Amount
                   _ => _.Amount.ToString("N2"),                          // Sale Amount included GST
                   _ => _.DiscountAmount.ToString("N2")                          // Sales Invoice Discount Amount

               );
            });
        }
        public FileDto ExportPOSSPToFile(IList<TransactionDto> data)
        {
            return CreateCsvPackage("POSSP", generator => {
                generator.AddObjects(data, 
                    _ => _.TransactionId,                                                                                               // invoice nno
                    _ => "",                                                                                                            // Reference No.
                    _ => _.PaymentTime.ToString("yyyyMMdd"),                                                                            // Payment Date
                    _ => _.PaymentType.ToString(),                                                                                      // Payment Mode
                    _ => _.CashlessPaidAmount.HasValue? _.CashlessPaidAmount.Value.ToString("N2") : _.Total.ToString("N2")     // Payment Amount
                );
            });
        }

        public byte[] GetFile(FileDto file)
        {
            return _tempFileCacheManager.GetFile(file.FileToken);
        }

        public FileDto ExportPOSSTToFile(IList<TransactionDto> data)
        {
            return CreateCsvPackage("POSST", generator => {
                var items = new List<dynamic>();
                data.ToList().ForEach(el => {
                    var discountPercent = el.DiscountPercentage;
                    var taxPercent = el.TaxPercentage;
                    el.Products.ToList().ForEach(item => {
                        dynamic exportItem = new ExpandoObject();
                        exportItem.TransactionId = el.TransactionId;
                        exportItem.Id = item.Product!=null? item.Product.SKU: "";
                        exportItem.Name = item.Product!=null? item.Product.Name : "Custom Price";
                        exportItem.Quantity = 1;
                        exportItem.Amount = item.Amount;
                        exportItem.Discount = Math.Round(item.Amount*100/(100+taxPercent) *discountPercent/100,2);
                        exportItem.NetAmount = item.Amount * exportItem.Quantity - exportItem.Discount;

                       
                    items.Add(exportItem);
                    });
                });

                generator.AddObjects(items,
                   _ => _.TransactionId,                            // invoice nno
                   _ => _.Id==null? "": _.Id,                       //Item No.
                   _ => _.Name,                                     //Item Description
                   _ => _.Quantity.ToString("N2"),                                 // Quantity
                   _ => _.Amount.ToString("N2"),                    // Unit Price                   
                   _ => _.Discount.ToString("N2"),                  // Line Discount Amount
                   _ => _.NetAmount.ToString("N2")                  // Nett Price [(Quantity*Unit Price)-Line Discount Amount]
                 

               );
            });
        }

        public FileDto ExportPOSSVToFile(IList<TransactionDto> data)
        {
            return CreateCsvPackage("POSSV", generator => {
               // generator.AddObjects(data, _ => _.TransactionId);
            });
        }
    }
}
