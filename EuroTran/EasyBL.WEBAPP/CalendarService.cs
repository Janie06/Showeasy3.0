using EasyBL.WebApi.Message;
using Entity.Sugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;

namespace EasyBL.WEBAPP
{
    public class CalendarService : ServiceBase
    {
        #region 抓去行事曆資料

        /// <summary>
        /// 函式名稱:GetList
        /// 函式說明:抓去行事曆資料
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on GetList</param>
        /// <returns>
        /// 回傳 rm(Object)
        ///</returns>
        public ResponseMessage GetList(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    //HttpContext hc = HttpContext.Current;
                    //string outlook_userName = ClaimsPrincipal.Current.FindFirst("name").Value;
                    var sStartDate = _fetchString(i_crm, @"StartDate");
                    var sEndDate = _fetchString(i_crm, @"EndDate");
                    var sCalType = _fetchString(i_crm, @"CalType");
                    var sOpenMent = _fetchString(i_crm, @"OpenMent");

                    var rStartDate = Convert.ToDateTime(sStartDate);
                    var rEndDate = Convert.ToDateTime(sEndDate);
                    const string sSQL = @"SELECT DISTINCT MemberID+',' FROM dbo.OTB_SYS_Members WHERE DepartmentID IN (SELECT * FROM [dbo].[OFN_SYS_GetParentDepartmentIdByUserID](@OrgID,@UserID)) FOR XML PATH('')";
                    var dic_pm = new Dictionary<string, string>
                        {
                            { @"OrgID", i_crm.ORIGID },
                            { @"UserID", i_crm.USERID }
                        };

                    var sParentDeptUsers = db.Ado.GetString(sSQL, dic_pm);

                    var saCalendar = db.Queryable<OTB_SYS_Calendar>()
                         .OrderBy(x => x.StartDate)
                         .Where(x => x.OrgID == i_crm.ORIGID && x.StartDate.Date >= rStartDate.Date && x.EndDate.Date <= rEndDate.Date)
                         .Where(x => x.UserID == i_crm.USERID
                         || (x.OpenMent == @"G" && x.GroupMembers.Contains(i_crm.USERID))
                         || (x.OpenMent == @"D" && sParentDeptUsers.Contains(x.UserID))
                         || x.OpenMent == @"C")
                         .Where(x => sCalType.Contains(x.CalType))
                         .Where(x => sOpenMent.Contains(x.OpenMent))
                         .Where(x => !x.DelStatus)
                         .ToList();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, saCalendar);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(CalendarService), @"行事曆", @"GetList（抓去行事曆資料）", @"", @"", @"");
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

        #endregion 抓去行事曆資料

        #region 拆入一條曆資料

        ///  <summary>
        /// 函式名稱:GetList
        /// 函式說明:拆入一條曆資料
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on Add</param>
        /// <returns>
        /// 回傳 rm(Object)
        ///</returns>
        public ResponseMessage Add(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sCalType = _fetchString(i_crm, @"CalType");
                    var UserID = _fetchString(i_crm, @"UserID") ?? i_crm.USERID;
                    var sTitle = _fetchString(i_crm, @"Title");
                    var sDescription = _fetchString(i_crm, @"Description");
                    var sStartDate = _fetchString(i_crm, @"StartDate");
                    var sEndDate = _fetchString(i_crm, @"EndDate");
                    var sImportment = _fetchString(i_crm, @"Importment");
                    var sColor = _fetchString(i_crm, @"Color");
                    var sAllDay = _fetchString(i_crm, @"AllDay");
                    var sOpenMent = _fetchString(i_crm, @"OpenMent");
                    var sGroupMembers = _fetchString(i_crm, @"GroupMembers");
                    var sRelationId = _fetchString(i_crm, @"RelationId");
                    var sUrl = _fetchString(i_crm, @"Url");
                    var sMemo = _fetchString(i_crm, @"Memo") ?? "";

                    var oCalendar = new OTB_SYS_Calendar
                    {
                        OrgID = i_crm.ORIGID,
                        UserID = UserID,
                        CalType = sCalType,
                        Title = sTitle,
                        Description = sDescription,
                        StartDate = Convert.ToDateTime(sStartDate),
                        EndDate = Convert.ToDateTime(sEndDate),
                        Importment = sImportment,
                        Color = sColor,
                        AllDay = sAllDay == @"1",
                        OpenMent = sOpenMent,
                        GroupMembers = sGroupMembers,
                        Url = sUrl,
                        RelationId = sRelationId,
                        Editable = null,
                        ClassName = null,
                        Memo = sMemo,
                        CreateUser = i_crm.USERID,
                        CreateDate = DateTime.Now,
                        ModifyUser = i_crm.USERID,
                        ModifyDate = DateTime.Now,
                        DelStatus = false
                    };
                    var sNo = db.Insertable<OTB_SYS_Calendar>(oCalendar).ExecuteReturnBigIdentity();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, sNo);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(CalendarService), @"行事曆", @"Add（拆入一條曆資料）", @"", @"", @"");
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



        #endregion 拆入一條曆資料


        #region 刪除行事曆

        /// <summary>
        ///  刪除行事曆
        /// </summary>
        /// <param name="relationId"></param>
        public void DeleteCalendar(string orgID, string userID, string relationId)
        {
            var sMsg = string.Empty;
            string fnName = nameof(DeleteCalendar);
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var oTB_SYS_Calendars = db.Queryable<OTB_SYS_Calendar>()
                        .Where( x => x.OrgID == orgID)
                        .WhereIF(!string.IsNullOrEmpty(userID), x => x.UserID == userID)
                        .WhereIF(!string.IsNullOrEmpty(relationId), x => x.RelationId == relationId)
                        .ToList();
                    oTB_SYS_Calendars.ForEach(x => { x.DelStatus = true; });
                    if (oTB_SYS_Calendars.Count > 0)
                    {
                        db.Updateable(oTB_SYS_Calendars).ExecuteCommand();
                    }
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：relationId = " + relationId, ex, orgID, userID, nameof(CalendarService), @"行事曆", fnName, @"", @"", @"");
            }
        }

        #endregion
    }
}