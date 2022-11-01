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
    public class InvoiceApplyForPersonal_QryService : ServiceBase
    {
        #region 請款單（個人）分頁查詢

        /// <summary>
        /// 請款單（個人）分頁查詢
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
                    var sPaymentWay = _fetchString(i_crm, @"PaymentWay");
                    var sStatus = _fetchString(i_crm, @"Status");
                    var sRoles = _fetchString(i_crm, @"Roles");
                    var sPaymentTimeStart = _fetchString(i_crm, @"PaymentTimeStart");
                    var sPaymentTimeEnd = _fetchString(i_crm, @"PaymentTimeEnd");
                    var bExcel = _fetchBool(i_crm, @"Excel");

                    var rPaymentTimeStart = new DateTime();
                    var rPaymentTimeEnd = new DateTime();
                    if (!string.IsNullOrEmpty(sPaymentTimeStart))
                    {
                        rPaymentTimeStart = SqlFunc.ToDate(sPaymentTimeStart);
                    }
                    if (!string.IsNullOrEmpty(sPaymentTimeEnd))
                    {
                        rPaymentTimeEnd = SqlFunc.ToDate(sPaymentTimeEnd).AddDays(1);
                    }

                    pml.DataList = db.Queryable<OTB_EIP_InvoiceApplyInfo, OTB_SYS_Members, OTB_SYS_Departments, OTB_SYS_Members, OTB_SYS_Members>
                        ((t1, t2, t3, t4, t5) =>
                        new object[] {
                                JoinType.Inner, t1.OrgID == t2.OrgID && t1.Applicant == t2.MemberID,
                                JoinType.Inner, t2.OrgID == t3.OrgID && t2.DepartmentID == t3.DepartmentID,
                                JoinType.Inner, t1.OrgID == t4.OrgID && t1.Handle_Person == t4.MemberID,
                                JoinType.Left, t1.OrgID == t5.OrgID && t1.Payee == t5.MemberID
                              }
                        )
                        .Where((t1, t2, t3, t4, t5) => t1.OrgID == i_crm.ORIGID && t1.KeyNote.Contains(sKeyNote) && sImportant.Contains(t1.Important) && sPaymentWay.Contains(t1.PaymentWay) && t1.PayeeType == "P")
                        .WhereIF(!string.IsNullOrEmpty(sApplicant), (t1, t2, t3, t4, t5) => t1.Applicant == sApplicant)
                        .WhereIF(!string.IsNullOrEmpty(sStatus), (t1, t2, t3, t4, t5) => sStatus.Contains(t1.Status))
                        .WhereIF(!(sRoles.Contains("Admin") || sRoles.Contains("EipManager") || sRoles.Contains("EipView")), (t1, t2, t3, t4, t5) => (t1.Applicant == i_crm.USERID || t1.Handle_Person == i_crm.USERID || t1.CheckFlows.Contains(i_crm.USERID)))
                        .WhereIF(!string.IsNullOrEmpty(sPaymentTimeStart), (t1, t2, t3, t4, t5) => t1.PaymentTime >= rPaymentTimeStart.Date)
                        .WhereIF(!string.IsNullOrEmpty(sPaymentTimeEnd), (t1, t2, t3, t4, t5) => t1.PaymentTime <= rPaymentTimeEnd.Date)
                        .Select((t1, t2, t3, t4, t5) => new View_EIP_InvoiceApplyInfo
                        {
                            Guid = SqlFunc.GetSelfAndAutoFill(t1.Guid),
                            ApplicantName = t2.MemberName,
                            Handle_PersonName = t4.MemberName,
                            PayeeCode = t5.WenZhongAcount,
                            DeptName = t3.DepartmentName
                        })
                        .MergeTable()
                        .OrderBy(sSortField, sSortOrder)
                        .ToPageList(pml.PageIndex, bExcel ? 100000 : pml.PageSize, ref iPageCount);
                    pml.Total = iPageCount;

                    rm = new SuccessResponseMessage(null, i_crm);
                    if (bExcel)
                    {
                        const string sFileName = "各項獎金";
                        var oHeader = new Dictionary<string, string>
                        {
                            { "RowIndex", "項次" },
                            { "PayeeCode", "員工代號" },
                            { "PayeeName", "員工姓名" },
                            { "EffectTime", "生效時間" },
                            { "Currency", "幣別" },
                            { "ExchangeRate", "匯率" },
                            { "FeeItemName", "事由" },
                            { "TicketCost", "機票費用" },
                            { "TravelCost", "差旅費" },
                            { "ActingMatCost", "代墊款項" },
                            { "OtherCost", "代墊款項" }
                        };
                        var dt_new = new DataTable();
                        dt_new.Columns.Add("RowIndex");
                        dt_new.Columns.Add("PayeeCode");
                        dt_new.Columns.Add("PayeeName");
                        dt_new.Columns.Add("EffectTime");
                        dt_new.Columns.Add("Currency");
                        dt_new.Columns.Add("ExchangeRate");
                        dt_new.Columns.Add("FeeItemName");
                        dt_new.Columns.Add("TicketCost");
                        dt_new.Columns.Add("TravelCost");
                        dt_new.Columns.Add("ActingMatCost");
                        dt_new.Columns.Add("OtherCost");

                        var listMerge = new List<Dictionary<string, int>>();
                        var saInvoiceApplyInfo = pml.DataList as List<View_EIP_InvoiceApplyInfo>;
                        foreach (var item in saInvoiceApplyInfo)
                        {
                            var jaPayeeInfo = (JArray)JsonConvert.DeserializeObject(item.PayeeInfo);
                            foreach (JObject payeeinfo in jaPayeeInfo)
                            {
                                var row_new = dt_new.NewRow();
                                var sCurrency = payeeinfo["Currency"].ToString();
                                var iAmount = Convert.ToDouble(payeeinfo["Amount"].ToString());
                                row_new["RowIndex"] = item.RowIndex;
                                row_new["PayeeCode"] = item.PayeeCode;
                                row_new["PayeeName"] = item.PayeeName;
                                row_new["EffectTime"] = (item.EffectTime == null) ? "" : Convert.ToDateTime(item.EffectTime).ToString("yyyy/MM/dd");
                                row_new["Currency"] = sCurrency;
                                row_new["ExchangeRate"] = 1;
                                row_new["FeeItemName"] = payeeinfo["FeeItemName"];
                                if (payeeinfo["FeeType"].ToString() == "001")
                                {
                                    row_new["TicketCost"] = sCurrency == "NTD" ? $"{iAmount:N0}" : $"{iAmount:N2}";
                                    row_new["TravelCost"] = "";
                                    row_new["ActingMatCost"] = "";
                                    row_new["OtherCost"] = "";
                                }
                                else if (payeeinfo["FeeType"].ToString() == "002")
                                {
                                    row_new["TicketCost"] = "";
                                    row_new["TravelCost"] = sCurrency == "NTD" ? $"{iAmount:N0}" : $"{iAmount:N2}";
                                    row_new["ActingMatCost"] = "";
                                    row_new["OtherCost"] = "";
                                }
                                else if (payeeinfo["FeeType"].ToString() == "003")
                                {
                                    row_new["TicketCost"] = "";
                                    row_new["TravelCost"] = "";
                                    row_new["ActingMatCost"] = sCurrency == "NTD" ? $"{iAmount:N0}" : $"{iAmount:N2}";
                                    row_new["OtherCost"] = "";
                                }
                                else if (payeeinfo["FeeType"].ToString() == "004")
                                {
                                    row_new["TicketCost"] = "";
                                    row_new["TravelCost"] = "";
                                    row_new["ActingMatCost"] = "";
                                    row_new["OtherCost"] = sCurrency == "NTD" ? $"{iAmount:N0}" : $"{iAmount:N2}";
                                }
                                dt_new.Rows.Add(row_new);
                            }
                        }
                        var dicAlain = ExcelService.GetExportAlain(oHeader, "Currency,ExchangeRate,PayeeCode", "TicketCost,TravelCost,ActingMatCost,OtherCost");

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
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBAPP.EIP.InvoiceApplyForPersonal_QryService", "", "QueryPage（請款單（個人）分頁查詢）", "", "", "");
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

        #endregion 請款單（個人）分頁查詢

        #region 請款單（個人）單筆查詢

        /// <summary>
        /// 請款單（個人）單筆查詢
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

                    var oEntity = db.Queryable<OTB_EIP_InvoiceApplyInfo, OTB_SYS_Members, OTB_SYS_Departments, OTB_SYS_Members, OTB_SYS_Members>
                        ((t1, t2, t3, t4, t5) =>
                        new object[] {
                                JoinType.Inner, t1.OrgID == t2.OrgID && t1.Applicant == t2.MemberID,
                                JoinType.Inner, t2.OrgID == t3.OrgID && t2.DepartmentID == t3.DepartmentID,
                                JoinType.Inner, t1.OrgID == t4.OrgID && t1.Handle_Person == t4.MemberID,
                                JoinType.Left, t1.OrgID == t5.OrgID && t1.Payee == t5.MemberID
                              }
                        )
                        .Where((t1, t2, t3, t4, t5) => t1.OrgID == i_crm.ORIGID && t1.Guid == sId)
                        .Select((t1, t2, t3, t4, t5) => new View_EIP_InvoiceApplyInfo
                        {
                            Guid = SqlFunc.GetSelfAndAutoFill(t1.Guid),
                            ApplicantName = t2.MemberName,
                            Handle_PersonName = t4.MemberName,
                            PayeeCode = t5.WenZhongAcount,
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

                    //重新抓取CheckFlow
                    var oFlow = db.Queryable<OTB_EIP_CheckFlow>().Single(x => x.OrgID == i_crm.ORIGID && x.Guid == oEntity.FlowId);

                    if (oFlow != null)
                    {
                        oEntity.Flows_Lock = oFlow.Flows_Lock ?? "N";
                        oEntity.Handle_Lock = oFlow.Handle_Lock ?? "N";
                        oEntity.CheckOrder = oFlow.Flows;
                        oEntity.Handle_DeptID = oFlow.Handle_DeptID;
                        oEntity.Handle_Person = oFlow.Handle_Person;
                    }
                    else {
                        sMsg = "抓取流程資料異常";
                    }

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, oEntity);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBAPP.EIP.InvoiceApplyForPersonal_QryService", "", "QueryOne（請款單（個人）單筆查詢）", "", "", "");
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

        #endregion 請款單（個人）單筆查詢
    }
}