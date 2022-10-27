using Aspose.Cells;
using EasyBL.WebApi.Message;
using Entity.Sugar;
using Entity.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SqlSugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;

namespace EasyBL.WEBAPP.OPM
{
    public class BillStatus_QryService : ServiceBase
    {
        #region 賬單狀態（分頁查詢）

        /// <summary>
        /// 賬單狀態（分頁查詢）
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage QueryPage(RequestMessage i_crm)
        {

            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var pml = new PageModel
                    {
                        PageIndex = _fetchInt(i_crm, @"pageIndex"),
                        PageSize = _fetchInt(i_crm, @"pageSize")
                    };
                    var iPageCount = 0;
                    var sSortField = _fetchString(i_crm, @"sortField");
                    var sSortOrder = _fetchString(i_crm, @"sortOrder");
                    var sBillNO = _fetchString(i_crm, @"BillNO");
                    var sExhibitionName = _fetchString(i_crm, @"ExhibitionName");
                    var sExhibitionSN = _fetchString(i_crm, @"ExhibitionSN");
                    var sPayer = _fetchString(i_crm, @"Payer");
                    var sPayerGuid = _fetchString(i_crm, @"PayerGuid");
                    var sResponsiblePerson = _fetchString(i_crm, @"ResponsiblePerson");
                    var sBillStatus = _fetchString(i_crm, @"BillStatus");
                    string sSearchBetween = _fetchString(i_crm, "SearchBetween");   //查詢區間
                    string sBillDateStart = _fetchString(i_crm, "BillDateStart");   //區間起始
                    string sBillDateEnd = _fetchString(i_crm, "BillDateEnd");       //區間結束
                    DateTime dtBillDateStart = DateTime.Now;
                    DateTime dtBillDateEnd = DateTime.Now;

                    var Filter = new CVPFilter();
                    var bExcel = _fetchBool(i_crm, @"Excel");
                    var sExcelType = _fetchString(i_crm, @"ExcelType");
                    var spOrgID = new SugarParameter("@OrgID", i_crm.ORIGID);
                    var spUserID = new SugarParameter("@UserID", i_crm.USERID);

                    var saRoles = db.Queryable<OTB_SYS_MembersToRule>().Where(x => x.OrgID == i_crm.ORIGID && x.MemberID == i_crm.USERID).Select(x => x.RuleID).ToList().ToArray();
                    var saDeptIdsByUser = db.Ado.SqlQuery<string>("SELECT DepartmentId FROM [dbo].[OFN_SYS_GetChilDepartmentIdByUserID](@OrgID,@UserID)", spOrgID, spUserID).ToArray();
                    var saChildUserIds = db.Ado.SqlQuery<string>("SELECT MemberID FROM [dbo].[OFN_SYS_GetMemberIDDownByChief](@OrgID,@UserID)", spOrgID, spUserID).ToArray();
                    var sDeptId = db.Queryable<OTB_SYS_Members>().Single(x => x.OrgID == i_crm.ORIGID && x.MemberID == i_crm.USERID).DepartmentID;
                    var saStatus = sBillStatus.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    pml.DataList = db.Queryable<OTB_OPM_BillInfo, OTB_SYS_Members, OTB_SYS_Members, OTB_OPM_Exhibition, OTB_CRM_Customers, OTB_OPM_ExportExhibition, OTB_OPM_ImportExhibition, OTB_OPM_OtherExhibition, OTB_OPM_OtherExhibitionTG>
                        ((t1, t2, t3, t4, t5, t6, t7, t8, t9) =>
                        new object[] {
                                JoinType.Inner, t1.OrgID == t2.OrgID && t1.CreateUser == t2.MemberID,
                                JoinType.Left, t1.OrgID == t3.OrgID && t1.ResponsiblePerson == t3.MemberID,
                                JoinType.Left, t1.ExhibitionNO == t4.SN.ToString(),
                                JoinType.Left, t1.Payer == t5.guid,
                                JoinType.Left, t1.OrgID == t6.OrgID && t1.ParentId == t6.ExportBillNO,
                                JoinType.Left, t1.OrgID == t7.OrgID && t1.ParentId == t7.ImportBillNO,
                                JoinType.Left, t1.OrgID == t8.OrgID && t1.ParentId == t8.Guid,
                                JoinType.Left, t1.OrgID == t8.OrgID && t1.ParentId == t9.Guid}
                        )
                        .Where((t1) => t1.OrgID == i_crm.ORIGID)
                        .WhereIF(!string.IsNullOrEmpty(sBillNO), (t1) => t1.BillNO.Contains(sBillNO))
                        .WhereIF(!string.IsNullOrEmpty(sExhibitionName), (t1, t2, t3, t4) => (t4.Exhibitioname_TW + t4.Exhibitioname_EN + t4.ExhibitioShotName_TW).Contains(sExhibitionName))
                        .WhereIF(!string.IsNullOrEmpty(sExhibitionSN), (t1, t2, t3, t4) => SqlFunc.ToString(t4.SN) == sExhibitionSN)
                        .WhereIF(!string.IsNullOrEmpty(sPayer), (t1, t2, t3, t4, t5) => (t5.CustomerCName + t5.CustomerEName).Contains(sPayer))
                        .WhereIF(!string.IsNullOrEmpty(sPayerGuid), (t1, t2, t3, t4, t5) => t5.guid == sPayerGuid)
                        .WhereIF(!string.IsNullOrEmpty(sResponsiblePerson), (t1) => t1.ResponsiblePerson == sResponsiblePerson)
                        .WhereIF(saStatus.Length > 0, (t1) => SqlFunc.ContainsArray(saStatus, t1.AuditVal))
                        //帳單區間
                        .WhereIF(sSearchBetween == "1" && DateTime.TryParse(sBillDateStart, out dtBillDateStart), (t1) => SqlFunc.ToDate(t1.BillFirstCheckDate) >= dtBillDateStart)
                        .WhereIF(sSearchBetween == "1" && DateTime.TryParse(sBillDateEnd, out dtBillDateEnd), (t1) => SqlFunc.ToDate(t1.BillFirstCheckDate) < dtBillDateEnd)
                        //銷帳區間
                        .WhereIF(sSearchBetween == "2" && DateTime.TryParse(sBillDateStart, out dtBillDateStart), (t1) => SqlFunc.ToDate(t1.BillWriteOffDate) >= dtBillDateStart)
                        .WhereIF(sSearchBetween == "2" && DateTime.TryParse(sBillDateEnd, out dtBillDateEnd), (t1) => SqlFunc.ToDate(t1.BillWriteOffDate) < dtBillDateEnd)
                        //帳單創建區間
                        .WhereIF(sSearchBetween == "3" && DateTime.TryParse(sBillDateStart, out dtBillDateStart), (t1) => SqlFunc.ToDate(t1.BillCreateDate) >= dtBillDateStart)
                        .WhereIF(sSearchBetween == "3" && DateTime.TryParse(sBillDateEnd, out dtBillDateEnd), (t1) => SqlFunc.ToDate(t1.BillCreateDate) < dtBillDateEnd)
                        //Excel下載,帳單成本金額
                        .WhereIF(bExcel && sExcelType == "BillAndPrice", (t1) => t1.AuditVal != "6")
                        .Where((t1) => t1.CreateUser == i_crm.USERID || t1.ResponsiblePerson == i_crm.USERID
                        ||
                       //SqlFunc.Subqueryable<OTB_SYS_Members>().Where(c => c.MemberID == t1.ResponsiblePerson && c.OrgID == t1.OrgID).Select(c => c.DepartmentID) == sDeptId ||  //挑選出相同部門的資料
                       //SqlFunc.Subqueryable<OTB_SYS_Members>().Where(p => p.MemberID == t1.CreateUser && p.OrgID == t1.OrgID).Select(c => c.DepartmentID) == sDeptId ////挑選出相同部門的資料
                       //||
                       SqlFunc.Subqueryable<OTB_SYS_Members>().Where(p => p.MemberID == t1.ResponsiblePerson && p.OrgID == t1.OrgID).Select(c => c.ImmediateSupervisor) == i_crm.USERID
                       ||
                       SqlFunc.MappingColumn(t1.BillNO, "[dbo].[OFN_OPM_CheckBillCreateUser](t1.BillType,t1.ParentId)") == i_crm.USERID
                       || SqlFunc.ContainsArray(saChildUserIds, t1.ResponsiblePerson) || SqlFunc.ContainsArray(saRoles, "Account") || SqlFunc.ContainsArray(saRoles, "CDD") || SqlFunc.ContainsArray(saRoles, "Admin") || SqlFunc.ContainsArray(saRoles, "Manager"))
                        .Select((t1, t2, t3, t4, t5, t6, t7, t8, t9) => new View_OPM_BillInfo
                        {
                            OrgID = t1.OrgID,
                            BillNO = t1.BillNO,
                            ExhibitioName = t4.Exhibitioname_TW,
                            BillType = t1.BillType,
                            ParentId = t1.ParentId,
                            IsRetn = t1.IsRetn,
                            ReFlow = t1.ReFlow,
                            PayerName = SqlFunc.IIF(SqlFunc.HasValue(t5.CustomerCName), t5.CustomerCName, t5.CustomerEName),
                            ResponsiblePersonName = t3.MemberName,
                            Currency = t1.Currency,
                            Payer = t1.Payer,
                            Number = t1.Number + t1.Unit,
                            Weight = t1.Weight,
                            Volume = t1.Volume,
                            CreateUserName = t2.MemberName,
                            BillCreateDate = t1.BillCreateDate,
                            AuditVal = t1.AuditVal,
                            ExchangeRate = t1.ExchangeRate,
                            Advance = t1.Advance,
                            AmountSum = t1.AmountSum,
                            TaxSum = t1.TaxSum,
                            AmountTaxSum = t1.AmountTaxSum,
                            TotalReceivable = t1.TotalReceivable,
                            FeeItems = t1.FeeItems,
                            CreateDate = SqlFunc.MappingColumn(t1.CreateDate, "CASE BillType WHEN 'ExhibitionExport_Upd' THEN t6.CreateDate WHEN 'ExhibitionImport_Upd' THEN t7.CreateDate WHEN 'OtherBusiness_Upd' THEN t8.CreateDate WHEN 'OtherExhibitionTG_Upd' THEN t9.CreateDate END"),
                            _ExchangeRate = SqlFunc.MappingColumn(t1.SN, "CONVERT(decimal,CASE ISNULL(t1.ExchangeRate,'') WHEN '' THEN '0' ELSE t1.ExchangeRate END)"),
                            _Advance = SqlFunc.MappingColumn(t1.SN, "CONVERT(decimal,REPLACE(t1.Advance,',',''))"),
                            _AmountSum = SqlFunc.MappingColumn(t1.SN, "CONVERT(decimal,REPLACE(t1.AmountSum,',',''))"),
                            _TaxSum = SqlFunc.MappingColumn(t1.SN, "CONVERT(decimal,REPLACE(t1.TaxSum,',',''))"),
                            _AmountTaxSum = SqlFunc.MappingColumn(t1.SN, "CONVERT(decimal,REPLACE(t1.AmountTaxSum,',',''))"),
                            _TotalReceivable = SqlFunc.MappingColumn(t1.SN, "CONVERT(decimal,REPLACE(t1.TotalReceivable,',',''))"),
                            //BillCheckDate = SqlFunc.MappingColumn(t1.BillCheckDate, "CONVERT(datetime,CASE ISNULL(BillCheckDate,'') WHEN '' THEN '0' ELSE BillCheckDate END)"),
                            //BillWriteOffDate = SqlFunc.MappingColumn(t1.BillCheckDate, "CONVERT(datetime,CASE ISNULL(BillWriteOffDate,'') WHEN '' THEN '0' ELSE BillWriteOffDate END)")

                            // 20180308 可能要額外判斷欄位是否為NULL或空值，不然會有轉換資料時會出錯
                            //_Advance = SqlFunc.MappingColumn(t1.SN, "Convert(decimal,case isnull(Advance,'') when '' then '0' else REPLACE(Advance,',','') end)"),
                            //_AmountSum = SqlFunc.MappingColumn(t1.SN, "Convert(decimal,case isnull(AmountSum,'') when '' then '0' else REPLACE(AmountSum,',','') end)"),
                            //_TaxSum = SqlFunc.MappingColumn(t1.SN, "Convert(decimal,case isnull(TaxSum,'') when '' then '0' else REPLACE(TaxSum,',','') end)"),
                            //_AmountTaxSum = SqlFunc.MappingColumn(t1.SN, "Convert(decimal,case isnull(AmountTaxSum,'') when '' then '0' else REPLACE(AmountTaxSum,',','') end)"),
                            //_TotalReceivable = SqlFunc.MappingColumn(t1.SN, "Convert(decimal,case isnull(TotalReceivable,'') when '' then '0' else REPLACE(TotalReceivable,',','') end)")

                        })
                        .MergeTable()
                        .OrderBy(("_ExchangeRate,_Advance,_AmountSum,_TaxSum,_AmountTaxSum,_TotalReceivable".Contains(sSortField) ? "_" : "") + sSortField, sSortOrder)
                        .ToPageList(pml.PageIndex, bExcel ? 100000 : pml.PageSize, ref iPageCount);
                    pml.Total = iPageCount;

                    Dictionary<string, decimal> dicCurrencyInfo = new Dictionary<string, decimal>();

                    foreach (View_OPM_BillInfo billinfo in pml.DataList as List<View_OPM_BillInfo>)
                    {
                        billinfo._ExchangeRate = 0M;
                        /* 匯率抓取 */
                        if (billinfo.CreateDate != default(DateTime))
                        {
                            if (!string.IsNullOrEmpty(billinfo.Currency))
                            {
                                int iYear = SqlFunc.ToDate(billinfo.CreateDate).Year;
                                int iMonth = SqlFunc.ToDate(billinfo.CreateDate).Month;

                                string sKey = $"{ iYear }|{iMonth}|{billinfo.Currency}";

                                if (!dicCurrencyInfo.Keys.Contains(sKey))
                                {
                                    var data = db.Queryable<OTB_SYS_Currency>()
                                                 .Single(p => p.OrgID == billinfo.OrgID &&
                                                              p.year == iYear &&
                                                              p.month == iMonth &&
                                                              p.currency == billinfo.Currency);

                                    if (data != null)
                                    {
                                        dicCurrencyInfo.Add(sKey, data.exchange_rate);
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }

                                billinfo.ExchangeRate = dicCurrencyInfo[sKey].ToString("#0.##");
                                billinfo._ExchangeRate = dicCurrencyInfo[sKey];

                            }
                        }
                    }

                    rm = new SuccessResponseMessage(null, i_crm);
                    if (bExcel)
                    {
                        var AllCBMUsage = CommonRPT.GetAllCBMUsages(db, i_crm.ORIGID);
                        #region 設定幣別名稱
                        var CurrencyName = CommonRPT.GetCurrencyUnit(i_crm.ORIGID);
                        var RoundingPoint = CommonRPT.GetRoundingPoint(i_crm.ORIGID);
                        var DollorFormated = "N" + RoundingPoint.ToString();
                        #endregion
                        var sFileName = "";
                        var oHeader = new Dictionary<string, string>();
                        var listMerge = new List<Dictionary<string, int>>();
                        var dicAlain = new Dictionary<string, string>();
                        var saBillInfo = pml.DataList as List<View_OPM_BillInfo>;
                        var dt_new = saBillInfo.ListToDataTable<View_OPM_BillInfo>();
                        var saBillPrepayFee = Common.GetSystemSetting(db, i_crm.ORIGID, "PrepayForCustomerCode");
                        var BillPrepayFeeList = saBillPrepayFee.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        var saActualPrepayFee = Common.GetSystemSetting(db, i_crm.ORIGID, "ActualPrepayForCustomerCode");
                        var ActualPrepayFeeList = saActualPrepayFee.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        switch (sExcelType)
                        {
                            case "BillList":
                                sFileName = "帳單狀態查詢資料";
                                oHeader = new Dictionary<string, string>
                                                {
                                                    { "RowIndex", "項次" },
                                                    { "BillNO", "帳單號碼" },
                                                    { "ExhibitioName", "展覽/活動名稱" },
                                                    { "PayerName", "付款人" },
                                                    { "ResponsiblePersonName", "負責業務" },
                                                    { "Currency", "幣別代號" },
                                                    //{ "ExchangeRate", "匯率" },
                                                    { "Advance", "預收金額(A)" },
                                                    { "AmountSum", "未稅金額(B)" },
                                                    { "TaxSum", "稅金(C)" },
                                                    { "AmountTaxSum", "含稅金額(D)=B+C" },
                                                    { "TotalReceivable", "總應收D-A" },
                                                    { "ExchangeRate", "匯率(會計用)(E)" },
                                                    { "UntaxAmountMutiRate", $"未稅金額({CurrencyName})(B*E)" }, //new
                                                    { "BillReimburseAmount", $"帳單代墊款({CurrencyName})" },
                                                    { "CreateUserName", "創建人員" },
                                                    { "BillCreateDate", "帳單創建時間" },
                                                    { "AuditVal", "帳單狀態" }
                                                };
                                dt_new.Columns.Add("UntaxAmountMutiRate");
                                dt_new.Columns.Add("BillReimburseAmount");
                                double UntaxAmountMutiRateTotalByLocalCurrency = 0;
                                decimal BillReimburseAmountTotalByLocalCurrency = 0;

                                foreach (DataRow row in dt_new.Rows)
                                {
                                    double.TryParse(row["ExchangeRate"].ToString(), out var dExchangeRate);
                                    double.TryParse(row["AmountSum"].ToString(), out var dAmountSum);

                                    row["Advance"] = row["Advance"].ToString() == "" ? "0" : $"{Convert.ToDouble(row["Advance"].ToString()):N2}";
                                    row["AmountSum"] = row["AmountSum"].ToString() == "" ? "0" : $"{dAmountSum:N2}";
                                    row["TaxSum"] = row["TaxSum"].ToString() == "" ? "0" : $"{Convert.ToDouble(row["TaxSum"].ToString()):N2}";
                                    row["AmountTaxSum"] = row["AmountTaxSum"].ToString() == "" ? "0" : $"{Convert.ToDouble(row["AmountTaxSum"].ToString()):N2}";
                                    row["TotalReceivable"] = row["TotalReceivable"].ToString() == "" ? "0" : $"{Convert.ToDouble(row["TotalReceivable"].ToString()):N2}";
                                    row["BillCreateDate"] = Convert.ToDateTime(row["BillCreateDate"].ToString()).ToString("yyyy/MM/dd HH:mm");
                                    //換算匯率

                                    //帳單代墊款(NTD)
                                    var BillFeeItemList = CommonRPT.ToFeeItems(row["FeeItems"].ObjToString());
                                    var BillReimburseAmount = BillFeeItemList.Where(c => BillPrepayFeeList.Contains(c.FinancialCode)).Sum(c => c.TWAmount); //帳單內特定費用代碼資料
                                    var BillReimburseAmountLocalCurrency = Rounding(BillReimburseAmount * dExchangeRate.ObjToDecimal(), RoundingPoint);
                                    row["BillReimburseAmount"] = $"{BillReimburseAmountLocalCurrency:N2}";
                                    BillReimburseAmountTotalByLocalCurrency += BillReimburseAmountLocalCurrency;
                                    //未稅金額(NTD)
                                    var UntaxAmountSubtotalLocalCurrency = Rounding(dAmountSum * dExchangeRate, RoundingPoint);
                                    row["UntaxAmountMutiRate"] = $"{UntaxAmountSubtotalLocalCurrency:N2}";
                                    UntaxAmountMutiRateTotalByLocalCurrency += UntaxAmountSubtotalLocalCurrency;
                                    var sBill_Status = "";
                                    switch (row["AuditVal"].ToString())
                                    {
                                        case "0"://未提交審核
                                            sBill_Status = "未提交審核";
                                            break;

                                        case "1"://提交審核中
                                            sBill_Status = "提交審核中";
                                            break;

                                        case "2"://已審核
                                            sBill_Status = "已審核";
                                            break;

                                        case "3"://不通過
                                            sBill_Status = "不通過";
                                            break;

                                        case "4"://已銷帳
                                            sBill_Status = "已銷帳";
                                            break;

                                        case "5"://已過帳
                                            sBill_Status = "已過帳";
                                            break;

                                        case "6"://已作廢
                                            sBill_Status = "已作廢";
                                            break;

                                        case "7"://抽單中
                                            sBill_Status = "抽單中";
                                            break;

                                        default:
                                            break;
                                    }
                                    row["AuditVal"] = sBill_Status;
                                }
                                var rowLast1 = dt_new.NewRow();
                                rowLast1["ExchangeRate"] = "總計" + CurrencyName;
                                rowLast1["UntaxAmountMutiRate"] = $"{UntaxAmountMutiRateTotalByLocalCurrency:N2}";
                                rowLast1["BillReimburseAmount"] = $"{BillReimburseAmountTotalByLocalCurrency:N2}";
                                dt_new.Rows.Add(rowLast1);
                                dicAlain = ExcelService.GetExportAlain(oHeader, "RowIndex", "ExchangeRate,Advance,AmountSum,TaxSum,TotalReceivable,AmountTaxSum,UntaxAmountMutiRate,BillReimburseAmount");
                                break;

                            case "BillAndPrice":
                                sFileName = "帳單成本金額";
                                #region 檢查幣別設定
                                var EmptyECurrency = saBillInfo.Where(c => string.IsNullOrWhiteSpace(c.Currency)).Select(c => c.BillNO).Distinct().ToList();
                                var EmptyExchangeRates = saBillInfo.Where(c => string.IsNullOrWhiteSpace(c.ExchangeRate)).Select(c => c.BillNO).Distinct().ToList();

                                if (EmptyECurrency.Any() || EmptyExchangeRates.Any())
                                {
                                    string ErrorMsg = string.Empty;
                                    if (EmptyECurrency.Any())
                                        ErrorMsg += $"帳單:{ string.Join(",", EmptyECurrency)}幣別為空。";
                                    if (EmptyExchangeRates.Any())
                                        ErrorMsg += $"帳單:{ string.Join(",", EmptyExchangeRates)}匯率為空。";
                                    if (i_crm.LANG == @"zh")
                                        ErrorMsg = ErrorMsg = ChineseStringUtility.ToSimplified(ErrorMsg);
                                    return new ErrorResponseMessage(ErrorMsg, i_crm);
                                }
                                #endregion

                                oHeader = new Dictionary<string, string>
                                            {
                                                { "RowIndex", "項次" },
                                                { "ExhibitioName", "展覽/活動名稱" },
                                                { "PayerName", "付款人" },
                                                { "BillNO", "帳單號碼" },
                                                { "AmountSum", $"未稅金額({CurrencyName})" },
                                                { "BillReimburseAmount", $"帳單代墊款({CurrencyName})" },
                                                { "ActualCost", $"實際成本({CurrencyName})" },
                                                { "ActualBillReimburseAmount", $"實際代墊款({CurrencyName})" },
                                                { "Number", "件數" },
                                                { "Weight", "重量" },
                                                { "Volume", "材積(CBM)" },
                                                { "TransportationMode", "運送方式" },
                                                { "IsRetn", "是否退運" }
                                            };
                                dt_new.Columns.Add("BillReimburseAmount");
                                dt_new.Columns.Add("ActualBillReimburseAmount");
                                dt_new.Columns.Add("ActualCost");
                                dt_new.Columns.Add("TransportationMode");
                                Double iTotalAmountSum = 0;
                                Double iTotalActualCost = 0;
                                Double iTotalBillReimburseAmount = 0;
                                Double iTotalActualBillReimburseAmount = 0;

                                foreach (DataRow row in dt_new.Rows)
                                {
                                    var ActualCostFeeItemJson = "";
                                    var sActualCost = "";
                                    var sTransportationMode = "";
                                    var _sBillNO = row["BillNO"].ToString();
                                    var sId = row["ParentId"].ToString();
                                    var sOrgId = row["OrgID"].ToString();
                                    var BillParentId = row[View_OPM_BillInfo.CN_PARENTID].ObjToString();
                                    var sIsRetn = row["IsRetn"].ToString();
                                    var sReFlow = row["ReFlow"].ToString();
                                    var sBillType = row["BillType"].ToString();
                                    var ThisBillCBMUsage = AllCBMUsage.Where(t1 => t1.ParentID == BillParentId && t1.IsReturn == sIsRetn).ToList();

                                    CommonRPT.CalcuCostAndProfit(db, ref ActualCostFeeItemJson, ref sActualCost, ref sTransportationMode, _sBillNO, sId, sIsRetn, sReFlow, sBillType);
                                    if (!string.IsNullOrEmpty(sTransportationMode))
                                    {
                                        var oArgument = db.Queryable<OTB_SYS_Arguments>().Single(it => it.OrgID == sOrgId && it.ArgumentClassID == "Transport" && it.ArgumentID == sTransportationMode);
                                        if (oArgument == null)
                                        {
                                            oArgument = new OTB_SYS_Arguments();
                                        }
                                        row["TransportationMode"] = oArgument.ArgumentValue ?? "";
                                    }
                                    row["IsRetn"] = sIsRetn == "Y" ? "是" : "否";
                                    var ActualCostFeeItemList = CommonRPT.ToFeeItems(ActualCostFeeItemJson);
                                    //實際成本(成本 + 代墊款 ): （整票貨總成本/整票貨CBM）＊單家廠商CBM
                                    var SharedActualCost = CommonRPT.GetShareCost(ActualCostFeeItemList, ThisBillCBMUsage, _sBillNO);
                                    SharedActualCost = Rounding(SharedActualCost, RoundingPoint);
                                    row["ActualCost"] = $"{SharedActualCost:N2}";
                                    iTotalActualCost += SharedActualCost.ObjToMoney();

                                    //帳單代墊款 BillReimburseAmount ==> bill裡面的
                                    var BillFeeItemList = CommonRPT.ToFeeItems(row["FeeItems"].ObjToString());
                                    var BillReimburseAmount = BillFeeItemList.Where(c => BillPrepayFeeList.Contains(c.FinancialCode)).Sum(c => c.TWAmount); //帳單內特定費用代碼資料
                                    BillReimburseAmount = Rounding(BillReimburseAmount, RoundingPoint);
                                    row["BillReimburseAmount"] = BillReimburseAmount;
                                    iTotalBillReimburseAmount += BillReimburseAmount.ObjToMoney();

                                    //實際代墊款(NT$)  ==> 成本裡面的
                                    var ActualBillReimburseAmount = CommonRPT.GetShareCost(ActualCostFeeItemList, ThisBillCBMUsage, _sBillNO, ActualPrepayFeeList);//抓實際成本的資料
                                    ActualBillReimburseAmount = Rounding(ActualBillReimburseAmount, RoundingPoint);
                                    row["ActualBillReimburseAmount"] = ActualBillReimburseAmount;
                                    iTotalActualBillReimburseAmount += ActualBillReimburseAmount.ObjToMoney();

                                    #region ConvertToTWD 僅只有Receivable
                                    double RowExchangeRate = 0;
                                    double AmountSum = 0;
                                    double.TryParse(row["ExchangeRate"].ToString(), out RowExchangeRate);
                                    double.TryParse(row["AmountSum"].ToString(), out AmountSum);

                                    double MultipliedAmountSum = Math.Round(AmountSum * RowExchangeRate, MidpointRounding.AwayFromZero);
                                    row["AmountSum"] = MultipliedAmountSum.ToString();
                                    iTotalAmountSum += MultipliedAmountSum;
                                    #endregion
                                }

                                var rowLast = dt_new.NewRow();
                                rowLast["RowIndex"] = dt_new.Rows.Count + 1;
                                rowLast["ExhibitioName"] = "";
                                rowLast["PayerName"] = "";
                                rowLast["BillNO"] = "總計" + CurrencyName;
                                rowLast["AmountSum"] = $"{iTotalAmountSum:N2}";
                                rowLast["ActualCost"] = $"{iTotalActualCost:N2}";
                                rowLast["BillReimburseAmount"] = $"{iTotalBillReimburseAmount:N2}";
                                rowLast["ActualBillReimburseAmount"] = $"{iTotalActualBillReimburseAmount:N2}";
                                rowLast["Number"] = "";
                                rowLast["Weight"] = "";
                                rowLast["Volume"] = "";
                                rowLast["TransportationMode"] = "";
                                rowLast["IsRetn"] = "";
                                dt_new.Rows.Add(rowLast);

                                dicAlain = ExcelService.GetExportAlain(oHeader, "RowIndex,IsRetn", "ActualCost,AmountSum,ActualBillReimburseAmount,BillReimburseAmount,Weight,Volume");

                                break;

                            default:
                                {
                                    break;
                                }
                        }
                        var bOk = new ExcelService().CreateExcelByTb(dt_new, out string sPath, oHeader, dicAlain, listMerge, sFileName);
                        if (bOk)
                        {
                            var hightLight = Color.FromArgb(226, 240, 217);
                            HightLightColumn(sExcelType, sPath, hightLight);
                        }

                        rm.DATA.Add(BLWording.REL, sPath);
                    }
                    else
                    {
                        rm.DATA.Add(BLWording.REL, pml);
                    }
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(BillStatus_QryService), "", "QueryPage（賬單狀態（分頁查詢））", "", "", "");
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

        #endregion 賬單狀態（分頁查詢）

        public OVW_OPM_BillInfo GetBillInfoByRow(DataRow row, string isRetun)
        {
            var sAuditVal = row["AuditVal"] == null ? "" : row["AuditVal"].ToString();
            var sVolume = row["Volume"] == null ? "0" : row["Volume"].ToString();
            OVW_OPM_BillInfo _BillInfo = new OVW_OPM_BillInfo()
            {
                OrgID = row["OrgID"].ToString(),
                ParentId = row["ParentId"].ToString(),
                IsRetn = isRetun,
                AuditVal = sAuditVal,
                Volume = sVolume
            };
            return _BillInfo;
        }

        public decimal Rounding(decimal value, int digit = 0)
        {
            if (digit <= 0)
                return Math.Round(value, MidpointRounding.AwayFromZero);
            else
                return Math.Round(value, digit, MidpointRounding.AwayFromZero);
        }
        public double Rounding(double value, int digit = 0)
        {
            if (digit <= 0)
                return Math.Round(value, MidpointRounding.AwayFromZero);
            else
                return Math.Round(value, digit, MidpointRounding.AwayFromZero);
        }

        public void HightLightColumn(string Type, string ExcelPath, Color color)
        {
            if (Type == "BillList")
            {
                var cellsApp = new ExcelService(ExcelPath);
                var cells = cellsApp.sheet.Cells;//单元格
                var StartColumnIndex = 11;
                var EndColumnIndex = StartColumnIndex + 3;
                var StartRowIndex = 3;
                var EndRowIndex = cells.Rows.Count;
                for (int row = StartRowIndex; row < EndRowIndex; row++)
                {
                    for (int col = StartColumnIndex; col < EndColumnIndex; col++)
                    {
                        var Style = cells[row, col].GetStyle();
                        Style.ForegroundColor = color;
                        //Style.BackgroundColor = color;
                        cells[row, col].SetStyle(Style);
                    }
                }
                cellsApp.workbook.Save(ExcelPath);
            }

        }

    }
}