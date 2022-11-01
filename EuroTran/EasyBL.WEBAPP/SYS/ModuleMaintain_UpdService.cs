using EasyBL.WebApi.Message;
using Entity.Sugar;
using SqlSugar;
using SqlSugar.Base;
using System;

namespace EasyBL.WEBAPP.WSM
{
    public class ModuleMaintain_UpdService : ServiceBase
    {
        #region 模組管理（單筆查詢）

        /// <summary>
        /// 模組管理（單筆查詢）
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
                    var sModuleID = _fetchString(i_crm, @"ModuleID");

                    var oEntity = db.Queryable<OTB_SYS_ModuleList, OTB_SYS_Members, OTB_SYS_Members>
                        ((t1, t2, t3) =>
                        new object[] {
                                JoinType.Left, t1.OrgID == t2.OrgID && t1.CreateUser == t2.MemberID,
                                JoinType.Left, t1.OrgID == t3.OrgID && t1.ModifyUser == t3.MemberID
                              }
                        )
                        .Where((t1, t2, t3) => t1.OrgID == i_crm.ORIGID && t1.ModuleID == sModuleID)
                        .Select((t1, t2, t3) => new OTB_SYS_ModuleList
                        {
                            ModuleID = SqlFunc.GetSelfAndAutoFill(t1.ModuleID),
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
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ArgumentMaintain_UpdService), "", "QueryOne（模組管理（單筆查詢））", "", "", "");
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

        #endregion 模組管理（單筆查詢）

        #region 模組管理（新增）

        /// <summary>
        /// 模組管理（新增）
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
                        var oEntity = _fetchEntity<OTB_SYS_ModuleList>(i_crm);
                        _setEntityBase(oEntity, i_crm);
                        oEntity.ShowTop = oEntity.ShowTop ?? false;
                        var iOldCout = db.Queryable<OTB_SYS_ModuleList>().Count(x => x.OrgID == i_crm.ORIGID);

                        if (oEntity.OrderByValue <= iOldCout)
                        {
                            var iRelUp = db.Updateable<OTB_SYS_ModuleList>()
                                            .UpdateColumns(x => new OTB_SYS_ModuleList { OrderByValue = x.OrderByValue + 1 })
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
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ModuleMaintain_UpdService), @"模組管理", @"Add（模組管理（新增））", @"", @"", @"");
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

        #endregion 模組管理（新增）

        #region 模組管理（修改）

        /// <summary>
        /// 模組管理（修改）
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
                        var oNewEntity = _fetchEntity<OTB_SYS_ModuleList>(i_crm);
                        _setEntityBase(oNewEntity, i_crm);
                        oNewEntity.ShowTop = oNewEntity.ShowTop ?? false;
                        var iOldEntity = db.Queryable<OTB_SYS_ModuleList>().Single(x => x.OrgID == i_crm.ORIGID && x.ModuleID == oNewEntity.ModuleID);

                        if (oNewEntity.OrderByValue > iOldEntity.OrderByValue)
                        {
                            var iRelUp = db.Updateable<OTB_SYS_ModuleList>()
                                            .UpdateColumns(x => new OTB_SYS_ModuleList { OrderByValue = x.OrderByValue - 1 })
                                            .Where(x => x.OrgID == oNewEntity.OrgID && x.OrderByValue <= oNewEntity.OrderByValue && x.OrderByValue > iOldEntity.OrderByValue).ExecuteCommand();
                        }
                        else
                        {
                            var iRelDown = db.Updateable<OTB_SYS_ModuleList>()
                                            .UpdateColumns(x => new OTB_SYS_ModuleList { OrderByValue = x.OrderByValue + 1 })
                                            .Where(x => x.OrgID == oNewEntity.OrgID && x.OrderByValue >= oNewEntity.OrderByValue && x.OrderByValue < iOldEntity.OrderByValue).ExecuteCommand();
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
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ModuleMaintain_UpdService), @"模組管理", @"Update（模組管理（修改））", @"", @"", @"");
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

        #endregion 模組管理（修改）

        #region 模組管理（刪除）

        /// <summary>
        /// 模組管理（刪除）
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
                        var sModuleID = _fetchString(i_crm, @"ModuleID");
                        var iExsitChild = db.Queryable<OTB_SYS_ModuleForProgram>().Count(x => x.OrgID == i_crm.ORIGID && x.ModuleID == sModuleID);
                        if (iExsitChild > 0)
                        {
                            sMsg = "message.ToDelPrograms";//請先刪除該模組下的程式資料
                            break;
                        }
                        var oEntity = db.Queryable<OTB_SYS_ModuleList>().Single(x => x.OrgID == i_crm.ORIGID && x.ModuleID == sModuleID);
                        var iRel = db.Deleteable<OTB_SYS_ModuleList>().Where(x => x.ModuleID == sModuleID).ExecuteCommand();
                        var iRelUp = db.Updateable<OTB_SYS_ModuleList>()
                                        .UpdateColumns(x => new OTB_SYS_ModuleList { OrderByValue = x.OrderByValue - 1 })
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
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ModuleMaintain_UpdService), @"模組管理", @"Delete（模組管理（刪除））", @"", @"", @"");
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

        #endregion 模組管理（刪除）

        #region 模組管理（查詢筆數）

        /// <summary>
        /// 模組管理（查詢筆數）
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
                    var sModuleID = _fetchString(i_crm, @"ModuleID");
                    var iCout = db.Queryable<OTB_SYS_ModuleList>()
                                  .WhereIF(!string.IsNullOrEmpty(sModuleID), x => x.ModuleID == sModuleID)
                                  .Count(x => x.OrgID == i_crm.ORIGID);

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iCout);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ArgumentClassMaintain_UpdService), "", "QueryCout（模組管理（查詢筆數））", "", "", "");
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

        #endregion 模組管理（查詢筆數）
    }
}