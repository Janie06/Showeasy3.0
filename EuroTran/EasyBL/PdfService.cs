using EasyBL.WebApi.Message;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace EasyBL
{
    public class PdfService : ServiceBase
    {
        #region Excel轉Pdf

        /// <summary>
        /// 函式名稱:ExcelToPdf
        /// 函式說明:系統登入
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on ExcelToPdf</param>
        /// <returns>
        /// 回傳 rm(Object)
        ///</returns>
        public ResponseMessage ExcelToPdf(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sError = null;
            string sOutputPath = null;
            try
            {
                do
                {
                    var sInputPath = _fetchString(i_crm, "InputPath");
                    var sOutputFolder = _fetchString(i_crm, "OutputFolder");
                    var sFileName = _fetchString(i_crm, "FileName");

                    var bOk = ExcelToPdf(out sOutputPath, sInputPath, sFileName, sOutputFolder);
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, sOutputPath);
                } while (false);
            }
            catch (Exception ex)
            {
                sError = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sError + "Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(PdfService), nameof(ExcelToPdf), "ExcelToPdf（Excel 轉PDF）", "", "", "");
            }
            finally
            {
                if (null != sError)
                {
                    rm = new ErrorResponseMessage(sError, i_crm);
                }
            }
            return rm;
        }

        #endregion Excel轉Pdf

        public static string HtmlToPdf(string sHtml, string sFileName, string outputFolder)
        {
            var sRoot = System.Web.HttpContext.Current.Server.MapPath("/EurotranFile");
            Common.FnCreateDir(sRoot + "\\" + outputFolder);//如果沒有該目錄就創建目錄
            var sOutputPath = sRoot + "\\" + outputFolder + "\\" + sFileName;
            var sHtmlPath = sOutputPath + ".html";
            var sPdfPath = sOutputPath + ".pdf";

            var sContent = System.Web.HttpContext.Current.Server.HtmlDecode(sHtml);
            if (File.Exists(sHtmlPath))
            {
                File.Delete(sHtmlPath);
            }
            using (FileStream fs = new FileStream(sHtmlPath, FileMode.Create))
            {
                using (StreamWriter w = new StreamWriter(fs, Encoding.UTF8))
                {
                    w.WriteLine(sContent);
                }
            }
            try
            {
                if (!string.IsNullOrEmpty(sHtmlPath) || !string.IsNullOrEmpty(sPdfPath))
                {
                    var resource = sRoot + "\\Resoure";
                    var dllstr = string.Format(resource + "\\wkhtmltopdf.exe");

                    if (File.Exists(dllstr))
                    {
                        var strParam = sHtmlPath + " " + sPdfPath;
                        var pInfo = new ProcessStartInfo
                        {
                            FileName = dllstr,
                            Arguments = strParam.ToString(),
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            //pInfo.RedirectStandardError = true;
                            RedirectStandardInput = true,
                            CreateNoWindow = false
                        };

                        using (var p = new Process
                        {
                            StartInfo = pInfo
                        })
                        {
                            p.Start();
                            p.WaitForExit();

                            try
                            {
                                if (!p.HasExited)
                                {
                                    p.Kill();
                                }
                            }
                            catch { }
                        }
                    }
                }
                File.Delete(sHtmlPath);//刪除html文件
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return sPdfPath.Replace("\\", "/");
        }

        public static bool ExcelToPdf(out string outputPath, string inputPath, string sFileName = null, string sTempFolder = null)
        {
            if (string.IsNullOrEmpty(sFileName)) { sFileName = Guid.NewGuid().ToString(); }
            if (string.IsNullOrEmpty(sTempFolder)) { sTempFolder = "Temp"; }
            var bOk = false;
            var sRoot = System.Web.HttpContext.Current.Server.MapPath("/EurotranFile");
            Common.FnCreateDir(sRoot + "\\" + sTempFolder);//如果沒有該目錄就創建目錄
            var sOutputPath = sRoot + "\\" + sTempFolder + "\\" + sFileName;
            var sPdfPath = sOutputPath + ".pdf";
            try
            {
                //Spire.Xls.Workbook xls = new Spire.Xls.Workbook();
                //xls.ConverterSetting.SheetFitToPage = true;
                //xls.LoadFromFile(inputPath, Spire.Xls.ExcelVersion.Version97to2003);
                //xls.SaveToFile(sPdfPath, Spire.Xls.FileFormat.PDF);
                //bOk = true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            outputPath = sPdfPath.Replace("\\", "/");
            return bOk;
        }

        public static bool DelFile(string sPath)
        {
            var blRet = false;
            try
            {
                if (sPath.StartsWith("Document") || sPath.StartsWith("OutFiles"))
                {
                    sPath = System.Web.HttpContext.Current.Server.MapPath("/" + sPath);
                }
                if (File.Exists(sPath))
                {
                    File.Delete(sPath);
                    blRet = true;
                }
            }
            catch
            {
                blRet = false;
            }
            return blRet;
        }

        //字符串转流
        public static MemoryStream StringToStream(string s)
        {
            // convert string to stream
            var byteArray = Encoding.Default.GetBytes(s);
            var stream = new MemoryStream(byteArray);
            return stream;
        }

        //流转字符串
        public static string StreamToString(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                var text = reader.ReadToEnd();
                return text;
            }
        }

        //字符串转字节数组
        public static Byte[] StringToByteArray(string s)
        {
            return Encoding.Default.GetBytes(s);
        }

        //字节数组转字符串
        public static string ByteArrayToString(Byte[] bytes)
        {
            return Encoding.Default.GetString(bytes);
        }
    }
}