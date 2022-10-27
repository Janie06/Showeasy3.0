using EasyBL.WebApi.Message;
using Entity.Sugar;
using Newtonsoft.Json.Linq;
using SqlSugar;
using SqlSugar.Base;
using System;

namespace EasyBL.WEBAPP.SYS
{
    public class ProgramMaintain_UpdService : ServiceBase
    {
        #region 獲取模組信息

        /// <summary>
        /// 函式名稱:Login
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on GetModuleInfo</param>
        /// <returns>
        /// 回傳 rm(Object)
        ///</returns>
        public ResponseMessage GetModuleInfo(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sError = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sp = db.Ado.GetParameters(i_crm.DATA);
                    var moduleInfo = db.Ado.SqlQuery<ModuleInfo>(@"SELECT ModuleID,case when ModuleID=dbo.OFN_SYS_GetParentIDByModuleID(OrgID,ModuleID) then '' else  dbo.OFN_SYS_ModuleNameByModuleID(OrgID,dbo.OFN_SYS_GetParentIDByModuleID(OrgID,ModuleID))+'-' end +ModuleName as ModuleName,(SELECT COUNT(0) FROM OTB_SYS_ModuleForProgram WHERE ModuleID=M.ModuleID and OrgID=M.OrgID) AS PrgCount,  (SELECT  OrderByValue FROM OTB_SYS_ModuleForProgram  WHERE OrgID=m.OrgID AND ModuleID=m.ModuleID AND ProgramID=@ProgramID) AS OrderByValue  FROM  dbo.OTB_SYS_ModuleList M WHERE  OrgID=@OrgID AND CHARINDEX(','+M.ModuleID+',',','+@AllModuleID+',')>0", sp);
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, moduleInfo);
                } while (false);
            }
            catch (Exception ex)
            {
                sError = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sError + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ProgramMaintain_UpdService), @"", @"GetModuleInfo（獲取模組信息）", @"", @"", @"");
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

        #endregion 獲取模組信息

        #region 程式管理（單筆查詢）

        /// <summary>
        /// 程式管理（單筆查詢）
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
                    var sProgramID = _fetchString(i_crm, @"ProgramID");

                    var oEntity = db.Queryable<OTB_SYS_ProgramList, OTB_SYS_Members, OTB_SYS_Members>
                        ((t1, t2, t3) =>
                        new object[] {
                                JoinType.Left, t1.OrgID == t2.OrgID && t1.CreateUser == t2.MemberID,
                                JoinType.Left, t1.OrgID == t3.OrgID && t1.ModifyUser == t3.MemberID
                              }
                        )
                        .Where((t1, t2, t3) => t1.OrgID == i_crm.ORIGID && t1.ProgramID == sProgramID)
                        .Select((t1, t2, t3) => new OTB_SYS_ProgramList
                        {
                            ProgramID = SqlFunc.GetSelfAndAutoFill(t1.ProgramID),
                            CreateUserName = t2.MemberName,
                            ModifyUserName = t3.MemberName
                        })
                        .Single();
                    var saAuth = db.Queryable<OTB_SYS_Arguments>()
                                    .Where(x => x.OrgID == i_crm.ORIGID && x.ArgumentClassID == "99999" && x.Effective == "Y")
                                    .ToList();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, oEntity);
                    rm.DATA.Add("actions", saAuth);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ProgramMaintain_UpdService), "", "QueryOne（程式管理（單筆查詢））", "", "", "");
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

        #endregion 程式管理（單筆查詢）

        #region 程式管理（新增）

        /// <summary>
        /// 程式管理（新增）
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
                        var oEntity = _fetchEntity<OTB_SYS_ProgramList>(i_crm);
                        _setEntityBase(oEntity, i_crm);

                        var saUpdOrder = i_crm.DATA["updorder"] as JArray;

                        foreach (JObject updorder in saUpdOrder)
                        {
                            var sModuleID = updorder.GetValue("ModuleID").ToString();
                            var iOldOrderByValue = int.Parse(updorder.GetValue("OldOrderByValue").ToString());
                            var iNewOrderByValue = int.Parse(updorder.GetValue("NewOrderByValue").ToString());
                            if (iNewOrderByValue > iOldOrderByValue)
                            {
                                var iRelUp = db.Updateable<OTB_SYS_ModuleForProgram>()
                                               .UpdateColumns(x => new OTB_SYS_ModuleForProgram { OrderByValue = x.OrderByValue - 1 })
                                               .Where(x => x.OrgID == i_crm.ORIGID && x.ModuleID == sModuleID && x.OrderByValue <= iNewOrderByValue && x.OrderByValue > iOldOrderByValue).ExecuteCommand();
                            }
                            else
                            {
                                var iRelDown = db.Updateable<OTB_SYS_ModuleForProgram>()
                                                 .UpdateColumns(x => new OTB_SYS_ModuleForProgram { OrderByValue = x.OrderByValue + 1 })
                                                 .Where(x => x.OrgID == i_crm.ORIGID && x.ModuleID == sModuleID && x.OrderByValue >= iNewOrderByValue && x.OrderByValue < iOldOrderByValue).ExecuteCommand();
                            }
                            var oModuleForProgram = new OTB_SYS_ModuleForProgram
                            {
                                OrgID = i_crm.ORIGID,
                                ModuleID = sModuleID,
                                ProgramID = oEntity.ProgramID,
                                OrderByValue = iNewOrderByValue
                            };
                            _setEntityBase(oModuleForProgram, i_crm);
                            db.Insertable(oModuleForProgram).ExecuteCommand();
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
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ProgramMaintain_UpdService), @"程式管理", @"Add（程式管理（新增））", @"", @"", @"");
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

        #endregion 程式管理（新增）

        #region 程式管理（修改）

        /// <summary>
        /// 程式管理（修改）
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
                        var oNewEntity = _fetchEntity<OTB_SYS_ProgramList>(i_crm);
                        _setEntityBase(oNewEntity, i_crm);

                        var saUpdOrder = i_crm.DATA["updorder"] as JArray;

                        //更新和新增當前模組
                        foreach (JObject updorder in saUpdOrder)
                        {
                            var sModuleID = updorder.GetValue("ModuleID").ToString();
                            var iOldOrderByValue = int.Parse(updorder.GetValue("OldOrderByValue").ToString());
                            var iNewOrderByValue = int.Parse(updorder.GetValue("NewOrderByValue").ToString());
                            if (iNewOrderByValue > iOldOrderByValue)
                            {
                                var iRelUp = db.Updateable<OTB_SYS_ModuleForProgram>()
                                               .UpdateColumns(x => new OTB_SYS_ModuleForProgram { OrderByValue = x.OrderByValue - 1 })
                                               .Where(x => x.OrgID == i_crm.ORIGID && x.ModuleID == sModuleID && x.OrderByValue <= iNewOrderByValue && x.OrderByValue > iOldOrderByValue).ExecuteCommand();
                            }
                            else
                            {
                                var iRelDown = db.Updateable<OTB_SYS_ModuleForProgram>()
                                                 .UpdateColumns(x => new OTB_SYS_ModuleForProgram { OrderByValue = x.OrderByValue + 1 })
                                                 .Where(x => x.OrgID == i_crm.ORIGID && x.ModuleID == sModuleID && x.OrderByValue >= iNewOrderByValue && x.OrderByValue < iOldOrderByValue).ExecuteCommand();
                            }
                            var oModuleForProgram = new OTB_SYS_ModuleForProgram
                            {
                                OrgID = i_crm.ORIGID,
                                ModuleID = sModuleID,
                                ProgramID = oNewEntity.ProgramID,
                                OrderByValue = iNewOrderByValue
                            };
                            _setEntityBase(oModuleForProgram, i_crm);
                            var oOldEntity = db.Queryable<OTB_SYS_ModuleForProgram>()
                                             .Single(x => x.OrgID == i_crm.ORIGID && x.ModuleID == sModuleID && x.ProgramID == oNewEntity.ProgramID);
                            if (oOldEntity == null)
                            {
                                db.Insertable(oModuleForProgram).ExecuteCommand();
                            }
                            else
                            {
                                db.Updateable(oModuleForProgram)
                                  .IgnoreColumns(x => new
                                  {
                                      x.CreateUser,
                                      x.CreateDate
                                  }).ExecuteCommand();
                            }
                        }
                        //更新和刪除已刪除的模組
                        var saDelEntitys = db.Queryable<OTB_SYS_ModuleForProgram>()
                                             .Where(x => x.OrgID == i_crm.ORIGID && !oNewEntity.ModuleID.Contains(x.ModuleID) && x.ProgramID == oNewEntity.ProgramID)
                                             .ToList();
                        foreach (var item in saDelEntitys)
                        {
                            db.Deleteable<OTB_SYS_ModuleForProgram>()
                              .Where(x => x.OrgID == item.OrgID && x.ModuleID == item.ModuleID && x.ProgramID == item.ProgramID)
                              .ExecuteCommand();
                            db.Updateable<OTB_SYS_ModuleForProgram>()
                                           .UpdateColumns(x => new OTB_SYS_ModuleForProgram { OrderByValue = x.OrderByValue - 1 })
                                           .Where(x => x.OrgID == item.OrgID && x.ModuleID == item.ModuleID && x.OrderByValue > item.OrderByValue)
                                           .ExecuteCommand();
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
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ProgramMaintain_UpdService), @"程式管理", @"Update（程式管理（修改））", @"", @"", @"");
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

        #endregion 程式管理（修改）

        #region 程式管理（刪除）

        /// <summary>
        /// 程式管理（刪除）
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
                        var sProgramID = _fetchString(i_crm, @"ProgramID");
                        var iRel = db.Deleteable<OTB_SYS_ProgramList>().Where(x => x.ProgramID == sProgramID).ExecuteCommand();
                        //更新和刪除已刪除的模組
                        var saDelEntitys = db.Queryable<OTB_SYS_ModuleForProgram>()
                                             .Where(x => x.OrgID == i_crm.ORIGID && x.ProgramID == sProgramID)
                                             .ToList();
                        foreach (var item in saDelEntitys)
                        {
                            db.Deleteable<OTB_SYS_ModuleForProgram>()
                              .Where(x => x.OrgID == item.OrgID && x.ModuleID == item.ModuleID && x.ProgramID == item.ProgramID)
                              .ExecuteCommand();
                            db.Updateable<OTB_SYS_ModuleForProgram>()
                                           .UpdateColumns(x => new OTB_SYS_ModuleForProgram { OrderByValue = x.OrderByValue - 1 })
                                           .Where(x => x.OrgID == item.OrgID && x.ModuleID == item.ModuleID && x.OrderByValue > item.OrderByValue)
                                           .ExecuteCommand();
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
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ProgramMaintain_UpdService), @"程式管理", @"Delete（程式管理（刪除））", @"", @"", @"");
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

        #endregion 程式管理（刪除）

        #region 程式管理（查詢筆數）

        /// <summary>
        /// 程式管理（查詢筆數）
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
                    var sProgramID = _fetchString(i_crm, @"ProgramID");
                    var iCout = db.Queryable<OTB_SYS_ProgramList>()
                        .Where(x => x.OrgID == i_crm.ORIGID)
                        .WhereIF(!string.IsNullOrEmpty(sProgramID), x => x.ProgramID == sProgramID)
                        .Count();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iCout);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ProgramMaintain_UpdService), "", "QueryCout（程式管理（查詢筆數））", "", "", "");
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

        #endregion 程式管理（查詢筆數）

        private class ModuleInfo
        {
            public ModuleInfo()
            {
                ModuleID = @"";
                ModuleName = @"";
                PrgCount = 0;
                OrderByValue = 1;
            }

            public string ModuleID { get; set; }
            public string ModuleName { get; set; }
            public int PrgCount { get; set; }
            public int OrderByValue { get; set; }
        }
    }
}