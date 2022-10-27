using EasyBL.WebApi.Message;
using Entity.Sugar;
using SqlSugar;
using SqlSugar.Base;
using System;

namespace EasyBL.WEBAPP.SYS
{
    public class Announcement_QryService : ServiceBase
    {
        #region 公告管理（分頁資料）

        /// <summary>
        /// 公告管理（分頁資料）
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
                    var sAnn_Type = _fetchString(i_crm, @"Ann_Type");
                    var sTitle = _fetchString(i_crm, @"Title");
                    var sStartDateTime = _fetchString(i_crm, @"StartDateTime");
                    var sEndDateTime = _fetchString(i_crm, @"EndDateTime");
                    var bExcel = _fetchBool(i_crm, @"Excel");

                    var rStartDateTime = new DateTime();
                    var rEndDateTime = new DateTime();
                    if (!string.IsNullOrEmpty(sStartDateTime))
                    {
                        rStartDateTime = SqlFunc.ToDate(sStartDateTime);
                    }
                    if (!string.IsNullOrEmpty(sEndDateTime))
                    {
                        rEndDateTime = SqlFunc.ToDate(sEndDateTime).AddDays(1);
                    }

                    pml.DataList = db.Queryable<OTB_SYS_Announcement>()
                        .Where(x => x.OrgID == i_crm.ORIGID && x.Title.Contains(sTitle))
                        .WhereIF(!string.IsNullOrEmpty(sAnn_Type), x => x.Ann_Type == sAnn_Type)
                        .WhereIF(!string.IsNullOrEmpty(sStartDateTime), x => x.StartDateTime >= rStartDateTime.Date)
                        .WhereIF(!string.IsNullOrEmpty(sEndDateTime), x => x.EndDateTime <= rEndDateTime.Date)
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
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Announcement_QryService), @"公告管理", @"QueryPage（公告管理（分頁資料））", @"", @"", @"");
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

        #endregion 公告管理（分頁資料）
    }
}