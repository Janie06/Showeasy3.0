using EasyBL.WebApi.Message;
using Entity.Sugar;
using Entity.ViewModels;
using SqlSugar;
using SqlSugar.Base;
using System;

namespace EasyBL.WEBAPP.WSM
{
    public class WebSiteSetupService : ServiceBase
    {
        #region 官網設定（分頁查詢）

        /// <summary>
        /// 官網設定（分頁查詢）
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

                    var sSetType = _fetchString(i_crm, @"SetType");
                    var sLangId = _fetchString(i_crm, @"LangId");
                    var sParentId = _fetchString(i_crm, @"ParentId");
                    var bExcel = _fetchBool(i_crm, @"Excel");

                    pml.DataList = db.Queryable<OTB_WSM_WebSiteSetting>()
                        .Where(x => x.OrgID == i_crm.ORIGID && x.SetType == sSetType && x.LangId == sLangId)
                        .WhereIF(!string.IsNullOrEmpty(sParentId), x => x.ParentId == sParentId)
                         .Select(x => new View_WSM_WebSiteSetting
                         {
                             Guid = SqlFunc.GetSelfAndAutoFill(x.Guid),
                             OrderCount = SqlFunc.Subqueryable<OTB_WSM_WebSiteSetting>()
                             .Where(p => p.OrgID == x.OrgID && p.LangId == x.LangId && p.SetType == x.SetType && ((SqlFunc.HasValue(p.ParentId) && p.ParentId == x.ParentId) || (!SqlFunc.HasValue(x.ParentId) && SqlFunc.IsNull(p.ParentId, "") == "")))
                             .Count()
                         })
                        .OrderBy(sSortField, sSortOrder)
                        .ToPageList(pml.PageIndex, pml.PageSize, ref iPageCount);
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
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(WebSiteSetupService), "", "QueryPage（官網設定（分頁查詢））", "", "", "");
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

        #endregion 官網設定（分頁查詢）

        #region 組織資訊（單筆）

        /// <summary>
        /// 組織資訊（單筆）
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
                    var oEntity = db.Queryable<OTB_SYS_Organization, OTB_SYS_Members, OTB_SYS_Members>
                        ((t1, t2, t3) =>
                        new object[] {
                                JoinType.Left, t1.OrgID == t2.OrgID && t1.CreateUser == t2.MemberID,
                                JoinType.Left, t1.OrgID == t3.OrgID && t1.ModifyUser == t3.MemberID
                              }
                        )
                        .Where((t1, t2, t3) => t1.OrgID == i_crm.ORIGID)
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
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(WebSiteSetupService), "", "QueryOne（組織資訊（單筆））", "", "", "");
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

        #endregion 組織資訊（單筆）

        #region 官網設定（基本資料修改）

        /// <summary>
        /// 官網設定（基本資料修改）
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
                        var oNewEntity = _fetchEntity<OTB_SYS_Organization>(i_crm);
                        _setEntityBase(oNewEntity, i_crm);

                        var iRel = db.Updateable(oNewEntity)
                            .UpdateColumns(x => new
                            {
                                x.VideoUrl,
                                x.VideoUrl_CN,
                                x.VideoUrl_EN,
                                x.Introduction,
                                x.Introduction_CN,
                                x.Introduction_EN,
                                x.MissionAndVision_TW,
                                x.MissionAndVision_CN,
                                x.MissionAndVision_EN,
                                x.ServiceTitle,
                                x.ServiceTitle_CN,
                                x.ServiceTitle_EN,
                                x.VideoDescription,
                                x.VideoDescription_CN,
                                x.VideoDescription_EN,
                                x.ModifyUser,
                                x.ModifyDate
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
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(WebSiteSetupService), @"官網設定", @"Update（官網設定（基本資料修改））", @"", @"", @"");
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

        #endregion 官網設定（基本資料修改）

        #region 官網設定（新增）

        /// <summary>
        /// 官網設定（新增）
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
                    var oEntity = _fetchEntity<OTB_WSM_WebSiteSetting>(i_crm);
                    oEntity.Guid = Guid.NewGuid().ToString();
                    oEntity.Title = oEntity.Title ?? "";
                    _setEntityBase(oEntity, i_crm);
                    var iRel = db.Insertable(oEntity).ExecuteCommand();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iRel);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);

                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(WebSiteSetupService), @"官網設定", @"Add（官網設定（新增））", @"", @"", @"");
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

        #endregion 官網設定（新增）

        #region 官網設定（修改）

        /// <summary>
        /// 官網設定（修改）
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
                    var oEntity = _fetchEntity<OTB_WSM_WebSiteSetting>(i_crm);
                    _setEntityBase(oEntity, i_crm);
                    var iRel = db.Updateable(oEntity)
                        .IgnoreColumns(x => new
                        {
                            x.OrgID,
                            x.CreateDate,
                            x.CreateUser
                        }).ExecuteCommand();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iRel);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(WebSiteSetupService), @"官網設定", @"Update（官網設定（修改））", @"", @"", @"");
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

        #endregion 官網設定（修改）

        #region 官網設定（刪除）

        /// <summary>
        /// 官網設定（刪除）
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
                    var sGuid = _fetchString(i_crm, @"Guid");

                    var oWebSiteSetting = db.Queryable<OTB_WSM_WebSiteSetting>().Single(x => x.Guid == sGuid);
                    var iRel = db.Deleteable<OTB_WSM_WebSiteSetting>().Where(x => x.Guid == sGuid).ExecuteCommand();

                    var iRelUp = db.Updateable<OTB_WSM_WebSiteSetting>()
                                    .UpdateColumns(x => new OTB_WSM_WebSiteSetting { OrderByValue = x.OrderByValue - 1 })
                                    .Where(x => x.OrgID == oWebSiteSetting.OrgID && x.SetType == oWebSiteSetting.SetType && x.LangId == oWebSiteSetting.LangId && SqlFunc.IsNull(x.ParentId, "") == SqlFunc.IsNull(oWebSiteSetting.ParentId, "") && x.OrderByValue > oWebSiteSetting.OrderByValue).ExecuteCommand();

                    i_crm.DATA.Add("FileID", oWebSiteSetting.CoverId);
                    i_crm.DATA.Add("IDType", "parent");
                    new CommonService().DelFile(i_crm);
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iRel);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(WebSiteSetupService), @"官網設定", @"Delete（官網設定（刪除））", @"", @"", @"");
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

        #endregion 官網設定（刪除）

        #region 官網設定（更新排序）

        /// <summary>
        /// 官網設定（更新排序）
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on UpdImportCustomers</param>
        /// <returns></returns>
        public ResponseMessage UpdateOrderByValue(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var sId = _fetchString(i_crm, @"Id");
                        var iOldOrderByValue = _fetchInt(i_crm, @"OldOrderByValue");
                        var iNewOrderByValue = _fetchInt(i_crm, @"NewOrderByValue");

                        var oOrderEntity = db.Queryable<OTB_WSM_WebSiteSetting>().Single(x => x.Guid == sId);

                        if (iNewOrderByValue > iOldOrderByValue)
                        {
                            var iRelUp = db.Updateable<OTB_WSM_WebSiteSetting>()
                                            .UpdateColumns(x => new OTB_WSM_WebSiteSetting { OrderByValue = x.OrderByValue - 1 })
                                            .Where(x => x.OrgID == oOrderEntity.OrgID && x.SetType == oOrderEntity.SetType && x.LangId == oOrderEntity.LangId && SqlFunc.IsNull(x.ParentId, "") == SqlFunc.IsNull(oOrderEntity.ParentId, "") && x.OrderByValue <= iNewOrderByValue && x.OrderByValue > iOldOrderByValue).ExecuteCommand();
                        }
                        else
                        {
                            var iRelDown = db.Updateable<OTB_WSM_WebSiteSetting>()
                                            .UpdateColumns(x => new OTB_WSM_WebSiteSetting { OrderByValue = x.OrderByValue + 1 })
                                            .Where(x => x.OrgID == oOrderEntity.OrgID && x.SetType == oOrderEntity.SetType && x.LangId == oOrderEntity.LangId && SqlFunc.IsNull(x.ParentId, "") == SqlFunc.IsNull(oOrderEntity.ParentId, "") && x.OrderByValue >= iNewOrderByValue && x.OrderByValue < iOldOrderByValue).ExecuteCommand();
                        }
                        var iRelSelf = db.Updateable(new OTB_WSM_WebSiteSetting { OrderByValue = iNewOrderByValue })
                                        .UpdateColumns(x => x.OrderByValue)
                                        .Where(x => x.Guid == sId).ExecuteCommand();

                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, iRelSelf);
                    } while (false);

                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(WebSiteSetupService), @"官網設定", @"UpdateOrderByValue（官網設定（更新排序））", @"", @"", @"");
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

        #endregion 官網設定（更新排序）
    }
}