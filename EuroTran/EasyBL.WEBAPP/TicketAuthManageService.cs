using EasyBL.WebApi.Message;
using Entity.Sugar;
using SqlSugar;
using SqlSugar.Base;
using System;

namespace EasyBL.WEBAPP
{
    public class TicketAuthManageService : ServiceBase
    {
        #region 授權管理（分頁查詢）

        /// <summary>
        /// 授權管理（分頁查詢）
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

                    var sUserId = _fetchString(i_crm, @"UserId");
                    var sUserName = _fetchString(i_crm, @"UserName");
                    var sIsVerify = _fetchString(i_crm, @"IsVerify");
                    var bExcel = _fetchBool(i_crm, @"Excel");

                    pml.DataList = db.Queryable<OTB_SYS_TicketAuth, OTB_SYS_Organization>((t1, t2) => new object[] {
                                JoinType.Left, t1.OrgID == t2.OrgID
                    })
                        .Where((t1, t2) => (t1.OrgID == i_crm.ORIGID || !SqlFunc.HasValue(t1.OrgID)) && sIsVerify.Contains(t1.IsVerify))
                        .WhereIF(!string.IsNullOrEmpty(sUserId), (t1, t2) => t1.UserID == sUserId)
                        .WhereIF(!string.IsNullOrEmpty(sUserName), (t1, t2) => t1.UserName == sUserName)
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
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(TicketAuthManageService), "", "QueryPage（授權管理（分頁查詢））", "", "", "");
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

        #endregion 授權管理（分頁查詢）

        #region 授權管理（新增）

        /// <summary>
        /// 授權管理（新增）
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on InsertImportCustomers</param>
        /// <returns></returns>
        public ResponseMessage GridInsert(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var oEntity = _fetchEntity<OTB_SYS_TicketAuth>(i_crm);
                    _setEntityBase(oEntity, i_crm);
                    oEntity.Token = WebApi.Common.SignExtension.CreateToken();
                    oEntity.OrgID = "";
                    oEntity.LoginIp = @"";
                    oEntity.IsVerify = @"N";
                    oEntity.CreateTime = DateTime.Now;
                    oEntity.ExpireTime = DateTime.Now.AddDays(2); //兩天过期
                    var oTicket = db.Queryable<OTB_SYS_TicketAuth>().Single(x => !SqlFunc.HasValue(x.OrgID) && x.UserID == oEntity.UserID);

                    if (oTicket != null)
                    {
                        sMsg = @"當前帳號已被使用";
                        break;
                    }

                    var iRel = db.Insertable(oEntity).ExecuteCommand();
                    if (iRel <= 0)
                    {
                        sMsg = @"新增失敗";
                        break;
                    }

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iRel);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);

                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(TicketAuthManageService), @"授權管理", @"Add（授權管理（新增））", @"", @"", @"");
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

        #endregion 授權管理（新增）

        #region 授權管理（修改）

        /// <summary>
        /// 授權管理（修改）
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on UpdImportCustomers</param>
        /// <returns></returns>
        public ResponseMessage GridUpdate(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var oEntity = _fetchEntity<OTB_SYS_TicketAuth>(i_crm);
                    _setEntityBase(oEntity, i_crm);
                    var iRel = db.Updateable(oEntity)
                        .UpdateColumns(x => new
                        {
                            x.UserID,
                            x.UserName,
                        }).ExecuteCommand();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iRel);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(TicketAuthManageService), @"授權管理", @"Update（授權管理（修改））", @"", @"", @"");
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

        #endregion 授權管理（修改）

        #region 授權管理（刪除）

        /// <summary>
        /// 授權管理（刪除）
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on UpdImportCustomers</param>
        /// <returns></returns>
        public ResponseMessage GridDelete(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var iNO = _fetchInt(i_crm, @"NO");

                    var iRel = db.Deleteable<OTB_SYS_TicketAuth>().Where(x => x.NO == iNO).ExecuteCommand();
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iRel);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(TicketAuthManageService), @"授權管理", @"Delete（授權管理（刪除））", @"", @"", @"");
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

        #endregion 授權管理（刪除）

        #region 重新產生Token和簽名

        /// <summary>
        /// 函式名稱:ReSetToken
        /// 函式說明:重新產生Token和簽名
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on ReSetToken</param>
        /// <returns>
        /// 回傳 rm(Object)
        ///</returns>
        public ResponseMessage ReSetToken(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sError = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sNO = _fetchString(i_crm, @"NO");
                    var ticket = new OTB_SYS_TicketAuth
                    {
                        Token = WebApi.Common.SignExtension.CreateToken(),
                        NO = int.Parse(sNO)
                    };
                    var iRel = db.Updateable(ticket).UpdateColumns(it => new { it.Token })
                        .Where(it => it.NO == ticket.NO).ExecuteCommand();
                    if (iRel <= 0)
                    {
                        sError = @"重新產生Token和簽名失敗";
                        break;
                    }
                    rm = new SuccessResponseMessage(null, i_crm);
                } while (false);
            }
            catch (Exception ex)
            {
                sError = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sError + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(TicketAuthManageService), nameof(ReSetToken), @"ReSetToken（重新產生Token和簽名）", @"", @"", @"");
            }
            finally
            {
                if (null != sError)
                {
                    rm = new ErrorResponseMessage(sError, i_crm);
                }
            }
            return rm;
        }

        #endregion 重新產生Token和簽名
    }
}