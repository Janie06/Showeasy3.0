using System;
using Microsoft.Office.Interop.Excel;

namespace EasyBL
{
    /// <SUMMARY>
    /// Microsoft.Office.Interop.ExcelEdit 的摘要说明
    /// </SUMMARY>
    public class ExcelEdit
    {
        public string mFilename;
        public Application app;
        public Workbooks wbs;
        public Workbook wb;
        public Worksheets wss;
        public Worksheet ws;

        public ExcelEdit()
        {
            // TODO: 在此处添加构造函数逻辑
        }

        public void Create()//创建一个Microsoft.Office.Interop.Excel对象
        {
            app = new Application();
            wbs = app.Workbooks;
            wb = wbs.Add(true);
        }

        public void Open(string FileName)//打开一个Microsoft.Office.Interop.Excel文件
        {
            app = new Application();
            wbs = app.Workbooks;
            wb = wbs.Open(FileName);
            //wb = wbs.Open(FileName, 0, true, 5,"", "", true, XlPlatform.xlWindows, "t", false, false, 0, true,Type.Missing,Type.Missing);
            //wb = wbs.Open(FileName,Type.Missing,Type.Missing,Type.Missing,Type.Missing,Type.Missing,Type.Missing,XlPlatform.xlWindows,Type.Missing,Type.Missing,Type.Missing,Type.Missing,Type.Missing,Type.Missing,Type.Missing);
            mFilename = FileName;
        }

        public Worksheet GetSheet(string SheetName)
        //获取一个工作表
        {
            var s = (Worksheet)wb.Worksheets[SheetName];
            return s;
        }

        public Worksheet AddSheet(string SheetName)
        //添加一个工作表
        {
            var s = (Worksheet)wb.Worksheets.Add(Type.Missing, Type.Missing, Type.Missing, Type.Missing);
            s.Name = SheetName;
            return s;
        }

        public void DelSheet(string SheetName)//删除一个工作表
        {
            ((Worksheet)wb.Worksheets[SheetName]).Delete();
        }

        public Worksheet ReNameSheet(string OldSheetName, string NewSheetName)//重命名一个工作表一
        {
            var s = (Worksheet)wb.Worksheets[OldSheetName];
            s.Name = NewSheetName;
            return s;
        }

        public Worksheet ReNameSheet(Worksheet Sheet, string NewSheetName)//重命名一个工作表二
        {
            Sheet.Name = NewSheetName;

            return Sheet;
        }

        public void SetCellValue(Worksheet ws, int x, int y, object value)
        //ws：要设值的工作表     X行Y列     value   值
        {
            ws.Cells[x, y] = value;
        }

        public void SetCellValue(string ws, int x, int y, object value)
        //ws：要设值的工作表的名称 X行Y列 value 值
        {
            GetSheet(ws).Cells[x, y] = value;
        }

        public void SetCellProperty(Worksheet ws, int Startx, int Starty, int Endx, int Endy, int size, string name, Constants color, Constants HorizontalAlignment)
        //设置一个单元格的属性   字体，   大小，颜色   ，对齐方式
        {
            name = "宋体";
            size = 12;
            color = Constants.xlAutomatic;
            HorizontalAlignment = Constants.xlRight;
            ws.get_Range(ws.Cells[Startx, Starty], ws.Cells[Endx, Endy]).Font.Name = name;
            ws.get_Range(ws.Cells[Startx, Starty], ws.Cells[Endx, Endy]).Font.Size = size;
            ws.get_Range(ws.Cells[Startx, Starty], ws.Cells[Endx, Endy]).Font.Color = color;
            ws.get_Range(ws.Cells[Startx, Starty], ws.Cells[Endx, Endy]).HorizontalAlignment = HorizontalAlignment;
        }

        public void SetCellProperty(string wsn, int Startx, int Starty, int Endx, int Endy, int size, string name, Constants color, Constants HorizontalAlignment)
        {
            //name = "宋体";
            //size = 12;
            //color = Constants.xlAutomatic;
            //HorizontalAlignment = Constants.xlRight;

            var ws = GetSheet(wsn);
            ws.get_Range(ws.Cells[Startx, Starty], ws.Cells[Endx, Endy]).Font.Name = name;
            ws.get_Range(ws.Cells[Startx, Starty], ws.Cells[Endx, Endy]).Font.Size = size;
            ws.get_Range(ws.Cells[Startx, Starty], ws.Cells[Endx, Endy]).Font.Color = color;

            ws.get_Range(ws.Cells[Startx, Starty], ws.Cells[Endx, Endy]).HorizontalAlignment = HorizontalAlignment;
        }

        public void UniteCells(Worksheet ws, int x1, int y1, int x2, int y2)
        //合并单元格
        {
            ws.get_Range(ws.Cells[x1, y1], ws.Cells[x2, y2]).Merge(Type.Missing);
        }

        public void UniteCells(string ws, int x1, int y1, int x2, int y2)
        //合并单元格
        {
            GetSheet(ws).get_Range(GetSheet(ws).Cells[x1, y1], GetSheet(ws).Cells[x2, y2]).Merge(Type.Missing);
        }

        public void InsertTable(System.Data.DataTable dt, string ws, int startX, int startY)
        //将内存中数据表格插入到Microsoft.Office.Interop.Excel指定工作表的指定位置 为在使用模板时控制格式时使用一
        {
            for (int i = 0; i <= dt.Rows.Count - 1; i++)
            {
                for (int j = 0; j <= dt.Columns.Count - 1; j++)
                {
                    GetSheet(ws).Cells[startX + i, j + startY] = dt.Rows[i][j].ToString();
                }
            }
        }

        public void InsertTable(System.Data.DataTable dt, Worksheet ws, int startX, int startY)
        //将内存中数据表格插入到Microsoft.Office.Interop.Excel指定工作表的指定位置二
        {
            for (int i = 0; i <= dt.Rows.Count - 1; i++)
            {
                for (int j = 0; j <= dt.Columns.Count - 1; j++)
                {
                    ws.Cells[startX + i, j + startY] = dt.Rows[i][j];
                }
            }
        }

        public void AddTable(System.Data.DataTable dt, string ws, int startX, int startY)
        //将内存中数据表格添加到Microsoft.Office.Interop.Excel指定工作表的指定位置一
        {
            for (int i = 0; i <= dt.Rows.Count - 1; i++)
            {
                for (int j = 0; j <= dt.Columns.Count - 1; j++)
                {
                    GetSheet(ws).Cells[i + startX, j + startY] = dt.Rows[i][j];
                }
            }
        }

        public void AddTable(System.Data.DataTable dt, Worksheet ws, int startX, int startY)
        //将内存中数据表格添加到Microsoft.Office.Interop.Excel指定工作表的指定位置二
        {
            for (int i = 0; i <= dt.Rows.Count - 1; i++)
            {
                for (int j = 0; j <= dt.Columns.Count - 1; j++)
                {
                    ws.Cells[i + startX, j + startY] = dt.Rows[i][j];
                }
            }
        }

        public void InsertPictures(string Filename, string ws)
        //插入图片操作一
        {
            //GetSheet(ws).Shapes.AddPicture(Filename, MsoTriState.msoFalse, MsoTriState.msoTrue, 10, 10, 150, 150);
            //后面的数字表示位置
        }

        //public void InsertPictures(string Filename, string ws, int Height, int Width)
        //插入图片操作二
        //{
        //    GetSheet(ws).Shapes.AddPicture(Filename, MsoTriState.msoFalse, MsoTriState.msoTrue, 10, 10, 150, 150);
        //    GetSheet(ws).Shapes.get_Range(Type.Missing).Height = Height;
        //    GetSheet(ws).Shapes.get_Range(Type.Missing).Width = Width;
        //}
        //public void InsertPictures(string Filename, string ws, int left, int top, int Height, int Width)
        //插入图片操作三
        //{
        //    GetSheet(ws).Shapes.AddPicture(Filename, MsoTriState.msoFalse, MsoTriState.msoTrue, 10, 10, 150, 150);
        //    GetSheet(ws).Shapes.get_Range(Type.Missing).IncrementLeft(left);
        //    GetSheet(ws).Shapes.get_Range(Type.Missing).IncrementTop(top);
        //    GetSheet(ws).Shapes.get_Range(Type.Missing).Height = Height;
        //    GetSheet(ws).Shapes.get_Range(Type.Missing).Width = Width;
        //}

        public void InsertActiveChart(XlChartType ChartType, string ws, int DataSourcesX1, int DataSourcesY1, int DataSourcesX2, int DataSourcesY2, XlRowCol ChartDataType)
        //插入图表操作
        {
            ChartDataType = XlRowCol.xlColumns;
            wb.Charts.Add(Type.Missing, Type.Missing, Type.Missing, Type.Missing);
            {
                wb.ActiveChart.ChartType = ChartType;
                wb.ActiveChart.SetSourceData(GetSheet(ws).get_Range(GetSheet(ws).Cells[DataSourcesX1, DataSourcesY1], GetSheet(ws).Cells[DataSourcesX2, DataSourcesY2]), ChartDataType);
                wb.ActiveChart.Location(XlChartLocation.xlLocationAsObject, ws);
            }
        }

        public bool Save()
        //保存文档
        {
            if (mFilename == "")
            {
                return false;
            }
            else
            {
                try
                {
                    wb.Save();
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        public bool SaveAs(object FileName)
        //文档另存为
        {
            try
            {
                wb.SaveAs(FileName, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, XlSaveAsAccessMode.xlExclusive, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool SaveAsPdf(object FileName)
        //文档另存为
        {
            try
            {
                wb.ExportAsFixedFormat(XlFixedFormatType.xlTypePDF, FileName);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public void Close()
        //关闭一个Microsoft.Office.Interop.Excel对象，销毁对象
        {
            //wb.Save();
            if (wb != null)
            {
                wb.Close(Type.Missing, Type.Missing, Type.Missing);
            }

            if (wbs != null)
            {
                wbs.Close();
            }

            if (app != null)
            {
                app.Quit();
            }

            wb = null;
            wbs = null;
            app = null;
            GC.Collect();
        }
    }
}