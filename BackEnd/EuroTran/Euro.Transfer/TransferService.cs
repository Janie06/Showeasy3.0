using Entity.Sugar;
using Euro.Transfer.Base;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Euro.Transfer
{
    public class TransferService : ServiceTools
    {
        #region transferBill

        /// <summary>
        /// 文字檔轉換（帳單）
        /// </summary>
        /// <param name="txtBox">todo: describe txtBox parameter on TransferBill</param>
        /// <param name="lbCount">todo: describe lbCount parameter on TransferBill</param>
        /// <param name="sOrgID">todo: describe sOrgID parameter on TransferBill</param>
        /// <param name="sUserID">todo: describe sUserID parameter on TransferBill</param>
        /// <param name="sWord">todo: describe sWord parameter on TransferBill</param>
        public static void TransferBill(HubTransfer.WriteOrLogsHandler writer, string sOrgID, string sUserID, string sWord)
        {
            try
            {
                var bills = JsonConvert.DeserializeObject<List<OTB_OPM_Bills>>(sWord);
                var query = bills.GroupBy(p => p.CustomerCode);
                foreach (IGrouping<string, OTB_OPM_Bills> billsgroup in query)
                {
                    foreach (OTB_OPM_Bills bill in billsgroup)
                    {
                        var sBillWords = PackBillWords(bill);
                        WriteWords(sBillWords, sOrgID, ".025", "A_");
                    }
                }

                var sbTxt = new StringBuilder();
                var sbLogAppend = new StringBuilder();
                var iCount = 0;
                foreach (OTB_OPM_Bills bill in bills)
                {
                    sbLogAppend.Append("狀態：  帳單號碼：" + bill.BillNO + " 轉檔成功  拋轉人：" + sOrgID + "-" + sUserID + "  " + DateTime.Now).Append("\n");
                    iCount++;
                }
                writer(sbLogAppend.ToString(), iCount);
                WriteWordLog("", sbLogAppend.ToString(), true, "Bills");//記錄所有log
            }
            catch (Exception ex)
            {
                //写错误日志
                WriteLog(Errorlog_Path, ex.ToString(), true);
            }
        }

        #endregion transferBill

        #region transferCus

        /// <summary>
        /// 文字檔轉換(客戶)
        /// </summary>
        /// <param name="txtBox">todo: describe txtBox parameter on TransferCus</param>
        /// <param name="lbCount">todo: describe lbCount parameter on TransferCus</param>
        /// <param name="sOrg">todo: describe sOrg parameter on TransferCus</param>
        /// <param name="sUser">todo: describe sUser parameter on TransferCus</param>
        /// <param name="sWord">todo: describe sWord parameter on TransferCus</param>
        public static void TransferCus(HubTransfer.WriteOrLogsHandler writer, string sOrg, string sUser, string sWord)
        {
            try
            {
                var customers = JsonConvert.DeserializeObject<List<OTB_CRM_CustomersTransfer>>(sWord);
                var query = customers.GroupBy(p => p.Feild01);
                foreach (IGrouping<string, OTB_CRM_CustomersTransfer> customersgroup in query)
                {
                    foreach (OTB_CRM_CustomersTransfer customer in customersgroup)
                    {
                        var sCustomersWords = PackCustomersWords(customer);
                        WriteWords(sCustomersWords, sOrg, ".011");
                    }
                }

                var sbTxt = new StringBuilder();
                var sbLogAppend = new StringBuilder();
                var iCount = 0;
                foreach (OTB_CRM_CustomersTransfer cus in customers)
                {
                    sbLogAppend.Append("狀態：  客戶代號：" + cus.Feild01 + " 轉檔成功  拋轉人：" + sOrg + "-" + sUser + "  " + DateTime.Now).Append("\n");
                    iCount++;
                }
                writer(sbLogAppend.ToString(), iCount);
                WriteWordLog("", sbLogAppend.ToString(), true, "Customers");//記錄所有log
            }
            catch (Exception ex)
            {
                //写错误日志
                WriteLog(Errorlog_Path, ex.ToString(), true);
            }
        }

        #endregion transferCus

        #region transferPrj

        /// <summary>
        /// 文字檔轉換(專案|展覽)
        /// </summary>
        /// <param name="txtBox">todo: describe txtBox parameter on TransferPrj</param>
        /// <param name="lbCount">todo: describe lbCount parameter on TransferPrj</param>
        /// <param name="sOrg">todo: describe sOrg parameter on TransferPrj</param>
        /// <param name="sUser">todo: describe sUser parameter on TransferPrj</param>
        /// <param name="sWord">todo: describe sWord parameter on TransferPrj</param>
        public static void TransferPrj(HubTransfer.WriteOrLogsHandler writer, string sOrg, string sUser, string sWord)
        {
            try
            {
                var exhibitions = JsonConvert.DeserializeObject<List<OTB_OPM_ExhibitionsTransfer>>(sWord);
                var query = exhibitions.GroupBy(p => p.PrjNO);
                foreach (IGrouping<string, OTB_OPM_ExhibitionsTransfer> exhibitionsgroup in query)
                {
                    foreach (OTB_OPM_ExhibitionsTransfer exhibition in exhibitionsgroup)
                    {
                        var sExhibitionWords = PackExhibitionsWords(exhibition);
                        WriteWords(sExhibitionWords, sOrg, ".034", "A_");
                    }
                }

                var sbTxt = new StringBuilder();
                var sbLogAppend = new StringBuilder();
                var iCount = 0;
                foreach (OTB_OPM_ExhibitionsTransfer cus in exhibitions)
                {
                    sbLogAppend.Append("狀態：  專案代號：" + cus.PrjNO + " 轉檔成功  拋轉人：" + sOrg + "-" + sUser + "  " + DateTime.Now).Append("\n");
                    iCount++;
                }
                writer(sbLogAppend.ToString(), iCount);
                WriteWordLog("", sbLogAppend.ToString(), true, "Exhibitions");//記錄所有log
            }
            catch (Exception ex)
            {
                //写错误日志
                WriteLog(Errorlog_Path, ex.ToString(), true);
            }
        }

        #endregion transferPrj

        #region packBillWords

        /// <summary>
        /// 組裝文字檔(帳單)
        /// </summary>
        /// <param name="bills">帳單資料</param>
        /// <returns></returns>
        private static string PackBillWords(OTB_OPM_Bills bill)
        {
            //sb.Append("\n"); //換行(目前第一行抓不到，不知道什麼原因)
            var sb = new StringBuilder();
            sb.Append(bill.BillNO.PadRight(15, ' '));//帳款號碼(1)
            sb.Append(bill.CheckDate.PadRight(8, ' ')); //對帳日期(2)【帳單日期：BillFirstCheckDate】
            sb.Append(bill.BillType.PadRight(2, ' '));//帳別(收付)(3)
            sb.Append(bill.CustomerCode.PadRight(10, ' ')); //客戶供應商代號(4)
            sb.Append(bill.ResponsiblePersonCode.PadRight(11, ' ')); //業務員代號(5)
            sb.Append(bill.LastGetBillDate.PadRight(8, ' ')); //最近收付日(6)
            sb.Append(bill.LastGetBillNO.PadRight(15, ' ')); //最近收付單號(7)
            sb.Append(bill.TaxType.PadRight(1, ' ')); //稅別(8)
            sb.Append(bill.NOTaxAmount.PadRight(15, ' ')); //未稅金額(9)
            sb.Append(bill.BillAmount.PadRight(15, ' ')); //帳款金額(10)
            sb.Append(bill.PaymentAmount.PadRight(15, ' ')); //收付金額(11)
            sb.Append(bill.Allowance.PadRight(15, ' ')); //折讓金額(12)
            sb.Append(bill.DebtAmount.PadRight(15, ' ')); //呆帳金額(13)
            sb.Append(bill.ExchangeAmount.PadRight(15, ' ')); //匯兌損益金額(14)
            sb.Append(bill.Settle.PadRight(1, ' ')); //結清否(15)
            sb.Append(bill.InvoiceStartNumber.PadRight(10, ' ')); //發票號碼(起)(16)
            sb.Append(bill.InvoiceEndNumber.PadRight(10, ' ')); //發票號碼(迄)(17)
            sb.Append(bill.Category.PadRight(4, ' ')); //傳票類別(18)
            sb.Append(bill.OrderNo.PadRight(15, ' ')); //訂單單號(19)
            sb.Append(bill.ClosingNote.PadRight(1, ' ')); //結帳註記(20)
            sb.Append(bill.GeneralInvoiceNumber.PadRight(15, ' ')); //立帳總帳傳票號碼(21)
            sb.Append(bill.GeneralSerialNumber.PadRight(4, ' ')); //立帳總帳傳票序號(22)
            sb.Append(bill.Remark1.PadRight(30, ' ')); //備註一(30C)(23)
            sb.Append(bill.AccountSource.PadRight(1, ' ')); //帳款來源(24)
            sb.Append(bill.UpdateDate.PadRight(20, ' ')); //更新日期(25)
            sb.Append(bill.UpdatePersonnel.PadRight(12, ' ')); //更新人員(26)
            sb.Append(bill.DepartmentSiteNumber.PadRight(10, ' ')); //部門\ 工地編號(27)
            sb.Append(bill.ProjectNumber.PadRight(10, ' ')); //專案\ 項目編號(28)
            sb.Append(bill.TransferBNotes.PadRight(1, ' ')); //轉B 帳註記(29)
            sb.Append(bill.ABNumber.PadRight(12, ' ')); //A|B 帳唯一流水號(30)
            sb.Append(bill.EnterNumber.PadRight(15, ' ')); //進銷單號(31)
            sb.Append(bill.ForeignCurrencyCode.PadRight(3, ' ')); //外幣代號(32)
            sb.Append(bill.ExchangeRate.PadRight(8, ' ')); //匯率(33)
            sb.Append(bill.ForeignAmount.PadRight(15, ' ')); //外幣金額(34)
            sb.Append(bill.PayAmount.PadRight(15, ' ')); //收付沖抵金額(35)
            sb.Append(bill.RefundAmount.PadRight(15, ' ')); //退款金額(36)
            sb.Append(bill.PaymentTerms.PadRight(1, ' ')); //收付條件(37)
            sb.Append(bill.AccountDate.PadRight(8, ' ')); //帳款日期(38)【帳單日期：BillFirstCheckDate】
            sb.Append(bill.DCreditCardNumber.PadRight(16, ' ')); //預設信用卡號(39)
            sb.Append(bill.ClosingDate.PadRight(8, ' '));//結帳日期(40)
            sb.Append(bill.CusField1.PadRight(100, ' '));//自定義欄位一(41)
            sb.Append(bill.CusField2.PadRight(100, ' '));//自定義欄位二(42)
            sb.Append(bill.CusField3.PadRight(100, ' '));//自定義欄位三(43)
            sb.Append(bill.CusField4.PadRight(100, ' '));//自定義欄位四(44)
            sb.Append(bill.CusField5.PadRight(100, ' '));//自定義欄位五(45)
            sb.Append(bill.CusField6.PadRight(15, ' ')); //自定義欄位六(46)
            sb.Append(bill.CusField7.PadRight(15, ' ')); //自定義欄位七(47)
            sb.Append(bill.CusField8.PadRight(15, ' ')); //自定義欄位八(48)
            sb.Append(bill.CusField9.PadRight(15, ' ')); //自定義欄位九(49)
            sb.Append(bill.CusField10.PadRight(15, ' ')); //自定義欄位十(50)
            sb.Append(bill.CusField11.PadRight(8, ' ')); //自定義欄位十一(51)
            sb.Append(bill.CusField12.PadRight(8, ' ')); //自定義欄位十二(52)
            sb.Append(bill.Remark2.PadRight(250, ' '));  //備註二(M)(53)
            sb.Append(bill.TWNOTaxAmount.PadRight(15, ' ')); //台幣未稅金額(54)
            sb.Append(bill.TWAmount.PadRight(15, ' ')); //台幣帳款金額(55)
            sb.Append("".PadRight(10, ' '));

            return sb.ToString();
        }

        #endregion packBillWords

        #region packCustomersWords

        /// <summary>
        /// 組裝文字檔(客戶)
        /// </summary>
        /// <param name="customers">todo: describe customers parameter on PackCustomersWords</param>
        /// <returns></returns>
        private static string PackCustomersWords(OTB_CRM_CustomersTransfer cus)
        {
            var sb = new StringBuilder();
            sb.Append(cus.Feild01.PadRight(10, ' '));
            sb.Append(cus.Feild02.PadRight(1, ' '));
            sb.Append(ChineseStringUtility.ToTraditional(Common.PadRightEx(cus.Feild03, 12, ' ')));
            sb.Append(ChineseStringUtility.ToTraditional(Common.PadRightEx(cus.Feild04, 60, ' ')));
            sb.Append(cus.Feild05.PadRight(12, ' '));
            sb.Append(cus.Feild06.PadRight(4, ' '));
            sb.Append(cus.Feild07.PadRight(8, ' '));
            sb.Append(cus.Feild08.PadRight(9, ' '));
            sb.Append(cus.Feild09.PadRight(5, ' '));
            sb.Append(ChineseStringUtility.ToTraditional(Common.PadRightEx(cus.Feild10, 60, ' ')));
            sb.Append(ChineseStringUtility.ToTraditional(Common.PadRightEx(cus.Feild11, 60, ' ')));
            sb.Append(ChineseStringUtility.ToTraditional(Common.PadRightEx(cus.Feild12, 60, ' ')));
            sb.Append(cus.Feild13.PadRight(20, ' '));
            sb.Append(cus.Feild14.PadRight(20, ' '));
            sb.Append(cus.Feild15.PadRight(20, ' '));
            sb.Append(cus.Feild16.PadRight(20, ' '));
            sb.Append(cus.Feild17.PadRight(20, ' '));
            sb.Append(cus.Feild18.PadRight(20, ' '));
            sb.Append(cus.Feild19.PadRight(20, ' '));
            sb.Append(cus.Feild20.PadRight(30, ' '));
            sb.Append(cus.Feild21.PadRight(12, ' '));
            sb.Append(cus.Feild22.PadRight(12, ' '));
            sb.Append(ChineseStringUtility.ToTraditional(Common.PadRightEx(cus.Feild23, 30, ' ')));
            sb.Append(cus.Feild24.PadRight(18, ' '));
            sb.Append(cus.Feild25.PadRight(1, ' '));
            sb.Append(cus.Feild26.PadRight(5, ' '));
            sb.Append(cus.Feild27.PadRight(15, ' '));
            sb.Append(cus.Feild28.PadRight(10, ' '));
            sb.Append(cus.Feild29.PadRight(11, ' '));
            sb.Append(cus.Feild30.PadRight(11, ' '));
            sb.Append(cus.Feild31.PadRight(8, ' '));
            sb.Append(cus.Feild32.PadRight(8, ' '));
            sb.Append(cus.Feild33.PadRight(15, ' '));
            sb.Append(cus.Feild34.PadRight(15, ' '));
            sb.Append(cus.Feild35.PadRight(15, ' '));
            sb.Append(cus.Feild36.PadRight(15, ' '));
            sb.Append(cus.Feild37.PadRight(1, ' '));
            sb.Append(cus.Feild38.PadRight(1, ' '));
            sb.Append(cus.Feild39.PadRight(30, ' '));
            sb.Append(cus.Feild40.PadRight(30, ' '));
            sb.Append(cus.Feild41.PadRight(1, ' '));
            sb.Append(cus.Feild42.PadRight(2, ' '));
            sb.Append(cus.Feild43.PadRight(2, ' '));
            sb.Append(cus.Feild44.PadRight(2, ' '));
            sb.Append(cus.Feild45.PadRight(2, ' '));
            sb.Append(cus.Feild46.PadRight(2, ' '));
            sb.Append(cus.Feild47.PadRight(2, ' '));
            sb.Append(cus.Feild48.PadRight(2, ' '));
            sb.Append(cus.Feild49.PadRight(2, ' '));
            sb.Append(cus.Feild50.PadRight(2, ' '));
            sb.Append(cus.Feild51.PadRight(2, ' '));
            sb.Append(cus.Feild52.PadRight(2, ' '));
            sb.Append(cus.Feild53.PadRight(2, ' '));
            sb.Append(cus.Feild54.PadRight(5, ' '));
            sb.Append(cus.Feild55.PadRight(5, ' '));
            sb.Append(cus.Feild56.PadRight(20, ' '));
            sb.Append(cus.Feild57.PadRight(10, ' '));
            sb.Append(cus.Feild58.PadRight(10, ' '));
            sb.Append(cus.Feild59.PadRight(250, ' '));
            sb.Append(cus.Feild60.PadRight(250, ' '));
            sb.Append(cus.Feild61.PadRight(250, ' '));
            sb.Append(cus.Feild62.PadRight(6, ' '));
            sb.Append(cus.Feild63.PadRight(3, ' '));
            sb.Append(cus.Feild64.PadRight(50, ' '));
            sb.Append(cus.Feild65.PadRight(50, ' '));
            sb.Append(cus.Feild66.PadRight(1, ' '));
            sb.Append(cus.Feild67.PadRight(1, ' '));
            sb.Append(cus.Feild68.PadRight(10, ' '));
            sb.Append(cus.Feild69.PadRight(100, ' '));
            sb.Append(cus.Feild70.PadRight(100, ' '));
            sb.Append(cus.Feild71.PadRight(100, ' '));
            sb.Append(cus.Feild72.PadRight(100, ' '));
            sb.Append(cus.Feild73.PadRight(100, ' '));
            sb.Append(cus.Feild74.PadRight(15, ' '));
            sb.Append(cus.Feild75.PadRight(15, ' '));
            sb.Append(cus.Feild76.PadRight(15, ' '));
            sb.Append(cus.Feild77.PadRight(15, ' '));
            sb.Append(cus.Feild78.PadRight(15, ' '));
            sb.Append(cus.Feild79.PadRight(8, ' '));
            sb.Append(cus.Feild80.PadRight(8, ' '));
            sb.Append(cus.Feild81.PadRight(11, ' '));
            sb.Append(cus.Feild82.PadRight(120, ' '));
            sb.Append(cus.Feild83.PadRight(240, ' '));
            sb.Append(cus.Feild84.PadRight(2, ' '));
            sb.Append(cus.Feild85.PadRight(2, ' '));
            sb.Append(cus.Feild86.PadRight(1, ' '));
            sb.Append(cus.Feild87.PadRight(1, ' '));
            sb.Append(cus.Feild88.PadRight(1, ' '));
            sb.Append(cus.Feild89.PadRight(20, ' '));
            sb.Append(cus.Feild90.PadRight(250, ' '));
            sb.Append(cus.Feild91.PadRight(1, ' '));
            sb.Append(cus.Feild92.PadRight(1, ' '));
            sb.Append(cus.Feild93.PadRight(1, ' '));
            sb.Append(cus.Feild94.PadRight(128, ' '));
            sb.Append(cus.Feild95.PadRight(20, ' '));
            sb.Append(cus.Feild96.PadRight(20, ' '));
            sb.Append(cus.Feild97.PadRight(1, ' '));
            sb.Append(cus.Feild98.PadRight(1, ' '));
            sb.Append(cus.Feild99.PadRight(1, ' '));
            sb.Append("".PadRight(10, ' '));

            return sb.ToString();
        }

        #endregion packCustomersWords

        #region packExhibitionsWords

        /// <summary>
        /// 組裝文字檔(專案|展覽)
        /// </summary>
        /// <param name="exhibitions">todo: describe exhibitions parameter on PackExhibitionsWords</param>
        /// <returns></returns>
        private static string PackExhibitionsWords(OTB_OPM_ExhibitionsTransfer exhibition)
        {
            var sb = new StringBuilder();
            sb.Append(exhibition.PrjNO.PadRight(10, ' '));
            sb.Append(ChineseStringUtility.ToTraditional(Common.PadRightEx(exhibition.PrjName, 60, ' ')));
            sb.Append(Common.PadRightEx(exhibition.PrjCharger, 11, ' '));
            sb.Append(exhibition.EndDate.PadRight(8, ' '));
            sb.Append("".PadRight(10, ' '));

            return sb.ToString();
        }

        #endregion packExhibitionsWords

        #region preBlank

        /// <summary>
        /// 欄位前方補空白
        /// </summary>
        /// <param name="len"></param>
        /// <param name="word">todo: describe word parameter on PreBlank</param>
        private static string PreBlank(string word, int len)
        {
            return word.PadLeft(len, ' ');
        }

        #endregion preBlank

        #region preBlank

        /// <summary>
        /// 欄位後方補空白
        /// </summary>
        /// <param name="len"></param>
        /// <param name="word">todo: describe word parameter on AfterBlank</param>
        private static string AfterBlank(string word, int len)
        {
            return word.PadRight(len, ' ');
        }

        #endregion preBlank
    }
}