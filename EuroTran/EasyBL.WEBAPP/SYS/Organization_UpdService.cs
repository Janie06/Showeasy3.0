using EasyBL.WebApi.Message;
using Entity.Sugar;
using SqlSugar;
using SqlSugar.Base;
using System;

namespace EasyBL.WEBAPP.WSM
{
    public class Organization_UpdService : ServiceBase
    {
        #region 組織管理（單筆查詢）

        /// <summary>
        /// 組織管理（單筆查詢）
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
                    var sOrgID = _fetchString(i_crm, @"OrgID");

                    var oEntity = db.Queryable<OTB_SYS_Organization, OTB_SYS_Members, OTB_SYS_Members>
                        ((t1, t2, t3) =>
                        new object[] {
                                JoinType.Left, t1.ParentOrgID == t2.OrgID && t1.CreateUser == t2.MemberID,
                                JoinType.Left, t1.ParentOrgID == t3.OrgID && t1.ModifyUser == t3.MemberID
                              }
                        )
                        .Where((t1, t2, t3) => t1.ParentOrgID == i_crm.ORIGID && t1.OrgID == sOrgID)
                        .Select((t1, t2, t3) => new OTB_SYS_Organization
                        {
                            OrgID = SqlFunc.GetSelfAndAutoFill(t1.OrgID),
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
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Organization_UpdService), "", "QueryOne（組織管理（單筆查詢））", "", "", "");
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

        #endregion 組織管理（單筆查詢）

        #region 組織管理（新增）

        /// <summary>
        /// 組織管理（新增）
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
                        var sOrgID = _fetchString(i_crm, @"OrgID");
                        var oEntity = _fetchEntity<OTB_SYS_Organization>(i_crm);
                        _setEntityBase(oEntity, i_crm);
                        oEntity.OrgID = sOrgID;
                        oEntity.ParentOrgID = i_crm.ORIGID;
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
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Organization_UpdService), @"組織管理", @"Add（組織管理（新增））", @"", @"", @"");
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

        #endregion 組織管理（新增）

        #region 組織管理（修改）

        /// <summary>
        /// 組織管理（修改）
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
                        var sOrgID = _fetchString(i_crm, @"OrgID");
                        var oNewEntity = _fetchEntity<OTB_SYS_Organization>(i_crm);
                        _setEntityBase(oNewEntity, i_crm);
                        oNewEntity.OrgID = sOrgID;
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
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Organization_UpdService), @"組織管理", @"Update（組織管理（修改））", @"", @"", @"");
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

        #endregion 組織管理（修改）

        #region 組織管理（刪除）

        /// <summary>
        /// 組織管理（刪除）
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
                        var sOrgID = _fetchString(i_crm, @"OrgID");
                        var oOrganization = db.Queryable<OTB_SYS_Organization>().Single(x => x.ParentOrgID == i_crm.ORIGID && x.OrgID == sOrgID);
                        var iRel = db.Deleteable<OTB_SYS_Organization>().Where(x => x.ParentOrgID == i_crm.ORIGID && x.OrgID == sOrgID).ExecuteCommand();
                        var fileService = new CommonService();
                        i_crm.DATA.Add("FileID", oOrganization.LoGoId);
                        i_crm.DATA.Add("IDType", "parent");
                        fileService.DelFile(i_crm);
                        i_crm.DATA["FileID"] = oOrganization.BackgroundImage;
                        fileService.DelFile(i_crm);
                        i_crm.DATA["FileID"] = oOrganization.WebsiteLgoId;
                        fileService.DelFile(i_crm);
                        i_crm.DATA["FileID"] = oOrganization.PicShowId;
                        fileService.DelFile(i_crm);
                        i_crm.DATA["FileID"] = oOrganization.WebsiteLgoId_CN;
                        fileService.DelFile(i_crm);
                        i_crm.DATA["FileID"] = oOrganization.PicShowId_CN;
                        fileService.DelFile(i_crm);
                        i_crm.DATA["FileID"] = oOrganization.WebsiteLgoId_EN;
                        fileService.DelFile(i_crm);
                        i_crm.DATA["FileID"] = oOrganization.PicShowId_EN;
                        fileService.DelFile(i_crm);
                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, iRel);
                    } while (false);

                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Organization_UpdService), @"組織管理", @"Delete（組織管理（刪除））", @"", @"", @"");
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

        #endregion 組織管理（刪除）

        #region 組織管理（查詢筆數）

        /// <summary>
        /// 組織管理（查詢筆數）
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
                    var sOrgID = _fetchString(i_crm, @"OrgID");
                    var iCout = db.Queryable<OTB_SYS_Organization>()
                        .WhereIF(!string.IsNullOrEmpty(sOrgID), x => x.ParentOrgID == i_crm.ORIGID && x.OrgID == sOrgID)
                        .Count();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iCout);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Organization_UpdService), "", "QueryCout（組織管理（查詢筆數））", "", "", "");
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

        #endregion 組織管理（查詢筆數）
    }
}