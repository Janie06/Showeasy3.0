using EasyBL.WebApi.Message;
using Entity.Sugar;
using SqlSugar.Base;
using System;

namespace EasyBL.WEBAPP.WSM
{
    public class News_UpdService : ServiceBase
    {
        #region 最新消息（新增）

        /// <summary>
        /// 最新消息（新增）
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on UpdImportCustomers</param>
        /// <returns></returns>
        public ResponseMessage Add(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var oEntity = _fetchEntity<OTB_WSM_News>(i_crm);
                        _setEntityBase(oEntity, i_crm);
                        var iOldCout = db.Queryable<OTB_WSM_News>()
                                         .Count(x => x.OrgID == i_crm.ORIGID && x.News_LanguageType == oEntity.News_LanguageType && x.News_Type == oEntity.News_Type);

                        if (oEntity.OrderByValue <= iOldCout)
                        {
                            var iRelUp = db.Updateable<OTB_WSM_News>()
                                            .UpdateColumns(x => new OTB_WSM_News { OrderByValue = x.OrderByValue + 1 })
                                            .Where(x => x.OrgID == i_crm.ORIGID && x.News_LanguageType == oEntity.News_LanguageType && x.News_Type == oEntity.News_Type && x.OrderByValue >= oEntity.OrderByValue)
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
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(News_UpdService), @"最新消息", @"Add（最新消息（新增））", @"", @"", @"");
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

        #endregion 最新消息（新增）

        #region 最新消息（修改）

        /// <summary>
        /// 最新消息（修改）
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
                        var oNewEntity = _fetchEntity<OTB_WSM_News>(i_crm);
                        _setEntityBase(oNewEntity, i_crm);
                        var iOldEntity = db.Queryable<OTB_WSM_News>().Single(x => x.OrgID == i_crm.ORIGID && x.SN == oNewEntity.SN);

                        if (oNewEntity.OrderByValue > iOldEntity.OrderByValue)
                        {
                            var iRelUp = db.Updateable<OTB_WSM_News>()
                                            .UpdateColumns(x => new OTB_WSM_News { OrderByValue = x.OrderByValue - 1 })
                                            .Where(x => x.OrgID == oNewEntity.OrgID && x.News_LanguageType == oNewEntity.News_LanguageType && x.News_Type == oNewEntity.News_Type && x.OrderByValue <= oNewEntity.OrderByValue && x.OrderByValue > iOldEntity.OrderByValue).ExecuteCommand();
                        }
                        else
                        {
                            var iRelDown = db.Updateable<OTB_WSM_News>()
                                            .UpdateColumns(x => new OTB_WSM_News { OrderByValue = x.OrderByValue + 1 })
                                            .Where(x => x.OrgID == oNewEntity.OrgID && x.News_LanguageType == oNewEntity.News_LanguageType && x.News_Type == oNewEntity.News_Type && x.OrderByValue >= oNewEntity.OrderByValue && x.OrderByValue < iOldEntity.OrderByValue).ExecuteCommand();
                        }

                        var iRel = db.Updateable(oNewEntity)
                            .IgnoreColumns(x => new
                            {
                                x.OrgID,
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
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(News_UpdService), @"最新消息", @"Update（最新消息（修改））", @"", @"", @"");
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

        #endregion 最新消息（修改）

        #region 最新消息（刪除）

        /// <summary>
        /// 最新消息（刪除）
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
                        var iId = _fetchInt(i_crm, @"Id");

                        var oEntity = db.Queryable<OTB_WSM_News>().Single(x => x.SN == iId);
                        var iRel = db.Deleteable<OTB_WSM_News>().Where(x => x.SN == iId).ExecuteCommand();
                        var iRelUp = db.Updateable<OTB_WSM_News>()
                                        .UpdateColumns(x => new OTB_WSM_News { OrderByValue = x.OrderByValue - 1 })
                                        .Where(x => x.OrgID == oEntity.OrgID && x.News_LanguageType == oEntity.News_LanguageType && x.News_Type == oEntity.News_Type && x.OrderByValue > oEntity.OrderByValue)
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
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(News_UpdService), @"最新消息", @"Delete（最新消息（刪除））", @"", @"", @"");
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

        #endregion 最新消息（刪除）
    }
}