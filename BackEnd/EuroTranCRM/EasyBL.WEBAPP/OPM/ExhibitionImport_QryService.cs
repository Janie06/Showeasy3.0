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
using System.Drawing;

namespace EasyBL.WEBAPP.OPM
{
    public class ExhibitionImport_QryService : ServiceBase
    {
        #region 進口分頁查詢

        /// <summary>
        /// 進口分頁查詢
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
                    var sImportBillName = _fetchString(i_crm, @"ImportBillName");
                    var sBillNO = _fetchString(i_crm, @"BillNO");
                    var sExhibitionDateStart = _fetchString(i_crm, @"ExhibitionDateStart");
                    var sExhibitionDateEnd = _fetchString(i_crm, @"ExhibitionDateEnd");
                    var sAgent = _fetchString(i_crm, @"Agent");
                    var sResponsiblePerson = _fetchString(i_crm, @"ResponsiblePerson");
                    var sImportPerson = _fetchString(i_crm, @"ImportPerson");
                    var sDeclarationClass = _fetchString(i_crm, @"DeclarationClass");
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

                    pml.DataList = db.Queryable<OTB_OPM_ImportExhibition, OTB_SYS_Members, OTB_OPM_Exhibition, OTB_CRM_Customers, OTB_CRM_Customers>
                        ((t1, t2, t3, t7, t8) =>
                        new object[] {
                                JoinType.Inner, t1.OrgID == t2.OrgID && t1.ResponsiblePerson == t2.MemberID,
                                JoinType.Left, t1.ExhibitionNO == t3.SN.ToString(), //t1.OrgID == t3.OrgID
                                JoinType.Left, t1.Supplier == t7.guid, //t1.OrgID == t7.OrgID
                                JoinType.Left, t1.Agent == t8.guid //t1.OrgID == t8.OrgID
                              }
                        )
                        .Where((t1) => t1.OrgID == i_crm.ORIGID)
                        .WhereIF(!string.IsNullOrEmpty(sRefNumber), (t1) => t1.RefNumber.Contains(sRefNumber))
                        .WhereIF(!string.IsNullOrEmpty(sImportBillName), (t1, t2, t3) => (t3.Exhibitioname_TW + t3.Exhibitioname_EN + t3.ExhibitioShotName_TW).Contains(sImportBillName))
                        .WhereIF(!string.IsNullOrEmpty(sResponsiblePerson), (t1) => t1.ResponsiblePerson == sResponsiblePerson)
                        .WhereIF(!string.IsNullOrEmpty(sImportPerson), (t1) => t1.ImportPerson.Contains(sImportPerson))
                        .WhereIF(!string.IsNullOrEmpty(sDeclarationClass), (t1) => t1.DeclarationClass == sDeclarationClass)
                        .WhereIF(!string.IsNullOrEmpty(sAgent), (t1, t2, t3, t7, t8) => (t8.CustomerCName + t8.CustomerEName).Contains(sAgent))
                        .WhereIF(!string.IsNullOrEmpty(sSupplier), (t1, t2, t3, t7) => (t7.CustomerCName + t7.CustomerEName).Contains(sSupplier))
                        .WhereIF(!string.IsNullOrEmpty(sBillLadNO), (t1) => (t1.BillLadNO + t1.ReImports).Contains(sBillLadNO))
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
                        .Select((t1, t2, t3, t7, t8) => new View_OPM_ImportExhibition
                        {
                            ImportBillNO = t1.ImportBillNO,
                            RefNumber = t1.RefNumber,
                            ImportBillName = t3.Exhibitioname_TW,
                            ExhibitionDateStart = t1.ExhibitionDateStart,
                            BillLadNO = t1.BillLadNO,
                            IsVoid = t1.IsVoid,
                            ReImports = t1.ReImports,
                            ReturnBills = SqlFunc.IsNull(t1.ReturnBills, "[]"),
                            REF = t1.REF,
                            Flow_Status = t1.Flow_Status,
                            SupplierCName = SqlFunc.IIF(SqlFunc.HasValue(t7.CustomerCName), t7.CustomerCName, t7.CustomerEName),
                            AgentName = SqlFunc.IIF(SqlFunc.HasValue(t8.CustomerCName), t8.CustomerCName, t8.CustomerEName),
                            ResponsiblePersonName = t2.MemberName,
                            IsAlert = SqlFunc.MappingColumn(t1.ImportBillNO, "[dbo].[OFN_OPM_CheckDate](t1.Release,t1.ExhibitionDateStart,5)"),
                            Bills = SqlFunc.IsNull(t1.Bills, "[]"),
                            IsSendMail = t1.IsSendMail
                        })
                        .MergeTable()
                        .OrderByIF(string.IsNullOrEmpty(sSortField), "IsAlert desc,ExhibitionDateStart desc,ImportBillName,RefNumber desc")
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
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBAPP.OPM.ExhibitionImport_QryService", "", "QueryPage（進口分頁查詢）", "", "", "");
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

        #endregion 進口分頁查詢

        #region 進口分頁查詢

        /// <summary>
        /// 進口分頁查詢
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
                    var sImportBillName = _fetchString(i_crm, @"ImportBillName");
                    var sBillNO = _fetchString(i_crm, @"BillNO");
                    var sExhibitionDateStart = _fetchString(i_crm, @"ExhibitionDateStart");
                    var sExhibitionDateEnd = _fetchString(i_crm, @"ExhibitionDateEnd");
                    var sAgent = _fetchString(i_crm, @"Agent");
                    var sResponsiblePerson = _fetchString(i_crm, @"ResponsiblePerson");
                    var sImportPerson = _fetchString(i_crm, @"ImportPerson");
                    var sDeclarationClass = _fetchString(i_crm, @"DeclarationClass");
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

                    pml.DataList = db.Queryable<OTB_OPM_ImportExhibition, OTB_SYS_Members, OTB_OPM_Exhibition, OTB_SYS_Arguments, OTB_SYS_Arguments, OTB_SYS_Arguments, OTB_CRM_Customers, OTB_CRM_Customers, OTB_CRM_Customers>
                        ((t1, t2, t3, t4, t5, t6, t7, t8, t9) =>
                        new object[] {
                                JoinType.Inner, t1.OrgID == t2.OrgID && t1.ResponsiblePerson == t2.MemberID,
                                JoinType.Left, t1.ExhibitionNO == t3.SN.ToString(), //t1.OrgID == t3.OrgID
                                JoinType.Left, t1.OrgID == t4.OrgID && t1.DeclarationClass == t4.ArgumentID && t4.ArgumentClassID == "DeclClass",
                                JoinType.Left, t1.OrgID == t5.OrgID && t1.TransportationMode == t5.ArgumentID && t5.ArgumentClassID == "Transport",
                                JoinType.Left, t1.OrgID == t6.OrgID && t1.CustomsClearance == t6.ArgumentID && t6.ArgumentClassID == "Clearance",
                                JoinType.Left, t1.Supplier == t7.guid, //t1.OrgID == t7.OrgID
                                JoinType.Left, t1.Agent == t8.guid, //t1.OrgID == t8.OrgID
                                JoinType.Left, t1.ImportPerson == t9.guid //t1.OrgID == t9.OrgID
                              }
                        )
                        .Where((t1) => t1.OrgID == i_crm.ORIGID)
                        .WhereIF(!string.IsNullOrEmpty(sRefNumber), (t1) => t1.RefNumber.Contains(sRefNumber))
                        .WhereIF(!string.IsNullOrEmpty(sImportBillName), (t1, t2, t3) => (t3.Exhibitioname_TW + t3.Exhibitioname_EN + t3.ExhibitioShotName_TW).Contains(sImportBillName))
                        .WhereIF(!string.IsNullOrEmpty(sResponsiblePerson), (t1) => t1.ResponsiblePerson == sResponsiblePerson)
                        .WhereIF(!string.IsNullOrEmpty(sImportPerson), (t1) => t1.ImportPerson.Contains(sImportPerson))
                        .WhereIF(!string.IsNullOrEmpty(sDeclarationClass), (t1) => t1.DeclarationClass == sDeclarationClass)
                        .WhereIF(!string.IsNullOrEmpty(sAgent), (t1, t2, t3, t4, t5, t6, t7, t8) => (t8.CustomerCName + t8.CustomerEName).Contains(sAgent))
                        .WhereIF(!string.IsNullOrEmpty(sSupplier), (t1, t2, t3, t4, t5, t6, t7) => (t7.CustomerCName + t7.CustomerEName).Contains(sSupplier))
                        .WhereIF(!string.IsNullOrEmpty(sBillLadNO), (t1) => (t1.BillLadNO + t1.ReImports).Contains(sBillLadNO))
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
                        .Select((t1, t2, t3, t4, t5, t6, t7, t8, t9) => new View_OPM_ImportExhibition
                        {
                            OrgID = t1.OrgID,
                            ImportBillNO = t1.ImportBillNO,
                            ExhibitionNO = t1.ExhibitionNO,
                            RefNumber = t1.RefNumber,
                            ImportPerson = t1.ImportPerson,
                            ImportDeclarationNO = t1.ImportDeclarationNO,
                            ExportBillNO = t1.ExportBillNO,
                            ExportDeclarationNO = t1.ExportDeclarationNO,
                            ImportBillName = t3.Exhibitioname_TW,
                            ImportBillEName = t3.Exhibitioname_EN,
                            ExhibitioShotName_TW = t3.ExhibitioShotName_TW,
                            ExhibitionDateStart = t1.ExhibitionDateStart,
                            ExhibitionDateEnd = t1.ExhibitionDateEnd,
                            Hall = t1.Hall,
                            MuseumMumber = t1.MuseumMumber,
                            DeclarationClass = t1.DeclarationClass,
                            ResponsiblePerson = t1.ResponsiblePerson,
                            Agent = t1.Agent,
                            Payer = t1.Payer,
                            Supplier = t1.Supplier,
                            Suppliers = t1.Suppliers,
                            ContactorName = t1.ContactorName,
                            BillLadNO = t1.BillLadNO,
                            BillLadNOSub = t1.BillLadNOSub,
                            ContainerNumber = t1.ContainerNumber,
                            Telephone = t1.Telephone,
                            ShipmentPort = t1.ShipmentPort,
                            DestinationPort = t1.DestinationPort,
                            _ArrivalTime = SqlFunc.MappingColumn(t1.ImportBillNO, "CONVERT(VARCHAR(16),ArrivalTime, 120)"),
                            StoragePlace = t1.StoragePlace,
                            BoxNo = t1.BoxNo,
                            Unit = t1.Unit,
                            Weight = t1.Weight,
                            Size = t1.Size,
                            Volume = t1.Volume,
                            VolumeWeight = t1.VolumeWeight,
                            Price = t1.Price,
                            GoodsType = t1.GoodsType,
                            Currency = t1.Currency,
                            ExchangeRate = t1.ExchangeRate,
                            _FreePeriod = SqlFunc.MappingColumn(t1.ImportBillNO, "CONVERT(VARCHAR(16),FreePeriod, 23)"),
                            Release = t1.Release,
                            SitiContactor = t1.SitiContactor,
                            SitiTelephone = t1.SitiTelephone,
                            _ApproachTime = SqlFunc.MappingColumn(t1.ImportBillNO, "CONVERT(VARCHAR(16),ApproachTime, 120)"),
                            _ExitTime = SqlFunc.MappingColumn(t1.ImportBillNO, "CONVERT(VARCHAR(16),ExitTime, 120)"),
                            TransportationMode = t1.TransportationMode,
                            CustomsClearance = t1.CustomsClearance,
                            SignatureFileId = t1.SignatureFileId,
                            Memo = t1.Memo,
                            IsVoid = t1.IsVoid,
                            Import = t1.Import,
                            ReImports = t1.ReImports,
                            ReturnBills = SqlFunc.IsNull(t1.ReturnBills, "[]"),
                            CostData = t1.CostData,
                            ReturnLoan = t1.ReturnLoan,
                            TaxInformation = t1.TaxInformation,
                            REF = t1.REF,
                            Flow_Status = t1.Flow_Status,
                            ExhibitionDate = SqlFunc.MappingColumn(t1.ImportBillNO, "CONVERT(varchar(100),t1.ExhibitionDateStart, 111)+'~'+CONVERT(VARCHAR(100),t1.ExhibitionDateEnd, 111)"),
                            SupplierCName = SqlFunc.IIF(SqlFunc.HasValue(t7.CustomerCName), t7.CustomerCName, t7.CustomerEName),
                            SupplierEName = t7.CustomerEName,
                            AgentName = SqlFunc.IIF(SqlFunc.HasValue(t8.CustomerCName), t8.CustomerCName, t8.CustomerEName),
                            ResponsiblePersonName = t2.MemberName,
                            DeclarationClassName = t4.ArgumentValue,
                            TransportationModeName = t5.ArgumentValue,
                            CustomsClearanceName = t6.ArgumentValue,
                            IsAlert = SqlFunc.MappingColumn(t1.ImportBillNO, "[dbo].[OFN_OPM_CheckDate](t1.Release,t1.ExhibitionDateStart,5)"),
                            DepartmentID = t1.DepartmentID,
                            Bills = SqlFunc.IsNull(t1.Bills, "[]"),
                            ImportPersonName = SqlFunc.IsNull(t9.CustomerCName, t9.CustomerEName),
                            SupplierType = t1.SupplierType,
                            IsSendMail = t1.IsSendMail
                        })
                        .MergeTable()
                        .OrderByIF(string.IsNullOrEmpty(sSortField), "IsAlert desc,ExhibitionDateStart desc,ImportBillName,RefNumber desc")
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
                        var Export_BusinessTrackingSchedule_List = new List<Dictionary<string, object>>();
                        var Export_BusinessTrackingSchedule_Group = new Dictionary<string, object>();
                        var saCustomers1 = pml.DataList;
                        var saBasicInformation = pml.DataList as List<View_OPM_ImportExhibition>;
                        var dt_new = saBasicInformation.ListToDataTable<View_OPM_ImportExhibition>();
                        switch (sExcelType)
                        {
                            case "Import_BasicInformation":
                                {
                                    sFileName = "出口基本資料";
                                    oHeader = new Dictionary<string, string>
                                                {
                                                    { "RowIndex", "項次" },
                                                    { "RefNumber", "查詢號碼" },
                                                    { "ImportBillNO", "帳單號碼" },
                                                    { "ExportBillNO", "退運帳單號碼" },
                                                    { "ResponsiblePersonName", "負責業務" },
                                                    { "ImportBillName", "展覽名稱" },
                                                    { "ImportBillEName", "英文展名" },
                                                    { "ExhibitionDate", "展覽日期起訖" },
                                                    { "Hall", "館別" },
                                                    { "MuseumMumber", "攤位號碼" },
                                                    { "SupplierCName", "參展廠商" },
                                                    { "AgentName", "國外代理" },
                                                    { "DeclarationClassName", "報關類別" },
                                                    { "ImportDeclarationNO", "進口報單號碼" },
                                                    { "ContainerNumber", "貨櫃號碼" },
                                                    { "Payer", "付款人" },
                                                    { "BillLadNO", "提單號碼" },
                                                    { "BillLadNOSub", "分提單號碼" },
                                                    { "BoxNo", "件數" },
                                                    { "Unit", "單位" },
                                                    { "Weight", "毛重(G.W.)" },
                                                    { "Price", "貨價" },
                                                    { "ShipmentPort", "起運地" },
                                                    { "DestinationPort", "目的地" },
                                                    { "_ArrivalTime", "預計抵達時間" },
                                                    { "StoragePlace", "貨物儲放倉庫" },
                                                    { "GoodsType", "貨型" },
                                                    { "Size", "尺寸" },
                                                    { "Volume", "材積重(CBM)" },
                                                    { "VolumeWeight", "材積重(C.W.)" },
                                                    { "_FreePeriod", "免費期" },
                                                    { "TransportationModeName", "運送方式" },
                                                    { "CustomsClearanceName", "通關方式" },
                                                    { "SitiContactor", "現場聯絡人" },
                                                    { "SitiTelephone", "現場聯絡電話" },
                                                    { "_ApproachTime", "進場時間" },
                                                    { "_ExitTime", "退場時間" },
                                                    { "Memo", "特別注意事項" }
                                                };

                                    var list_Halls1 = db.Queryable<OTB_SYS_Arguments>()
                                        .Where(it => it.OrgID == i_crm.ORIGID && it.ArgumentClassID == "Hall").ToList();
                                    var m = new Map();
                                    foreach (OTB_SYS_Arguments arg in list_Halls1)
                                    {
                                        m.Put(arg.ArgumentID, arg.ArgumentValue);
                                    }
                                    foreach (DataRow row in dt_new.Rows)
                                    {
                                        row["Hall"] = m[row["Hall"].ToString()] ?? "";
                                        var sBills = "";
                                        var sReBills = "";
                                        var JaBills = (JArray)JsonConvert.DeserializeObject(row["Bills"].ToString());
                                        var JaReturns = (JArray)JsonConvert.DeserializeObject(row["ReturnBills"].ToString());
                                        var builder = new System.Text.StringBuilder();
                                        builder.Append(sBills);
                                        foreach (JObject bill in JaBills)
                                        {
                                            builder.Append(bill["BillNO"] + "，");
                                        }
                                        sBills = builder.ToString();
                                        foreach (JObject rebill in JaReturns)
                                        {
                                            if (rebill["Bills"] != null)
                                            {
                                                var JReBills = (JArray)JsonConvert.DeserializeObject(rebill["Bills"].ToString());
                                                var builder1 = new System.Text.StringBuilder();
                                                builder1.Append(sReBills);
                                                foreach (JObject bill in JReBills)
                                                {
                                                    builder1.Append(bill["BillNO"] + "，");
                                                }
                                                sReBills = builder1.ToString();
                                            }
                                        }
                                        row["ImportBillNO"] = sBills;
                                        row["ExportBillNO"] = sReBills;
                                    }
                                    dicAlain = ExcelService.GetExportAlain(oHeader, "RefNumber,_ClosingDate,_ClosingDate,_ArrivalTime,_ExitTime,_FreePeriod,_ApproachTime");
                                    oHeader_Last = oHeader;
                                }
                                break;

                            case "Import_BusinessTrackingSchedule":
                                {
                                    var Import_BusinessTrackingSchedule_List = new List<Dictionary<string, object>>();
                                    var Import_BusinessTrackingSchedule_Group = new Dictionary<string, object>();
                                    sFileName = "進口業務追蹤進度表";
                                    oHeader = new Dictionary<string, string>
                                                {
                                                    { "RowIndex", "項次" },
                                                    { "RefNumber", "查詢號碼" },
                                                    { "ImportBillNO", "帳單號碼" },
                                                    { "ExportBillNO", "退運帳單號碼" },
                                                    { "ResponsiblePersonName", "負責業務" },
                                                    { "ImportBillName", "展覽名稱" },
                                                    { "ImportBillEName", "英文展名" },
                                                    { "ExhibitionDate", "展覽日期起訖" },
                                                    { "Hall", "館別" },
                                                    { "MuseumMumber", "攤位號碼" },
                                                    { "SupplierCName", "參展廠商" },
                                                    { "AgentName", "國外代理" },
                                                    { "DeclarationClassName", "報關類別" },
                                                    { "ImportDeclarationNO", "進口報單號碼" },
                                                    { "ContainerNumber", "貨櫃號碼" },
                                                    { "Payer", "付款人" },
                                                    { "BillLadNO", "提單號碼" },
                                                    { "BillLadNOSub", "分提單號碼" },
                                                    { "BoxNo", "件數" },
                                                    { "Unit", "單位" },
                                                    { "Weight", "毛重(G.W.)" },
                                                    { "ShipmentPort", "起運地" },
                                                    { "DestinationPort", "目的地" },
                                                    { "ArrivalTime", "預計抵達時間" },
                                                    { "StoragePlace", "貨物儲放倉庫" },
                                                    { "GoodsType", "貨型" },
                                                    { "Size", "尺寸" },
                                                    { "Volume", "材積(CBM)" },
                                                    { "VolumeWeight", "材積重(C.W.)" },
                                                    { "Price", "貨價" },
                                                    { "FreePeriod", "免費期" },
                                                    { "TransportationModeName", "運送方式" },
                                                    { "CustomsClearanceName", "通關方式" },
                                                    { "SitiContactor", "現場聯絡人" },
                                                    { "SitiTelephone", "現場聯絡電話" },
                                                    { "_ApproachTime", "進場時間" },
                                                    { "_ExitTime", "退場時間" },
                                                    { "Memo", "特別注意事項" }
                                                };
                                    dicAlain = ExcelService.GetExportAlain(oHeader, new string[] { "Email", "Contactor", "ContactorEmail" });

                                    Import_BusinessTrackingSchedule_Group.Add("Header", oHeader);
                                    Import_BusinessTrackingSchedule_Group.Add("HeaderName", "基本資料");
                                    Import_BusinessTrackingSchedule_Group.Add("Color", Color.Chocolate);
                                    Import_BusinessTrackingSchedule_List.Add(Import_BusinessTrackingSchedule_Group);
                                    oHeader = new Dictionary<string, string>();
                                    Import_BusinessTrackingSchedule_Group = new Dictionary<string, object>();

                                    oHeader.Add("Import_ReceiveFile_Date", "已收文件");
                                    oHeader.Add("Import_GoodsArrival_Date", "貨物抵達");
                                    oHeader.Add("Import_CustomsDeclaration_Date", "報關作業");
                                    oHeader.Add("Import_CargoRelease_Date", "貨物放行");
                                    oHeader.Add("Import_ExhibitionWarehouse_Date", "轉至展館倉庫");
                                    oHeader.Add("Import_Sign_Date", "客戶簽收");

                                    Import_BusinessTrackingSchedule_Group.Add("Header", oHeader);
                                    Import_BusinessTrackingSchedule_Group.Add("HeaderName", "貨物進口");
                                    Import_BusinessTrackingSchedule_Group.Add("Color", Color.Aqua);
                                    Import_BusinessTrackingSchedule_List.Add(Import_BusinessTrackingSchedule_Group);
                                    oHeader = new Dictionary<string, string>();
                                    Import_BusinessTrackingSchedule_Group = new Dictionary<string, object>();

                                    var list_Halls2 = db.Queryable<OTB_SYS_Arguments>()
                                        .Where(it => it.OrgID == i_crm.ORIGID && it.ArgumentClassID == "Hall").ToList();
                                    var mH = new Map();
                                    foreach (OTB_SYS_Arguments arg in list_Halls2)
                                    {
                                        mH.Put(arg.ArgumentID, arg.ArgumentValue);
                                    }
                                    var ja = new JArray();
                                    foreach (DataRow row in dt_new.Rows)
                                    {
                                        var jaReImports = (JArray)JsonConvert.DeserializeObject(row["ReImports"].ToString());
                                        if (jaReImports.Count > ja.Count)
                                        {
                                            ja = jaReImports;
                                        }
                                        row["Hall"] = mH[row["Hall"].ToString()] ?? "";
                                        var sBills = "";
                                        var sReBills = "";
                                        var JaBills = (JArray)JsonConvert.DeserializeObject(row["Bills"].ToString());
                                        var JaReturns = (JArray)JsonConvert.DeserializeObject(row["ReturnBills"].ToString());
                                        var builder = new System.Text.StringBuilder();
                                        builder.Append(sBills);
                                        foreach (JObject bill in JaBills)
                                        {
                                            builder.Append(bill["BillNO"] + "，");
                                        }
                                        sBills = builder.ToString();
                                        foreach (JObject rebill in JaReturns)
                                        {
                                            if (rebill["Bills"] != null)
                                            {
                                                var JReBills = (JArray)JsonConvert.DeserializeObject(rebill["Bills"].ToString());
                                                var builder1 = new System.Text.StringBuilder();
                                                builder1.Append(sReBills);
                                                foreach (JObject bill in JReBills)
                                                {
                                                    builder1.Append(bill["BillNO"] + "，");
                                                }
                                                sReBills = builder1.ToString();
                                            }
                                        }
                                        row["ImportBillNO"] = sBills;
                                        row["ExportBillNO"] = sReBills;
                                    }
                                    for (var idx = 1; idx <= ja.Count; idx++)
                                    {
                                        oHeader.Add("ReImport" + idx + "_FileValidation_Date", "文件確認");
                                        oHeader.Add("ReImport" + idx + "_ReCustomsDeclaration_Date", "報關作業");
                                        oHeader.Add("ReImport" + idx + "_ReCargoRelease_Date", "貨物放行");
                                        oHeader.Add("ReImport" + idx + "_HuiYun_Date", "回運中");
                                        oHeader.Add("ReImport" + idx + "_ReachDestination_Date", "抵達目的地");
                                        oHeader.Add("ReImport" + idx + "_Sign_Date", "客戶簽收");

                                        Import_BusinessTrackingSchedule_Group.Add("Header", oHeader);
                                        Import_BusinessTrackingSchedule_Group.Add("HeaderName", "退運資料" + idx);
                                        Import_BusinessTrackingSchedule_Group.Add("Color", Color.Yellow);
                                        Import_BusinessTrackingSchedule_List.Add(Import_BusinessTrackingSchedule_Group);
                                        oHeader = new Dictionary<string, string>();
                                        Import_BusinessTrackingSchedule_Group = new Dictionary<string, object>();
                                        dt_new.Columns.Add("ReImport" + idx + "_FileValidation_Date");
                                        dt_new.Columns.Add("ReImport" + idx + "_ReCustomsDeclaration_Date");
                                        dt_new.Columns.Add("ReImport" + idx + "_ReCargoRelease_Date");
                                        dt_new.Columns.Add("ReImport" + idx + "_HuiYun_Date");
                                        dt_new.Columns.Add("ReImport" + idx + "_ReachDestination_Date");
                                        dt_new.Columns.Add("ReImport" + idx + "_Sign_Date");
                                    }

                                    dt_new.Columns.Add("Import_ReceiveFile_Date");
                                    dt_new.Columns.Add("Import_GoodsArrival_Date");
                                    dt_new.Columns.Add("Import_CustomsDeclaration_Date");
                                    dt_new.Columns.Add("Import_CargoRelease_Date");
                                    dt_new.Columns.Add("Import_ExhibitionWarehouse_Date");
                                    dt_new.Columns.Add("Import_Sign_Date");

                                    foreach (DataRow row in dt_new.Rows)
                                    {
                                        var jImport = (JObject)JsonConvert.DeserializeObject(row["Import"].ToString());
                                        var jaReImports = (JArray)JsonConvert.DeserializeObject(row["ReImports"].ToString());
                                        if (jImport != null)
                                        {
                                            row["Import_ReceiveFile_Date"] = ((JObject)jImport["ReceiveFile"])["Date"];
                                            row["Import_GoodsArrival_Date"] = ((JObject)jImport["GoodsArrival"])["Date"];
                                            row["Import_CustomsDeclaration_Date"] = ((JObject)jImport["CustomsDeclaration"])["Date"];
                                            row["Import_CargoRelease_Date"] = ((JObject)jImport["CargoRelease"])["Date"];
                                            row["Import_ExhibitionWarehouse_Date"] = jImport["ExhibitionWarehouse"] == null ? "" : ((JObject)jImport["ExhibitionWarehouse"])["Date"];
                                            row["Import_Sign_Date"] = ((JObject)jImport["Sign"])["Date"];
                                        };
                                        if (jaReImports != null && jaReImports.Count > 0)
                                        {
                                            var iIndex = 1;
                                            foreach (JObject jo in jaReImports)
                                            {
                                                var jFlow = (JObject)jo["ReImport"];
                                                row["ReImport" + iIndex + "_FileValidation_Date"] = ((JObject)jFlow["FileValidation"])["Date"];
                                                row["ReImport" + iIndex + "_ReCustomsDeclaration_Date"] = ((JObject)jFlow["ReCustomsDeclaration"])["Date"];
                                                row["ReImport" + iIndex + "_ReCargoRelease_Date"] = ((JObject)jFlow["ReCargoRelease"])["Date"];
                                                row["ReImport" + iIndex + "_HuiYun_Date"] = ((JObject)jFlow["HuiYun"])["Date"];
                                                row["ReImport" + iIndex + "_ReachDestination_Date"] = ((JObject)jFlow["ReachDestination"])["Date"];
                                                row["ReImport" + iIndex + "_Sign_Date"] = ((JObject)jFlow["Sign"])["Date"];
                                                iIndex++;
                                            }
                                        };
                                    }
                                    oHeader_Last = Import_BusinessTrackingSchedule_List;
                                }
                                break;

                            case "Import_ReturnRecord":
                                sFileName = "進口退運記錄表";
                                var Import_ReturnRecordHeader_List = new List<Dictionary<string, object>>();
                                var Import_ReturnRecordHeader_Group = new Dictionary<string, object>();
                                oHeader = new Dictionary<string, string>
                                                {
                                                    { "RowIndex", "項次" },
                                                    { "ImportBillName", "展覽名稱" },
                                                    { "ImportBillEName", "英文展名" },
                                                    { "SupplierCName", "參展廠商" },
                                                    { "ImportDeclarationNO", "進口報單號碼" },
                                                    { "ExportBillNO", "退運出口帳單號碼" },
                                                    { "DeclarationClassName", "報關類別" },
                                                    { "ShipmentPort", "起運地" },
                                                    { "BoxNo", "件數" },
                                                    { "Unit", "單位" },
                                                    { "Weight", "毛重(G.W.)" },
                                                    { "Volume", "材積(CBM)" },
                                                    { "VolumeWeight", "材積重(C.W.)" }
                                                };

                                Import_ReturnRecordHeader_Group.Add("Header", oHeader);
                                Import_ReturnRecordHeader_Group.Add("HeaderName", "基本資料");
                                Import_ReturnRecordHeader_Group.Add("Color", Color.Chocolate);
                                Import_ReturnRecordHeader_List.Add(Import_ReturnRecordHeader_Group);
                                oHeader = new Dictionary<string, string>();
                                Import_ReturnRecordHeader_Group = new Dictionary<string, object>();

                                var jaRe = new JArray();
                                foreach (DataRow row in dt_new.Rows)
                                {
                                    var jaReImports = (JArray)JsonConvert.DeserializeObject(row["ReImports"].ToString());
                                    if (jaReImports.Count > jaRe.Count)
                                    {
                                        jaRe = jaReImports;
                                    }
                                }
                                for (var idx = 1; idx <= jaRe.Count; idx++)
                                {
                                    oHeader.Add("ReImport" + idx + "_ReturnOrNotItems", "回運/不回運項目");
                                    oHeader.Add("ReImport" + idx + "_EnteringData", "進倉資料");
                                    oHeader.Add("ReImport" + idx + "_ReShipmentPort", "目的地");
                                    oHeader.Add("ReImport" + idx + "_ShippingAgency", "船代");
                                    oHeader.Add("ReImport" + idx + "_TransportationMode", "運送方式");
                                    oHeader.Add("ReImport" + idx + "_ReturnNumber", "退運件數");
                                    oHeader.Add("ReImport" + idx + "_Unit", "單位");
                                    oHeader.Add("ReImport" + idx + "_HeavyTruckBack", "退運材重");
                                    oHeader.Add("ReImport" + idx + "_LadingBill", "S/O");
                                    oHeader.Add("ReImport" + idx + "_FlightInformation", "班機資料");
                                    oHeader.Add("ReImport" + idx + "_ShippingMarks", "嘜頭");
                                    oHeader.Add("ReImport" + idx + "_Memo", "備註");
                                    oHeader.Add("ReImport" + idx + "_ArrivalTime", "預計抵達時間");

                                    Import_ReturnRecordHeader_Group.Add("Header", oHeader);
                                    Import_ReturnRecordHeader_Group.Add("HeaderName", "退運資料" + idx);
                                    Import_ReturnRecordHeader_Group.Add("Color", Color.Yellow);
                                    Import_ReturnRecordHeader_List.Add(Import_ReturnRecordHeader_Group);
                                    oHeader = new Dictionary<string, string>();
                                    Import_ReturnRecordHeader_Group = new Dictionary<string, object>();
                                    dt_new.Columns.Add("ReImport" + idx + "_ReturnOrNotItems");
                                    dt_new.Columns.Add("ReImport" + idx + "_EnteringData");
                                    dt_new.Columns.Add("ReImport" + idx + "_ReShipmentPort");
                                    dt_new.Columns.Add("ReImport" + idx + "_ShippingAgency");
                                    dt_new.Columns.Add("ReImport" + idx + "_TransportationMode");
                                    dt_new.Columns.Add("ReImport" + idx + "_ReturnNumber");
                                    dt_new.Columns.Add("ReImport" + idx + "_Unit");
                                    dt_new.Columns.Add("ReImport" + idx + "_HeavyTruckBack");
                                    dt_new.Columns.Add("ReImport" + idx + "_LadingBill");
                                    dt_new.Columns.Add("ReImport" + idx + "_FlightInformation");
                                    dt_new.Columns.Add("ReImport" + idx + "_ShippingMarks");
                                    dt_new.Columns.Add("ReImport" + idx + "_Memo");
                                    dt_new.Columns.Add("ReImport" + idx + "_ArrivalTime");
                                }

                                foreach (DataRow row in dt_new.Rows)
                                {
                                    var jaReImports = (JArray)JsonConvert.DeserializeObject(row["ReImports"].ToString());
                                    if (jaReImports != null && jaReImports.Count > 0)
                                    {
                                        var iIndex = 1;
                                        foreach (JObject jo in jaReImports)
                                        {
                                            var jReImportData = (JObject)jo["ReImportData"];
                                            row["ReImport" + iIndex + "_ReturnOrNotItems"] = jReImportData["ReturnOrNotItems"];
                                            row["ReImport" + iIndex + "_EnteringData"] = jReImportData["EnteringData"];
                                            row["ReImport" + iIndex + "_ReShipmentPort"] = jReImportData["ReShipmentPort"];
                                            row["ReImport" + iIndex + "_ShippingAgency"] = jReImportData["ShippingAgency"];
                                            row["ReImport" + iIndex + "_ReShipmentPort"] = jReImportData["ReShipmentPort"];
                                            var sTransportationMode = jReImportData["TransportationMode"].ToString();
                                            if (sTransportationMode != "")
                                            {
                                                var oArgument = db.Queryable<OTB_SYS_Arguments>()
                                                    .Single(it => it.OrgID == i_crm.ORIGID && it.ArgumentClassID == "Transport" && it.ArgumentID == sTransportationMode);
                                                if (oArgument == null)
                                                {
                                                    oArgument = new OTB_SYS_Arguments();
                                                }
                                                row["ReImport" + iIndex + "_TransportationMode"] = oArgument.ArgumentValue ?? "";
                                            }
                                            row["ReImport" + iIndex + "_ReturnNumber"] = jReImportData["ReturnNumber"];
                                            row["ReImport" + iIndex + "_Unit"] = jReImportData["Unit"];
                                            row["ReImport" + iIndex + "_HeavyTruckBack"] = jReImportData["HeavyTruckBack"];
                                            row["ReImport" + iIndex + "_LadingBill"] = jReImportData["LadingBill"];
                                            row["ReImport" + iIndex + "_FlightInformation"] = jReImportData["FlightInformation"];
                                            row["ReImport" + iIndex + "_ShippingMarks"] = jReImportData["ShippingMarks"];
                                            row["ReImport" + iIndex + "_Memo"] = jReImportData["Memo"];
                                            row["ReImport" + iIndex + "_ArrivalTime"] = jReImportData["ArrivalTime"];
                                            iIndex++;
                                        }
                                    };
                                }
                                oHeader_Last = Import_ReturnRecordHeader_List;
                                break;

                            case "Import_AreasList":
                                sFileName = "進口退押款清冊";
                                oHeader_Last = new Dictionary<string, string>
                                                    {
                                                        { "RowIndex", "項次" },
                                                        { "ImportBillName", "展覽名稱" },
                                                        { "ImportBillEName", "英文展名" },
                                                        { "SupplierCName", "參展廠商" },
                                                        { "ImportDeclarationNO", "進口報單號碼" },
                                                        { "Import_CustomsDeclaration_Date", "進口報關日" },
                                                        { "ReturnLoan_Margin", "保證金金額" },
                                                        { "ReturnedData_ReturnOrNotItems", "回運項次" },
                                                        { "ReturnLoan_VerificationDate", "核銷日期" },
                                                        { "ReturnedData_ReImportDeclarationNO", "出口報單號碼" },
                                                        { "ReImport_ReCustomsDeclaration_Date", "出口報關日" },
                                                        { "ReImport_ReCargoRelease_Date", "出口日期" },
                                                        { "ReturnLoan_ToPayTaxes", "抵繳稅捐" },
                                                        { "ReturnLoan_RefundAmount", "已退款金額" },
                                                        { "ReturnLoan_ImportDate", "滙入日期" }
                                                    };

                                dt_new.Columns.Add("Import_CustomsDeclaration_Date");
                                dt_new.Columns.Add("ReturnLoan_Margin");
                                dt_new.Columns.Add("ReturnedData_ReturnOrNotItems");
                                dt_new.Columns.Add("ReturnedData_ReImportDeclarationNO");
                                dt_new.Columns.Add("ReImport_ReCustomsDeclaration_Date");
                                dt_new.Columns.Add("ReImport_ReCargoRelease_Date");
                                dt_new.Columns.Add("ReturnLoan_ToPayTaxes");
                                dt_new.Columns.Add("ReturnLoan_RefundAmount");
                                dt_new.Columns.Add("ReturnLoan_ImportDate");
                                dt_new.Columns.Add("ReturnLoan_VerificationDate");
                                foreach (DataRow row in dt_new.Rows)
                                {
                                    var jImport = (JObject)JsonConvert.DeserializeObject(row["Import"].ToString());
                                    var jaReImports = (JArray)JsonConvert.DeserializeObject(row["ReImports"].ToString());
                                    var jReturnLoan = (JObject)JsonConvert.DeserializeObject(row["ReturnLoan"].ToString());
                                    var jTaxInformation = (JObject)JsonConvert.DeserializeObject(row["TaxInformation"].ToString());

                                    if (jImport != null)
                                    {
                                        row["Import_CustomsDeclaration_Date"] = ((JObject)jImport["CustomsDeclaration"])["Date"];
                                    };
                                    if (jaReImports != null)
                                    {
                                        if (jaReImports != null && jaReImports.Count > 0)
                                        {
                                            var sReImport_ReCustomsDeclaration_Date = "";
                                            var sReImport_ReCargoRelease_Date = "";
                                            var sReturnedData_ReturnOrNotItems = "";
                                            var sReturnedData_ReImportDeclarationNO = "";
                                            var builder = new System.Text.StringBuilder();
                                            builder.Append(sReImport_ReCustomsDeclaration_Date);
                                            var builder1 = new System.Text.StringBuilder();
                                            builder1.Append(sReturnedData_ReturnOrNotItems);
                                            var builder2 = new System.Text.StringBuilder();
                                            builder2.Append(sReImport_ReCargoRelease_Date);
                                            var builder3 = new System.Text.StringBuilder();
                                            builder3.Append(sReturnedData_ReImportDeclarationNO);
                                            foreach (JObject jo in jaReImports)
                                            {
                                                var oReImport = (JObject)jo["ReImport"];
                                                var oReImportData = (JObject)jo["ReImportData"];
                                                builder.Append("[" + ((JObject)oReImport["ReCustomsDeclaration"])["Date"] + "] ");
                                                builder2.Append("[" + ((JObject)oReImport["ReCargoRelease"])["Date"] + "] ");
                                                builder1.Append(oReImportData["ReturnOrNotItems"] + " | ");
                                                builder3.Append(oReImportData["ReImportDeclarationNO"] + " | ");
                                            }
                                            sReturnedData_ReImportDeclarationNO = builder3.ToString();
                                            sReImport_ReCargoRelease_Date = builder2.ToString();
                                            sReturnedData_ReturnOrNotItems = builder1.ToString();
                                            sReImport_ReCustomsDeclaration_Date = builder.ToString();
                                            row["ReImport_ReCustomsDeclaration_Date"] = sReImport_ReCustomsDeclaration_Date;
                                            row["ReImport_ReCargoRelease_Date"] = sReImport_ReCargoRelease_Date;
                                            row["ReturnedData_ReturnOrNotItems"] = (sReturnedData_ReturnOrNotItems + "*").Replace("| *", "");
                                            row["ReturnedData_ReImportDeclarationNO"] = (sReturnedData_ReImportDeclarationNO + "*").Replace("| *", "");
                                        }
                                    };
                                    if (jReturnLoan != null)
                                    {
                                        row["ReturnLoan_Margin"] = "$ " + $"{double.Parse((jReturnLoan["Margin"] != null && jReturnLoan["Margin"].ToString() != "") ? jReturnLoan["Margin"].ToString() : "0"):N0}";
                                        row["ReturnLoan_ToPayTaxes"] = "$ " + $"{double.Parse((jReturnLoan["ToPayTaxes"] != null && jReturnLoan["ToPayTaxes"].ToString() != "") ? jReturnLoan["ToPayTaxes"].ToString() : "0"):N0}";
                                        row["ReturnLoan_RefundAmount"] = "$ " + $"{double.Parse((jReturnLoan["RefundAmount"] != null && jReturnLoan["RefundAmount"].ToString() != "") ? jReturnLoan["RefundAmount"].ToString() : "0"):N0}";
                                        row["ReturnLoan_ImportDate"] = jReturnLoan["ImportDate"];
                                        row["ReturnLoan_VerificationDate"] = jReturnLoan["VerificationDate"];
                                    };
                                }
                                dicAlain = ExcelService.GetExportAlain(oHeader, "RowIndex", "ReturnLoan_Margin,ReturnLoan_ToPayTaxes,ReturnLoan_RefundAmount");
                                break;

                            case "Import_BondedSheet":
                                sFileName = "保稅工作表";
                                var Import_BondedSheet_List = new List<Dictionary<string, object>>();
                                var Import_BondedSheet_Group = new Dictionary<string, object>();
                                oHeader = new Dictionary<string, string>
                                        {
                                            { "RowIndex", "項次" },
                                            { "TaxInformation_BondedWarehouseCode", "保稅倉庫代碼" },
                                            { "ImportBillName", "中文展名" },
                                            { "ImportBillEName", "英文展名" },
                                            { "SupplierCName", "展商" }
                                        };

                                Import_BondedSheet_Group.Add("Header", oHeader);
                                Import_BondedSheet_Group.Add("HeaderName", "基本資料");
                                Import_BondedSheet_Group.Add("Color", Color.Chocolate);
                                Import_BondedSheet_List.Add(Import_BondedSheet_Group);
                                oHeader = new Dictionary<string, string>();
                                Import_BondedSheet_Group = new Dictionary<string, object>();

                                oHeader.Add("ImportDeclarationNO", "報單號碼");
                                oHeader.Add("GoodsType", "貨型");
                                oHeader.Add("BoxNo", "件數");
                                oHeader.Add("Unit", "單位");
                                oHeader.Add("TaxInformation_ReleaseDate", "放行日期");
                                oHeader.Add("TaxInformation_JincangDate", "進倉日期");
                                oHeader.Add("TaxInformation_Customs", "海關");
                                oHeader.Add("TaxInformation_SendCar", "派車");
                                oHeader.Add("TaxInformation_WarehouseDate", "出倉日期");
                                oHeader.Add("TaxInformation_WarehouseCustoms", "海關");
                                oHeader.Add("TaxInformation_CustomsBill", "出倉單");
                                oHeader.Add("TaxInformation_StorageDate", "回儲日期");
                                oHeader.Add("TaxInformation_StorageDateCustoms", "海關");
                                oHeader.Add("TaxInformation_StorageDateBill", "回儲單");

                                Import_BondedSheet_Group.Add("Header", oHeader);
                                Import_BondedSheet_Group.Add("HeaderName", "D8");
                                Import_BondedSheet_Group.Add("Color", Color.Aqua);
                                Import_BondedSheet_List.Add(Import_BondedSheet_Group);
                                oHeader = new Dictionary<string, string>();
                                Import_BondedSheet_Group = new Dictionary<string, object>();

                                oHeader.Add("TaxInformation_D5OrderNumber", "報單號碼");
                                oHeader.Add("TaxInformation_D5Number", "件數");
                                oHeader.Add("TaxInformation_D5Unit", "單位");
                                oHeader.Add("TaxInformation_D5Customs", "海關");
                                oHeader.Add("TaxInformation_D5SendCar", "派車");
                                oHeader.Add("TaxInformation_D5WarehouseDate", "出倉日期");

                                Import_BondedSheet_Group.Add("Header", oHeader);
                                Import_BondedSheet_Group.Add("HeaderName", "D5");
                                Import_BondedSheet_Group.Add("Color", Color.HotPink);
                                Import_BondedSheet_List.Add(Import_BondedSheet_Group);
                                oHeader = new Dictionary<string, string>();
                                Import_BondedSheet_Group = new Dictionary<string, object>();

                                oHeader.Add("TaxInformation_D2OrderNumber", "報單號碼");
                                oHeader.Add("TaxInformation_Memo", "備註");
                                oHeader.Add("TaxInformation_DefImportedItems", "原進口項次");
                                oHeader.Add("TaxInformation_DeclarationDate", "報關傳輸日期");
                                oHeader.Add("TaxInformation_CustomsApplicationIn", "申請海關（驗）");
                                oHeader.Add("TaxInformation_CustomsApplicationOut", "申請海關（出）");
                                oHeader.Add("TaxInformation_SingleDate", "投單日期");
                                oHeader.Add("TaxInformation_InspectionDate", "驗貨日期");
                                oHeader.Add("TaxInformation_TaxDate", "繳稅日期");
                                oHeader.Add("TaxInformation_D2WarehouseDate", "出倉日期");

                                Import_BondedSheet_Group.Add("Header", oHeader);
                                Import_BondedSheet_Group.Add("HeaderName", "D2");
                                Import_BondedSheet_Group.Add("Color", Color.Yellow);
                                Import_BondedSheet_List.Add(Import_BondedSheet_Group);
                                oHeader = new Dictionary<string, string>();
                                Import_BondedSheet_Group = new Dictionary<string, object>();

                                dt_new.Columns.Add("TaxInformation_BondedWarehouseCode");
                                dt_new.Columns.Add("TaxInformation_ReleaseDate");
                                dt_new.Columns.Add("TaxInformation_JincangDate");
                                dt_new.Columns.Add("TaxInformation_Customs");
                                dt_new.Columns.Add("TaxInformation_SendCar");
                                dt_new.Columns.Add("TaxInformation_WarehouseDate");
                                dt_new.Columns.Add("TaxInformation_WarehouseCustoms");
                                dt_new.Columns.Add("TaxInformation_CustomsBill");
                                dt_new.Columns.Add("TaxInformation_StorageDate");
                                dt_new.Columns.Add("TaxInformation_StorageDateCustoms");
                                dt_new.Columns.Add("TaxInformation_StorageDateBill");

                                dt_new.Columns.Add("TaxInformation_D5OrderNumber");
                                dt_new.Columns.Add("TaxInformation_D5Number");
                                dt_new.Columns.Add("TaxInformation_D5Unit");
                                dt_new.Columns.Add("TaxInformation_D5Customs");
                                dt_new.Columns.Add("TaxInformation_D5SendCar");
                                dt_new.Columns.Add("TaxInformation_D5WarehouseDate");

                                dt_new.Columns.Add("TaxInformation_D2OrderNumber");
                                dt_new.Columns.Add("TaxInformation_Memo");
                                dt_new.Columns.Add("TaxInformation_DefImportedItems");
                                dt_new.Columns.Add("TaxInformation_DeclarationDate");
                                dt_new.Columns.Add("TaxInformation_CustomsApplicationIn");
                                dt_new.Columns.Add("TaxInformation_CustomsApplicationOut");
                                dt_new.Columns.Add("TaxInformation_SingleDate");
                                dt_new.Columns.Add("TaxInformation_InspectionDate");
                                dt_new.Columns.Add("TaxInformation_TaxDate");
                                dt_new.Columns.Add("TaxInformation_D2WarehouseDate");

                                foreach (DataRow row in dt_new.Rows)
                                {
                                    var jTaxInformation = (JObject)JsonConvert.DeserializeObject(row["TaxInformation"].ToString());

                                    if (jTaxInformation != null)
                                    {
                                        row["TaxInformation_BondedWarehouseCode"] = jTaxInformation["BondedWarehouseCode"];
                                        row["TaxInformation_ReleaseDate"] = jTaxInformation["ReleaseDate"];
                                        row["TaxInformation_JincangDate"] = jTaxInformation["JincangDate"];
                                        row["TaxInformation_Customs"] = jTaxInformation["Customs"];
                                        row["TaxInformation_SendCar"] = jTaxInformation["SendCar"];
                                        row["TaxInformation_WarehouseDate"] = jTaxInformation["WarehouseDate"];
                                        row["TaxInformation_WarehouseCustoms"] = jTaxInformation["WarehouseCustoms"];
                                        row["TaxInformation_CustomsBill"] = jTaxInformation["CustomsBill"] != null ? "√" : "";
                                        row["TaxInformation_StorageDate"] = jTaxInformation["StorageDate"];
                                        row["TaxInformation_StorageDateCustoms"] = jTaxInformation["StorageDateCustoms"];
                                        row["TaxInformation_StorageDateBill"] = jTaxInformation["StorageDateBill"] != null ? "√" : "";
                                        row["TaxInformation_D5OrderNumber"] = jTaxInformation["D5OrderNumber"];

                                        row["TaxInformation_D5OrderNumber"] = jTaxInformation["D5OrderNumber"];
                                        row["TaxInformation_D5Number"] = jTaxInformation["D5Number"];
                                        row["TaxInformation_D5Unit"] = jTaxInformation["D5Unit"];
                                        row["TaxInformation_D5Customs"] = jTaxInformation["D5Customs"];
                                        row["TaxInformation_D5SendCar"] = jTaxInformation["D5SendCar"];
                                        row["TaxInformation_D5WarehouseDate"] = jTaxInformation["D5WarehouseDate"];

                                        row["TaxInformation_D2OrderNumber"] = jTaxInformation["D2OrderNumber"];
                                        row["TaxInformation_Memo"] = jTaxInformation["Memo"];
                                        row["TaxInformation_DefImportedItems"] = jTaxInformation["DefImportedItems"];
                                        row["TaxInformation_DeclarationDate"] = jTaxInformation["DeclarationDate"];
                                        row["TaxInformation_CustomsApplicationIn"] = jTaxInformation["CustomsApplicationIn"];
                                        row["TaxInformation_CustomsApplicationOut"] = jTaxInformation["CustomsApplicationOut"];
                                        row["TaxInformation_InspectionDate"] = jTaxInformation["InspectionDate"];
                                        row["TaxInformation_TaxDate"] = jTaxInformation["TaxDate"];
                                        row["TaxInformation_D2WarehouseDate"] = jTaxInformation["D2WarehouseDate"];
                                    };
                                }
                                oHeader_Last = Import_BondedSheet_List;
                                break;

                            case "Import_AdvanceAndRetreatWorkSheet":
                                sFileName = "進口進退場工作表";
                                oHeader_Last = new Dictionary<string, string>
                                                {
                                                    { "RowIndex", "項次" },
                                                    { "ImportBillName", "展覽名稱" },
                                                    { "SupplierEName", "參展商英文名稱" },
                                                    { "SupplierCName", "參展商中文名稱" },
                                                    { "Hall", "館別" },
                                                    { "MuseumMumber", "攤位號碼" },
                                                    { "SitiContactor", "現場聯絡人" },
                                                    { "SitiTelephone", "現場聯絡電話" },
                                                    { "BillLadNO", "提單號碼" },
                                                    { "BoxNo", "件數" },
                                                    { "Unit", "單位" },
                                                    { "Weight", "毛重(G.W.)" },
                                                    { "Volume", "材積(CBM)" },
                                                    { "VolumeWeight", "材積重(C.W.)" },
                                                    { "_ApproachTime", "進場時間" },
                                                    { "_ExitTime", "退場時間" }
                                                };

                                var list_Halls3 = db.Queryable<OTB_SYS_Arguments>()
                                    .Where(it => it.OrgID == i_crm.ORIGID && it.ArgumentClassID == "Hall").ToList();
                                var mHall = new Map();
                                foreach (OTB_SYS_Arguments arg in list_Halls3)
                                {
                                    mHall.Put(arg.ArgumentID, arg.ArgumentValue);
                                }

                                var dt = saBasicInformation.ListToDataTable<View_OPM_ImportExhibition>();
                                dt.Clear();
                                var iRowIndex = 1;
                                foreach (DataRow row in dt_new.Rows)
                                {
                                    if (row["SupplierType"].ToString() == "S")
                                    {
                                        var row_new = dt.NewRow();
                                        row_new["RowIndex"] = iRowIndex;
                                        row_new["ImportBillName"] = row["ImportBillName"];
                                        row_new["SupplierEName"] = row["SupplierEName"];
                                        row_new["SupplierCName"] = row["SupplierCName"];
                                        row_new["Hall"] = mHall[row["Hall"].ToString()] ?? "";
                                        row_new["MuseumMumber"] = row["MuseumMumber"];
                                        row_new["SitiContactor"] = row["SitiContactor"];
                                        row_new["SitiTelephone"] = row["SitiTelephone"];
                                        row_new["BillLadNO"] = row["BillLadNO"];
                                        row_new["BoxNo"] = row["BoxNo"];
                                        row_new["Unit"] = row["Unit"];
                                        row_new["Weight"] = row["Weight"];
                                        row_new["Volume"] = row["Volume"];
                                        row_new["VolumeWeight"] = row["VolumeWeight"];
                                        row_new["_ApproachTime"] = row["_ApproachTime"];
                                        row_new["_ExitTime"] = row["_ExitTime"];
                                        dt.Rows.Add(row_new);
                                        iRowIndex++;
                                    }
                                    else
                                    {
                                        var jaSuppliers = (JArray)JsonConvert.DeserializeObject(row["Suppliers"].ToString());
                                        foreach (JObject jo in jaSuppliers)
                                        {
                                            var row_new = dt.NewRow();
                                            row_new["RowIndex"] = iRowIndex;
                                            row_new["ImportBillName"] = row["ImportBillName"];
                                            row_new["SupplierEName"] = jo["SupplierEName"];
                                            row_new["SupplierCName"] = jo["SupplierName"];
                                            row_new["Hall"] = mHall[jo["Hall"].ToString()] ?? "";
                                            row_new["MuseumMumber"] = jo["MuseumMumber"];
                                            row_new["SitiContactor"] = jo["ContactorName"];
                                            row_new["SitiTelephone"] = jo["Telephone"];
                                            row_new["BillLadNO"] = row["BillLadNO"];
                                            row_new["BoxNo"] = jo["BoxNo"];
                                            row_new["Unit"] = jo["Unit"];
                                            row_new["Weight"] = jo["Weight"];
                                            row_new["Volume"] = jo["Volume"];
                                            row_new["VolumeWeight"] = jo["VolumeWeight"];
                                            row_new["_ApproachTime"] = jo["ApproachTime"];
                                            row_new["_ExitTime"] = jo["ExitTime"];
                                            dt.Rows.Add(row_new);
                                            iRowIndex++;
                                        }
                                    }
                                }
                                dt_new = dt;
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
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBAPP.OPM.ExhibitionImport_QryService", "", "GetExcel（進口匯出）", "", "", "");
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

        #endregion 進口分頁查詢

        #region 進口單筆查詢

        /// <summary>
        /// 進口單筆查詢
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

                    var oImportExhibition = db.Queryable<OTB_OPM_ImportExhibition, OTB_OPM_Exhibition, OTB_CRM_Customers, OTB_CRM_Customers>
                        ((t1, t2, t3, t4) =>
                        new object[] {
                                JoinType.Inner, t1.ExhibitionNO == t2.SN.ToString(), //t1.OrgID == t2.OrgID
                                JoinType.Left, t1.Supplier == t3.guid, //t1.OrgID == t3.OrgID
                                JoinType.Left, t1.Agent == t4.guid //t1.OrgID == t4.OrgID
                              }
                        )
                        .Where((t1, t2, t3, t4) => t1.OrgID == i_crm.ORIGID && t1.ImportBillNO == sId)
                        .Select((t1, t2, t3, t4) => new View_OPM_ImportExhibition
                        {
                            ImportBillNO = SqlFunc.GetSelfAndAutoFill(t1.ImportBillNO),
                            Exhibitioname_TW = t2.Exhibitioname_TW,
                            Exhibitioname_EN = t2.Exhibitioname_EN,
                            SupplierCName = SqlFunc.IIF(SqlFunc.HasValue(t3.CustomerCName), t3.CustomerCName, t3.CustomerEName),
                            AgentName = SqlFunc.IIF(SqlFunc.HasValue(t4.CustomerCName), t4.CustomerCName, t4.CustomerEName)
                        }).Single();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, oImportExhibition);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBAPP.OPM.ExhibitionImport_QryService", "", "QueryOne（進口單筆查詢）", "", "", "");
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

        #endregion 進口單筆查詢
    }
}