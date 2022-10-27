using EasyBL.WebApi.Message;
using EasyNet;
using Entity;
using Entity.Sugar;
using JumpKick.HttpLib;
using SqlSugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EasyBL.WEBAPP.EIP
{
    public class LeaveRequest_UpdService : ServiceBase
    {
        #region 請假區間編輯（單筆查詢）

        /// <summary>
        /// 請假區間編輯（單筆查詢）
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage QueryOne(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sId = _fetchString(i_crm, @"Guid");

                    var oEntity = db.Queryable<OTB_EIP_LeaveRequest>().Single(x => x.OrgID == i_crm.ORIGID && x.guid == sId);

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, oEntity);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(LeaveRequest_UpdService), "", "QueryOne（請假區間設定（單筆查詢））", "", "", "");
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

        #endregion 請假區間編輯（單筆查詢）

        #region 請假區間編輯（新增）

        /// <summary>
        /// 請假區間編輯（新增）
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on UpdImportCustomers</param>
        /// <returns></returns>
        public ResponseMessage Insert(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        string sGUID = Guid.NewGuid().ToString();
                        //客戶資料表身
                        var oEntity = _fetchEntity<OTB_EIP_LeaveRequest>(i_crm);
                        _setEntityBase(oEntity, i_crm);
                        oEntity.guid = sGUID;
                        oEntity.UsedHours = 0;
                        oEntity.RemainHours = oEntity.PaymentHours;
                        oEntity.DelFlag = false;
                        var iRel = db.Insertable(oEntity).ExecuteReturnEntity();
                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, iRel);
                    } while (false);

                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(LeaveRequest_UpdService), @"請假區間編輯", @"Add（請假區間編輯（新增））", @"", @"", @"");
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

        #endregion 請假區間編輯（新增）

        #region 請假區間編輯（修改）

        /// <summary>
        /// 請假區間編輯（修改）
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on UpdImportCustomers</param>
        /// <returns></returns>
        public ResponseMessage Update(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var sId = _fetchString(i_crm, @"guid");
                        var sUpdayOur = _fetchString(i_crm, @"UpdLeaveHours");
                        decimal.TryParse(sUpdayOur, out var dUpdLeaveHours);
                        var oNewEntity = _fetchEntity<OTB_EIP_LeaveRequest>(i_crm);
                        _setEntityBase(oNewEntity, i_crm);
                        if (oNewEntity.PaymentHours == null)
                            oNewEntity.PaymentHours = 0;
                        if (oNewEntity.RemainHours == null)
                            oNewEntity.RemainHours = 0;
                        if (oNewEntity.UsedHours == null)
                            oNewEntity.UsedHours = 0;
                        string sOrgId = oNewEntity.OrgID;
                        oNewEntity.PaymentHours += dUpdLeaveHours;
                        oNewEntity.RemainHours += dUpdLeaveHours;
                        var iRel = db.Updateable(oNewEntity)
                                     .IgnoreColumns(x => new
                                     {
                                         x.CreateUser,
                                         x.CreateDate
                                     }).ExecuteCommand();

                        var NewResult = db.Queryable<OTB_EIP_LeaveRequest>()
                                .Where(p => p.OrgID == sOrgId && p.guid == oNewEntity.guid)
                                .Single();

                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, NewResult);
                    } while (false);

                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(LeaveRequest_UpdService), @"請假區間編輯", @"Update（請假區間編輯（修改））", @"", @"", @"");
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

        #endregion 請假區間編輯（修改）

        #region 請假區間編輯（刪除）

        /// <summary>
        /// 請假區間編輯（刪除）
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on UpdImportCustomers</param>
        /// <returns></returns>
        public ResponseMessage Delete(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var sId = _fetchString(i_crm, @"Guid");
                        var iRel = db.Deleteable<OTB_EIP_LeaveRequest>().Where(x => x.guid == sId).ExecuteCommand();
                        var iMstRel = db.Deleteable<OTB_EIP_LeaveRequest>().Where(x => x.guid == sId).ExecuteCommand();
                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, iRel);
                    } while (false);

                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(LeaveRequest_UpdService), @"請假區間編輯", @"Delete（請假區間編輯（刪除））", @"", @"", @"");
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

        #endregion 請假區間編輯（刪除）
        
    }
}