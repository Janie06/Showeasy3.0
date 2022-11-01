using EasyBL.WebApi.Message;
using Entity.Sugar;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SqlSugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;

namespace EasyBL.WEBAPP.OPM
{
    public class Accounting_QryService : ServiceBase
    {
        public class BillPayerInfo
        {
            public int RowIndex { get; set; }
            public string ExhibitioName { get; set; }
            public string BillNO { get; set; }
            public string Payer { get; set; }
            public string CreateDate { get; set; }
            public string Amount { get; set; }
        }

        #region 成本押款和稅金查詢

        /// <summary>
        /// 成本押款和稅金查詢
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on GetBillsList</param>
        /// <returns></returns>
        public ResponseMessage GetBillsList(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            var list = new List<BillPayerInfo>();
            try
            {
                do
                {
                    var sFeeClass = _fetchString(i_crm, @"FeeClass");
                    var sCheckDateStart = _fetchString(i_crm, @"CheckDateStart");
                    var sCheckDateEnd = _fetchString(i_crm, @"CheckDateEnd");
                    var rCheckDateStart = SqlFunc.ToDate(sCheckDateStart).Date;
                    var rCheckDateEnd = SqlFunc.ToDate(sCheckDateEnd).Date;
                    var saFeeClass = sFeeClass.Split(',');
                    foreach (string feeclass in saFeeClass)
                    {
                        var listIm = db.Queryable<OTB_OPM_ImportExhibition, OTB_OPM_Exhibition>((a, b) =>
                            new object[] { JoinType.Inner, a.OrgID == b.OrgID && a.ExhibitionNO == b.SN.ToString() })
                            .Where((a, b) => a.OrgID == i_crm.ORIGID && (SqlFunc.Contains(a.ActualCost, feeclass) || SqlFunc.Contains(a.ReturnBills, feeclass)))
                            .Select((a, b) => new { a.ActualCost, a.ReturnBills, a.ImportBillName, b.ExhibitionCode }).ToList();
                        foreach (var im in listIm)
                        {
                            var JActualCost = (JObject)JsonConvert.DeserializeObject(im.ActualCost);
                            if (JActualCost[@"FeeItems"] != null)
                            {
                                var jaFeeItems = JActualCost[@"FeeItems"] as JArray;
                                foreach (JObject jo in jaFeeItems)
                                {
                                    if (jo[@"FinancialCode"] != null && jo[@"FinancialCode"].ToString() == feeclass)
                                    {
                                        var sBillNO = jo[@"BillNO"] == null ? @"" : jo[@"BillNO"].ToString();
                                        var sBillPayer = jo[@"BillPayer"] == null ? @"" : jo[@"BillPayer"].ToString();
                                        var saBillPayer = sBillPayer.Split('-');
                                        var oCustomers = new OTB_CRM_Customers();
                                        if (sBillNO != null)
                                        {
                                            oCustomers = db.Queryable<OTB_CRM_Customers, OTB_OPM_BillInfo>((a, b) =>
                                                                     new object[] { JoinType.Inner, a.OrgID == b.OrgID && a.guid == b.Payer })
                                                                    .Where((a, b) => a.OrgID == i_crm.ORIGID && b.BillNO == sBillNO)
                                                                    .Select((a, b) => a).Single();
                                        }
                                        var bpinfo = new BillPayerInfo
                                        {
                                            ExhibitioName = im.ExhibitionCode + @" - " + im.ImportBillName,
                                            BillNO = sBillPayer == @"" ? @"" : saBillPayer[0],
                                            Payer = (oCustomers == null ? @"" : (oCustomers.CustomerNO ?? @"") + @" - ") + (sBillPayer == @"" ? @"" : saBillPayer[1]),
                                            CreateDate = sBillPayer == @"" ? @"" : saBillPayer[2],
                                            Amount = jo[@"FinancialTWAmount"].ToString()
                                        };
                                        list.Add(bpinfo);
                                    }
                                }
                            }
                            var JaReturnBills = (JArray)JsonConvert.DeserializeObject(im.ReturnBills);
                            foreach (JObject bill in JaReturnBills)
                            {
                                if (bill[@"ActualCost"] != null)
                                {
                                    var JReturnActualCost = (JObject)JsonConvert.DeserializeObject(bill[@"ActualCost"].ToString());
                                    if (JReturnActualCost[@"FeeItems"] != null)
                                    {
                                        var jaFeeItems = JReturnActualCost[@"FeeItems"] as JArray;
                                        foreach (JObject jo in jaFeeItems)
                                        {
                                            if (jo[@"FinancialCode"] != null && jo[@"FinancialCode"].ToString() == feeclass)
                                            {
                                                var sBillNO = jo[@"BillNO"] == null ? @"" : jo[@"BillNO"].ToString();
                                                var sBillPayer = jo[@"BillPayer"] == null ? @"" : jo[@"BillPayer"].ToString();
                                                var saBillPayer = sBillPayer.Split('-');
                                                var oCustomers = new OTB_CRM_Customers();
                                                if (sBillNO != null)
                                                {
                                                    oCustomers = db.Queryable<OTB_CRM_Customers, OTB_OPM_BillInfo>((a, b) =>
                                                                             new object[] { JoinType.Inner, a.OrgID == b.OrgID && a.guid == b.Payer })
                                                                            .Where((a, b) => a.OrgID == i_crm.ORIGID && b.BillNO == sBillNO)
                                                                            .Select((a, b) => a).Single();
                                                }
                                                var bpinfo = new BillPayerInfo
                                                {
                                                    ExhibitioName = im.ExhibitionCode + @" - " + im.ImportBillName,
                                                    BillNO = sBillPayer == @"" ? @"" : saBillPayer[0],
                                                    Payer = (oCustomers == null ? @"" : (oCustomers.CustomerNO ?? @"") + @" - ") + (sBillPayer == @"" ? @"" : saBillPayer[1]),
                                                    CreateDate = sBillPayer == @"" ? @"" : saBillPayer[2],
                                                    Amount = jo[@"FinancialTWAmount"].ToString()
                                                };
                                                list.Add(bpinfo);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        var listEx = db.Queryable<OTB_OPM_ExportExhibition, OTB_OPM_Exhibition>((a, b) =>
                            new object[] { JoinType.Inner, a.OrgID == b.OrgID && a.ExhibitionNO == b.SN.ToString() })
                            .Where((a, b) => a.OrgID == i_crm.ORIGID && (SqlFunc.Contains(a.ActualCost, feeclass) || SqlFunc.Contains(a.ReturnBills, feeclass)))
                            .Select((a, b) => new { a.ActualCost, a.ReturnBills, a.ExportBillName, b.ExhibitionCode }).ToList();
                        foreach (var _ex in listEx)
                        {
                            var JActualCost = (JObject)JsonConvert.DeserializeObject(_ex.ActualCost);
                            if (JActualCost[@"FeeItems"] != null)
                            {
                                var jaFeeItems = JActualCost[@"FeeItems"] as JArray;
                                foreach (JObject jo in jaFeeItems)
                                {
                                    if (jo[@"FinancialCode"] != null && jo[@"FinancialCode"].ToString() == feeclass)
                                    {
                                        var sBillNO = jo[@"BillNO"] == null ? @"" : jo[@"BillNO"].ToString();
                                        var sBillPayer = jo[@"BillPayer"] == null ? @"" : jo[@"BillPayer"].ToString();
                                        var saBillPayer = sBillPayer.Split('-');
                                        var oCustomers = new OTB_CRM_Customers();
                                        if (sBillNO != null)
                                        {
                                            oCustomers = db.Queryable<OTB_CRM_Customers, OTB_OPM_BillInfo>((a, b) =>
                                                                     new object[] { JoinType.Inner, a.OrgID == b.OrgID && a.guid == b.Payer })
                                                                    .Where((a, b) => a.OrgID == i_crm.ORIGID && b.BillNO == sBillNO)
                                                                    .Select((a, b) => a).Single();
                                        }
                                        var bpinfo = new BillPayerInfo
                                        {
                                            ExhibitioName = _ex.ExhibitionCode + @" - " + _ex.ExportBillName,
                                            BillNO = sBillPayer == @"" ? @"" : saBillPayer[0],
                                            Payer = (oCustomers == null ? @"" : (oCustomers.CustomerNO ?? @"") + @" - ") + (sBillPayer == @"" ? @"" : saBillPayer[1]),
                                            CreateDate = sBillPayer == @"" ? @"" : saBillPayer[2],
                                            Amount = jo[@"FinancialTWAmount"].ToString()
                                        };
                                        list.Add(bpinfo);
                                    }
                                }
                            }
                            var JaReturnBills = (JArray)JsonConvert.DeserializeObject(_ex.ReturnBills);
                            foreach (JObject bill in JaReturnBills)
                            {
                                if (bill[@"ActualCost"] != null)
                                {
                                    var JReturnActualCost = (JObject)JsonConvert.DeserializeObject(bill[@"ActualCost"].ToString());
                                    if (JReturnActualCost[@"FeeItems"] != null)
                                    {
                                        var jaFeeItems = JReturnActualCost[@"FeeItems"] as JArray;
                                        foreach (JObject jo in jaFeeItems)
                                        {
                                            if (jo[@"FinancialCode"] != null && jo[@"FinancialCode"].ToString() == feeclass)
                                            {
                                                var sBillNO = jo[@"BillNO"] == null ? @"" : jo[@"BillNO"].ToString();
                                                var sBillPayer = jo[@"BillPayer"] == null ? @"" : jo[@"BillPayer"].ToString();
                                                var saBillPayer = sBillPayer.Split('-');
                                                var oCustomers = new OTB_CRM_Customers();
                                                if (sBillNO != null)
                                                {
                                                    oCustomers = db.Queryable<OTB_CRM_Customers, OTB_OPM_BillInfo>((a, b) =>
                                                                             new object[] { JoinType.Inner, a.OrgID == b.OrgID && a.guid == b.Payer })
                                                                            .Where((a, b) => a.OrgID == i_crm.ORIGID && b.BillNO == sBillNO)
                                                                            .Select((a, b) => a).Single();
                                                }
                                                var bpinfo = new BillPayerInfo
                                                {
                                                    ExhibitioName = _ex.ExhibitionCode + @" - " + _ex.ExportBillName,
                                                    BillNO = sBillPayer == @"" ? @"" : saBillPayer[0],
                                                    Payer = (oCustomers == null ? @"" : (oCustomers.CustomerNO ?? @"") + @" - ") + (sBillPayer == @"" ? @"" : saBillPayer[1]),
                                                    CreateDate = sBillPayer == @"" ? @"" : saBillPayer[2],
                                                    Amount = jo[@"FinancialTWAmount"].ToString()
                                                };
                                                list.Add(bpinfo);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        var listOt = db.Queryable<OTB_OPM_OtherExhibition, OTB_OPM_Exhibition>((a, b) =>
                            new object[] { JoinType.Inner, a.OrgID == b.OrgID && a.ExhibitionNO == b.SN.ToString() })
                            .Where((a, b) => a.OrgID == i_crm.ORIGID && SqlFunc.Contains(a.ActualCost, feeclass))
                            .Select((a, b) => new { a.ActualCost, a.ImportBillName, b.ExhibitionCode }).ToList();
                        foreach (var ot in listOt)
                        {
                            var JActualCost = (JObject)JsonConvert.DeserializeObject(ot.ActualCost);
                            if (JActualCost[@"FeeItems"] != null)
                            {
                                var jaFeeItems = JActualCost[@"FeeItems"] as JArray;
                                foreach (JObject jo in jaFeeItems)
                                {
                                    if (jo[@"FinancialCode"] != null && jo[@"FinancialCode"].ToString() == feeclass)
                                    {
                                        var sBillNO = jo[@"BillNO"] == null ? @"" : jo[@"BillNO"].ToString();
                                        var sBillPayer = jo[@"BillPayer"] == null ? @"" : jo[@"BillPayer"].ToString();
                                        var saBillPayer = sBillPayer.Split('-');
                                        var oCustomers = new OTB_CRM_Customers();
                                        if (sBillNO != null)
                                        {
                                            oCustomers = db.Queryable<OTB_CRM_Customers, OTB_OPM_BillInfo>((a, b) =>
                                                                     new object[] { JoinType.Inner, a.OrgID == b.OrgID && a.guid == b.Payer })
                                                                    .Where((a, b) => a.OrgID == i_crm.ORIGID && b.BillNO == sBillNO)
                                                                    .Select((a, b) => a).Single();
                                        }
                                        var bpinfo = new BillPayerInfo
                                        {
                                            ExhibitioName = ot.ExhibitionCode + @" - " + ot.ImportBillName,
                                            BillNO = sBillPayer == @"" ? @"" : saBillPayer[0],
                                            Payer = (oCustomers == null ? @"" : (oCustomers.CustomerNO ?? @"") + @" - ") + (sBillPayer == @"" ? @"" : saBillPayer[1]),
                                            CreateDate = sBillPayer == @"" ? @"" : saBillPayer[2],
                                            Amount = jo[@"FinancialTWAmount"].ToString()
                                        };
                                        list.Add(bpinfo);
                                    }
                                }
                            }
                        }

                        var listOt_TG = db.Queryable<OTB_OPM_OtherExhibitionTG, OTB_OPM_Exhibition>((a, b) =>
                            new object[] { JoinType.Inner, a.OrgID == b.OrgID && a.ExhibitionNO == b.SN.ToString() })
                            .Where((a, b) => a.OrgID == i_crm.ORIGID && SqlFunc.Contains(a.ActualCost, feeclass))
                            .Select((a, b) => new { a.ActualCost, a.ImportBillName, b.ExhibitionCode }).ToList();
                        foreach (var ot in listOt_TG)
                        {
                            var JActualCost = (JObject)JsonConvert.DeserializeObject(ot.ActualCost);
                            if (JActualCost[@"FeeItems"] != null)
                            {
                                var jaFeeItems = JActualCost[@"FeeItems"] as JArray;
                                foreach (JObject jo in jaFeeItems)
                                {
                                    if (jo[@"FinancialCode"] != null && jo[@"FinancialCode"].ToString() == feeclass)
                                    {
                                        var sBillNO = jo[@"BillNO"] == null ? @"" : jo[@"BillNO"].ToString();
                                        var sBillPayer = jo[@"BillPayer"] == null ? @"" : jo[@"BillPayer"].ToString();
                                        var saBillPayer = sBillPayer.Split('-');
                                        var oCustomers = new OTB_CRM_Customers();
                                        if (sBillNO != null)
                                        {
                                            oCustomers = db.Queryable<OTB_CRM_Customers, OTB_OPM_BillInfo>((a, b) =>
                                                                     new object[] { JoinType.Inner, a.OrgID == b.OrgID && a.guid == b.Payer })
                                                                    .Where((a, b) => a.OrgID == i_crm.ORIGID && b.BillNO == sBillNO)
                                                                    .Select((a, b) => a).Single();
                                        }
                                        var bpinfo = new BillPayerInfo
                                        {
                                            ExhibitioName = ot.ExhibitionCode + @" - " + ot.ImportBillName,
                                            BillNO = sBillPayer == @"" ? @"" : saBillPayer[0],
                                            Payer = (oCustomers == null ? @"" : (oCustomers.CustomerNO ?? @"") + @" - ") + (sBillPayer == @"" ? @"" : saBillPayer[1]),
                                            CreateDate = sBillPayer == @"" ? @"" : saBillPayer[2],
                                            Amount = jo[@"FinancialTWAmount"].ToString()
                                        };
                                        list.Add(bpinfo);
                                    }
                                }
                            }
                        }
                    }
                    var listRetn = new List<BillPayerInfo>();
                    var iRowNumber = 1;
                    foreach (BillPayerInfo Info in list)
                    {
                        var sCreateDate = Info.CreateDate ?? @"";
                        var rCreateDate = SqlFunc.ToDate(sCreateDate).Date;
                        if ((sCheckDateStart == @"" && sCheckDateEnd == @"")
                            || (sCreateDate != @"" && sCheckDateStart != @"" && sCheckDateEnd == @"" && rCheckDateStart <= rCreateDate)
                            || (sCreateDate != @"" && sCheckDateStart == @"" && sCheckDateEnd != @"" && rCheckDateEnd >= rCreateDate)
                            || (sCreateDate != @"" && sCheckDateStart != @"" && sCheckDateEnd != @"" && rCheckDateStart <= rCreateDate) && rCheckDateEnd >= rCreateDate)
                        {
                            Info.RowIndex = iRowNumber;
                            listRetn.Add(Info);
                            iRowNumber++;
                        }
                    }
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, listRetn);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Accounting_QryService), @"成本押款和稅金查詢", @"GetBillsList（成本押款和稅金查詢）", @"", @"", @"");
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

        #endregion 成本押款和稅金查詢

        #region 實際成本押款和稅金查詢匯出

        /// <summary>
        /// 實際成本押款和稅金查詢匯出
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on ExcelBillsList</param>
        /// <returns></returns>
        public ResponseMessage ExcelBillsList(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            var list = new List<BillPayerInfo>();
            try
            {
                do
                {
                    var sFeeClass = _fetchString(i_crm, @"FeeClass");
                    var sCheckDateStart = _fetchString(i_crm, @"CheckDateStart");
                    var sCheckDateEnd = _fetchString(i_crm, @"CheckDateEnd");
                    var rCheckDateStart = SqlFunc.ToDate(sCheckDateStart).Date;
                    var rCheckDateEnd = SqlFunc.ToDate(sCheckDateEnd).Date;
                    var saFeeClass = sFeeClass.Split(',');
                    foreach (string feeclass in saFeeClass)
                    {
                        var listIm = db.Queryable<OTB_OPM_ImportExhibition, OTB_OPM_Exhibition>((a, b) =>
                            new object[] { JoinType.Inner, a.OrgID == b.OrgID && a.ExhibitionNO == b.SN.ToString() })
                            .Where((a, b) => a.OrgID == i_crm.ORIGID && (SqlFunc.Contains(a.ActualCost, feeclass) || SqlFunc.Contains(a.ReturnBills, feeclass)))
                            .Select((a, b) => new { a.ActualCost, a.ReturnBills, a.ImportBillName, b.ExhibitionCode }).ToList();
                        foreach (var im in listIm)
                        {
                            var JActualCost = (JObject)JsonConvert.DeserializeObject(im.ActualCost);
                            if (JActualCost[@"FeeItems"] != null)
                            {
                                var jaFeeItems = JActualCost[@"FeeItems"] as JArray;
                                foreach (JObject jo in jaFeeItems)
                                {
                                    if (jo[@"FinancialCode"] != null && jo[@"FinancialCode"].ToString() == feeclass)
                                    {
                                        var sBillNO = jo[@"BillNO"] == null ? @"" : jo[@"BillNO"].ToString();
                                        var sBillPayer = jo[@"BillPayer"] == null ? @"" : jo[@"BillPayer"].ToString();
                                        var saBillPayer = sBillPayer.Split('-');
                                        var oCustomers = new OTB_CRM_Customers();
                                        if (sBillNO != null)
                                        {
                                            oCustomers = db.Queryable<OTB_CRM_Customers, OTB_OPM_BillInfo>((a, b) =>
                                                                     new object[] { JoinType.Inner, a.OrgID == b.OrgID && a.guid == b.Payer })
                                                                    .Where((a, b) => a.OrgID == i_crm.ORIGID && b.BillNO == sBillNO)
                                                                    .Select((a, b) => a).Single();
                                        }
                                        var bpinfo = new BillPayerInfo
                                        {
                                            ExhibitioName = im.ExhibitionCode + @" - " + im.ImportBillName,
                                            BillNO = sBillPayer == @"" ? @"" : saBillPayer[0],
                                            Payer = (oCustomers == null ? @"" : (oCustomers.CustomerNO ?? @"") + @" - ") + (sBillPayer == @"" ? @"" : saBillPayer[1]),
                                            CreateDate = sBillPayer == @"" ? @"" : saBillPayer[2],
                                            Amount = jo[@"FinancialTWAmount"].ToString()
                                        };
                                        list.Add(bpinfo);
                                    }
                                }
                            }
                            var JaReturnBills = (JArray)JsonConvert.DeserializeObject(im.ReturnBills);
                            foreach (JObject bill in JaReturnBills)
                            {
                                if (bill[@"ActualCost"] != null)
                                {
                                    var JReturnActualCost = (JObject)JsonConvert.DeserializeObject(bill[@"ActualCost"].ToString());
                                    if (JReturnActualCost[@"FeeItems"] != null)
                                    {
                                        var jaFeeItems = JReturnActualCost[@"FeeItems"] as JArray;
                                        foreach (JObject jo in jaFeeItems)
                                        {
                                            if (jo[@"FinancialCode"] != null && jo[@"FinancialCode"].ToString() == feeclass)
                                            {
                                                var sBillNO = jo[@"BillNO"] == null ? @"" : jo[@"BillNO"].ToString();
                                                var sBillPayer = jo[@"BillPayer"] == null ? @"" : jo[@"BillPayer"].ToString();
                                                var saBillPayer = sBillPayer.Split('-');
                                                var oCustomers = new OTB_CRM_Customers();
                                                if (sBillNO != null)
                                                {
                                                    oCustomers = db.Queryable<OTB_CRM_Customers, OTB_OPM_BillInfo>((a, b) =>
                                                                             new object[] { JoinType.Inner, a.OrgID == b.OrgID && a.guid == b.Payer })
                                                                            .Where((a, b) => a.OrgID == i_crm.ORIGID && b.BillNO == sBillNO)
                                                                            .Select((a, b) => a).Single();
                                                }
                                                var bpinfo = new BillPayerInfo
                                                {
                                                    ExhibitioName = im.ExhibitionCode + @" - " + im.ImportBillName,
                                                    BillNO = sBillPayer == @"" ? @"" : saBillPayer[0],
                                                    Payer = (oCustomers == null ? @"" : (oCustomers.CustomerNO ?? @"") + @" - ") + (sBillPayer == @"" ? @"" : saBillPayer[1]),
                                                    CreateDate = sBillPayer == @"" ? @"" : saBillPayer[2],
                                                    Amount = jo[@"FinancialTWAmount"].ToString()
                                                };
                                                list.Add(bpinfo);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        var listEx = db.Queryable<OTB_OPM_ExportExhibition, OTB_OPM_Exhibition>((a, b) =>
                            new object[] { JoinType.Inner, a.OrgID == b.OrgID && a.ExhibitionNO == b.SN.ToString() })
                            .Where((a, b) => a.OrgID == i_crm.ORIGID && (SqlFunc.Contains(a.ActualCost, feeclass) || SqlFunc.Contains(a.ReturnBills, feeclass)))
                            .Select((a, b) => new { a.ActualCost, a.ReturnBills, a.ExportBillName, b.ExhibitionCode }).ToList();
                        foreach (var _ex in listEx)
                        {
                            var JActualCost = (JObject)JsonConvert.DeserializeObject(_ex.ActualCost);
                            if (JActualCost[@"FeeItems"] != null)
                            {
                                var jaFeeItems = JActualCost[@"FeeItems"] as JArray;
                                foreach (JObject jo in jaFeeItems)
                                {
                                    if (jo[@"FinancialCode"] != null && jo[@"FinancialCode"].ToString() == feeclass)
                                    {
                                        var sBillNO = jo[@"BillNO"] == null ? @"" : jo[@"BillNO"].ToString();
                                        var sBillPayer = jo[@"BillPayer"] == null ? @"" : jo[@"BillPayer"].ToString();
                                        var saBillPayer = sBillPayer.Split('-');
                                        var oCustomers = new OTB_CRM_Customers();
                                        if (sBillNO != null)
                                        {
                                            oCustomers = db.Queryable<OTB_CRM_Customers, OTB_OPM_BillInfo>((a, b) =>
                                                                     new object[] { JoinType.Inner, a.OrgID == b.OrgID && a.guid == b.Payer })
                                                                    .Where((a, b) => a.OrgID == i_crm.ORIGID && b.BillNO == sBillNO)
                                                                    .Select((a, b) => a).Single();
                                        }
                                        var bpinfo = new BillPayerInfo
                                        {
                                            ExhibitioName = _ex.ExhibitionCode + @" - " + _ex.ExportBillName,
                                            BillNO = sBillPayer == @"" ? @"" : saBillPayer[0],
                                            Payer = (oCustomers == null ? @"" : (oCustomers.CustomerNO ?? @"") + @" - ") + (sBillPayer == @"" ? @"" : saBillPayer[1]),
                                            CreateDate = sBillPayer == @"" ? @"" : saBillPayer[2],
                                            Amount = jo[@"FinancialTWAmount"].ToString()
                                        };
                                        list.Add(bpinfo);
                                    }
                                }
                            }
                            var JaReturnBills = (JArray)JsonConvert.DeserializeObject(_ex.ReturnBills);
                            foreach (JObject bill in JaReturnBills)
                            {
                                if (bill[@"ActualCost"] != null)
                                {
                                    var JReturnActualCost = (JObject)JsonConvert.DeserializeObject(bill[@"ActualCost"].ToString());
                                    if (JReturnActualCost[@"FeeItems"] != null)
                                    {
                                        var jaFeeItems = JReturnActualCost[@"FeeItems"] as JArray;
                                        foreach (JObject jo in jaFeeItems)
                                        {
                                            if (jo[@"FinancialCode"] != null && jo[@"FinancialCode"].ToString() == feeclass)
                                            {
                                                var sBillNO = jo[@"BillNO"] == null ? @"" : jo[@"BillNO"].ToString();
                                                var sBillPayer = jo[@"BillPayer"] == null ? @"" : jo[@"BillPayer"].ToString();
                                                var saBillPayer = sBillPayer.Split('-');
                                                var oCustomers = new OTB_CRM_Customers();
                                                if (sBillNO != null)
                                                {
                                                    oCustomers = db.Queryable<OTB_CRM_Customers, OTB_OPM_BillInfo>((a, b) =>
                                                                             new object[] { JoinType.Inner, a.OrgID == b.OrgID && a.guid == b.Payer })
                                                                            .Where((a, b) => a.OrgID == i_crm.ORIGID && b.BillNO == sBillNO)
                                                                            .Select((a, b) => a).Single();
                                                }
                                                var bpinfo = new BillPayerInfo
                                                {
                                                    ExhibitioName = _ex.ExhibitionCode + @" - " + _ex.ExportBillName,
                                                    BillNO = sBillPayer == @"" ? @"" : saBillPayer[0],
                                                    Payer = (oCustomers == null ? @"" : (oCustomers.CustomerNO ?? @"") + @" - ") + (sBillPayer == @"" ? @"" : saBillPayer[1]),
                                                    CreateDate = sBillPayer == @"" ? @"" : saBillPayer[2],
                                                    Amount = jo[@"FinancialTWAmount"].ToString()
                                                };
                                                list.Add(bpinfo);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        var listOt = db.Queryable<OTB_OPM_OtherExhibition, OTB_OPM_Exhibition>((a, b) =>
                            new object[] { JoinType.Inner, a.OrgID == b.OrgID && a.ExhibitionNO == b.SN.ToString() })
                            .Where((a, b) => a.OrgID == i_crm.ORIGID && SqlFunc.Contains(a.ActualCost, feeclass))
                            .Select((a, b) => new { a.ActualCost, a.ImportBillName, b.ExhibitionCode }).ToList();
                        foreach (var ot in listOt)
                        {
                            var JActualCost = (JObject)JsonConvert.DeserializeObject(ot.ActualCost);
                            if (JActualCost[@"FeeItems"] != null)
                            {
                                var jaFeeItems = JActualCost[@"FeeItems"] as JArray;
                                foreach (JObject jo in jaFeeItems)
                                {
                                    if (jo[@"FinancialCode"] != null && jo[@"FinancialCode"].ToString() == feeclass)
                                    {
                                        var sBillNO = jo[@"BillNO"] == null ? @"" : jo[@"BillNO"].ToString();
                                        var sBillPayer = jo[@"BillPayer"] == null ? @"" : jo[@"BillPayer"].ToString();
                                        var saBillPayer = sBillPayer.Split('-');
                                        var oCustomers = new OTB_CRM_Customers();
                                        if (sBillNO != null)
                                        {
                                            oCustomers = db.Queryable<OTB_CRM_Customers, OTB_OPM_BillInfo>((a, b) =>
                                                                     new object[] { JoinType.Inner, a.OrgID == b.OrgID && a.guid == b.Payer })
                                                                    .Where((a, b) => a.OrgID == i_crm.ORIGID && b.BillNO == sBillNO)
                                                                    .Select((a, b) => a).Single();
                                        }
                                        var bpinfo = new BillPayerInfo
                                        {
                                            ExhibitioName = ot.ExhibitionCode + @" - " + ot.ImportBillName,
                                            BillNO = sBillPayer == @"" ? @"" : saBillPayer[0],
                                            Payer = (oCustomers == null ? @"" : (oCustomers.CustomerNO ?? @"") + @" - ") + (sBillPayer == @"" ? @"" : saBillPayer[1]),
                                            CreateDate = sBillPayer == @"" ? @"" : saBillPayer[2],
                                            Amount = jo[@"FinancialTWAmount"].ToString()
                                        };
                                        list.Add(bpinfo);
                                    }
                                }
                            }
                        }

                        var listOt_TG = db.Queryable<OTB_OPM_OtherExhibitionTG, OTB_OPM_Exhibition>((a, b) =>
                            new object[] { JoinType.Inner, a.OrgID == b.OrgID && a.ExhibitionNO == b.SN.ToString() })
                            .Where((a, b) => a.OrgID == i_crm.ORIGID && SqlFunc.Contains(a.ActualCost, feeclass))
                            .Select((a, b) => new { a.ActualCost, a.ImportBillName, b.ExhibitionCode }).ToList();
                        foreach (var ot in listOt_TG)
                        {
                            var JActualCost = (JObject)JsonConvert.DeserializeObject(ot.ActualCost);
                            if (JActualCost[@"FeeItems"] != null)
                            {
                                var jaFeeItems = JActualCost[@"FeeItems"] as JArray;
                                foreach (JObject jo in jaFeeItems)
                                {
                                    if (jo[@"FinancialCode"] != null && jo[@"FinancialCode"].ToString() == feeclass)
                                    {
                                        var sBillNO = jo[@"BillNO"] == null ? @"" : jo[@"BillNO"].ToString();
                                        var sBillPayer = jo[@"BillPayer"] == null ? @"" : jo[@"BillPayer"].ToString();
                                        var saBillPayer = sBillPayer.Split('-');
                                        var oCustomers = new OTB_CRM_Customers();
                                        if (sBillNO != null)
                                        {
                                            oCustomers = db.Queryable<OTB_CRM_Customers, OTB_OPM_BillInfo>((a, b) =>
                                                                     new object[] { JoinType.Inner, a.OrgID == b.OrgID && a.guid == b.Payer })
                                                                    .Where((a, b) => a.OrgID == i_crm.ORIGID && b.BillNO == sBillNO)
                                                                    .Select((a, b) => a).Single();
                                        }
                                        var bpinfo = new BillPayerInfo
                                        {
                                            ExhibitioName = ot.ExhibitionCode + @" - " + ot.ImportBillName,
                                            BillNO = sBillPayer == @"" ? @"" : saBillPayer[0],
                                            Payer = (oCustomers == null ? @"" : (oCustomers.CustomerNO ?? @"") + @" - ") + (sBillPayer == @"" ? @"" : saBillPayer[1]),
                                            CreateDate = sBillPayer == @"" ? @"" : saBillPayer[2],
                                            Amount = jo[@"FinancialTWAmount"].ToString()
                                        };
                                        list.Add(bpinfo);
                                    }
                                }
                            }
                        }
                    }
                    var listRetn = new List<BillPayerInfo>();
                    var iRowNumber = 1;
                    foreach (BillPayerInfo Info in list)
                    {
                        var sCreateDate = Info.CreateDate ?? @"";
                        var rCreateDate = SqlFunc.ToDate(sCreateDate).Date;
                        if ((sCheckDateStart == @"" && sCheckDateEnd == @"")
                            || (sCreateDate != @"" && sCheckDateStart != @"" && sCheckDateEnd == @"" && rCheckDateStart <= Convert.ToDateTime(sCreateDate))
                            || (sCreateDate != @"" && sCheckDateStart == @"" && sCheckDateEnd != @"" && rCheckDateEnd >= rCreateDate)
                            || (sCreateDate != @"" && sCheckDateStart != @"" && sCheckDateEnd != @"" && rCheckDateStart <= rCreateDate) && rCheckDateEnd >= rCreateDate)
                        {
                            Info.RowIndex = iRowNumber;
                            listRetn.Add(Info);
                            iRowNumber++;
                        }
                    }

                    var sFileName = @"實際成本押款和稅金資料";
                    var oHeader = new Dictionary<string, string>();
                    var sHeader1 = @"帳單號碼";
                    var sHeader2 = @"活動/展覽名稱";
                    var sHeader3 = @"付款人";
                    var sHeader4 = @"金額";
                    var sHeader5 = @"創建時間";
                    if (i_crm.LANG == @"zh")
                    {
                        sHeader1 = ChineseStringUtility.ToSimplified(sHeader1);
                        sHeader2 = ChineseStringUtility.ToSimplified(sHeader2);
                        sHeader3 = ChineseStringUtility.ToSimplified(sHeader3);
                        sHeader4 = ChineseStringUtility.ToSimplified(sHeader4);
                        sHeader5 = ChineseStringUtility.ToSimplified(sHeader5);
                    }
                    oHeader.Add(@"BillNO", sHeader1);
                    oHeader.Add(@"ExhibitioName", sHeader2);
                    oHeader.Add(@"Payer", sHeader3);
                    oHeader.Add(@"Amount", sHeader4);
                    oHeader.Add(@"CreateDate", sHeader5);
                    var bOk = new ExcelService().CreateExcelByList(listRetn, out string sPath, oHeader, null, sFileName);
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, sPath);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Accounting_QryService), @"成本押款和稅金查詢匯出", @"ExcelBillsList（成本押款和稅金查詢匯出）", @"", @"", @"");
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

        #endregion 實際成本押款和稅金查詢匯出
    }
}