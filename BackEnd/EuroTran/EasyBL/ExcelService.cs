using Aspose.Cells;
using EasyBL.WebApi.Message;
using EasyNet.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;

namespace EasyBL
{
    public class ExcelService : ServiceBase
    {
        public Workbook workbook;
        public Worksheet sheet;

        public ExcelService(string file = null)
        {
            workbook = !string.IsNullOrEmpty(file) ? new Workbook(file) : new Workbook();
            sheet = workbook.Worksheets[0];
        }

        /// <summary>
        /// 產出excel
        /// </summary>
        /// <param name="i_dicData"></param>
        /// <param name="o_sFilePath"></param>
        /// <param name="dicItems"></param>
        /// <param name="sFileName"></param>
        /// <param name="sSheetName"></param>
        /// <returns></returns>
        public bool CreateExcel(List<Dictionary<string, object>> i_dicData, out string o_sFilePath, Dictionary<string, object> dicItems = null, string sFileName = "", string sSheetName = "sheet1")
        {
            try
            {
                var saItemsWidth = new Dictionary<string, object>();
                string sFilePath = null;
                string sFileFullName = null;
                var bStatus = false;
                do
                {
                    workbook.Worksheets[0].Name = sSheetName;

                    var style_L = GetStyle(0, false, TextAlignmentType.Left, Color.White, false);
                    var style_C = GetStyle(0, false, TextAlignmentType.Center, Color.White, true);
                    var style_Header = GetStyle(12, true, TextAlignmentType.Center, Color.FromArgb(153, 204, 0), false);

                    var cells = sheet.Cells;//单元格
                    var iCol = dicItems == null ? i_dicData[0].Keys.Count : dicItems.Keys.Count; ;//表格列数
                    var iRow = i_dicData.Count;//表格行数
                    var iCurentRow = 3;//當前行

                    //生成标题行
                    cells.Merge(0, 0, 1, iCol);//合并单元格
                    cells.Merge(1, 0, 1, iCol);//合并单元格
                    cells[0, 0].PutValue(sSheetName);//填写标题
                    cells[1, 0].PutValue("匯出時間:" + DateTime.Now.ToString("yyyy/MM/dd"));//匯出時間
                    SetCellsStyle(new int[] { 0, 0, 1, iCol }, 14, true, TextAlignmentType.Center, Color.White);//标题row樣式
                    SetCellsStyle(new int[] { 1, 0, 1, iCol }, 12, false, TextAlignmentType.Right, Color.White);//時間row樣式
                    cells.SetRowHeight(0, 25);
                    cells.SetRowHeight(1, 20);

                    //填充表头
                    if (dicItems != null)
                    {
                        var index = 0;
                        foreach (string sKey in dicItems.Keys)
                        {
                            var sValue = dicItems[sKey];
                            if (sValue == null)
                            {
                                sValue = "";
                            }
                            cells[2, index].PutValue(sValue);//表頭
                            cells[2, index].SetStyle(style_Header);//時間Header row樣式
                            //setCellsWisth(index, sValue.ToString(), true, saItemsWidth);
                            index++;
                        }
                    }
                    //填充内容
                    foreach (Dictionary<string, object> oData in i_dicData)
                    {
                        if (dicItems != null)
                        {
                            var cellIndex = 0;
                            foreach (string sKey in dicItems.Keys)
                            {
                                if (oData.Keys.Contains(sKey))
                                {
                                    var sValue = oData[sKey];
                                    if (sValue == null)
                                    {
                                        sValue = "";
                                    }
                                    cells[iCurentRow, cellIndex].PutValue(sValue);//表頭
                                    if (cellIndex == 0)
                                    {
                                        cells[iCurentRow, cellIndex].SetStyle(style_C);//項次居中
                                    }
                                    else
                                    {
                                        cells[iCurentRow, cellIndex].SetStyle(style_L);//內容居左
                                    }
                                    cellIndex++;
                                }
                            }
                        }
                        else
                        {
                            var cellIndex = 0;
                            foreach (string sKey in oData.Keys)
                            {
                                var sValue = oData[sKey];
                                if (sValue == null)
                                {
                                    sValue = "";
                                }
                                cells[iCurentRow, cellIndex].PutValue(sValue);//表頭
                                if (cellIndex == 0)
                                {
                                    cells[iCurentRow, cellIndex].SetStyle(style_C);//項次居中
                                }
                                else
                                {
                                    cells[iCurentRow, cellIndex].SetStyle(style_L);//內容居左
                                }
                                //setCellsWisth(cellIndex, sValue.ToString(), true, saItemsWidth);
                                cellIndex++;
                            }
                        }
                        iCurentRow++;
                    }
                    //SetCellsStyle(new int[] { 3, 1, iCurentRow - 3, iCol - 1 }, 12, false, TextAlignmentType.Left, Color.White);
                    //foreach (string key in saItemsWidth.Keys)
                    //{
                    //    cells.SetColumnWidth(int.Parse(key), double.Parse(saItemsWidth[key].ToString()));
                    //}
                    //sheet.AutoFitRows();
                    cells.SetRowHeight(2, 30);
                    sheet.AutoFitColumns();
                    sFilePath = Common.ConfigGetValue("", "OutFilesPath");
                    sFileName = (sFileName ?? "") + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
                    var sSeverPath = AppDomain.CurrentDomain.BaseDirectory + sFilePath;
                    sFileFullName = sSeverPath + "/" + sFileName;
                    Common.FnCreateDir(sSeverPath);
                    //保存
                    workbook.Save(sFileFullName);
                    bStatus = true;
                }
                while (false);

                o_sFilePath = sFileFullName.Replace("\\", "/");
                return bStatus;
            }
            catch (Exception ex)
            {
                Logger.Error("E" + ex.Message, ex);
                o_sFilePath = "";
                return false;
            }
        }

        /// <summary>
        /// 產出excel
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="o_sFilePath"></param>
        /// <param name="oHeader"></param>
        /// <param name="dicAlain"></param>
        /// <param name="Merges"></param>
        /// <param name="sFileName"></param>
        /// <param name="sSheetName"></param>
        /// <returns></returns>
        public bool CreateExcelByTb(DataTable dt, out string o_sFilePath, object oHeader = null, Dictionary<string, string> dicAlain = null, List<Dictionary<string, int>> Merges = null, string sFileName = "", string sSheetName = "sheet1")
        {
            try
            {
                var saItemsWidth = new Dictionary<string, object>();
                Dictionary<string, string> Headers = null;
                string sFilePath = null;
                string sFileFullName = null;
                var bStatus = false;
                if (dicAlain == null)
                {
                    dicAlain = new Dictionary<string, string>();
                }
                workbook.Worksheets[0].Name = sSheetName;

                var cells = sheet.Cells;//单元格
                //为单元格添加样式
                var style = workbook.CreateStyle();
                //设置居中
                style.HorizontalAlignment = TextAlignmentType.Left;
                var style_C = GetStyle(0, false, TextAlignmentType.Center, Color.White, true);
                var style_L = GetStyle(0, false, TextAlignmentType.Left, Color.White, false);
                var style_R = GetStyle(0, false, TextAlignmentType.Right, Color.White, false);
                //靠右通常為數字，改成顯示數字，參考:https://apireference.aspose.com/net/cells/aspose.cells/style/properties/number
                style_R.Number = 4; //	#,##0.00
                var style_Del = GetStyle(0, false, TextAlignmentType.Left, Color.LightGray, true);
                var style_Header = GetStyle(12, true, TextAlignmentType.Center, Color.FromArgb(153, 204, 0), false);
                //靠右的設定
                var FormatredNumber = dicAlain.Where(c => c.Value == "right" ).Select( c => c.Key).ToList();

                //行索引
                var rowIndex = 3;
                //列总数
                var colCount = dt.Columns.Count;

                if (oHeader != null)
                {
                    if (oHeader.GetType() == typeof(Dictionary<string, string>))
                    {
                        Headers = oHeader as Dictionary<string, string>;
                        SetHeader(cells, Headers, style_Header, 2);
                    }
                    else
                    {
                        SetGroupHeader(cells, oHeader, out Headers);
                        SetHeader(cells, Headers, style_Header, 3);
                        rowIndex++;
                        cells.SetRowHeight(3, 30);//设置行高
                    }
                    colCount = Headers.Keys.Count;
                    cells.SetRowHeight(2, 30);//设置行高
                }
                else
                {
                    for (int i = 0; i < colCount; i++)
                    {
                        cells[2, i].PutValue(dt.Columns[i].ColumnName);
                        cells[2, i].SetStyle(style_Header);
                    }
                }

                //生成标题行
                cells.Merge(0, 0, 1, colCount);//合并单元格
                cells.Merge(1, 0, 1, colCount);//合并单元格
                cells[0, 0].PutValue(sFileName);//填写标题
                cells[1, 0].PutValue("匯出時間:" + DateTime.Now.ToString("yyyy/MM/dd"));//匯出時間
                SetCellsStyle(new int[] { 0, 0, 1, colCount }, 14, true, TextAlignmentType.Center, Color.White);//标题row樣式
                SetCellsStyle(new int[] { 1, 0, 1, colCount }, 12, false, TextAlignmentType.Right, Color.White);//時間row樣式
                cells.SetRowHeight(0, 25);
                cells.SetRowHeight(1, 20);

                foreach (DataRow row in dt.Rows)
                {
                    var bDeleteRow = false;
                    if (dt.Columns.Contains(BLWording.ISVOID) && row[BLWording.ISVOID].ToString() == "Y") { bDeleteRow = true; }
                    //填充資料
                    if (Headers != null)
                    {
                        var cellIndex = 0;
                        foreach (string sKey in Headers.Keys)
                        {
                            if (row[sKey] != null)
                            {
                                var sValue = row[sKey];
                                if (sValue == null)
                                {
                                    sValue = "";
                                }


                                //設定值
                                if (FormatredNumber.Contains(sKey) )
                                {
                                    var ConvertData = sValue.ToString();
                                    cells[rowIndex, cellIndex].PutValue(ConvertData, true);
                                }
                                else
                                {
                                    cells[rowIndex, cellIndex].PutValue(sValue);
                                }

                                //設定style:
                                if (dicAlain.Keys.Contains(sKey))
                                {
                                    var sVal = dicAlain[sKey].ToString();
                                    if (sVal == "center")
                                    {
                                        cells[rowIndex, cellIndex].SetStyle(style_C);//項次居中
                                    }
                                    else if (sVal == "right")
                                    {
                                        cells[rowIndex, cellIndex].SetStyle(style_R);//項次居右
                                    }
                                    else
                                    {
                                        cells[rowIndex, cellIndex].SetStyle(style_L);//項次居左
                                    }
                                }
                                else
                                {
                                    cells[rowIndex, cellIndex].SetStyle(style_L);//項次居左
                                }
                                if (bDeleteRow)
                                {
                                    cells[rowIndex, cellIndex].SetStyle(style_Del);//刪除的資料
                                }
                                cellIndex++;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < colCount; i++)
                        {
                            sheet.Cells[rowIndex, i].PutValue(row[i]);
                            if (i == 0)
                            {
                                cells[rowIndex, i].SetStyle(style_C);//項次居中
                            }
                            else
                            {
                                cells[rowIndex, i].SetStyle(style_L);//內容居左
                            }
                            if (bDeleteRow)
                            {
                                cells[rowIndex, i].SetStyle(style_Del);//刪除的資料
                            }
                        }
                    }
                    rowIndex++;
                }
                if (Merges != null)//合并单元格
                {
                    foreach (Dictionary<string, int> Merge in Merges)
                    {
                        cells.Merge(Merge["FirstRow"], Merge["FirstCol"], Merge["RowCount"], Merge["ColCount"]);
                    }
                }
                sheet.AutoFitColumns();
                sFilePath = Common.ConfigGetValue("", "OutFilesPath");
                sFileName = (sFileName ?? "") + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
                var sSeverPath = AppDomain.CurrentDomain.BaseDirectory + sFilePath;
                sFileFullName = sSeverPath + "/" + sFileName;
                Common.FnCreateDir(sSeverPath);
                workbook.Save(sFileFullName);
                bStatus = true;
                o_sFilePath = sFileFullName.Replace("\\", "/");
                return bStatus;
            }
            catch (Exception ex)
            {
                Logger.Error("导出文件时出错" + ex.Message, ex);
                //LogAndSendEmail(sMsg + "Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "ExcelService", "Excel匯出", "CopyLanguage（複製語系檔案）", "", "", "");
                o_sFilePath = "";
                return false;
            }
        }

        /// <summary>
        /// 產出excel
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="o_sFilePath"></param>
        /// <param name="dicItems"></param>
        /// <param name="dicAlain"></param>
        /// <param name="sFileName"></param>
        /// <param name="sSheetName"></param>
        /// <returns></returns>
        public bool CreateExcelByList<T>(IList<T> list, out string o_sFilePath, Dictionary<string, string> dicItems = null, Dictionary<string, string> dicAlain = null, string sFileName = "", string sSheetName = "sheet1") where T : new()
        {
            try
            {
                var properties = ReflectionHelper.GetProperties(new T().GetType());
                var saItemsWidth = new Dictionary<string, object>();
                string sFilePath = null;
                string sFileFullName = null;
                var bStatus = false;
                if (dicAlain == null)
                {
                    dicAlain = new Dictionary<string, string>();
                }

                workbook.Worksheets[0].Name = sSheetName;

                var cells = sheet.Cells;//单元格
                //为单元格添加样式
                var style = workbook.CreateStyle();
                var style_L = GetStyle(0, false, TextAlignmentType.Left, Color.White, false);
                var style_R = GetStyle(0, false, TextAlignmentType.Right, Color.White, true);
                var style_C = GetStyle(0, false, TextAlignmentType.Center, Color.White, true);
                var style_Header = GetStyle(12, true, TextAlignmentType.Center, Color.FromArgb(153, 204, 0), false);
                //设置居中
                style.HorizontalAlignment = TextAlignmentType.Left;
                //行索引
                var rowIndex = 3;
                //列总数
                var colCount = dicItems == null ? properties.Length : dicItems.Keys.Count;

                //生成标题行
                cells.Merge(0, 0, 1, colCount);//合并单元格
                cells.Merge(1, 0, 1, colCount);//合并单元格
                cells[0, 0].PutValue(sFileName);//填写标题
                cells[1, 0].PutValue("匯出時間:" + DateTime.Now.ToString("yyyy/MM/dd"));//匯出時間
                SetCellsStyle(new int[] { 0, 0, 1, colCount }, 14, true, TextAlignmentType.Center, Color.White);//标题row樣式
                SetCellsStyle(new int[] { 1, 0, 1, colCount }, 12, false, TextAlignmentType.Right, Color.White);//時間row樣式
                cells.SetRowHeight(0, 25);
                cells.SetRowHeight(1, 20);

                //Header名的处理
                if (dicItems != null)
                {
                    var index = 0;
                    foreach (string sKey in dicItems.Keys)
                    {
                        var sValue = dicItems[sKey];
                        if (sValue == null)
                        {
                            sValue = "";
                        }
                        cells[2, index].PutValue(sValue);//表頭
                        cells[2, index].SetStyle(style_Header);//時間Header row樣式
                        index++;
                    }
                    cells.SetRowHeight(2, 30);//设置行高
                }
                else
                {
                    rowIndex--;
                }

                foreach (T entiy in list)
                {
                    //填充資料
                    if (dicItems != null)
                    {
                        var cellIndex = 0;
                        foreach (string sKey in dicItems.Keys)
                        {
                            SetCells<T>(cells, entiy, sKey, dicAlain, style_C, style_L, style_R, rowIndex, ref cellIndex);
                        }
                    }
                    else
                    {
                        var icol = 0;
                        foreach (PropertyInfo property in properties)
                        {
                            SetCells<T>(cells, entiy, property.Name, dicAlain, style_C, style_L, style_R, rowIndex, ref icol);
                        }
                    }
                    rowIndex++;
                }

                sheet.AutoFitColumns();
                sFilePath = Common.ConfigGetValue("", "OutFilesPath");
                sFileName = (sFileName ?? "") + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
                var sSeverPath = AppDomain.CurrentDomain.BaseDirectory + sFilePath;
                sFileFullName = sSeverPath + "/" + sFileName;
                Common.FnCreateDir(sSeverPath);
                workbook.Save(sFileFullName);
                bStatus = true;
                o_sFilePath = sFileFullName.Replace("\\", "/");
                return bStatus;
            }
            catch (Exception ex)
            {
                Logger.Error("导出文件时出错" + ex.Message, ex);
                o_sFilePath = "";
                return false;
            }
        }

        public RequestMessage Excel(RequestMessage i_crmInput)
        {
            RequestMessage crm = null;
            return crm;
        }

        /// <summary>
        /// 設置單元格樣式
        /// </summary>
        /// <param name="saPosition"></param>
        /// <param name="sFontSize"></param>
        /// <param name="bIsBold"></param>
        /// <param name="sAlign"></param>
        /// <param name="sBgColor"></param>
        private void SetCellsStyle(int[] saPosition, int sFontSize, bool bIsBold, TextAlignmentType sAlign, Color sBgColor)
        {
            var style = workbook.CreateStyle();
            var cells = sheet.Cells;
            var range = cells.CreateRange(saPosition[0], saPosition[1], saPosition[2], saPosition[3]);

            style.Font.Name = "宋体";//字體設置
            style.HorizontalAlignment = sAlign;//文字居左/中/右 ---TextAlignmentType.Center
            //style.Font.Color = Color.Blue;
            style.Font.Size = sFontSize;//文字大小 ----12
            style.Font.IsBold = bIsBold;//粗体 ----false
            style.ForegroundColor = sBgColor;//背景顏色
            style.Pattern = BackgroundType.Solid;//设置背景類型
            style.IsTextWrapped = true;//单元格内容自动换行

            // 邊線設置
            style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
            style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
            style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
            style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;

            var flg = new StyleFlag() { All = true };
            range.ApplyStyle(style, flg);
        }

        /// <summary>
        /// 固定的樣式
        /// </summary>
        /// <param name="sFontSize"></param>
        /// <param name="bIsBold"></param>
        /// <param name="sAlign"></param>
        /// <param name="sBgColor"></param>
        /// <param name="bIsWrap"></param>
        /// <returns></returns>
        public Style GetStyle(int sFontSize, bool bIsBold, TextAlignmentType sAlign, Color sBgColor, bool bIsWrap)
        {
            var style = workbook.CreateStyle();
            style.HorizontalAlignment = sAlign;//文字居左/中/右 ---TextAlignmentType.Center
            //style.Font.Color = Color.Blue;
            if (sFontSize != 0)
            {
                style.Font.Size = sFontSize;//文字大小 ----12
            }
            style.Font.IsBold = bIsBold;//粗体 ----false
            style.ForegroundColor = sBgColor;//背景顏色
            style.Pattern = BackgroundType.Solid;//设置背景類型
            style.IsTextWrapped = bIsWrap;//单元格内容自动换行

            // 邊線設置
            style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
            style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
            style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
            style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
            return style;
        }

        /// <summary>
        /// </summary>
        /// <param name="sStart"></param>
        /// <param name="sEnd"></param>
        /// <param name="sColor"></param>
        private static void AddContainsStyle(string sStart, string sEnd, Color sColor)
        {
            //FormatConditionCollection conds = GetFormatCondition(sStart + ":" + sEnd, sColor);//LightSteelBlue
            //int idx = conds.AddCondition(FormatConditionType.CellValue);
            //FormatCondition cond = conds[idx];
            //cond.Style.Pattern = BackgroundType.Solid;
            //cond.Style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
            //cond.Style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
            //cond.Style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
            //cond.Style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
        }

        /// <summary>
        /// 設置分組header
        /// </summary>
        /// <param name="oCells"></param>
        /// <param name="oHeader"></param>
        /// <param name="o_oHeader"></param>
        private void SetGroupHeader(Cells oCells, object oHeader, out Dictionary<string, string> o_oHeader)
        {
            var dicHeader = new Dictionary<string, string>();
            var listHeader = oHeader as List<Dictionary<string, object>>;
            var iLastNO = 0;
            foreach (Dictionary<string, object> header in listHeader)
            {
                var _header = header["Header"] as Dictionary<string, string>;
                var sHeaderName = header["HeaderName"].ToString();
                var color = (Color)header["Color"];
                var style_Header = GetStyle(12, true, TextAlignmentType.Center, color, false);
                oCells.Merge(2, iLastNO, 1, _header.Keys.Count);//合并单元格
                oCells[2, iLastNO].PutValue(sHeaderName);//填写标题
                oCells[2, iLastNO].SetStyle(style_Header);//項次居中
                iLastNO += _header.Keys.Count;
                foreach (string key in _header.Keys)
                {
                    dicHeader.Add(key, _header[key]);
                }
            }

            o_oHeader = dicHeader;
        }

        /// <summary>
        /// 設置header
        /// </summary>
        /// <param name="oCells"></param>
        /// <param name="dicHeader"></param>
        /// <param name="style_Header"></param>
        /// <param name="iRow"></param>
        private static void SetHeader(Cells oCells, Dictionary<string, string> dicHeader, Style style_Header, int iRow)
        {
            var index = 0;
            foreach (string sKey in dicHeader.Keys)
            {
                object sValue = dicHeader[sKey];
                if (sValue == null)
                {
                    sValue = "";
                }
                oCells[iRow, index].PutValue(sValue);//表頭
                oCells[iRow, index].SetStyle(style_Header);//時間Header row樣式
                index++;
            }
        }

        /// <summary>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cells"></param>
        /// <param name="entiy"></param>
        /// <param name="name"></param>
        /// <param name="alains"></param>
        /// <param name="stylec"></param>
        /// <param name="stylel"></param>
        /// <param name="styler"></param>
        /// <param name="rowIndex"></param>
        /// <param name="index"></param>
        private static void SetCells<T>(Cells cells, T entiy, string name, Dictionary<string, string> alains, Style stylec, Style stylel, Style styler, int rowIndex, ref int index)
        {
            var sValue = ReflectionHelper.GetPropertyValue(entiy, name);
            var sType = ReflectionHelper.GetPropertyType(entiy, name);

            if (sValue == null)
            {
                sValue = "";
            }
            if (sType != null && sType.IndexOf("System.DateTime") > 0)
            {
                sValue = Convert.ToDateTime(sValue).ToString("yyyy/MM/dd HH:mm");
            }
            cells[rowIndex, index].PutValue(sValue);//表頭

            if (alains.Keys.Contains(name))
            {
                var sVal = alains[name].ToString();
                if (sVal == "center")
                {
                    cells[rowIndex, index].SetStyle(stylec);//項次居中
                }
                else if (sVal == "right")
                {
                    cells[rowIndex, index].SetStyle(styler);//項次居右
                }
                else
                {
                    cells[rowIndex, index].SetStyle(stylel);//項次居左
                }
            }
            else
            {
                cells[rowIndex, index].SetStyle(stylel);//項次居左
            }
            index++;
        }

        // This method adds formatted conditions.
        private FormatConditionCollection GetFormatCondition(string cellAreaName, Color color)
        {
            // Adds an empty conditional formattings
            var index = sheet.ConditionalFormattings.Add();
            // Get the formatted conditions
            var formatConditions = sheet.ConditionalFormattings[index];
            // Get the cell area calling the custom GetCellAreaByName method
            var area = GetCellAreaByName(cellAreaName);
            // Add the formatted conditions cell area.
            formatConditions.AddArea(area);
            // Call the custom FillCell method
            FillCell(cellAreaName, color);
            // Return the formatted conditions
            return formatConditions;
        }

        // This method specifies the cell shading color for the conditional formattings cellarea range.
        private void FillCell(string cellAreaName, Color color)
        {
            var area = GetCellAreaByName(cellAreaName);
            var k = 0;
            for (int i = area.StartColumn; i <= area.EndColumn; i++)
            {
                for (int j = area.StartRow; j <= area.EndRow; j++)
                {
                    var c = sheet.Cells[j, i];
                    if (!color.IsEmpty)
                    {
                        var s = c.GetStyle();
                        s.ForegroundColor = color;
                        s.Pattern = BackgroundType.Solid;
                        c.SetStyle(s);
                    }
                    // Set some random values to the cells in the cellarea range
                    var value = j + i + k;
                    c.PutValue(value);
                    k++;
                }
            }
        }

        // This method specifies the CellArea range (start row, start col, end row, end col etc.) For
        // the conditional formatting
        internal static CellArea GetCellAreaByName(string s)
        {
            var area = new CellArea();
            var strCellRange = s.Replace("$", "").Split(':');
            CellsHelper.CellNameToIndex(strCellRange[0], out area.StartRow, out int column);
            area.StartColumn = column;
            if (strCellRange.Length == 1)
            {
                area.EndRow = area.StartRow;
                area.EndColumn = area.StartColumn;
            }
            else
            {
                CellsHelper.CellNameToIndex(strCellRange[1], out area.EndRow, out column);
                area.EndColumn = column;
            }
            return area;
        }

        private static void SetCellsWisth(int iCol, string sText, Dictionary<string, object> o_saItemsWidth)
        {
            double iWidth_New = 0;
            var c = sText.ToCharArray();

            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] >= 0x4e00 && c[i] <= 0x9fbb)
                {
                    iWidth_New += 2.5;
                }
                else
                {
                    iWidth_New += 1.2;
                }
            }

            iWidth_New = iWidth_New > 50 ? 50 : iWidth_New;//最大給50

            if (o_saItemsWidth.Keys.Contains(iCol.ToString()))
            {
                foreach (string key in o_saItemsWidth.Keys)
                {
                    if (key == iCol.ToString() && iWidth_New > double.Parse(o_saItemsWidth[key].ToString()))
                    {
                        o_saItemsWidth[key] = iWidth_New;
                        break;
                    }
                }
            }
            else
            {
                o_saItemsWidth.Add(iCol.ToString(), iWidth_New);
            }
        }

        public static Dictionary<string, string> GetExportAlain(object oHeader, object AlainCenter = null, object AlainRight = null)
        {
            var dicAlain = new Dictionary<string, string>();
            AlainCenter = AlainCenter ?? "";
            AlainRight = AlainRight ?? "";
            if (oHeader.GetType() == typeof(Dictionary<string, string>))
            {
                var dicHeader = oHeader as Dictionary<string, string>;
                PushAlain(dicHeader, AlainCenter, AlainRight, ref dicAlain);
            }
            else
            {
                var listHeader = oHeader as List<Dictionary<string, object>>;

                foreach (Dictionary<string, object> group in listHeader)
                {
                    var dicCols = group["Header"] as Dictionary<string, string>;
                    PushAlain(dicCols, AlainCenter, AlainRight, ref dicAlain);
                }
            }
            return dicAlain;
        }

        private static void PushAlain(Dictionary<string, string> cols, object AlainCenter, object AlainRight, ref Dictionary<string, string> dicAlain)
        {
            var saAlainCenter = new string[] { };
            var sAlainCenter = string.Empty;
            var saAlainRight = new string[] { };
            var sAlainRight = string.Empty;
            if (typeof(string) == AlainCenter.GetType())
            {
                sAlainCenter = AlainCenter.ToString();
            }
            else
            {
                saAlainCenter = AlainCenter as string[];
            }
            if (typeof(string) == AlainRight.GetType())
            {
                sAlainRight = AlainRight.ToString();
            }
            else
            {
                saAlainRight = AlainRight as string[];
            }
            foreach (string key in cols.Keys)
            {
                if (key == "RowIndex")
                {
                    dicAlain.Add("RowIndex", "center");
                }
                else if (sAlainCenter.Contains(key) || saAlainCenter.Contains(key))
                {
                    dicAlain.Add(key, "center");
                }
                else
                {
                    if (sAlainRight.Contains(key) || saAlainRight.Contains(key))
                    {
                        dicAlain.Add(key, "right");
                    }
                    else
                    {
                        dicAlain.Add(key, "left");
                    }
                }
            }
        }
    }
}