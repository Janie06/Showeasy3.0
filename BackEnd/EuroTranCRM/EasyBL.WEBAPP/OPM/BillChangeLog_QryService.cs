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

namespace EasyBL.WEBAPP.OPM
{
    public class BillChangeLog_QryService : ServiceBase
    {
        #region 賬單狀態（分頁查詢）

        /// <summary>
        /// 賬單狀態（分頁查詢）
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

                    var sBillNO = _fetchString(i_crm, @"BillNO");
                    var Operation = _fetchString(i_crm, @"Operation");
                    var sModifyDateStart = _fetchString(i_crm, @"ModifyDateStart");
                    var sModifyDateEnd = _fetchString(i_crm, @"ModifyDateEnd");

                    var rCreateDateStart = new DateTime();
                    var rCreateDateEnd = new DateTime();
                    if (!string.IsNullOrEmpty(sModifyDateStart))
                    {
                        rCreateDateStart = SqlFunc.ToDate(sModifyDateStart);
                    }
                    if (!string.IsNullOrEmpty(sModifyDateEnd))
                    {
                        rCreateDateEnd = SqlFunc.ToDate(sModifyDateEnd).AddDays(1);
                    }

                    var spOrgID = new SugarParameter("@OrgID", i_crm.ORIGID);
                    var spUserID = new SugarParameter("@UserID", i_crm.USERID);
                    pml.DataList = db.Queryable<OTB_OPM_BillChangeLog>()
                        .Where((t1) => t1.OrgID == i_crm.ORIGID)
                        .WhereIF(!string.IsNullOrEmpty(sBillNO), (t1) => t1.BillNO.Contains(sBillNO))
                        .WhereIF(!string.IsNullOrEmpty(sModifyDateStart), (t1) => SqlFunc.ToDate(t1.ModifyDate) >= rCreateDateStart.Date)
                        .WhereIF(!string.IsNullOrEmpty(sModifyDateEnd), (t1) => SqlFunc.ToDate(t1.ModifyDate) <= rCreateDateEnd.Date)
                        .WhereIF(!string.IsNullOrEmpty(Operation), (t1) => t1.Operation.Contains(Operation))
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
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(BillStatus_QryService), "", "QueryPage（賬單狀態（分頁查詢））", "", "", "");
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

        #endregion 賬單狀態（分頁查詢）
    }
}