using EasyBL.WebApi.Message;
using Entity.Sugar;
using SqlSugar;
using SqlSugar.Base;
using System;

namespace EasyBL.WEBAPP.SYS
{
    public class LogInfo_QryService : ServiceBase
    {
        #region 系統日誌記錄查詢（分頁資料）

        /// <summary>
        /// 系統日誌記錄查詢（分頁資料）
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
                    var sLogType = _fetchString(i_crm, @"LogType");
                    var sActor = _fetchString(i_crm, @"Actor");
                    var sKeyWords = _fetchString(i_crm, @"KeyWords");
                    var sLogTimeStart = _fetchString(i_crm, @"LogTimeStart");
                    var sLogTimeEnd = _fetchString(i_crm, @"LogTimeEnd");
                    var bExcel = _fetchBool(i_crm, @"Excel");

                    var rLogTimeStart = new DateTime();
                    var rLogTimeEnd = new DateTime();
                    if (!string.IsNullOrEmpty(sLogTimeStart))
                    {
                        rLogTimeStart = SqlFunc.ToDate(sLogTimeStart);
                    }
                    if (!string.IsNullOrEmpty(sLogTimeEnd))
                    {
                        rLogTimeEnd = SqlFunc.ToDate(sLogTimeEnd).AddDays(1);
                    }

                    pml.DataList = db.Queryable<OTB_SYS_LogInfo>()
                        .Where(x => x.OrgID == i_crm.ORIGID)
                        .WhereIF(!string.IsNullOrEmpty(sLogType), x => x.LogType == sLogType)
                        .WhereIF(!string.IsNullOrEmpty(sActor), x => x.CreateUser == sActor)
                        .WhereIF(!string.IsNullOrEmpty(sKeyWords), x => x.LogInfo.Contains(sKeyWords))
                        .WhereIF(!string.IsNullOrEmpty(sLogTimeStart), x => x.CreateDate >= rLogTimeStart.Date)
                        .WhereIF(!string.IsNullOrEmpty(sLogTimeEnd), x => x.CreateDate <= rLogTimeEnd.Date)
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
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(LogInfo_QryService), @"系統日誌記錄查詢", @"QueryPage（系統日誌記錄查詢（分頁資料））", @"", @"", @"");
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

        #endregion 系統日誌記錄查詢（分頁資料）
    }
}