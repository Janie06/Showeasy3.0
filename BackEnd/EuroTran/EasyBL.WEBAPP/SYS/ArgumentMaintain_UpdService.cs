using EasyBL.WebApi.Message;
using Entity.Sugar;
using SqlSugar;
using SqlSugar.Base;
using System;

namespace EasyBL.WEBAPP.WSM
{
    public class ArgumentMaintain_UpdService : ServiceBase
    {
        #region 參數值（單筆查詢）

        /// <summary>
        /// 參數值（單筆查詢）
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
                    var sArgumentID = _fetchString(i_crm, @"ArgumentID");

                    var oEntity = db.Queryable<OTB_SYS_Arguments, OTB_SYS_Members, OTB_SYS_Members>
                        ((t1, t2, t3) =>
                        new object[] {
                                JoinType.Left, t1.OrgID == t2.OrgID && t1.CreateUser == t2.MemberID,
                                JoinType.Left, t1.OrgID == t3.OrgID && t1.ModifyUser == t3.MemberID
                              }
                        )
                        .Where((t1, t2, t3) => t1.OrgID == i_crm.ORIGID && t1.ArgumentClassID == sArgumentClassID && t1.ArgumentID == sArgumentID)
                        .Select((t1, t2, t3) => new OTB_SYS_Arguments
                        {
                            ArgumentID = SqlFunc.GetSelfAndAutoFill(t1.ArgumentID),
                            CreateUserName = t2.MemberName,
                            ModifyUserName = t3.MemberName
                        })
                        .Single();

                    //var json = db.Queryable<OTB_SYS_Profiles>().ToJson();
                    //var json2 = db.Queryable<OTB_SYS_Members>().ToList();
                    //var data = db.Queryable<OTB_SYS_Members>().Select<KeyValuePair<string, string>>("MemberID,MemberName").ToList();

                    //foreach (var item in json2)
                    //{
                    //    Console.WriteLine(item.MemberName);

                    // Console.WriteLine(item.Department.DepartmentName);

                    //    Console.WriteLine(item.Rules.Count);
                    //}

                    //var list = db.Queryable<OTB_SYS_Members>().Select<ExpandoObject>("*").ToList();
                    //foreach (var item in list)
                    //{
                    //    var dic = item.ToDictionary(x => x.Key, x => x.Value);
                    //    //dic的Key为列的名称, value为列的值
                    //}

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, oEntity);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ArgumentMaintain_UpdService), "", "QueryOne（參數值（單筆查詢））", "", "", "");
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

        #endregion 參數值（單筆查詢）

        #region 參數值（多筆）

        /// <summary>
        /// 參數值（多筆）
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage QueryList(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var saArgumentClass = db.Queryable<OTB_SYS_Arguments>()
                                            .Where(X => X.OrgID == i_crm.ORIGID && X.Effective == "Y" && X.DelStatus == "N").ToList();
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, saArgumentClass);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBAPP.SYS.Leave_QryService", "", "QueryList（參數值（多筆））", "", "", "");
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

        #endregion 參數值（多筆）

        #region 參數值（新增）

        /// <summary>
        /// 參數值（新增）
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
                        var oEntity = _fetchEntity<OTB_SYS_Arguments>(i_crm);
                        _setEntityBase(oEntity, i_crm);
                        oEntity.LevelOfArgument = 0;
                        if (!string.IsNullOrEmpty(oEntity.ParentArgument))
                        {
                            var oParent = db.Queryable<OTB_SYS_Arguments>()
                                            .Single(x => x.OrgID == i_crm.ORIGID && x.ArgumentClassID == oEntity.ArgumentClassID && x.ArgumentID == oEntity.ParentArgument);
                            oEntity.LevelOfArgument = oParent.LevelOfArgument + 1;
                        }
                        oEntity.DelStatus = "N";
                        var iOldCout = db.Queryable<OTB_SYS_Arguments>().Count(x => x.OrgID == i_crm.ORIGID && x.ArgumentClassID == oEntity.ArgumentClassID);

                        if (oEntity.OrderByValue <= iOldCout)
                        {
                            var iRelUp = db.Updateable<OTB_SYS_Arguments>()
                                            .UpdateColumns(x => new OTB_SYS_Arguments { OrderByValue = x.OrderByValue + 1 })
                                            .Where(x => x.OrgID == i_crm.ORIGID && x.ArgumentClassID == oEntity.ArgumentClassID && x.OrderByValue >= oEntity.OrderByValue)
                                            .ExecuteCommand();
                        }

                        var oEntity_Del = db.Queryable<OTB_SYS_Arguments>()
                                            .Single(x => x.OrgID == i_crm.ORIGID && x.ArgumentClassID == oEntity.ArgumentClassID && x.ArgumentID == oEntity.ArgumentID && x.DelStatus == "Y");
                        var iRel = 0;
                        if (oEntity_Del != null)
                        {
                            oEntity_Del.OrderByValue = oEntity.OrderByValue;
                            oEntity_Del.LevelOfArgument = oEntity.LevelOfArgument;
                            oEntity_Del.DelStatus = "N";
                            oEntity_Del.Memo = oEntity.Memo;
                            oEntity_Del.ArgumentValue = oEntity.ArgumentValue;
                            oEntity_Del.ArgumentValue_CN = oEntity.ArgumentValue_CN;
                            oEntity_Del.ArgumentValue_EN = oEntity.ArgumentValue_EN;
                            oEntity_Del.ParentArgument = oEntity.ParentArgument;
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
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ArgumentMaintain_UpdService), @"參數值", @"Add（參數值（新增））", @"", @"", @"");
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

        #endregion 參數值（新增）

        #region 參數值（修改）

        /// <summary>
        /// 參數值（修改）
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
                        var oNewEntity = _fetchEntity<OTB_SYS_Arguments>(i_crm);
                        _setEntityBase(oNewEntity, i_crm);
                        oNewEntity.DelStatus = "N";
                        oNewEntity.LevelOfArgument = 0;
                        if (!string.IsNullOrEmpty(oNewEntity.ParentArgument))
                        {
                            var oParent = db.Queryable<OTB_SYS_Arguments>()
                                            .Single(x => x.OrgID == i_crm.ORIGID && x.ArgumentClassID == oNewEntity.ArgumentClassID && x.ArgumentID == oNewEntity.ParentArgument);
                            oNewEntity.LevelOfArgument = oParent.LevelOfArgument + 1;
                        }
                        var oOldEntity = db.Queryable<OTB_SYS_Arguments>().Single(x => x.OrgID == i_crm.ORIGID && x.ArgumentClassID == oNewEntity.ArgumentClassID && x.ArgumentID == oNewEntity.ArgumentID);

                        if (oNewEntity.OrderByValue > oOldEntity.OrderByValue)
                        {
                            var iRelUp = db.Updateable<OTB_SYS_Arguments>()
                                            .UpdateColumns(x => new OTB_SYS_Arguments { OrderByValue = x.OrderByValue - 1 })
                                            .Where(x => x.OrgID == oNewEntity.OrgID && x.ArgumentClassID == oNewEntity.ArgumentClassID && x.OrderByValue <= oNewEntity.OrderByValue && x.OrderByValue > oOldEntity.OrderByValue).ExecuteCommand();
                        }
                        else
                        {
                            var iRelDown = db.Updateable<OTB_SYS_Arguments>()
                                            .UpdateColumns(x => new OTB_SYS_Arguments { OrderByValue = x.OrderByValue + 1 })
                                            .Where(x => x.OrgID == oNewEntity.OrgID && x.ArgumentClassID == oNewEntity.ArgumentClassID && x.OrderByValue >= oNewEntity.OrderByValue && x.OrderByValue < oOldEntity.OrderByValue).ExecuteCommand();
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
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ArgumentMaintain_UpdService), @"參數值", @"Update（參數值（修改））", @"", @"", @"");
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

        #endregion 參數值（修改）

        #region 參數值（刪除）

        /// <summary>
        /// 參數值（刪除）
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
                        var sArgumentID = _fetchString(i_crm, @"ArgumentID");
                        var oEntity = db.Queryable<OTB_SYS_Arguments>().Single(x => x.OrgID == i_crm.ORIGID && x.ArgumentClassID == sArgumentClassID && x.ArgumentID == sArgumentID);
                        var iRel = db.Updateable<OTB_SYS_Arguments>()
                                        .UpdateColumns(x => new OTB_SYS_Arguments { DelStatus = "Y" })
                                        .Where(x => x.OrgID == i_crm.ORIGID && x.ArgumentClassID == sArgumentClassID && x.ArgumentID == sArgumentID).ExecuteCommand();
                        var iRelUp = db.Updateable<OTB_SYS_Arguments>()
                                        .UpdateColumns(x => new OTB_SYS_Arguments { OrderByValue = x.OrderByValue - 1 })
                                        .Where(x => x.OrgID == oEntity.OrgID && x.ArgumentClassID == sArgumentClassID && x.OrderByValue > oEntity.OrderByValue)
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
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ArgumentMaintain_UpdService), @"參數值", @"Delete（參數值（刪除））", @"", @"", @"");
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

        #endregion 參數值（刪除）

        #region 參數值（查詢筆數）

        /// <summary>
        /// 參數值（查詢筆數）
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
                    var sArgumentID = _fetchString(i_crm, @"ArgumentID");
                    var iCout = db.Queryable<OTB_SYS_Arguments>()
                        .Where(x => x.OrgID == i_crm.ORIGID && x.DelStatus == "N")
                        .WhereIF(!string.IsNullOrEmpty(sArgumentClassID), x => x.ArgumentClassID == sArgumentClassID)
                        .WhereIF(!string.IsNullOrEmpty(sArgumentID), x => x.ArgumentID == sArgumentID)
                        .Count();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iCout);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ArgumentMaintain_UpdService), "", "QueryCout（參數值（查詢筆數））", "", "", "");
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

        #endregion 參數值（查詢筆數）
    }
}