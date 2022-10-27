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
    public class CrmComService : ServiceBase
    {
        #region 獲取客訴單號

        /// <summary>
        /// 獲取客訴單號
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on ComplaintNumber</param>
        /// <returns></returns>
        public ResponseMessage GetComplaintNumber(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.DB;
            try
            {
                do
                {
                    var sGuid = _fetchString(i_crm, @"Guid");

                    var listComplaint = db.Queryable<OTB_CRM_Complaint>()
                        .OrderBy(x => x.CreateDate)
                        .Where(x => x.DataType == "E" || x.DataType == "H-O")
                        .Select(x => new {
                            ComplaintNumber = x.ComplaintNumber
                        })
                        .ToList();
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, listComplaint);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(CrmComService), @"進出口管理模組", @"GetComplaintNumber（獲取客訴單號）", @"", @"", @"");
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

        #endregion 獲取客訴單號
    }
}
