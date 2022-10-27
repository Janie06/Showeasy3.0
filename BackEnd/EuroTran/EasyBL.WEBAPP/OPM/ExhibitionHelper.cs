using DocumentFormat.OpenXml.Wordprocessing;
using Entity.Sugar;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Linq;

namespace EasyBL.WEBAPP.OPM
{
    public class ExhibitionHelper
    {
        #region RenderFeeItemsTW

        /// <summary>
        /// 組裝費用表格
        /// </summary>
        /// <param name="row"></param>
        /// <param name="jo"></param>
        /// <param name="iCellCount"></param>
        /// <param name="iIdex"></param>
        public static void RenderFeeItemsTW(TableRow row, JObject jo, int iCellCount, int iIdex)
        {
            for (int i = 0; i < iCellCount; i++)
            {
                var tcell = row.Elements<TableCell>().ElementAt(i);
                var tmpPa = tcell.Elements<Paragraph>().FirstOrDefault();
                var tmpRun = tmpPa.Elements<Run>().FirstOrDefault();
                var tmpText = tmpRun.Elements<Text>().FirstOrDefault();
                switch (i)
                {
                    case 0:
                        tmpText.Text = iIdex.ToString();
                        break;

                    case 1:
                        var sRemark = jo[@"Memo"] == null ? @"" : jo[@"Memo"].ToString();
                        var sCostStatement = jo[@"FinancialCostStatement"].ToString();
                        tmpText.Text = sCostStatement == @"" ? sRemark : jo[@"FinancialCostStatement"] + (sRemark == @"" ? @"" : @"（" + sRemark + @"）");
                        break;

                    case 2:
                        tmpText.Text = jo[@"FinancialCurrency"].ToString();
                        break;

                    case 3:
                        var sFinancialUnitPrice = jo[@"FinancialUnitPrice"].ToString();
                        tmpText.Text = $@"{Convert.ToDecimal(sFinancialUnitPrice.Trim() == @"" ? @"0" : sFinancialUnitPrice):N}";
                        break;

                    case 4:
                        tmpText.Text = jo[@"FinancialNumber"].ToString();
                        break;

                    case 5:
                        tmpText.Text = jo[@"FinancialUnit"].ToString();
                        break;

                    case 6:
                        var sFinancialAmount = jo[@"FinancialAmount"].ToString();
                        tmpText.Text = $@"{Convert.ToDecimal(sFinancialAmount.Trim() == @"" ? @"0" : sFinancialAmount):N}";
                        break;

                    case 7:
                        tmpText.Text = jo[@"FinancialExchangeRate"].ToString();
                        break;

                    case 8:
                        var sFinancialTWAmount = jo[@"FinancialTWAmount"].ToString();
                        tmpText.Text = $@"{Convert.ToDecimal(sFinancialTWAmount.Trim() == @"" ? @"0" : sFinancialTWAmount):N}";
                        break;

                    default:
                        break;
                }
            }
        }

        #endregion RenderFeeItemsTW

        #region RenderFeeItemsTW

        /// <summary>
        /// 組裝費用表格
        /// </summary>
        /// <param name="row"></param>
        /// <param name="jo"></param>
        /// <param name="iCellCount"></param>
        /// <param name="iIdex"></param>
        public static void RenderFeeItemsFR(TableRow row, JObject jo, int iCellCount, int iIdex)
        {
            for (int i = 0; i < iCellCount; i++)
            {
                var tcell = row.Elements<TableCell>().ElementAt(i);
                var tmpPa = tcell.Elements<Paragraph>().FirstOrDefault();
                var tmpRun = tmpPa.Elements<Run>().FirstOrDefault();
                var tmpText = tmpRun.Elements<Text>().FirstOrDefault();
                switch (i)
                {
                    case 0:
                        tmpText.Text = iIdex.ToString();
                        break;

                    case 1:
                        var sRemark = jo[@"Memo"] == null ? @"" : jo[@"Memo"].ToString();
                        var sCostStatement = jo[@"FinancialCostStatement"].ToString();
                        tmpText.Text = sCostStatement == @"" ? sRemark : jo[@"FinancialCostStatement"] + (sRemark == @"" ? @"" : @"（" + sRemark + @"）");
                        break;

                    case 2:
                        tmpText.Text = jo[@"FinancialCurrency"].ToString();
                        break;

                    case 3:
                        var sFinancialUnitPrice = jo[@"FinancialUnitPrice"].ToString();
                        tmpText.Text = $@"{Convert.ToDecimal(sFinancialUnitPrice.Trim() == @"" ? @"0" : sFinancialUnitPrice):N}";
                        break;

                    case 4:
                        tmpText.Text = jo[@"FinancialNumber"].ToString();
                        break;

                    case 5:
                        tmpText.Text = jo[@"FinancialUnit"].ToString();
                        break;

                    case 6:
                        var sFinancialTWAmount = jo[@"FinancialTWAmount"].ToString();
                        tmpText.Text = $@"{Convert.ToDecimal(sFinancialTWAmount.Trim() == @"" ? @"0" : sFinancialTWAmount):N}";//保留兩位小數
                        break;

                    default:
                        break;
                }
            }
        }

        #endregion RenderFeeItemsTW

        #region RenderMemo

        /// <summary>
        /// 組裝費用表格
        /// </summary>
        /// <param name="body">todo: describe body parameter on RenderMemo</param>
        /// <param name="memo">todo: describe memo parameter on RenderMemo</param>
        public static void RenderMemo(Body body, string memo)
        {
            Paragraph pMemo = null;
            foreach (DocumentFormat.OpenXml.OpenXmlElement el in body.ChildElements)
            {
                if (el.GetType() == typeof(Paragraph) && el.InnerText.IndexOf(@"[PARM27]") > -1)
                {
                    pMemo = (Paragraph)el;
                    break;
                }
            }
            var saMemo = memo.Split(new string[] { @"\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string str in saMemo)
            {
                var pCopy = pMemo.Clone() as Paragraph;
                var sPart = pMemo.InnerXml.Replace(@"[PARM27]", str);
                if (sPart.Contains(@"&") || sPart.Contains(@"<") || sPart.Contains(@">") || sPart.Contains(@"""") || sPart.Contains(@"'"))
                {
                    sPart = Common.EncodeEscapeChar(sPart);
                    //sPart = sPart.Replace(@"&", @"+");
                    ////sPart = sPart.Replace("<", "&gt;");
                    ////sPart = sPart.Replace(">", "&lt;");
                    ////sPart = sPart.Replace("\"", "&quot;");
                    ////sPart = sPart.Replace("\'", "&apos;");
                }
                pCopy.InnerXml = sPart;
                pMemo.InsertBeforeSelf(pCopy);
            }
            pMemo.Remove();
        }

        #endregion RenderMemo

        /// <summary> 实现数据的四舍五入法
        　　 /// </summary>
        /// <param name="v">要进行处理的数据</param>
        /// <param name="x">保留的小数位数</param>
        /// <returns>四舍五入后的结果</returns>
        public static double Round(double v, int x)
        {
            return Math.Round(v, x, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// 優先取得發票地址(InvoiceAddress)，再來是地址(Address)
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        public static string GetBillAddress(OTB_CRM_Customers customer)
        {
            if (customer == null)
                return "";
            string InvoiceAddress = string.IsNullOrWhiteSpace(customer.InvoiceAddress) ? "": customer.InvoiceAddress;
            string Address = string.IsNullOrWhiteSpace(customer.Address) ? "" : customer.Address;
            return InvoiceAddress.Length > 0 ? InvoiceAddress : Address;
        }

        /// <summary>
        /// 首字大寫
        /// </summary>
        /// <param name="memberID"></param>
        /// <returns></returns>
        public static string GetEnglishName(string memberID)
        {
            if (string.IsNullOrEmpty(memberID))
                return "";
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            memberID = memberID.Replace(".", " ");
            return textInfo.ToTitleCase(memberID);
        }

        /// <summary>
        /// 進口加註文字:
        /// 報價/預估成本:進口/Inbound
        /// 退運報價/預估成本: 回運/ Return
        /// </summary>
        /// <param name="GetExhibitioImport"></param>
        /// <param name="IsEnglish"></param>
        /// <param name="IsReturn"></param>
        /// <returns></returns>
        public static string GetExhibitioImportComment(bool English = false, bool Return = false)
        {
            //英文:Inbound/Return
            if (English)
            {
                if (Return)
                    return "(Return)";
                else
                    return "(Inbound)";
            }
            //中文:進口/回運
            else
            {
                if (Return)
                    return "(回運)";
                else
                    return "(進口)";
            }
        }

        /// <summary>
        /// 出口加註文字:
        /// 報價/預估成本:出口/ Outbound
        /// 退運報價/預估成本: 回運/ Return
        /// </summary>
        /// <param name="ExhibitioExportName"></param>
        /// <param name="IsEnglish"></param>
        /// <param name="IsReturn"></param>
        /// <returns></returns>
        public static string GetExhibitioExportComment( bool English = false, bool Return = false)
        {
            //英文:Outbound/Return
            if (English)
            {
                if (Return)
                    return "(Return)";
                else
                    return "(Outbound)";
            }
            //中文:出口/回運
            else
            {
                if (Return)
                    return "(回運)";
                else
                    return "(出口)";
            }
        }
    }
}