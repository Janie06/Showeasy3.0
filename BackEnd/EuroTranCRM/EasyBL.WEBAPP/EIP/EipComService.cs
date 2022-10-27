using EasyBL.WebApi.Message;
using Entity.Sugar;
using SqlSugar;
using SqlSugar.Base;
using System;

namespace EasyBL.WEBAPP.EIP
{
    public class EipComService : ServiceBase
    {
        #region 通過帳單號碼抓去專案代號

        /// <summary>
        /// 通過帳單號碼抓去專案代號
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetPrjCodeByBillNO(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.DB;
            try
            {
                do
                {
                    var sBillNO = _fetchString(i_crm, @"BillNO");
                    var oBillInfo = db.Queryable<OTB_OPM_BillInfo, OTB_OPM_Exhibition>((t1, t2) => t1.ExhibitionNO == t2.SN.ToString())
                        .Where((t1, t2) => t1.OrgID == i_crm.ORIGID && t1.BillNO == sBillNO)
                        .Select((t1, t2) => new { t1.BillNO, PrjCode = t2.ExhibitionCode }).Single();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, oBillInfo);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(EipComService), @"EIPCOMMON", @"GetPrjCodeByBillNO（通過帳單號碼抓去專案代號）", @"", @"", @"");
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

        #endregion 通過帳單號碼抓去專案代號

        #region 抓去個人主頁請假資訊

        /// <summary>
        /// 抓去個人主頁請假資訊
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetLeavelist(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.DB;
            try
            {
                do
                {
                    var rTomorrow = DateTime.Now.AddDays(2);
                    var saLeave = db.Queryable<OTB_EIP_Leave, OTB_SYS_Members>((a, b) =>
                    new object[] {
                          JoinType.Inner,a.OrgID==b.OrgID && a.AskTheDummy==b.MemberID
                    }).OrderBy(a => a.StartDate)
                        .Where((a, b) => a.OrgID == i_crm.ORIGID && a.EndDate.Value >= DateTime.Now.Date && a.StartDate.Value <= rTomorrow.Date)
                        .Where((a, b) => @"B,H-O,E".Contains(a.Status))
                        .Select((a, b) => new { Info = a, AskTheDummyName = b.MemberName, b.MemberPic, b.OrgID }).ToList();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, saLeave);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(EipComService), @"EIPCOMMON", @"GetLeavelist（抓去個人主頁請假資訊）", @"", @"", @"");
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

        #endregion 抓去個人主頁請假資訊

        #region 獲取簽核流程

        /// <summary>
        /// 獲取簽核流程
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on GetFlows</param>
        /// <returns></returns>
        public ResponseMessage GetFlows(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sFlow_Type = _fetchString(i_crm, @"Flow_Type");
                    var sShareTo = _fetchString(i_crm, @"ShareTo");
                    var saCheckFlow = db.Queryable<OTB_EIP_CheckFlow>()
                          .OrderBy(x => x.ModifyDate)
                          .Where(x => x.OrgID == i_crm.ORIGID)
                          .WhereIF(!string.IsNullOrEmpty(sFlow_Type), x => x.Flow_Type == sFlow_Type)
                          .WhereIF(!string.IsNullOrEmpty(sShareTo), x => x.ShareTo.Contains(sShareTo))
                          .Select(x => new
                          {
                              x.Guid,
                              x.Flow_Name,
                              x.Flows
                          })
                          .ToList();
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, saCheckFlow);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(EipComService), @"EIPCOMMON", @"GetFlows（獲取簽核流程）", @"", @"", @"");
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

        #endregion 獲取簽核流程
    }
}