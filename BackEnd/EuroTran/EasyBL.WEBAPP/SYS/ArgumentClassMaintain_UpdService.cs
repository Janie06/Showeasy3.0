using EasyBL.WebApi.Message;
using Entity.Sugar;
using SqlSugar;
using SqlSugar.Base;
using System;

namespace EasyBL.WEBAPP.WSM
{
    public class ArgumentClassMaintain_UpdService : ServiceBase
    {
        #region 參數類別管理（單筆查詢）

        /// <summary>
        /// 參數類別管理（單筆查詢）
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
                    var sArgumentClassID = _fetchString(i_crm, @"ArgumentClassID");

                    var oEntity = db.Queryable<OTB_SYS_ArgumentClass, OTB_SYS_Members, OTB_SYS_Members>
                        ((t1, t2, t3) =>
                        new object[] {
                                JoinType.Left, t1.OrgID == t2.OrgID && t1.CreateUser == t2.MemberID,
                                JoinType.Left, t1.OrgID == t3.OrgID && t1.ModifyUser == t3.MemberID
                              }
                        )
                        .Where((t1, t2, t3) => t1.OrgID == i_crm.ORIGID && t1.ArgumentClassID == sArgumentClassID)
                        .Select((t1, t2, t3) => new OTB_SYS_ArgumentClass
                        {
                            ArgumentClassID = SqlFunc.GetSelfAndAutoFill(t1.ArgumentClassID),
                            CreateUserName = t2.MemberName,
                            ModifyUserName = t3.MemberName
                        })
                        .Single();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, oEntity);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ArgumentMaintain_UpdService), "", "QueryOne（參數類別管理（單筆查詢））", "", "", "");
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

        #endregion 參數類別管理（單筆查詢）

        #region 參數類別管理（新增）

        /// <summary>
        /// 參數類別管理（新增）
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
                        var oEntity = _fetchEntity<OTB_SYS_ArgumentClass>(i_crm);
                        _setEntityBase(oEntity, i_crm);
                        oEntity.DelStatus = "N";
                        var iOldCout = db.Queryable<OTB_SYS_ArgumentClass>().Count(x => x.OrgID == i_crm.ORIGID && x.DelStatus == "N");

                        if (oEntity.OrderByValue <= iOldCout)
                        {
                            var iRelUp = db.Updateable<OTB_SYS_ArgumentClass>()
                                            .UpdateColumns(x => new OTB_SYS_ArgumentClass { OrderByValue = x.OrderByValue + 1 })
                                            .Where(x => x.OrgID == i_crm.ORIGID && x.OrderByValue >= oEntity.OrderByValue)
                                            .ExecuteCommand();
                        }

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
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ArgumentClassMaintain_UpdService), @"參數類別管理", @"Add（參數類別管理（新增））", @"", @"", @"");
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

        #endregion 參數類別管理（新增）

        #region 參數類別管理（修改）

        /// <summary>
        /// 參數類別管理（修改）
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
                        var oNewEntity = _fetchEntity<OTB_SYS_ArgumentClass>(i_crm);
                        _setEntityBase(oNewEntity, i_crm);
                        oNewEntity.DelStatus = "N";
                        var oOldEntity = db.Queryable<OTB_SYS_ArgumentClass>().Single(x => x.OrgID == i_crm.ORIGID && x.ArgumentClassID == oNewEntity.ArgumentClassID);

                        if (oNewEntity.OrderByValue > oOldEntity.OrderByValue)
                        {
                            var iRelUp = db.Updateable<OTB_SYS_ArgumentClass>()
                                            .UpdateColumns(x => new OTB_SYS_ArgumentClass { OrderByValue = x.OrderByValue - 1 })
                                            .Where(x => x.OrgID == oNewEntity.OrgID && x.OrderByValue <= oNewEntity.OrderByValue && x.OrderByValue > oOldEntity.OrderByValue).ExecuteCommand();
                        }
                        else
                        {
                            var iRelDown = db.Updateable<OTB_SYS_ArgumentClass>()
                                            .UpdateColumns(x => new OTB_SYS_ArgumentClass { OrderByValue = x.OrderByValue + 1 })
                                            .Where(x => x.OrgID == oNewEntity.OrgID && x.OrderByValue >= oNewEntity.OrderByValue && x.OrderByValue < oOldEntity.OrderByValue).ExecuteCommand();
                        }

                        var iRel = db.Updateable(oNewEntity)
                            .IgnoreColumns(x => new
                            {
                                x.CreateUser,
                                x.CreateDate
                            }).ExecuteCommand();
                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, iRel);
                    } while (false);

                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ArgumentClassMaintain_UpdService), @"參數類別管理", @"Update（參數類別管理（修改））", @"", @"", @"");
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

        #endregion 參數類別管理（修改）

        #region 參數類別管理（刪除）

        /// <summary>
        /// 參數類別管理（刪除）
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
                        var sArgumentClassID = _fetchString(i_crm, @"ArgumentClassID");
                        var iExsitChild = db.Queryable<OTB_SYS_Arguments>().Count(x => x.OrgID == i_crm.ORIGID && x.ArgumentClassID == sArgumentClassID);
                        if (iExsitChild > 0)
                        {
                            sMsg = "message.ToDelArguments";//請先刪除該類別下邊參數值
                            break;
                        }
                        var oEntity = db.Queryable<OTB_SYS_ArgumentClass>().Single(x => x.OrgID == i_crm.ORIGID && x.ArgumentClassID == sArgumentClassID);
                        var iRel = db.Deleteable<OTB_SYS_ArgumentClass>().Where(x => x.OrgID == i_crm.ORIGID && x.ArgumentClassID == sArgumentClassID).ExecuteCommand();
                        var iRelUp = db.Updateable<OTB_SYS_ArgumentClass>()
                                       .UpdateColumns(x => new OTB_SYS_ArgumentClass { OrderByValue = x.OrderByValue - 1 })
                                       .Where(x => x.OrgID == oEntity.OrgID && x.OrderByValue > oEntity.OrderByValue)
                                       .ExecuteCommand();
                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, iRel);
                    } while (false);

                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ArgumentClassMaintain_UpdService), @"參數類別管理", @"Delete（參數類別管理（刪除））", @"", @"", @"");
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

        #endregion 參數類別管理（刪除）

        #region 參數類別管理（查詢筆數）

        /// <summary>
        /// 參數類別管理（查詢筆數）
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage QueryCout(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sArgumentClassID = _fetchString(i_crm, @"ArgumentClassID");
                    var iCout = db.Queryable<OTB_SYS_ArgumentClass>()
                        .Where(x => x.OrgID == i_crm.ORIGID && x.DelStatus == "N")
                        .WhereIF(!string.IsNullOrEmpty(sArgumentClassID), x => x.ArgumentClassID == sArgumentClassID)
                        .Count();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iCout);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ArgumentClassMaintain_UpdService), "", "QueryCout（參數類別管理（查詢筆數））", "", "", "");
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

        #endregion 參數類別管理（查詢筆數）
    }
}