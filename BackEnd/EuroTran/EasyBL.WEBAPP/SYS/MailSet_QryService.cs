using EasyBL.WebApi.Message;
using Entity.Sugar;
using SqlSugar;
using SqlSugar.Base;
using System;

namespace EasyBL.WEBAPP.SYS
{
    public class MailSet_QryService : ServiceBase
    {
        #region 郵件管理（分頁查詢）

        /// <summary>
        /// 郵件管理（分頁查詢）
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

                    var sEmailID = _fetchString(i_crm, @"EmailID");
                    var sEmailName = _fetchString(i_crm, @"EmailName");
                    var sEmailSubject = _fetchString(i_crm, @"EmailSubject");
                    var bExcel = _fetchBool(i_crm, @"Excel");

                    pml.DataList = db.Queryable<OTB_SYS_Email>()
                        .Where(x => x.OrgID == i_crm.ORIGID && x.EmailID.Contains(sEmailID) && x.EmailName.Contains(sEmailName) && x.EmailSubject.Contains(sEmailSubject))
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
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(MailSet_QryService), "", "QueryPage（郵件管理（分頁查詢））", "", "", "");
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

        #endregion 郵件管理（分頁查詢）
    }
}