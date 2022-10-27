using EasyBL.WebApi.Message;
using Entity.Sugar;
using Entity.ViewModels;
using SqlSugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;
using System.Data;

namespace EasyBL.WEBAPP.EIP
{
    public class Leave_QryService : ServiceBase
    {
        #region 請假單分頁查詢

        /// <summary>
        /// 請假單分頁查詢
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

                    var sKeyNote = _fetchString(i_crm, @"KeyNote") ?? "";
                    var sAskTheDummy = _fetchString(i_crm, @"AskTheDummy");
                    var sImportant = _fetchString(i_crm, @"Important") ?? "";
                    var sStatus = _fetchString(i_crm, @"Status");
                    var sRoles = _fetchString(i_crm, @"Roles") ?? "Admin";
                    var sLeaveDateStart = _fetchString(i_crm, @"LeaveDateStart");
                    var sLeaveDateEnd = _fetchString(i_crm, @"LeaveDateEnd");
                    var sHolidayCategory = _fetchString(i_crm, @"HolidayCategory");
                    var bExcel = _fetchBool(i_crm, @"Excel");

                    var rLeaveDateStart = new DateTime();
                    var rLeaveDateEnd = new DateTime();
                    if (!string.IsNullOrEmpty(sLeaveDateStart))
                    {
                        rLeaveDateStart = SqlFunc.ToDate(sLeaveDateStart);
                    }
                    if (!string.IsNullOrEmpty(sLeaveDateEnd))
                    {
                        rLeaveDateEnd = SqlFunc.ToDate(sLeaveDateEnd).AddDays(1);
                    }

                    pml.DataList = db.Queryable<OTB_EIP_Leave, OTB_SYS_Members, OTB_SYS_Members, OTB_SYS_Members, OTB_SYS_Arguments>
                        ((t1, t2, t3, t4, t5) =>
                        new object[] {
                                JoinType.Inner, t1.OrgID == t2.OrgID && t1.AskTheDummy == t2.MemberID,
                                JoinType.Inner, t1.OrgID == t3.OrgID && t1.Agent_Person == t3.MemberID,
                                JoinType.Inner, t1.OrgID == t4.OrgID && t1.Handle_Person == t4.MemberID,
                                JoinType.Inner, t1.OrgID == t5.OrgID && t1.HolidayCategory == t5.ArgumentID && t5.ArgumentClassID=="LeaveType"
                              }
                        )
                        .Where((t1, t2, t3, t4) => t1.OrgID == i_crm.ORIGID && t1.KeyNote.Contains(sKeyNote))
                        .WhereIF(!string.IsNullOrEmpty(sImportant), (t1, t2, t3, t4) => sImportant.Contains(t1.Important))
                        .WhereIF(!string.IsNullOrEmpty(sAskTheDummy), (t1, t2, t3, t4) => t1.AskTheDummy == sAskTheDummy)
                        .WhereIF(!string.IsNullOrEmpty(sStatus), (t1, t2, t3, t4) => sStatus.Contains(t1.Status))
                        .WhereIF(!(sRoles.Contains("Admin") || sRoles.Contains("EipManager") || sRoles.Contains("EipView")), (t1, t2, t3, t4) => (t1.AskTheDummy == i_crm.USERID || t1.Handle_Person == i_crm.USERID || t1.Agent_Person == i_crm.USERID || t1.CheckFlows.Contains(i_crm.USERID)))
                        .WhereIF(!string.IsNullOrEmpty(sLeaveDateStart), (t1, t2, t3, t4) => t1.EndDate >= rLeaveDateStart.Date)
                        .WhereIF(!string.IsNullOrEmpty(sLeaveDateEnd), (t1, t2, t3, t4) => t1.StartDate <= rLeaveDateEnd.Date)
                        .WhereIF(!string.IsNullOrEmpty(sHolidayCategory), (t1, t2, t3, t4) => t1.HolidayCategory == sHolidayCategory)
                        .Select((t1, t2, t3, t4, t5) => new View_EIP_Leave
                        {
                            Guid = SqlFunc.GetSelfAndAutoFill(t1.Guid),
                            WenZhongAcount = t2.WenZhongAcount,
                            AskTheDummyName = t2.MemberName,
                            Agent_PersonName = t3.MemberName,
                            Handle_PersonName = t4.MemberName,
                            HolidayCategoryName = t5.ArgumentValue
                        })
                        .MergeTable()
                        .OrderBy(sSortField, sSortOrder)
                        .ToPageList(pml.PageIndex, bExcel ? 100000 : pml.PageSize, ref iPageCount);
                    pml.Total = iPageCount;

                    rm = new SuccessResponseMessage(null, i_crm);
                    if (bExcel)
                    {
                        const string sFileName = "請假單";
                        var oHeader = new Dictionary<string, string>
                        {
                            { "RowIndex", "項次" },
                            { "WenZhongAcount", "員工代號" },
                            { "AskTheDummyName", "員工姓名" },
                            { "HolidayCategory", "假別" },
                            { "AttendanceDate", "出勤日期" },
                            { "DateStart", "開始日期" },
                            { "TimeStart", "開始時間" },
                            { "DateEnd", "結束日期" },
                            { "TimeEnd", "結束時間" },
                            { "TotalTime", "請假時數 " },
                            { "Field1", "扣伙食費 " },
                            { "Field2", "扣交通津貼 " },
                            { "LeaveReason", "事由" },
                            { "Memo", "備註" },
                            { "Field3", "工作門市代號" },
                            { "Field4", "職務技能代號" }
                        };
                        var dt_new = new DataTable();
                        dt_new.Columns.Add("RowIndex");
                        dt_new.Columns.Add("WenZhongAcount");
                        dt_new.Columns.Add("AskTheDummyName");
                        dt_new.Columns.Add("HolidayCategory");
                        dt_new.Columns.Add("AttendanceDate");
                        dt_new.Columns.Add("DateStart");
                        dt_new.Columns.Add("TimeStart");
                        dt_new.Columns.Add("DateEnd");
                        dt_new.Columns.Add("TimeEnd");
                        dt_new.Columns.Add("TotalTime");
                        dt_new.Columns.Add("Field1");
                        dt_new.Columns.Add("Field2");
                        dt_new.Columns.Add("LeaveReason");
                        dt_new.Columns.Add("Memo");
                        dt_new.Columns.Add("Field3");
                        dt_new.Columns.Add("Field4");

                        var listMerge = new List<Dictionary<string, int>>();
                        var saLeave = pml.DataList as List<View_EIP_Leave>;
                        foreach (var item in saLeave)
                        {
                            var row_new = dt_new.NewRow();
                            row_new["RowIndex"] = item.RowIndex;
                            row_new["WenZhongAcount"] = item.WenZhongAcount;
                            row_new["AskTheDummyName"] = item.AskTheDummyName;
                            row_new["HolidayCategory"] = item.HolidayCategory;
                            row_new["DateStart"] = Convert.ToDateTime(item.StartDate).ToString("yyyy/MM/dd");
                            row_new["TimeStart"] = Convert.ToDateTime(item.StartDate).ToString("HH:mm");
                            row_new["DateEnd"] = Convert.ToDateTime(item.EndDate).ToString("yyyy/MM/dd");
                            row_new["TimeEnd"] = Convert.ToDateTime(item.EndDate).ToString("HH:mm");
                            row_new["AttendanceDate"] = row_new["DateStart"];
                            row_new["TotalTime"] = item.TotalTime;
                            row_new["LeaveReason"] = Common.CutByteString(item.LeaveReason, 40);
                            row_new["Field1"] = "";
                            row_new["Field2"] = "";
                            row_new["Field3"] = "";
                            row_new["Field4"] = "";
                            row_new["Memo"] = item.Memo;
                            dt_new.Rows.Add(row_new);
                        }
                        var dicAlain = ExcelService.GetExportAlain(oHeader, "WenZhongAcount,AskTheDummyName,HolidayCategory,TotalTime,DateStart,TimeStart,DateEnd,TimeEnd,AttendanceDate");

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
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBAPP.EIP.Leave_QryService", "", "QueryPage（請假單分頁查詢）", "", "", "");
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

        #endregion 請假單分頁查詢

        #region 請假單單筆查詢

        /// <summary>
        /// 請假單單筆查詢
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

                    var oEntity = db.Queryable<OTB_EIP_Leave, OTB_SYS_Members, OTB_SYS_Departments, OTB_SYS_Members, OTB_SYS_Members, OTB_SYS_Arguments>
                        ((t1, t2, t3, t4, t5, t6) =>
                        new object[] {
                                JoinType.Inner, t1.OrgID == t2.OrgID && t1.AskTheDummy == t2.MemberID,
                                JoinType.Inner, t2.OrgID == t3.OrgID && t2.DepartmentID == t3.DepartmentID,
                                JoinType.Inner, t1.OrgID == t4.OrgID && t1.Agent_Person == t4.MemberID,
                                JoinType.Inner, t1.OrgID == t5.OrgID && t1.Handle_Person == t5.MemberID,
                                JoinType.Inner, t1.OrgID == t6.OrgID && t1.HolidayCategory == t6.ArgumentID && t6.ArgumentClassID=="LeaveType"
                              }
                        )
                        .Where((t1, t2, t3, t4, t5, t6) => t1.OrgID == i_crm.ORIGID && t1.Guid == sId)
                        .Select((t1, t2, t3, t4, t5, t6) => new View_EIP_Leave
                        {
                            Guid = SqlFunc.GetSelfAndAutoFill(t1.Guid),
                            WenZhongAcount = t2.WenZhongAcount,
                            AskTheDummyName = t2.MemberName,
                            DeptName = t3.DepartmentName,
                            Agent_PersonName = t4.MemberName,
                            Handle_PersonName = t5.MemberName,
                            HolidayCategoryName = t6.ArgumentValue
                        }).Single();
                    if (!string.IsNullOrEmpty(oEntity.RelationId))
                    {
                        var oRelation = db.Queryable<OTB_EIP_Leave>().Single(x => x.OrgID == i_crm.ORIGID && x.Guid == oEntity.RelationId);
                        if (oRelation != null)
                        {
                            oEntity.ExFeild1 = oRelation.KeyNote;
                        }
                    }

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, oEntity);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBAPP.EIP.Leave_QryService", "", "QueryOne（請假單單筆查詢）", "", "", "");
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

        #endregion 請假單單筆查詢
    }
}