using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using NPOI;
using System.Data;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;

namespace EasyBL
{
    public class CSVHelper
    {

        /// <summary>
        /// ReadCSV：問卷調查轉換CSV資料
        /// </summary>
        /// <param name="i_sPath"></param>
        /// <param name="o_sMsg"></param>
        /// <returns></returns>
        public static List<Dictionary<string, object>> ReadCSV(string i_sPath, out string o_sMsg)
        {
            o_sMsg = null;
            List<Dictionary<string, object>> lCSVData = new List<Dictionary<string, object>>();

            do
            {
                try
                {
                    if (string.IsNullOrEmpty(i_sPath))
                    {
                        o_sMsg = "FilePath Not Null";
                        break;
                    }

                    if (File.Exists(i_sPath) == false)
                    {
                        o_sMsg = "File Not Exists";
                        break;
                    }

                    List<string> lColumnName = new List<string>();  //欄位名稱

                    Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

                    using (FileStream fs = new FileStream(i_sPath, FileMode.Open, FileAccess.Read))
                    {

                        StreamReader sr = new StreamReader(fs, Encoding.UTF8);
                        string sLine = string.Empty;
                        bool blHeaderRow = true;

                        while ((sLine = sr.ReadLine()) != null)
                        {
                            if (blHeaderRow)
                            {
                                //第一列為標題列
                                List<string> lTempHeaderRow = CSVParser.Split(sLine).ToList();
                                Dictionary<string, int> dicRepeatCount = new Dictionary<string, int>();
                                foreach (string HeaderColumn in lTempHeaderRow)
                                {
                                    int iRepeatCount = 0;

                                    string sRpcHeaderColumn = HeaderColumn.Replace("\"", "");

                                    if (lColumnName.Contains(sRpcHeaderColumn))
                                    {
                                        if (dicRepeatCount.Keys.Contains(sRpcHeaderColumn) == false)
                                        {
                                            dicRepeatCount.Add(sRpcHeaderColumn, iRepeatCount);
                                        }
                                        else
                                        {
                                            iRepeatCount = dicRepeatCount[sRpcHeaderColumn];
                                        }

                                        iRepeatCount = iRepeatCount + 1;

                                        lColumnName.Add(sRpcHeaderColumn + iRepeatCount.ToString());

                                        dicRepeatCount[sRpcHeaderColumn] = iRepeatCount;
                                    }
                                    else
                                    {
                                        lColumnName.Add(sRpcHeaderColumn);
                                    }
                                }

                                blHeaderRow = false;
                            }
                            else
                            {
                                Dictionary<string, object> dicRowData = new Dictionary<string, object>();
                                List<string> lRowData = CSVParser.Split(sLine).ToList();            //分割資料列

                                for (int iColIdx = 0; iColIdx < lColumnName.Count; iColIdx++)
                                {
                                    dicRowData.Add(lColumnName[iColIdx], lRowData[iColIdx]);
                                }

                                lCSVData.Add(dicRowData);
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    o_sMsg = ex.Message;
                }
            } while (false);

            return lCSVData;
        }

        /// <summary>
        /// CRM_CustomerDataConvert：CRM客戶資料轉換
        /// </summary>
        /// <param name="i_sPath"></param>
        /// <param name="o_sMsg"></param>
        /// <returns></returns>
        public static DataTable CRM_CustomerDataConvert(string i_sPath, out string o_sMsg)
        {
            o_sMsg = null;
            FileStream fs = null;
            DataTable dt = new DataTable();

            IWorkbook wb = null;
            using (fs = File.Open(i_sPath, FileMode.Open, FileAccess.Read))
            {
                switch (Path.GetExtension(i_sPath).ToUpper())
                {
                    case ".XLS":
                        {
                            wb = new HSSFWorkbook(fs);
                        }
                        break;
                    case ".XLSX":
                        {
                            wb = new XSSFWorkbook(fs);
                        }
                        break;
                }
                if (wb.NumberOfSheets > 0)
                {
                    ISheet sheet = wb.GetSheetAt(0);
                    if (sheet == null)
                    {
                        sheet = wb.GetSheetAt(0);
                    }
                    IRow headerRow = sheet.GetRow(0);

                    int index = 0;
                    //處理標題列
                    for (int i = headerRow.FirstCellNum; i < headerRow.LastCellNum; i++)
                    {
                        dt.Columns.Add(headerRow.GetCell(i).StringCellValue.Trim());

                    }
                    IRow row = null;
                    DataRow dr = null;
                    CellType ct = CellType.Blank;
                    //標題列之後的資料
                    for (int i = sheet.FirstRowNum + 1; i <= sheet.LastRowNum; i++)
                    {
                        dr = dt.NewRow();
                        row = sheet.GetRow(i);
                        if (row.Cells.All(d => d.CellType == CellType.Blank)) continue;
                        if (row == null) continue;
                        for (int j = row.FirstCellNum; j < row.LastCellNum; j++)
                        {
                            if (row.GetCell(j) == null)
                            {
                                dr[j] = DBNull.Value;
                            }
                            else
                            {
                                ct = row.GetCell(j).CellType;
                                //如果此欄位格式為公式 則去取得CachedFormulaResultType
                                switch (ct)
                                {
                                    case CellType.Formula:
                                        ct = row.GetCell(j).CachedFormulaResultType;
                                        break;
                                    case CellType.Numeric:
                                        var da = DateUtil.IsCellDateFormatted(row.GetCell(j)) ? dr[j] = row.GetCell(j).DateCellValue : dr[j] = row.GetCell(j).NumericCellValue;
                                        dr[j] = da;
                                        break;
                                    case CellType.String:
                                        dr[j] = row.GetCell(j).ToString();
                                        break;
                                    default:
                                        dr[j] = row.GetCell(j).ToString().Replace("$", "");
                                        break;
                                }
                            }
                        }
                        dt.Rows.Add(dr);
                    }
                }

            }

            return dt;
        }
    }
}
