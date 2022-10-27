using EasyBL.WebApi.Message;
using Entity.Sugar;
using Entity.ViewModels;
using SqlSugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;
using System.Data;

namespace EasyBL.WEBAPP.OPM
{
    public class Exhibition_QryService : ServiceBase
    {
        #region 展覽管理分頁查詢

        /// <summary>
        /// 展覽管理分頁查詢
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

                    var sExhibitionCode = _fetchString(i_crm, @"ExhibitionCode");
                    var sExhibitioname = _fetchString(i_crm, @"Exhibitioname");
                    //var sExhibitionDateStart = _fetchString(i_crm, @"ExhibitionDateStart");
                    //var sExhibitionDateEnd = _fetchString(i_crm, @"ExhibitionDateEnd");
                    //var sCreateDateStart = _fetchString(i_crm, @"CreateDateStart");
                    //var sCreateDateEnd = _fetchString(i_crm, @"CreateDateEnd");
                    //var sModifyDateStart = _fetchString(i_crm, @"ModifyDateStart");
                    //var sModifyDateEnd = _fetchString(i_crm, @"ModifyDateEnd");
                    //var sIsShowWebSite = _fetchString(i_crm, @"IsShowWebSite");
                    var sCreateUser = _fetchString(i_crm, @"CreateUser");
                    //var sArea = _fetchString(i_crm, @"Area");
                    var sState = _fetchString(i_crm, @"State");
                    //var sExhibitionAddress = _fetchString(i_crm, @"ExhibitionAddress");
                    var sEffective = _fetchString(i_crm, @"Effective");
                    var sIsTransfer = _fetchString(i_crm, @"IsTransfer");
                    var sIsDeal = _fetchString(i_crm, @"IsDeal");
                    var bExcel = _fetchBool(i_crm, @"Excel");
                    var sExcelType = _fetchString(i_crm, @"ExcelType");

                    //var rExhibitionDateStart = new DateTime();
                    //var rExhibitionDateEnd = new DateTime();
                    //var rCreateDateStart = new DateTime();
                    //var rCreateDateEnd = new DateTime();
                    //var rModifyDateStart = new DateTime();
                    //var rModifyDateEnd = new DateTime();
                    //if (!string.IsNullOrEmpty(sExhibitionDateStart))
                    //{
                    //    rExhibitionDateStart = SqlFunc.ToDate(sExhibitionDateStart);
                    //}
                    //if (!string.IsNullOrEmpty(sExhibitionDateEnd))
                    //{
                    //    rExhibitionDateEnd = SqlFunc.ToDate(sExhibitionDateEnd).AddDays(1);
                    //}
                    //if (!string.IsNullOrEmpty(sCreateDateStart))
                    //{
                    //    rCreateDateStart = SqlFunc.ToDate(sCreateDateStart);
                    //}
                    //if (!string.IsNullOrEmpty(sCreateDateEnd))
                    //{
                    //    rCreateDateEnd = SqlFunc.ToDate(sCreateDateEnd).AddDays(1);
                    //}
                    //if (!string.IsNullOrEmpty(sModifyDateStart))
                    //{
                    //    rModifyDateStart = SqlFunc.ToDate(sModifyDateStart);
                    //}
                    //if (!string.IsNullOrEmpty(sModifyDateEnd))
                    //{
                    //    rModifyDateEnd = SqlFunc.ToDate(sModifyDateEnd).AddDays(1);
                    //}

                    var sIndustry = _fetchString(i_crm, @"Industry");
                    var sListSource = _fetchString(i_crm, @"ListSource");
                    var sResponsiblePerson = _fetchString(i_crm, @"ResponsiblePerson");
                    var sDateType = _fetchString(i_crm, @"DateType");
                    var sDateStart = _fetchString(i_crm, @"DateStart");
                    var sDateEnd = _fetchString(i_crm, @"DateEnd");
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

                    pml.DataList = db.Queryable<OTB_OPM_Exhibition, OTB_SYS_Members, OTB_SYS_Arguments, OTB_SYS_Arguments, OTB_SYS_Arguments>
                        ((t1, t2, t3, t4, t5) =>
                        new object[] {
                                JoinType.Left, t1.OrgID == t2.OrgID && t1.CreateUser == t2.MemberID,
                                JoinType.Left, t1.OrgID == t3.OrgID && t1.Area == t3.ArgumentID && t3.ArgumentClassID == "Area",
                                JoinType.Left, t1.OrgID == t4.OrgID && t1.State == t4.ArgumentID && t4.ArgumentClassID == "Area",
                                JoinType.Left, t1.OrgID == t5.OrgID && t1.ExhibitionAddress == t5.ArgumentID && t5.ArgumentClassID == "EHalls"
                              }
                        )
                        .Where((t1, t2, t3, t4, t5) => t1.ExhibitionCode.Contains(sExhibitionCode) && sEffective.Contains(t1.Effective) && sIsTransfer.Contains(t1.IsTransfer) && (t1.Exhibitioname_EN.Contains(sExhibitioname) || t1.Exhibitioname_TW.Contains(sExhibitioname) || t1.Exhibitioname_CN.Contains(sExhibitioname) || t1.ExhibitioShotName_TW.Contains(sExhibitioname) || t1.ExhibitioShotName_CN.Contains(sExhibitioname) || t1.ExhibitioShotName_EN.Contains(sExhibitioname)))
                        //.WhereIF((i_crm.ORIGID == "SG" || i_crm.ORIGID == "SE"), (t1, t2, t3, t4, t5) => t1.OrgID == i_crm.ORIGID)
                        .WhereIF((i_crm.ORIGID != "TE" && i_crm.ORIGID != "TG"), (t1, t2, t3, t4, t5) => t1.OrgID == i_crm.ORIGID)
                        .WhereIF((i_crm.ORIGID == "TE" || i_crm.ORIGID == "TG"), (t1, t2, t3, t4, t5) => t1.OrgID == "TE" || t1.OrgID == "TG")
                        //.WhereIF(!string.IsNullOrEmpty(sExhibitionDateStart), (t1, t2, t3, t4, t5) => t1.ExhibitionDateStart >= rExhibitionDateStart.Date)
                        //.WhereIF(!string.IsNullOrEmpty(sExhibitionDateEnd), (t1, t2, t3, t4, t5) => t1.ExhibitionDateEnd <= rExhibitionDateEnd.Date)
                        //.WhereIF(!string.IsNullOrEmpty(sCreateDateStart), (t1, t2, t3, t4, t5) => t1.CreateDate >= rCreateDateStart.Date)
                        //.WhereIF(!string.IsNullOrEmpty(sCreateDateEnd), (t1, t2, t3, t4, t5) => t1.CreateDate <= rCreateDateEnd.Date)
                        //.WhereIF(!string.IsNullOrEmpty(sModifyDateStart), (t1, t2, t3, t4, t5) => t1.ModifyDate >= rModifyDateStart.Date)
                        //.WhereIF(!string.IsNullOrEmpty(sModifyDateEnd), (t1, t2, t3, t4, t5) => t1.ModifyDate <= rModifyDateEnd.Date)
                        .WhereIF(!string.IsNullOrEmpty(sDateStart) && sDateType == "1", (t1, t2) => t1.ExhibitionDateStart >= rDateStart.Date)
                        .WhereIF(!string.IsNullOrEmpty(sDateEnd) && sDateType == "1", (t1, t2) => t1.ExhibitionDateEnd <= rDateEnd.Date)
                        .WhereIF(!string.IsNullOrEmpty(sDateStart) && sDateType == "2", (t1, t2) => t1.CreateDate >= rDateStart.Date)
                        .WhereIF(!string.IsNullOrEmpty(sDateEnd) && sDateType == "2", (t1, t2) => t1.CreateDate <= rDateEnd.Date)
                        .WhereIF(!string.IsNullOrEmpty(sDateStart) && sDateType == "3", (t1, t2) => t1.ModifyDate >= rDateStart.Date)
                        .WhereIF(!string.IsNullOrEmpty(sDateEnd) && sDateType == "3", (t1, t2) => t1.ModifyDate <= rDateEnd.Date)
                        //.WhereIF(!string.IsNullOrEmpty(sArea), (t1, t2, t3, t4, t5) => t1.Area == sArea)
                        .WhereIF(!string.IsNullOrEmpty(sIndustry), (t1, t2, t3, t4, t5) => t1.Industry == sIndustry)
                        .WhereIF(!string.IsNullOrEmpty(sState), (t1, t2, t3, t4, t5) => t1.State == sState)
                        //.WhereIF(!string.IsNullOrEmpty(sExhibitionAddress), (t1, t2, t3, t4, t5) => t1.ExhibitionAddress == sExhibitionAddress)
                        .WhereIF(!string.IsNullOrEmpty(sCreateUser), (t1, t2, t3, t4, t5) => t1.CreateUser == sCreateUser)
                        .WhereIF(!string.IsNullOrEmpty(sResponsiblePerson), (t1, t2, t3, t4, t5) => t1.ResponsiblePerson == sResponsiblePerson)
                        .WhereIF(sIsDeal == "Y", t1 =>
                            (SqlFunc.Subqueryable<OTB_OPM_ImportExhibition>().Where(p1 => p1.ExhibitionNO == t1.SN.ToString()).Count() > 0) ||
                            (SqlFunc.Subqueryable<OTB_OPM_ExportExhibition>().Where(p2 => p2.ExhibitionNO == t1.SN.ToString()).Count() > 0) ||
                            (SqlFunc.Subqueryable<OTB_OPM_OtherExhibition>().Where(p3 => p3.ExhibitionNO == t1.SN.ToString()).Count() > 0) ||
                            (SqlFunc.Subqueryable<OTB_OPM_OtherExhibitionTG>().Where(p4 => p4.ExhibitionNO == t1.SN.ToString()).Count() > 0)
                            )
                        .WhereIF(sIsDeal == "N", t1 =>
                            (SqlFunc.Subqueryable<OTB_OPM_ImportExhibition>().Where(p5 => p5.ExhibitionNO == t1.SN.ToString()).Count() == 0) &&
                            (SqlFunc.Subqueryable<OTB_OPM_ExportExhibition>().Where(p6 => p6.ExhibitionNO == t1.SN.ToString()).Count() == 0) &&
                            (SqlFunc.Subqueryable<OTB_OPM_OtherExhibition>().Where(p7 => p7.ExhibitionNO == t1.SN.ToString()).Count() == 0) &&
                            (SqlFunc.Subqueryable<OTB_OPM_OtherExhibitionTG>().Where(p8 => p8.ExhibitionNO == t1.SN.ToString()).Count() == 0)
                            )
                        .WhereIF(!string.IsNullOrEmpty(sListSource), t1 =>
                            (SqlFunc.Subqueryable<OTB_OPM_ExhibitionCustomers>().Where(p9 => p9.ExhibitionNO == t1.SN.ToString() && p9.ListSource == sListSource).Count() > 0)
                            )
                        .Select((t1, t2, t3, t4, t5) => new View_OPM_Exhibition
                        {
                            SN = SqlFunc.GetSelfAndAutoFill(t1.SN),
                            CreateUserName = t2.MemberName,
                            AreaName = t3.ArgumentValue,
                            Area_CN = t3.ArgumentValue_CN,
                            Area_EN = t3.ArgumentValue_EN,
                            StateName = t4.ArgumentValue,
                            State_CN = t4.ArgumentValue_CN,
                            State_EN = t4.ArgumentValue_EN,
                            ExhibitionAddressName = t5.ArgumentValue,
                            ExhibitionAddress_CN = t5.ArgumentValue_CN,
                            ExhibitionAddress_EN = t5.ArgumentValue_EN,
                            ExhibitioFullName = SqlFunc.IIF(SqlFunc.HasValue(t1.ExhibitioShotName_TW), "（" + SqlFunc.IsNull(t1.ExhibitioShotName_TW, "") + "）", "") + t1.Exhibitioname_TW,
                            Exhibitioname = t1.Exhibitioname_CN + t1.Exhibitioname_EN,
                            //IsDeal = SqlFunc.IIF(SqlFunc.Subqueryable<OTB_OPM_ImportExhibition>().Where(p1 => p1.ExhibitionNO == t1.SN.ToString()).Count() > 0, "Y", "N")
                            IsDeal = SqlFunc.MappingColumn(t1.OrgID, "dbo.[OFN_OPM_CheckExhibitionDeal](t1.SN)"),
                            ResponsiblePersonName = SqlFunc.MappingColumn(t1.OrgID, "dbo.[OFN_SYS_MemberNameByMemberIDwithoutOrgID](t1.ResponsiblePerson)")
                            //IsDeal = ((SqlFunc.Subqueryable<OTB_OPM_ImportExhibition>().Where(p1 => p1.ExhibitionNO == t1.SN.ToString()).Count() > 0) ||
                            //(SqlFunc.Subqueryable<OTB_OPM_ExportExhibition>().Where(p2 => p2.ExhibitionNO == t1.SN.ToString()).Count() > 0) ||
                            //(SqlFunc.Subqueryable<OTB_OPM_OtherExhibition>().Where(p3 => p3.ExhibitionNO == t1.SN.ToString()).Count() > 0) ||
                            //(SqlFunc.Subqueryable<OTB_OPM_OtherExhibitionTG>().Where(p4 => p4.ExhibitionNO == t1.SN.ToString()).Count() > 0) ? "Y" : "N")
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
                        var saCustomers1 = pml.DataList;
                        var saExhibition = pml.DataList as List<View_OPM_Exhibition>;
                        switch (sExcelType)
                        {
                            case "Exhibition_BasicInformation":
                                {
                                    sFileName = "展覽管理基本資料";
                                    oHeader = new Dictionary<string, string>
                                                {
                                                    { "RowIndex", "項次" },
                                                    { "ExhibitionCode", "專案代號" },
                                                    { "ExhibitioShotName_TW", "活動/展覽簡稱" },
                                                    { "Exhibitioname_TW", "活動/展覽名稱" },
                                                    { "Exhibitioname_EN", "活動/展覽英文展名" },
                                                    { "ExhibitionDateStart", "展覽期間" },
                                                    { "CreateUserName", "創建人" },
                                                    { "CreateDate", "創建時間" }
                                                };
                                    dt_new.Columns.Add("RowIndex");
                                    dt_new.Columns.Add("ExhibitionCode");
                                    dt_new.Columns.Add("ExhibitioShotName_TW");
                                    dt_new.Columns.Add("Exhibitioname_TW");
                                    dt_new.Columns.Add("Exhibitioname_EN");
                                    dt_new.Columns.Add("ExhibitionDateStart");
                                    dt_new.Columns.Add("CreateUserName");
                                    dt_new.Columns.Add("CreateDate");
                                    foreach (var exhibition in saExhibition)
                                    {
                                        var row_new = dt_new.NewRow();
                                        var sStart = "";
                                        var sEnd = "";
                                        if (exhibition.ExhibitionDateStart != null)
                                        {
                                            sStart = Convert.ToDateTime(exhibition.ExhibitionDateStart).ToString("yyyy/MM/dd");
                                        }
                                        if (exhibition.ExhibitionDateEnd != null)
                                        {
                                            sEnd = Convert.ToDateTime(exhibition.ExhibitionDateEnd).ToString("yyyy/MM/dd");
                                        }
                                        if (exhibition.CreateDate != null)
                                        {
                                            row_new["CreateDate"] = Convert.ToDateTime(exhibition.CreateDate).ToString("yyyy/MM/dd HH:mm");
                                        }
                                        else
                                        {
                                            row_new["CreateDate"] = @"";
                                        }
                                        row_new["RowIndex"] = exhibition.RowIndex;
                                        row_new["ExhibitionCode"] = exhibition.ExhibitionCode;
                                        row_new["ExhibitioShotName_TW"] = exhibition.ExhibitioShotName_TW;
                                        row_new["Exhibitioname_TW"] = exhibition.Exhibitioname_TW;
                                        row_new["Exhibitioname_EN"] = exhibition.Exhibitioname_EN;
                                        row_new["CreateUserName"] = exhibition.CreateUserName;
                                        row_new["ExhibitionDateStart"] = sStart + "~" + sEnd;
                                        dt_new.Rows.Add(row_new);
                                    }
                                    dicAlain = ExcelService.GetExportAlain(oHeader, "ExhibitionCode,ExhibitionDateStart,IsShowWebSite,CreateUserName,CreateDate");
                                }
                                break;

                            case "Exhibition_WenzhongPrjFile":
                                {
                                    sFileName = "文中專案檔";
                                    oHeader = new Dictionary<string, string>
                                                    {
                                                        { "RowIndex", "項次" },
                                                        { "ExhibitionCode", "專案代號" },
                                                        { "ExhibitioShotName_TW", "專案名稱" },
                                                        { "CusField1", "專案負責人" },
                                                        { "CusField2", "失效日期" }
                                                    };
                                    dt_new.Columns.Add("RowIndex");
                                    dt_new.Columns.Add("ExhibitionCode");
                                    dt_new.Columns.Add("ExhibitioShotName_TW");
                                    dt_new.Columns.Add("CusField1");
                                    dt_new.Columns.Add("CusField2");
                                    foreach (var exhibition in saExhibition)
                                    {
                                        var row_new = dt_new.NewRow();
                                        row_new["RowIndex"] = exhibition.RowIndex;
                                        row_new["ExhibitionCode"] = exhibition.ExhibitionCode;
                                        row_new["ExhibitioShotName_TW"] = exhibition.ExhibitioShotName_TW;
                                        row_new["CusField1"] = exhibition.CreateUser.Split('.')[0];
                                        row_new["CusField2"] = "";
                                        dt_new.Rows.Add(row_new);
                                    }
                                    dicAlain = ExcelService.GetExportAlain(oHeader, new string[] { "ExhibitionCode", "CusField1", "CusField2" });
                                }
                                break;

                            default:
                                {
                                    break;
                                }
                        }
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
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBAPP.OPM.Exhibition_QryService", "", "QueryPage（展覽管理分頁查詢）", "", "", "");
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

        #endregion 展覽管理分頁查詢

        #region 展覽管理單筆查詢

        /// <summary>
        /// 展覽管理單筆查詢
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
                    var iId = _fetchInt(i_crm, @"Guid");

                    var oExportExhibition = db.Queryable<OTB_OPM_Exhibition, OTB_SYS_Members, OTB_SYS_Arguments, OTB_SYS_Arguments, OTB_SYS_Arguments>
                        ((t1, t2, t3, t4, t5) =>
                        new object[] {
                                JoinType.Left, t1.OrgID == t2.OrgID && t1.CreateUser == t2.MemberID,
                                JoinType.Left, t1.OrgID == t3.OrgID && t1.Area == t3.ArgumentID && t3.ArgumentClassID == "Area",
                                JoinType.Left, t1.OrgID == t4.OrgID && t1.State == t4.ArgumentID && t4.ArgumentClassID == "Area",
                                JoinType.Left, t1.OrgID == t5.OrgID && t1.ExhibitionAddress == t5.ArgumentID && t5.ArgumentClassID == "EHalls"
                              }
                        )
                        .Where((t1, t2, t3) => t1.SN == iId)
                        .Select((t1, t2, t3, t4, t5) => new View_OPM_Exhibition
                        {
                            SN = SqlFunc.GetSelfAndAutoFill(t1.SN),
                            CreateUserName = t2.MemberName,
                            AreaName = t3.ArgumentValue,
                            Area_CN = t3.ArgumentValue_CN,
                            Area_EN = t3.ArgumentValue_EN,
                            StateName = t4.ArgumentValue,
                            State_CN = t4.ArgumentValue_CN,
                            State_EN = t4.ArgumentValue_EN,
                            ExhibitionAddressName = t5.ArgumentValue,
                            ExhibitionAddress_CN = t5.ArgumentValue_CN,
                            ExhibitionAddress_EN = t5.ArgumentValue_EN,
                            ExhibitioFullName = SqlFunc.IIF(SqlFunc.HasValue(t1.ExhibitioShotName_TW), "（" + SqlFunc.IsNull(t1.ExhibitioShotName_TW, "") + "）", "") + t1.Exhibitioname_TW,
                            Exhibitioname = t1.Exhibitioname_CN + t1.Exhibitioname_EN
                        }).Single();

                    //,
                    //ListSource = SqlFunc.MappingColumn(t1.ExhibitionCode, "(select distinct ListSource + ',' from OTB_OPM_ExhibitionCustomers mr where mr.ExhibitionNO = t1.SN for xml path(''))")

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, oExportExhibition);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBAPP.OPM.Exhibition_QryService", "", "QueryOne（展覽管理單筆查詢）", "", "", "");
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

        #endregion 展覽管理單筆查詢
    }
}