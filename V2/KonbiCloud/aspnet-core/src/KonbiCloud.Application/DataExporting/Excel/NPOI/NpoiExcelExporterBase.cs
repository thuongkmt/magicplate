using System;
using System.Collections.Generic;
using System.IO;
using Abp.AspNetZeroCore.Net;
using Abp.Collections.Extensions;
using Abp.Dependency;
using KonbiCloud.Dto;
using KonbiCloud.Storage;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace KonbiCloud.DataExporting.Excel.NPOI
{
    public abstract class NpoiExcelExporterBase : KonbiCloudServiceBase, ITransientDependency
    {
        private readonly ITempFileCacheManager _tempFileCacheManager;
        private Dictionary<string, int> _definedCellStyles = new Dictionary<string, int>();

        protected NpoiExcelExporterBase(ITempFileCacheManager tempFileCacheManager)
        {
            _tempFileCacheManager = tempFileCacheManager;
        }

        protected FileDto CreateExcelPackage(string fileName, Action<XSSFWorkbook> creator)
        {
            var file = new FileDto(fileName, MimeTypeNames.ApplicationVndOpenxmlformatsOfficedocumentSpreadsheetmlSheet);
            var workbook = new XSSFWorkbook();
            
            creator(workbook);
            
            Save(workbook, file);
            
            return file;
        }

        protected void AddHeader(ISheet sheet, params string[] headerTexts)
        {
            if (headerTexts.IsNullOrEmpty())
            {
                return;
            }

            sheet.CreateRow(0);
            
            for (var i = 0; i < headerTexts.Length; i++)
            {
                AddHeader(sheet, i, headerTexts[i]);
            }
        }

        protected void AddHeader(ISheet sheet, int columnIndex, string headerText)
        {
            var cell = sheet.GetRow(0).CreateCell(columnIndex);
            cell.SetCellValue(headerText);
            var cellStyle = sheet.Workbook.CreateCellStyle();
            var font = sheet.Workbook.CreateFont();
            font.Boldweight = 12;
            font.FontHeightInPoints = 12;
            font.IsBold = true;
            cellStyle.SetFont(font);
            cell.CellStyle = cellStyle;
        }

        protected void AddObjects<T>(ISheet sheet, int startRowIndex, IList<T> items, params Func<T, object>[] propertySelectors)
        {
            if (items.IsNullOrEmpty() || propertySelectors.IsNullOrEmpty())
            {
                return;
            }

            for (var i = 1; i <= items.Count; i++)
            {
                var row = sheet.CreateRow(i);
                
                for (var j = 0; j < propertySelectors.Length; j++)
                {
                    var cell = row.CreateCell(j);
                    var value = propertySelectors[j](items[i - 1]);
                    if (value != null)
                    {

                        cell.SetCellValue(value.ToString());
                    }
                }
            }
        }

        protected void Save(XSSFWorkbook excelPackage, FileDto file)
        {
            using (var stream = new MemoryStream())
            {
                excelPackage.Write(stream);
                _tempFileCacheManager.SetFile(file.FileToken, stream.ToArray());
            }
        }
        
        protected void SetCellDataFormat(ICell cell, string dataFormat)
        {
            if (cell == null)
            {
                return;
            }
            ICellStyle cellStyle = GetCellStyleFromFormat(cell,dataFormat);
            if(cellStyle != null)
            {
                cell.CellStyle = cellStyle;

            }          
            if (double.TryParse(cell.StringCellValue, out var dou))
            {
                cell.SetCellValue(dou);
                return;
            }
            if (DateTime.TryParse(cell.StringCellValue, out var datetime))
            {
                cell.SetCellValue(datetime);
                return;
            }
           
        }

        private ICellStyle GetCellStyleFromFormat(ICell cell, string dataFormat)
        {
          
            if(_definedCellStyles.TryGetValue(dataFormat,out int cellStyleIndex))
            {
                return cell.Sheet.Workbook.GetCellStyleAt(cellStyleIndex);
            }
            else
            {
                var cellStyle = cell.Sheet.Workbook.CreateCellStyle();
                var format = cell.Sheet.Workbook.CreateDataFormat();
                cellStyle.DataFormat = format.GetFormat(dataFormat);               
             
                _definedCellStyles.Add(dataFormat, cellStyle.Index);
                return cellStyle;
            }
           
        }
    }
}