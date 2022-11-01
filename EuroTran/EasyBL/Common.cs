using Entity.Sugar;
using Newtonsoft.Json.Linq;
using SqlSugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Packaging;
using System.Linq;

namespace EasyBL
{
    /// <summary>
    /// Common 的摘要描述
    /// </summary>
    public class Common
    {
        #region GetAppSettings

        /// <summary>
        /// 獲取WebService的配置信息
        /// </summary>
        /// <param name="sKey">todo: describe sKey parameter on GetAppSettings</param>
        /// <example></example>
        /// <returns>appSettings中配置的value值</returns>
        public static string GetAppSettings(string sKey)
        {
            var sVal = ConfigurationManager.AppSettings[sKey];
            return sVal ?? @"";
        }

        #endregion GetAppSettings

        #region UpdateAppSettings

        /// <summary>
        /// 修改config配置
        /// </summary>
        /// <param name="sKey">todo: describe sKey parameter on UpdateAppSettings</param>
        /// <param name="sValue">todo: describe sValue parameter on UpdateAppSettings</param>
        /// <returns></returns>
        public static bool UpdateAppSettings(string sKey, string sValue)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (!config.HasFile)
            {
                throw new ArgumentException(@"程序配置文件缺失！");
            }
            var _key = config.AppSettings.Settings[sKey];
            if (_key == null)
                config.AppSettings.Settings.Add(sKey, sValue);
            else
                config.AppSettings.Settings[sKey].Value = sValue;
            config.Save(ConfigurationSaveMode.Modified);
            return true;
        }

        #endregion UpdateAppSettings

        #region ConfigSetValue

        /// <summary>
        /// 写操作
        /// </summary>
        /// <param name="sExecutablePath">todo: describe sExecutablePath parameter on ConfigSetValue</param>
        /// <param name="sKey">todo: describe sKey parameter on ConfigSetValue</param>
        /// <param name="sValue">todo: describe sValue parameter on ConfigSetValue</param>
        public static void ConfigSetValue(string sExecutablePath, string sKey, string sValue)
        {
            if (!Directory.Exists(sExecutablePath))
            {
                sExecutablePath = System.Windows.Forms.Application.StartupPath.ToString();
            }
            var xDoc = new XmlDocument();
            //获取可执行文件的路径和名称
            xDoc.Load(sExecutablePath + @".config");

            XmlNode xNode;
            XmlElement xElem1;
            XmlElement xElem2;
            xNode = xDoc.SelectSingleNode(@"//connectionStrings");
            // xDoc.Load(System.Windows.Forms.Application.ExecutablePath + ".config");
            xElem1 = (XmlElement)xNode.SelectSingleNode(@"//add[@name='" + sKey + @"']");
            if (xElem1 != null) xElem1.SetAttribute(@"connectionString", sValue);
            else
            {
                xElem2 = xDoc.CreateElement(@"add");
                xElem2.SetAttribute(@"name", sKey);
                xElem2.SetAttribute(@"connectionString", sValue);
                xNode.AppendChild(xElem2);
            }
            xDoc.Save(sExecutablePath + @"Euro.Transfer.exe.config");
        }

        #endregion ConfigSetValue

        #region ConfigGetValue

        /// <summary>
        /// 读操作
        /// </summary>
        /// <param name="sExecutablePath">todo: describe sExecutablePath parameter on ConfigGetValue</param>
        /// <param name="sKey">todo: describe sKey parameter on ConfigGetValue</param>
        /// <returns></returns>
        public static string ConfigGetValue(string sExecutablePath, string sKey)
        {
            if (!Directory.Exists(sExecutablePath))
            {
                sExecutablePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"");
            }
            var xDoc = new XmlDocument();
            try
            {
                xDoc.Load(sExecutablePath + @"Web.config");

                XmlNode xNode;
                XmlElement xElem;
                xNode = xDoc.SelectSingleNode(@"//appSettings");
                xElem = (XmlElement)xNode.SelectSingleNode(@"//add[@key='" + sKey + @"']");
                if (xElem != null)
                    return xElem.GetAttribute(@"value");
                else
                    return @"";
            }
            catch (Exception)
            {
                return @"";
            }
        }

        #endregion ConfigGetValue

        #region FnDataSetToDataTable

        /// <summary>
        /// 返回DataSet中第一個DataTable
        /// </summary>
        /// <param name="ds">需要轉換的DataSet</param>
        /// <returns></returns>
        public static DataTable FnDataSetToDataTable(DataSet ds)
        {
            var dt = new DataTable();
            if (ds != null && ds.Tables.Count > 0)
            {
                dt = ds.Tables[0];
            }
            return dt;
        }

        #endregion FnDataSetToDataTable

        #region FnDataSetToDataTable

        /// <summary>
        /// 返回DataSet中第一個DataTable
        /// </summary>
        /// <param name="ds">需要轉換的DataSet</param>
        /// <param name="sTableName">todo: describe sTableName parameter on FnDataSetToDataTable</param>
        /// <returns></returns>
        public static DataTable FnDataSetToDataTable(DataSet ds, string sTableName)
        {
            var dt = new DataTable();
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[sTableName] != null)
            {
                dt = ds.Tables[sTableName];
            }
            return dt;
        }

        #endregion FnDataSetToDataTable

        #region FnDateDiffDay

        /// <summary>
        /// 取得時間差(天),超過y
        /// </summary>
        /// <param name="rDateTime1">todo: describe rDateTime1 parameter on FnDateDiffDays</param>
        /// <param name="rDateTime2">todo: describe rDateTime2 parameter on FnDateDiffDays</param>
        /// <returns></returns>
        public static int FnDateDiffDays(DateTime rDateTime1, DateTime rDateTime2)
        {
            var dateDiffDays = 0;
            var ts1 = new TimeSpan(Convert.ToDateTime(rDateTime1.ToString(@"yyyy/MM/dd")).Ticks);
            var ts2 = new TimeSpan(Convert.ToDateTime(rDateTime2.ToString(@"yyyy/MM/dd")).Ticks);
            var ts = ts1.Subtract(ts2).Duration();
            dateDiffDays = ts.Days + 1;
            return dateDiffDays;
        }

        #endregion FnDateDiffDay

        #region FnDateDiffHours

        /// <summary>
        /// 取得時間差(小時),超過y
        /// </summary>
        /// <param name="rDateTime1">todo: describe rDateTime1 parameter on FnDateDiffHours</param>
        /// <param name="rDateTime2">todo: describe rDateTime2 parameter on FnDateDiffHours</param>
        /// <returns></returns>
        public static int FnDateDiffHours(DateTime rDateTime1, DateTime rDateTime2)
        {
            var dateDiffHours = 0;
            var ts1 = new TimeSpan(rDateTime1.Ticks);
            var ts2 = new TimeSpan(rDateTime2.Ticks);
            var ts = ts1.Subtract(ts2).Duration();
            dateDiffHours = ts.Hours;
            return dateDiffHours;
        }

        #endregion FnDateDiffHours

        #region DateToTw

        /// <summary>
        /// 西元年轉民國年
        /// </summary>
        /// <param name="sYDate">todo: describe sYDate parameter on DateToTw</param>
        /// <returns>民國年</returns>
        public static string DateToTw(string sYDate)
        {
            var strDate = @"";
            string[] aryTmp;
            try
            {
                //檢查民國或民國前 (民國元年1912、民國前1年1911)
                var oDate = DateTime.Parse(sYDate);

                var blnPN = ((oDate.Year - 1911) > 0 ? true : false);
                if (blnPN)
                {
                    //民國
                    var twCultureInfo = new CultureInfo(@"zh-TW");
                    twCultureInfo.DateTimeFormat.Calendar = new TaiwanCalendar();

                    strDate = DateTime.Parse(sYDate).Date.ToString(@"d", twCultureInfo);

                    if (oDate.Year < 1921)
                    {
                        aryTmp = SplitString(strDate, @"/");
                        strDate = $@"{int.Parse(aryTmp[0])}{oDate:M/d}";
                    }
                }
                else
                {
                    //民國前
                    strDate = $@"{(oDate.Year - 1911 - 1)}/{oDate:M/d}";
                }

                return strDate;
            }
            catch (Exception)
            {
                return @"";
            }
        }

        #endregion DateToTw

        #region DateToYYYY

        /// <summary>
        /// 民國年轉西元年
        /// </summary>
        /// <param name="sYDate">todo: describe sYDate parameter on DateToYYYY</param>
        /// <returns>西元年</returns>
        public static string DateToYYYY(string sYDate)
        {
            var strDate = @"";
            try
            {
                //檢查民國或民國前 (民國元年1912、民國前1年1911)
                var strPN = sYDate.Substring(0, 1).Trim();
                switch (strPN)
                {
                    case @"-":
                        //民國前
                        var aryTmp = SplitString(sYDate, @"/");
                        strDate = $@"{(1911 + int.Parse(aryTmp[0]) + 1)}/{aryTmp[1]}/{aryTmp[2]}";
                        break;

                    default:
                        if (strPN == @"+" || strPN == @"-")
                            sYDate = sYDate.Substring(1);

                        //民國
                        var twCultureInfo = new CultureInfo(@"zh-TW");
                        twCultureInfo.DateTimeFormat.Calendar = new TaiwanCalendar();

                        //民國年2位轉3位----------------------------add by bruce 20140715
                        var tw_ary = sYDate.Split('/');
                        if (tw_ary.Length > 0)
                        {
                            var tw_yyy = tw_ary[0];
                            if (tw_yyy.Length == 2) sYDate = @"0" + sYDate;
                        }
                        //民國年2位轉3位----------------------------add by bruce 20140715

                        strDate = DateTime.Parse(sYDate, twCultureInfo).Date.ToString(@"d");
                        break;
                }

                return strDate;
            }
            catch (Exception)
            {
                return @"";
            }
        }

        public static string[] SplitString(string sStr, string delimiter)
        {
            string[] split = null;
            var delimit = delimiter.ToCharArray();
            split = sStr.Split(delimit);
            return split;
        }

        #endregion DateToYYYY

        #region GetSystemSetting

        /// <summary>
        /// 獲取系統設定
        /// </summary>
        /// <param name="db">todo: describe db parameter on GetSystemSetting</param>
        /// <param name="sOrgID">todo: describe sOrgID parameter on GetSystemSetting</param>
        /// <param name="sItemID">todo: describe sItemID parameter on GetSystemSetting</param>
        /// <returns></returns>
        public static string GetSystemSetting(SqlSugarClient db, string sOrgID, string sItemID)
        {
            var sSettingValue = string.Empty;
            if (!string.IsNullOrWhiteSpace(sOrgID) && !string.IsNullOrWhiteSpace(sItemID))
            {
                try
                {
                    var oSet = db.Queryable<OTB_SYS_SystemSetting>().Single(it => it.OrgID == sOrgID && it.SettingItem == sItemID);
                    if (oSet != null)
                    {
                        sSettingValue = oSet.SettingValue;
                    }
                }
                catch (Exception ex)
                {
                    LogService.MailSend(ex.Message + @" sOrgID：" + sOrgID + @";sItemID：" + sItemID, ex, @"", @"", @"EasyBL.Common.GetSystemSetting", nameof(Common), @"GetSystemSetting（獲取系統設定）", @"", @"", @"");
                }
            }
            return sSettingValue;
        }

        #endregion GetSystemSetting

        #region GetSystemSettings

        /// <summary>
        /// 獲取系統設定（多值）
        /// </summary>
        /// <param name="sOrgID">todo: describe sOrgID parameter on GetSystemSettings</param>
        /// <param name="skeys">todo: describe skeys parameter on GetSystemSettings</param>
        /// <returns></returns>
        public static EasyNet.Common.Map GetSystemSettings(string sOrgID, string[] skeys)
        {
            var mSettingItem = new EasyNet.Common.Map();
            if (!string.IsNullOrWhiteSpace(sOrgID) && skeys.Length > 0)
            {
                try
                {
                    var db = SugarBase.GetIntance();
                    var list = db.Queryable<OTB_SYS_SystemSetting>().Where(it => it.OrgID == sOrgID && it.Effective == @"Y" && SqlFunc.ContainsArray(skeys, it.SettingItem)).ToList();
                    foreach (OTB_SYS_SystemSetting set in list)
                    {
                        mSettingItem.Put(set.SettingItem, set.SettingValue);
                    }
                }
                catch (Exception ex)
                {
                    LogService.MailSend(ex.Message + @" sOrgID：" + sOrgID + @";sItemID：" + skeys, ex, @"", @"", @"Common.GetSystemSettings", nameof(Common), @"GetSystemSettings（獲取系統設定（多值））", @"", @"", @"");
                }
            }
            return mSettingItem;
        }

        #endregion GetSystemSettings

        #region ToDataTable

        /// <summary>
        /// DataRow轉換table
        /// </summary>
        /// <param name="dr">todo: describe dr parameter on ToDataTable</param>
        public static DataTable ToDataTable(DataRow[] dr)
        {
            if (dr == null || dr.Length == 0) return null;
            var tmp = dr[0].Table.Clone();  // 复制DataRow的表结构
            foreach (DataRow row in dr)
                tmp.Rows.Add(row.ItemArray);  // 将DataRow添加到DataTable中
            return tmp;
        }

        #endregion ToDataTable

        #region FnCopyFile

        /// <summary>
        /// 複製文件
        /// </summary>
        /// <param name="sFromFullPath">todo: describe sFromFullPath parameter on FnCopyFile</param>
        /// <param name="sToFullPath">todo: describe sToFullPath parameter on FnCopyFile</param>
        public static void FnCopyFile(string sFromFullPath, string sToFullPath)
        {
            //strFromFullPath = Server.MapPath(strFromFullPath);
            if (File.Exists(sFromFullPath))
            {
                if (File.Exists(sToFullPath))
                {
                    sToFullPath = sToFullPath.Insert(sToFullPath.LastIndexOf(@".") - 1, @"(1)");
                    File.Copy(sFromFullPath, sToFullPath, false);
                }
                else
                {
                    File.Copy(sFromFullPath, sToFullPath);
                }
            }
        }

        #endregion FnCopyFile

        #region FnCreateDir

        /// <summary>
        /// 創建目錄
        /// </summary>
        /// <param name="sPath">todo: describe sPath parameter on FnCreateDir</param>
        public static void FnCreateDir(string sPath)
        {
            if (!Directory.Exists(sPath))
            {
                Directory.CreateDirectory(sPath);
            }
        }

        #endregion FnCreateDir

        #region FnDeleteDir

        /// <summary>
        /// 删除文件夹
        /// </summary>
        /// <param name="sPath">todo: describe sPath parameter on FnDeleteDir</param>
        public static void FnDeleteDir(string sPath)
        {
            if (Directory.Exists(sPath))
            {
                Directory.Delete(sPath);
            }
        }

        #endregion FnDeleteDir

        #region FnDeletePathFiles

        /// <summary>
        /// 删除文件夹內文件
        /// </summary>
        /// <param name="sPath">todo: describe sPath parameter on FnDeletePathFiles</param>
        public static void FnDeletePathFiles(string sPath)
        {
            foreach (string file in Directory.GetFiles(sPath))
            {
                File.Delete(file);
            }
        }

        #endregion FnDeletePathFiles

        #region FnToTWDate

        /// <summary>
        ///日期轉西元轉民國
        /// </summary>
        /// <param name="date">todo: describe date parameter on FnToTWDate</param>
        public static string FnToTWDate(object date)
        {
            if (date == null || date.ToString() == @"") return @"";
            var dNew = Convert.ToDateTime(date);
            var twC = new TaiwanCalendar();
            return twC.GetYear(dNew) + dNew.ToString(@"MMdd");
        }

        //*************************************************

        #endregion FnToTWDate

        #region GoogleTranslate

        public static string GoogleTranslate(string sourceWord, string fromLanguage, string toLanguage)
        {
            /*
            调用： http://ajax.googleapis.com/ajax/services/language/translate?v=1.0&langpair=zh-CN|en&q=中国人是好人
            返回的json格式如下：
            {"responseData": {"translatedText":"Chinese people are good people"}, "responseDetails": null, "responseStatus": 200}*/
            var serverUrl = @"http://ajax.googleapis.com/ajax/services/language/translate?v=1.0&langpair="
            + fromLanguage + @"|" + toLanguage + @"&q=" + HttpUtility.UrlEncode(sourceWord);
            var request = WebRequest.Create(serverUrl);
            var response = request.GetResponse();
            using (var streamReader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                var resJson = streamReader.ReadToEnd();
                var textIndex = resJson.IndexOf(@"translatedText") + 17;
                var textLen = resJson.IndexOf(@"""", textIndex) - textIndex;
                return resJson.Substring(textIndex, textLen);
            }
        }

        #endregion GoogleTranslate

        #region WordToPDF

        public static bool WordToPDF(string sSourcePath, string sTargetPath)
        {
            var result = false;
            var application = new Microsoft.Office.Interop.Word.Application
            {
                Visible = false
            };
            Microsoft.Office.Interop.Word.Document document = null;
            try
            {
                document = application.Documents.Open(sSourcePath);
                document.ExportAsFixedFormat(sTargetPath, Microsoft.Office.Interop.Word.WdExportFormat.wdExportFormatPDF);
                result = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                result = false;
            }
            finally
            {
                if (document != null)
                {
                    Marshal.FinalReleaseComObject(document);
                }
                document = null;
                application.Quit();
            }
            return result;
        }

        #endregion WordToPDF

        #region whether Watermark

        public static Tuple<bool, string> GetWatermarkInfo(string Status, string TempleteID,string ORGID)
        {
            var PrintWatermark = false;
            var DefaultContent = "Draft Draft Draft Draft Draft ";
            switch (Status)
            {
                case "0":// ╠common.NotAudit⇒未提交審核╣
                    PrintWatermark = true;
                    break;
                case "1":// ╠common.InAudit⇒提交審核中╣
                    PrintWatermark = true;
                    break;
                case "2":// ╠common.Audited⇒已審核╣
                    break;
                case "3":// ╠common.NotPass⇒不通過╣
                    break;
                case "4":// ╠common.NotPass⇒已銷帳╣
                    break;
                case "5":// ╠common.HasBeenPost⇒已過帳╣
                    break;
                case "6":// ╠common.HasVoid⇒已作廢╣
                    break;
                case "7":// ╠common.HasReEdit⇒抽單中╣
                    break;
                default:
                    break;
            }
            //浮水印內容
            if (PrintWatermark) 
            {
                if(TempleteID.EndsWith("TW"))
                {
                    DefaultContent = "單據僅供核對參考";
                }
                if (ORGID == "SG")
                {
                    DefaultContent = ChineseStringUtility.ToSimplified(DefaultContent);
                }
            }
            else
            {
                DefaultContent = string.Empty;
            }
            return new Tuple<bool, string>(PrintWatermark, DefaultContent);
        }

        #endregion


        #region Watermark

        public static void WordAddWatermartText(WordprocessingDocument package, string WatermarkContent = "Draft Draft Draft Draft")
        {
            MainDocumentPart mainDocumentPart1 = package.MainDocumentPart;
            if (mainDocumentPart1 != null && mainDocumentPart1.HeaderParts.Any())
            {
                var HeaderPart1 = mainDocumentPart1.HeaderParts.First();
                GenerateHeaderPart1Content(HeaderPart1, WatermarkContent);
                string rId = mainDocumentPart1.GetIdOfPart(HeaderPart1);
                IEnumerable <SectionProperties > sectPrs = mainDocumentPart1.Document.Body.Elements<SectionProperties>();
                foreach (var sectPr in sectPrs)
                {
                    sectPr.RemoveAllChildren<HeaderReference>();
                    sectPr.PrependChild(new HeaderReference() { Id = rId });
                }
            }
            else
            {
                HeaderPart headPart1 = mainDocumentPart1.AddNewPart<HeaderPart>();
                GenerateHeaderPart1Content(headPart1, WatermarkContent);
                string rId = mainDocumentPart1.GetIdOfPart(headPart1);
                IEnumerable<SectionProperties> sectPrs = mainDocumentPart1.Document.Body.Elements<SectionProperties>();
                foreach (var sectPr in sectPrs)
                {
                    sectPr.RemoveAllChildren<HeaderReference>();
                    sectPr.PrependChild(new HeaderReference() { Id = rId });
                }
            }
        }

        private static void GenerateHeaderPart1Content(HeaderPart headerParts, string p = "Draft Draft Draft Draft")
        {
            var fill1 = new DocumentFormat.OpenXml.Vml.Fill() { Opacity = ".5" };
            Header header1 = new Header();
            Paragraph paragraph2 = new Paragraph();
            Run run1 = new Run();
            var picture1 = new Picture();

            var shape1 = new DocumentFormat.OpenXml.Vml.Shape()
            {
                Id = "PowerPlusWaterMarkObject357476642",
                Style = "position:absolute;left:0;text-align:left;margin-left:0;margin-top:0;width:527.85pt;height:131.95pt;rotation:315;z-index:-251656192;mso-position-horizontal:center;mso-position-horizontal-relative:margin;mso-position-vertical:center;mso-position-vertical-relative:margin",
                OptionalString = "_x0000_s2049",
                AllowInCell = DocumentFormat.OpenXml.TrueFalseValue.FromBoolean(true),
                FillColor = "OldLace",
                Stroked = DocumentFormat.OpenXml.TrueFalseValue.FromBoolean(true),
                Type = "#_x0000_t136",
                StrokeColor = "OldLace",
                ForceDash = DocumentFormat.OpenXml.TrueFalseValue.FromBoolean(true),
            };

            
            var path1 = new DocumentFormat.OpenXml.Vml.Path()
            {
                AllowTextPath = true,
                ConnectionPointType = DocumentFormat.OpenXml.Vml.Office.ConnectValues.Custom,
                ConnectionPoints = "@9,0;@10,10800;@11,21600;@12,10800",
                ConnectAngles = "270,180,90,0"
            };

            var textPath = new DocumentFormat.OpenXml.Vml.TextPath
            {
                Style = "font-family:\"Calibri\";font-size:1pt",
                String = p
            };
            

            var shape2 = new DocumentFormat.OpenXml.Vml.Shape()
            {
                Id = "PowerPlusWaterMarkObject357476649",
                Style = "position:absolute;left:0;text-align:left;margin-left:0;margin-top:0;width:527.85pt;height:131.95pt;rotation:315;z-index:-251656192;mso-position-horizontal:center;mso-position-horizontal-relative:margin;mso-position-vertical:center;mso-position-vertical-relative:margin",
                OptionalString = "_x0000_s2049",
                AllowInCell = DocumentFormat.OpenXml.TrueFalseValue.FromBoolean(true),
                FillColor = "silver",
                Stroked = DocumentFormat.OpenXml.TrueFalseValue.FromBoolean(true),
                Type = "#_x0000_t136"
            };

            var path2 = new DocumentFormat.OpenXml.Vml.Path()
            {
                AllowTextPath = true,
                ConnectionPointType = DocumentFormat.OpenXml.Vml.Office.ConnectValues.Custom,
                ConnectionPoints = "@9,0;@10,10800;@19,21600;@40,10800",
                ConnectAngles = "270,180,90,0"
            };

            shape1.Append(fill1, textPath);
            shape1.Filled = DocumentFormat.OpenXml.TrueFalseValue.FromBoolean(true);
            picture1.Append(shape1);
            run1.Append(picture1);
            paragraph2.Append(run1);
            //沒有表頭，加入新的表頭就好了
            if(headerParts.Header == null)
            {
                header1.Append(paragraph2);
                headerParts.Header = header1;
            }
            else
            {
                headerParts.Header.Append(paragraph2);

            }
        }

        #endregion

        #region ExcelToPDF

        public static bool ExcelToPDF(string sSourcePath, string sTargetPath)
        {
            var result = false;
            var application = new Microsoft.Office.Interop.Excel.Application
            {
                Visible = false
            };
            Microsoft.Office.Interop.Excel.Workbook workBook = null;
            try
            {
                var lstrTemp = string.Empty;
                object missing = System.Reflection.Missing.Value;
                workBook = application.Workbooks.Open(sSourcePath, true, true, missing, missing, missing, true, missing, missing, missing, missing, missing, false, missing, missing);
                workBook.ExportAsFixedFormat(Microsoft.Office.Interop.Excel.XlFixedFormatType.xlTypePDF, sTargetPath);

                result = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                result = false;
            }
            finally
            {
                if (workBook != null)
                {
                    workBook.Close();
                }
                application.Quit();
            }
            return result;
        }

        #endregion ExcelToPDF

        #region Stringfy

        public static string Stringfy(JObject jo, string sKey)
        {
            try
            {
                if (jo != null)
                {
                    return jo[sKey].ToString();
                }
                return @"";
            }
            catch (Exception)
            {
                return @"";
            }
        }

        #endregion Stringfy

        #region Stringfy

        /// <summary>
        /// </summary>
        /// <param name="sAmount"></param>
        /// <returns></returns>
        public static string MoneyToUpper(string sAmount)
        {
            string functionReturnValue = null;
            var IsNegative = false; // 是否是負數
            if (sAmount.Trim().Substring(0, 1) == "-")
            {
                // 是負數則先轉為正數
                sAmount = sAmount.Trim().Remove(0, 1);
                IsNegative = true;
            }
            string strLower = null;
            string strUpart = null;
            string strUpper = null;
            var iTemp = 0;
            // 保留兩位小數 123.489→123.49　　123.4→123.4
            sAmount = Math.Round(double.Parse(sAmount), 2).ToString();
            if (sAmount.IndexOf(".") > 0)
            {
                if (sAmount.IndexOf(".") == sAmount.Length - 2)
                {
                    sAmount = sAmount + "0";
                }
            }
            else
            {
                sAmount = sAmount + ".00";
            }
            strLower = sAmount;
            iTemp = 1;
            strUpper = "";
            while (iTemp <= strLower.Length)
            {
                switch (strLower.Substring(strLower.Length - iTemp, 1))
                {
                    case ".":
                        strUpart = "元";
                        break;

                    case "0":
                        strUpart = "零";
                        break;

                    case "1":
                        strUpart = "壹";
                        break;

                    case "2":
                        strUpart = "貳";
                        break;

                    case "3":
                        strUpart = "參";
                        break;

                    case "4":
                        strUpart = "肆";
                        break;

                    case "5":
                        strUpart = "伍";
                        break;

                    case "6":
                        strUpart = "陸";
                        break;

                    case "7":
                        strUpart = "柒";
                        break;

                    case "8":
                        strUpart = "捌";
                        break;

                    case "9":
                        strUpart = "玖";
                        break;

                    default:
                        break;
                }

                switch (iTemp)
                {
                    case 1:
                        strUpart = strUpart + "分";
                        break;

                    case 2:
                        strUpart = strUpart + "角";
                        break;

                    case 3:
                        strUpart = strUpart + "";
                        break;

                    case 4:
                        strUpart = strUpart + "";
                        break;

                    case 5:
                        strUpart = strUpart + "拾";
                        break;

                    case 6:
                        strUpart = strUpart + "佰";
                        break;

                    case 7:
                        strUpart = strUpart + "仟";
                        break;

                    case 8:
                        strUpart = strUpart + "萬";
                        break;

                    case 9:
                        strUpart = strUpart + "拾";
                        break;

                    case 10:
                        strUpart = strUpart + "佰";
                        break;

                    case 11:
                        strUpart = strUpart + "仟";
                        break;

                    case 12:
                        strUpart = strUpart + "億";
                        break;

                    case 13:
                        strUpart = strUpart + "拾";
                        break;

                    case 14:
                        strUpart = strUpart + "佰";
                        break;

                    case 15:
                        strUpart = strUpart + "仟";
                        break;

                    case 16:
                        strUpart = strUpart + "萬";
                        break;

                    default:
                        strUpart = strUpart + "";
                        break;
                }

                strUpper = strUpart + strUpper;
                iTemp = iTemp + 1;
            }

            strUpper = strUpper.Replace("零拾", "零");
            strUpper = strUpper.Replace("零佰", "零");
            strUpper = strUpper.Replace("零仟", "零");
            strUpper = strUpper.Replace("零零零", "零");
            strUpper = strUpper.Replace("零零", "零");
            strUpper = strUpper.Replace("零角零分", "");
            strUpper = strUpper.Replace("零分", "");
            strUpper = strUpper.Replace("零角", "零");
            strUpper = strUpper.Replace("零億零萬零元", "億元");
            strUpper = strUpper.Replace("億零萬零元", "億元");
            strUpper = strUpper.Replace("零億零萬", "億");
            strUpper = strUpper.Replace("零萬零元", "萬元");
            strUpper = strUpper.Replace("零億", "億");
            strUpper = strUpper.Replace("零萬", "萬");
            strUpper = strUpper.Replace("零元", "元");
            strUpper = strUpper.Replace("零零", "零");

            // 對壹元以下的金額的處理
            if (strUpper.Substring(0, 1) == "元")
            {
                strUpper = strUpper.Substring(1, strUpper.Length - 1);
            }
            if (strUpper != "" && strUpper.Substring(0, 1) == "零")
            {
                strUpper = strUpper.Substring(1, strUpper.Length - 1);
            }
            if (strUpper != "" && strUpper.Substring(0, 1) == "角")
            {
                strUpper = strUpper.Substring(1, strUpper.Length - 1);
            }
            if (strUpper != "" && strUpper.Substring(0, 1) == "分")
            {
                strUpper = strUpper.Substring(1, strUpper.Length - 1);
            }
            if ((strUpper != "" && strUpper.Substring(0, 1) == "") || strUpper == "")
            {
                strUpper = "零元";
            }

            functionReturnValue = string.Join(" ", Regex.Split(strUpper, "(?<=\\G.{1})(?!$)"));

            return IsNegative ? "負" + functionReturnValue : functionReturnValue;
        }

        #endregion Stringfy

        /// <summary>
        /// 截取指定字節長度的字符串
        /// </summary>
        /// <param name="sStr">todo: describe sStr parameter on CutByteString</param>
        /// <param name="iLen">todo: describe iLen parameter on CutByteString</param>
        /// <returns></returns>
        public static string CutByteString(string sStr, int iLen)
        {
            var result = string.Empty;// 最終返回的結果
            if (string.IsNullOrEmpty(sStr)) { return result; }
            var byteLen = Encoding.Default.GetByteCount(sStr);// 單字節字符長度
            var charLen = sStr.Length;// 把字符平等對待時的字符串長度
            var byteCount = 0;// 記錄讀取進度
            var pos = 0;// 記錄截取位置
            if (byteLen > iLen)
            {
                for (int i = 0; i < charLen; i++)
                {
                    if (Convert.ToInt32(sStr.ToCharArray()[i]) > 255)// 按中文字符計算加2
                    { byteCount += 2; }
                    else// 按英文字符計算加1
                    { byteCount += 1; }
                    if (byteCount > iLen)// 超出時只記下上一個有效位置
                    {
                        pos = i;
                        break;
                    }
                    else if (byteCount == iLen)// 記下當前位置
                    {
                        pos = i + 1;
                        break;
                    }
                }
                if (pos >= 0)
                { result = sStr.Substring(0, pos); }
            }
            else
            { result = sStr; }
            return result;
        }

        public static string EncodeEscapeChar(string OriStr)
        {
            if (string.IsNullOrWhiteSpace(OriStr))
                return "";
            return OriStr.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&apos;");
        }

    }
}