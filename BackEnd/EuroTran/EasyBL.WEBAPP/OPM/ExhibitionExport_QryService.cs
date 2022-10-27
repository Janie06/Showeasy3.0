using EasyBL.WebApi.Message;
using Entity.Sugar;
using Entity.ViewModels;
using Newtonsoft.Json.Linq;
using SqlSugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using EasyBL;
using Newtonsoft.Json;
using System.Linq;

namespace EasyBL.WEBAPP.OPM
{
    public class ExhibitionExport_QryService : ServiceBase
    {
        #region 出口分頁查詢

        /// <summary>
        /// 出口分頁查詢
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

                    var sRefNumber = _fetchString(i_crm, @"RefNumber");
                    var sExportBillName = _fetchString(i_crm, @"ExportBillName");
                    var sBillNO = _fetchString(i_crm, @"BillNO");
                    var sExhibitionDateStart = _fetchString(i_crm, @"ExhibitionDateStart");
                    var sExhibitionDateEnd = _fetchString(i_crm, @"ExhibitionDateEnd");
                    var sAgent = _fetchString(i_crm, @"Agent");
                    var sResponsiblePerson = _fetchString(i_crm, @"ResponsiblePerson");
                    var sBillLadNO = _fetchString(i_crm, @"BillLadNO");
                    var sJobTracking = _fetchString(i_crm, @"JobTracking");
                    var sIsIncludeVoid = _fetchString(i_crm, @"IsIncludeVoid");
                    var sSupplier = _fetchString(i_crm, @"Supplier");
                    var sREF = _fetchString(i_crm, @"REF");
                    var sDepartmentID = _fetchString(i_crm, @"DepartmentID");
                    var sFlow_Status = _fetchString(i_crm, @"Flow_Status");
                    var bExcel = _fetchBool(i_crm, @"Excel");
                    var sExcelType = _fetchString(i_crm, @"ExcelType");

                    string[] saJobTracking = null;
                    string[] saIsIncludeVoid = null;

                    var rExhibitionDateStart = new DateTime();
                    var rExhibitionDateEnd = new DateTime();
                    if (!string.IsNullOrEmpty(sExhibitionDateStart))
                    {
                        rExhibitionDateStart = SqlFunc.ToDate(sExhibitionDateStart);
                    }
                    if (!string.IsNullOrEmpty(sExhibitionDateEnd))
                    {
                        rExhibitionDateEnd = SqlFunc.ToDate(sExhibitionDateEnd).AddDays(1);
                    }
                    if (!string.IsNullOrEmpty(sJobTracking))
                    {
                        saJobTracking = sJobTracking.Split(',');
                    }
                    if (!string.IsNullOrEmpty(sIsIncludeVoid))
                    {
                        saIsIncludeVoid = sIsIncludeVoid.Split(',');
                    }

                    var spOrgID = new SugarParameter("@OrgID", i_crm.ORIGID);
                    var spUserID = new SugarParameter("@UserID", i_crm.USERID);
                    var spDepartmentID = new SugarParameter("@DepartmentID", sDepartmentID);

                    var saRoles = db.Queryable<OTB_SYS_MembersToRule>().Where(x => x.OrgID == i_crm.ORIGID && x.MemberID == i_crm.USERID).Select(x => x.RuleID).ToList().ToArray();
                    var saChildDeptIds = db.Ado.SqlQuery<string>("select DepartmentId from [dbo].[OFN_SYS_GetChilDepartmentIdByDepartmentId](@OrgID,@DepartmentID)", spOrgID, spDepartmentID).ToArray();
                    var saDeptIdsByUser = db.Ado.SqlQuery<string>("SELECT DepartmentId FROM [dbo].[OFN_SYS_GetChilDepartmentIdByUserID](@OrgID,@UserID)", spOrgID, spUserID).ToArray();
                    var saChildUserIds = db.Ado.SqlQuery<string>("SELECT MemberID FROM [dbo].[OFN_SYS_GetMemberIDDownByChief](@OrgID,@UserID)", spOrgID, spUserID).ToArray();

                    pml.DataList = db.Queryable<OTB_OPM_ExportExhibition, OTB_SYS_Members, OTB_OPM_Exhibition, OTB_CRM_Customers, OTB_CRM_Customers>
                        ((t1, t2, t3, t5, t6) =>
                        new object[] {
                                JoinType.Inner, t1.OrgID == t2.OrgID && t1.ResponsiblePerson == t2.MemberID,
                                JoinType.Left, t1.OrgID == t3.OrgID && t1.ExhibitionNO == t3.SN.ToString(),
                                JoinType.Left, t1.OrgID == t5.OrgID && t1.Agent == t5.guid,
                                JoinType.Left, t1.OrgID == t6.OrgID && t1.Organizer == t6.guid
                              }
                        )
                        .Where((t1) => t1.OrgID == i_crm.ORIGID)
                        .WhereIF(!string.IsNullOrEmpty(sRefNumber), (t1) => (t1.RefNumber + t1.Exhibitors).Contains(sRefNumber))
                        .WhereIF(!string.IsNullOrEmpty(sExportBillName), (t1, t2, t3) => (t3.Exhibitioname_TW + t3.Exhibitioname_EN + t3.ExhibitioShotName_TW).Contains(sExportBillName))
                        .WhereIF(!string.IsNullOrEmpty(sResponsiblePerson), (t1) => t1.ResponsiblePerson == sResponsiblePerson)
                        .WhereIF(!string.IsNullOrEmpty(sAgent), (t1, t2, t3, t4, t5) => (t5.CustomerCName + t5.CustomerEName).Contains(sAgent))
                        .WhereIF(!string.IsNullOrEmpty(sSupplier), (t1, t2, t3, t5, t6) => (t6.CustomerCName + t6.CustomerEName).Contains(sSupplier))
                        .WhereIF(!string.IsNullOrEmpty(sBillLadNO), (t1) => (t1.BillLadNO + t1.Exhibitors).Contains(sBillLadNO))
                        .WhereIF(!string.IsNullOrEmpty(sFlow_Status), (t1) => t1.Flow_Status == sFlow_Status)
                        .WhereIF(!string.IsNullOrEmpty(sBillNO), (t1) => (t1.Bills + t1.ReturnBills).Contains("\"BillNO\":%\"" + sBillNO))
                        .WhereIF(!string.IsNullOrEmpty(sREF), (t1) => t1.REF.Contains(sREF))
                        .WhereIF(!string.IsNullOrEmpty(sDepartmentID), (t1) => SqlFunc.ContainsArray(saChildDeptIds, t1.DepartmentID))
                        .WhereIF(!string.IsNullOrEmpty(sJobTracking), (t1) => SqlFunc.ContainsArray(saJobTracking, t1.Release))
                        .WhereIF(!string.IsNullOrEmpty(sIsIncludeVoid), (t1) => SqlFunc.ContainsArray(saIsIncludeVoid, t1.IsVoid))
                        .WhereIF(!string.IsNullOrEmpty(sExhibitionDateStart), (t1) => t1.ExhibitionDateStart >= rExhibitionDateStart.Date)
                        .WhereIF(!string.IsNullOrEmpty(sExhibitionDateEnd), (t1) => t1.ExhibitionDateEnd <= rExhibitionDateEnd.Date)
                        .Where((t1) => t1.CreateUser == i_crm.USERID || t1.ResponsiblePerson == i_crm.USERID || SqlFunc.ContainsArray(saDeptIdsByUser, t1.DepartmentID) ||
                       SqlFunc.Subqueryable<OTB_SYS_Members>().Where(c => c.MemberID == t1.CreateUser && c.OrgID == t1.OrgID).Select(c => c.ImmediateSupervisor) == i_crm.USERID ||
                       SqlFunc.Subqueryable<OTB_SYS_Members>().Where(p => p.MemberID == t1.ResponsiblePerson && p.OrgID == t1.OrgID).Select(c => c.ImmediateSupervisor) == i_crm.USERID || SqlFunc.ContainsArray(saChildUserIds, t1.CreateUser) || SqlFunc.ContainsArray(saChildUserIds, t1.ResponsiblePerson) || SqlFunc.ContainsArray(saRoles, "Account") || SqlFunc.ContainsArray(saRoles, "CDD") || SqlFunc.ContainsArray(saRoles, "Admin") || SqlFunc.ContainsArray(saRoles, "Manager"))
                        .Select((t1, t2, t3, t5) => new View_OPM_ExportExhibition
                        {
                            OrgID = t1.OrgID,
                            ExportBillNO = t1.ExportBillNO,
                            RefNumber = t1.RefNumber,
                            ExportBillName = t3.Exhibitioname_TW,
                            ExportBillEName = t3.Exhibitioname_EN,
                            ExhibitionDateStart = t1.ExhibitionDateStart,
                            ExhibitionDateEnd = t1.ExhibitionDateEnd,
                            BillLadNO = t1.BillLadNO,
                            IsVoid = t1.IsVoid,
                            Flow_Status = t1.Flow_Status,
                            Bills = t1.Bills,
                            ReturnBills = t1.ReturnBills,
                            REF = t1.REF,
                            IsAlert = SqlFunc.MappingColumn(t1.ImportBillNO, "[dbo].[OFN_OPM_CheckDate](t1.Release,t1.ExhibitionDateStart,5)"),
                            AgentName = SqlFunc.IIF(SqlFunc.HasValue(t5.CustomerCName), t5.CustomerCName, t5.CustomerEName),
                            ResponsiblePersonName = t2.MemberName,
                        })
                        .MergeTable()
                        .OrderByIF(string.IsNullOrEmpty(sSortField), "IsAlert desc,ExhibitionDateStart desc,ExportBillName,RefNumber desc")
                        .OrderByIF(!string.IsNullOrEmpty(sSortField), sSortField + " " + sSortOrder)
                        .ToPageList(pml.PageIndex, bExcel ? 100000 : pml.PageSize, ref iPageCount);
                    pml.Total = iPageCount;

                    rm = new SuccessResponseMessage(null, i_crm);
                    if (bExcel)
                    {
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
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBAPP.OPM.ImportExhibition_QryService", "", "QueryPage（出口分頁查詢）", "", "", "");
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

        #endregion 出口分頁查詢

        #region 出口分頁查詢

        /// <summary>
        /// 出口分頁查詢
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetExcel(RequestMessage i_crm)
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

                    var sRefNumber = _fetchString(i_crm, @"RefNumber");
                    var sExportBillName = _fetchString(i_crm, @"ExportBillName");
                    var sBillNO = _fetchString(i_crm, @"BillNO");
                    var sExhibitionDateStart = _fetchString(i_crm, @"ExhibitionDateStart");
                    var sExhibitionDateEnd = _fetchString(i_crm, @"ExhibitionDateEnd");
                    var sAgent = _fetchString(i_crm, @"Agent");
                    var sResponsiblePerson = _fetchString(i_crm, @"ResponsiblePerson");
                    var sBillLadNO = _fetchString(i_crm, @"BillLadNO");
                    var sJobTracking = _fetchString(i_crm, @"JobTracking");
                    var sIsIncludeVoid = _fetchString(i_crm, @"IsIncludeVoid");
                    var sSupplier = _fetchString(i_crm, @"Supplier");
                    var sREF = _fetchString(i_crm, @"REF");
                    var sDepartmentID = _fetchString(i_crm, @"DepartmentID");
                    var sFlow_Status = _fetchString(i_crm, @"Flow_Status");
                    var bExcel = _fetchBool(i_crm, @"Excel");
                    var sExcelType = _fetchString(i_crm, @"ExcelType");

                    string[] saJobTracking = null;
                    string[] saIsIncludeVoid = null;

                    var rExhibitionDateStart = new DateTime();
                    var rExhibitionDateEnd = new DateTime();
                    if (!string.IsNullOrEmpty(sExhibitionDateStart))
                    {
                        rExhibitionDateStart = SqlFunc.ToDate(sExhibitionDateStart);
                    }
                    if (!string.IsNullOrEmpty(sExhibitionDateEnd))
                    {
                        rExhibitionDateEnd = SqlFunc.ToDate(sExhibitionDateEnd).AddDays(1);
                    }
                    if (!string.IsNullOrEmpty(sJobTracking))
                    {
                        saJobTracking = sJobTracking.Split(',');
                    }
                    if (!string.IsNullOrEmpty(sIsIncludeVoid))
                    {
                        saIsIncludeVoid = sIsIncludeVoid.Split(',');
                    }

                    var spOrgID = new SugarParameter("@OrgID", i_crm.ORIGID);
                    var spUserID = new SugarParameter("@UserID", i_crm.USERID);
                    var spDepartmentID = new SugarParameter("@DepartmentID", sDepartmentID);

                    var saRoles = db.Queryable<OTB_SYS_MembersToRule>().Where(x => x.OrgID == i_crm.ORIGID && x.MemberID == i_crm.USERID).Select(x => x.RuleID).ToList().ToArray();
                    var saChildDeptIds = db.Ado.SqlQuery<string>("select DepartmentId from [dbo].[OFN_SYS_GetChilDepartmentIdByDepartmentId](@OrgID,@DepartmentID)", spOrgID, spDepartmentID).ToArray();
                    var saDeptIdsByUser = db.Ado.SqlQuery<string>("SELECT DepartmentId FROM [dbo].[OFN_SYS_GetChilDepartmentIdByUserID](@OrgID,@UserID)", spOrgID, spUserID).ToArray();
                    var saChildUserIds = db.Ado.SqlQuery<string>("SELECT MemberID FROM [dbo].[OFN_SYS_GetMemberIDDownByChief](@OrgID,@UserID)", spOrgID, spUserID).ToArray();

                    pml.DataList = db.Queryable<OTB_OPM_ExportExhibition, OTB_SYS_Members, OTB_OPM_Exhibition, OTB_SYS_Arguments, OTB_CRM_Customers, OTB_CRM_Customers>
                        ((t1, t2, t3, t4, t5, t6) =>
                        new object[] {
                                JoinType.Inner, t1.OrgID == t2.OrgID && t1.ResponsiblePerson == t2.MemberID,
                                JoinType.Left, t1.OrgID == t3.OrgID && t1.ExhibitionNO == t3.SN.ToString(),
                                JoinType.Left, t1.OrgID == t4.OrgID && t1.TransportationMode == t4.ArgumentID && t4.ArgumentClassID == "Transport",
                                JoinType.Left, t1.OrgID == t5.OrgID && t1.Agent == t5.guid,
                                JoinType.Left, t1.OrgID == t6.OrgID && t1.Organizer == t6.guid
                              }
                        )
                        .Where((t1) => t1.OrgID == i_crm.ORIGID)
                        .WhereIF(!string.IsNullOrEmpty(sRefNumber), (t1) => (t1.RefNumber + t1.Exhibitors).Contains(sRefNumber))
                        .WhereIF(!string.IsNullOrEmpty(sExportBillName), (t1, t2, t3) => (t3.Exhibitioname_TW + t3.Exhibitioname_EN + t3.ExhibitioShotName_TW).Contains(sExportBillName))
                        .WhereIF(!string.IsNullOrEmpty(sResponsiblePerson), (t1) => t1.ResponsiblePerson == sResponsiblePerson)
                        .WhereIF(!string.IsNullOrEmpty(sAgent), (t1, t2, t3, t4, t5) => (t5.CustomerCName + t5.CustomerEName).Contains(sAgent))
                        .WhereIF(!string.IsNullOrEmpty(sSupplier), (t1, t2, t3, t4, t5, t6) => (t6.CustomerCName + t6.CustomerEName).Contains(sSupplier))
                        .WhereIF(!string.IsNullOrEmpty(sBillLadNO), (t1) => (t1.BillLadNO + t1.Exhibitors).Contains(sBillLadNO))
                        .WhereIF(!string.IsNullOrEmpty(sFlow_Status), (t1) => t1.Flow_Status == sFlow_Status)
                        .WhereIF(!string.IsNullOrEmpty(sBillNO), (t1) => (t1.Bills + t1.ReturnBills).Contains(sBillNO))
                        .WhereIF(!string.IsNullOrEmpty(sREF), (t1) => t1.REF.Contains(sREF))
                        .WhereIF(!string.IsNullOrEmpty(sDepartmentID), (t1) => SqlFunc.ContainsArray(saChildDeptIds, t1.DepartmentID))
                        .WhereIF(!string.IsNullOrEmpty(sJobTracking), (t1) => SqlFunc.ContainsArray(saJobTracking, t1.Release))
                        .WhereIF(!string.IsNullOrEmpty(sIsIncludeVoid), (t1) => SqlFunc.ContainsArray(saIsIncludeVoid, t1.IsVoid))
                        .WhereIF(!string.IsNullOrEmpty(sExhibitionDateStart), (t1) => t1.ExhibitionDateStart >= rExhibitionDateStart.Date)
                        .WhereIF(!string.IsNullOrEmpty(sExhibitionDateEnd), (t1) => t1.ExhibitionDateEnd <= rExhibitionDateEnd.Date)
                        .Where((t1) => t1.CreateUser == i_crm.USERID || t1.ResponsiblePerson == i_crm.USERID || SqlFunc.ContainsArray(saDeptIdsByUser, t1.DepartmentID) ||
                       SqlFunc.Subqueryable<OTB_SYS_Members>().Where(c => c.MemberID == t1.CreateUser && c.OrgID == t1.OrgID).Select(c => c.ImmediateSupervisor) == i_crm.USERID ||
                       SqlFunc.Subqueryable<OTB_SYS_Members>().Where(p => p.MemberID == t1.ResponsiblePerson && p.OrgID == t1.OrgID).Select(c => c.ImmediateSupervisor) == i_crm.USERID || SqlFunc.ContainsArray(saChildUserIds, t1.CreateUser) || SqlFunc.ContainsArray(saChildUserIds, t1.ResponsiblePerson) || SqlFunc.ContainsArray(saRoles, "Account") || SqlFunc.ContainsArray(saRoles, "CDD") || SqlFunc.ContainsArray(saRoles, "Admin") || SqlFunc.ContainsArray(saRoles, "Manager"))
                        .Select((t1, t2, t3, t4, t5, t6) => new View_OPM_ExportExhibition
                        {
                            OrgID = t1.OrgID,
                            ExportBillNO = t1.ExportBillNO,
                            ExhibitionNO = t1.ExhibitionNO,
                            RefNumber = t1.RefNumber,
                            ExportDeclarationNO = t1.ExportDeclarationNO,
                            ImportBillNO = t1.ImportBillNO,
                            ExportBillName = t3.Exhibitioname_TW,
                            ExportBillEName = t3.Exhibitioname_EN,
                            ExhibitioShotName_TW = t3.ExhibitioShotName_TW,
                            ExhibitionDateStart = t1.ExhibitionDateStart,
                            ExhibitionDateEnd = t1.ExhibitionDateEnd,
                            CarriersNumber = t1.CarriersNumber,
                            ResponsiblePerson = t1.ResponsiblePerson,
                            BillLadNO = t1.BillLadNO,
                            BillLadNOSub = t1.BillLadNOSub,
                            ContainerNumber = t1.ContainerNumber,
                            ShipmentPort = t1.ShipmentPort,
                            Destination = t1.Destination,
                            ShippingCompany = t1.ShippingCompany,
                            Suppliers = t1.Suppliers,
                            Release = t1.Release,
                            Exhibitors = t1.Exhibitors,
                            IsVoid = t1.IsVoid,
                            Flow_Status = t1.Flow_Status,
                            Bills = t1.Bills,
                            ReturnBills = t1.ReturnBills,
                            REF = t1.REF,
                            DepartmentID = t1.DepartmentID,
                            Memo = t1.Memo,
                            IsAlert = SqlFunc.MappingColumn(t1.ImportBillNO, "[dbo].[OFN_OPM_CheckDate](t1.Release,t1.ExhibitionDateStart,5)"),
                            ExhibitionDate = SqlFunc.MappingColumn(t1.ImportBillNO, "CONVERT(varchar(100),t1.ExhibitionDateStart, 111)+'~'+CONVERT(VARCHAR(100),t1.ExhibitionDateEnd, 111)"),
                            OrganizerName = SqlFunc.IIF(SqlFunc.HasValue(t6.CustomerCName), t6.CustomerCName, t6.CustomerEName),
                            AgentName = SqlFunc.IIF(SqlFunc.HasValue(t5.CustomerCName), t5.CustomerCName, t5.CustomerEName),
                            ResponsiblePersonName = t2.MemberName,
                            TransportationModeName = t4.ArgumentValue,
                            _DocumentDeadline = SqlFunc.MappingColumn(t1.ImportBillNO, "CONVERT(VARCHAR(100),DocumentDeadline, 111)"),
                            _ClosingDate = SqlFunc.MappingColumn(t1.ImportBillNO, "CONVERT(VARCHAR(100),ClosingDate, 111)"),
                            _ETC = SqlFunc.MappingColumn(t1.ImportBillNO, "CONVERT(VARCHAR(100),ETC, 111)"),
                            _ETD = SqlFunc.MappingColumn(t1.ImportBillNO, "CONVERT(VARCHAR(100),ETD, 111)"),
                            _ETA = SqlFunc.MappingColumn(t1.ImportBillNO, "CONVERT(VARCHAR(100),ETA, 111)"),
                            _ReminderAgentExecutionDate = SqlFunc.MappingColumn(t1.ImportBillNO, "CONVERT(VARCHAR(100),ReminderAgentExecutionDate, 111)"),
                            _PreExhibitionDate = SqlFunc.MappingColumn(t1.ImportBillNO, "CONVERT(VARCHAR(100),PreExhibitionDate, 111)"),
                            _ExitDate = SqlFunc.MappingColumn(t1.ImportBillNO, "CONVERT(VARCHAR(100),ExitDate, 111)"),
                        })
                        .MergeTable()
                        .OrderByIF(string.IsNullOrEmpty(sSortField), "IsAlert desc,ExhibitionDateStart desc,ExportBillName,RefNumber desc")
                        .OrderByIF(!string.IsNullOrEmpty(sSortField), sSortField + " " + sSortOrder)
                        .ToPageList(pml.PageIndex, bExcel ? 100000 : pml.PageSize, ref iPageCount);
                    pml.Total = iPageCount;

                    rm = new SuccessResponseMessage(null, i_crm);
                    if (bExcel)
                    {
                        var sFileName = "";
                        var oHeader_Last = new object();
                        var oHeader = new Dictionary<string, string>();
                        var listMerge = new List<Dictionary<string, int>>();
                        var dicAlain = new Dictionary<string, string>();
                        var dt_new = new DataTable();
                        var saCustomers1 = pml.DataList;
                        var saBasicInformation = pml.DataList as List<View_OPM_ExportExhibition>;
                        switch (sExcelType)
                        {
                            case "Export_BasicInformation":
                                {
                                    dt_new = saBasicInformation.ListToDataTable<View_OPM_ExportExhibition>();
                                    sFileName = "出口基本資料";
                                    oHeader = new Dictionary<string, string>
                                                {
                                                    { "RowIndex", "項次" },
                                                    { "RefNumber", "查詢號碼" },
                                                    { "ResponsiblePersonName", "負責業務" },
                                                    { "ExportBillName", "展覽名稱" },
                                                    { "ExportBillEName", "英文展名" },
                                                    { "ExhibitionDate", "展覽日期起訖" },
                                                    { "OrganizerName", "主辦單位" },
                                                    { "AgentName", "國外代理" },
                                                    { "CarriersNumber", "承運家數" },
                                                    { "ShipmentPort", "起運地" },
                                                    { "Destination", "目的地" },
                                                    { "BillLadNO", "提單號碼" },
                                                    { "BillLadNOSub", "分提單號碼" },
                                                    { "ContainerNumber", "貨櫃號碼" },
                                                    { "ExportDeclarationNO", "出口報單號碼" },
                                                    { "TransportationModeName", "運送方式" },
                                                    { "ShippingCompany", "船公司" },
                                                    { "_DocumentDeadline", "文件截止日" },
                                                    { "_ClosingDate", "收貨截止日" },
                                                    { "_ETC", "ETC" },
                                                    { "_ETD", "ETD" },
                                                    { "_ETA", "ETA" },
                                                    { "_ReminderAgentExecutionDate", "提醒代理執行日" },
                                                    { "_PreExhibitionDate", "佈展日期" },
                                                    { "_ExitDate", "退場日期" },
                                                    { "Memo", "特別注意事項" }
                                                };
                                    dicAlain = ExcelService.GetExportAlain(oHeader, "RefNumber,_ClosingDate,_ClosingDate,_ETC,_ETD,_ETA,_ReminderAgentExecutionDate,_PreExhibitionDate,_ExitDate");
                                    oHeader_Last = oHeader;
                                }
                                break;

                            case "Export_BusinessTrackingSchedule":
                                {
                                    var Export_BusinessTrackingSchedule_List = new List<Dictionary<string, object>>();
                                    var Export_BusinessTrackingSchedule_Group = new Dictionary<string, object>();
                                    var dt = saBasicInformation.ListToDataTable<View_OPM_ExportExhibition>();
                                    sFileName = "業務追蹤進度表";
                                    oHeader = new Dictionary<string, string>
                                                {
                                                    { "RowIndex", "項次" },
                                                    { "RefNumber", "查詢號碼" },
                                                    { "ResponsiblePersonName", "負責業務" },
                                                    { "ExportBillName", "展覽名稱" },
                                                    { "ExportBillEName", "英文展名" },
                                                    { "ExhibitionDate", "展覽日期起訖" },
                                                    { "OrganizerName", "主辦單位" },
                                                    { "AgentName", "國外代理" },
                                                    { "CarriersNumber", "承運家數" },
                                                    { "ShipmentPort", "起運地" },
                                                    { "Destination", "目的地" },
                                                    { "BillLadNO", "提單號碼" },
                                                    { "BillLadNOSub", "分提單號碼" },
                                                    { "ContainerNumber", "貨櫃號碼" },
                                                    { "ExportDeclarationNO", "出口報單號碼" },
                                                    { "TransportationModeName", "運送方式" },
                                                    { "ShippingCompany", "船公司" },
                                                    { "DocumentDeadline", "文件截止日" },
                                                    { "ClosingDate", "收貨截止日" },
                                                    { "ETC", "     ETC     " },
                                                    { "ETD", "     ETD     " },
                                                    { "ETA", "     ETA     " },
                                                    { "ReminderAgentExecutionDate", "提醒代理執行日" },
                                                    { "PreExhibitionDate", "佈展日期" },
                                                    { "ExitDate", "退場日期" },
                                                    { "Memo", "特別注意事項" }
                                                };
                                    dicAlain = ExcelService.GetExportAlain(oHeader, new string[] { "Email", "Contactor", "ContactorEmail" });

                                    Export_BusinessTrackingSchedule_Group.Add("Header", oHeader);
                                    Export_BusinessTrackingSchedule_Group.Add("HeaderName", "基本資料");
                                    Export_BusinessTrackingSchedule_Group.Add("Color", Color.Chocolate);
                                    Export_BusinessTrackingSchedule_List.Add(Export_BusinessTrackingSchedule_Group);
                                    oHeader = new Dictionary<string, string>();
                                    Export_BusinessTrackingSchedule_Group = new Dictionary<string, object>();

                                    oHeader.Add("Exhibitors_CustomerCode", "廠商代碼");
                                    oHeader.Add("Exhibitors_SupplierName", "廠商名稱");
                                    oHeader.Add("Exhibitors_RefSupplierNo", "廠商查詢號碼");
                                    oHeader.Add("Exhibitors_ExportBillNO", "帳單號碼");

                                    Export_BusinessTrackingSchedule_Group.Add("Header", oHeader);
                                    Export_BusinessTrackingSchedule_Group.Add("HeaderName", "參展商資料");
                                    Export_BusinessTrackingSchedule_Group.Add("Color", Color.Aqua);
                                    Export_BusinessTrackingSchedule_List.Add(Export_BusinessTrackingSchedule_Group);
                                    oHeader = new Dictionary<string, string>();
                                    Export_BusinessTrackingSchedule_Group = new Dictionary<string, object>();

                                    oHeader.Add("ExportData_Intowarehouse_Date", "貨物進倉");
                                    oHeader.Add("ExportData_Intowarehouse_Number", "到貨件數");
                                    oHeader.Add("ExportData_CustomsDeclaration_Date", "報關作業");
                                    oHeader.Add("ExportData_ExportRelease_Date", "出口放行");

                                    Export_BusinessTrackingSchedule_Group.Add("Header", oHeader);
                                    Export_BusinessTrackingSchedule_Group.Add("HeaderName", "貨物出口");
                                    Export_BusinessTrackingSchedule_Group.Add("Color", Color.HotPink);
                                    Export_BusinessTrackingSchedule_List.Add(Export_BusinessTrackingSchedule_Group);
                                    oHeader = new Dictionary<string, string>();
                                    Export_BusinessTrackingSchedule_Group = new Dictionary<string, object>();

                                    oHeader.Add("ClearanceData_GoodsArrival_Date", "貨物抵達");
                                    oHeader.Add("ClearanceData_CargoRelease_Date", "貨物放行");
                                    oHeader.Add("ClearanceData_WaitingApproach_Date", "等待進場");
                                    oHeader.Add("ClearanceData_ServiceBooth_Date", "送達攤位");

                                    Export_BusinessTrackingSchedule_Group.Add("Header", oHeader);
                                    Export_BusinessTrackingSchedule_Group.Add("HeaderName", "目的地清關作業");
                                    Export_BusinessTrackingSchedule_Group.Add("Color", Color.Yellow);
                                    Export_BusinessTrackingSchedule_List.Add(Export_BusinessTrackingSchedule_Group);
                                    oHeader = new Dictionary<string, string>();
                                    Export_BusinessTrackingSchedule_Group = new Dictionary<string, object>();

                                    dt_new = new DataTable();
                                    foreach (DataColumn col in dt.Columns)
                                    {
                                        dt_new.Columns.Add(col.ToString());
                                    }

                                    dt_new.Columns.Add("Exhibitors_CustomerCode");
                                    dt_new.Columns.Add("Exhibitors_SupplierName");
                                    dt_new.Columns.Add("Exhibitors_RefSupplierNo");
                                    dt_new.Columns.Add("Exhibitors_ExportBillNO");

                                    dt_new.Columns.Add("ExportData_Intowarehouse_Date");
                                    dt_new.Columns.Add("ExportData_Intowarehouse_Number");
                                    dt_new.Columns.Add("ExportData_CustomsDeclaration_Date");
                                    dt_new.Columns.Add("ExportData_ExportRelease_Date");

                                    dt_new.Columns.Add("ClearanceData_GoodsArrival_Date");
                                    dt_new.Columns.Add("ClearanceData_CargoRelease_Date");
                                    dt_new.Columns.Add("ClearanceData_WaitingApproach_Date");
                                    dt_new.Columns.Add("ClearanceData_ServiceBooth_Date");

                                    //添加Header
                                    var dicHeaderKeys = new Dictionary<string, int>();
                                    foreach (DataRow row in dt.Rows)
                                    {
                                        var JExhibitors = (JArray)JsonConvert.DeserializeObject(row["Exhibitors"].ToString());
                                        if (JExhibitors.Count > 0)
                                        {
                                            //廠商資料
                                            foreach (JObject Exhibitor in JExhibitors)
                                            {
                                                if (!dicHeaderKeys.Keys.Contains("ReImport") && Exhibitor["ReImport"] != null)
                                                {
                                                    oHeader.Add("ReImport_CargoCollection_Date", "貨物收取");
                                                    oHeader.Add("ReImport_Number", "退運件數");
                                                    oHeader.Add("ReImport_Unit", "單位");
                                                    oHeader.Add("ReImport_FileValidation_Date", "文件確認");
                                                    oHeader.Add("ReImport_TransportationMode", "運送方式");
                                                    oHeader.Add("ReImport_GoodsType", "貨型");
                                                    oHeader.Add("ReImport_HuiYun_Date", "回運中");
                                                    oHeader.Add("ReImport_GoodsArrival_Date", "貨物抵達");
                                                    oHeader.Add("ReImport_CustomsDeclaration_Date", "報關作業");
                                                    oHeader.Add("ReImport_CargoRelease_Date", "貨物放行");
                                                    oHeader.Add("ReImport_Sign_Date", "客戶簽收");

                                                    Export_BusinessTrackingSchedule_Group.Add("Header", oHeader);
                                                    Export_BusinessTrackingSchedule_Group.Add("HeaderName", "展貨退運");
                                                    Export_BusinessTrackingSchedule_Group.Add("Color", Color.LightSalmon);
                                                    Export_BusinessTrackingSchedule_List.Add(Export_BusinessTrackingSchedule_Group);
                                                    oHeader = new Dictionary<string, string>();
                                                    Export_BusinessTrackingSchedule_Group = new Dictionary<string, object>();

                                                    dt_new.Columns.Add("ReImport_CargoCollection_Date");
                                                    dt_new.Columns.Add("ReImport_Number");
                                                    dt_new.Columns.Add("ReImport_Unit");
                                                    dt_new.Columns.Add("ReImport_FileValidation_Date");
                                                    dt_new.Columns.Add("ReImport_TransportationMode");
                                                    dt_new.Columns.Add("ReImport_GoodsType");
                                                    dt_new.Columns.Add("ReImport_HuiYun_Date");
                                                    dt_new.Columns.Add("ReImport_GoodsArrival_Date");
                                                    dt_new.Columns.Add("ReImport_CustomsDeclaration_Date");
                                                    dt_new.Columns.Add("ReImport_CargoRelease_Date");
                                                    dt_new.Columns.Add("ReImport_Sign_Date");

                                                    dicHeaderKeys.Add("ReImport", dicHeaderKeys.Keys.Count + 1);
                                                }
                                                if (!dicHeaderKeys.Keys.Contains("TranserThird") && Exhibitor["TranserThird"] != null)
                                                {
                                                    oHeader.Add("TranserThird_CargoCollection_Date", "貨物收取");
                                                    oHeader.Add("TranserThird_Number", "件數");
                                                    oHeader.Add("TranserThird_Unit", "單位");
                                                    oHeader.Add("TranserThird_FileValidation_Date", "文件確認");
                                                    oHeader.Add("TranserThird_TransportationMode", "運送方式");
                                                    oHeader.Add("TranserThird_GoodsType", "貨型");
                                                    oHeader.Add("TranserThird_Destination", "目的地");
                                                    oHeader.Add("TranserThird_InTransit_Date", "回運中");
                                                    oHeader.Add("TranserThird_GoodsArrival_Date", "貨物抵達");
                                                    oHeader.Add("TranserThird_CargoRelease_Date", "貨物放行");
                                                    oHeader.Add("TranserThird_ServiceBooth_Date", "送達攤位");

                                                    Export_BusinessTrackingSchedule_Group.Add("Header", oHeader);
                                                    Export_BusinessTrackingSchedule_Group.Add("HeaderName", "轉運其他地區");
                                                    Export_BusinessTrackingSchedule_Group.Add("Color", Color.Bisque);
                                                    Export_BusinessTrackingSchedule_List.Add(Export_BusinessTrackingSchedule_Group);
                                                    oHeader = new Dictionary<string, string>();
                                                    Export_BusinessTrackingSchedule_Group = new Dictionary<string, object>();

                                                    dt_new.Columns.Add("TranserThird_CargoCollection_Date");
                                                    dt_new.Columns.Add("TranserThird_Number");
                                                    dt_new.Columns.Add("TranserThird_Unit");
                                                    dt_new.Columns.Add("TranserThird_FileValidation_Date");
                                                    dt_new.Columns.Add("TranserThird_TransportationMode");
                                                    dt_new.Columns.Add("TranserThird_GoodsType");
                                                    dt_new.Columns.Add("TranserThird_Destination");
                                                    dt_new.Columns.Add("TranserThird_InTransit_Date");
                                                    dt_new.Columns.Add("TranserThird_GoodsArrival_Date");
                                                    dt_new.Columns.Add("TranserThird_CargoRelease_Date");
                                                    dt_new.Columns.Add("TranserThird_ServiceBooth_Date");

                                                    dicHeaderKeys.Add("TranserThird", dicHeaderKeys.Keys.Count + 1);
                                                }
                                                if (!dicHeaderKeys.Keys.Contains("PartThird") && Exhibitor["PartThird"] != null)
                                                {
                                                    oHeader.Add("PartThird_CargoCollection_Date", "貨物收取");
                                                    oHeader.Add("PartThird_Number", "件數");
                                                    oHeader.Add("PartThird_Unit", "單位");
                                                    oHeader.Add("PartThird_FileValidation_Date", "文件確認");
                                                    oHeader.Add("PartThird_TransportationMode", "運送方式");
                                                    oHeader.Add("PartThird_GoodsType", "貨型");
                                                    oHeader.Add("PartThird_Destination", "目的地");
                                                    oHeader.Add("PartThird_InTransit_Date", "回運中");
                                                    oHeader.Add("PartThird_GoodsArrival_Date", "貨物抵達");
                                                    oHeader.Add("PartThird_CargoRelease_Date", "貨物放行");
                                                    oHeader.Add("PartThird_Sign_Date", "客戶簽收");

                                                    Export_BusinessTrackingSchedule_Group.Add("Header", oHeader);
                                                    Export_BusinessTrackingSchedule_Group.Add("HeaderName", "轉運其他地區(部份轉運其他地區;部份退運回台)");
                                                    Export_BusinessTrackingSchedule_Group.Add("Color", Color.LightCoral);
                                                    Export_BusinessTrackingSchedule_List.Add(Export_BusinessTrackingSchedule_Group);
                                                    oHeader = new Dictionary<string, string>();
                                                    Export_BusinessTrackingSchedule_Group = new Dictionary<string, object>();

                                                    dt_new.Columns.Add("PartThird_CargoCollection_Date");
                                                    dt_new.Columns.Add("PartThird_Number");
                                                    dt_new.Columns.Add("PartThird_Unit");
                                                    dt_new.Columns.Add("PartThird_FileValidation_Date");
                                                    dt_new.Columns.Add("PartThird_TransportationMode");
                                                    dt_new.Columns.Add("PartThird_GoodsType");
                                                    dt_new.Columns.Add("PartThird_Destination");
                                                    dt_new.Columns.Add("PartThird_InTransit_Date");
                                                    dt_new.Columns.Add("PartThird_GoodsArrival_Date");
                                                    dt_new.Columns.Add("PartThird_CargoRelease_Date");
                                                    dt_new.Columns.Add("PartThird_Sign_Date");

                                                    dicHeaderKeys.Add("PartThird", dicHeaderKeys.Keys.Count + 1);
                                                }
                                                if (!dicHeaderKeys.Keys.Contains("PartReImport") && Exhibitor["PartReImport"] != null)
                                                {
                                                    oHeader.Add("PartReImport_CargoCollection_Date", "貨物收取");
                                                    oHeader.Add("PartReImport_Number", "退運件數");
                                                    oHeader.Add("PartReImport_Unit", "單位");
                                                    oHeader.Add("PartReImport_FileValidation_Date", "文件確認");
                                                    oHeader.Add("PartReImport_GoodsType", "貨型");
                                                    oHeader.Add("PartReImport_HuiYun_Date", "回運中");
                                                    oHeader.Add("PartReImport_GoodsArrival_Date", "貨物抵達");
                                                    oHeader.Add("PartReImport_CustomsDeclaration_Date", "報關作業");
                                                    oHeader.Add("PartReImport_CargoRelease_Date", "貨物放行");
                                                    oHeader.Add("PartReImport_Sign_Date", "客戶簽收");

                                                    Export_BusinessTrackingSchedule_Group.Add("Header", oHeader);
                                                    Export_BusinessTrackingSchedule_Group.Add("HeaderName", "展後回運(部份轉運其他地區;部份退運回台)");
                                                    Export_BusinessTrackingSchedule_Group.Add("Color", Color.LightSalmon);
                                                    Export_BusinessTrackingSchedule_List.Add(Export_BusinessTrackingSchedule_Group);
                                                    oHeader = new Dictionary<string, string>();
                                                    Export_BusinessTrackingSchedule_Group = new Dictionary<string, object>();

                                                    dt_new.Columns.Add("PartReImport_CargoCollection_Date");
                                                    dt_new.Columns.Add("PartReImport_Number");
                                                    dt_new.Columns.Add("PartReImport_Unit");
                                                    dt_new.Columns.Add("PartReImport_FileValidation_Date");
                                                    dt_new.Columns.Add("PartReImport_GoodsType");
                                                    dt_new.Columns.Add("PartReImport_HuiYun_Date");
                                                    dt_new.Columns.Add("PartReImport_GoodsArrival_Date");
                                                    dt_new.Columns.Add("PartReImport_CustomsDeclaration_Date");
                                                    dt_new.Columns.Add("PartReImport_CargoRelease_Date");
                                                    dt_new.Columns.Add("PartReImport_Sign_Date");

                                                    dicHeaderKeys.Add("PartReImport", dicHeaderKeys.Keys.Count + 1);
                                                }
                                                if (!dicHeaderKeys.Keys.Contains("ReImportFour") && Exhibitor["ReImportFour"] != null)
                                                {
                                                    oHeader.Add("ReImportFour_CargoCollection_Date", "貨物收取");
                                                    oHeader.Add("ReImportFour_Number", "退運件數");
                                                    oHeader.Add("ReImportFour_Unit", "單位");
                                                    oHeader.Add("ReImportFour_FileValidation_Date", "文件確認");
                                                    oHeader.Add("ReImportFour_TransportationMode", "運送方式");
                                                    oHeader.Add("ReImportFour_GoodsType", "貨型");
                                                    oHeader.Add("ReImportFour_HuiYun_Date", "回運中");
                                                    oHeader.Add("ReImportFour_GoodsArrival_Date", "貨物抵達");
                                                    oHeader.Add("ReImportFour_CustomsDeclaration_Date", "報關作業");
                                                    oHeader.Add("ReImportFour_CargoRelease_Date", "貨物放行");
                                                    oHeader.Add("ReImportFour_Sign_Date", "客戶簽收");

                                                    Export_BusinessTrackingSchedule_Group.Add("Header", oHeader);
                                                    Export_BusinessTrackingSchedule_Group.Add("HeaderName", "展後回運(出貨至第第三地後)");
                                                    Export_BusinessTrackingSchedule_Group.Add("Color", Color.LightSalmon);
                                                    Export_BusinessTrackingSchedule_List.Add(Export_BusinessTrackingSchedule_Group);
                                                    oHeader = new Dictionary<string, string>();
                                                    Export_BusinessTrackingSchedule_Group = new Dictionary<string, object>();

                                                    dt_new.Columns.Add("ReImportFour_CargoCollection_Date");
                                                    dt_new.Columns.Add("ReImportFour_Number");
                                                    dt_new.Columns.Add("ReImportFour_Unit");
                                                    dt_new.Columns.Add("ReImportFour_FileValidation_Date");
                                                    dt_new.Columns.Add("ReImportFour_TransportationMode");
                                                    dt_new.Columns.Add("ReImportFour_GoodsType");
                                                    dt_new.Columns.Add("ReImportFour_HuiYun_Date");
                                                    dt_new.Columns.Add("ReImportFour_GoodsArrival_Date");
                                                    dt_new.Columns.Add("ReImportFour_CustomsDeclaration_Date");
                                                    dt_new.Columns.Add("ReImportFour_CargoRelease_Date");
                                                    dt_new.Columns.Add("ReImportFour_Sign_Date");

                                                    dicHeaderKeys.Add("ReImportFour", dicHeaderKeys.Keys.Count + 1);
                                                }
                                                if (!dicHeaderKeys.Keys.Contains("TransferFour") && Exhibitor["TransferFour"] != null)
                                                {
                                                    oHeader.Add("TransferFour_CargoCollection_Date", "貨物收取");
                                                    oHeader.Add("TransferFour_Number", "件數");
                                                    oHeader.Add("TransferFour_Unit", "單位");
                                                    oHeader.Add("TransferFour_FileValidation_Date", "文件確認");
                                                    oHeader.Add("TransferFour_TransportationMode", "運送方式");
                                                    oHeader.Add("TransferFour_GoodsType", "貨型");
                                                    oHeader.Add("TransferFour_Destination", "目的地");
                                                    oHeader.Add("TransferFour_InTransit_Date", "回運中");
                                                    oHeader.Add("TransferFour_GoodsArrival_Date", "貨物抵達");
                                                    oHeader.Add("TransferFour_CargoRelease_Date", "貨物放行");
                                                    oHeader.Add("TransferFour_Sign_Date", "客戶簽收");

                                                    Export_BusinessTrackingSchedule_Group.Add("Header", oHeader);
                                                    Export_BusinessTrackingSchedule_Group.Add("HeaderName", "出貨至第四地");
                                                    Export_BusinessTrackingSchedule_Group.Add("Color", Color.PaleGreen);
                                                    Export_BusinessTrackingSchedule_List.Add(Export_BusinessTrackingSchedule_Group);
                                                    oHeader = new Dictionary<string, string>();
                                                    Export_BusinessTrackingSchedule_Group = new Dictionary<string, object>();

                                                    dt_new.Columns.Add("TransferFour_CargoCollection_Date");
                                                    dt_new.Columns.Add("TransferFour_Number");
                                                    dt_new.Columns.Add("TransferFour_Unit");
                                                    dt_new.Columns.Add("TransferFour_FileValidation_Date");
                                                    dt_new.Columns.Add("TransferFour_TransportationMode");
                                                    dt_new.Columns.Add("TransferFour_GoodsType");
                                                    dt_new.Columns.Add("TransferFour_Destination");
                                                    dt_new.Columns.Add("TransferFour_InTransit_Date");
                                                    dt_new.Columns.Add("TransferFour_GoodsArrival_Date");
                                                    dt_new.Columns.Add("TransferFour_CargoRelease_Date");
                                                    dt_new.Columns.Add("TransferFour_Sign_Date");

                                                    dicHeaderKeys.Add("TransferFour", dicHeaderKeys.Keys.Count + 1);
                                                }
                                                if (!dicHeaderKeys.Keys.Contains("ReImportFive") && Exhibitor["ReImportFive"] != null)
                                                {
                                                    oHeader.Add("ReImportFive_CargoCollection_Date", "貨物收取");
                                                    oHeader.Add("ReImportFive_Number", "退運件數");
                                                    oHeader.Add("ReImportFive_Unit", "單位");
                                                    oHeader.Add("ReImportFive_FileValidation_Date", "文件確認");
                                                    oHeader.Add("ReImportFive_TransportationMode", "運送方式");
                                                    oHeader.Add("ReImportFive_GoodsType", "貨型");
                                                    oHeader.Add("ReImportFive_HuiYun_Date", "回運中");
                                                    oHeader.Add("ReImportFive_GoodsArrival_Date", "貨物抵達");
                                                    oHeader.Add("ReImportFive_CustomsDeclaration_Date", "報關作業");
                                                    oHeader.Add("ReImportFive_CargoRelease_Date", "貨物放行");
                                                    oHeader.Add("ReImportFive_Sign_Date", "客戶簽收");

                                                    Export_BusinessTrackingSchedule_Group.Add("Header", oHeader);
                                                    Export_BusinessTrackingSchedule_Group.Add("HeaderName", "展後回運(出貨至第四地後)");
                                                    Export_BusinessTrackingSchedule_Group.Add("Color", Color.CornflowerBlue);
                                                    Export_BusinessTrackingSchedule_List.Add(Export_BusinessTrackingSchedule_Group);
                                                    oHeader = new Dictionary<string, string>();
                                                    Export_BusinessTrackingSchedule_Group = new Dictionary<string, object>();

                                                    dt_new.Columns.Add("ReImportFive_CargoCollection_Date");
                                                    dt_new.Columns.Add("ReImportFive_Number");
                                                    dt_new.Columns.Add("ReImportFive_Unit");
                                                    dt_new.Columns.Add("ReImportFive_FileValidation_Date");
                                                    dt_new.Columns.Add("ReImportFive_TransportationMode");
                                                    dt_new.Columns.Add("ReImportFive_GoodsType");
                                                    dt_new.Columns.Add("ReImportFive_HuiYun_Date");
                                                    dt_new.Columns.Add("ReImportFive_GoodsArrival_Date");
                                                    dt_new.Columns.Add("ReImportFive_CustomsDeclaration_Date");
                                                    dt_new.Columns.Add("ReImportFive_CargoRelease_Date");
                                                    dt_new.Columns.Add("ReImportFive_Sign_Date");

                                                    dicHeaderKeys.Add("ReImportFive", dicHeaderKeys.Keys.Count + 1);
                                                }
                                                if (!dicHeaderKeys.Keys.Contains("TransferFive") && Exhibitor["TransferFive"] != null)
                                                {
                                                    oHeader.Add("TransferFive_CargoCollection_Date", "貨物收取");
                                                    oHeader.Add("TransferFive_Number", "件數");
                                                    oHeader.Add("TransferFive_Unit", "單位");
                                                    oHeader.Add("TransferFive_FileValidation_Date", "文件確認");
                                                    oHeader.Add("TransferFive_TransportationMode", "運送方式");
                                                    oHeader.Add("TransferFive_GoodsType", "貨型");
                                                    oHeader.Add("TransferFive_Destination", "目的地");
                                                    oHeader.Add("TransferFive_InTransit_Date", "回運中");
                                                    oHeader.Add("TransferFive_GoodsArrival_Date", "貨物抵達");
                                                    oHeader.Add("TransferFive_CargoRelease_Date", "貨物放行");
                                                    oHeader.Add("TransferFive_Sign_Date", "客戶簽收");

                                                    Export_BusinessTrackingSchedule_Group.Add("Header", oHeader);
                                                    Export_BusinessTrackingSchedule_Group.Add("HeaderName", "出貨至第五地");
                                                    Export_BusinessTrackingSchedule_Group.Add("Color", Color.ForestGreen);
                                                    Export_BusinessTrackingSchedule_List.Add(Export_BusinessTrackingSchedule_Group);
                                                    oHeader = new Dictionary<string, string>();
                                                    Export_BusinessTrackingSchedule_Group = new Dictionary<string, object>();

                                                    dt_new.Columns.Add("TransferFive_CargoCollection_Date");
                                                    dt_new.Columns.Add("TransferFive_Number");
                                                    dt_new.Columns.Add("TransferFive_Unit");
                                                    dt_new.Columns.Add("TransferFive_FileValidation_Date");
                                                    dt_new.Columns.Add("TransferFive_TransportationMode");
                                                    dt_new.Columns.Add("TransferFive_GoodsType");
                                                    dt_new.Columns.Add("TransferFive_Destination");
                                                    dt_new.Columns.Add("TransferFive_InTransit_Date");
                                                    dt_new.Columns.Add("TransferFive_GoodsArrival_Date");
                                                    dt_new.Columns.Add("TransferFive_CargoRelease_Date");
                                                    dt_new.Columns.Add("TransferFive_Sign_Date");

                                                    dicHeaderKeys.Add("TransferFive", dicHeaderKeys.Keys.Count + 1);
                                                }
                                                if (!dicHeaderKeys.Keys.Contains("ReImportSix") && Exhibitor["ReImportSix"] != null)
                                                {
                                                    oHeader.Add("ReImportSix_CargoCollection_Date", "貨物收取");
                                                    oHeader.Add("ReImportSix_Number", "退運件數");
                                                    oHeader.Add("ReImportSix_Unit", "單位");
                                                    oHeader.Add("ReImportSix_FileValidation_Date", "文件確認");
                                                    oHeader.Add("ReImportSix_TransportationMode", "運送方式");
                                                    oHeader.Add("ReImportSix_GoodsType", "貨型");
                                                    oHeader.Add("ReImportSix_HuiYun_Date", "回運中");
                                                    oHeader.Add("ReImportSix_GoodsArrival_Date", "貨物抵達");
                                                    oHeader.Add("ReImportSix_CustomsDeclaration_Date", "報關作業");
                                                    oHeader.Add("ReImportSix_CustomsDeclaration_CustomsClearance", "通關方式");
                                                    oHeader.Add("ReImportSix_CargoRelease_Date", "貨物放行");
                                                    oHeader.Add("ReImportSix_Sign_Date", "客戶簽收");

                                                    Export_BusinessTrackingSchedule_Group.Add("Header", oHeader);
                                                    Export_BusinessTrackingSchedule_Group.Add("HeaderName", "展後回運(出貨至第五地後)");
                                                    Export_BusinessTrackingSchedule_Group.Add("Color", Color.DimGray);
                                                    Export_BusinessTrackingSchedule_List.Add(Export_BusinessTrackingSchedule_Group);
                                                    oHeader = new Dictionary<string, string>();
                                                    Export_BusinessTrackingSchedule_Group = new Dictionary<string, object>();

                                                    dt_new.Columns.Add("ReImportSix_CargoCollection_Date");
                                                    dt_new.Columns.Add("ReImportSix_Number");
                                                    dt_new.Columns.Add("ReImportSix_Unit");
                                                    dt_new.Columns.Add("ReImportSix_FileValidation_Date");
                                                    dt_new.Columns.Add("ReImportSix_TransportationMode");
                                                    dt_new.Columns.Add("ReImportSix_GoodsType");
                                                    dt_new.Columns.Add("ReImportSix_HuiYun_Date");
                                                    dt_new.Columns.Add("ReImportSix_GoodsArrival_Date");
                                                    dt_new.Columns.Add("ReImportSix_CustomsDeclaration_Date");
                                                    dt_new.Columns.Add("ReImportSix_CustomsDeclaration_CustomsClearance");
                                                    dt_new.Columns.Add("ReImportSix_CargoRelease_Date");
                                                    dt_new.Columns.Add("ReImportSix_Sign_Date");

                                                    dicHeaderKeys.Add("ReImportSix", dicHeaderKeys.Keys.Count + 1);
                                                }
                                                if (!dicHeaderKeys.Keys.Contains("TransferSix") && Exhibitor["TransferSix"] != null)
                                                {
                                                    oHeader.Add("TransferSix_CargoCollection_Date", "貨物收取");
                                                    oHeader.Add("TransferSix_Number", "件數");
                                                    oHeader.Add("TransferSix_Unit", "單位");
                                                    oHeader.Add("TransferSix_FileValidation_Date", "文件確認");
                                                    oHeader.Add("TransferSix_TransportationMode", "運送方式");
                                                    oHeader.Add("TransferSix_GoodsType", "貨型");
                                                    oHeader.Add("TransferSix_Destination", "目的地");
                                                    oHeader.Add("TransferSix_InTransit_Date", "回運中");
                                                    oHeader.Add("TransferSix_GoodsArrival_Date", "貨物抵達");
                                                    oHeader.Add("TransferSix_CargoRelease_Date", "貨物放行");
                                                    oHeader.Add("TransferSix_Sign_Date", "客戶簽收");

                                                    Export_BusinessTrackingSchedule_Group.Add("Header", oHeader);
                                                    Export_BusinessTrackingSchedule_Group.Add("HeaderName", "出貨至第六地");
                                                    Export_BusinessTrackingSchedule_Group.Add("Color", Color.SlateGray);
                                                    Export_BusinessTrackingSchedule_List.Add(Export_BusinessTrackingSchedule_Group);
                                                    oHeader = new Dictionary<string, string>();
                                                    Export_BusinessTrackingSchedule_Group = new Dictionary<string, object>();

                                                    dt_new.Columns.Add("TransferSix_CargoCollection_Date");
                                                    dt_new.Columns.Add("TransferSix_Number");
                                                    dt_new.Columns.Add("TransferSix_Unit");
                                                    dt_new.Columns.Add("TransferSix_FileValidation_Date");
                                                    dt_new.Columns.Add("TransferSix_TransportationMode");
                                                    dt_new.Columns.Add("TransferSix_GoodsType");
                                                    dt_new.Columns.Add("TransferSix_Destination");
                                                    dt_new.Columns.Add("TransferSix_InTransit_Date");
                                                    dt_new.Columns.Add("TransferSix_GoodsArrival_Date");
                                                    dt_new.Columns.Add("TransferSix_CargoRelease_Date");
                                                    dt_new.Columns.Add("TransferSix_Sign_Date");

                                                    dicHeaderKeys.Add("TransferSix", dicHeaderKeys.Keys.Count + 1);
                                                }
                                            }
                                        }
                                    }

                                    var iFirstRow = 4;
                                    foreach (DataRow row in dt.Rows)
                                    {
                                        var row_new = dt_new.NewRow();//
                                        var JExhibitors = (JArray)JsonConvert.DeserializeObject(row["Exhibitors"].ToString());
                                        if (JExhibitors.Count > 0)
                                        {
                                            //合併基本資料
                                            var iMergeColCount = ((Dictionary<string, string>)Export_BusinessTrackingSchedule_List[0]["Header"]).Keys.Count;
                                            for (var iFirstCol = 0; iFirstCol < iMergeColCount; iFirstCol++)
                                            {
                                                var dicMerge = new Dictionary<string, int>
                                    {
                                        { "FirstRow", iFirstRow },
                                        { "FirstCol", iFirstCol },
                                        { "RowCount", JExhibitors.Count },
                                        { "ColCount", 1 }
                                    };
                                                listMerge.Add(dicMerge);
                                            }
                                            //廠商資料
                                            foreach (JObject Exhibitor in JExhibitors)
                                            {
                                                row_new = dt_new.NewRow();
                                                foreach (DataColumn col in dt.Columns)
                                                {
                                                    row_new[col.ToString()] = row[col.ToString()];
                                                }
                                                var jExportData = (JObject)Exhibitor["ExportData"];
                                                var jClearanceData = (JObject)Exhibitor["ClearanceData"];
                                                var jReImport = (JObject)Exhibitor["ReImport"];
                                                var jTranserThird = (JObject)Exhibitor["TranserThird"];
                                                var jPartThird = (JObject)Exhibitor["PartThird"];
                                                var jPartReImport = (JObject)Exhibitor["PartReImport"];
                                                var jReImportFour = (JObject)Exhibitor["ReImportFour"];
                                                var jTransferFour = (JObject)Exhibitor["TransferFour"];
                                                var jReImportFive = (JObject)Exhibitor["ReImportFive"];
                                                var jTransferFive = (JObject)Exhibitor["TransferFive"];
                                                var jReImportSix = (JObject)Exhibitor["ReImportSix"];
                                                var jTransferSix = (JObject)Exhibitor["TransferSix"];

                                                row_new["Exhibitors_CustomerCode"] = Exhibitor["CustomerCode"];
                                                row_new["Exhibitors_SupplierName"] = Exhibitor["SupplierName"];
                                                row_new["Exhibitors_RefSupplierNo"] = Exhibitor["RefSupplierNo"];
                                                row_new["Exhibitors_ExportBillNO"] = Exhibitor["BillNO"] ?? "";
                                                if (jExportData != null)
                                                {
                                                    row_new["ExportData_Intowarehouse_Date"] = ((JObject)jExportData["Intowarehouse"])["Date"];
                                                    row_new["ExportData_Intowarehouse_Number"] = ((JObject)jExportData["Intowarehouse"])["Number"];
                                                    row_new["ExportData_CustomsDeclaration_Date"] = ((JObject)jExportData["CustomsDeclaration"])["Date"];
                                                    row_new["ExportData_ExportRelease_Date"] = ((JObject)jExportData["ExportRelease"])["Date"];
                                                }

                                                if (jClearanceData != null)
                                                {
                                                    row_new["ClearanceData_GoodsArrival_Date"] = ((JObject)jClearanceData["GoodsArrival"])["Date"];
                                                    row_new["ClearanceData_CargoRelease_Date"] = ((JObject)jClearanceData["CargoRelease"])["Date"];
                                                    row_new["ClearanceData_WaitingApproach_Date"] = jClearanceData["WaitingApproach"] == null ? "" : ((JObject)jClearanceData["WaitingApproach"])["Date"];
                                                    row_new["ClearanceData_ServiceBooth_Date"] = ((JObject)jClearanceData["ServiceBooth"])["Date"];
                                                }

                                                if (jReImport != null)
                                                {
                                                    row_new["ReImport_CargoCollection_Date"] = ((JObject)jReImport["CargoCollection"])["Date"];
                                                    row_new["ReImport_Number"] = jReImport["Number"];
                                                    row_new["ReImport_Unit"] = jReImport["Unit"];
                                                    row_new["ReImport_FileValidation_Date"] = ((JObject)jReImport["FileValidation"])["Date"];
                                                    var sTransportationMode = jReImport["TransportationMode"] == null ? "" : jReImport["TransportationMode"].ToString();
                                                    if (sTransportationMode != "")
                                                    {
                                                        var oArgument = db.Queryable<OTB_SYS_Arguments>()
                                                            .Single(it => it.OrgID == i_crm.ORIGID && it.ArgumentClassID == "Transport" && it.ArgumentID == sTransportationMode);
                                                        if (oArgument == null)
                                                        {
                                                            oArgument = new OTB_SYS_Arguments();
                                                        }
                                                        row_new["ReImport_TransportationMode"] = oArgument.ArgumentValue ?? "";
                                                    }
                                                    row_new["ReImport_GoodsType"] = jReImport["GoodsType"];
                                                    row_new["ReImport_HuiYun_Date"] = ((JObject)jReImport["HuiYun"])["Date"];
                                                    row_new["ReImport_GoodsArrival_Date"] = ((JObject)jReImport["GoodsArrival"])["Date"];
                                                    row_new["ReImport_CustomsDeclaration_Date"] = ((JObject)jReImport["CustomsDeclaration"])["Date"];
                                                    row_new["ReImport_CargoRelease_Date"] = ((JObject)jReImport["CargoRelease"])["Date"];
                                                    row_new["ReImport_Sign_Date"] = ((JObject)jReImport["Sign"])["Date"];
                                                }

                                                if (jTranserThird != null)
                                                {
                                                    row_new["TranserThird_CargoCollection_Date"] = ((JObject)jTranserThird["CargoCollection"])["Date"];
                                                    row_new["TranserThird_Number"] = jTranserThird["Number"];
                                                    row_new["TranserThird_Unit"] = jTranserThird["Number"];
                                                    row_new["TranserThird_FileValidation_Date"] = ((JObject)jTranserThird["FileValidation"])["Date"];
                                                    var sTransportationMode = jTranserThird["TransportationMode"] == null ? "" : jTranserThird["TransportationMode"].ToString();
                                                    if (sTransportationMode != "")
                                                    {
                                                        var oArgument = db.Queryable<OTB_SYS_Arguments>()
                                                            .Single(it => it.OrgID == i_crm.ORIGID && it.ArgumentClassID == "Transport" && it.ArgumentID == sTransportationMode);
                                                        if (oArgument == null)
                                                        {
                                                            oArgument = new OTB_SYS_Arguments();
                                                        }
                                                        row_new["TranserThird_TransportationMode"] = oArgument.ArgumentValue ?? "";
                                                    }
                                                    row_new["TranserThird_GoodsType"] = jTranserThird["GoodsType"];
                                                    row_new["TranserThird_Destination"] = jTranserThird["Destination"];
                                                    row_new["TranserThird_InTransit_Date"] = ((JObject)jTranserThird["InTransit"])["Date"];
                                                    row_new["TranserThird_GoodsArrival_Date"] = ((JObject)jTranserThird["GoodsArrival"])["Date"];
                                                    row_new["TranserThird_CargoRelease_Date"] = ((JObject)jTranserThird["CargoRelease"])["Date"];
                                                    row_new["TranserThird_ServiceBooth_Date"] = ((JObject)jTranserThird["ServiceBooth"])["Date"];
                                                }
                                                if (jPartThird != null)
                                                {
                                                    row_new["PartThird_CargoCollection_Date"] = ((JObject)jPartThird["CargoCollection"])["Date"];
                                                    row_new["PartThird_Number"] = jPartThird["Number"];
                                                    row_new["PartThird_Unit"] = jPartThird["Unit"];
                                                    row_new["PartThird_FileValidation_Date"] = ((JObject)jPartThird["FileValidation"])["Date"];
                                                    var sTransportationMode = jPartThird["TransportationMode"] == null ? "" : jPartThird["TransportationMode"].ToString();
                                                    if (sTransportationMode != "")
                                                    {
                                                        var oArgument = db.Queryable<OTB_SYS_Arguments>()
                                                            .Single(it => it.OrgID == i_crm.ORIGID && it.ArgumentClassID == "Transport" && it.ArgumentID == sTransportationMode);
                                                        if (oArgument == null)
                                                        {
                                                            oArgument = new OTB_SYS_Arguments();
                                                        }
                                                        row_new["PartThird_TransportationMode"] = oArgument.ArgumentValue ?? "";
                                                    }
                                                    row_new["PartThird_GoodsType"] = jPartThird["GoodsType"];
                                                    row_new["PartThird_Destination"] = jPartThird["Destination"];
                                                    row_new["PartThird_InTransit_Date"] = ((JObject)jPartThird["InTransit"])["Date"];
                                                    row_new["PartThird_GoodsArrival_Date"] = ((JObject)jPartThird["GoodsArrival"])["Date"];
                                                    row_new["PartThird_CargoRelease_Date"] = ((JObject)jPartThird["CargoRelease"])["Date"];
                                                    row_new["PartThird_Sign_Date"] = ((JObject)jPartThird["Sign"])["Date"];
                                                }
                                                if (jPartReImport != null)
                                                {
                                                    row_new["PartReImport_CargoCollection_Date"] = ((JObject)jPartReImport["CargoCollection"])["Date"];
                                                    row_new["PartReImport_Number"] = jPartReImport["Number"];
                                                    row_new["PartReImport_Unit"] = jPartReImport["Unit"];
                                                    row_new["PartReImport_FileValidation_Date"] = ((JObject)jPartReImport["FileValidation"])["Date"];
                                                    row_new["PartReImport_GoodsType"] = jPartReImport["GoodsType"];
                                                    row_new["PartReImport_HuiYun_Date"] = ((JObject)jPartReImport["HuiYun"])["Date"];
                                                    row_new["PartReImport_GoodsArrival_Date"] = ((JObject)jPartReImport["GoodsArrival"])["Date"];
                                                    row_new["PartReImport_CustomsDeclaration_Date"] = ((JObject)jPartReImport["CustomsDeclaration"])["Date"];
                                                    row_new["PartReImport_CargoRelease_Date"] = ((JObject)jPartReImport["CargoRelease"])["Date"];
                                                    row_new["PartReImport_Sign_Date"] = ((JObject)jPartReImport["Sign"])["Date"];
                                                }
                                                if (jReImportFour != null)
                                                {
                                                    row_new["ReImportFour_CargoCollection_Date"] = ((JObject)jReImportFour["CargoCollection"])["Date"];
                                                    row_new["ReImportFour_Number"] = jReImportFour["Number"];
                                                    row_new["ReImportFour_Unit"] = jReImportFour["Unit"];
                                                    row_new["ReImportFour_FileValidation_Date"] = ((JObject)jReImportFour["FileValidation"])["Date"];
                                                    var sTransportationMode = jReImportFour["TransportationMode"] == null ? "" : jReImportFour["TransportationMode"].ToString();
                                                    if (sTransportationMode != "")
                                                    {
                                                        var oArgument = db.Queryable<OTB_SYS_Arguments>()
                                                            .Single(it => it.OrgID == i_crm.ORIGID && it.ArgumentClassID == "Transport" && it.ArgumentID == sTransportationMode);
                                                        if (oArgument == null)
                                                        {
                                                            oArgument = new OTB_SYS_Arguments();
                                                        }
                                                        row_new["ReImportFour_TransportationMode"] = oArgument.ArgumentValue ?? "";
                                                    }
                                                    row_new["ReImportFour_GoodsType"] = jReImportFour["GoodsType"];
                                                    row_new["ReImportFour_HuiYun_Date"] = ((JObject)jReImportFour["HuiYun"])["Date"];
                                                    row_new["ReImportFour_GoodsArrival_Date"] = ((JObject)jReImportFour["GoodsArrival"])["Date"];
                                                    row_new["ReImportFour_CustomsDeclaration_Date"] = ((JObject)jReImportFour["CustomsDeclaration"])["Date"];
                                                    row_new["ReImportFour_CargoRelease_Date"] = ((JObject)jReImportFour["CargoRelease"])["Date"];
                                                    row_new["ReImportFour_Sign_Date"] = ((JObject)jReImportFour["Sign"])["Date"];
                                                }
                                                if (jTransferFour != null)
                                                {
                                                    row_new["TransferFour_CargoCollection_Date"] = ((JObject)jTransferFour["CargoCollection"])["Date"];
                                                    row_new["TransferFour_Number"] = jTransferFour["Number"];
                                                    row_new["TransferFour_Unit"] = jTransferFour["Unit"];
                                                    row_new["TransferFour_FileValidation_Date"] = ((JObject)jTransferFour["FileValidation"])["Date"];
                                                    var sTransportationMode = jTransferFour["TransportationMode"] == null ? "" : jTransferFour["TransportationMode"].ToString();
                                                    if (sTransportationMode != "")
                                                    {
                                                        var oArgument = db.Queryable<OTB_SYS_Arguments>()
                                                            .Single(it => it.OrgID == i_crm.ORIGID && it.ArgumentClassID == "Transport" && it.ArgumentID == sTransportationMode);
                                                        if (oArgument == null)
                                                        {
                                                            oArgument = new OTB_SYS_Arguments();
                                                        }
                                                        row_new["TransferFour_TransportationMode"] = oArgument.ArgumentValue ?? "";
                                                    }
                                                    row_new["TransferFour_GoodsType"] = jTransferFour["GoodsType"];
                                                    row_new["TransferFour_Destination"] = jTransferFour["Destination"];
                                                    row_new["TransferFour_InTransit_Date"] = ((JObject)jTransferFour["InTransit"])["Date"];
                                                    row_new["TransferFour_GoodsArrival_Date"] = ((JObject)jTransferFour["GoodsArrival"])["Date"];
                                                    row_new["TransferFour_CargoRelease_Date"] = ((JObject)jTransferFour["CargoRelease"])["Date"];
                                                    row_new["TransferFour_Sign_Date"] = ((JObject)jTransferFour["Sign"])["Date"];
                                                }
                                                if (jReImportFive != null)
                                                {
                                                    row_new["ReImportFive_CargoCollection_Date"] = ((JObject)jReImportFive["CargoCollection"])["Date"];
                                                    row_new["ReImportFive_Number"] = jReImportFive["Number"];
                                                    row_new["ReImportFive_Unit"] = jReImportFive["Unit"];
                                                    row_new["ReImportFive_FileValidation_Date"] = ((JObject)jReImportFive["FileValidation"])["Date"];
                                                    var sTransportationMode = jReImportFive["TransportationMode"] == null ? "" : jReImportFive["TransportationMode"].ToString();
                                                    if (sTransportationMode != "")
                                                    {
                                                        var oArgument = db.Queryable<OTB_SYS_Arguments>()
                                                            .Single(it => it.OrgID == i_crm.ORIGID && it.ArgumentClassID == "Transport" && it.ArgumentID == sTransportationMode);
                                                        if (oArgument == null)
                                                        {
                                                            oArgument = new OTB_SYS_Arguments();
                                                        }
                                                        row_new["ReImportFive_TransportationMode"] = oArgument.ArgumentValue ?? "";
                                                    }
                                                    row_new["ReImportFive_GoodsType"] = jReImportFive["GoodsType"];
                                                    row_new["ReImportFive_HuiYun_Date"] = ((JObject)jReImportFive["HuiYun"])["Date"];
                                                    row_new["ReImportFive_GoodsArrival_Date"] = ((JObject)jReImportFive["GoodsArrival"])["Date"];
                                                    row_new["ReImportFive_CustomsDeclaration_Date"] = ((JObject)jReImportFive["CustomsDeclaration"])["Date"];
                                                    row_new["ReImportFive_CargoRelease_Date"] = ((JObject)jReImportFive["CargoRelease"])["Date"];
                                                    row_new["ReImportFive_Sign_Date"] = ((JObject)jReImportFive["Sign"])["Date"];
                                                }
                                                if (jTransferFive != null)
                                                {
                                                    row_new["TransferFive_CargoCollection_Date"] = ((JObject)jTransferFive["CargoCollection"])["Date"];
                                                    row_new["TransferFive_Number"] = jTransferFive["Number"];
                                                    row_new["TransferFive_Unit"] = jTransferFive["Unit"];
                                                    row_new["TransferFive_FileValidation_Date"] = ((JObject)jTransferFive["FileValidation"])["Date"];
                                                    var sTransportationMode = jTransferFive["TransportationMode"] == null ? "" : jTransferFive["TransportationMode"].ToString();
                                                    if (sTransportationMode != "")
                                                    {
                                                        var oArgument = db.Queryable<OTB_SYS_Arguments>()
                                                            .Single(it => it.OrgID == i_crm.ORIGID && it.ArgumentClassID == "Transport" && it.ArgumentID == sTransportationMode);
                                                        if (oArgument == null)
                                                        {
                                                            oArgument = new OTB_SYS_Arguments();
                                                        }
                                                        row_new["TransferFive_TransportationMode"] = oArgument.ArgumentValue ?? "";
                                                    }
                                                    row_new["TransferFive_GoodsType"] = jTransferFive["GoodsType"];
                                                    row_new["TransferFive_Destination"] = jTransferFive["Destination"];
                                                    row_new["TransferFive_InTransit_Date"] = ((JObject)jTransferFive["InTransit"])["Date"];
                                                    row_new["TransferFive_GoodsArrival_Date"] = ((JObject)jTransferFive["GoodsArrival"])["Date"];
                                                    row_new["TransferFive_CargoRelease_Date"] = ((JObject)jTransferFive["CargoRelease"])["Date"];
                                                    row_new["TransferFive_Sign_Date"] = ((JObject)jTransferFive["Sign"])["Date"];
                                                }
                                                if (jReImportSix != null)
                                                {
                                                    row_new["ReImportSix_CargoCollection_Date"] = ((JObject)jReImportSix["CargoCollection"])["Date"];
                                                    row_new["ReImportSix_Number"] = jReImportSix["Number"];
                                                    row_new["ReImportSix_Unit"] = jReImportSix["Unit"];
                                                    row_new["ReImportSix_FileValidation_Date"] = ((JObject)jReImportSix["FileValidation"])["Date"];
                                                    var sTransportationMode = jReImportSix["TransportationMode"] == null ? "" : jReImportSix["TransportationMode"].ToString();
                                                    if (sTransportationMode != "")
                                                    {
                                                        var oArgument = db.Queryable<OTB_SYS_Arguments>()
                                                            .Single(it => it.OrgID == i_crm.ORIGID && it.ArgumentClassID == "Transport" && it.ArgumentID == sTransportationMode);
                                                        if (oArgument == null)
                                                        {
                                                            oArgument = new OTB_SYS_Arguments();
                                                        }
                                                        row_new["ReImportSix_TransportationMode"] = oArgument.ArgumentValue ?? "";
                                                    }
                                                    row_new["ReImportSix_GoodsType"] = jReImportSix["GoodsType"];
                                                    row_new["ReImportSix_HuiYun_Date"] = ((JObject)jReImportSix["HuiYun"])["Date"];
                                                    row_new["ReImportSix_GoodsArrival_Date"] = ((JObject)jReImportSix["GoodsArrival"])["Date"];
                                                    row_new["ReImportSix_CustomsDeclaration_Date"] = ((JObject)jReImportSix["CustomsDeclaration"])["Date"];
                                                    var sCustomsClearance = ((JObject)jReImportSix["CustomsDeclaration"])["CustomsClearance"].ToString();
                                                    if (sCustomsClearance != "")
                                                    {
                                                        var oArgument = db.Queryable<OTB_SYS_Arguments>()
                                                            .Single(it => it.OrgID == i_crm.ORIGID && it.ArgumentClassID == "Clearance" && it.ArgumentID == sCustomsClearance);
                                                        if (oArgument == null)
                                                        {
                                                            oArgument = new OTB_SYS_Arguments();
                                                        }
                                                        row_new["ReImportSix_CustomsDeclaration_CustomsClearance"] = oArgument.ArgumentValue ?? "";
                                                    }
                                                    row_new["ReImportSix_CargoRelease_Date"] = ((JObject)jReImportSix["CargoRelease"])["Date"];
                                                    row_new["ReImportSix_Sign_Date"] = ((JObject)jReImportSix["Sign"])["Date"];
                                                }
                                                if (jTransferSix != null)
                                                {
                                                    row_new["TransferSix_CargoCollection_Date"] = ((JObject)jTransferSix["CargoCollection"])["Date"];
                                                    row_new["TransferSix_Number"] = jTransferSix["Number"];
                                                    row_new["TransferSix_Unit"] = jTransferSix["Unit"];
                                                    row_new["TransferSix_FileValidation_Date"] = ((JObject)jTransferSix["FileValidation"])["Date"];
                                                    var sTransportationMode = jTransferSix["TransportationMode"] == null ? "" : jTransferSix["TransportationMode"].ToString();
                                                    if (sTransportationMode != "")
                                                    {
                                                        var oArgument = db.Queryable<OTB_SYS_Arguments>()
                                                            .Single(it => it.OrgID == i_crm.ORIGID && it.ArgumentClassID == "Transport" && it.ArgumentID == sTransportationMode);
                                                        if (oArgument == null)
                                                        {
                                                            oArgument = new OTB_SYS_Arguments();
                                                        }
                                                        row_new["TransferSi_TransportationMode"] = oArgument.ArgumentValue ?? "";
                                                    }
                                                    row_new["TransferSix_GoodsType"] = jTransferSix["GoodsType"];
                                                    row_new["TransferSix_Destination"] = jTransferSix["Destination"];
                                                    row_new["TransferSix_InTransit_Date"] = ((JObject)jTransferSix["InTransit"])["Date"];
                                                    row_new["TransferSix_GoodsArrival_Date"] = ((JObject)jTransferSix["GoodsArrival"])["Date"];
                                                    row_new["TransferSix_CargoRelease_Date"] = ((JObject)jTransferSix["CargoRelease"])["Date"];
                                                    row_new["TransferSix_Sign_Date"] = ((JObject)jTransferSix["Sign"])["Date"];
                                                }

                                                dt_new.Rows.Add(row_new);
                                                iFirstRow++;
                                            }
                                        }
                                        else
                                        {
                                            foreach (DataColumn col in dt.Columns)
                                            {
                                                row_new[col.ToString()] = row[col.ToString()];
                                            }
                                            dt_new.Rows.Add(row_new);
                                            iFirstRow++;
                                        }
                                    }

                                    oHeader_Last = Export_BusinessTrackingSchedule_List;
                                }
                                break;

                            default:
                                {
                                    break;
                                }
                        }
                        var bOk = new ExcelService().CreateExcelByTb(dt_new, out string sPath, oHeader_Last, dicAlain, listMerge, sFileName);

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
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBAPP.OPM.ImportExhibition_QryService", "", "QueryPage（出口分頁查詢）", "", "", "");
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

        #endregion 出口分頁查詢

        #region 出口單筆查詢

        /// <summary>
        /// 出口單筆查詢
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

                    var oExportExhibition = db.Queryable<OTB_OPM_ExportExhibition, OTB_CRM_Customers, OTB_OPM_Exhibition>
                        ((t1, t2, t3) =>
                        new object[] {
                                JoinType.Left, t1.OrgID == t2.OrgID && t1.Agent == t2.guid,
                                JoinType.Left, t1.OrgID == t3.OrgID && t1.ExhibitionNO == t3.SN.ToString()
                              }
                        )
                        .Where((t1, t2, t3) => t1.OrgID == i_crm.ORIGID && t1.ExportBillNO == sId)
                        .Select((t1, t2, t3) => new View_OPM_ExportExhibition
                        {
                            ExportBillNO = SqlFunc.GetSelfAndAutoFill(t1.ExportBillNO),
                            Exhibitioname_TW = t3.Exhibitioname_TW,
                            Exhibitioname_EN = t3.Exhibitioname_EN,
                            AgentName = SqlFunc.IIF(SqlFunc.HasValue(t2.CustomerCName), t2.CustomerCName, t2.CustomerEName)
                        }).Single();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, oExportExhibition);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBAPP.OPM.ImportExhibition_QryService", "", "QueryOne（出口單筆查詢）", "", "", "");
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

        #endregion 出口單筆查詢
    }
}