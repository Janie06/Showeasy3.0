using EasyBL.WebApi.Message;
using Entity.Sugar;
using SqlSugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyBL.WEBAPP.SYS
{
    public class AnnouncementList_QryService : ServiceBase
    {
        #region 公告列表（分頁查詢）

        /// <summary>
        /// 公告列表（分頁查詢）
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

                    var sTitle = _fetchString(i_crm, @"Title");
                    var sAnn_Type = _fetchString(i_crm, @"Ann_Type");
                    var sStartDateTime = _fetchString(i_crm, @"StartDateTime");
                    var sEndDateTime = _fetchString(i_crm, @"EndDateTime");
                    var bExcel = _fetchBool(i_crm, @"Excel");

                    var rStartDateTime = new DateTime();
                    var rEndDateTime = new DateTime();
                    if (!string.IsNullOrEmpty(sStartDateTime))
                    {
                        rStartDateTime = SqlFunc.ToDate(sStartDateTime);
                    }
                    if (!string.IsNullOrEmpty(sEndDateTime))
                    {
                        rEndDateTime = SqlFunc.ToDate(sEndDateTime).AddDays(1);
                    }

                    pml.DataList = db.Queryable<OTB_SYS_Announcement>()
                        .Where(x => x.OrgID == i_crm.ORIGID && x.Title.Contains(sTitle))
                        .WhereIF(!string.IsNullOrEmpty(sAnn_Type), x => x.Ann_Type == sAnn_Type)
                        .WhereIF(!string.IsNullOrEmpty(sStartDateTime), x => x.CreateDate >= rStartDateTime.Date)
                        .WhereIF(!string.IsNullOrEmpty(sEndDateTime), x => x.CreateDate <= rEndDateTime.Date)
                        .OrderBy(sSortField, sSortOrder)
                        .ToPageList(pml.PageIndex, bExcel ? 100000 : pml.PageSize, ref iPageCount);
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
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBAPP.SYS.AnnouncementList_QryService", "", "QueryPage（公告列表（分頁查詢））", "", "", "");
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

        #endregion 公告列表（分頁查詢）

        #region 公告列表（置頂修改）

        /// <summary>
        /// 公告列表（置頂修改）
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on UpdImportCustomers</param>
        /// <returns></returns>
        public ResponseMessage UpdateGoTop(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var oNewEntity = _fetchEntity<OTB_SYS_Announcement>(i_crm);
                        _setEntityBase(oNewEntity, i_crm);
                        if (oNewEntity.GoTop == true)
                        {
                            oNewEntity.GoTop_Time = DateTime.Now;
                        }
                        var command = db.Updateable(oNewEntity);
                        if (oNewEntity.GoTop == true)
                        {
                            command.UpdateColumns(x => new
                            {
                                x.GoTop,
                                x.GoTop_Time,
                                x.ModifyUser,
                                x.ModifyDate
                            });
                        }
                        else
                        {
                            command.UpdateColumns(x => new
                            {
                                x.GoTop,
                                x.ModifyUser,
                                x.ModifyDate
                            });
                        }
                        var iRel = command.ExecuteCommand();
                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, iRel);
                    } while (false);

                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(AnnouncementList_QryService), @"公告管理編輯", @"Update（公告列表（置頂修改））", @"", @"", @"");
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

        #endregion 公告列表（置頂修改）
    }
}