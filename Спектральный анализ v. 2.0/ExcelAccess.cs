using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Спектральный_анализ_v._2._0
{
    public class AccessExcel
    {
        private ExcelPackage _appExcel;
        private ExcelWorksheet _xlsSheet;
        private DataTable _dataTable = new DataTable();
        public void DoAccess(string path)
        {
            try
            {
                _appExcel = new ExcelPackage(new FileInfo(path));
                while (_appExcel.Workbook.Worksheets.Count == 0)
                    _appExcel.Workbook.Worksheets.Add("Лист 1");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Clear()
        {
            _dataTable?.Clear();
        }

        public void FinishAccess()
        {
            _appExcel.Save();
            _appExcel.Dispose();
        }

        public double GetValueCell(int columnIndex, int rowIndex)
        {
            return (double)_xlsSheet.Cells[columnIndex, rowIndex].Value;
        }

        public void WriteRow(int index, int[] row, int count)
        {
            _xlsSheet = _appExcel.Workbook.Worksheets[1];
            for (int i = 0; i < count; i++)
            {
                _xlsSheet.Cells[i + 1, index].Value = row[i];
            }
        }

        public void WriteCell(int iRow, int iColumn, double data)
        {
            _xlsSheet = _appExcel.Workbook.Worksheets[1];
            _xlsSheet.Cells[iRow, iColumn].Value = data;
        }
        public void WriteCell(int iRow, int iColumn, string data)
        {
            _xlsSheet = _appExcel.Workbook.Worksheets[1];
            _xlsSheet.Cells[iRow, iColumn].Value = data;
        }
        public void SetColorCell(int iRow, int iColumn, int R, int G, int B)
        {
            _xlsSheet = _appExcel.Workbook.Worksheets[1];
            _xlsSheet.Cells[iRow, iColumn].Style.Fill.PatternType = ExcelFillStyle.Solid;
            _xlsSheet.Cells[iRow, iColumn].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, R, G, B));

        }
    }
}
