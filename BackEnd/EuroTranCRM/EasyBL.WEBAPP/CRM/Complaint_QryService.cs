using EasyBL.WebApi.Message;
using EasyNet;
using Entity;
using Entity.Sugar;
using Entity.ViewModels;
using JumpKick.HttpLib;
using SqlSugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Aspose.Cells;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.IO;
using EasyBL;

namespace EasyBL.WEBAPP.CRM
{
    public class Complaint_QryService : ServiceBase
    {
        #region 取得組團單位

        /// <summary>
        /// 取得組團單位
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on GetGroupUnit</param>
        /// <returns></returns>
        public ResponseMessage GetGroupUnit(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var QueryData = db.Queryable<OTB_CRM_Customers>().Where(x => x.IsGroupUnit == "Y").ToList();

                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, QueryData);
                    } while (false);

                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Complaint_QryService), @"取得組團單位", @"GetGroupUnit（取得組團單位）", @"", @"", @"");
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

        #region 取得配合代理

        /// <summary>
        /// 取得配合代理
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on GetCoopAgent</param>
        /// <returns></returns>
        public ResponseMessage GetCoopAgent(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var QueryData = db.Queryable<OTB_CRM_Customers>().Where(x => x.TransactionType == "C" || x.TransactionType == "D").ToList();

                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, QueryData);
                    } while (false);

                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Complaint_QryService), @"取得配合代理", @"GetCoopAgent（取得配合代理）", @"", @"", @"");
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

        #region 取得客戶名稱

        /// <summary>
        /// 取得客戶名稱
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on GetCustomers</param>
        /// <returns></returns>
        public ResponseMessage GetCustomers(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var QueryData = db.Queryable<OTB_CRM_Customers>().ToList();

                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, QueryData);
                    } while (false);

                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Complaint_QryService), @"取得客戶名稱", @"GetCustomers（取得客戶名稱）", @"", @"", @"");
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

        #region 客訴人頁面查詢

        /// <summary>
        /// 客訴人頁面查詢
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on QueryPage</param>
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
                    var pm1 = new PageModel
                    {
                        PageIndex = _fetchInt(i_crm, @"pageIndex"),
                        PageSize = _fetchInt(i_crm, @"pageSize")
                    };
                    var sSortField = _fetchString(i_crm, @"sortField");
                    var sSortOrder = _fetchString(i_crm, @"sortOrder");
                    var iPageCount = 0;
                    var sComplaintNumber = _fetchString(i_crm, @"ComplaintNumber");
                    var sComplaintTitle = _fetchString(i_crm, @"ComplaintTitle");
                    var sExhibitionName_TW = _fetchString(i_crm, @"ExhibitionName_TW");
                    var sCustomerCName = _fetchString(i_crm, @"CustomerCName");
                    var sComplainant = _fetchString(i_crm, @"Complainant");
                    var sCreateUser = _fetchString(i_crm, @"CreateUser");
                    var sDateStart = _fetchString(i_crm, @"CreateDateStart");
                    var sDateEnd = _fetchString(i_crm, @"CreateDateEnd");
                    var sComplaintSource = _fetchString(i_crm, @"ComplaintSource");
                    var sGroupUnit = _fetchString(i_crm, @"GroupUnit");
                    var sCoopAgent = _fetchString(i_crm, @"CoopAgent");
                    var sImportant = _fetchString(i_crm, @"Important");
                    var sComplaintType = _fetchString(i_crm, @"ComplaintType");
                    var sDataType = _fetchString(i_crm, @"DataType");

                    var bExcel = _fetchBool(i_crm, @"Excel");
                    var sExcelType = _fetchString(i_crm, @"ExcelType");

                    /*
                     關聯 OTB_CRM_Customers 取得CustomerID
                     */
                    //var sCustomerId = db.Queryable<OTB_CRM_Customers>()
                    //    .Where(x => (x.CustomerCName == sCustomerCName || x.CustomerEName == sCustomerCName || x.CustomerShotCName == sCustomerCName || x.CustomerShotEName == sCustomerCName))
                    //    .Select(x=> new {
                    //            x.guid
                    //    })
                    //    .ToList();
                    /*
                     關聯 OTB_OPM_Exhibition 取得ExhibitionCode
                     */
                    //var sExhibitionNO = db.Queryable<OTB_OPM_Exhibition>()
                    //    .Where(x => x.Exhibitioname_TW == sExhibitionName_TW || x.Exhibitioname_EN == sExhibitionName_TW || x.Exhibitioname_CN == sExhibitionName_TW || x.ExhibitioShotName_TW == sExhibitionName_TW || x.ExhibitioShotName_EN == sExhibitionName_TW || x.ExhibitioShotName_CN == sExhibitionName_TW)
                    //    .Select(x => new
                    //    {
                    //        x.ExhibitionCode
                    //    })
                    //    .ToList();

                    //var saCustomerId = "";
                    //var saExhibitionNO = "";
                    //if (sCustomerCName != "")
                    //{
                    //    saCustomerId = sCustomerId[0].guid;
                    //}
                    
                    //if (sExhibitionName_TW != "")
                    //{
                    //    saExhibitionNO = sExhibitionNO[0].ExhibitionCode;
                    //}
                    var rDateStart = new DateTime();
                    var rDateEnd = new DateTime();

                    if (!string.IsNullOrEmpty(sDateStart))
                    {
                        rDateStart = SqlFunc.ToDate(sDateStart);
                    }
                    if (!string.IsNullOrEmpty(sDateEnd))
                    {
                        rDateEnd = SqlFunc.ToDate(sDateEnd).AddDays(1);
                    }
                    pm1.DataList = db.Queryable<OTB_CRM_Complaint, OTB_CRM_Customers, OTB_OPM_Exhibition>
                        (
                            (t1, t2, t3) => new object[] {
                                JoinType.Left, t1.CustomerId == t2.guid,
                                JoinType.Left, t1.ExhibitionNO == t3.SN.ToString()
                              }
                        )
                        .Where((t1, t2, t3) => t1.ComplaintNumber.Contains(sComplaintNumber) && t1.ComplaintTitle.Contains(sComplaintTitle) && t3.Exhibitioname_TW.Contains(sExhibitionName_TW) && t1.Complainant.Contains(sComplainant))
                        .WhereIF(!string.IsNullOrEmpty(sCustomerCName), (t1, t2, t3) => t2.CustomerCName.Contains(sCustomerCName))
                        .WhereIF(!string.IsNullOrEmpty(sDateStart), (t1, t2, t3) => t1.CreateDate >= rDateStart.Date)
                        .WhereIF(!string.IsNullOrEmpty(sDateEnd), (t1, t2, t3) => t1.CreateDate <= rDateEnd.Date)
                        .WhereIF(!string.IsNullOrEmpty(sCreateUser), (t1, t2, t3) => t1.CreateUser == sCreateUser)
                        .WhereIF(!string.IsNullOrEmpty(sComplaintSource), (t1, t2, t3) => t1.ComplaintSource == sComplaintSource)
                        .WhereIF(!string.IsNullOrEmpty(sGroupUnit), (t1, t2, t3) => t1.GroupUnit == sGroupUnit)
                        .WhereIF(!string.IsNullOrEmpty(sCoopAgent), (t1, t2, t3) => t1.CoopAgent == sCoopAgent)
                        .WhereIF(!string.IsNullOrEmpty(sImportant), (t1,t2,t3) =>　t1.Important == sImportant)
                        .WhereIF(!string.IsNullOrEmpty(sDataType), (t1,t2,t3) => sDataType.Contains(t1.DataType))
                        .WhereIF(!string.IsNullOrEmpty(sComplaintType), (t1,t2,t3) => sComplaintType.Contains(t1.ComplaintType))
                        .Select((t1, t2, t3) => new View_CRM_Complaint {
                            Guid = t1.Guid,
                            ComplaintNumber = t1.ComplaintNumber,
                            ComplaintTitle = t1.ComplaintTitle,
                            ComplaintType = t1.ComplaintType,
                            ExhibitioShotName_TW = t3.ExhibitioShotName_TW,
                            CustomerCName = t2.CustomerCName,
                            Complainant = t1.Complainant,
                            CreateUser = t1.CreateUser,
                            CreateDate = t1.CreateDate,
                            ModifyDate = t1.ModifyDate,
                            DataType = t1.DataType,
                            Handle_Person = t1.Handle_Person,
                            Handle_DeptID = t1.Handle_DeptID

                        })
                        .MergeTable()
                        .OrderBy(sSortField, sSortOrder)
                        .ToPageList(pm1.PageIndex, bExcel ? 100000 : pm1.PageSize, ref iPageCount);
                    pm1.Total = iPageCount;

                    rm = new SuccessResponseMessage(null, i_crm);
                    if (bExcel)
                    {
                        var sFileName = "";
                        var oHeader = new Dictionary<string, string>();
                        var listMerge = new List<Dictionary<string, int>>();
                        var dicAlain = new Dictionary<string, string>();
                        var dt_new = new DataTable();
                        var saCustomers1 = pm1.DataList;
                        var saExhibition = pm1.DataList as List<View_CRM_Complaint>;
                        switch (sExcelType)
                        {
                            case "Complaint_BasicInformation":
                                {
                                    sFileName = "客訴管理基本資料";
                                    oHeader = new Dictionary<string, string>
                                                {
                                                    { "RowIndex", "項次" },
                                                    { "ComplaintNumber", "客訴編號" },
                                                    { "ComplaintTitle", "客訴主旨" },
                                                    { "ComplaintType", "客訴類型" },
                                                    { "ExhibitioShotName_TW", "活動/展覽簡稱" },
                                                    { "CustomerCName", "客戶名稱" },
                                                    { "Complainant", "客訴人" },
                                                    { "CreateUser", "創建人" },
                                                    { "CreateDate", "創建時間" }
                                                };
                                    dt_new.Columns.Add("RowIndex");
                                    dt_new.Columns.Add("ComplaintNumber");
                                    dt_new.Columns.Add("ComplaintTitle");
                                    dt_new.Columns.Add("ComplaintType");
                                    dt_new.Columns.Add("ExhibitioShotName_TW");
                                    dt_new.Columns.Add("CustomerCName");
                                    dt_new.Columns.Add("Complainant");
                                    dt_new.Columns.Add("CreateUser");
                                    dt_new.Columns.Add("CreateDate");
                                    foreach (var exhibition in saExhibition)
                                    {
                                        var row_new = dt_new.NewRow();
                                        if (exhibition.CreateDate != null)
                                        {
                                            row_new["CreateDate"] = Convert.ToDateTime(exhibition.CreateDate).ToString("yyyy/MM/dd HH:mm");
                                        }
                                        else
                                        {
                                            row_new["CreateDate"] = @"";
                                        }
                                        row_new["RowIndex"] = exhibition.RowIndex;
                                        row_new["ComplaintNumber"] = exhibition.ComplaintNumber;
                                        row_new["ComplaintTitle"] = exhibition.ComplaintTitle;
                                        row_new["ComplaintType"] = exhibition.ComplaintType;
                                        row_new["ExhibitioShotName_TW"] = exhibition.ExhibitioShotName_TW;
                                        row_new["CustomerCName"] = exhibition.CustomerCName;
                                        row_new["Complainant"] = exhibition.Complainant;
                                        row_new["CreateUser"] = exhibition.CreateUser;
                                        row_new["CreateDate"] = exhibition.CreateDate;
                                        dt_new.Rows.Add(row_new);
                                    }
                                    dicAlain = ExcelService.GetExportAlain(oHeader, "ExhibitionCode,ExhibitionDateStart,IsShowWebSite,CreateUserName,CreateDate");
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
                        rm.DATA.Add(BLWording.REL, pm1);
                    }
                    //rm.DATA.Add(BLWording.REL, pm1);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(BusinessOpportunity_QryService), @"客訴人頁面查詢", @"QueryPage（客訴人頁面查詢）", @"", @"", @"");
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

        #region 客訴頁面單筆查詢

        /// <summary>
        /// 客訴頁面單筆查詢
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
                    var iId = _fetchString(i_crm, @"Guid");
                    var sCreateUser = db.Queryable<OTB_CRM_Complaint>()
                        .Where((t1) => t1.Guid == iId)
                        .Select(t1 => new
                        {
                            CreateUser = t1.CreateUser 
                        }).Single();

                    var sDepartmentID = db.Queryable<OTB_SYS_Members>()
                        .Where(t1 => t1.MemberID == sCreateUser.CreateUser )
                        .Select(t1 => new
                        {
                            DepartmentID = t1.DepartmentID,
                            ModifyDate = t1.ModifyDate
                        }).ToList().OrderByDescending(x=> x.ModifyDate).First();

                    var sDepartmentName = db.Queryable<OTB_SYS_Departments>()
                        .Where(t1 => t1.DepartmentID == sDepartmentID.DepartmentID)
                        .Select(t1 => new
                        {
                            DepartmentName = t1.DepartmentName,
                            ModifyDate = t1.ModifyDate
                        }).ToList().OrderByDescending(x => x.ModifyDate).First();

                    var oExportExhibition = db.Queryable<OTB_CRM_Complaint>()
                        .Where((t1) => t1.Guid == iId)
                        .Select((t1) => new View_CRM_Complaint
                        {
                            Guid = t1.Guid,
                            ComplaintNumber = t1.ComplaintNumber,
                            ComplaintTitle = t1.ComplaintTitle,
                            ComplaintType = t1.ComplaintType,
                            Important = t1.Important,
                            ExhibitionName = t1.ExhibitionNO,
                            ComplaintSource = t1.ComplaintSource,
                            CoopAgent = t1.CoopAgent,
                            GroupUnit = t1.GroupUnit,
                            Description = t1.Description,
                            CustomerCName = t1.CustomerId,
                            Complainant = t1.Complainant,
                            Department = t1.Department,
                            JobTitle = t1.JobTitle,
                            Email1 = t1.Email1,
                            Email2 = t1.Email2,
                            Telephone1 = t1.Telephone1,
                            Telephone2 = t1.Telephone2,
                            FlowId = t1.FlowId,
                            CheckOrder = t1.CheckOrder,
                            DataType = t1.DataType,
                            IsHandled = t1.IsHandled,
                            CheckFlows = t1.CheckFlows,
                            HandleFlows = t1.HandleFlows,
                            VoidReason = t1.VoidReason,
                            Flows_Lock = t1.Flows_Lock,
                            Handle_Lock = t1.Handle_Lock,
                            Memo = t1.Memo,
                            CreateUser = t1.CreateUser,
                            CreateDate = t1.CreateDate,
                            ModifyUser = t1.ModifyUser,
                            ModifyDate = t1.ModifyDate,
                            Handle_Person = t1.Handle_Person,
                            Handle_DeptID = t1.Handle_DeptID,
                            CreateUserName = SqlFunc.MappingColumn(t1.Guid, "dbo.[OFN_SYS_MemberNameByMemberIDwithoutOrgID](CreateUser)"),
                            DepartmentName = sDepartmentName.DepartmentName,
                        }).Single();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, oExportExhibition);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBAPP.CRM.Complaint_QryService", "", "QueryOne（客訴頁面單筆查詢）", "", "", "");
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

        #endregion 客訴頁面單筆查詢
    }
}
