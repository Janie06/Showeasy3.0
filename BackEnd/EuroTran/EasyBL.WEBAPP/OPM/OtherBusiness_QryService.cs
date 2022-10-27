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

namespace EasyBL.WEBAPP.OPM
{
    public class OtherBusiness_QryService : ServiceBase
    {
        #region 其他（分頁查詢）

        /// <summary>
        /// 其他（分頁查詢）
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

                    var sExhibitionName = _fetchString(i_crm, @"ExhibitionName");
                    var sBillNO = _fetchString(i_crm, @"BillNO");
                    var sAgent = _fetchString(i_crm, @"Agent");
                    var sResponsiblePerson = _fetchString(i_crm, @"ResponsiblePerson");
                    var sDeclarationClass = _fetchString(i_crm, @"DeclarationClass");
                    var sIsIncludeVoid = _fetchString(i_crm, @"IsIncludeVoid");
                    var sCustomer = _fetchString(i_crm, @"Customer");
                    var sDepartmentID = _fetchString(i_crm, @"DepartmentID");
                    var bExcel = _fetchBool(i_crm, @"Excel");
                    var sExcelType = _fetchString(i_crm, @"ExcelType");
                    string[] saIsIncludeVoid = null;
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

                    pml.DataList = db.Queryable<OTB_OPM_OtherExhibition, OTB_SYS_Members, OTB_OPM_Exhibition, OTB_SYS_Arguments, OTB_CRM_Customers, OTB_CRM_Customers>
                        ((t1, t2, t3, t4, t5, t6) =>
                        new object[] {
                                JoinType.Inner, t1.OrgID == t2.OrgID && t1.ResponsiblePerson == t2.MemberID,
                                JoinType.Left, t1.OrgID == t3.OrgID && t1.ExhibitionNO == t3.SN.ToString(),
                                JoinType.Left, t1.OrgID == t4.OrgID && t1.TransportationMode == t4.ArgumentID && t4.ArgumentClassID == "DeclClass",
                                JoinType.Left, t1.OrgID == t5.OrgID && t1.Supplier == t5.guid,
                                JoinType.Left, t1.OrgID == t6.OrgID && t1.Agent == t6.guid
                              }
                        )
                        .Where((t1) => t1.OrgID == i_crm.ORIGID)
                        .WhereIF(!string.IsNullOrEmpty(sBillNO), (t1) => t1.Bills.Contains("\"BillNO\":%\"" + sBillNO))
                        .WhereIF(!string.IsNullOrEmpty(sExhibitionName), (t1, t2, t3) => (t3.Exhibitioname_TW + t3.Exhibitioname_EN + t3.ExhibitioShotName_TW).Contains(sExhibitionName))
                        .WhereIF(!string.IsNullOrEmpty(sResponsiblePerson), (t1) => t1.ResponsiblePerson == sResponsiblePerson)
                        .WhereIF(!string.IsNullOrEmpty(sDeclarationClass), (t1) => t1.DeclarationClass == sDeclarationClass)
                        .WhereIF(!string.IsNullOrEmpty(sCustomer), (t1, t2, t3, t4, t5) => (t5.CustomerCName + t5.CustomerEName).Contains(sCustomer))
                        .WhereIF(!string.IsNullOrEmpty(sAgent), (t1, t2, t3, t4, t5, t6) => (t6.CustomerCName + t6.CustomerEName).Contains(sAgent))
                        .WhereIF(!string.IsNullOrEmpty(sDepartmentID), (t1) => SqlFunc.ContainsArray(saChildDeptIds, t1.DepartmentID))
                        .WhereIF(!string.IsNullOrEmpty(sIsIncludeVoid), (t1) => SqlFunc.ContainsArray(saIsIncludeVoid, t1.IsVoid))
                        .Where((t1) => t1.CreateUser == i_crm.USERID || t1.ResponsiblePerson == i_crm.USERID || SqlFunc.ContainsArray(saDeptIdsByUser, t1.DepartmentID) ||
                       SqlFunc.Subqueryable<OTB_SYS_Members>().Where(c => c.MemberID == t1.CreateUser && c.OrgID == t1.OrgID).Select(c => c.ImmediateSupervisor) == i_crm.USERID ||
                       SqlFunc.Subqueryable<OTB_SYS_Members>().Where(p => p.MemberID == t1.ResponsiblePerson && p.OrgID == t1.OrgID).Select(c => c.ImmediateSupervisor) == i_crm.USERID || SqlFunc.ContainsArray(saChildUserIds, t1.CreateUser) || SqlFunc.ContainsArray(saChildUserIds, t1.ResponsiblePerson) || SqlFunc.ContainsArray(saRoles, "Account") || SqlFunc.ContainsArray(saRoles, "CDD") || SqlFunc.ContainsArray(saRoles, "Admin") || SqlFunc.ContainsArray(saRoles, "Manager"))
                        .Select((t1, t2, t3, t4, t5, t6) => new View_OPM_OtherExhibition
                        {
                            OrgID = t1.OrgID,
                            Guid = t1.Guid,
                            ExhibitionNO = t1.ExhibitionNO,
                            ExhibitionDateStart = t1.ExhibitionDateStart,
                            ExhibitionName = t3.Exhibitioname_TW,
                            Exhibitioname_EN = t3.Exhibitioname_EN,
                            ExhibitioShotName_TW = t3.ExhibitioShotName_TW,
                            ResponsiblePersonName = t2.MemberName,
                            DeclarationClassName = t4.ArgumentValue,
                            CustomerNM = SqlFunc.IIF(SqlFunc.HasValue(t5.CustomerCName), t5.CustomerCName, t5.CustomerEName),
                            AgentName = SqlFunc.IIF(SqlFunc.HasValue(t6.CustomerCName), t6.CustomerCName, t6.CustomerEName),
                            ArrivalTimeShow = SqlFunc.MappingColumn(t1.Guid, "CONVERT(VARCHAR(16),ArrivalTime, 120)"),
                            FreePeriodShow = SqlFunc.MappingColumn(t1.Guid, "CONVERT(VARCHAR(16),FreePeriod, 23)"),
                            ResponsiblePerson = t1.ResponsiblePerson,
                            Supplier = t1.Supplier,
                            Agent = t1.Agent,
                            DeclarationClass = t1.DeclarationClass,
                            ImportDeclarationNO = t1.ImportDeclarationNO,
                            ContainerNumber = t1.ContainerNumber,
                            Payer = t1.Payer,
                            BillLadNO = t1.BillLadNO,
                            BillLadNOSub = t1.BillLadNOSub,
                            BoxNo = t1.BoxNo,
                            Unit = t1.Unit,
                            Weight = t1.Weight,
                            ShipmentPort = t1.ShipmentPort,
                            DestinationPort = t1.DestinationPort,
                            ArrivalTime = t1.ArrivalTime,
                            StoragePlace = t1.StoragePlace,
                            GoodsType = t1.GoodsType,
                            Size = t1.Size,
                            VolumeWeight = t1.VolumeWeight,
                            FreePeriod = t1.FreePeriod,
                            ExchangeRate = t1.ExchangeRate,
                            IsVoid = t1.IsVoid,
                            Volume = t1.Volume,
                            Price = t1.Price,
                            ShipmentPortCode = t1.ShipmentPortCode,
                            DestinationPortCode = t1.DestinationPortCode,
                            VoidReason = t1.VoidReason,
                            Bills = t1.Bills,
                            DepartmentID = t1.DepartmentID,
                            Memo = t1.Memo,
                            CreateDate = t1.CreateDate
                        })
                        .MergeTable()
                        .OrderByIF(string.IsNullOrEmpty(sSortField), "isnull(ExhibitionDateStart,CreateDate) desc,ExhibitionName")
                        .OrderByIF(!string.IsNullOrEmpty(sSortField), sSortField + " " + sSortOrder)
                        .ToPageList(pml.PageIndex, bExcel ? 100000 : pml.PageSize, ref iPageCount);
                    pml.Total = iPageCount;

                    rm = new SuccessResponseMessage(null, i_crm);
                    if (bExcel)
                    {
                        var sFileName = "";
                        var oHeader = new Dictionary<string, string>();
                        var listMerge = new List<Dictionary<string, int>>();
                        var dicAlain = new Dictionary<string, string>();
                        var saCustomers1 = pml.DataList;
                        var saOtherExhibition = pml.DataList as List<View_OPM_OtherExhibition>;
                        var dt_new = saOtherExhibition.ListToDataTable<View_OPM_OtherExhibition>();
                        switch (sExcelType)
                        {
                            case "OtherBusiness_BasicInformation":
                                sFileName = "其他業務基本資料";
                                oHeader = new Dictionary<string, string>
                                                {
                                                    { "RowIndex", "項次" },
                                                    { "Guid", "帳單號碼" },
                                                    { "ExhibitionName", "活動/展覽名稱" },
                                                    { "ResponsiblePersonName", "負責業務" },
                                                    { "AgentName", "國外代理" },
                                                    { "DeclarationClassName", "報關類別" },
                                                    { "ImportDeclarationNO", "報單號碼" },
                                                    { "ContainerNumber", "貨櫃號碼" },
                                                    { "BillLadNO", "提單號碼" },
                                                    { "BillLadNOSub", "分提單號碼" },
                                                    { "BoxNo", "件數" },
                                                    { "Unit", "單位" },
                                                    { "Weight", "毛重(G.W.)" },
                                                    { "ShipmentPort", "起運地" },
                                                    { "DestinationPort", "目的地" },
                                                    { "ArrivalTimeShow", "抵達時間" },
                                                    { "StoragePlace", "貨物儲放倉庫" },
                                                    { "GoodsType", "貨型" },
                                                    { "Size", "尺寸" },
                                                    { "Volume", "材積(CBM)" },
                                                    { "VolumeWeight", "材積重(C.W.)" },
                                                    { "Price", "貨價" },
                                                    { "FreePeriodShow", "免費期" },
                                                    { "Memo", "特別注意事項" }
                                                };

                                foreach (DataRow row in dt_new.Rows)
                                {
                                    var sBills = "";
                                    var JaBills = (JArray)JsonConvert.DeserializeObject(row["Bills"].ToString());
                                    foreach (JObject bill in JaBills)
                                    {
                                        sBills += bill["BillNO"].ToString() + "，";
                                    }
                                    row["Guid"] = sBills;
                                }
                                break;

                            default:
                                {
                                    break;
                                }
                        }
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
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(OtherBusiness_QryService), "", "QueryPage（其他（分頁查詢））", "", "", "");
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

        #endregion 其他（分頁查詢）
    }
}