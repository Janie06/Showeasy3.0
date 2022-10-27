using EasyBL.WebApi.Message;
using Entity.Sugar;
using Entity.ViewModels;
using SqlSugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;
using System.Data;

namespace EasyBL.WEBAPP.CRM
{
    public class Contactors_QryService : ServiceBase
    {
        #region 聯絡人管理分頁查詢

        /// <summary>
        /// 聯絡人管理分頁查詢
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
                    var sContactorName = _fetchString(i_crm, @"ContactorName");
                    var sCustomerName = _fetchString(i_crm, @"CustomerName");
                    var sUniCode = _fetchString(i_crm, @"UniCode");
                    var sCreateUser = _fetchString(i_crm, @"CreateUser");
                    var sEmail = _fetchString(i_crm, @"Email");
                    var sPreferences = _fetchString(i_crm, @"Preferences");
                    var sPersonality = _fetchString(i_crm, @"Personality");
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

                    pml.DataList = db.Queryable<OTB_CRM_Contactors, OTB_CRM_Customers>
                        ((t1, t2) =>
                        new object[] {
                                JoinType.Inner, t1.CustomerId == t2.guid
                              }
                        )
                        //.Where((t1, t2) => t2.Effective == "Y" && t1.ContactorName.Contains(sContactorName) && t1.Email1.Contains(sEmail) && t1.Preferences.Contains(sPreferences))
                        .Where((t1, t2) => t2.Effective == "Y" && t1.ContactorName.Contains(sContactorName) && t2.UniCode.Contains(sUniCode) && (t2.CustomerCName.Contains(sCustomerName) || t2.CustomerShotCName.Contains(sCustomerName) || t2.CustomerEName.Contains(sCustomerName) || t2.CustomerShotEName.Contains(sCustomerName)))
                        .WhereIF(!string.IsNullOrEmpty(sEmail), (t1, t2) => t1.Email1.Contains(sEmail))
                        .WhereIF(!string.IsNullOrEmpty(sPreferences), (t1, t2) => t1.Preferences.Contains(sPreferences))
                        .WhereIF(!string.IsNullOrEmpty(sPersonality), (t1, t2) => t1.Personality.Contains(sPersonality))
                        .WhereIF(!string.IsNullOrEmpty(sDateStart) && sDateType == "1", (t1, t2) => t1.CreateDate >= rDateStart.Date)
                        .WhereIF(!string.IsNullOrEmpty(sDateEnd) && sDateType == "1", (t1, t2) => t1.CreateDate <= rDateEnd.Date)
                        .WhereIF(!string.IsNullOrEmpty(sDateStart) && sDateType == "2", (t1, t2) => t1.ModifyDate >= rDateStart.Date)
                        .WhereIF(!string.IsNullOrEmpty(sDateEnd) && sDateType == "2", (t1, t2) => t1.ModifyDate <= rDateEnd.Date)
                        .WhereIF(!string.IsNullOrEmpty(sDateStart) && sDateType == "3", (t1, t2) => int.Parse(t1.Birthday.Replace("/", "")) >= int.Parse(sDateStart.Replace("/", "")))
                        .WhereIF(!string.IsNullOrEmpty(sDateEnd) && sDateType == "3", (t1, t2) => int.Parse(t1.Birthday.Replace("/", "")) <= int.Parse(sDateEnd.Replace("/", "")))
                        .WhereIF(!string.IsNullOrEmpty(sCreateUser), (t1, t2) => t1.CreateUser == sCreateUser)
                        .Select((t1, t2) => new View_CRM_Contactors
                        {
                            guid = t1.guid,
                            Call = t1.Call,
                            ContactorName = t1.ContactorName,
                            JobTitle = t1.JobTitle,
                            CustomerShotCName = t2.CustomerShotCName,
                            UniCode = t2.UniCode,
                            Telephone1 = t1.Telephone1,
                            Ext1 = t1.Ext1,
                            Email1 = t1.Email1,
                            Birthday = t1.Birthday,
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
                        var saContactors = pml.DataList as List<View_CRM_Contactors>;
                        sFileName = "聯絡人管理";
                        oHeader = new Dictionary<string, string>
                                                {
                                                    { "Call", "項次" },
                                                    { "ContactorName", "聯絡人姓名" },
                                                    { "JobTitle", "職稱" },
                                                    { "CustomerShotCName", "客戶簡稱" },
                                                    { "UniCode", "統一編號" },
                                                    { "Telephone1", "電話" },
                                                    { "Ext1", "分機" },
                                                    { "Email1", "Email" },
                                                    { "Birthday", "生日" },
                                                    { "ModifyDate", "最新修改時間" }
                                                };
                        dt_new.Columns.Add("Call");
                        dt_new.Columns.Add("ContactorName");
                        dt_new.Columns.Add("JobTitle");
                        dt_new.Columns.Add("CustomerShotCName");
                        dt_new.Columns.Add("UniCode");
                        dt_new.Columns.Add("Telephone1");
                        dt_new.Columns.Add("Ext1");
                        dt_new.Columns.Add("Email1");
                        dt_new.Columns.Add("Birthday");
                        dt_new.Columns.Add("ModifyDate");
                        foreach (var item in saContactors)
                        {
                            var row_new = dt_new.NewRow();
                            var sCall = "";
                            if (item.Call == "1")
                            {
                                sCall = "Mr.";
                            }
                            else if (item.Call == "2")
                            {
                                sCall = "Miss.";
                            }
                            else
                            {
                                sCall = "";
                            }
                            row_new["Call"] = sCall;
                            row_new["ContactorName"] = item.ContactorName;
                            row_new["JobTitle"] = item.JobTitle;
                            row_new["CustomerShotCName"] = item.CustomerShotCName;
                            row_new["UniCode"] = item.UniCode;
                            row_new["Telephone1"] = item.Telephone1;
                            row_new["Ext1"] = item.Ext1;
                            row_new["Email1"] = item.Email1;
                            row_new["Birthday"] = item.Birthday;
                            row_new["ModifyDate"] = item.ModifyDate;
                            dt_new.Rows.Add(row_new);
                        }
                        //dicAlain = ExcelService.GetExportAlain(oHeader, "ExhibitionCode,ExhibitionDateStart,IsShowWebSite,CreateUserName,CreateDate");
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
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBAPP.OPM.Contactors_QryService", "", "QueryPage（聯絡人管理分頁查詢）", "", "", "");
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

        #endregion 聯絡人管理分頁查詢

        #region 聯絡人管理單筆查詢

        /// <summary>
        /// 聯絡人管理單筆查詢
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

                    var oEntity = db.Queryable<OTB_CRM_Contactors, OTB_CRM_Customers>
                        ((t1, t2) =>
                        new object[] {
                                JoinType.Inner, t1.CustomerId == t2.guid
                              }
                        ).Single(t1 => t1.guid == sId);

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, oEntity);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Contactors_QryService), "", "QueryOne（聯絡人管理（單筆查詢））", "", "", "");
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

        #endregion 聯絡人管理單筆查詢

        #region 聯絡人管理雙筆查詢

        /// <summary>
        /// 聯絡人管理雙筆查詢
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage QueryTwo(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sId1 = _fetchString(i_crm, @"guid1");
                    var sId2 = _fetchString(i_crm, @"guid2");

                    //var oEntity = db.Queryable<OTB_CRM_Contactors>().Where(x => x.guid == sId1 || x.guid == sId2).ToList();
                    //var oEntity = db.Queryable<OTB_CRM_Contactors>().Where(x => x.guid == sId1 || x.guid == sId2)
                    //    .Select(x => new View_CRM_Contactors { CustomerCName = x.CustomerId }).ToList();

                    var oEntity = db.Queryable<OTB_CRM_Contactors, OTB_CRM_Customers>
                        ((t1, t2) =>
                        new object[] {
                                JoinType.Inner, t1.CustomerId == t2.guid
                              }
                        )
                        .Where((t1, t2) => t1.guid == sId1 || t1.guid == sId2)
                        .Select((t1, t2) => new View_CRM_Contactors
                        {
                            CustomerId = t1.CustomerId,
                            CustomerCName = t2.CustomerCName,
                            CustomerShotCName = t2.CustomerShotCName,
                            CustomerEName = t2.CustomerEName,
                            ContactorName = t1.ContactorName,
                            NickName = t1.NickName,
                            Call = t1.Call,
                            Birthday = t1.Birthday,
                            MaritalStatus = t1.MaritalStatus,
                            PersonalMobilePhone = t1.PersonalMobilePhone,
                            PersonalEmail = t1.PersonalEmail,
                            LINE = t1.LINE,
                            WECHAT = t1.WECHAT,
                            Personality = t1.Personality,
                            Preferences = t1.Preferences,
                            PersonalAddress = t1.PersonalAddress,
                            Memo = t1.Memo,
                            ImmediateSupervisor = t1.ImmediateSupervisor,
                            JobTitle = t1.JobTitle,
                            Department = t1.Department,
                            Email1 = t1.Email1,
                            Email2 = t1.Email2,
                            Telephone1 = t1.Telephone1,
                            Telephone2 = t1.Telephone2,
                            Ext1 = t1.Ext1,
                            Ext2 = t1.Ext2,
                            ChoseReason = t1.ChoseReason
                        })
                        .MergeTable()
                        .ToList();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, oEntity);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Contactors_QryService), "", "QueryTwo（聯絡人管理（雙筆查詢））", "", "", "");
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

        #endregion 聯絡人管理雙筆查詢

        #region 聯絡人管理（客戶關聯查詢）

        /// <summary>
        /// 聯絡人管理（客戶關聯查詢）
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage QueryByCustomer(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sId = _fetchString(i_crm, @"CustomerId");

                    var oEntity = db.Queryable<OTB_CRM_Contactors>().Where(x => x.CustomerId == sId).OrderBy("OrderByValue", "asc").ToList();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, oEntity);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Contactors_QryService), "", "QueryByCustomer（聯絡人管理（客戶關聯查詢））", "", "", "");
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

        #endregion 聯絡人管理（客戶關聯查詢）
    }
}