using EasyBL.WebApi.Message;
using EasyNet;
using Entity.Sugar;
using Entity.ViewModels;
using SqlSugar;
using SqlSugar.Base;
using System;

namespace EasyBL.WEBAPP.SYS
{
    public class Task_QryService : ServiceBase
    {
        #region 代辦管理

        /// <summary>
        /// 代辦管理
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on GetTasks</param>
        /// <returns></returns>
        public ResponseMessage GetTasks(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.DB;
            try
            {
                do
                {
                    var saTasks = db.Queryable<OTB_SYS_Task, OTB_SYS_Members, OTB_SYS_Members, OTB_SYS_ProgramList>
                        ((t1, t2, t3, t4) =>
                        new object[] {
                                JoinType.Left, t1.OrgID == t2.OrgID && t1.Owner == t2.MemberID,
                                JoinType.Left, t2.OrgID == t3.OrgID && t2.CreateUser == t3.MemberID,
                                JoinType.Left, t1.OrgID == t4.OrgID && t1.SourceFrom == t4.ProgramID
                              }
                        )
                        .Where((t1, t2, t3, t4) => t1.OrgID == i_crm.ORIGID && t1.Owner == i_crm.USERID && t1.Status != @"O" && SqlFunc.HasValue(t1.EventNo))
                        .OrderBy((t1, t2, t3, t4) => t1.CreateDate, OrderByType.Desc)
                        .Select((t1, t2, t3, t4) => new
                        {
                            OwnerName = t2.MemberName,
                            CreateUserName = t3.MemberName,
                            ProgressShow = t1.Progress + "%",
                            ImportantName = SqlFunc.IIF(t1.Important == "M", "普通", "緊急"),
                            SourceFromName = t4.ProgramName,
                            EventID = SqlFunc.GetSelfAndAutoFill(t1.EventID),
                        }).ToList();

                    //var saTasks = db.Queryable<OVW_SYS_Task>()
                    //    .Where(p => p.OrgID == i_crm.ORIGID && p.Owner == i_crm.USERID && p.Status != @"O" && SqlFunc.IsNull(p.EventNo, @"") != @"")
                    //    .OrderBy(p => p.CreateDate, OrderByType.Desc).ToList();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, saTasks);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Task_QryService), @"代辦事項", @"GetTasks（代辦管理）", @"", @"", @"");
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

        #endregion 代辦管理

        #region 代辦管理（分頁資料）

        /// <summary>
        /// 代辦管理（分頁資料）
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on QueryPage</param>
        /// <returns></returns>
        public ResponseMessage QueryPage(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.DB;
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
                    var sEventName = _fetchString(i_crm, @"EventName");
                    var sOwner = _fetchString(i_crm, @"Owner");
                    var sCreateUser = _fetchString(i_crm, @"CreateUser");
                    var sIsIncludReady = _fetchString(i_crm, @"IsIncludReady");
                    var bIsEIP = _fetchBool(i_crm, @"IsEIP");

                    pml.DataList = db.Queryable<OTB_SYS_Task, OTB_SYS_Members, OTB_SYS_Members, OTB_SYS_ProgramList>
                        ((t1, t2, t3, t4) =>
                        new object[] {
                                JoinType.Inner, t1.OrgID == t2.OrgID && t1.Owner == t2.MemberID,
                                JoinType.Inner, t1.OrgID == t3.OrgID && t1.CreateUser == t3.MemberID,
                                JoinType.Inner, t1.OrgID == t4.OrgID && t1.SourceFrom == t4.ProgramID
                              }
                        )
                        .Where((t1, t2, t3, t4) => t1.OrgID == i_crm.ORIGID)
                        .WhereIF(sEventName == null, (t1, t2, t3, t4) => t1.Owner == i_crm.USERID && t1.Status != @"O" && SqlFunc.HasValue(t1.EventNo))
                        .WhereIF(sEventName == null && bIsEIP, (t1, t2, t3, t4) => SqlFunc.HasValue(t1.EIP_Status))
                        .WhereIF(sEventName == null && !bIsEIP, (t1, t2, t3, t4) => !SqlFunc.HasValue(t1.EIP_Status))
                        .WhereIF(sEventName != null, (t1, t2, t3, t4) => t1.EventName.Contains(sEventName))
                        .WhereIF(sEventName != null && !string.IsNullOrEmpty(sOwner), (t1, t2, t3, t4) => t1.Owner == sOwner)
                        .WhereIF(sEventName != null && !string.IsNullOrEmpty(sCreateUser), (t1, t2, t3, t4) => t1.CreateUser == sCreateUser)
                        .WhereIF(sEventName != null, (t1, t2, t3, t4) => sIsIncludReady.Contains(t1.Status))
                        .WhereIF(sEventName != null, (t1, t2, t3, t4) => t1.Owner == i_crm.USERID || t1.CreateUser == i_crm.USERID)
                        .Select((t1, t2, t3, t4) => new View_SYS_Task
                        {
                            EventID = SqlFunc.GetSelfAndAutoFill(t1.EventID),
                            OwnerName = t2.MemberName,
                            CreateUserName = t3.MemberName,
                            ProgressShow = t1.Progress.ToString() + "%",
                            ImportantName = SqlFunc.IIF(t1.Important == "M", "普通", "緊急"),
                            SourceFromName = t4.ProgramName
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
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Task_QryService), @"代辦事項", @"GetTasksPage（代辦管理（分頁資料））", @"", @"", @"");
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

        #endregion 代辦管理（分頁資料）

        /// <summary>
        /// 修改代辦狀態
        /// </summary>
        /// <param name="sOrgId"></param>
        /// <param name="sSourceID"></param>
        /// <param name="db">todo: describe db parameter on TaskStatusUpd</param>
        /// <param name="sOwner">todo: describe sOwner parameter on TaskStatusUpd</param>
        public static void TaskStatusUpd(SqlSugarClient db, string sOrgId, string sSourceID, string sOwner = null)
        {
            var oTaskUpd = new OTB_SYS_Task
            {
                Status = @"O",
                Progress = 100
            };
            db.Updateable(oTaskUpd).UpdateColumns(it => new { it.Status, it.ModifyDate, it.ModifyUser })
                .Where(it => it.SourceID == sSourceID && it.OrgID == sOrgId && (it.Owner == sOwner || SqlFunc.IsNullOrEmpty(sOwner))).ExecuteCommand();
        }

        /// <summary>
        /// 修改代辦狀態
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on TipsAdd</param>
        /// <param name="sTitle">todo: describe sTitle parameter on TipsAdd</param>
        /// <param name="sOwner">todo: describe sOwner parameter on TipsAdd</param>
        /// <param name="Url">todo: describe Url parameter on TipsAdd</param>
        /// <param name="sTipsType">todo: describe sTipsType parameter on TipsAdd</param>
        public static OTB_SYS_Tips TipsAdd(RequestMessage i_crm, string sTitle, string sOwner, string Url, string sTipsType)
        {
            var oTipsAdd = new OTB_SYS_Tips
            {
                OrgID = i_crm.ORIGID,
                Owner = sOwner.Trim().Replace("\r\n",""),
                TipsType = sTipsType,
                Title = sTitle,
                IsRead = @"N",
                Url = Url,
                CreateUser = i_crm.USERID,
                CreateDate = DateTime.Now,
                ModifyUser = i_crm.USERID,
                ModifyDate = DateTime.Now
            };
            return oTipsAdd;
        }

        /// <summary>
        /// 修改代辦狀態
        /// </summary>
        /// <param name="sSourceID"></param>
        /// <param name="i_crm">todo: describe i_crm parameter on TaskAdd</param>
        /// <param name="sOwner">todo: describe sOwner parameter on TaskAdd</param>
        /// <param name="sTitle">todo: describe sTitle parameter on TaskAdd</param>
        /// <param name="sSourceFrom">todo: describe sSourceFrom parameter on TaskAdd</param>
        /// <param name="sParams">todo: describe sParams parameter on TaskAdd</param>
        /// <param name="sEIP_Status">todo: describe sEIP_Status parameter on TaskAdd</param>
        public static OTB_SYS_Task TaskAdd(RequestMessage i_crm, string sSourceID, string sOwner, string sTitle, string sSourceFrom, string sParams, string sEIP_Status = null)
        {
            var oTaskAdd = new OTB_SYS_Task();
            var sEventID = Guid.NewGuid().ToString();
            oTaskAdd.OrgID = i_crm.ORIGID;
            oTaskAdd.EventID = sEventID;
            oTaskAdd.EventName = sTitle;
            oTaskAdd.Owner = sOwner.Trim().Replace("\r\n", "");
            oTaskAdd.StartDate = DateTime.Now;
            oTaskAdd.Progress = 0;
            oTaskAdd.PreProgress = 0;
            oTaskAdd.Important = @"M";
            oTaskAdd.Status = @"U";
            oTaskAdd.SourceFrom = sSourceFrom;
            oTaskAdd.SourceID = sSourceID;
            oTaskAdd.SourceFromID = sSourceID;
            oTaskAdd.Params = sParams;
            oTaskAdd.EIP_Status = sEIP_Status ?? @"";
            oTaskAdd.CreateUser = i_crm.USERID;
            oTaskAdd.CreateDate = DateTime.Now;
            oTaskAdd.ModifyUser = i_crm.USERID;
            oTaskAdd.ModifyDate = DateTime.Now;
            oTaskAdd.EventNo = SerialNumber.GetMaxNumberByType(i_crm.ORIGID, @"", MaxNumberType.DayForSix, i_crm.USERID, 4);
            return oTaskAdd;
        }
    }
}