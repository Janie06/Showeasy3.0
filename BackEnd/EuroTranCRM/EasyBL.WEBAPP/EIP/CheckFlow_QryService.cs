using EasyBL.WebApi.Message;
using Entity.Sugar;
using Entity.ViewModels;
using SqlSugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyBL.WEBAPP.EIP
{
    public class CheckFlow_QryService : ServiceBase
    {
        #region 簽核流程分頁查詢

        /// <summary>
        /// 簽核流程分頁查詢
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
                    var sSortField = _fetchString(i_crm, @"sortField") ?? "ModifyDate";
                    var sSortOrder = _fetchString(i_crm, @"sortOrder") ?? "desc";

                    var sFlow_Type = _fetchString(i_crm, @"Flow_Type") ?? "";
                    var sFlow_Name = _fetchString(i_crm, @"Flow_Name");
                    var sFlows = _fetchString(i_crm, @"Flows") ?? "";
                    var bExcel = _fetchBool(i_crm, @"Excel");

                    pml.DataList = db.Queryable<OTB_EIP_CheckFlow, OTB_SYS_Members, OTB_SYS_Members, OTB_SYS_Arguments>
                        ((t1, t2, t3, t4) =>
                        new object[] {
                                JoinType.Inner, t1.OrgID == t2.OrgID && t1.Handle_Person == t2.MemberID,
                                JoinType.Inner, t1.OrgID == t3.OrgID && t1.ModifyUser == t3.MemberID,
                                JoinType.Inner, t1.OrgID == t4.OrgID && t1.Flow_Type == t4.ArgumentID && t4.ArgumentClassID == "Flow_Type"
                              }
                        )
                        .Where((t1, t2, t3, t4) => t1.OrgID == i_crm.ORIGID)
                        .WhereIF(!string.IsNullOrEmpty(sFlows), (t1, t2, t3, t4) => t1.Flows.Contains(sFlows))
                        .WhereIF(!string.IsNullOrEmpty(sFlow_Name), (t1, t2, t3, t4) => t1.Flow_Name.Contains(sFlow_Name))
                        .WhereIF(!string.IsNullOrEmpty(sFlow_Type), (t1, t2, t3, t4) => t1.Flow_Type == sFlow_Type)
                        .Select((t1, t2, t3, t4) => new View_EIP_CheckFlow
                        {
                            Guid = SqlFunc.GetSelfAndAutoFill(t1.Guid),
                            Handle_PersonName = t2.MemberName,
                            ModifyUserName = t3.MemberName,
                            Flow_TypeName = t4.ArgumentValue
                        })
                        .MergeTable()
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
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBAPP.EIP.CheckFlow_QryService", "", "QueryPage（簽核流程分頁查詢）", "", "", "");
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

        #endregion 簽核流程分頁查詢

        #region 簽核流程筆查詢

        /// <summary>
        /// 簽核流程筆查詢
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
                    var sId = _fetchString(i_crm, @"Guid");

                    var oInvoiceApplyInfo = db.Queryable<OTB_EIP_CheckFlow, OTB_SYS_Members, OTB_SYS_Members, OTB_SYS_Arguments>
                        ((t1, t2, t3, t4) =>
                        new object[] {
                                JoinType.Inner, t1.OrgID == t2.OrgID && t1.Handle_Person == t2.MemberID,
                                JoinType.Inner, t1.OrgID == t3.OrgID && t1.ModifyUser == t3.MemberID,
                                JoinType.Inner, t1.OrgID == t4.OrgID && t1.Flow_Type == t4.ArgumentID && t4.ArgumentClassID == "Flow_Type"
                              }
                        )
                        .Where((t1, t2, t3, t4) => t1.OrgID == i_crm.ORIGID && t1.Guid == sId)
                        .Select((t1, t2, t3, t4) => new View_EIP_CheckFlow
                        {
                            Guid = SqlFunc.GetSelfAndAutoFill(t1.Guid),
                            Handle_PersonName = t2.WenZhongAcount,
                            ModifyUserName = t3.MemberName,
                            Flow_TypeName = t4.ArgumentValue
                        }).Single();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, oInvoiceApplyInfo);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBAPP.EIP.CheckFlow_QryService", "", "QueryOne（簽核流程筆查詢）", "", "", "");
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

        #endregion 簽核流程筆查詢
    }
}