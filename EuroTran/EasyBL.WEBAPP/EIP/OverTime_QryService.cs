using EasyBL.WebApi.Message;
using EasyNet;
using Entity.Sugar;
using Entity.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SqlSugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace EasyBL.WEBAPP.EIP
{
    public class OverTime_QryService : ServiceBase
    {
        #region 加班申請單分頁查詢

        /// <summary>
        /// 加班申請單分頁查詢
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

                    var sKeyNote = _fetchString(i_crm, @"KeyNote");
                    var sAskTheDummy = _fetchString(i_crm, @"AskTheDummy");
                    var sImportant = _fetchString(i_crm, @"Important");
                    var sStatus = _fetchString(i_crm, @"Status");
                    var sRoles = _fetchString(i_crm, @"Roles");
                    var bExcel = _fetchBool(i_crm, @"Excel");

                    pml.DataList = db.Queryable<OTB_EIP_OverTime, OTB_SYS_Members, OTB_SYS_Departments, OTB_SYS_Members>
                        ((t1, t2, t3, t4) =>
                        new object[] {
                                JoinType.Inner, t1.OrgID == t2.OrgID && t1.AskTheDummy == t2.MemberID,
                                JoinType.Inner, t2.OrgID == t3.OrgID && t2.DepartmentID == t3.DepartmentID,
                                JoinType.Inner, t1.OrgID == t4.OrgID && t1.Handle_Person == t4.MemberID
                              }
                        )
                        .Where((t1, t2, t3, t4) => t1.OrgID == i_crm.ORIGID && t1.KeyNote.Contains(sKeyNote) && sImportant.Contains(t1.Important))
                        .WhereIF(!string.IsNullOrEmpty(sAskTheDummy), (t1, t2, t3, t4) => t1.AskTheDummy == sAskTheDummy)
                        .WhereIF(!string.IsNullOrEmpty(sStatus), (t1, t2, t3, t4) => sStatus.Contains(t1.Status))
                        .WhereIF(!(sRoles.Contains("Admin") || sRoles.Contains("EipManager") || sRoles.Contains("EipView")), (t1, t2, t3, t4) => (t1.AskTheDummy == i_crm.USERID || t1.Handle_Person == i_crm.USERID || t1.CheckFlows.Contains(i_crm.USERID)))
                        .Select((t1, t2, t3, t4) => new View_EIP_OverTime
                        {
                            Guid = SqlFunc.GetSelfAndAutoFill(t1.Guid),
                            AskTheDummyName = t2.MemberName,
                            Handle_PersonName = t4.MemberName,
                            DeptName = t3.DepartmentName
                        })
                        .MergeTable()
                        .OrderBy(sSortField, sSortOrder)
                        .ToPageList(pml.PageIndex, bExcel ? 100000 : pml.PageSize, ref iPageCount);
                    pml.Total = iPageCount;

                    rm = new SuccessResponseMessage(null, i_crm);
                    if (bExcel)
                    {
                        const string sFileName = "加班單";
                        var oHeader = new Dictionary<string, string>
                        {
                            { "RowIndex", "項次" },
                            { "WenZhongAcount", "員工代號" },
                            { "EmployeeName", "員工姓名" },
                            { "OvertimeClass", "加班別" },
                            { "AttendanceDate", "出勤日期" },
                            { "StartDate", "開始日期" },
                            { "StartTime", "開始時間" },
                            { "EndDate", "結束日期" },
                            { "EndTime", "結束時間" },
                            { "OvertimeHours", "加班時數 " },
                            { "TakeTimeOffHours", "補休時數 " },
                            { "Field1", "補休期限 " },
                            { "Field2", "抵用時數 " },
                            { "Field3", "誤餐費 " },
                            { "PrjCode", "專案代號 " },
                            { "Reason", "事由" },
                            { "Memo", "備註" },
                            { "Field4", "工作門市代號" },
                            { "Field5", "職務技能代號" }
                        };
                        var dt_new = new DataTable();
                        dt_new.Columns.Add("RowIndex");
                        dt_new.Columns.Add("WenZhongAcount");
                        dt_new.Columns.Add("EmployeeName");
                        dt_new.Columns.Add("OvertimeClass");
                        dt_new.Columns.Add("AttendanceDate");
                        dt_new.Columns.Add("StartDate");
                        dt_new.Columns.Add("StartTime");
                        dt_new.Columns.Add("EndDate");
                        dt_new.Columns.Add("EndTime");
                        dt_new.Columns.Add("OvertimeHours");
                        dt_new.Columns.Add("TakeTimeOffHours");
                        dt_new.Columns.Add("Field1");
                        dt_new.Columns.Add("Field2");
                        dt_new.Columns.Add("Field3");
                        dt_new.Columns.Add("PrjCode");
                        dt_new.Columns.Add("Reason");
                        dt_new.Columns.Add("Memo");
                        dt_new.Columns.Add("Field4");
                        dt_new.Columns.Add("Field5");

                        var listMerge = new List<Dictionary<string, int>>();
                        var saInvoiceApplyInfo = pml.DataList as List<View_EIP_OverTime>;
                        var saMembers = db.Queryable<OTB_SYS_Members>()
                            .Select(x => new { x.OrgID, x.MemberID, x.MemberName, x.WenZhongAcount })
                            .Where(x => x.OrgID == i_crm.ORIGID).ToList();
                        var iIndex = 1;
                        foreach (var item in saInvoiceApplyInfo)
                        {
                            var jaOverTimes = (JArray)JsonConvert.DeserializeObject(item.OverTimes);
                            foreach (JObject overtime in jaOverTimes)
                            {
                                var row_new = dt_new.NewRow();
                                var sEmployeeCode = overtime["EmployeeCode"].ToString();
                                var oEmployee = saMembers.Single(x => x.MemberID == sEmployeeCode);
                                row_new["RowIndex"] = iIndex;
                                row_new["WenZhongAcount"] = oEmployee.WenZhongAcount;
                                row_new["EmployeeName"] = overtime["EmployeeName"];
                                row_new["OvertimeClass"] = overtime["OvertimeClass"];
                                row_new["AttendanceDate"] = overtime["AttendanceDate"];
                                row_new["StartDate"] = overtime["StartDate"];
                                row_new["StartTime"] = overtime["StartTime"];
                                row_new["EndDate"] = overtime["EndDate"];
                                row_new["EndTime"] = overtime["EndTime"];
                                row_new["OvertimeHours"] = overtime["OvertimeHours"];
                                row_new["TakeTimeOffHours"] = overtime["TakeTimeOffHours"];
                                row_new["Field1"] = "";
                                row_new["Field2"] = "";
                                row_new["Field3"] = "";
                                row_new["PrjCode"] = overtime["PrjCode"];
                                row_new["Reason"] = overtime["Reason"];
                                row_new["Memo"] = "";
                                row_new["Field4"] = "";
                                row_new["Field5"] = "";
                                dt_new.Rows.Add(row_new);
                                iIndex++;
                            }
                        }
                        var dicAlain = ExcelService.GetExportAlain(oHeader, "WenZhongAcount,EmployeeName,OvertimeClass,AttendanceDate,StartDate,StartTime,EndDate,EndTime,OvertimeHours,TakeTimeOffHours,PrjCode");

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
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBAPP.EIP.OverTime_QryService", "", "QueryPage（加班申請單分頁查詢）", "", "", "");
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

        #endregion 加班申請單分頁查詢

        #region 加班申請單單筆查詢

        /// <summary>
        /// 加班申請單單筆查詢
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

                    var oEntity = db.Queryable<OTB_EIP_OverTime, OTB_SYS_Members, OTB_SYS_Departments, OTB_SYS_Members>
                        ((t1, t2, t3, t4) =>
                        new object[] {
                                JoinType.Inner, t1.OrgID == t2.OrgID && t1.AskTheDummy == t2.MemberID,
                                JoinType.Inner, t2.OrgID == t3.OrgID && t2.DepartmentID == t3.DepartmentID,
                                JoinType.Inner, t1.OrgID == t4.OrgID && t1.Handle_Person == t4.MemberID
                              }
                        )
                        .Where((t1, t2, t3, t4) => t1.OrgID == i_crm.ORIGID && t1.Guid == sId)
                        .Select((t1, t2, t3, t4) => new View_EIP_OverTime
                        {
                            Guid = SqlFunc.GetSelfAndAutoFill(t1.Guid),
                            AskTheDummyName = t2.MemberName,
                            Handle_PersonName = t4.MemberName,
                            DeptName = t3.DepartmentName
                        }).Single();
                    if (!string.IsNullOrEmpty(oEntity.RelationId))
                    {
                        var oRelation = db.Queryable<OTB_EIP_OverTime>().Single(x => x.OrgID == i_crm.ORIGID && x.Guid == oEntity.RelationId);
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
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBAPP.EIP.OverTime_QryService", "", "QueryOne（加班申請單單筆查詢）", "", "", "");
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

        #endregion 加班申請單單筆查詢
    }
}