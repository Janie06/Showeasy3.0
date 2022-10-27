using EasyBL.WebApi.Message;
using Entity;
using Entity.Sugar;
using Entity.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SqlSugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace EasyBL.WEBAPP.OPM
{
    public class OpmComService : ServiceBase
    {
        #region 驗證發票號碼是否重複key入

        /// <summary>
        /// 驗證發票號碼是否重複key入
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on CheckInvoiceNumber</param>
        /// <returns></returns>
        public ResponseMessage CheckInvoiceNumber(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.DB;
            try
            {
                do
                {
                    var bExsit = false;
                    var sInvoiceNumber = _fetchString(i_crm, @"InvoiceNumber");
                    bExsit = db.Queryable<OTB_OPM_BillInfo>().Any(it => it.InvoiceNumber == sInvoiceNumber && it.OrgID == i_crm.ORIGID);
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, bExsit);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(OpmComService), @"進出口管理模組", @"CheckInvoiceNumber（驗證發票號碼是否重複key入）", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
            }
            return rm;
        }

        #endregion 驗證發票號碼是否重複key入

        #region 獲取賬單資料

        /// <summary>
        /// 獲取賬單資料
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on GetBills</param>
        /// <returns></returns>
        public ResponseMessage GetBills(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.DB;
            try
            {
                do
                {
                    var sBillNO = _fetchString(i_crm, @"BillNO");
                    //20200721 調整抓取資料回推由一年改為兩年
                    var rDate = DateTime.Now.AddYears(-2);
                    var saBillInfo = db.Queryable<OTB_OPM_BillInfo>()
                        .OrderBy(x => x.CreateDate)
                        .Where(x => x.OrgID == i_crm.ORIGID && x.CreateDate > rDate)
                        .WhereIF(!string.IsNullOrEmpty(sBillNO), x => x.BillNO == sBillNO)
                        .Select(x => new { x.BillNO })
                        .ToList();
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, saBillInfo);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(OpmComService), @"進出口管理模組", @"GetBills（獲取賬單資料）", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
            }
            return rm;
        }

        #endregion 獲取賬單資料

        #region 預設預約單資料

        /// <summary>
        /// 預設預約單資料
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on InitAppoint</param>
        /// <returns></returns>
        public ResponseMessage InitAppoint(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sAppointNO = _fetchString(i_crm, @"AppointNO");

                    //預約單基本資料
                    var oTempl = db.Queryable<OTB_WSM_PackingOrder, OTB_OPM_Exhibition>(
                        (t1, t2) => new object[]
                                    {
                                      JoinType.Inner, t1.OrgID == t2.OrgID && t1.ExhibitionNO == t2.SN.ToString()
                                    }
                        )
                        .Where((t1, t2) => t1.OrgID == i_crm.ORIGID && t1.AppointNO == sAppointNO)
                        .Select((t1, t2) => new OTB_OPM_OtherExhibition
                        {
                            OrgID = t1.OrgID,
                            ExhibitionNO = t2.SN.ToString(),
                            ImportBillName = t2.Exhibitioname_TW,
                            ImportBillEName = t2.Exhibitioname_EN,
                            ExhibitionDateStart = t2.ExhibitionDateStart,
                            ExhibitionDateEnd = t2.ExhibitionDateEnd,
                            Hall = t2.ExhibitionAddress,
                            MuseumMumber = t1.MuseumMumber,
                            Supplier = @"",
                            Contactor = t1.AppointUser,
                            Telephone = t1.AppointTel,
                            SitiContactor = t1.Contactor,
                            SitiTelephone = t1.ContactTel,
                            ApproachTime = t1.ApproachTime,
                            ExitTime = t1.ExitTime
                        })
                        .Single();

                    var oImportCustomers = db.Queryable<OTB_WSM_PackingOrder, OTB_CRM_ImportCustomers>(
                        (t1, t2) => new object[]
                                    {
                                      JoinType.Inner, t1.OrgID == t2.OrgID && t1.CustomerId == t2.guid.ToString()
                                    }
                        )
                        .Where((t1, t2) => t1.OrgID == i_crm.ORIGID && t1.AppointNO == sAppointNO)
                        .Select((t1, t2) => new
                        {
                            t2.FormalGuid,
                        })
                        .Single();

                    oTempl.Supplier = oImportCustomers.FormalGuid;

                    //該展覽所有廠商（排除已經產生賬單的資料）
                    var saImportCustomers = db.Queryable<OTB_WSM_PackingOrder, OTB_CRM_ImportCustomers>(
                        (t1, t2) => new object[]
                                    {
                                      JoinType.Inner, t1.OrgID == t2.OrgID && t1.CustomerId == t2.guid.ToString()
                                    }
                        )
                        .Where((t1, t2) => t1.OrgID == i_crm.ORIGID && t1.ExhibitionNO == oTempl.ExhibitionNO && t2.IsFormal == true && SqlFunc.IsNullOrEmpty(t1.OtherId))
                        .Select((t1, t2) => new
                        {
                            t1.AppointNO,
                            t2.FormalGuid,
                        })
                        .ToList();

                    var saKeys = saImportCustomers.Select(x => x.FormalGuid).ToList();

                    //抓去正式客戶資料
                    var saCustomers = db.Queryable<OTB_CRM_Customers>()
                        .Where(x => x.OrgID == i_crm.ORIGID && saKeys.Contains(x.guid))
                        .Select(x => new
                        {
                            AppointNO = "",
                            x.guid,
                            x.CustomerNO,
                            x.UniCode,
                            x.CustomerCName,
                            x.CustomerEName,
                            x.Telephone,
                            x.Email
                        })
                        .ToList();

                    //添加預約單號碼
                    var saLast = (from e in saCustomers
                                  join f in saImportCustomers on e.guid equals f.FormalGuid
                                  select new
                                  {
                                      f.AppointNO,
                                      e.guid,
                                      e.CustomerNO,
                                      e.UniCode,
                                      e.CustomerCName,
                                      e.CustomerEName,
                                      e.Telephone,
                                      e.Email
                                  }).Distinct();

                    var map = new Map
                    {
                        { "Base", oTempl },
                        { "Customers", saLast }
                    };
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, map);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(OpmComService), @"進出口管理模組", @"InitAppoint（預設預約單資料）", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
            }
            return rm;
        }

        #endregion 預設預約單資料

        #region 獲取賬單關聯

        /// <summary>
        /// 獲取賬單關聯
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on InitAppoint</param>
        /// <returns></returns>
        public ResponseMessage GetBillAssociated(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sAppointNO = _fetchString(i_crm, @"AppointNO");
                    var sOtherId = _fetchString(i_crm, @"OtherId");
                    var sSupplierID = _fetchString(i_crm, @"SupplierID");
                    var map = new Map();
                    var tb_FeeItems = new DataTable(@"tb");
                    if (i_crm.ORIGID == "TG")
                    {
                        var oPackingOrder = db.Queryable<OTB_WSM_PackingOrder, OTB_CRM_ImportCustomers, OTB_CRM_Customers>(
                            (t1, t2, t3) => new object[]
                                                {
                                                          JoinType.Inner, t1.OrgID == t2.OrgID && t1.CustomerId == t2.guid.ToString(),
                                                          JoinType.Inner, t2.OrgID == t3.OrgID && t2.FormalGuid == t3.guid
                                                }
                            )
                            .Where((t1, t2, t3) => t1.OrgID == i_crm.ORIGID)
                            .WhereIF(!string.IsNullOrEmpty(sAppointNO), (t1, t2, t3) => t1.AppointNO == sAppointNO)
                            .WhereIF(!string.IsNullOrEmpty(sOtherId), (t1, t2, t3) => t1.OtherId == sOtherId)
                            .WhereIF(!string.IsNullOrEmpty(sSupplierID), (t1, t2, t3) => t2.FormalGuid == sSupplierID && t3.guid == sSupplierID)
                            .Select((t1, t2, t3) => t1).Single();

                        if (oPackingOrder != null)
                        {
                            var jaPackingInfo = (JArray)JsonConvert.DeserializeObject(oPackingOrder.PackingInfo);
                            var iAllNumber = 0;
                            var iAllWeight = 0;
                            Decimal iAllVolume = 0;
                            var sContactorId = "";
                            var sContactorName = "";
                            if (jaPackingInfo.Count > 0)
                            {
                                foreach (var jo in jaPackingInfo)
                                {
                                    var iNumber = int.Parse(jo["ExpoNumber"].ToString());
                                    var iWeight = int.Parse(jo["ExpoWeight"].ToString()) * iNumber;
                                    var iVolume = Math.Round(Convert.ToDecimal(jo["ExpoLen"].ToString()) * Convert.ToDecimal(jo["ExpoWidth"].ToString()) * Convert.ToDecimal(jo["ExpoHeight"].ToString()) * iNumber / Convert.ToDecimal("1000000"), 2);
                                    iAllNumber += iNumber;
                                    iAllWeight += iWeight;
                                    iAllVolume = iAllVolume + iVolume;
                                }
                                var oCustomers = db.Queryable<OTB_WSM_PackingOrder, OTB_CRM_ImportCustomers, OTB_CRM_Customers>(
                                    (t1, t2, t3) => new object[]
                                                        {
                                                          JoinType.Inner, t1.OrgID == t2.OrgID && t1.CustomerId == t2.guid.ToString(),
                                                          JoinType.Inner, t2.OrgID == t3.OrgID && t2.FormalGuid == t3.guid
                                                        }
                                    )
                                    .Where((t1, t2, t3) => t1.OrgID == i_crm.ORIGID)
                                    .WhereIF(!string.IsNullOrEmpty(sAppointNO), (t1, t2, t3) => t1.AppointNO == sAppointNO)
                                    .WhereIF(!string.IsNullOrEmpty(sOtherId), (t1, t2, t3) => t1.OtherId == sOtherId)
                                    .WhereIF(!string.IsNullOrEmpty(sSupplierID), (t1, t2, t3) => t2.FormalGuid == sSupplierID && t3.guid == sSupplierID)
                                    .Select((t1, t2, t3) => t3).Single();

                                if (!string.IsNullOrEmpty(oCustomers.Contactors))
                                {
                                    var jaContactors = (JArray)JsonConvert.DeserializeObject(oCustomers.Contactors);
                                    foreach (var joContacto in jaContactors)
                                    {
                                        if (joContacto["FullName"].ToString() == oPackingOrder.Contactor)
                                        {
                                            sContactorId = joContacto["guid"].ToString();
                                            sContactorName = joContacto["FullName"].ToString();
                                        }
                                    }
                                }

                                map = new Map
                                        {
                                            {"CustomerId",oCustomers.guid },
                                            {"CustomerCode",oCustomers.CustomerNO },
                                            {"UniCode",oCustomers.UniCode },
                                            {"ContactorId",sContactorId },
                                            {"ContactorName",sContactorName },
                                            {"ContactTel",oPackingOrder.ContactTel },
                                            {"Number",iAllNumber },
                                            {"Unit","PKG" },
                                            {"Weight",iAllWeight },
                                            {"Volume",iAllVolume }
                                        };
                            }

                            var saPackingInfo = (JArray)JsonConvert.DeserializeObject(oPackingOrder.PackingInfo);
                            tb_FeeItems.Columns.Add(@"guid", typeof(String));//費用代號
                            tb_FeeItems.Columns.Add(@"FinancialCode", typeof(String));//費用代號
                            tb_FeeItems.Columns.Add(@"FinancialCostStatement", typeof(String));//費用說明
                            tb_FeeItems.Columns.Add(@"FinancialCurrency", typeof(String));//幣別
                            tb_FeeItems.Columns.Add(@"FinancialUnitPrice", typeof(String));//單價
                            tb_FeeItems.Columns.Add(@"FinancialNumber", typeof(String));//FinancialNumber
                            tb_FeeItems.Columns.Add(@"FinancialUnit", typeof(String));//單位
                            tb_FeeItems.Columns.Add(@"FinancialAmount", typeof(String));//金額
                            tb_FeeItems.Columns.Add(@"FinancialExchangeRate", typeof(String));//匯率
                            tb_FeeItems.Columns.Add(@"FinancialTWAmount", typeof(String));//台幣金額
                            tb_FeeItems.Columns.Add(@"FinancialTaxRate", typeof(String));//稅率
                            tb_FeeItems.Columns.Add(@"FinancialTax", typeof(String));//稅額
                            tb_FeeItems.Columns.Add(@"Memo", typeof(String));//稅額
                            tb_FeeItems.Columns.Add(@"CreateUser", typeof(String));//創建人
                            tb_FeeItems.Columns.Add(@"CreateDate", typeof(String));//創建時間

                            var oExhibitionRules = db.Queryable<OTB_WSM_ExhibitionRules, OTB_OPM_Exhibition>(
                                (t1, t2) => new object[]
                                                            {
                                                          JoinType.Inner, t1.OrgID == t2.OrgID && t1.Guid == t2.CostRulesId
                                                            }
                                )
                                .Where((t1, t2) => t1.OrgID == i_crm.ORIGID && t2.SN.ToString() == oPackingOrder.ExhibitionNO)
                                .Select((t1, t2) => t1).Single();

                            foreach (JObject packingInfo in saPackingInfo)
                            {
                                try
                                {
                                    var bExpoStack = (bool)(packingInfo["ExpoStack"] ?? "false");
                                    var bExpoStorage = (bool)(packingInfo["ExpoStorage"] ?? "false");
                                    var bExpoSplit = (bool)(packingInfo["ExpoSplit"] ?? "false");
                                    var bExpoPack = (bool)(packingInfo["ExpoPack"] ?? "false");
                                    var bExpoFeed = (bool)(packingInfo["ExpoFeed"] ?? "false");

                                    var tb_row = tb_FeeItems.NewRow();
                                    if (bExpoStack)
                                    {//堆高機
                                        tb_row = GetFeeItemsInfo(i_crm, tb_FeeItems, packingInfo, oExhibitionRules, 1);
                                        tb_FeeItems.Rows.Add(tb_row);
                                    }
                                    if (bExpoSplit)
                                    {//進場服務費
                                        tb_row = GetFeeItemsInfo(i_crm, tb_FeeItems, packingInfo, oExhibitionRules, 2);
                                        tb_FeeItems.Rows.Add(tb_row);
                                    }
                                    if (bExpoPack)
                                    {//退場服務費
                                        tb_row = GetFeeItemsInfo(i_crm, tb_FeeItems, packingInfo, oExhibitionRules, 3);
                                        tb_FeeItems.Rows.Add(tb_row);
                                    }
                                    if (bExpoFeed)
                                    {//空箱收送與儲存（空箱收送）
                                        tb_row = GetFeeItemsInfo(i_crm, tb_FeeItems, packingInfo, oExhibitionRules, 4);
                                        tb_FeeItems.Rows.Add(tb_row);
                                    }
                                    if (bExpoStorage)
                                    {//空箱收送與儲存（儲存）
                                        tb_row = GetFeeItemsInfo(i_crm, tb_FeeItems, packingInfo, oExhibitionRules, 5);
                                        tb_FeeItems.Rows.Add(tb_row);
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                    var mapRel = new Map
                    {
                        { "Base", map },
                        { "Fees", tb_FeeItems }
                    };
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, mapRel);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(OpmComService), @"進出口管理模組", @"GetBillAssociated（獲取賬單關聯）", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
            }
            return rm;
        }

        /// <summary>
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on GetFeeItemsInfo</param>
        /// <param name="tb">todo: describe tb parameter on GetFeeItemsInfo</param>
        /// <param name="joInfo">todo: describe joInfo parameter on GetFeeItemsInfo</param>
        /// <param name="rule">todo: describe rule parameter on GetFeeItemsInfo</param>
        /// <param name="flag">todo: describe flag parameter on GetFeeItemsInfo</param>
        /// <returns></returns>
        private static DataRow GetFeeItemsInfo(RequestMessage i_crm, DataTable tb, JObject joInfo, OTB_WSM_ExhibitionRules rule, int flag)
        {
            var tb_row = tb.NewRow();
            var dTransferRate = Math.Round(Convert.ToDecimal("100") / Convert.ToDecimal("105"), 5);
            var iExpoNumber = Convert.ToInt32(joInfo["ExpoNumber"].ToString());
            var dTotalCBM = Convert.ToDecimal(joInfo["TotalCBM"].ToString());
            var dExpoWeightTon = Convert.ToDecimal(joInfo["ExpoWeightTon"].ToString());
            var iExpoDays = Convert.ToInt32(joInfo["ExpoDays"].ToString());

            var iPackingPrice = Math.Round(Convert.ToDecimal(rule.PackingPrice) * dTransferRate, 2);
            var iFeedingPrice = Math.Round(Convert.ToDecimal(rule.FeedingPrice) * dTransferRate, 2);
            var iStoragePrice = Math.Round(Convert.ToDecimal(rule.StoragePrice) * dTransferRate, 2);

            dTotalCBM = dTotalCBM < 1 ? 1 : dTotalCBM;
            tb_row[@"guid"] = Guid.NewGuid();
            tb_row[@"Memo"] = @"";
            tb_row[@"FinancialCurrency"] = @"NTD";
            tb_row[@"FinancialNumber"] = iExpoNumber;
            switch (flag)
            {
                case 1://堆高機
                    {
                        tb_row[@"FinancialCode"] = @"TEC06";
                        tb_row[@"FinancialCostStatement"] = @"堆高機";
                        var jaCostRules = (JArray)JsonConvert.DeserializeObject(rule.CostRules);
                        var oCurRule = GetCurRule(joInfo, jaCostRules);
                        var iPrice = Math.Round(int.Parse(oCurRule["Price"].ToString()) * dTransferRate, 2);
                        tb_row[@"FinancialUnitPrice"] = iPrice;
                        if (oCurRule["PricingMode"].ToString() == "T")
                        {
                            tb_row[@"FinancialNumber"] = dExpoWeightTon * iExpoNumber;
                            tb_row[@"FinancialUnit"] = @"TON";
                            tb_row[@"FinancialAmount"] = iPrice * dExpoWeightTon * iExpoNumber;
                        }
                        else
                        {
                            tb_row[@"FinancialUnit"] = @"SHPT";
                            tb_row[@"FinancialAmount"] = iPrice * iExpoNumber;
                        }
                    }
                    break;

                case 2://進場服務費
                    {
                        tb_row[@"FinancialCode"] = @"TEC34";
                        tb_row[@"FinancialCostStatement"] = @"進場服務費";
                        tb_row[@"FinancialUnitPrice"] = iPackingPrice;
                        tb_row[@"FinancialNumber"] = dTotalCBM * iExpoNumber;
                        tb_row[@"FinancialUnit"] = @"CBM";
                        tb_row[@"FinancialAmount"] = iPackingPrice * dTotalCBM * iExpoNumber;
                    }
                    break;

                case 3://退場服務費
                    {
                        tb_row[@"FinancialCode"] = @"TEC35";
                        tb_row[@"FinancialCostStatement"] = @"退場服務費";
                        tb_row[@"FinancialUnitPrice"] = iPackingPrice;
                        tb_row[@"FinancialNumber"] = dTotalCBM * iExpoNumber;
                        tb_row[@"FinancialUnit"] = @"CBM";
                        tb_row[@"FinancialAmount"] = iPackingPrice * dTotalCBM * iExpoNumber;
                    }
                    break;

                case 4://空箱收送與儲存（空箱收送）
                    {
                        tb_row[@"FinancialCode"] = @"TEC05";
                        tb_row[@"FinancialCostStatement"] = @"空箱收送與儲存";
                        tb_row[@"FinancialUnitPrice"] = iFeedingPrice;
                        tb_row[@"FinancialNumber"] = dTotalCBM * iExpoNumber;
                        tb_row[@"FinancialUnit"] = @"CBM";
                        tb_row[@"FinancialAmount"] = iFeedingPrice * dTotalCBM * iExpoNumber;
                    }
                    break;

                case 5://空箱收送與儲存（儲存）
                    {
                        tb_row[@"FinancialCode"] = @"TEC05";
                        tb_row[@"FinancialCostStatement"] = @"空箱收送與儲存";
                        tb_row[@"FinancialUnitPrice"] = iStoragePrice * dTotalCBM * iExpoNumber;
                        tb_row[@"FinancialNumber"] = iExpoDays;
                        tb_row[@"FinancialUnit"] = @"DAY";
                        tb_row[@"FinancialAmount"] = iStoragePrice * dTotalCBM * iExpoNumber * iExpoDays;
                    }
                    break;

                default:
                    break;
            }

            tb_row[@"FinancialTWAmount"] = tb_row[@"FinancialAmount"];
            tb_row[@"FinancialTaxRate"] = 0.05;
            tb_row[@"FinancialExchangeRate"] = 1;
            tb_row[@"FinancialTax"] = Convert.ToDouble(tb_row[@"FinancialTWAmount"].ToString()) * 0.05;
            tb_row[@"CreateUser"] = i_crm.USERID ?? @"";
            tb_row[@"CreateDate"] = DateTime.Now.ToString(@"yyyy/MM/dd HH:mm:ss");
            return tb_row;
        }

        private static JObject GetCurRule(JObject joData, JArray rules)
        {
            var jo = new JObject();
            var weight = (Decimal)joData["ExpoWeightTon"];
            foreach (JObject rule in rules)
            {
                var rule_min = PackNum(rule["Weight_Min"]);
                var rule_max = PackNum(rule["Weight_Max"]);
                if (weight >= rule_min && weight < rule_max || (weight == 30 && rule_max == 30))
                {
                    jo = rule;
                    break;
                }
            }
            return jo;
        }

        private static Decimal PackNum(object str)
        {
            return Convert.ToDecimal((str == null ? "" : str.ToString()) == "" ? "0" : str);
        }

        #endregion 獲取賬單關聯

        #region 複製費用項目

        /// <summary>
        /// 複製費用項目
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on InitAppoint</param>
        /// <returns></returns>
        public ResponseMessage GetBillInfos(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sExhibitionNO = _fetchString(i_crm, @"ExhibitionNO");
                    var sBillNO = _fetchString(i_crm, @"BillNO");
                    var saBillInfo = db.Queryable<OTB_OPM_BillInfo, OTB_SYS_Members, OTB_SYS_Members, OTB_OPM_Exhibition, OTB_CRM_Customers>(
                        (t1, t2, t3, t4, t5) => new object[]
                                            {
                                               JoinType.Inner, t1.OrgID == t2.OrgID && t1.CreateUser == t2.MemberID,
                                               JoinType.Inner, t1.OrgID == t3.OrgID && t1.ResponsiblePerson == t3.MemberID,
                                               JoinType.Inner, t1.OrgID == t4.OrgID && t1.ExhibitionNO == t4.SN.ToString(),
                                               JoinType.Left, t1.OrgID == t5.OrgID && t1.Payer == t5.guid
                                            }
                        )
                        .Where((t1, t2, t3, t4, t5) => t1.OrgID == i_crm.ORIGID)
                        .WhereIF(!string.IsNullOrEmpty(sExhibitionNO), (t1, t2, t3, t4, t5) => t1.ExhibitionNO == sExhibitionNO)
                        .WhereIF(!string.IsNullOrEmpty(sBillNO), (t1, t2, t3, t4, t5) => t1.BillNO == sBillNO)
                        .Select((t1, t2, t3, t4, t5) => new View_OPM_BillInfo
                        {
                            SN = SqlFunc.GetSelfAndAutoFill(t1.SN),
                            CreateUserName = t2.MemberName,
                            ResponsiblePersonName = t3.MemberName,
                            ExhibitioShotName = t4.ExhibitioShotName_CN,
                            ExhibitioName = t4.Exhibitioname_TW,
                            PayerName = SqlFunc.IIF(SqlFunc.HasValue(t5.CustomerCName), t5.CustomerCName, t5.CustomerEName)
                        })
                        .OrderBy("t1.CreateDate")
                        .ToList();
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, saBillInfo);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(OpmComService), @"進出口管理模組", @"GetBillInfos（複製費用項目）", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
            }
            return rm;
        }

        #endregion 複製費用項目

        #region 取得帳單存取權限

        public ResponseMessage GetExhibitionBillAuthorize(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var MemberToAccess = new List<string>();
                    //	Account
                    //select MemberIDs from OVW_SYS_Rules where RuleID in ('Account','Admin')
                    var sRuleIDs = _fetchString(i_crm, @"RuleID");
                    var sResponsiblePerson = _fetchString(i_crm, @"ResponsiblePerson");

                    var sMemberdb = new SimpleClient<OTB_SYS_Members>(db);



                    var spOrgID = new SugarParameter("@OrgID", i_crm.ORIGID);
                    var spResponsiblePerson = new SugarParameter("@MemberID", sResponsiblePerson);
                    var spRuleID = new SugarParameter("@RuleID", sRuleIDs);

                    //直屬主管、部門主管
                    var Supervisors = db.Ado.SqlQuery<string>(@"SELECT M.ImmediateSupervisor + ',' + D.ChiefOfDepartmentID FROM OTB_SYS_Members M
                    LEFT JOIN OTB_SYS_Departments D ON D.DepartmentID = M.DepartmentID WHERE M.OrgID = @OrgID AND M.MemberID = @MemberID ", spOrgID, spResponsiblePerson);
                    MemberToAccess.AddRange(Supervisors);

                    //
                    var oVW_SYS_Rules = db.Queryable<OVW_SYS_Rules>().Where(r => r.DelStatus == "N" && r.OrgID == i_crm.ORIGID)
                        .WhereIF(!string.IsNullOrWhiteSpace(sRuleIDs), r => sRuleIDs.Contains(r.RuleID)).Select(c => c.MemberIDs).ToList();
                    MemberToAccess.AddRange(oVW_SYS_Rules);

                    var BillAuditor = Common.GetSystemSetting(db, i_crm.ORIGID, "BillAuditor");
                    MemberToAccess.Add(BillAuditor);

                    //var saBillInfo = db.Queryable<OTB_OPM_BillInfo, OTB_SYS_Members, OTB_SYS_Members, OTB_OPM_Exhibition, OTB_CRM_Customers>(
                    //    (t1, t2, t3, t4, t5) => new object[]
                    //                        {
                    //                           JoinType.Inner, t1.OrgID == t2.OrgID && t1.CreateUser == t2.MemberID,
                    //                           JoinType.Inner, t1.OrgID == t3.OrgID && t1.ResponsiblePerson == t3.MemberID,
                    //                           JoinType.Inner, t1.OrgID == t4.OrgID && t1.ExhibitionNO == t4.SN.ToString(),
                    //                           JoinType.Left, t1.OrgID == t5.OrgID && t1.Payer == t5.guid
                    //                        }
                    //    )
                    //    .Where((t1, t2, t3, t4, t5) => t1.OrgID == i_crm.ORIGID)
                    //    .WhereIF(!string.IsNullOrEmpty(sExhibitionNO), (t1, t2, t3, t4, t5) => t1.ExhibitionNO == sExhibitionNO)
                    //    .WhereIF(!string.IsNullOrEmpty(sBillNO), (t1, t2, t3, t4, t5) => t1.BillNO == sBillNO)
                    //    .Select((t1, t2, t3, t4, t5) => new View_OPM_BillInfo
                    //    {
                    //        SN = SqlFunc.GetSelfAndAutoFill(t1.SN),
                    //        CreateUserName = t2.MemberName,
                    //        ResponsiblePersonName = t3.MemberName,
                    //        ExhibitioShotName = t4.ExhibitioShotName_CN,
                    //        ExhibitioName = t4.Exhibitioname_TW,
                    //        PayerName = SqlFunc.IIF(SqlFunc.HasValue(t5.CustomerCName), t5.CustomerCName, t5.CustomerEName)
                    //    })
                    //    .OrderBy("t1.CreateDate")
                    //    .ToList();
                    var AccessAccounts = String.Join(",", MemberToAccess);
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, AccessAccounts);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(OpmComService), @"進出口管理模組", @"GetExhibitionBillAuthorize（取得帳單存取權限）", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
            }
            return rm;
        }

        #endregion
    }
}