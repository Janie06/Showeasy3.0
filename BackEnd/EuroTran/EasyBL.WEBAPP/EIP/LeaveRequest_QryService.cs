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
using System.Linq;
using System.Text.RegularExpressions;

namespace EasyBL.WEBAPP.EIP
{
    public class LeaveRequest_QryService : ServiceBase
    {
        #region 請假區間設定（分頁查詢）

        /// <summary>
        /// 客戶管理（分頁查詢）
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

                    var sHolidayCategory = _fetchString(i_crm, @"HolidayCategory");
                    var sUserID = _fetchString(i_crm, @"UserID");
                    var sLeaveDateStart = _fetchString(i_crm, @"LeaveDateStart");
                    var sLeaveDateEnd = _fetchString(i_crm, @"LeaveDateEnd");
                    var rLeaveDateStart = new DateTime();
                    var rLeaveDateEnd = new DateTime();

                    if (!string.IsNullOrEmpty(sLeaveDateStart))
                    {
                        rLeaveDateStart = SqlFunc.ToDate(sLeaveDateStart);
                    }
                    if (!string.IsNullOrEmpty(sLeaveDateEnd))
                    {
                        rLeaveDateEnd = SqlFunc.ToDate(sLeaveDateEnd);
                    }
                    var saRoles = db.Queryable<OTB_SYS_MembersToRule>().Where(x => x.OrgID == i_crm.ORIGID && x.MemberID == i_crm.USERID).Select(x => x.RuleID).ToList().ToArray();
                    pml.DataList = db.Queryable<OTB_EIP_LeaveRequest, OTB_SYS_Members, OTB_SYS_Arguments>
                        ((t1, t2, t3) =>
                        new object[] {
                                JoinType.Inner, t1.OrgID == t2.OrgID && t1.MemberID == t2.MemberID,
                                JoinType.Inner, t1.OrgID == t3.OrgID && t1.Leave == t3.ArgumentID && t3.ArgumentClassID=="LeaveType"
                              }
                        )
                        .Where(t1 => t1.OrgID == i_crm.ORIGID)
                        .WhereIF(!string.IsNullOrEmpty(sHolidayCategory), (t1) => t1.Leave == sHolidayCategory)
                        .WhereIF(!string.IsNullOrEmpty(sUserID), (t1) => t1.MemberID == sUserID)
                        .WhereIF(!string.IsNullOrEmpty(sLeaveDateStart), (t1) => t1.EnableDate >= rLeaveDateStart.Date)
                        .WhereIF(!string.IsNullOrEmpty(sLeaveDateEnd), (t1) => t1.ExpirationDate <= rLeaveDateEnd.Date)
                        .Where((t1) => t1.MemberID == i_crm.USERID
                        || SqlFunc.ContainsArray(saRoles, "EipManager") || SqlFunc.ContainsArray(saRoles, "Admin") || SqlFunc.ContainsArray(saRoles, "Manager"))
                        .Select((t1, t2, t3) => new View_EIP_LeaveRequest
                        {
                            guid = SqlFunc.GetSelfAndAutoFill(t1.guid),
                            WenZhongAcount = t2.WenZhongAcount,
                            HolidayCategoryName = t3.ArgumentValue,
                            MemberName = t2.MemberName,
                        })
                        .MergeTable()
                        .OrderBy(sSortField, sSortOrder)
                        .ToPageList(pml.PageIndex, pml.PageSize, ref iPageCount);
                    pml.Total = iPageCount;

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, pml);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(LeaveRequest_QryService), "", "QueryPage（客戶管理（分頁查詢））", "", "", "");
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

        #endregion 請假區間設定（分頁查詢）

        #region 請假區間設定（單筆查詢）

        /// <summary>
        /// 客戶管理（單筆查詢）
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

                    var oEntity = db.Queryable<OTB_EIP_LeaveRequest>().Single(x => x.OrgID == i_crm.ORIGID && x.guid == sId);

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, oEntity);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(LeaveRequest_QryService), "", "QueryOne（請假區間設定（單筆查詢））", "", "", "");
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

        #endregion 請假區間設定（單筆查詢）


        #region 取得有效的請假（多筆查詢）

        /// <summary>
        /// 取得有效的請假
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetAvailableHLeaveHours(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    //
                    var sUserID = _fetchString(i_crm, @"UserID");
                    var sOrgID = i_crm.ORIGID;
                    var sLeaveDateStart = _fetchString(i_crm, @"LeaveDateStart");
                    var sLeaveDateEnd = _fetchString(i_crm, @"LeaveDateEnd");
                    //var sHolidayCategory = _fetchString(i_crm, @"HolidayCategory");


                    var rLeaveDateStart = new DateTime();
                    var rLeaveDateEnd = new DateTime();
                    if (!string.IsNullOrEmpty(sLeaveDateStart))
                    {
                        rLeaveDateStart = SqlFunc.ToDate(sLeaveDateStart);
                    }
                    if (!string.IsNullOrEmpty(sLeaveDateEnd))
                    {
                        rLeaveDateEnd = SqlFunc.ToDate(sLeaveDateEnd);
                    }

                    var leaveRequests = db.Queryable<OTB_EIP_LeaveRequest>()
                        .Where(t1 => t1.RemainHours > 0 && t1.OrgID == sOrgID)
                        .WhereIF(!string.IsNullOrEmpty(sUserID), (t1) => t1.MemberID == sUserID)
                        .WhereIF(!string.IsNullOrEmpty(sLeaveDateStart), (t1) => t1.EnableDate <= rLeaveDateStart.Date && t1.ExpirationDate >= rLeaveDateStart.Date)
                        .WhereIF(!string.IsNullOrEmpty(sLeaveDateStart), (t1) => t1.EnableDate <= rLeaveDateEnd.Date && t1.ExpirationDate >= rLeaveDateEnd.Date)
                        .OrderBy(t1 => t1.EnableDate)
                        .OrderBy(t1 => t1.ExpirationDate)
                        .ToList();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, leaveRequests);

                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(LeaveRequest_QryService), "", "GetAvailableHLeaveHours（取得有效的請假（多筆查詢））", "", "", "");
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

        #endregion 請假區間設定（單筆查詢）


        #region 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetAllLeaveRequest(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sUserID = _fetchString(i_crm, @"UserID");
                    var sOrgID = i_crm.ORIGID;
                    var iCurrentYear = _fetchInt(i_crm, @"CurrentYear");

                    var leaveRequests = db.Queryable<OTB_EIP_LeaveRequest>()
                        .Where(t1 => t1.OrgID == sOrgID)
                        .WhereIF(!string.IsNullOrEmpty(sUserID), (t1) => t1.MemberID == sUserID)
                        .OrderBy(t1 => t1.EnableDate).ToList();
                    leaveRequests = leaveRequests.Where(t1 =>
                    {
                        if (t1.ExpirationDate.HasValue && t1.EnableDate.HasValue)
                        {
                            if (t1.ExpirationDate?.Year == iCurrentYear || t1.EnableDate?.Year == iCurrentYear)
                                return true;
                        }
                        return false;
                    }).ToList();


                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, leaveRequests);

                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(LeaveRequest_QryService), "", "GetAllLeaveRequest（請假區間設定（單筆查詢））", "", "", "");
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