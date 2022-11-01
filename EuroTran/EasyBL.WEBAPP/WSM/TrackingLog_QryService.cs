using EasyBL.WebApi.Message;
using Entity.Sugar;
using Entity.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SqlSugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyBL.WEBAPP.WSM
{
    public class TrackingLog_QryService : ServiceBase
    {
        #region 查詢貨況查詢記錄分組明細

        /// <summary>
        /// 查詢貨況查詢記錄分組明細
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetGroupInfo(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.DB;
            try
            {
                do
                {
                    var sDepartmentID = _fetchString(i_crm, @"DepartmentID");
                    var sQueryNumber = _fetchString(i_crm, @"QueryNumber");
                    var sQueryTimeStart = _fetchString(i_crm, @"QueryTimeStart");
                    var sQueryTimeEnd = _fetchString(i_crm, @"QueryTimeEnd");

                    var sbSql = new StringBuilder();
                    var spm = new List<SugarParameter>();
                    sbSql.Append("select count(1) Count,QueryIp,IPInfo,QueryNumber from OTB_WSM_TrackingLog where 1=1 ");
                    if (!string.IsNullOrEmpty(sDepartmentID))
                    {
                        sbSql.Append(" and DepartmentIDs like '%@DepartmentIDs%' ");
                        spm.Add(new SugarParameter("@DepartmentIDs", sDepartmentID));
                    }
                    if (!string.IsNullOrEmpty(sQueryTimeStart))
                    {
                        sbSql.Append(" and querytime>= @QueryTimeStart ");
                        spm.Add(new SugarParameter("@QueryTimeStart", sQueryTimeStart));
                    }
                    if (!string.IsNullOrEmpty(sQueryTimeEnd))
                    {
                        sbSql.Append(" and querytime<= @QueryTimeEnd ");
                        spm.Add(new SugarParameter("@QueryTimeEnd", sQueryTimeEnd));
                    }
                    sbSql.Append(" group by QueryIp,IPInfo,QueryNumber ");
                    var list = db.SqlQueryable<View_WSM_GroupTrackingLog>(sbSql.ToString()).AddParameters(spm).Where(it => it.QueryNumber == sQueryNumber).ToList();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, list);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(TrackingLog_QryService), @"貨況查詢", @"GetGroupInfo（查詢貨況查詢記錄分組明細）", @"", @"", @"");
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

        #endregion 查詢貨況查詢記錄分組明細

        #region Tracking（Log記錄）分頁查詢

        /// <summary>
        /// Tracking（Log記錄）分頁查詢
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

                    var sQueryNumber = _fetchString(i_crm, @"QueryNumber");
                    var sExhibitioName = _fetchString(i_crm, @"ExhibitioName");
                    var sDepartmentID = _fetchString(i_crm, @"DepartmentID");
                    var sQueryTimeStart = _fetchString(i_crm, @"QueryTimeStart");
                    var sQueryTimeEnd = _fetchString(i_crm, @"QueryTimeEnd");
                    var bExcel = _fetchBool(i_crm, @"Excel");

                    var rQueryTimeStart = new DateTime();
                    var rQueryTimeEnd = new DateTime();
                    if (!string.IsNullOrEmpty(sQueryTimeStart))
                    {
                        rQueryTimeStart = SqlFunc.ToDate(sQueryTimeStart);
                    }
                    if (!string.IsNullOrEmpty(sQueryTimeEnd))
                    {
                        rQueryTimeEnd = SqlFunc.ToDate(sQueryTimeEnd).AddDays(1);
                    }

                    var q1 = db.Queryable<OTB_WSM_TrackingLog, OTB_OPM_ImportExhibition, OTB_OPM_ExportExhibition>
                        ((t1, t2, t3) =>
                        new object[] {
                                JoinType.Left, t1.OrgID == t2.OrgID && t1.ParentId == t2.ImportBillNO,
                                JoinType.Left, t1.OrgID == t3.OrgID && t1.ParentId == t3.ExportBillNO
                              }
                        )
                        .Select((t1, t2, t3) => new View_WSM_TrackingLog
                        {
                            NO = SqlFunc.GetSelfAndAutoFill(t1.NO),
                            ExhibitionNO = SqlFunc.IsNull(t2.ExhibitionNO, t3.ExhibitionNO),
                            ResponsiblePerson = SqlFunc.IsNull(t2.ResponsiblePerson, t3.ResponsiblePerson)
                        });
                    var q2 = db.Queryable<OTB_OPM_Exhibition>();

                    pml.DataList = db.Queryable
                        (q1, q2, (v1, v2) => v1.OrgID == v2.OrgID && v1.ExhibitionNO == v2.SN.ToString())
                        .Where((v1, v2) => v1.DepartmentIDs.Contains(sDepartmentID) && v1.QueryNumber.Contains(sQueryNumber) && (v2.Exhibitioname_TW.Contains(sExhibitioName) || v2.Exhibitioname_EN.Contains(sExhibitioName)))
                        .WhereIF(!string.IsNullOrEmpty(sQueryTimeStart), (v1, v2) => v1.QueryTime >= rQueryTimeStart.Date)
                        .WhereIF(!string.IsNullOrEmpty(sQueryTimeEnd), (v1, v2) => v1.QueryTime <= rQueryTimeEnd.Date)
                        .Select((v1, v2) => new View_WSM_TrackingLog
                        {
                            NO = SqlFunc.GetSelfAndAutoFill(v1.NO),
                            Exhibitioname_TW = v2.Exhibitioname_TW,
                            Exhibitioname_EN = v2.Exhibitioname_EN
                        })
                        .MergeTable()
                        .OrderBy(sSortField, sSortOrder)
                        .ToPageList(pml.PageIndex, bExcel ? 100000 : pml.PageSize, ref iPageCount);
                    pml.Total = iPageCount;

                    rm = new SuccessResponseMessage(null, i_crm);
                    if (bExcel)
                    {
                        const string sFileName = "Tracking查詢記錄";
                        var oHeader = new Dictionary<string, string>
                        {
                            { "RowIndex", "項次" },
                            { "QueryNumber", "查詢碼" },
                            { "Exhibitioname_TW", "活動/展覽名稱" },
                            { "Exhibitioname_EN", "英文展名" },
                            { "AgentName", "國外代理" },
                            { "CustomerName", "客戶/參展廠商" },
                            { "QueryIp", "IP" },
                            { "QueryInfo", "IP地址信息" },
                            { "QueryTime", "查詢時間 " }
                        };
                        var saViewTrackingLog = pml.DataList as List<View_WSM_TrackingLog>;
                        foreach (var item in saViewTrackingLog)
                        {
                            var joIPInfo = (JObject)JsonConvert.DeserializeObject(item.IPInfo);
                            item.IPInfo = joIPInfo["country"] + " " + joIPInfo["area"] + " " + joIPInfo["region"] + " " + joIPInfo["city"];
                            item.IPInfo = ChineseStringUtility.ToTraditional(item.IPInfo);
                        }
                        var dicAlain = ExcelService.GetExportAlain(oHeader, "QueryTime");
                        var bOk = new ExcelService().CreateExcelByList(saViewTrackingLog, out string sPath, oHeader, dicAlain, sFileName);
                        rm.DATA.Add(BLWording.REL, sPath);
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
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBAPP.WSM.TrackingLog_QryService", "", "QueryPage（Tracking（Log記錄）分頁查詢）", "", "", "");
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

        #endregion Tracking（Log記錄）分頁查詢

        #region Tracking（Log記錄）（刪除）

        /// <summary>
        /// Tracking（Log記錄）（刪除）
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on UpdImportCustomers</param>
        /// <returns></returns>
        public ResponseMessage GridDelete(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var iNO = _fetchInt(i_crm, @"NO");
                        var iRel = db.Deleteable<OTB_WSM_TrackingLog>()
                                     .Where(x => x.OrgID == i_crm.ORIGID && x.NO == iNO).ExecuteCommand();
                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, iRel);
                    } while (false);

                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(TrackingLog_QryService), @"多語系管理", @"GridDelete（Tracking（Log記錄）（刪除））", @"", @"", @"");
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

        #endregion Tracking（Log記錄）（刪除）
    }
}