using EasyBL.WebApi.Message;
using Entity.Sugar;
using SqlSugar;
using SqlSugar.Base;
using System;

namespace EasyBL.WEBAPP.SYS
{
    public class LoginLog_QryService : ServiceBase
    {
        #region 系統登陸記錄查詢（分頁資料）

        /// <summary>
        /// 系統登陸記錄查詢（分頁資料）
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
                    var sAccount = _fetchString(i_crm, @"Account");
                    var sLoginTimeStart = _fetchString(i_crm, @"LoginTimeStart");
                    var sLoginTimeEnd = _fetchString(i_crm, @"LoginTimeEnd");
                    var bExcel = _fetchBool(i_crm, @"Excel");

                    var rLoginTimeStart = new DateTime();
                    var rLoginTimeEnd = new DateTime();
                    if (!string.IsNullOrEmpty(sLoginTimeStart))
                    {
                        rLoginTimeStart = SqlFunc.ToDate(sLoginTimeStart);
                    }
                    if (!string.IsNullOrEmpty(sLoginTimeEnd))
                    {
                        rLoginTimeEnd = SqlFunc.ToDate(sLoginTimeEnd).AddDays(1);
                    }

                    pml.DataList = db.Queryable<OTB_SYS_LoginLog>()
                        .Where(x => x.OrgId == i_crm.ORIGID)
                        .WhereIF(!string.IsNullOrEmpty(sAccount), x => x.UserId == sAccount)
                        .WhereIF(!string.IsNullOrEmpty(sLoginTimeStart), x => x.LoginTime >= rLoginTimeStart.Date)
                        .WhereIF(!string.IsNullOrEmpty(sLoginTimeEnd), x => x.LoginTime <= rLoginTimeEnd.Date)
                        .OrderBy(sSortField, sSortOrder)
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
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(LoginLog_QryService), @"系統登陸記錄查詢", @"QueryPage（系統登陸記錄查詢（分頁資料））", @"", @"", @"");
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

        #endregion 系統登陸記錄查詢（分頁資料）
    }
}