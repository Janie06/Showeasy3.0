using EasyBL.WebApi.Message;
using Entity.Sugar;
using SqlSugar;
using SqlSugar.Base;
using System;

namespace EasyBL.WEBAPP.WSM
{
    public class Task_UpdService : ServiceBase
    {
        #region 代辦管理（單筆查詢）

        /// <summary>
        /// 代辦管理（單筆查詢）
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
                    var sEventID = _fetchString(i_crm, @"EventID");

                    var oEntity = db.Queryable<OTB_SYS_Task, OTB_SYS_Members, OTB_SYS_Members, OTB_SYS_Members>
                        ((t1, t2, t3, t4) =>
                        new object[] {
                                JoinType.Left, t1.OrgID == t2.OrgID && t1.CreateUser == t2.MemberID,
                                JoinType.Left, t1.OrgID == t3.OrgID && t1.ModifyUser == t3.MemberID,
                                JoinType.Inner, t1.OrgID == t4.OrgID && t1.Owner == t4.MemberID
                              }
                        )
                        .Where((t1, t2, t3, t4) => t1.OrgID == i_crm.ORIGID && t1.EventID == sEventID)
                        .Select((t1, t2, t3, t4) => new OTB_SYS_Task
                        {
                            EventID = SqlFunc.GetSelfAndAutoFill(t1.EventID),
                            CreateUserName = t2.MemberName,
                            ModifyUserName = t3.MemberName,
                            ExFeild1 = t4.MemberName
                        })
                        .Single();
                    var saTaskReply = db.Queryable<OTB_SYS_TaskReply>().Where(x => x.EventID == sEventID).ToList();
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, oEntity);
                    rm.DATA.Add("taskreply", saTaskReply);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Task_UpdService), "", "QueryOne（代辦管理（單筆查詢））", "", "", "");
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

        #endregion 代辦管理（單筆查詢）

        #region 代辦管理（新增）

        /// <summary>
        /// 代辦管理（新增）
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
                        var oEntity = _fetchEntity<OTB_SYS_Task>(i_crm);
                        _setEntityBase(oEntity, i_crm);
                        var iRel = db.Insertable(oEntity).ExecuteCommand();
                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, iRel);
                    } while (false);

                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Task_UpdService), @"代辦管理", @"Add（代辦管理（新增））", @"", @"", @"");
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

        #endregion 代辦管理（新增）

        #region 代辦管理（修改）

        /// <summary>
        /// 代辦管理（修改）
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
                        var oNewEntity = _fetchEntity<OTB_SYS_Task>(i_crm);
                        _setEntityBase(oNewEntity, i_crm);
                        var iRel = db.Updateable(oNewEntity)
                            .IgnoreColumns(x => new
                            {
                                x.CreateUser,
                                x.CreateDate
                            }).ExecuteCommand();

                        var sReplyStatus = _fetchString(i_crm, @"ReplyStatus");
                        var sReplyContent = _fetchString(i_crm, @"ReplyContent");
                        if (!string.IsNullOrEmpty(sReplyContent))
                        {
                            var oTaskReply = new OTB_SYS_TaskReply
                            {
                                EventID = oNewEntity.EventID,
                                ReplyDate = DateTime.Now,
                                ReplyStatus = sReplyStatus,
                                ReplyContent = sReplyContent
                            };
                            _setEntityBase(oTaskReply, i_crm);
                            iRel += db.Insertable(oTaskReply).ExecuteCommand();
                        }

                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, iRel);
                    } while (false);

                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Task_UpdService), @"代辦管理", @"Update（代辦管理（修改））", @"", @"", @"");
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

        #endregion 代辦管理（修改）

        #region 代辦管理（刪除）

        /// <summary>
        /// 代辦管理（刪除）
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
                        var sEventID = _fetchString(i_crm, @"EventID");
                        var iRel = db.Deleteable<OTB_SYS_Task>().Where(x => x.OrgID == i_crm.ORIGID && x.EventID == sEventID).ExecuteCommand();
                        iRel += db.Deleteable<OTB_SYS_TaskReply>().Where(x => x.EventID == sEventID).ExecuteCommand();
                        i_crm.DATA.Add("FileID", sEventID);
                        i_crm.DATA.Add("IDType", "parent");
                        new CommonService().DelFile(i_crm);
                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, iRel);
                    } while (false);

                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Task_UpdService), @"代辦管理", @"Delete（代辦管理（刪除））", @"", @"", @"");
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

        #endregion 代辦管理（刪除）
    }
}