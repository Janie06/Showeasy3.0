using Aspose.Cells;
using EasyBL.WebApi.Message;
using Entity.Sugar;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SqlSugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using EasyBL;
using Entity.ViewModels;

namespace EasyBL.WEBAPP.CRM
{
    public class SatisfactionCase_QryService : ServiceBase
    {
        #region 滿意度案件分頁查詢
        /// <summary>
        /// 滿意度案件分頁查詢
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
                    var sComplaintNumber = _fetchString(i_crm, @"ComplaintNumber"); // 案件名稱
                    var sComplaintTitle = _fetchString(i_crm, @"ComplaintTitle"); // 展覽名稱 
                    var sCreateUser = _fetchString(i_crm, @"CreateUser"); // 創建人
                    var sCustomerName = _fetchString(i_crm, @"CustomerName"); // 公司名稱
                    var sDateType = _fetchString(i_crm, @"DateType");
                    var sDateStart = _fetchString(i_crm, @"DateStart");
                    var sDateEnd = _fetchString(i_crm, @"DateEnd");

                    var bExcel = _fetchBool(i_crm, @"Excel");
                    var sExcelType = _fetchString(i_crm, @"ExcelType");

                    var rDateStart = new DateTime();
                    var rDateEnd = new DateTime();

                    if (!string.IsNullOrEmpty(sDateStart))
                    {
                        rDateStart = SqlFunc.ToDate(sDateStart);
                    }
                    if (!string.IsNullOrEmpty(sDateEnd))
                    {
                        rDateEnd = SqlFunc.ToDate(sDateEnd).AddDays(1);
                    }

                    pml.DataList = db.Queryable<OTB_CRM_SatisfactionCase, OTB_CRM_SatisfactionCustomer, OTB_OPM_Exhibition>
                        ((t1, t2, t3) =>
                        new object[] {
                                JoinType.Left, t1.SN == t2.CaseSN.ToString(),
                                JoinType.Left, t1.ExhibitionNO == t3.SN.ToString()
                              }
                        )
                        .WhereIF(!string.IsNullOrEmpty(sComplaintNumber), (t1, t2, t3) => t1.CaseName.Contains(sComplaintNumber))
                        .WhereIF(!string.IsNullOrEmpty(sComplaintTitle), (t1, t2, t3) => t3.Exhibitioname_TW.Contains(sComplaintTitle) || t3.Exhibitioname_EN.Contains(sComplaintTitle))
                        .WhereIF(!string.IsNullOrEmpty(sCreateUser), (t1, t2, t3) => t1.CreateUser.Contains(sCreateUser))
                        .WhereIF(!string.IsNullOrEmpty(sCustomerName), (t1, t2, t3) => t2.CustomerName.Contains(sCustomerName))
                        .WhereIF(!string.IsNullOrEmpty(sDateStart) && sDateType == "1", (t1, t2, t3) => t1.CreateDate >= rDateStart.Date)
                        .WhereIF(!string.IsNullOrEmpty(sDateEnd) && sDateType == "1", (t1, t2, t3) => t1.CreateDate <= rDateEnd.Date)
                        .WhereIF(!string.IsNullOrEmpty(sDateStart) && sDateType == "2", (t1, t2, t3) => t1.ModifyDate >= rDateStart.Date)
                        .WhereIF(!string.IsNullOrEmpty(sDateEnd) && sDateType == "2", (t1, t2, t3) => t1.ModifyDate <= rDateEnd.Date)
                        .GroupBy((t1, t2, t3) => t1.SN)
                        .GroupBy((t1, t2, t3) => t1.CaseName)
                        .GroupBy((t1, t2, t3) => t3.Exhibitioname_TW)
                        .GroupBy((t1, t2, t3) => t3.ExhibitionDateStart)
                        .GroupBy((t1, t2, t3) => t3.ExhibitionDateEnd)
                        .GroupBy((t1, t2, t3) => t1.CreateUser)
                        .GroupBy((t1, t2, t3) => t1.CreateDate)
                        .GroupBy((t1, t2, t3) => t1.ModifyDate)
                        .Select((t1, t2, t3) => new View_CRM_SatisfactionCase
                        {
                            SN = t1.SN,
                            CaseName = t1.CaseName,
                            Exhibitioname_TW = t3.Exhibitioname_TW,
                            ExhibitionDateStart = t3.ExhibitionDateStart,
                            ExhibitionDateEnd = t3.ExhibitionDateEnd,
                            CreateUser = t1.CreateUser,
                            CreateDate = t1.CreateDate,
                            ModifyDate = t1.ModifyDate
                        })
                        .MergeTable()
                        .OrderBy(sSortField, sSortOrder)
                        .ToPageList(pml.PageIndex, bExcel ? 100000 : pml.PageSize, ref iPageCount);

                    pml.Total = iPageCount;
                    
                    rm = new SuccessResponseMessage(null, i_crm);

                    if (bExcel)
                    {
                        var sFileName = "";
                        var oHeader = new Dictionary<string, string>();
                        var listMerge = new List<Dictionary<string, int>>();
                        var dicAlain = new Dictionary<string, string>();
                        var dt_new = new DataTable();
                        var saContactor = pml.DataList;
                        var type = saContactor;
                        var saSatisfactionCase = pml.DataList as List<View_CRM_SatisfactionCase>;
                        
                        sFileName = "滿意度案件";
                        oHeader = new Dictionary<string, string>
                                                {
                                                    { "CaseName", "滿意度案件名稱" },
                                                    { "Exhibitioname_TW", "展覽名稱" },
                                                    { "ExhibitionDate", "展覽區間" },
                                                    { "CreateUser", "創建人" },
                                                    { "CreateDate", "創建時間" },
                                                    { "ModifyDate", "最新修改時間" }
                                                };
                        dt_new.Columns.Add("CaseName");
                        dt_new.Columns.Add("Exhibitioname_TW");
                        dt_new.Columns.Add("ExhibitionDate");
                        dt_new.Columns.Add("CreateUser");
                        dt_new.Columns.Add("CreateDate");
                        dt_new.Columns.Add("ModifyDate");
                        foreach (var item in saSatisfactionCase)
                        {
                            var sDateRange = "";
                            var sStart = "";
                            var sEnd = "";
                            if (item.ExhibitionDateStart != null && item.ExhibitionDateEnd != null)
                            {
                                sStart = Convert.ToDateTime(item.ExhibitionDateStart).ToString("yyyy/MM/dd");
                                sEnd = Convert.ToDateTime(item.ExhibitionDateEnd).ToString("yyyy/MM/dd");
                                sDateRange = sStart + "~" + sEnd;
                            }
                            
                            var row_new = dt_new.NewRow();
                            row_new["CaseName"] = item.CaseName;
                            row_new["Exhibitioname_TW"] = item.Exhibitioname_TW;
                            row_new["ExhibitionDate"] = sDateRange;
                            row_new["CreateUser"] = item.CreateUser;
                            row_new["CreateDate"] = item.CreateDate;
                            row_new["ModifyDate"] = item.ModifyDate;
                            dt_new.Rows.Add(row_new);
                        }
                        dicAlain = ExcelService.GetExportAlain(oHeader, "ExhibitionCode,ExhibitionDateStart,IsShowWebSite,CreateUserName,CreateDate");
                        var bOk = new ExcelService().CreateExcelByTb(dt_new, out string sPath, oHeader, dicAlain, listMerge, sFileName);

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
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(SatisfactionCase_QryService), "", "QueryPage（滿意度案件分頁查詢）", "", "", "");
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

        #endregion

        #region 滿意度案件單筆查詢
        /// <summary>
        /// 滿意度案件單筆查詢
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
                    var saSatisfactionCase = new List<Dictionary<string, object>>();

                    var sSN = _fetchString(i_crm, @"SN");

                    var SatisfactionCaseList = db.Queryable<OTB_CRM_SatisfactionCase, OTB_OPM_Exhibition>
                        ((t1, t2) =>
                        new object[] {
                                JoinType.Left, t1.ExhibitionNO == t2.SN.ToString()
                              }
                        )                    
                        .Select((t1, t2) => new View_CRM_SatisfactionCase
                        {
                            SN = t1.SN,
                            ExhibitionNO=t1.ExhibitionNO,
                            CaseName = t1.CaseName,
                            Exhibitioname_TW = t2.Exhibitioname_TW,
                            ExhibitionDateStart = t2.ExhibitionDateStart,
                            ExhibitionDateEnd = t2.ExhibitionDateEnd,
                            ResponsiblePerson = t2.ResponsiblePerson,
                            CreateDate = t2.CreateDate
                        })
                        .MergeTable()
                        .Single(it=> it.SN == sSN);

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, SatisfactionCaseList);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(SatisfactionCase_QryService), "", "QueryOne（滿意度案件（單筆查詢））", "", "", "");
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

        #endregion

       
    }
}
