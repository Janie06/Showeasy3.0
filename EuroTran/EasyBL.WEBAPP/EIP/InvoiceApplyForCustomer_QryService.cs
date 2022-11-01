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

namespace EasyBL.WEBAPP.EIP
{
    public class InvoiceApplyForCustomer_QryService : ServiceBase
    {
        #region 請款單（廠商）分頁查詢

        /// <summary>
        /// 請款單（廠商）分頁查詢
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

                    var sKeyNote = _fetchString(i_crm, @"KeyNote");
                    var sApplicant = _fetchString(i_crm, @"Applicant");
                    var sImportant = _fetchString(i_crm, @"Important");
                    var sStatus = _fetchString(i_crm, @"Status");
                    var sRoles = _fetchString(i_crm, @"Roles");
                    var sCreateDateStart = _fetchString(i_crm, @"CreateDateStart");
                    var sCreateDateEnd = _fetchString(i_crm, @"CreateDateEnd");
                    var bExcel = _fetchBool(i_crm, @"Excel");

                    var rCreateDateStart = new DateTime();
                    var rCreateDateEnd = new DateTime();
                    if (!string.IsNullOrEmpty(sCreateDateStart))
                    {
                        rCreateDateStart = SqlFunc.ToDate(sCreateDateStart);
                    }
                    if (!string.IsNullOrEmpty(sCreateDateEnd))
                    {
                        rCreateDateEnd = SqlFunc.ToDate(sCreateDateEnd).AddDays(1);
                    }

                    pml.DataList = db.Queryable<OTB_EIP_InvoiceApplyInfo, OTB_SYS_Members, OTB_SYS_Departments, OTB_SYS_Members, OTB_CRM_Customers>
                        ((t1, t2, t3, t4, t5) =>
                        new object[] {
                                JoinType.Inner, t1.OrgID == t2.OrgID && t1.Applicant == t2.MemberID,
                                JoinType.Inner, t2.OrgID == t3.OrgID && t2.DepartmentID == t3.DepartmentID,
                                JoinType.Inner, t1.OrgID == t4.OrgID && t1.Handle_Person == t4.MemberID,
                                JoinType.Left, t1.OrgID == t5.OrgID && t1.Payee == t5.guid
                              }
                        )
                        .Where((t1, t2, t3, t4, t5) => t1.OrgID == i_crm.ORIGID && t1.KeyNote.Contains(sKeyNote) && sImportant.Contains(t1.Important) && t1.PayeeType == "C")
                        .WhereIF(!string.IsNullOrEmpty(sApplicant), (t1, t2, t3, t4, t5) => t1.Applicant == sApplicant)
                        .WhereIF(!string.IsNullOrEmpty(sStatus), (t1, t2, t3, t4, t5) => sStatus.Contains(t1.Status))
                        .WhereIF(!(sRoles.Contains("Admin") || sRoles.Contains("EipManager") || sRoles.Contains("EipView")), (t1, t2, t3, t4, t5) => (t1.Applicant == i_crm.USERID || t1.Handle_Person == i_crm.USERID || t1.CheckFlows.Contains(i_crm.USERID)))
                        .WhereIF(!string.IsNullOrEmpty(sCreateDateStart), (t1, t2, t3, t4, t5) => t1.CreateDate >= rCreateDateStart.Date)
                        .WhereIF(!string.IsNullOrEmpty(sCreateDateEnd), (t1, t2, t3, t4, t5) => t1.CreateDate <= rCreateDateEnd.Date)
                        .Select((t1, t2, t3, t4, t5) => new View_EIP_InvoiceApplyInfo
                        {
                            Guid = SqlFunc.GetSelfAndAutoFill(t1.Guid),
                            ApplicantName = t2.MemberName,
                            Handle_PersonName = t4.MemberName,
                            CustomerNO = t5.CustomerNO,
                            DeptName = t3.DepartmentName
                        })
                        .MergeTable()
                        .OrderBy(sSortField, sSortOrder)
                        .ToPageList(pml.PageIndex, bExcel ? 100000 : pml.PageSize, ref iPageCount);
                    pml.Total = iPageCount;

                    rm = new SuccessResponseMessage(null, i_crm);
                    if (bExcel)
                    {
                        const string sFileName = "請款單（廠商）費用明細";
                        var oHeader = new Dictionary<string, string>
                        {
                            { "RowIndex", "項次" },
                            { "PayeeCode", "受款人代號" },
                            { "Payee", "受款人" },
                            { "FeeItemName", "費用名稱" },
                            { "BillNO", "帳單號碼" },
                            { "PrjCode", "專案代號" },
                            { "Currency", "幣別" },
                            { "Amount", "金額" },
                            { "PaymentWay", "付款方式" },
                            { "PaymentTime", "支付時間" }
                        };
                        var dt_new = new DataTable();
                        dt_new.Columns.Add("RowIndex");
                        dt_new.Columns.Add("PayeeCode");
                        dt_new.Columns.Add("Payee");
                        dt_new.Columns.Add("FeeItemName");
                        dt_new.Columns.Add("BillNO");
                        dt_new.Columns.Add("PrjCode");
                        dt_new.Columns.Add("Currency");
                        dt_new.Columns.Add("Amount");
                        dt_new.Columns.Add("PaymentWay");
                        dt_new.Columns.Add("PaymentTime");

                        var listMerge = new List<Dictionary<string, int>>();
                        var saInvoiceApplyInfo = pml.DataList as List<View_EIP_InvoiceApplyInfo>;
                        foreach (var item in saInvoiceApplyInfo)
                        {
                            var jaPayeeInfo = (JArray)JsonConvert.DeserializeObject(item.PayeeInfo);
                            var sPaymentWay = item.PaymentWay;
                            foreach (JObject payeeinfo in jaPayeeInfo)
                            {
                                var row_new = dt_new.NewRow();
                                var sCurrency = payeeinfo["Currency"].ToString();
                                var iAmount = Convert.ToDouble(payeeinfo["Amount"].ToString());
                                row_new["RowIndex"] = item.RowIndex;
                                row_new["PayeeCode"] = item.CustomerNO;
                                row_new["Payee"] = item.PayeeName;
                                row_new["FeeItemName"] = payeeinfo["FeeItemName"];
                                row_new["BillNO"] = payeeinfo["BillNO"];
                                row_new["PrjCode"] = (payeeinfo["PrjCode"] ?? "") + " " + (payeeinfo["PrjName"] ?? "");
                                row_new["Currency"] = sCurrency;
                                row_new["Amount"] = sCurrency == "NTD" ? $"{iAmount:N0}" : $"{iAmount:N2}";
                                row_new["PaymentWay"] = sPaymentWay == "A" ? "現金" : sPaymentWay == "B" ? "預支現金" : sPaymentWay == "C" ? "支票" : sPaymentWay == "D" ? "轉賬/匯款" : "依公司規定";
                                row_new["PaymentTime"] = item.PaymentTime == null ? "" : Convert.ToDateTime(item.PaymentTime).ToString("yyyy/MM/dd");
                                dt_new.Rows.Add(row_new);
                            }
                        }
                        var dicAlain = ExcelService.GetExportAlain(oHeader, new string[] { "Currency", "PayeeCode", "BillNO" }, "Amount");

                        var bOk = new ExcelService().CreateExcelByTb(dt_new, out string sPath, oHeader, dicAlain, listMerge, sFileName);
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
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBAPP.EIP.InvoiceApplyForCustomer_QryService", "", "QueryPage（請款單（廠商）分頁查詢）", "", "", "");
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

        #endregion 請款單（廠商）分頁查詢

        #region 請款單（廠商）單筆查詢

        /// <summary>
        /// 請款單（廠商）單筆查詢
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage QueryOne(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sId = _fetchString(i_crm, @"Guid");

                    var oEntity = db.Queryable<OTB_EIP_InvoiceApplyInfo, OTB_SYS_Members, OTB_SYS_Departments, OTB_SYS_Members, OTB_CRM_Customers>
                        ((t1, t2, t3, t4, t5) =>
                        new object[] {
                                JoinType.Inner, t1.OrgID == t2.OrgID && t1.Applicant == t2.MemberID,
                                JoinType.Inner, t2.OrgID == t3.OrgID && t2.DepartmentID == t3.DepartmentID,
                                JoinType.Inner, t1.OrgID == t4.OrgID && t1.Handle_Person == t4.MemberID,
                                JoinType.Left, t1.OrgID == t5.OrgID && t1.Payee == t5.guid
                              }
                        )
                        .Where((t1, t2, t3, t4, t5) => t1.OrgID == i_crm.ORIGID && t1.Guid == sId)
                        .Select((t1, t2, t3, t4, t5) => new View_EIP_InvoiceApplyInfo
                        {
                            Guid = SqlFunc.GetSelfAndAutoFill(t1.Guid),
                            ApplicantName = t2.MemberName,
                            Handle_PersonName = t4.MemberName,
                            CustomerNO = t5.CustomerNO,
                            DeptName = t3.DepartmentName
                        }).Single();
                    if (!string.IsNullOrEmpty(oEntity.RelationId))
                    {
                        var oRelation = db.Queryable<OTB_EIP_InvoiceApplyInfo>().Single(x => x.OrgID == i_crm.ORIGID && x.Guid == oEntity.RelationId);
                        if (oRelation != null)
                        {
                            oEntity.ExFeild1 = oRelation.KeyNote;
                        }
                    }

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, oEntity);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBAPP.EIP.InvoiceApplyForCustomer_QryService", "", "QueryOne（請款單（廠商）單筆查詢）", "", "", "");
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

        #endregion 請款單（廠商）單筆查詢
    }
}