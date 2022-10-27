using EasyBL.WebApi.Message;
using Entity.Sugar;
using SqlSugar;
using SqlSugar.Base;
using System;

namespace EasyBL.WEBAPP.WSM
{
    public class DepartmentsMaintain_UpdService : ServiceBase
    {
        #region 部門管理（單筆查詢）

        /// <summary>
        /// 部門管理（單筆查詢）
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
                    var sDepartmentID = _fetchString(i_crm, @"DepartmentID");

                    var oEntity = db.Queryable<OTB_SYS_Departments, OTB_SYS_Members, OTB_SYS_Members>
                        ((t1, t2, t3) =>
                        new object[] {
                                JoinType.Left, t1.OrgID == t2.OrgID && t1.CreateUser == t2.MemberID,
                                JoinType.Left, t1.OrgID == t3.OrgID && t1.ModifyUser == t3.MemberID
                              }
                        )
                        .Where((t1, t2, t3) => t1.OrgID == i_crm.ORIGID && t1.DepartmentID == sDepartmentID)
                        .Select((t1, t2, t3) => new OTB_SYS_Departments
                        {
                            DepartmentID = SqlFunc.GetSelfAndAutoFill(t1.DepartmentID),
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
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ArgumentMaintain_UpdService), "", "QueryOne（部門管理（單筆查詢））", "", "", "");
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

        #endregion 部門管理（單筆查詢）

        #region 部門管理（新增）

        /// <summary>
        /// 部門管理（新增）
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
                        var oEntity = _fetchEntity<OTB_SYS_Departments>(i_crm);
                        _setEntityBase(oEntity, i_crm);
                        oEntity.DelStatus = "N";
                        var iOldCout = db.Queryable<OTB_SYS_Departments>().Count(x => x.OrgID == i_crm.ORIGID && x.DelStatus == "N");

                        if (oEntity.OrderByValue <= iOldCout)
                        {
                            var iRelUp = db.Updateable<OTB_SYS_Departments>()
                                            .UpdateColumns(x => new OTB_SYS_Departments { OrderByValue = x.OrderByValue + 1 })
                                            .Where(x => x.OrgID == i_crm.ORIGID && x.DelStatus == "N" && x.OrderByValue >= oEntity.OrderByValue)
                                            .ExecuteCommand();
                        }

                        var oEntity_Del = db.Queryable<OTB_SYS_Departments>()
                                            .Single(x => x.OrgID == i_crm.ORIGID && x.DepartmentID == oEntity.DepartmentID && x.DelStatus == "Y");
                        var iRel = 0;
                        if (oEntity_Del != null)
                        {
                            oEntity_Del.OrderByValue = oEntity.OrderByValue;
                            oEntity_Del.LevelOfDepartment = oEntity.LevelOfDepartment;
                            oEntity_Del.DelStatus = "N";
                            oEntity_Del.Memo = oEntity.Memo;
                            oEntity_Del.DepartmentName = oEntity.DepartmentName;
                            oEntity_Del.DepartmentShortName = oEntity.DepartmentShortName;
                            oEntity_Del.ChiefOfDepartmentID = oEntity.ChiefOfDepartmentID;
                            oEntity_Del.ParentDepartmentID = oEntity.ParentDepartmentID;
                            oEntity_Del.Effective = oEntity.Effective;
                            iRel = db.Updateable(oEntity_Del)
                                     .IgnoreColumns(x => new
                                     {
                                         x.CreateUser,
                                         x.CreateDate
                                     }).ExecuteCommand();
                        }
                        else
                        {
                            iRel = db.Insertable(oEntity).ExecuteCommand();
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
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(DepartmentsMaintain_UpdService), @"部門管理", @"Add（部門管理（新增））", @"", @"", @"");
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

        #endregion 部門管理（新增）

        #region 部門管理（修改）

        /// <summary>
        /// 部門管理（修改）
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
                        var oNewEntity = _fetchEntity<OTB_SYS_Departments>(i_crm);
                        oNewEntity.DelStatus = "N";
                        _setEntityBase(oNewEntity, i_crm);
                        var iOldEntity = db.Queryable<OTB_SYS_Departments>().Single(x => x.OrgID == i_crm.ORIGID && x.DepartmentID == oNewEntity.DepartmentID);

                        if (oNewEntity.OrderByValue > iOldEntity.OrderByValue)
                        {
                            var iRelUp = db.Updateable<OTB_SYS_Departments>()
                                            .UpdateColumns(x => new OTB_SYS_Departments { OrderByValue = x.OrderByValue - 1 })
                                            .Where(x => x.OrgID == oNewEntity.OrgID && x.DelStatus == "N" && x.OrderByValue <= oNewEntity.OrderByValue && x.OrderByValue > iOldEntity.OrderByValue).ExecuteCommand();
                        }
                        else
                        {
                            var iRelDown = db.Updateable<OTB_SYS_Departments>()
                                            .UpdateColumns(x => new OTB_SYS_Departments { OrderByValue = x.OrderByValue + 1 })
                                            .Where(x => x.OrgID == oNewEntity.OrgID && x.DelStatus == "N" && x.OrderByValue >= oNewEntity.OrderByValue && x.OrderByValue < iOldEntity.OrderByValue).ExecuteCommand();
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
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(DepartmentsMaintain_UpdService), @"部門管理", @"Update（部門管理（修改））", @"", @"", @"");
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

        #endregion 部門管理（修改）

        #region 部門管理（刪除）

        /// <summary>
        /// 部門管理（刪除）
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
                        var sDepartmentID = _fetchString(i_crm, @"DepartmentID");
                        var iExsitChild = db.Queryable<OTB_SYS_Members>().Count(x => x.OrgID == i_crm.ORIGID && x.MemberID == sDepartmentID);
                        if (iExsitChild > 0)
                        {
                            sMsg = "message.ToDelMembers";//請先刪除該部門下的人員資料
                            break;
                        }
                        var oEntity = db.Queryable<OTB_SYS_Departments>().Single(x => x.OrgID == i_crm.ORIGID && x.DepartmentID == sDepartmentID);
                        var iRel = db.Updateable<OTB_SYS_Departments>()
                                        .UpdateColumns(x => new OTB_SYS_Departments { DelStatus = "Y" })
                                        .Where(x => x.OrgID == i_crm.ORIGID && x.DepartmentID == sDepartmentID);
                        var iRelUp = db.Updateable<OTB_SYS_Departments>()
                                        .UpdateColumns(x => new OTB_SYS_Departments { OrderByValue = x.OrderByValue - 1 })
                                        .Where(x => x.OrgID == oEntity.OrgID && x.DelStatus == "N" && x.OrderByValue > oEntity.OrderByValue)
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
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(DepartmentsMaintain_UpdService), @"部門管理", @"Delete（部門管理（刪除））", @"", @"", @"");
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

        #endregion 部門管理（刪除）

        #region 部門管理（查詢筆數）

        /// <summary>
        /// 部門管理（查詢筆數）
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
                    var sDepartmentID = _fetchString(i_crm, @"DepartmentID");
                    var iCout = db.Queryable<OTB_SYS_Departments>()
                        .Where(x => x.OrgID == i_crm.ORIGID && x.DelStatus == "N")
                        .WhereIF(!string.IsNullOrEmpty(sDepartmentID), x => x.DepartmentID == sDepartmentID)
                        .Count();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iCout);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(DepartmentsMaintain_UpdService), "", "QueryCout（部門管理（查詢筆數））", "", "", "");
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

        #endregion 部門管理（查詢筆數）
    }
}