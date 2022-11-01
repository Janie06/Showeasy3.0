using System;
using System.Collections.Generic;
using System.Linq;
using EasyBL.WebApi.Message;
using Entity.Sugar;
using SqlSugar;
using SqlSugar.Base;
using Newtonsoft.Json.Linq;
using EasyBL;
using System.Text.RegularExpressions;

namespace TestAP.Helper
{
    public class ConvertData
    {
        public Dictionary<string, int> dicSucceccCount = new Dictionary<string, int>();
        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
        //出口帳款
        public bool ExcExport { get; set; } = false;
        //退運出口帳款
        public bool ExcExportReturn { get; set; } = false;
        //進口帳款
        public bool ExcImport { get; set; } = false;
        //退運進口帳款
        public bool ExcImportReturn { get; set; } = false;
        //其他帳款
        public bool ExcOther { get; set; } = false;
        //退運其他帳款
        //其他(駒驛)帳款
        public bool ExcOtherTG { get; set; } = false;
        //退運其他(駒驛)帳款
        //展覽
        public bool ExcExhibtion { get; set; } = false;
        //客戶
        public bool ExcCustomer { get; set; } = false;
        //特定帳款號碼
        public List<string> BillsNo { get; set; } = new List<string>();
        //特定專案號碼
        public List<string> ExhibtionsNo { get; set; } = new List<string>();
        //特定客戶號碼
        public List<string> CustomersNo { get; set; } = new List<string>();

        public Dictionary<string, int> StartConvert(out string o_sMsg)
        {
            string sMsg = null;
            o_sMsg = null;

            ResponseMessage rm = null;

            try
            {

                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        if (string.IsNullOrEmpty(StartDate))
                        {
                            sMsg = "請設定StartDate";
                            break;
                        }

                        if (string.IsNullOrEmpty(EndDate))
                        {
                            sMsg = "請設定EndDate";
                            break;
                        }

                        Console.WriteLine("轉換進口資料開始...");

                        sMsg = Import(StartDate, EndDate, db);

                        if (sMsg != null)
                        {
                            break;
                        }

                        Console.WriteLine("轉換進口資料結束...");

                        Console.WriteLine("轉換出口資料開始...");

                        sMsg = Export(StartDate, EndDate, db);

                        if (sMsg != null)
                        {
                            break;
                        }

                        Console.WriteLine("轉換出口資料結束...");

                        Console.WriteLine("轉換其他資料開始...");

                        sMsg = Other(StartDate, EndDate, db);

                        if (sMsg != null)
                        {
                            break;
                        }

                        Console.WriteLine("轉換其他資料結束...");

                        Console.WriteLine("轉換其他(駒驛)資料開始...");

                        sMsg = OtherTG(StartDate, EndDate, db);

                        if (sMsg != null)
                        {
                            break;
                        }

                        Console.WriteLine("轉換其他(駒驛)資料結束...");

                        Console.WriteLine("轉換專案(展覽)資料開始...");

                        sMsg = Exhibition(StartDate, EndDate, db);

                        if (sMsg != null)
                        {
                            break;
                        }

                        Console.WriteLine("轉換專案(展覽)資料結束...");

                        Console.WriteLine("轉換客戶資料開始...");

                        sMsg = Customer(StartDate, EndDate, db);

                        if (sMsg != null)
                        {
                            break;
                        }

                        Console.WriteLine("轉換客戶資料結束...");

                    } while (false);

                    if (sMsg != null)
                    {
                        db.Ado.RollbackTran();
                    }

                    return rm;
                }

                );

            }
            catch (Exception ex)
            {
                sMsg = ex.Message;
            }

            if (sMsg != null)
            {
                o_sMsg = sMsg;
            }

            return dicSucceccCount;
        }

        public string Import(string sStartDate, string sEndDate, SqlSugarClient db)
        {
            string sMsg = null;
            DateTime dtSettingStartDate = Convert.ToDateTime(sStartDate);
            DateTime dtSettingEnd = Convert.ToDateTime(sEndDate);

            #region 進口

            var data1 = db.Queryable<OTB_OPM_ImportExhibition>()
            .Where(x => x.ModifyDate <= SqlFunc.ToDate(sEndDate) && x.ModifyDate >= SqlFunc.ToDate(sStartDate))
            .ToList();


            foreach (OTB_OPM_ImportExhibition import in data1)
            {
                if (ExcImport || BillsNo.Any())
                {
                    //解析bill
                    if (!string.IsNullOrEmpty(import.Bills))
                    {
                        JArray ja = JArray.Parse(import.Bills);

                        if (ja.Count > 0)
                        {
                            var sdb = new SimpleClient<OTB_OPM_ImportExhibition>(db);
                            var oOpm = sdb.GetById(import.ImportBillNO);
                            if (oOpm == null)
                            {
                                sMsg = $"進口單號：{ import.ImportBillNO } => 系統找不到對應的基本資料，請核查！";
                                break;
                            }

                            foreach (JObject jo in ja.OfType<JObject>())
                            {
                                string sBillNo = GetObjectValue(jo, "BillNO");
                                string sAuditVal = GetObjectValue(jo, "AuditVal");
                                string sBillCheckDate = GetObjectValue(jo, "BillCheckDate");
                                string sPayer = GetObjectValue(jo, "Payer");
                                string sUrl = @"ExhibitionImport_Upd|?Action=Upd&GoTab=3&ImportBillNO=" + oOpm.ImportBillNO + @"&BillNO=" + sBillNo;

                                if (BillsNo.Any() && !BillsNo.Contains(sBillNo))
                                {
                                    continue;
                                }

                                DateTime dtCreateDate;
                                if (DateTime.TryParse(sBillCheckDate, out dtCreateDate))
                                {
                                    if (dtCreateDate <= dtSettingEnd && dtCreateDate >= dtSettingStartDate)
                                    {
                                        var oPayer = new OTB_CRM_Customers();
                                        if (!string.IsNullOrEmpty(sPayer))
                                        {
                                            oPayer = db.Queryable<OTB_CRM_Customers>().Single(it => it.guid == sPayer);
                                            if (oPayer == null)
                                            {
                                                sMsg = $"進口單號：{ import.ImportBillNO } => 系統找不到付款人資訊";
                                                break;
                                            }
                                        }

                                        //狀態4(已銷帳)/6(已作廢)當成已審核拋轉
                                        if (sAuditVal == "2" || sAuditVal == "4" || sAuditVal == "6")
                                        {
                                            #region 審核

                                            var oBillsAdd = new OTB_OPM_Bills
                                            {
                                                OrgID = import.OrgID,
                                                BillNO = sBillNo, //帳款號碼(1)
                                                CheckDate = Common.FnToTWDate(GetObjectValue(jo, "BillFirstCheckDate")),//對帳日期(2)
                                                BillType = @"20",//帳別(收付)(3)
                                                CustomerCode = oPayer.CustomerNO //客戶供應商代號(4)
                                            };

                                            var sResponsiblePerson = oOpm.ResponsiblePerson.Split('.')[0];
                                            oBillsAdd.ResponsiblePersonCode = Common.CutByteString(sResponsiblePerson, 11); //業務員代號(5)
                                            oBillsAdd.ResponsiblePersonFullCode = oOpm.ResponsiblePerson; //業務員全代號
                                            oBillsAdd.LastGetBillDate = @""; //最近收付日(6)
                                            oBillsAdd.LastGetBillNO = @""; //最近收付單號(7)
                                            oBillsAdd.TaxType = decimal.Parse(GetObjectValue(jo, "TaxSum").Replace(@",", @"")) > 0 ? @"5" : @"6";//稅別(8)
                                            oBillsAdd.NOTaxAmount = GetObjectValue(jo, "AmountSum"); //未稅金額(9)
                                            oBillsAdd.BillAmount = GetObjectValue(jo, "AmountTaxSum"); //帳款金額(10)
                                            oBillsAdd.PaymentAmount = @"0"; //收付金額(11)
                                            oBillsAdd.Allowance = @"0"; //折讓金額(12)
                                            oBillsAdd.DebtAmount = @"0"; //呆帳金額(13)
                                            oBillsAdd.ExchangeAmount = @"0"; //匯兌損益金額(14)
                                            oBillsAdd.Settle = @"N"; //結清否(15)
                                            oBillsAdd.InvoiceStartNumber = @""; //發票號碼(起)(16)
                                            oBillsAdd.InvoiceEndNumber = @"";//發票號碼(迄)(17)
                                            oBillsAdd.Category = @"";//傳票類別(18)
                                            oBillsAdd.OrderNo = @"";//訂單單號(19)
                                            oBillsAdd.ClosingNote = @"N"; //結帳註記(20)
                                            oBillsAdd.GeneralInvoiceNumber = @""; //立帳總帳傳票號碼(21)
                                            oBillsAdd.GeneralSerialNumber = @""; //立帳總帳傳票序號(22)
                                            oBillsAdd.Remark1 = @""; //備註一(30C)(23)
                                            oBillsAdd.AccountSource = @"0"; //帳款來源(24)
                                            oBillsAdd.UpdateDate = @""; //更新日期(25)
                                            oBillsAdd.UpdatePersonnel = @""; //更新人員(26)
                                            oBillsAdd.DepartmentSiteNumber = oOpm.DepartmentID; //部門\ 工地編號(27)
                                            if (!string.IsNullOrWhiteSpace(oOpm.ExhibitionNO))
                                            {
                                                var oExhibition = db.Queryable<OTB_OPM_Exhibition>().Single(it => it.SN == oOpm.ExhibitionNO.ObjToInt());
                                                oBillsAdd.ProjectNumber = oExhibition == null ? @"" : oExhibition.ExhibitionCode; //專案\ 項目編號(28)
                                            }
                                            else
                                            {
                                                oBillsAdd.ProjectNumber = @""; //專案\ 項目編號(28)
                                            }
                                            oBillsAdd.TransferBNotes = @""; //轉B 帳註記(29)
                                            oBillsAdd.ABNumber = @""; //A|B 帳唯一流水號(30)
                                            oBillsAdd.EnterNumber = @""; //進銷單號(31)
                                            var sCurrency = GetObjectValue(jo, "Currency");
                                            if (import.OrgID == "SG")
                                            {
                                                if (sCurrency == "RMB")
                                                {
                                                    sCurrency = "";
                                                }
                                            }
                                            else
                                            {
                                                if (sCurrency == "NTD")
                                                {
                                                    sCurrency = "";
                                                }
                                            }
                                            oBillsAdd.ForeignCurrencyCode = sCurrency; //外幣代號(32)
                                            var sExchangeRate = GetObjectValue(jo, "ExchangeRate");
                                            oBillsAdd.ExchangeRate = sExchangeRate; //匯率(33)
                                            oBillsAdd.ForeignAmount = (decimal.Parse(GetObjectValue(jo, "AmountTaxSum")) * decimal.Parse(sExchangeRate == @"" ? @"1" : sExchangeRate)).ToString(); //外幣金額(34)
                                            oBillsAdd.PayAmount = @"0"; //收付沖抵金額(35)
                                            oBillsAdd.RefundAmount = @"0"; //退款金額(36)
                                            oBillsAdd.PaymentTerms = @""; //收付條件(37)
                                            oBillsAdd.AccountDate = Common.FnToTWDate(GetObjectValue(jo, "BillFirstCheckDate")); //帳款日期(38)
                                            oBillsAdd.DCreditCardNumber = @""; //預設信用卡號(39)
                                            oBillsAdd.ClosingDate = @""; //結帳日期(40)
                                            oBillsAdd.CusField1 = @""; //自定義欄位一(41)
                                            oBillsAdd.CusField2 = @""; //自定義欄位二(42)
                                            oBillsAdd.CusField3 = @""; //自定義欄位三(43)
                                            oBillsAdd.CusField4 = @""; //自定義欄位四(44)
                                            oBillsAdd.CusField5 = @""; //自定義欄位五(45)
                                            oBillsAdd.CusField6 = @"0"; //自定義欄位六(46)
                                            oBillsAdd.CusField7 = @"0"; //自定義欄位七(47)
                                            oBillsAdd.CusField8 = @"0"; //自定義欄位八(48)
                                            oBillsAdd.CusField9 = @"0"; //自定義欄位九(49)
                                            oBillsAdd.CusField10 = @"0";  //自定義欄位十(50)
                                            oBillsAdd.CusField11 = @""; //自定義欄位十一(51)
                                            oBillsAdd.CusField12 = @""; //自定義欄位十二(52)
                                            oBillsAdd.Remark2 = @""; //備註二(M)(53)
                                            oBillsAdd.TWNOTaxAmount = GetObjectValue(jo, "AmountSum"); //台幣未稅金額(54)
                                            oBillsAdd.TWAmount = GetObjectValue(jo, "AmountTaxSum"); //台幣帳款金額(55)
                                            oBillsAdd.CreateUser = import.ModifyUser;
                                            oBillsAdd.CreateDate = ((DateTime)import.ModifyDate).ToString(@"yyyy/MM/dd HH:mm:ss");
                                            oBillsAdd.BillFirstCheckDate = string.IsNullOrEmpty(GetObjectValue(jo, "BillFirstCheckDate")) ? DateTime.Now.ToString(@"yyyy/MM/dd HH:mm:ss") : GetObjectValue(jo, "BillFirstCheckDate");
                                            oBillsAdd.Advance = GetObjectValue(jo, "Advance"); //預收
                                            oBillsAdd.TaxSum = GetObjectValue(jo, "TaxSum"); //稅額
                                            oBillsAdd.TotalReceivable = GetObjectValue(jo, "TotalReceivable"); //總應收
                                            oBillsAdd.IsRetn = string.IsNullOrEmpty(GetObjectValue(jo, "IsRetn")) ? @"N" : GetObjectValue(jo, "IsRetn"); //是否為退運
                                            oBillsAdd.Url = sUrl; //Url
                                            db.Insertable(oBillsAdd).ExecuteCommand();

                                            if (dicSucceccCount.Keys.Contains("進口審核"))
                                            {
                                                dicSucceccCount["進口審核"] = dicSucceccCount["進口審核"] + 1;
                                            }
                                            else
                                            {
                                                dicSucceccCount.Add("進口審核", 1);
                                            }

                                            #endregion
                                        }
                                        else if (sAuditVal == "5")
                                        {
                                            #region 過帳

                                            var oBillsAdd = new OTB_OPM_Bills
                                            {
                                                OrgID = import.OrgID,
                                                BillNO = sBillNo, //帳款號碼(1)
                                                CheckDate = Common.FnToTWDate(GetObjectValue(jo, "BillFirstCheckDate")),//對帳日期(2)
                                                BillType = @"20",//帳別(收付)(3)
                                                CustomerCode = oPayer.CustomerNO //客戶供應商代號(4)
                                            };
                                            var sResponsiblePerson = oOpm.ResponsiblePerson.Split('.')[0];
                                            oBillsAdd.ResponsiblePersonCode = Common.CutByteString(sResponsiblePerson, 11); //業務員代號(5)
                                            oBillsAdd.ResponsiblePersonFullCode = oOpm.ResponsiblePerson; //業務員全代號
                                            oBillsAdd.LastGetBillDate = @""; //最近收付日(6)
                                            oBillsAdd.LastGetBillNO = @""; //最近收付單號(7)
                                            oBillsAdd.TaxType = decimal.Parse(GetObjectValue(jo, "TaxSum").Replace(@",", @"")) > 0 ? @"5" : @"6";//稅別(8)
                                            oBillsAdd.NOTaxAmount = GetObjectValue(jo, "AmountSum"); //未稅金額(9)
                                            oBillsAdd.BillAmount = GetObjectValue(jo, "AmountTaxSum"); //帳款金額(10)
                                            oBillsAdd.PaymentAmount = @"0"; //收付金額(11)
                                            oBillsAdd.Allowance = @"0"; //折讓金額(12)
                                            oBillsAdd.DebtAmount = @"0"; //呆帳金額(13)
                                            oBillsAdd.ExchangeAmount = @"0"; //匯兌損益金額(14)
                                            oBillsAdd.Settle = @"N"; //結清否(15)
                                            oBillsAdd.InvoiceStartNumber = GetObjectValue(jo, "InvoiceNumber"); //發票號碼(起)(16)
                                            oBillsAdd.InvoiceEndNumber = GetObjectValue(jo, "InvoiceNumber");//發票號碼(迄)(17)
                                            oBillsAdd.Category = @"";//傳票類別(18)
                                            oBillsAdd.OrderNo = @"";//訂單單號(19)
                                            oBillsAdd.ClosingNote = @"N"; //結帳註記(20)
                                            oBillsAdd.GeneralInvoiceNumber = @""; //立帳總帳傳票號碼(21)
                                            oBillsAdd.GeneralSerialNumber = @""; //立帳總帳傳票序號(22)
                                            oBillsAdd.Remark1 = @""; //備註一(30C)(23)
                                            oBillsAdd.AccountSource = @"0"; //帳款來源(24)
                                            oBillsAdd.UpdateDate = @""; //更新日期(25)
                                            oBillsAdd.UpdatePersonnel = @""; //更新人員(26)
                                            oBillsAdd.DepartmentSiteNumber = oOpm.DepartmentID; //部門\ 工地編號(27)
                                            if (!string.IsNullOrWhiteSpace(oOpm.ExhibitionNO))
                                            {
                                                var oExhibition = db.Queryable<OTB_OPM_Exhibition>().Single(it => it.SN == oOpm.ExhibitionNO.ObjToInt());
                                                oBillsAdd.ProjectNumber = oExhibition == null ? @"" : oExhibition.ExhibitionCode; //專案\ 項目編號(28)
                                            }
                                            else
                                            {
                                                oBillsAdd.ProjectNumber = @""; //專案\ 項目編號(28)
                                            }
                                            oBillsAdd.TransferBNotes = @""; //轉B 帳註記(29)
                                            oBillsAdd.ABNumber = @""; //A|B 帳唯一流水號(30)
                                            oBillsAdd.EnterNumber = @""; //進銷單號(31)
                                            var sCurrency = GetObjectValue(jo, "Currency");
                                            if (import.OrgID == "SG")
                                            {
                                                if (sCurrency == "RMB")
                                                {
                                                    sCurrency = "";
                                                }
                                            }
                                            else
                                            {
                                                if (sCurrency == "NTD")
                                                {
                                                    sCurrency = "";
                                                }
                                            }
                                            oBillsAdd.ForeignCurrencyCode = sCurrency; //外幣代號(32)
                                            var sExchangeRate = GetObjectValue(jo, "ExchangeRate");
                                            oBillsAdd.ExchangeRate = sExchangeRate; //匯率(33)
                                            oBillsAdd.ForeignAmount = (decimal.Parse(GetObjectValue(jo, "AmountTaxSum")) * decimal.Parse(sExchangeRate == @"" ? @"1" : sExchangeRate)).ToString(); //外幣金額(34)
                                            oBillsAdd.PayAmount = @"0"; //收付沖抵金額(35)
                                            oBillsAdd.RefundAmount = @"0"; //退款金額(36)
                                            oBillsAdd.PaymentTerms = @""; //收付條件(37)
                                            oBillsAdd.AccountDate = Common.FnToTWDate(GetObjectValue(jo, "BillFirstCheckDate")); //帳款日期(38)
                                            oBillsAdd.DCreditCardNumber = @""; //預設信用卡號(39)
                                            oBillsAdd.ClosingDate = @""; //結帳日期(40)
                                            oBillsAdd.CusField1 = @""; //自定義欄位一(41)
                                            oBillsAdd.CusField2 = @""; //自定義欄位二(42)
                                            oBillsAdd.CusField3 = @""; //自定義欄位三(43)
                                            oBillsAdd.CusField4 = @""; //自定義欄位四(44)
                                            oBillsAdd.CusField5 = @""; //自定義欄位五(45)
                                            oBillsAdd.CusField6 = @"0"; //自定義欄位六(46)
                                            oBillsAdd.CusField7 = @"0"; //自定義欄位七(47)
                                            oBillsAdd.CusField8 = @"0"; //自定義欄位八(48)
                                            oBillsAdd.CusField9 = @"0"; //自定義欄位九(49)
                                            oBillsAdd.CusField10 = @"0";  //自定義欄位十(50)
                                            oBillsAdd.CusField11 = @""; //自定義欄位十一(51)
                                            oBillsAdd.CusField12 = @""; //自定義欄位十二(52)
                                            oBillsAdd.Remark2 = @""; //備註二(M)(53)
                                            oBillsAdd.TWNOTaxAmount = GetObjectValue(jo, "AmountSum"); //台幣未稅金額(54)
                                            oBillsAdd.TWAmount = GetObjectValue(jo, "AmountTaxSum"); //台幣帳款金額(55)
                                            oBillsAdd.CreateUser = import.ModifyUser;
                                            oBillsAdd.CreateDate = ((DateTime)import.ModifyDate).ToString(@"yyyy/MM/dd HH:mm:ss");
                                            oBillsAdd.BillFirstCheckDate = string.IsNullOrEmpty(GetObjectValue(jo, "BillFirstCheckDate")) ? DateTime.Now.ToString(@"yyyy/MM/dd HH:mm:ss") : GetObjectValue(jo, "BillFirstCheckDate");
                                            oBillsAdd.Advance = GetObjectValue(jo, "Advance"); //預收
                                            oBillsAdd.TaxSum = GetObjectValue(jo, "TaxSum"); //稅額
                                            oBillsAdd.TotalReceivable = GetObjectValue(jo, "TotalReceivable"); //總應收
                                            oBillsAdd.IsRetn = string.IsNullOrEmpty(GetObjectValue(jo, "IsRetn")) ? @"N" : GetObjectValue(jo, "IsRetn"); //是否為退運
                                            oBillsAdd.Url = sUrl; //Url
                                            db.Insertable(oBillsAdd).ExecuteCommand();

                                            if (dicSucceccCount.Keys.Contains("進口過帳"))
                                            {
                                                dicSucceccCount["進口過帳"] = dicSucceccCount["進口過帳"] + 1;
                                            }
                                            else
                                            {
                                                dicSucceccCount.Add("進口過帳", 1);
                                            }

                                            #endregion
                                        }
                                    }
                                }
                            }

                        }

                    }
                }

                if (ExcImportReturn || BillsNo.Any())
                {
                    //解析ReturnBill
                    JArray ja = JArray.Parse(import.ReturnBills);

                    if (ja.Count > 0)
                    {
                        var sdb = new SimpleClient<OTB_OPM_ImportExhibition>(db);
                        var oOpm = sdb.GetById(import.ImportBillNO);
                        if (oOpm == null)
                        {
                            sMsg = $"退運進口單號：{ import.ImportBillNO } => 系統找不到對應的基本資料，請核查！";
                            break;
                        }

                        foreach (JObject joH in ja.OfType<JObject>())
                        {
                            JArray ja1 = JArray.Parse(GetObjectValue(joH, "Bills"));

                            if (ja1.Count > 0)
                            {
                                foreach (JObject jo in ja1.OfType<JObject>())
                                {
                                    string sBillNo = GetObjectValue(jo, "BillNO");
                                    string sAuditVal = GetObjectValue(jo, "AuditVal");
                                    string sBillCheckDate = GetObjectValue(jo, "BillCheckDate");
                                    string sPayer = GetObjectValue(jo, "Payer");
                                    string sUrl = @"ExhibitionImport_Upd|?Action=Upd&GoTab=5&ImportBillNO=" + oOpm.ImportBillNO + @"&BillNO=" + sBillNo;

                                    if (BillsNo.Any() && !BillsNo.Contains(sBillNo))
                                    {
                                        continue;
                                    }

                                    DateTime dtCreateDate;
                                    if (DateTime.TryParse(sBillCheckDate, out dtCreateDate))
                                    {
                                        if (dtCreateDate <= dtSettingEnd && dtCreateDate >= dtSettingStartDate)
                                        {
                                            var oPayer = new OTB_CRM_Customers();
                                            if (!string.IsNullOrEmpty(sPayer))
                                            {
                                                oPayer = db.Queryable<OTB_CRM_Customers>().Single(it => it.guid == sPayer);
                                                if (oPayer == null)
                                                {
                                                    sMsg = $"退運進口單號：{ import.ImportBillNO } => 系統找不到付款人資訊";
                                                    break;
                                                }
                                            }

                                            //狀態4(已銷帳)/6(已作廢)當成已審核拋轉
                                            if (sAuditVal == "2" || sAuditVal == "4" || sAuditVal == "6")
                                            {
                                                #region 審核

                                                var oBillsAdd = new OTB_OPM_Bills
                                                {
                                                    OrgID = import.OrgID,
                                                    BillNO = sBillNo, //帳款號碼(1)
                                                    CheckDate = Common.FnToTWDate(GetObjectValue(jo, "BillFirstCheckDate")),//對帳日期(2)
                                                    BillType = @"20",//帳別(收付)(3)
                                                    CustomerCode = oPayer.CustomerNO //客戶供應商代號(4)
                                                };

                                                var sResponsiblePerson = oOpm.ResponsiblePerson.Split('.')[0];
                                                oBillsAdd.ResponsiblePersonCode = Common.CutByteString(sResponsiblePerson, 11); //業務員代號(5)
                                                oBillsAdd.ResponsiblePersonFullCode = oOpm.ResponsiblePerson; //業務員全代號
                                                oBillsAdd.LastGetBillDate = @""; //最近收付日(6)
                                                oBillsAdd.LastGetBillNO = @""; //最近收付單號(7)
                                                oBillsAdd.TaxType = decimal.Parse(GetObjectValue(jo, "TaxSum").Replace(@",", @"")) > 0 ? @"5" : @"6";//稅別(8)
                                                oBillsAdd.NOTaxAmount = GetObjectValue(jo, "AmountSum"); //未稅金額(9)
                                                oBillsAdd.BillAmount = GetObjectValue(jo, "AmountTaxSum"); //帳款金額(10)
                                                oBillsAdd.PaymentAmount = @"0"; //收付金額(11)
                                                oBillsAdd.Allowance = @"0"; //折讓金額(12)
                                                oBillsAdd.DebtAmount = @"0"; //呆帳金額(13)
                                                oBillsAdd.ExchangeAmount = @"0"; //匯兌損益金額(14)
                                                oBillsAdd.Settle = @"N"; //結清否(15)
                                                oBillsAdd.InvoiceStartNumber = @""; //發票號碼(起)(16)
                                                oBillsAdd.InvoiceEndNumber = @"";//發票號碼(迄)(17)
                                                oBillsAdd.Category = @"";//傳票類別(18)
                                                oBillsAdd.OrderNo = @"";//訂單單號(19)
                                                oBillsAdd.ClosingNote = @"N"; //結帳註記(20)
                                                oBillsAdd.GeneralInvoiceNumber = @""; //立帳總帳傳票號碼(21)
                                                oBillsAdd.GeneralSerialNumber = @""; //立帳總帳傳票序號(22)
                                                oBillsAdd.Remark1 = @""; //備註一(30C)(23)
                                                oBillsAdd.AccountSource = @"0"; //帳款來源(24)
                                                oBillsAdd.UpdateDate = @""; //更新日期(25)
                                                oBillsAdd.UpdatePersonnel = @""; //更新人員(26)
                                                oBillsAdd.DepartmentSiteNumber = oOpm.DepartmentID; //部門\ 工地編號(27)
                                                if (!string.IsNullOrWhiteSpace(oOpm.ExhibitionNO))
                                                {
                                                    var oExhibition = db.Queryable<OTB_OPM_Exhibition>().Single(it => it.SN == oOpm.ExhibitionNO.ObjToInt());
                                                    oBillsAdd.ProjectNumber = oExhibition == null ? @"" : oExhibition.ExhibitionCode; //專案\ 項目編號(28)
                                                }
                                                else
                                                {
                                                    oBillsAdd.ProjectNumber = @""; //專案\ 項目編號(28)
                                                }
                                                oBillsAdd.TransferBNotes = @""; //轉B 帳註記(29)
                                                oBillsAdd.ABNumber = @""; //A|B 帳唯一流水號(30)
                                                oBillsAdd.EnterNumber = @""; //進銷單號(31)
                                                var sCurrency = GetObjectValue(jo, "Currency");
                                                if (import.OrgID == "SG")
                                                {
                                                    if (sCurrency == "RMB")
                                                    {
                                                        sCurrency = "";
                                                    }
                                                }
                                                else
                                                {
                                                    if (sCurrency == "NTD")
                                                    {
                                                        sCurrency = "";
                                                    }
                                                }
                                                oBillsAdd.ForeignCurrencyCode = sCurrency; //外幣代號(32)
                                                var sExchangeRate = GetObjectValue(jo, "ExchangeRate");
                                                oBillsAdd.ExchangeRate = sExchangeRate; //匯率(33)
                                                oBillsAdd.ForeignAmount = (decimal.Parse(GetObjectValue(jo, "AmountTaxSum")) * decimal.Parse(sExchangeRate == @"" ? @"1" : sExchangeRate)).ToString(); //外幣金額(34)
                                                oBillsAdd.PayAmount = @"0"; //收付沖抵金額(35)
                                                oBillsAdd.RefundAmount = @"0"; //退款金額(36)
                                                oBillsAdd.PaymentTerms = @""; //收付條件(37)
                                                oBillsAdd.AccountDate = Common.FnToTWDate(GetObjectValue(jo, "BillFirstCheckDate")); //帳款日期(38)
                                                oBillsAdd.DCreditCardNumber = @""; //預設信用卡號(39)
                                                oBillsAdd.ClosingDate = @""; //結帳日期(40)
                                                oBillsAdd.CusField1 = @""; //自定義欄位一(41)
                                                oBillsAdd.CusField2 = @""; //自定義欄位二(42)
                                                oBillsAdd.CusField3 = @""; //自定義欄位三(43)
                                                oBillsAdd.CusField4 = @""; //自定義欄位四(44)
                                                oBillsAdd.CusField5 = @""; //自定義欄位五(45)
                                                oBillsAdd.CusField6 = @"0"; //自定義欄位六(46)
                                                oBillsAdd.CusField7 = @"0"; //自定義欄位七(47)
                                                oBillsAdd.CusField8 = @"0"; //自定義欄位八(48)
                                                oBillsAdd.CusField9 = @"0"; //自定義欄位九(49)
                                                oBillsAdd.CusField10 = @"0";  //自定義欄位十(50)
                                                oBillsAdd.CusField11 = @""; //自定義欄位十一(51)
                                                oBillsAdd.CusField12 = @""; //自定義欄位十二(52)
                                                oBillsAdd.Remark2 = @""; //備註二(M)(53)
                                                oBillsAdd.TWNOTaxAmount = GetObjectValue(jo, "AmountSum"); //台幣未稅金額(54)
                                                oBillsAdd.TWAmount = GetObjectValue(jo, "AmountTaxSum"); //台幣帳款金額(55)
                                                oBillsAdd.CreateUser = import.ModifyUser;
                                                oBillsAdd.CreateDate = ((DateTime)import.ModifyDate).ToString(@"yyyy/MM/dd HH:mm:ss");
                                                oBillsAdd.BillFirstCheckDate = string.IsNullOrEmpty(GetObjectValue(jo, "BillFirstCheckDate")) ? DateTime.Now.ToString(@"yyyy/MM/dd HH:mm:ss") : GetObjectValue(jo, "BillFirstCheckDate");
                                                oBillsAdd.Advance = GetObjectValue(jo, "Advance"); //預收
                                                oBillsAdd.TaxSum = GetObjectValue(jo, "TaxSum"); //稅額
                                                oBillsAdd.TotalReceivable = GetObjectValue(jo, "TotalReceivable"); //總應收
                                                oBillsAdd.IsRetn = string.IsNullOrEmpty(GetObjectValue(jo, "IsRetn")) ? @"N" : GetObjectValue(jo, "IsRetn"); //是否為退運
                                                oBillsAdd.Url = sUrl; //Url
                                                db.Insertable(oBillsAdd).ExecuteCommand();

                                                if (dicSucceccCount.Keys.Contains("退運進口審核"))
                                                {
                                                    dicSucceccCount["退運進口審核"] = dicSucceccCount["退運進口審核"] + 1;
                                                }
                                                else
                                                {
                                                    dicSucceccCount.Add("退運進口審核", 1);
                                                }

                                                #endregion
                                            }
                                            else if (sAuditVal == "5")
                                            {
                                                #region 過帳

                                                var oBillsAdd = new OTB_OPM_Bills
                                                {
                                                    OrgID = import.OrgID,
                                                    BillNO = sBillNo, //帳款號碼(1)
                                                    CheckDate = Common.FnToTWDate(GetObjectValue(jo, "BillFirstCheckDate")),//對帳日期(2)
                                                    BillType = @"20",//帳別(收付)(3)
                                                    CustomerCode = oPayer.CustomerNO //客戶供應商代號(4)
                                                };
                                                var sResponsiblePerson = oOpm.ResponsiblePerson.Split('.')[0];
                                                oBillsAdd.ResponsiblePersonCode = Common.CutByteString(sResponsiblePerson, 11); //業務員代號(5)
                                                oBillsAdd.ResponsiblePersonFullCode = oOpm.ResponsiblePerson; //業務員全代號
                                                oBillsAdd.LastGetBillDate = @""; //最近收付日(6)
                                                oBillsAdd.LastGetBillNO = @""; //最近收付單號(7)
                                                oBillsAdd.TaxType = decimal.Parse(GetObjectValue(jo, "TaxSum").Replace(@",", @"")) > 0 ? @"5" : @"6";//稅別(8)
                                                oBillsAdd.NOTaxAmount = GetObjectValue(jo, "AmountSum"); //未稅金額(9)
                                                oBillsAdd.BillAmount = GetObjectValue(jo, "AmountTaxSum"); //帳款金額(10)
                                                oBillsAdd.PaymentAmount = @"0"; //收付金額(11)
                                                oBillsAdd.Allowance = @"0"; //折讓金額(12)
                                                oBillsAdd.DebtAmount = @"0"; //呆帳金額(13)
                                                oBillsAdd.ExchangeAmount = @"0"; //匯兌損益金額(14)
                                                oBillsAdd.Settle = @"N"; //結清否(15)
                                                oBillsAdd.InvoiceStartNumber = GetObjectValue(jo, "InvoiceNumber"); //發票號碼(起)(16)
                                                oBillsAdd.InvoiceEndNumber = GetObjectValue(jo, "InvoiceNumber");//發票號碼(迄)(17)
                                                oBillsAdd.Category = @"";//傳票類別(18)
                                                oBillsAdd.OrderNo = @"";//訂單單號(19)
                                                oBillsAdd.ClosingNote = @"N"; //結帳註記(20)
                                                oBillsAdd.GeneralInvoiceNumber = @""; //立帳總帳傳票號碼(21)
                                                oBillsAdd.GeneralSerialNumber = @""; //立帳總帳傳票序號(22)
                                                oBillsAdd.Remark1 = @""; //備註一(30C)(23)
                                                oBillsAdd.AccountSource = @"0"; //帳款來源(24)
                                                oBillsAdd.UpdateDate = @""; //更新日期(25)
                                                oBillsAdd.UpdatePersonnel = @""; //更新人員(26)
                                                oBillsAdd.DepartmentSiteNumber = oOpm.DepartmentID; //部門\ 工地編號(27)
                                                if (!string.IsNullOrWhiteSpace(oOpm.ExhibitionNO))
                                                {
                                                    var oExhibition = db.Queryable<OTB_OPM_Exhibition>().Single(it => it.SN == oOpm.ExhibitionNO.ObjToInt());
                                                    oBillsAdd.ProjectNumber = oExhibition == null ? @"" : oExhibition.ExhibitionCode; //專案\ 項目編號(28)
                                                }
                                                else
                                                {
                                                    oBillsAdd.ProjectNumber = @""; //專案\ 項目編號(28)
                                                }
                                                oBillsAdd.TransferBNotes = @""; //轉B 帳註記(29)
                                                oBillsAdd.ABNumber = @""; //A|B 帳唯一流水號(30)
                                                oBillsAdd.EnterNumber = @""; //進銷單號(31)
                                                var sCurrency = GetObjectValue(jo, "Currency");
                                                if (import.OrgID == "SG")
                                                {
                                                    if (sCurrency == "RMB")
                                                    {
                                                        sCurrency = "";
                                                    }
                                                }
                                                else
                                                {
                                                    if (sCurrency == "NTD")
                                                    {
                                                        sCurrency = "";
                                                    }
                                                }
                                                oBillsAdd.ForeignCurrencyCode = sCurrency; //外幣代號(32)
                                                var sExchangeRate = GetObjectValue(jo, "ExchangeRate");
                                                oBillsAdd.ExchangeRate = sExchangeRate; //匯率(33)
                                                oBillsAdd.ForeignAmount = (decimal.Parse(GetObjectValue(jo, "AmountTaxSum")) * decimal.Parse(sExchangeRate == @"" ? @"1" : sExchangeRate)).ToString(); //外幣金額(34)
                                                oBillsAdd.PayAmount = @"0"; //收付沖抵金額(35)
                                                oBillsAdd.RefundAmount = @"0"; //退款金額(36)
                                                oBillsAdd.PaymentTerms = @""; //收付條件(37)
                                                oBillsAdd.AccountDate = Common.FnToTWDate(GetObjectValue(jo, "BillFirstCheckDate")); //帳款日期(38)
                                                oBillsAdd.DCreditCardNumber = @""; //預設信用卡號(39)
                                                oBillsAdd.ClosingDate = @""; //結帳日期(40)
                                                oBillsAdd.CusField1 = @""; //自定義欄位一(41)
                                                oBillsAdd.CusField2 = @""; //自定義欄位二(42)
                                                oBillsAdd.CusField3 = @""; //自定義欄位三(43)
                                                oBillsAdd.CusField4 = @""; //自定義欄位四(44)
                                                oBillsAdd.CusField5 = @""; //自定義欄位五(45)
                                                oBillsAdd.CusField6 = @"0"; //自定義欄位六(46)
                                                oBillsAdd.CusField7 = @"0"; //自定義欄位七(47)
                                                oBillsAdd.CusField8 = @"0"; //自定義欄位八(48)
                                                oBillsAdd.CusField9 = @"0"; //自定義欄位九(49)
                                                oBillsAdd.CusField10 = @"0";  //自定義欄位十(50)
                                                oBillsAdd.CusField11 = @""; //自定義欄位十一(51)
                                                oBillsAdd.CusField12 = @""; //自定義欄位十二(52)
                                                oBillsAdd.Remark2 = @""; //備註二(M)(53)
                                                oBillsAdd.TWNOTaxAmount = GetObjectValue(jo, "AmountSum"); //台幣未稅金額(54)
                                                oBillsAdd.TWAmount = GetObjectValue(jo, "AmountTaxSum"); //台幣帳款金額(55)
                                                oBillsAdd.CreateUser = import.ModifyUser;
                                                oBillsAdd.CreateDate = ((DateTime)import.ModifyDate).ToString(@"yyyy/MM/dd HH:mm:ss");
                                                oBillsAdd.BillFirstCheckDate = string.IsNullOrEmpty(GetObjectValue(jo, "BillFirstCheckDate")) ? DateTime.Now.ToString(@"yyyy/MM/dd HH:mm:ss") : GetObjectValue(jo, "BillFirstCheckDate");
                                                oBillsAdd.Advance = GetObjectValue(jo, "Advance"); //預收
                                                oBillsAdd.TaxSum = GetObjectValue(jo, "TaxSum"); //稅額
                                                oBillsAdd.TotalReceivable = GetObjectValue(jo, "TotalReceivable"); //總應收
                                                oBillsAdd.IsRetn = string.IsNullOrEmpty(GetObjectValue(jo, "IsRetn")) ? @"N" : GetObjectValue(jo, "IsRetn"); //是否為退運
                                                oBillsAdd.Url = sUrl; //Url
                                                db.Insertable(oBillsAdd).ExecuteCommand();

                                                if (dicSucceccCount.Keys.Contains("退運進口過帳"))
                                                {
                                                    dicSucceccCount["退運進口過帳"] = dicSucceccCount["退運進口過帳"] + 1;
                                                }
                                                else
                                                {
                                                    dicSucceccCount.Add("退運進口過帳", 1);
                                                }

                                                #endregion
                                            }
                                        }
                                    }
                                }
                            }
                        }

                    }
                }

            }

            #endregion

            return sMsg;
        }

        public string Export(string sStartDate, string sEndDate, SqlSugarClient db)
        {
            string sMsg = null;
            DateTime dtSettingStartDate = Convert.ToDateTime(sStartDate);
            DateTime dtSettingEnd = Convert.ToDateTime(sEndDate);

            #region 出口

            var data2 = db.Queryable<OTB_OPM_ExportExhibition>()
            .Where(x => x.ModifyDate <= SqlFunc.ToDate(sEndDate) && x.ModifyDate >= SqlFunc.ToDate(sStartDate))
            .ToList();


            foreach (OTB_OPM_ExportExhibition export in data2)
            {
                if (ExcExport || BillsNo.Any())
                {
                    //解析bill
                    if (!string.IsNullOrEmpty(export.Bills))
                    {
                        JArray ja = JArray.Parse(export.Bills);

                        if (ja.Count > 0)
                        {
                            var sdb = new SimpleClient<OTB_OPM_ExportExhibition>(db);
                            var oOpm = sdb.GetById(export.ExportBillNO);
                            if (oOpm == null)
                            {
                                sMsg = $"出口單號：{ export.ExportBillNO } => 系統找不到對應的基本資料，請核查！";
                                break;
                            }

                            foreach (JObject jo in ja.OfType<JObject>())
                            {
                                string sBillNo = GetObjectValue(jo, "BillNO");
                                string sAuditVal = GetObjectValue(jo, "AuditVal");
                                string sBillCheckDate = GetObjectValue(jo, "BillCheckDate");
                                string sPayer = GetObjectValue(jo, "Payer");
                                string sUrl = @"ExhibitionExport_Upd|?Action=Upd&GoTab=3&ExportBillNO=" + oOpm.ExportBillNO + @"&BillNO=" + sBillNo;
                                DateTime dtCreateDate;

                                if (BillsNo.Any() && !BillsNo.Contains(sBillNo))
                                {
                                    continue;
                                }

                                if (DateTime.TryParse(sBillCheckDate, out dtCreateDate))
                                {
                                    if (dtCreateDate <= dtSettingEnd && dtCreateDate >= dtSettingStartDate)
                                    {
                                        var oPayer = new OTB_CRM_Customers();
                                        if (!string.IsNullOrEmpty(sPayer))
                                        {
                                            oPayer = db.Queryable<OTB_CRM_Customers>().Single(it => it.guid == sPayer);
                                            if (oPayer == null)
                                            {
                                                sMsg = $"出口單號：{ export.ExportBillNO } => 系統找不到付款人資訊";
                                                break;
                                            }
                                        }

                                        //狀態4(已銷帳)/6(已作廢)當成已審核拋轉
                                        if (sAuditVal == "2" || sAuditVal == "4" || sAuditVal == "6")
                                        {
                                            #region 審核

                                            var oBillsAdd = new OTB_OPM_Bills
                                            {
                                                OrgID = export.OrgID,
                                                BillNO = sBillNo, //帳款號碼(1)
                                                CheckDate = Common.FnToTWDate(GetObjectValue(jo, "BillFirstCheckDate")),//對帳日期(2)
                                                BillType = @"20",//帳別(收付)(3)
                                                CustomerCode = oPayer.CustomerNO //客戶供應商代號(4)
                                            };

                                            var sResponsiblePerson = oOpm.ResponsiblePerson.Split('.')[0];
                                            oBillsAdd.ResponsiblePersonCode = Common.CutByteString(sResponsiblePerson, 11); //業務員代號(5)
                                            oBillsAdd.ResponsiblePersonFullCode = oOpm.ResponsiblePerson; //業務員全代號
                                            oBillsAdd.LastGetBillDate = @""; //最近收付日(6)
                                            oBillsAdd.LastGetBillNO = @""; //最近收付單號(7)
                                            oBillsAdd.TaxType = decimal.Parse(GetObjectValue(jo, "TaxSum").Replace(@",", @"")) > 0 ? @"5" : @"6";//稅別(8)
                                            oBillsAdd.NOTaxAmount = GetObjectValue(jo, "AmountSum"); //未稅金額(9)
                                            oBillsAdd.BillAmount = GetObjectValue(jo, "AmountTaxSum"); //帳款金額(10)
                                            oBillsAdd.PaymentAmount = @"0"; //收付金額(11)
                                            oBillsAdd.Allowance = @"0"; //折讓金額(12)
                                            oBillsAdd.DebtAmount = @"0"; //呆帳金額(13)
                                            oBillsAdd.ExchangeAmount = @"0"; //匯兌損益金額(14)
                                            oBillsAdd.Settle = @"N"; //結清否(15)
                                            oBillsAdd.InvoiceStartNumber = @""; //發票號碼(起)(16)
                                            oBillsAdd.InvoiceEndNumber = @"";//發票號碼(迄)(17)
                                            oBillsAdd.Category = @"";//傳票類別(18)
                                            oBillsAdd.OrderNo = @"";//訂單單號(19)
                                            oBillsAdd.ClosingNote = @"N"; //結帳註記(20)
                                            oBillsAdd.GeneralInvoiceNumber = @""; //立帳總帳傳票號碼(21)
                                            oBillsAdd.GeneralSerialNumber = @""; //立帳總帳傳票序號(22)
                                            oBillsAdd.Remark1 = @""; //備註一(30C)(23)
                                            oBillsAdd.AccountSource = @"0"; //帳款來源(24)
                                            oBillsAdd.UpdateDate = @""; //更新日期(25)
                                            oBillsAdd.UpdatePersonnel = @""; //更新人員(26)
                                            oBillsAdd.DepartmentSiteNumber = oOpm.DepartmentID; //部門\ 工地編號(27)
                                            if (!string.IsNullOrWhiteSpace(oOpm.ExhibitionNO))
                                            {
                                                var oExhibition = db.Queryable<OTB_OPM_Exhibition>().Single(it => it.SN == oOpm.ExhibitionNO.ObjToInt());
                                                oBillsAdd.ProjectNumber = oExhibition == null ? @"" : oExhibition.ExhibitionCode; //專案\ 項目編號(28)
                                            }
                                            else
                                            {
                                                oBillsAdd.ProjectNumber = @""; //專案\ 項目編號(28)
                                            }
                                            oBillsAdd.TransferBNotes = @""; //轉B 帳註記(29)
                                            oBillsAdd.ABNumber = @""; //A|B 帳唯一流水號(30)
                                            oBillsAdd.EnterNumber = @""; //進銷單號(31)
                                            var sCurrency = GetObjectValue(jo, "Currency");
                                            if (export.OrgID == "SG")
                                            {
                                                if (sCurrency == "RMB")
                                                {
                                                    sCurrency = "";
                                                }
                                            }
                                            else
                                            {
                                                if (sCurrency == "NTD")
                                                {
                                                    sCurrency = "";
                                                }
                                            }
                                            oBillsAdd.ForeignCurrencyCode = sCurrency; //外幣代號(32)
                                            var sExchangeRate = GetObjectValue(jo, "ExchangeRate");
                                            oBillsAdd.ExchangeRate = sExchangeRate; //匯率(33)
                                            oBillsAdd.ForeignAmount = (decimal.Parse(GetObjectValue(jo, "AmountTaxSum")) * decimal.Parse(sExchangeRate == @"" ? @"1" : sExchangeRate)).ToString(); //外幣金額(34)
                                            oBillsAdd.PayAmount = @"0"; //收付沖抵金額(35)
                                            oBillsAdd.RefundAmount = @"0"; //退款金額(36)
                                            oBillsAdd.PaymentTerms = @""; //收付條件(37)
                                            oBillsAdd.AccountDate = Common.FnToTWDate(GetObjectValue(jo, "BillFirstCheckDate")); //帳款日期(38)
                                            oBillsAdd.DCreditCardNumber = @""; //預設信用卡號(39)
                                            oBillsAdd.ClosingDate = @""; //結帳日期(40)
                                            oBillsAdd.CusField1 = @""; //自定義欄位一(41)
                                            oBillsAdd.CusField2 = @""; //自定義欄位二(42)
                                            oBillsAdd.CusField3 = @""; //自定義欄位三(43)
                                            oBillsAdd.CusField4 = @""; //自定義欄位四(44)
                                            oBillsAdd.CusField5 = @""; //自定義欄位五(45)
                                            oBillsAdd.CusField6 = @"0"; //自定義欄位六(46)
                                            oBillsAdd.CusField7 = @"0"; //自定義欄位七(47)
                                            oBillsAdd.CusField8 = @"0"; //自定義欄位八(48)
                                            oBillsAdd.CusField9 = @"0"; //自定義欄位九(49)
                                            oBillsAdd.CusField10 = @"0";  //自定義欄位十(50)
                                            oBillsAdd.CusField11 = @""; //自定義欄位十一(51)
                                            oBillsAdd.CusField12 = @""; //自定義欄位十二(52)
                                            oBillsAdd.Remark2 = @""; //備註二(M)(53)
                                            oBillsAdd.TWNOTaxAmount = GetObjectValue(jo, "AmountSum"); //台幣未稅金額(54)
                                            oBillsAdd.TWAmount = GetObjectValue(jo, "AmountTaxSum"); //台幣帳款金額(55)
                                            oBillsAdd.CreateUser = export.ModifyUser;
                                            oBillsAdd.CreateDate = ((DateTime)export.ModifyDate).ToString(@"yyyy/MM/dd HH:mm:ss");
                                            oBillsAdd.BillFirstCheckDate = string.IsNullOrEmpty(GetObjectValue(jo, "BillFirstCheckDate")) ? DateTime.Now.ToString(@"yyyy/MM/dd HH:mm:ss") : GetObjectValue(jo, "BillFirstCheckDate");
                                            oBillsAdd.Advance = GetObjectValue(jo, "Advance"); //預收
                                            oBillsAdd.TaxSum = GetObjectValue(jo, "TaxSum"); //稅額
                                            oBillsAdd.TotalReceivable = GetObjectValue(jo, "TotalReceivable"); //總應收
                                            oBillsAdd.IsRetn = string.IsNullOrEmpty(GetObjectValue(jo, "IsRetn")) ? @"N" : GetObjectValue(jo, "IsRetn"); //是否為退運
                                            oBillsAdd.Url = sUrl; //Url
                                            db.Insertable(oBillsAdd).ExecuteCommand();

                                            if (dicSucceccCount.Keys.Contains("出口審核"))
                                            {
                                                dicSucceccCount["出口審核"] = dicSucceccCount["出口審核"] + 1;
                                            }
                                            else
                                            {
                                                dicSucceccCount.Add("出口審核", 1);
                                            }

                                            #endregion
                                        }
                                        else if (sAuditVal == "5")
                                        {
                                            #region 過帳

                                            var oBillsAdd = new OTB_OPM_Bills
                                            {
                                                OrgID = export.OrgID,
                                                BillNO = sBillNo, //帳款號碼(1)
                                                CheckDate = Common.FnToTWDate(GetObjectValue(jo, "BillFirstCheckDate")),//對帳日期(2)
                                                BillType = @"20",//帳別(收付)(3)
                                                CustomerCode = oPayer.CustomerNO //客戶供應商代號(4)
                                            };
                                            var sResponsiblePerson = oOpm.ResponsiblePerson.Split('.')[0];
                                            oBillsAdd.ResponsiblePersonCode = Common.CutByteString(sResponsiblePerson, 11); //業務員代號(5)
                                            oBillsAdd.ResponsiblePersonFullCode = oOpm.ResponsiblePerson; //業務員全代號
                                            oBillsAdd.LastGetBillDate = @""; //最近收付日(6)
                                            oBillsAdd.LastGetBillNO = @""; //最近收付單號(7)
                                            oBillsAdd.TaxType = decimal.Parse(GetObjectValue(jo, "TaxSum").Replace(@",", @"")) > 0 ? @"5" : @"6";//稅別(8)
                                            oBillsAdd.NOTaxAmount = GetObjectValue(jo, "AmountSum"); //未稅金額(9)
                                            oBillsAdd.BillAmount = GetObjectValue(jo, "AmountTaxSum"); //帳款金額(10)
                                            oBillsAdd.PaymentAmount = @"0"; //收付金額(11)
                                            oBillsAdd.Allowance = @"0"; //折讓金額(12)
                                            oBillsAdd.DebtAmount = @"0"; //呆帳金額(13)
                                            oBillsAdd.ExchangeAmount = @"0"; //匯兌損益金額(14)
                                            oBillsAdd.Settle = @"N"; //結清否(15)
                                            oBillsAdd.InvoiceStartNumber = GetObjectValue(jo, "InvoiceNumber"); //發票號碼(起)(16)
                                            oBillsAdd.InvoiceEndNumber = GetObjectValue(jo, "InvoiceNumber");//發票號碼(迄)(17)
                                            oBillsAdd.Category = @"";//傳票類別(18)
                                            oBillsAdd.OrderNo = @"";//訂單單號(19)
                                            oBillsAdd.ClosingNote = @"N"; //結帳註記(20)
                                            oBillsAdd.GeneralInvoiceNumber = @""; //立帳總帳傳票號碼(21)
                                            oBillsAdd.GeneralSerialNumber = @""; //立帳總帳傳票序號(22)
                                            oBillsAdd.Remark1 = @""; //備註一(30C)(23)
                                            oBillsAdd.AccountSource = @"0"; //帳款來源(24)
                                            oBillsAdd.UpdateDate = @""; //更新日期(25)
                                            oBillsAdd.UpdatePersonnel = @""; //更新人員(26)
                                            oBillsAdd.DepartmentSiteNumber = oOpm.DepartmentID; //部門\ 工地編號(27)
                                            if (!string.IsNullOrWhiteSpace(oOpm.ExhibitionNO))
                                            {
                                                var oExhibition = db.Queryable<OTB_OPM_Exhibition>().Single(it => it.SN == oOpm.ExhibitionNO.ObjToInt());
                                                oBillsAdd.ProjectNumber = oExhibition == null ? @"" : oExhibition.ExhibitionCode; //專案\ 項目編號(28)
                                            }
                                            else
                                            {
                                                oBillsAdd.ProjectNumber = @""; //專案\ 項目編號(28)
                                            }
                                            oBillsAdd.TransferBNotes = @""; //轉B 帳註記(29)
                                            oBillsAdd.ABNumber = @""; //A|B 帳唯一流水號(30)
                                            oBillsAdd.EnterNumber = @""; //進銷單號(31)
                                            var sCurrency = GetObjectValue(jo, "Currency");
                                            if (export.OrgID == "SG")
                                            {
                                                if (sCurrency == "RMB")
                                                {
                                                    sCurrency = "";
                                                }
                                            }
                                            else
                                            {
                                                if (sCurrency == "NTD")
                                                {
                                                    sCurrency = "";
                                                }
                                            }
                                            oBillsAdd.ForeignCurrencyCode = sCurrency; //外幣代號(32)
                                            var sExchangeRate = GetObjectValue(jo, "ExchangeRate");
                                            oBillsAdd.ExchangeRate = sExchangeRate; //匯率(33)
                                            oBillsAdd.ForeignAmount = (decimal.Parse(GetObjectValue(jo, "AmountTaxSum")) * decimal.Parse(sExchangeRate == @"" ? @"1" : sExchangeRate)).ToString(); //外幣金額(34)
                                            oBillsAdd.PayAmount = @"0"; //收付沖抵金額(35)
                                            oBillsAdd.RefundAmount = @"0"; //退款金額(36)
                                            oBillsAdd.PaymentTerms = @""; //收付條件(37)
                                            oBillsAdd.AccountDate = Common.FnToTWDate(GetObjectValue(jo, "BillFirstCheckDate")); //帳款日期(38)
                                            oBillsAdd.DCreditCardNumber = @""; //預設信用卡號(39)
                                            oBillsAdd.ClosingDate = @""; //結帳日期(40)
                                            oBillsAdd.CusField1 = @""; //自定義欄位一(41)
                                            oBillsAdd.CusField2 = @""; //自定義欄位二(42)
                                            oBillsAdd.CusField3 = @""; //自定義欄位三(43)
                                            oBillsAdd.CusField4 = @""; //自定義欄位四(44)
                                            oBillsAdd.CusField5 = @""; //自定義欄位五(45)
                                            oBillsAdd.CusField6 = @"0"; //自定義欄位六(46)
                                            oBillsAdd.CusField7 = @"0"; //自定義欄位七(47)
                                            oBillsAdd.CusField8 = @"0"; //自定義欄位八(48)
                                            oBillsAdd.CusField9 = @"0"; //自定義欄位九(49)
                                            oBillsAdd.CusField10 = @"0";  //自定義欄位十(50)
                                            oBillsAdd.CusField11 = @""; //自定義欄位十一(51)
                                            oBillsAdd.CusField12 = @""; //自定義欄位十二(52)
                                            oBillsAdd.Remark2 = @""; //備註二(M)(53)
                                            oBillsAdd.TWNOTaxAmount = GetObjectValue(jo, "AmountSum"); //台幣未稅金額(54)
                                            oBillsAdd.TWAmount = GetObjectValue(jo, "AmountTaxSum"); //台幣帳款金額(55)
                                            oBillsAdd.CreateUser = export.ModifyUser;
                                            oBillsAdd.CreateDate = ((DateTime)export.ModifyDate).ToString(@"yyyy/MM/dd HH:mm:ss");
                                            oBillsAdd.BillFirstCheckDate = string.IsNullOrEmpty(GetObjectValue(jo, "BillFirstCheckDate")) ? DateTime.Now.ToString(@"yyyy/MM/dd HH:mm:ss") : GetObjectValue(jo, "BillFirstCheckDate");
                                            oBillsAdd.Advance = GetObjectValue(jo, "Advance"); //預收
                                            oBillsAdd.TaxSum = GetObjectValue(jo, "TaxSum"); //稅額
                                            oBillsAdd.TotalReceivable = GetObjectValue(jo, "TotalReceivable"); //總應收
                                            oBillsAdd.IsRetn = string.IsNullOrEmpty(GetObjectValue(jo, "IsRetn")) ? @"N" : GetObjectValue(jo, "IsRetn"); //是否為退運
                                            oBillsAdd.Url = sUrl; //Url
                                            db.Insertable(oBillsAdd).ExecuteCommand();

                                            if (dicSucceccCount.Keys.Contains("出口過帳"))
                                            {
                                                dicSucceccCount["出口過帳"] = dicSucceccCount["出口過帳"] + 1;
                                            }
                                            else
                                            {
                                                dicSucceccCount.Add("出口過帳", 1);
                                            }

                                            #endregion
                                        }
                                    }
                                }
                            }

                        }
                    }
                }

                if (ExcExportReturn || BillsNo.Any())
                {
                    //解析ReturnBill
                    JArray ja = JArray.Parse(export.ReturnBills);

                    if (ja.Count > 0)
                    {
                        var sdb = new SimpleClient<OTB_OPM_ExportExhibition>(db);
                        var oOpm = sdb.GetById(export.ExportBillNO);
                        if (oOpm == null)
                        {
                            sMsg = $"退運出口單號：{ export.ExportBillNO } => 系統找不到對應的基本資料，請核查！";
                            break;
                        }

                        foreach (JObject joH in ja.OfType<JObject>())
                        {
                            JArray ja1 = JArray.Parse(GetObjectValue(joH, "Bills"));

                            if (ja1.Count > 0)
                            {
                                foreach (JObject jo in ja1.OfType<JObject>())
                                {
                                    string sBillNo = GetObjectValue(jo, "BillNO");
                                    string sAuditVal = GetObjectValue(jo, "AuditVal");
                                    string sBillCheckDate = GetObjectValue(jo, "BillCheckDate");
                                    string sPayer = GetObjectValue(jo, "Payer");
                                    string sUrl = @"ExhibitionExport_Upd|?Action=Upd&GoTab=5&ExportBillNO=" + oOpm.ExportBillNO + @"&BillNO=" + sBillNo;
                                    DateTime dtCreateDate;

                                    if (BillsNo.Any() && !BillsNo.Contains(sBillNo))
                                    {
                                        continue;
                                    }

                                    if (DateTime.TryParse(sBillCheckDate, out dtCreateDate))
                                    {
                                        if (dtCreateDate <= dtSettingEnd && dtCreateDate >= dtSettingStartDate)
                                        {
                                            var oPayer = new OTB_CRM_Customers();
                                            if (!string.IsNullOrEmpty(sPayer))
                                            {
                                                oPayer = db.Queryable<OTB_CRM_Customers>().Single(it => it.guid == sPayer);
                                                if (oPayer == null)
                                                {
                                                    sMsg = $"退運出口單號：{ export.ExportBillNO } => 系統找不到付款人資訊";
                                                    break;
                                                }
                                            }

                                            //狀態4(已銷帳)/6(已作廢)當成已審核拋轉
                                            if (sAuditVal == "2" || sAuditVal == "4" || sAuditVal == "6")
                                            {
                                                #region 審核

                                                var oBillsAdd = new OTB_OPM_Bills
                                                {
                                                    OrgID = export.OrgID,
                                                    BillNO = sBillNo, //帳款號碼(1)
                                                    CheckDate = Common.FnToTWDate(GetObjectValue(jo, "BillFirstCheckDate")),//對帳日期(2)
                                                    BillType = @"20",//帳別(收付)(3)
                                                    CustomerCode = oPayer.CustomerNO //客戶供應商代號(4)
                                                };

                                                var sResponsiblePerson = oOpm.ResponsiblePerson.Split('.')[0];
                                                oBillsAdd.ResponsiblePersonCode = Common.CutByteString(sResponsiblePerson, 11); //業務員代號(5)
                                                oBillsAdd.ResponsiblePersonFullCode = oOpm.ResponsiblePerson; //業務員全代號
                                                oBillsAdd.LastGetBillDate = @""; //最近收付日(6)
                                                oBillsAdd.LastGetBillNO = @""; //最近收付單號(7)
                                                oBillsAdd.TaxType = decimal.Parse(GetObjectValue(jo, "TaxSum").Replace(@",", @"")) > 0 ? @"5" : @"6";//稅別(8)
                                                oBillsAdd.NOTaxAmount = GetObjectValue(jo, "AmountSum"); //未稅金額(9)
                                                oBillsAdd.BillAmount = GetObjectValue(jo, "AmountTaxSum"); //帳款金額(10)
                                                oBillsAdd.PaymentAmount = @"0"; //收付金額(11)
                                                oBillsAdd.Allowance = @"0"; //折讓金額(12)
                                                oBillsAdd.DebtAmount = @"0"; //呆帳金額(13)
                                                oBillsAdd.ExchangeAmount = @"0"; //匯兌損益金額(14)
                                                oBillsAdd.Settle = @"N"; //結清否(15)
                                                oBillsAdd.InvoiceStartNumber = @""; //發票號碼(起)(16)
                                                oBillsAdd.InvoiceEndNumber = @"";//發票號碼(迄)(17)
                                                oBillsAdd.Category = @"";//傳票類別(18)
                                                oBillsAdd.OrderNo = @"";//訂單單號(19)
                                                oBillsAdd.ClosingNote = @"N"; //結帳註記(20)
                                                oBillsAdd.GeneralInvoiceNumber = @""; //立帳總帳傳票號碼(21)
                                                oBillsAdd.GeneralSerialNumber = @""; //立帳總帳傳票序號(22)
                                                oBillsAdd.Remark1 = @""; //備註一(30C)(23)
                                                oBillsAdd.AccountSource = @"0"; //帳款來源(24)
                                                oBillsAdd.UpdateDate = @""; //更新日期(25)
                                                oBillsAdd.UpdatePersonnel = @""; //更新人員(26)
                                                oBillsAdd.DepartmentSiteNumber = oOpm.DepartmentID; //部門\ 工地編號(27)
                                                if (!string.IsNullOrWhiteSpace(oOpm.ExhibitionNO))
                                                {
                                                    var oExhibition = db.Queryable<OTB_OPM_Exhibition>().Single(it => it.SN == oOpm.ExhibitionNO.ObjToInt());
                                                    oBillsAdd.ProjectNumber = oExhibition == null ? @"" : oExhibition.ExhibitionCode; //專案\ 項目編號(28)
                                                }
                                                else
                                                {
                                                    oBillsAdd.ProjectNumber = @""; //專案\ 項目編號(28)
                                                }
                                                oBillsAdd.TransferBNotes = @""; //轉B 帳註記(29)
                                                oBillsAdd.ABNumber = @""; //A|B 帳唯一流水號(30)
                                                oBillsAdd.EnterNumber = @""; //進銷單號(31)
                                                var sCurrency = GetObjectValue(jo, "Currency");
                                                if (export.OrgID == "SG")
                                                {
                                                    if (sCurrency == "RMB")
                                                    {
                                                        sCurrency = "";
                                                    }
                                                }
                                                else
                                                {
                                                    if (sCurrency == "NTD")
                                                    {
                                                        sCurrency = "";
                                                    }
                                                }
                                                oBillsAdd.ForeignCurrencyCode = sCurrency; //外幣代號(32)
                                                var sExchangeRate = GetObjectValue(jo, "ExchangeRate");
                                                oBillsAdd.ExchangeRate = sExchangeRate; //匯率(33)
                                                oBillsAdd.ForeignAmount = (decimal.Parse(GetObjectValue(jo, "AmountTaxSum")) * decimal.Parse(sExchangeRate == @"" ? @"1" : sExchangeRate)).ToString(); //外幣金額(34)
                                                oBillsAdd.PayAmount = @"0"; //收付沖抵金額(35)
                                                oBillsAdd.RefundAmount = @"0"; //退款金額(36)
                                                oBillsAdd.PaymentTerms = @""; //收付條件(37)
                                                oBillsAdd.AccountDate = Common.FnToTWDate(GetObjectValue(jo, "BillFirstCheckDate")); //帳款日期(38)
                                                oBillsAdd.DCreditCardNumber = @""; //預設信用卡號(39)
                                                oBillsAdd.ClosingDate = @""; //結帳日期(40)
                                                oBillsAdd.CusField1 = @""; //自定義欄位一(41)
                                                oBillsAdd.CusField2 = @""; //自定義欄位二(42)
                                                oBillsAdd.CusField3 = @""; //自定義欄位三(43)
                                                oBillsAdd.CusField4 = @""; //自定義欄位四(44)
                                                oBillsAdd.CusField5 = @""; //自定義欄位五(45)
                                                oBillsAdd.CusField6 = @"0"; //自定義欄位六(46)
                                                oBillsAdd.CusField7 = @"0"; //自定義欄位七(47)
                                                oBillsAdd.CusField8 = @"0"; //自定義欄位八(48)
                                                oBillsAdd.CusField9 = @"0"; //自定義欄位九(49)
                                                oBillsAdd.CusField10 = @"0";  //自定義欄位十(50)
                                                oBillsAdd.CusField11 = @""; //自定義欄位十一(51)
                                                oBillsAdd.CusField12 = @""; //自定義欄位十二(52)
                                                oBillsAdd.Remark2 = @""; //備註二(M)(53)
                                                oBillsAdd.TWNOTaxAmount = GetObjectValue(jo, "AmountSum"); //台幣未稅金額(54)
                                                oBillsAdd.TWAmount = GetObjectValue(jo, "AmountTaxSum"); //台幣帳款金額(55)
                                                oBillsAdd.CreateUser = export.ModifyUser;
                                                oBillsAdd.CreateDate = ((DateTime)export.ModifyDate).ToString(@"yyyy/MM/dd HH:mm:ss");
                                                oBillsAdd.BillFirstCheckDate = string.IsNullOrEmpty(GetObjectValue(jo, "BillFirstCheckDate")) ? DateTime.Now.ToString(@"yyyy/MM/dd HH:mm:ss") : GetObjectValue(jo, "BillFirstCheckDate");
                                                oBillsAdd.Advance = GetObjectValue(jo, "Advance"); //預收
                                                oBillsAdd.TaxSum = GetObjectValue(jo, "TaxSum"); //稅額
                                                oBillsAdd.TotalReceivable = GetObjectValue(jo, "TotalReceivable"); //總應收
                                                oBillsAdd.IsRetn = string.IsNullOrEmpty(GetObjectValue(jo, "IsRetn")) ? @"N" : GetObjectValue(jo, "IsRetn"); //是否為退運
                                                oBillsAdd.Url = sUrl; //Url
                                                db.Insertable(oBillsAdd).ExecuteCommand();

                                                if (dicSucceccCount.Keys.Contains("退運出口審核"))
                                                {
                                                    dicSucceccCount["退運出口審核"] = dicSucceccCount["退運出口審核"] + 1;
                                                }
                                                else
                                                {
                                                    dicSucceccCount.Add("退運出口審核", 1);
                                                }

                                                #endregion
                                            }
                                            else if (sAuditVal == "5")
                                            {
                                                #region 過帳

                                                var oBillsAdd = new OTB_OPM_Bills
                                                {
                                                    OrgID = export.OrgID,
                                                    BillNO = sBillNo, //帳款號碼(1)
                                                    CheckDate = Common.FnToTWDate(GetObjectValue(jo, "BillFirstCheckDate")),//對帳日期(2)
                                                    BillType = @"20",//帳別(收付)(3)
                                                    CustomerCode = oPayer.CustomerNO //客戶供應商代號(4)
                                                };
                                                var sResponsiblePerson = oOpm.ResponsiblePerson.Split('.')[0];
                                                oBillsAdd.ResponsiblePersonCode = Common.CutByteString(sResponsiblePerson, 11); //業務員代號(5)
                                                oBillsAdd.ResponsiblePersonFullCode = oOpm.ResponsiblePerson; //業務員全代號
                                                oBillsAdd.LastGetBillDate = @""; //最近收付日(6)
                                                oBillsAdd.LastGetBillNO = @""; //最近收付單號(7)
                                                oBillsAdd.TaxType = decimal.Parse(GetObjectValue(jo, "TaxSum").Replace(@",", @"")) > 0 ? @"5" : @"6";//稅別(8)
                                                oBillsAdd.NOTaxAmount = GetObjectValue(jo, "AmountSum"); //未稅金額(9)
                                                oBillsAdd.BillAmount = GetObjectValue(jo, "AmountTaxSum"); //帳款金額(10)
                                                oBillsAdd.PaymentAmount = @"0"; //收付金額(11)
                                                oBillsAdd.Allowance = @"0"; //折讓金額(12)
                                                oBillsAdd.DebtAmount = @"0"; //呆帳金額(13)
                                                oBillsAdd.ExchangeAmount = @"0"; //匯兌損益金額(14)
                                                oBillsAdd.Settle = @"N"; //結清否(15)
                                                oBillsAdd.InvoiceStartNumber = GetObjectValue(jo, "InvoiceNumber"); //發票號碼(起)(16)
                                                oBillsAdd.InvoiceEndNumber = GetObjectValue(jo, "InvoiceNumber");//發票號碼(迄)(17)
                                                oBillsAdd.Category = @"";//傳票類別(18)
                                                oBillsAdd.OrderNo = @"";//訂單單號(19)
                                                oBillsAdd.ClosingNote = @"N"; //結帳註記(20)
                                                oBillsAdd.GeneralInvoiceNumber = @""; //立帳總帳傳票號碼(21)
                                                oBillsAdd.GeneralSerialNumber = @""; //立帳總帳傳票序號(22)
                                                oBillsAdd.Remark1 = @""; //備註一(30C)(23)
                                                oBillsAdd.AccountSource = @"0"; //帳款來源(24)
                                                oBillsAdd.UpdateDate = @""; //更新日期(25)
                                                oBillsAdd.UpdatePersonnel = @""; //更新人員(26)
                                                oBillsAdd.DepartmentSiteNumber = oOpm.DepartmentID; //部門\ 工地編號(27)
                                                if (!string.IsNullOrWhiteSpace(oOpm.ExhibitionNO))
                                                {
                                                    var oExhibition = db.Queryable<OTB_OPM_Exhibition>().Single(it => it.SN == oOpm.ExhibitionNO.ObjToInt());
                                                    oBillsAdd.ProjectNumber = oExhibition == null ? @"" : oExhibition.ExhibitionCode; //專案\ 項目編號(28)
                                                }
                                                else
                                                {
                                                    oBillsAdd.ProjectNumber = @""; //專案\ 項目編號(28)
                                                }
                                                oBillsAdd.TransferBNotes = @""; //轉B 帳註記(29)
                                                oBillsAdd.ABNumber = @""; //A|B 帳唯一流水號(30)
                                                oBillsAdd.EnterNumber = @""; //進銷單號(31)
                                                var sCurrency = GetObjectValue(jo, "Currency");
                                                if (export.OrgID == "SG")
                                                {
                                                    if (sCurrency == "RMB")
                                                    {
                                                        sCurrency = "";
                                                    }
                                                }
                                                else
                                                {
                                                    if (sCurrency == "NTD")
                                                    {
                                                        sCurrency = "";
                                                    }
                                                }
                                                oBillsAdd.ForeignCurrencyCode = sCurrency; //外幣代號(32)
                                                var sExchangeRate = GetObjectValue(jo, "ExchangeRate");
                                                oBillsAdd.ExchangeRate = sExchangeRate; //匯率(33)
                                                oBillsAdd.ForeignAmount = (decimal.Parse(GetObjectValue(jo, "AmountTaxSum")) * decimal.Parse(sExchangeRate == @"" ? @"1" : sExchangeRate)).ToString(); //外幣金額(34)
                                                oBillsAdd.PayAmount = @"0"; //收付沖抵金額(35)
                                                oBillsAdd.RefundAmount = @"0"; //退款金額(36)
                                                oBillsAdd.PaymentTerms = @""; //收付條件(37)
                                                oBillsAdd.AccountDate = Common.FnToTWDate(GetObjectValue(jo, "BillFirstCheckDate")); //帳款日期(38)
                                                oBillsAdd.DCreditCardNumber = @""; //預設信用卡號(39)
                                                oBillsAdd.ClosingDate = @""; //結帳日期(40)
                                                oBillsAdd.CusField1 = @""; //自定義欄位一(41)
                                                oBillsAdd.CusField2 = @""; //自定義欄位二(42)
                                                oBillsAdd.CusField3 = @""; //自定義欄位三(43)
                                                oBillsAdd.CusField4 = @""; //自定義欄位四(44)
                                                oBillsAdd.CusField5 = @""; //自定義欄位五(45)
                                                oBillsAdd.CusField6 = @"0"; //自定義欄位六(46)
                                                oBillsAdd.CusField7 = @"0"; //自定義欄位七(47)
                                                oBillsAdd.CusField8 = @"0"; //自定義欄位八(48)
                                                oBillsAdd.CusField9 = @"0"; //自定義欄位九(49)
                                                oBillsAdd.CusField10 = @"0";  //自定義欄位十(50)
                                                oBillsAdd.CusField11 = @""; //自定義欄位十一(51)
                                                oBillsAdd.CusField12 = @""; //自定義欄位十二(52)
                                                oBillsAdd.Remark2 = @""; //備註二(M)(53)
                                                oBillsAdd.TWNOTaxAmount = GetObjectValue(jo, "AmountSum"); //台幣未稅金額(54)
                                                oBillsAdd.TWAmount = GetObjectValue(jo, "AmountTaxSum"); //台幣帳款金額(55)
                                                oBillsAdd.CreateUser = export.ModifyUser;
                                                oBillsAdd.CreateDate = ((DateTime)export.ModifyDate).ToString(@"yyyy/MM/dd HH:mm:ss");
                                                oBillsAdd.BillFirstCheckDate = string.IsNullOrEmpty(GetObjectValue(jo, "BillFirstCheckDate")) ? DateTime.Now.ToString(@"yyyy/MM/dd HH:mm:ss") : GetObjectValue(jo, "BillFirstCheckDate");
                                                oBillsAdd.Advance = GetObjectValue(jo, "Advance"); //預收
                                                oBillsAdd.TaxSum = GetObjectValue(jo, "TaxSum"); //稅額
                                                oBillsAdd.TotalReceivable = GetObjectValue(jo, "TotalReceivable"); //總應收
                                                oBillsAdd.IsRetn = string.IsNullOrEmpty(GetObjectValue(jo, "IsRetn")) ? @"N" : GetObjectValue(jo, "IsRetn"); //是否為退運
                                                oBillsAdd.Url = sUrl; //Url
                                                db.Insertable(oBillsAdd).ExecuteCommand();

                                                if (dicSucceccCount.Keys.Contains("退運出口過帳"))
                                                {
                                                    dicSucceccCount["退運出口過帳"] = dicSucceccCount["退運出口過帳"] + 1;
                                                }
                                                else
                                                {
                                                    dicSucceccCount.Add("退運出口過帳", 1);
                                                }

                                                #endregion
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            return sMsg;
        }

        public string Other(string sStartDate, string sEndDate, SqlSugarClient db)
        {
            string sMsg = null;
            DateTime dtSettingStartDate = Convert.ToDateTime(sStartDate);
            DateTime dtSettingEnd = Convert.ToDateTime(sEndDate);

            if (ExcOther || BillsNo.Any())
            {

                #region 其他

                var data3 = db.Queryable<OTB_OPM_OtherExhibition>()
                .Where(x => x.ModifyDate <= SqlFunc.ToDate(sEndDate) && x.ModifyDate >= SqlFunc.ToDate(sStartDate))
                .ToList();

                foreach (OTB_OPM_OtherExhibition other in data3)
                {
                    //解析bill
                    if (!string.IsNullOrEmpty(other.Bills))
                    {
                        JArray ja = JArray.Parse(other.Bills);

                        if (ja.Count > 0)
                        {
                            var sdb = new SimpleClient<OTB_OPM_OtherExhibition>(db);
                            var oOpm = sdb.GetById(other.Guid);
                            if (oOpm == null)
                            {
                                sMsg = $"其他GUID：{ other.Guid } => 系統找不到對應的基本資料，請核查！";
                                break;
                            }

                            foreach (JObject jo in ja.OfType<JObject>())
                            {
                                string sBillNo = GetObjectValue(jo, "BillNO");
                                string sAuditVal = GetObjectValue(jo, "AuditVal");
                                string sBillCheckDate = GetObjectValue(jo, "BillCheckDate");
                                string sPayer = GetObjectValue(jo, "Payer");
                                string sUrl = @"OtherBusiness_Upd|?Action=Upd&GoTab=2&ExportBillNO=" + oOpm.Guid + @"&BillNO=" + sBillNo;
                                DateTime dtCreateDate;

                                if (BillsNo.Any() && !BillsNo.Contains(sBillNo))
                                {
                                    continue;
                                }

                                if (DateTime.TryParse(sBillCheckDate, out dtCreateDate))
                                {
                                    if (dtCreateDate <= dtSettingEnd && dtCreateDate >= dtSettingStartDate)
                                    {
                                        var oPayer = new OTB_CRM_Customers();
                                        if (!string.IsNullOrEmpty(sPayer))
                                        {
                                            oPayer = db.Queryable<OTB_CRM_Customers>().Single(it => it.guid == sPayer);
                                            if (oPayer == null)
                                            {
                                                sMsg = $"其他GUID：{ other.Guid } => 系統找不到付款人資訊";
                                                break;
                                            }
                                        }

                                        //狀態4(已銷帳)/6(已作廢)當成已審核拋轉
                                        if (sAuditVal == "2" || sAuditVal == "4" || sAuditVal == "6")
                                        {
                                            #region 審核

                                            var oBillsAdd = new OTB_OPM_Bills
                                            {
                                                OrgID = other.OrgID,
                                                BillNO = sBillNo, //帳款號碼(1)
                                                CheckDate = Common.FnToTWDate(GetObjectValue(jo, "BillFirstCheckDate")),//對帳日期(2)
                                                BillType = @"20",//帳別(收付)(3)
                                                CustomerCode = oPayer.CustomerNO //客戶供應商代號(4)
                                            };

                                            var sResponsiblePerson = oOpm.ResponsiblePerson.Split('.')[0];
                                            oBillsAdd.ResponsiblePersonCode = Common.CutByteString(sResponsiblePerson, 11); //業務員代號(5)
                                            oBillsAdd.ResponsiblePersonFullCode = oOpm.ResponsiblePerson; //業務員全代號
                                            oBillsAdd.LastGetBillDate = @""; //最近收付日(6)
                                            oBillsAdd.LastGetBillNO = @""; //最近收付單號(7)
                                            oBillsAdd.TaxType = decimal.Parse(GetObjectValue(jo, "TaxSum").Replace(@",", @"")) > 0 ? @"5" : @"6";//稅別(8)
                                            oBillsAdd.NOTaxAmount = GetObjectValue(jo, "AmountSum"); //未稅金額(9)
                                            oBillsAdd.BillAmount = GetObjectValue(jo, "AmountTaxSum"); //帳款金額(10)
                                            oBillsAdd.PaymentAmount = @"0"; //收付金額(11)
                                            oBillsAdd.Allowance = @"0"; //折讓金額(12)
                                            oBillsAdd.DebtAmount = @"0"; //呆帳金額(13)
                                            oBillsAdd.ExchangeAmount = @"0"; //匯兌損益金額(14)
                                            oBillsAdd.Settle = @"N"; //結清否(15)
                                            oBillsAdd.InvoiceStartNumber = @""; //發票號碼(起)(16)
                                            oBillsAdd.InvoiceEndNumber = @"";//發票號碼(迄)(17)
                                            oBillsAdd.Category = @"";//傳票類別(18)
                                            oBillsAdd.OrderNo = @"";//訂單單號(19)
                                            oBillsAdd.ClosingNote = @"N"; //結帳註記(20)
                                            oBillsAdd.GeneralInvoiceNumber = @""; //立帳總帳傳票號碼(21)
                                            oBillsAdd.GeneralSerialNumber = @""; //立帳總帳傳票序號(22)
                                            oBillsAdd.Remark1 = @""; //備註一(30C)(23)
                                            oBillsAdd.AccountSource = @"0"; //帳款來源(24)
                                            oBillsAdd.UpdateDate = @""; //更新日期(25)
                                            oBillsAdd.UpdatePersonnel = @""; //更新人員(26)
                                            oBillsAdd.DepartmentSiteNumber = oOpm.DepartmentID; //部門\ 工地編號(27)
                                            if (!string.IsNullOrWhiteSpace(oOpm.ExhibitionNO))
                                            {
                                                var oExhibition = db.Queryable<OTB_OPM_Exhibition>().Single(it => it.SN == oOpm.ExhibitionNO.ObjToInt());
                                                oBillsAdd.ProjectNumber = oExhibition == null ? @"" : oExhibition.ExhibitionCode; //專案\ 項目編號(28)
                                            }
                                            else
                                            {
                                                oBillsAdd.ProjectNumber = @""; //專案\ 項目編號(28)
                                            }
                                            oBillsAdd.TransferBNotes = @""; //轉B 帳註記(29)
                                            oBillsAdd.ABNumber = @""; //A|B 帳唯一流水號(30)
                                            oBillsAdd.EnterNumber = @""; //進銷單號(31)
                                            var sCurrency = GetObjectValue(jo, "Currency");
                                            if (other.OrgID == "SG")
                                            {
                                                if (sCurrency == "RMB")
                                                {
                                                    sCurrency = "";
                                                }
                                            }
                                            else
                                            {
                                                if (sCurrency == "NTD")
                                                {
                                                    sCurrency = "";
                                                }
                                            }
                                            oBillsAdd.ForeignCurrencyCode = sCurrency; //外幣代號(32)
                                            var sExchangeRate = GetObjectValue(jo, "ExchangeRate");
                                            oBillsAdd.ExchangeRate = sExchangeRate; //匯率(33)
                                            oBillsAdd.ForeignAmount = (decimal.Parse(GetObjectValue(jo, "AmountTaxSum")) * decimal.Parse(sExchangeRate == @"" ? @"1" : sExchangeRate)).ToString(); //外幣金額(34)
                                            oBillsAdd.PayAmount = @"0"; //收付沖抵金額(35)
                                            oBillsAdd.RefundAmount = @"0"; //退款金額(36)
                                            oBillsAdd.PaymentTerms = @""; //收付條件(37)
                                            oBillsAdd.AccountDate = Common.FnToTWDate(GetObjectValue(jo, "BillFirstCheckDate")); //帳款日期(38)
                                            oBillsAdd.DCreditCardNumber = @""; //預設信用卡號(39)
                                            oBillsAdd.ClosingDate = @""; //結帳日期(40)
                                            oBillsAdd.CusField1 = @""; //自定義欄位一(41)
                                            oBillsAdd.CusField2 = @""; //自定義欄位二(42)
                                            oBillsAdd.CusField3 = @""; //自定義欄位三(43)
                                            oBillsAdd.CusField4 = @""; //自定義欄位四(44)
                                            oBillsAdd.CusField5 = @""; //自定義欄位五(45)
                                            oBillsAdd.CusField6 = @"0"; //自定義欄位六(46)
                                            oBillsAdd.CusField7 = @"0"; //自定義欄位七(47)
                                            oBillsAdd.CusField8 = @"0"; //自定義欄位八(48)
                                            oBillsAdd.CusField9 = @"0"; //自定義欄位九(49)
                                            oBillsAdd.CusField10 = @"0";  //自定義欄位十(50)
                                            oBillsAdd.CusField11 = @""; //自定義欄位十一(51)
                                            oBillsAdd.CusField12 = @""; //自定義欄位十二(52)
                                            oBillsAdd.Remark2 = @""; //備註二(M)(53)
                                            oBillsAdd.TWNOTaxAmount = GetObjectValue(jo, "AmountSum"); //台幣未稅金額(54)
                                            oBillsAdd.TWAmount = GetObjectValue(jo, "AmountTaxSum"); //台幣帳款金額(55)
                                            oBillsAdd.CreateUser = other.ModifyUser;
                                            oBillsAdd.CreateDate = ((DateTime)other.ModifyDate).ToString(@"yyyy/MM/dd HH:mm:ss");
                                            oBillsAdd.BillFirstCheckDate = string.IsNullOrEmpty(GetObjectValue(jo, "BillFirstCheckDate")) ? DateTime.Now.ToString(@"yyyy/MM/dd HH:mm:ss") : GetObjectValue(jo, "BillFirstCheckDate");
                                            oBillsAdd.Advance = GetObjectValue(jo, "Advance"); //預收
                                            oBillsAdd.TaxSum = GetObjectValue(jo, "TaxSum"); //稅額
                                            oBillsAdd.TotalReceivable = GetObjectValue(jo, "TotalReceivable"); //總應收
                                            oBillsAdd.IsRetn = string.IsNullOrEmpty(GetObjectValue(jo, "IsRetn")) ? @"N" : GetObjectValue(jo, "IsRetn"); //是否為退運
                                            oBillsAdd.Url = sUrl; //Url
                                            db.Insertable(oBillsAdd).ExecuteCommand();

                                            if (dicSucceccCount.Keys.Contains("其他審核"))
                                            {
                                                dicSucceccCount["其他審核"] = dicSucceccCount["其他審核"] + 1;
                                            }
                                            else
                                            {
                                                dicSucceccCount.Add("其他審核", 1);
                                            }

                                            #endregion
                                        }
                                        else if (sAuditVal == "5")
                                        {
                                            #region 過帳

                                            var oBillsAdd = new OTB_OPM_Bills
                                            {
                                                OrgID = other.OrgID,
                                                BillNO = sBillNo, //帳款號碼(1)
                                                CheckDate = Common.FnToTWDate(GetObjectValue(jo, "BillFirstCheckDate")),//對帳日期(2)
                                                BillType = @"20",//帳別(收付)(3)
                                                CustomerCode = oPayer.CustomerNO //客戶供應商代號(4)
                                            };
                                            var sResponsiblePerson = oOpm.ResponsiblePerson.Split('.')[0];
                                            oBillsAdd.ResponsiblePersonCode = Common.CutByteString(sResponsiblePerson, 11); //業務員代號(5)
                                            oBillsAdd.ResponsiblePersonFullCode = oOpm.ResponsiblePerson; //業務員全代號
                                            oBillsAdd.LastGetBillDate = @""; //最近收付日(6)
                                            oBillsAdd.LastGetBillNO = @""; //最近收付單號(7)
                                            oBillsAdd.TaxType = decimal.Parse(GetObjectValue(jo, "TaxSum").Replace(@",", @"")) > 0 ? @"5" : @"6";//稅別(8)
                                            oBillsAdd.NOTaxAmount = GetObjectValue(jo, "AmountSum"); //未稅金額(9)
                                            oBillsAdd.BillAmount = GetObjectValue(jo, "AmountTaxSum"); //帳款金額(10)
                                            oBillsAdd.PaymentAmount = @"0"; //收付金額(11)
                                            oBillsAdd.Allowance = @"0"; //折讓金額(12)
                                            oBillsAdd.DebtAmount = @"0"; //呆帳金額(13)
                                            oBillsAdd.ExchangeAmount = @"0"; //匯兌損益金額(14)
                                            oBillsAdd.Settle = @"N"; //結清否(15)
                                            oBillsAdd.InvoiceStartNumber = GetObjectValue(jo, "InvoiceNumber"); //發票號碼(起)(16)
                                            oBillsAdd.InvoiceEndNumber = GetObjectValue(jo, "InvoiceNumber");//發票號碼(迄)(17)
                                            oBillsAdd.Category = @"";//傳票類別(18)
                                            oBillsAdd.OrderNo = @"";//訂單單號(19)
                                            oBillsAdd.ClosingNote = @"N"; //結帳註記(20)
                                            oBillsAdd.GeneralInvoiceNumber = @""; //立帳總帳傳票號碼(21)
                                            oBillsAdd.GeneralSerialNumber = @""; //立帳總帳傳票序號(22)
                                            oBillsAdd.Remark1 = @""; //備註一(30C)(23)
                                            oBillsAdd.AccountSource = @"0"; //帳款來源(24)
                                            oBillsAdd.UpdateDate = @""; //更新日期(25)
                                            oBillsAdd.UpdatePersonnel = @""; //更新人員(26)
                                            oBillsAdd.DepartmentSiteNumber = oOpm.DepartmentID; //部門\ 工地編號(27)
                                            if (!string.IsNullOrWhiteSpace(oOpm.ExhibitionNO))
                                            {
                                                var oExhibition = db.Queryable<OTB_OPM_Exhibition>().Single(it => it.SN == oOpm.ExhibitionNO.ObjToInt());
                                                oBillsAdd.ProjectNumber = oExhibition == null ? @"" : oExhibition.ExhibitionCode; //專案\ 項目編號(28)
                                            }
                                            else
                                            {
                                                oBillsAdd.ProjectNumber = @""; //專案\ 項目編號(28)
                                            }
                                            oBillsAdd.TransferBNotes = @""; //轉B 帳註記(29)
                                            oBillsAdd.ABNumber = @""; //A|B 帳唯一流水號(30)
                                            oBillsAdd.EnterNumber = @""; //進銷單號(31)
                                            var sCurrency = GetObjectValue(jo, "Currency");
                                            if (other.OrgID == "SG")
                                            {
                                                if (sCurrency == "RMB")
                                                {
                                                    sCurrency = "";
                                                }
                                            }
                                            else
                                            {
                                                if (sCurrency == "NTD")
                                                {
                                                    sCurrency = "";
                                                }
                                            }
                                            oBillsAdd.ForeignCurrencyCode = sCurrency; //外幣代號(32)
                                            var sExchangeRate = GetObjectValue(jo, "ExchangeRate");
                                            oBillsAdd.ExchangeRate = sExchangeRate; //匯率(33)
                                            oBillsAdd.ForeignAmount = (decimal.Parse(GetObjectValue(jo, "AmountTaxSum")) * decimal.Parse(sExchangeRate == @"" ? @"1" : sExchangeRate)).ToString(); //外幣金額(34)
                                            oBillsAdd.PayAmount = @"0"; //收付沖抵金額(35)
                                            oBillsAdd.RefundAmount = @"0"; //退款金額(36)
                                            oBillsAdd.PaymentTerms = @""; //收付條件(37)
                                            oBillsAdd.AccountDate = Common.FnToTWDate(GetObjectValue(jo, "BillFirstCheckDate")); //帳款日期(38)
                                            oBillsAdd.DCreditCardNumber = @""; //預設信用卡號(39)
                                            oBillsAdd.ClosingDate = @""; //結帳日期(40)
                                            oBillsAdd.CusField1 = @""; //自定義欄位一(41)
                                            oBillsAdd.CusField2 = @""; //自定義欄位二(42)
                                            oBillsAdd.CusField3 = @""; //自定義欄位三(43)
                                            oBillsAdd.CusField4 = @""; //自定義欄位四(44)
                                            oBillsAdd.CusField5 = @""; //自定義欄位五(45)
                                            oBillsAdd.CusField6 = @"0"; //自定義欄位六(46)
                                            oBillsAdd.CusField7 = @"0"; //自定義欄位七(47)
                                            oBillsAdd.CusField8 = @"0"; //自定義欄位八(48)
                                            oBillsAdd.CusField9 = @"0"; //自定義欄位九(49)
                                            oBillsAdd.CusField10 = @"0";  //自定義欄位十(50)
                                            oBillsAdd.CusField11 = @""; //自定義欄位十一(51)
                                            oBillsAdd.CusField12 = @""; //自定義欄位十二(52)
                                            oBillsAdd.Remark2 = @""; //備註二(M)(53)
                                            oBillsAdd.TWNOTaxAmount = GetObjectValue(jo, "AmountSum"); //台幣未稅金額(54)
                                            oBillsAdd.TWAmount = GetObjectValue(jo, "AmountTaxSum"); //台幣帳款金額(55)
                                            oBillsAdd.CreateUser = other.ModifyUser;
                                            oBillsAdd.CreateDate = ((DateTime)other.ModifyDate).ToString(@"yyyy/MM/dd HH:mm:ss");
                                            oBillsAdd.BillFirstCheckDate = string.IsNullOrEmpty(GetObjectValue(jo, "BillFirstCheckDate")) ? DateTime.Now.ToString(@"yyyy/MM/dd HH:mm:ss") : GetObjectValue(jo, "BillFirstCheckDate");
                                            oBillsAdd.Advance = GetObjectValue(jo, "Advance"); //預收
                                            oBillsAdd.TaxSum = GetObjectValue(jo, "TaxSum"); //稅額
                                            oBillsAdd.TotalReceivable = GetObjectValue(jo, "TotalReceivable"); //總應收
                                            oBillsAdd.IsRetn = string.IsNullOrEmpty(GetObjectValue(jo, "IsRetn")) ? @"N" : GetObjectValue(jo, "IsRetn"); //是否為退運
                                            oBillsAdd.Url = sUrl; //Url
                                            db.Insertable(oBillsAdd).ExecuteCommand();

                                            if (dicSucceccCount.Keys.Contains("其他過帳"))
                                            {
                                                dicSucceccCount["其他過帳"] = dicSucceccCount["其他過帳"] + 1;
                                            }
                                            else
                                            {
                                                dicSucceccCount.Add("其他過帳", 1);
                                            }

                                            #endregion
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                #endregion

            }

            return sMsg;
        }

        public string OtherTG(string sStartDate, string sEndDate, SqlSugarClient db)
        {
            string sMsg = null;
            DateTime dtSettingStartDate = Convert.ToDateTime(sStartDate);
            DateTime dtSettingEnd = Convert.ToDateTime(sEndDate);

            if (ExcOtherTG || BillsNo.Any())
            {

                #region 其他(駒驛)

                var data6 = db.Queryable<OTB_OPM_OtherExhibitionTG>()
                .Where(x => x.ModifyDate <= SqlFunc.ToDate(sEndDate) && x.ModifyDate >= SqlFunc.ToDate(sStartDate))
                .ToList();

                foreach (OTB_OPM_OtherExhibitionTG OtherTG in data6)
                {
                    //解析bill
                    if (!string.IsNullOrEmpty(OtherTG.Bills))
                    {
                        JArray ja = JArray.Parse(OtherTG.Bills);

                        if (ja.Count > 0)
                        {
                            var sdb = new SimpleClient<OTB_OPM_OtherExhibitionTG>(db);
                            var oOpm = sdb.GetById(OtherTG.Guid);
                            if (oOpm == null)
                            {
                                sMsg = $"其他(駒驛)GUID：{ OtherTG.Guid } => 系統找不到對應的基本資料，請核查！";
                                break;
                            }

                            foreach (JObject jo in ja.OfType<JObject>())
                            {
                                string sBillNo = GetObjectValue(jo, "BillNO");
                                string sAuditVal = GetObjectValue(jo, "AuditVal");
                                string sBillCheckDate = GetObjectValue(jo, "BillCheckDate");
                                string sPayer = GetObjectValue(jo, "Payer");
                                string sUrl = @"OtherExhibitionTG_Upd|?Action=Upd&GoTab=3&Guid=" + oOpm.Guid + @"&BillNO=" + sBillNo;
                                DateTime dtCreateDate;

                                if (BillsNo.Any() && !BillsNo.Contains(sBillNo))
                                {
                                    continue;
                                }

                                var oPayer = new OTB_CRM_Customers();
                                if (!string.IsNullOrEmpty(sPayer))
                                {
                                    oPayer = db.Queryable<OTB_CRM_Customers>().Single(it => it.guid == sPayer);
                                    if (oPayer == null)
                                    {
                                        sMsg = $"其他(駒驛)GUID：{ OtherTG.Guid } => 系統找不到付款人資訊";
                                        break;
                                    }
                                }

                                //狀態4(已銷帳)/6(已作廢)當成已審核拋轉
                                if (sAuditVal == "2" || sAuditVal == "4" || sAuditVal == "6")
                                {
                                    #region 審核

                                    var oBillsAdd = new OTB_OPM_Bills
                                    {
                                        OrgID = OtherTG.OrgID,
                                        BillNO = sBillNo, //帳款號碼(1)
                                        CheckDate = Common.FnToTWDate(GetObjectValue(jo, "BillFirstCheckDate")),//對帳日期(2)
                                        BillType = @"20",//帳別(收付)(3)
                                        CustomerCode = oPayer.CustomerNO //客戶供應商代號(4)
                                    };

                                    var sResponsiblePerson = oOpm.ResponsiblePerson.Split('.')[0];
                                    oBillsAdd.ResponsiblePersonCode = Common.CutByteString(sResponsiblePerson, 11); //業務員代號(5)
                                    oBillsAdd.ResponsiblePersonFullCode = oOpm.ResponsiblePerson; //業務員全代號
                                    oBillsAdd.LastGetBillDate = @""; //最近收付日(6)
                                    oBillsAdd.LastGetBillNO = @""; //最近收付單號(7)
                                    oBillsAdd.TaxType = decimal.Parse(GetObjectValue(jo, "TaxSum").Replace(@",", @"")) > 0 ? @"5" : @"6";//稅別(8)
                                    oBillsAdd.NOTaxAmount = GetObjectValue(jo, "AmountSum"); //未稅金額(9)
                                    oBillsAdd.BillAmount = GetObjectValue(jo, "AmountTaxSum"); //帳款金額(10)
                                    oBillsAdd.PaymentAmount = @"0"; //收付金額(11)
                                    oBillsAdd.Allowance = @"0"; //折讓金額(12)
                                    oBillsAdd.DebtAmount = @"0"; //呆帳金額(13)
                                    oBillsAdd.ExchangeAmount = @"0"; //匯兌損益金額(14)
                                    oBillsAdd.Settle = @"N"; //結清否(15)
                                    oBillsAdd.InvoiceStartNumber = @""; //發票號碼(起)(16)
                                    oBillsAdd.InvoiceEndNumber = @"";//發票號碼(迄)(17)
                                    oBillsAdd.Category = @"";//傳票類別(18)
                                    oBillsAdd.OrderNo = @"";//訂單單號(19)
                                    oBillsAdd.ClosingNote = @"N"; //結帳註記(20)
                                    oBillsAdd.GeneralInvoiceNumber = @""; //立帳總帳傳票號碼(21)
                                    oBillsAdd.GeneralSerialNumber = @""; //立帳總帳傳票序號(22)
                                    oBillsAdd.Remark1 = @""; //備註一(30C)(23)
                                    oBillsAdd.AccountSource = @"0"; //帳款來源(24)
                                    oBillsAdd.UpdateDate = @""; //更新日期(25)
                                    oBillsAdd.UpdatePersonnel = @""; //更新人員(26)
                                    oBillsAdd.DepartmentSiteNumber = oOpm.DepartmentID; //部門\ 工地編號(27)
                                    if (!string.IsNullOrWhiteSpace(oOpm.ExhibitionNO))
                                    {
                                        var oExhibition = db.Queryable<OTB_OPM_Exhibition>().Single(it => it.SN == oOpm.ExhibitionNO.ObjToInt());
                                        oBillsAdd.ProjectNumber = oExhibition == null ? @"" : oExhibition.ExhibitionCode; //專案\ 項目編號(28)
                                    }
                                    else
                                    {
                                        oBillsAdd.ProjectNumber = @""; //專案\ 項目編號(28)
                                    }
                                    oBillsAdd.TransferBNotes = @""; //轉B 帳註記(29)
                                    oBillsAdd.ABNumber = @""; //A|B 帳唯一流水號(30)
                                    oBillsAdd.EnterNumber = @""; //進銷單號(31)
                                    var sCurrency = GetObjectValue(jo, "Currency");
                                    if (OtherTG.OrgID == "SG")
                                    {
                                        if (sCurrency == "RMB")
                                        {
                                            sCurrency = "";
                                        }
                                    }
                                    else
                                    {
                                        if (sCurrency == "NTD")
                                        {
                                            sCurrency = "";
                                        }
                                    }
                                    oBillsAdd.ForeignCurrencyCode = sCurrency; //外幣代號(32)
                                    var sExchangeRate = GetObjectValue(jo, "ExchangeRate");
                                    oBillsAdd.ExchangeRate = sExchangeRate; //匯率(33)
                                    oBillsAdd.ForeignAmount = (decimal.Parse(GetObjectValue(jo, "AmountTaxSum")) * decimal.Parse(sExchangeRate == @"" ? @"1" : sExchangeRate)).ToString(); //外幣金額(34)
                                    oBillsAdd.PayAmount = @"0"; //收付沖抵金額(35)
                                    oBillsAdd.RefundAmount = @"0"; //退款金額(36)
                                    oBillsAdd.PaymentTerms = @""; //收付條件(37)
                                    oBillsAdd.AccountDate = Common.FnToTWDate(GetObjectValue(jo, "BillFirstCheckDate")); //帳款日期(38)
                                    oBillsAdd.DCreditCardNumber = @""; //預設信用卡號(39)
                                    oBillsAdd.ClosingDate = @""; //結帳日期(40)
                                    oBillsAdd.CusField1 = @""; //自定義欄位一(41)
                                    oBillsAdd.CusField2 = @""; //自定義欄位二(42)
                                    oBillsAdd.CusField3 = @""; //自定義欄位三(43)
                                    oBillsAdd.CusField4 = @""; //自定義欄位四(44)
                                    oBillsAdd.CusField5 = @""; //自定義欄位五(45)
                                    oBillsAdd.CusField6 = @"0"; //自定義欄位六(46)
                                    oBillsAdd.CusField7 = @"0"; //自定義欄位七(47)
                                    oBillsAdd.CusField8 = @"0"; //自定義欄位八(48)
                                    oBillsAdd.CusField9 = @"0"; //自定義欄位九(49)
                                    oBillsAdd.CusField10 = @"0";  //自定義欄位十(50)
                                    oBillsAdd.CusField11 = @""; //自定義欄位十一(51)
                                    oBillsAdd.CusField12 = @""; //自定義欄位十二(52)
                                    oBillsAdd.Remark2 = @""; //備註二(M)(53)
                                    oBillsAdd.TWNOTaxAmount = GetObjectValue(jo, "AmountSum"); //台幣未稅金額(54)
                                    oBillsAdd.TWAmount = GetObjectValue(jo, "AmountTaxSum"); //台幣帳款金額(55)
                                    oBillsAdd.CreateUser = OtherTG.ModifyUser;
                                    oBillsAdd.CreateDate = ((DateTime)OtherTG.ModifyDate).ToString(@"yyyy/MM/dd HH:mm:ss");
                                    oBillsAdd.BillFirstCheckDate = string.IsNullOrEmpty(GetObjectValue(jo, "BillFirstCheckDate")) ? DateTime.Now.ToString(@"yyyy/MM/dd HH:mm:ss") : GetObjectValue(jo, "BillFirstCheckDate");
                                    oBillsAdd.Advance = GetObjectValue(jo, "Advance"); //預收
                                    oBillsAdd.TaxSum = GetObjectValue(jo, "TaxSum"); //稅額
                                    oBillsAdd.TotalReceivable = GetObjectValue(jo, "TotalReceivable"); //總應收
                                    oBillsAdd.IsRetn = string.IsNullOrEmpty(GetObjectValue(jo, "IsRetn")) ? @"N" : GetObjectValue(jo, "IsRetn"); //是否為退運
                                    oBillsAdd.Url = sUrl; //Url
                                    db.Insertable(oBillsAdd).ExecuteCommand();

                                    if (dicSucceccCount.Keys.Contains("其他(駒驛)審核"))
                                    {
                                        dicSucceccCount["其他(駒驛)審核"] = dicSucceccCount["其他(駒驛)審核"] + 1;
                                    }
                                    else
                                    {
                                        dicSucceccCount.Add("其他(駒驛)審核", 1);
                                    }

                                    #endregion
                                }
                                else if (sAuditVal == "5")
                                {
                                    #region 過帳

                                    var oBillsAdd = new OTB_OPM_Bills
                                    {
                                        OrgID = OtherTG.OrgID,
                                        BillNO = sBillNo, //帳款號碼(1)
                                        CheckDate = Common.FnToTWDate(GetObjectValue(jo, "BillFirstCheckDate")),//對帳日期(2)
                                        BillType = @"20",//帳別(收付)(3)
                                        CustomerCode = oPayer.CustomerNO //客戶供應商代號(4)
                                    };
                                    var sResponsiblePerson = oOpm.ResponsiblePerson.Split('.')[0];
                                    oBillsAdd.ResponsiblePersonCode = Common.CutByteString(sResponsiblePerson, 11); //業務員代號(5)
                                    oBillsAdd.ResponsiblePersonFullCode = oOpm.ResponsiblePerson; //業務員全代號
                                    oBillsAdd.LastGetBillDate = @""; //最近收付日(6)
                                    oBillsAdd.LastGetBillNO = @""; //最近收付單號(7)
                                    oBillsAdd.TaxType = decimal.Parse(GetObjectValue(jo, "TaxSum").Replace(@",", @"")) > 0 ? @"5" : @"6";//稅別(8)
                                    oBillsAdd.NOTaxAmount = GetObjectValue(jo, "AmountSum"); //未稅金額(9)
                                    oBillsAdd.BillAmount = GetObjectValue(jo, "AmountTaxSum"); //帳款金額(10)
                                    oBillsAdd.PaymentAmount = @"0"; //收付金額(11)
                                    oBillsAdd.Allowance = @"0"; //折讓金額(12)
                                    oBillsAdd.DebtAmount = @"0"; //呆帳金額(13)
                                    oBillsAdd.ExchangeAmount = @"0"; //匯兌損益金額(14)
                                    oBillsAdd.Settle = @"N"; //結清否(15)
                                    oBillsAdd.InvoiceStartNumber = GetObjectValue(jo, "InvoiceNumber"); //發票號碼(起)(16)
                                    oBillsAdd.InvoiceEndNumber = GetObjectValue(jo, "InvoiceNumber");//發票號碼(迄)(17)
                                    oBillsAdd.Category = @"";//傳票類別(18)
                                    oBillsAdd.OrderNo = @"";//訂單單號(19)
                                    oBillsAdd.ClosingNote = @"N"; //結帳註記(20)
                                    oBillsAdd.GeneralInvoiceNumber = @""; //立帳總帳傳票號碼(21)
                                    oBillsAdd.GeneralSerialNumber = @""; //立帳總帳傳票序號(22)
                                    oBillsAdd.Remark1 = @""; //備註一(30C)(23)
                                    oBillsAdd.AccountSource = @"0"; //帳款來源(24)
                                    oBillsAdd.UpdateDate = @""; //更新日期(25)
                                    oBillsAdd.UpdatePersonnel = @""; //更新人員(26)
                                    oBillsAdd.DepartmentSiteNumber = oOpm.DepartmentID; //部門\ 工地編號(27)
                                    if (!string.IsNullOrWhiteSpace(oOpm.ExhibitionNO))
                                    {
                                        var oExhibition = db.Queryable<OTB_OPM_Exhibition>().Single(it => it.SN == oOpm.ExhibitionNO.ObjToInt());
                                        oBillsAdd.ProjectNumber = oExhibition == null ? @"" : oExhibition.ExhibitionCode; //專案\ 項目編號(28)
                                    }
                                    else
                                    {
                                        oBillsAdd.ProjectNumber = @""; //專案\ 項目編號(28)
                                    }
                                    oBillsAdd.TransferBNotes = @""; //轉B 帳註記(29)
                                    oBillsAdd.ABNumber = @""; //A|B 帳唯一流水號(30)
                                    oBillsAdd.EnterNumber = @""; //進銷單號(31)
                                    var sCurrency = GetObjectValue(jo, "Currency");
                                    if (OtherTG.OrgID == "SG")
                                    {
                                        if (sCurrency == "RMB")
                                        {
                                            sCurrency = "";
                                        }
                                    }
                                    else
                                    {
                                        if (sCurrency == "NTD")
                                        {
                                            sCurrency = "";
                                        }
                                    }
                                    oBillsAdd.ForeignCurrencyCode = sCurrency; //外幣代號(32)
                                    var sExchangeRate = GetObjectValue(jo, "ExchangeRate");
                                    oBillsAdd.ExchangeRate = sExchangeRate; //匯率(33)
                                    oBillsAdd.ForeignAmount = (decimal.Parse(GetObjectValue(jo, "AmountTaxSum")) * decimal.Parse(sExchangeRate == @"" ? @"1" : sExchangeRate)).ToString(); //外幣金額(34)
                                    oBillsAdd.PayAmount = @"0"; //收付沖抵金額(35)
                                    oBillsAdd.RefundAmount = @"0"; //退款金額(36)
                                    oBillsAdd.PaymentTerms = @""; //收付條件(37)
                                    oBillsAdd.AccountDate = Common.FnToTWDate(GetObjectValue(jo, "BillFirstCheckDate")); //帳款日期(38)
                                    oBillsAdd.DCreditCardNumber = @""; //預設信用卡號(39)
                                    oBillsAdd.ClosingDate = @""; //結帳日期(40)
                                    oBillsAdd.CusField1 = @""; //自定義欄位一(41)
                                    oBillsAdd.CusField2 = @""; //自定義欄位二(42)
                                    oBillsAdd.CusField3 = @""; //自定義欄位三(43)
                                    oBillsAdd.CusField4 = @""; //自定義欄位四(44)
                                    oBillsAdd.CusField5 = @""; //自定義欄位五(45)
                                    oBillsAdd.CusField6 = @"0"; //自定義欄位六(46)
                                    oBillsAdd.CusField7 = @"0"; //自定義欄位七(47)
                                    oBillsAdd.CusField8 = @"0"; //自定義欄位八(48)
                                    oBillsAdd.CusField9 = @"0"; //自定義欄位九(49)
                                    oBillsAdd.CusField10 = @"0";  //自定義欄位十(50)
                                    oBillsAdd.CusField11 = @""; //自定義欄位十一(51)
                                    oBillsAdd.CusField12 = @""; //自定義欄位十二(52)
                                    oBillsAdd.Remark2 = @""; //備註二(M)(53)
                                    oBillsAdd.TWNOTaxAmount = GetObjectValue(jo, "AmountSum"); //台幣未稅金額(54)
                                    oBillsAdd.TWAmount = GetObjectValue(jo, "AmountTaxSum"); //台幣帳款金額(55)
                                    oBillsAdd.CreateUser = OtherTG.ModifyUser;
                                    oBillsAdd.CreateDate = ((DateTime)OtherTG.ModifyDate).ToString(@"yyyy/MM/dd HH:mm:ss");
                                    oBillsAdd.BillFirstCheckDate = string.IsNullOrEmpty(GetObjectValue(jo, "BillFirstCheckDate")) ? DateTime.Now.ToString(@"yyyy/MM/dd HH:mm:ss") : GetObjectValue(jo, "BillFirstCheckDate");
                                    oBillsAdd.Advance = GetObjectValue(jo, "Advance"); //預收
                                    oBillsAdd.TaxSum = GetObjectValue(jo, "TaxSum"); //稅額
                                    oBillsAdd.TotalReceivable = GetObjectValue(jo, "TotalReceivable"); //總應收
                                    oBillsAdd.IsRetn = string.IsNullOrEmpty(GetObjectValue(jo, "IsRetn")) ? @"N" : GetObjectValue(jo, "IsRetn"); //是否為退運
                                    oBillsAdd.Url = sUrl; //Url
                                    db.Insertable(oBillsAdd).ExecuteCommand();

                                    if (dicSucceccCount.Keys.Contains("其他(駒驛)過帳"))
                                    {
                                        dicSucceccCount["其他(駒驛)過帳"] = dicSucceccCount["其他(駒驛)過帳"] + 1;
                                    }
                                    else
                                    {
                                        dicSucceccCount.Add("其他(駒驛)過帳", 1);
                                    }

                                    #endregion
                                }
                            }
                        }
                    }
                }

                #endregion

            }

            return sMsg;
        }

        public string Exhibition(string sStartDate, string sEndDate, SqlSugarClient db, bool ReturnBills = false)
        {
            string sMsg = null;

            if (ExcExhibtion || ExhibtionsNo.Any())
            {

                #region 專案(展覽)

                var data4 = db.Queryable<OTB_OPM_Exhibition>()
                .Where(x => x.CreateDate <= SqlFunc.ToDate(sEndDate) && x.CreateDate >= SqlFunc.ToDate(sStartDate) && x.IsTransfer == "Y")
                .ToList();

                foreach (OTB_OPM_Exhibition exhibition in data4)
                {
                    if (ExhibtionsNo.Any() && !ExhibtionsNo.Contains(exhibition.ExhibitionCode))
                    {
                        continue;
                    }

                    var oExhibitionsTransferUpd = new OTB_OPM_ExhibitionsTransfer
                    {
                        OrgID = exhibition.OrgID,
                        PrjNO = exhibition.ExhibitionCode,
                        PrjName = Common.CutByteString(exhibition.ExhibitioShotName_TW, 60)
                    };
                    var sCreateUser = exhibition.CreateUser.Split('.')[0];
                    oExhibitionsTransferUpd.PrjCharger = Common.CutByteString(sCreateUser, 11);
                    oExhibitionsTransferUpd.EndDate = @"";
                    db.Insertable(oExhibitionsTransferUpd).ExecuteCommand();

                    if (dicSucceccCount.Keys.Contains("專案(展覽)"))
                    {
                        dicSucceccCount["專案(展覽)"] = dicSucceccCount["專案(展覽)"] + 1;
                    }
                    else
                    {
                        dicSucceccCount.Add("專案(展覽)", 1);
                    }
                }

                #endregion

            }

            return sMsg;
        }

        public string Customer(string sStartDate, string sEndDate, SqlSugarClient db, bool ReturnBills = false)
        {
            string sMsg = null;

            if (ExcCustomer || ExhibtionsNo.Any())
            {

                #region 客戶

                var data5 = db.Queryable<OTB_CRM_Customers>()
                .Where(x => x.CreateDate <= SqlFunc.ToDate(sEndDate) && x.CreateDate >= SqlFunc.ToDate(sStartDate) && x.IsAudit == "Y")
                .ToList();

                foreach (OTB_CRM_Customers cus in data5)
                {
                    var sdb = new SimpleClient<OTB_CRM_Customers>(db);
                    var customer = sdb.GetById(cus.guid);

                    if (customer == null)
                    {
                        sMsg = $"客戶GUID：{ cus.guid }系統找不到對應的客戶資料，請核查！";
                        break;
                    }

                    if (ExhibtionsNo.Any() && !ExhibtionsNo.Contains(customer.CustomerNO))
                    {
                        continue;
                    }

                    var oCustomersTransferAdd = new OTB_CRM_CustomersTransfer();
                    oCustomersTransferAdd.OrgID = customer.OrgID;
                    oCustomersTransferAdd.Feild01 = customer.CustomerNO;
                    oCustomersTransferAdd.Feild02 = @"0";
                    oCustomersTransferAdd.Feild03 = Common.CutByteString(customer.CustomerShotCName, 12);
                    oCustomersTransferAdd.Feild04 = Common.CutByteString(customer.CustomerCName == @"" ? customer.CustomerEName : customer.CustomerCName, 60);
                    oCustomersTransferAdd.Feild05 = @"";
                    oCustomersTransferAdd.Feild06 = @"";
                    oCustomersTransferAdd.Feild07 = customer.UniCode;
                    oCustomersTransferAdd.Feild08 = @"";
                    oCustomersTransferAdd.Feild09 = @"";
                    oCustomersTransferAdd.Feild10 = Common.CutByteString(customer.InvoiceAddress, 60);
                    oCustomersTransferAdd.Feild11 = Common.CutByteString(customer.Address, 60);
                    oCustomersTransferAdd.Feild12 = @"";
                    oCustomersTransferAdd.Feild13 = @"";
                    oCustomersTransferAdd.Feild14 = Common.CutByteString(customer.Telephone, 20);
                    oCustomersTransferAdd.Feild15 = @"";
                    oCustomersTransferAdd.Feild16 = Common.CutByteString(customer.FAX, 20);
                    oCustomersTransferAdd.Feild17 = @"";
                    oCustomersTransferAdd.Feild18 = @"";
                    oCustomersTransferAdd.Feild19 = @"";
                    oCustomersTransferAdd.Feild20 = @"";
                    oCustomersTransferAdd.Feild21 = @"";
                    oCustomersTransferAdd.Feild22 = @"";
                    oCustomersTransferAdd.Feild23 = Common.CutByteString(customer.Memo, 30);
                    oCustomersTransferAdd.Feild24 = @"100";
                    oCustomersTransferAdd.Feild25 = @"";
                    oCustomersTransferAdd.Feild26 = @"";
                    oCustomersTransferAdd.Feild27 = @"100";
                    oCustomersTransferAdd.Feild28 = @"";
                    oCustomersTransferAdd.Feild29 = Common.CutByteString(customer.CreateUser.Split('.')[0], 11);
                    oCustomersTransferAdd.Feild30 = @"";
                    oCustomersTransferAdd.Feild31 = @"";
                    oCustomersTransferAdd.Feild32 = @"";
                    oCustomersTransferAdd.Feild33 = @"";
                    oCustomersTransferAdd.Feild34 = @"";
                    oCustomersTransferAdd.Feild35 = @"";
                    oCustomersTransferAdd.Feild36 = @"";
                    oCustomersTransferAdd.Feild37 = @"B,C".IndexOf(customer.TransactionType) > -1 ? @"6" : @"5";
                    oCustomersTransferAdd.Feild38 = @"2";
                    oCustomersTransferAdd.Feild39 = @"";
                    oCustomersTransferAdd.Feild40 = @"";
                    oCustomersTransferAdd.Feild41 = @"1";
                    oCustomersTransferAdd.Feild42 = @"";
                    oCustomersTransferAdd.Feild43 = @"";
                    oCustomersTransferAdd.Feild44 = @"";
                    oCustomersTransferAdd.Feild45 = @"";
                    oCustomersTransferAdd.Feild46 = @"";
                    oCustomersTransferAdd.Feild47 = @"";
                    oCustomersTransferAdd.Feild48 = @"";
                    oCustomersTransferAdd.Feild49 = @"";
                    oCustomersTransferAdd.Feild50 = @"";
                    oCustomersTransferAdd.Feild51 = @"";
                    oCustomersTransferAdd.Feild52 = @"";
                    oCustomersTransferAdd.Feild53 = @"";
                    oCustomersTransferAdd.Feild54 = @"";
                    oCustomersTransferAdd.Feild55 = @"";
                    oCustomersTransferAdd.Feild56 = @"";
                    oCustomersTransferAdd.Feild57 = @"";
                    oCustomersTransferAdd.Feild58 = customer.CustomerNO;
                    oCustomersTransferAdd.Feild59 = @"";
                    oCustomersTransferAdd.Feild60 = @"";
                    oCustomersTransferAdd.Feild61 = @"";
                    oCustomersTransferAdd.Feild62 = @"";
                    oCustomersTransferAdd.Feild63 = @"";
                    oCustomersTransferAdd.Feild64 = @"";
                    oCustomersTransferAdd.Feild65 = @"";
                    oCustomersTransferAdd.Feild66 = @"";
                    oCustomersTransferAdd.Feild67 = @"";
                    oCustomersTransferAdd.Feild68 = @"";
                    oCustomersTransferAdd.Feild69 = @"";
                    oCustomersTransferAdd.Feild70 = @"";
                    oCustomersTransferAdd.Feild71 = @"";
                    oCustomersTransferAdd.Feild72 = @"";
                    oCustomersTransferAdd.Feild73 = @"";
                    oCustomersTransferAdd.Feild74 = @"";
                    oCustomersTransferAdd.Feild75 = @"";
                    oCustomersTransferAdd.Feild76 = @"";
                    oCustomersTransferAdd.Feild77 = @"";
                    oCustomersTransferAdd.Feild78 = @"";
                    oCustomersTransferAdd.Feild79 = @"";
                    oCustomersTransferAdd.Feild80 = @"";
                    oCustomersTransferAdd.Feild81 = @"";
                    oCustomersTransferAdd.Feild82 = Common.CutByteString(customer.CustomerEName, 120);
                    var sAddress = customer.Address;
                    var cn = new Regex(@"[一-龥]+");//正则表达式 表示汉字范围
                    if (cn.IsMatch(sAddress))
                    {
                        sAddress = @"";
                    }
                    oCustomersTransferAdd.Feild83 = Common.CutByteString(sAddress, 240);
                    oCustomersTransferAdd.Feild84 = @"";
                    oCustomersTransferAdd.Feild85 = @"";
                    oCustomersTransferAdd.Feild86 = @"";
                    oCustomersTransferAdd.Feild87 = @"";
                    oCustomersTransferAdd.Feild88 = @"";
                    oCustomersTransferAdd.Feild89 = @"";
                    oCustomersTransferAdd.Feild90 = @"";
                    oCustomersTransferAdd.Feild91 = @"0";
                    oCustomersTransferAdd.Feild92 = @"0";
                    oCustomersTransferAdd.Feild93 = @"";
                    oCustomersTransferAdd.Feild94 = @"";
                    oCustomersTransferAdd.Feild95 = @"";
                    oCustomersTransferAdd.Feild96 = @"";
                    oCustomersTransferAdd.Feild97 = @"";
                    oCustomersTransferAdd.Feild98 = @"";
                    oCustomersTransferAdd.Feild99 = @"";
                    db.Insertable(oCustomersTransferAdd).ExecuteCommand();

                    if (dicSucceccCount.Keys.Contains("客戶"))
                    {
                        dicSucceccCount["客戶"] = dicSucceccCount["客戶"] + 1;
                    }
                    else
                    {
                        dicSucceccCount.Add("客戶", 1);
                    }

                }

                #endregion

            }

            return sMsg;
        }


        public string GetObjectValue(JObject i_oJObject, string i_sColumn)
        {
            string sReturnValue = string.Empty;

            if (i_oJObject[i_sColumn] != null)
            {
                sReturnValue = i_oJObject[i_sColumn].ToString();
            }

            return sReturnValue;
        }

        public bool StringConvertBool(string i_sValue)
        {
            bool blReturn = false;

            bool.TryParse((i_sValue as string ?? string.Empty), out blReturn);

            return blReturn;
        }
    }
}
