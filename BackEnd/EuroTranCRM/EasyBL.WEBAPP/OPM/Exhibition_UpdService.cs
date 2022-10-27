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

namespace EasyBL.WEBAPP.OPM
{
    public class Exhibition_UpdService : ServiceBase
    {
        #region 獲取參加該展覽的所有廠商

        /// <summary>
        /// 獲取參加該展覽的所有廠商
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on GetCustomers</param>
        /// <returns></returns>
        public ResponseMessage GetCustomers(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sSN = _fetchString(i_crm, @"SN");
                    var sFlag = _fetchString(i_crm, @"Flag");
                    var SList = new List<ExistingCustomerInfo>();
                    var saCustomers = new List<Dictionary<string, object>>();
                    var saCustomersD = new List<Dictionary<string, object>>();
                    //出口
                    var saExport = db.Queryable<OTB_OPM_ExportExhibition, OTB_CRM_Customers>((t1, t2) => new object[] {
                          JoinType.Left,t1.Agent==t2.guid && t1.OrgID==t2.OrgID && t2.Effective == "Y"})
                          .Select((t1, t2) => new { t1.IsVoid, t1.OrgID, t1.ExhibitionNO, t1.Exhibitors, t1.Agent, AgentCName = t2.CustomerCName, AgentEName = t2.CustomerEName, t1.AgentContactorName, t1.AgentTelephone }).MergeTable()
                          .Where(it => it.ExhibitionNO == sSN && it.OrgID == i_crm.ORIGID && it.IsVoid == "N").ToList();
                    if (saExport.Count > 0)
                    {
                        foreach (var opm in saExport)
                        {
                            if (!string.IsNullOrEmpty(opm.Exhibitors))
                            {
                                var saExhibitors = (JArray)JsonConvert.DeserializeObject(opm.Exhibitors);
                                foreach (JObject jo in saExhibitors)
                                {
                                    if (jo[@"SupplierID"] != null && jo[@"SupplierID"].ToString() != @"")
                                    //if (jo[@"SupplierID"] != null && jo[@"SupplierID"].ToString() != @"" && !saCustomers.Any(x => (x[@"guid"].ToString() == jo[@"SupplierID"].ToString())))
                                    {
                                        var dic = new Dictionary<string, object>
                                        {
                                            { @"RowIndex", saCustomers.Count + 1 },
                                            { @"guid", jo[@"SupplierID"].ToString() },
                                            { @"AgentCName", string.IsNullOrEmpty(opm.AgentCName) ? opm.AgentEName : opm.AgentCName },
                                            { @"CustomerCName", jo[@"SupplierName"] == null ? @"" : jo[@"SupplierName"].ToString() },
                                            { @"CustomerEName", jo[@"SupplierEName"] == null ? @"" : jo[@"SupplierEName"].ToString() },
                                            { @"ContactorName", jo[@"ContactorName"] == null ? @"" : jo[@"ContactorName"].ToString() },
                                            { @"Telephone", jo[@"Telephone"] == null ? @"" : jo[@"Telephone"].ToString() }
                                        };
                                        saCustomers.Add(dic);
                                    }
                                }
                            }
                            //if (!string.IsNullOrEmpty(opm.Agent) && !saCustomers.Any(x => (x["guid"].ToString() == opm.Agent)))
                            //{
                            //    Dictionary<string, object> dic = new Dictionary<string, object>
                            //    {
                            //        { "RowIndex", saCustomers.Count + 1 },
                            //        { "guid", opm.Agent },
                            //        { "AgentCName", string.IsNullOrEmpty(opm.AgentCName) ? opm.AgentEName : opm.AgentCName },
                            //        { "CustomerCName", "" },
                            //        { "CustomerEName", "" },
                            //        { "ContactorName", opm.AgentContactorName },
                            //        { "Telephone", opm.AgentTelephone }
                            //    };
                            //    saCustomers.Add(dic);
                            //}
                        }
                    }
                    //進口
                    var saImport = db.Queryable<OTB_OPM_ImportExhibition, OTB_CRM_Customers, OTB_CRM_Customers>((t1, t2, t3) => new object[] {
                          JoinType.Left,t1.Supplier==t2.guid && t1.OrgID==t2.OrgID && t2.Effective == "Y",
                          JoinType.Left,t1.Agent==t3.guid && t1.OrgID==t3.OrgID && t3.Effective == "Y" })
                          .Select((t1, t2, t3) => new { t1.IsVoid, t1.OrgID, t1.ExhibitionNO, t1.Supplier, t1.Agent, t2.CustomerCName, t2.CustomerEName, AgentCName = t3.CustomerCName, AgentEName = t3.CustomerEName, t1.ContactorName, t1.Telephone, t1.AgentContactorName, t1.AgentTelephone }).MergeTable()
                          .Where(it => it.ExhibitionNO == sSN && it.OrgID == i_crm.ORIGID && it.IsVoid == "N").ToList();
                    if (saImport.Count > 0)
                    {
                        foreach (var opm in saImport)
                        {
                            if (!string.IsNullOrEmpty(opm.Supplier))
                            //if (!string.IsNullOrEmpty(opm.Supplier) && !saCustomers.Any(x => (x[@"guid"].ToString() == opm.Supplier)))
                            {
                                var dic = new Dictionary<string, object>
                                {
                                    { @"RowIndex", saCustomers.Count + 1 },
                                    { @"guid", opm.Supplier },
                                    { @"AgentCName", string.IsNullOrEmpty(opm.AgentCName) ? opm.AgentEName : opm.AgentCName },
                                    { @"CustomerCName", opm.CustomerCName },
                                    { @"CustomerEName", opm.CustomerEName },
                                    { @"ContactorName", opm.ContactorName },
                                    { @"Telephone", opm.Telephone }
                                };
                                saCustomers.Add(dic);
                            }
                            //if (!string.IsNullOrEmpty(opm.Agent) && !saCustomers.Any(x => (x["guid"].ToString() == opm.Agent)))
                            //{
                            //    Dictionary<string, object> dic = new Dictionary<string, object>
                            //    {
                            //        { "RowIndex", saCustomers.Count + 1 },
                            //        { "guid", opm.Agent },
                            //        { "AgentCName", string.IsNullOrEmpty(opm.AgentCName) ? opm.AgentEName : opm.AgentCName },
                            //        { "CustomerCName", "" },
                            //        { "CustomerEName", "" },
                            //        { "ContactorName", opm.AgentContactorName },
                            //        { "Telephone", opm.AgentTelephone }
                            //    };
                            //    saCustomers.Add(dic);
                            //}
                        }
                    }
                    //其他
                    var saOther = db.Queryable<OTB_OPM_OtherExhibition, OTB_CRM_Customers, OTB_CRM_Customers>((t1, t2, t3) => new object[] {
                          JoinType.Left,t1.Supplier==t2.guid && t1.OrgID==t2.OrgID &&  t2.Effective == "Y",
                          JoinType.Left,t1.Agent==t3.guid && t1.OrgID==t3.OrgID &&  t3.Effective == "Y"})
                          .Select((t1, t2, t3) => new { t1.IsVoid, t1.OrgID, t1.ExhibitionNO, t1.Supplier, t1.Agent, t2.CustomerCName, t2.CustomerEName, AgentCName = t3.CustomerCName, AgentEName = t3.CustomerEName, t1.ContactorName, t1.Telephone, t1.AgentContactorName, t1.AgentTelephone }).MergeTable()
                          .Where(it => it.ExhibitionNO == sSN && it.OrgID == i_crm.ORIGID && it.IsVoid == "N").ToList();
                    if (saOther.Count > 0)
                    {
                        foreach (var opm in saOther)
                        {
                            if (!string.IsNullOrEmpty(opm.Supplier))
                            //if (!string.IsNullOrEmpty(opm.Supplier) && !saCustomers.Any(x => (x[@"guid"].ToString() == opm.Supplier)))
                            {
                                var dic = new Dictionary<string, object>
                                {
                                    { @"RowIndex", saCustomers.Count + 1 },
                                    { @"guid", opm.Supplier },
                                    { @"AgentCName", string.IsNullOrEmpty(opm.AgentCName) ? opm.AgentEName : opm.AgentCName },
                                    { @"CustomerCName", opm.CustomerCName },
                                    { @"CustomerEName", opm.CustomerEName },
                                    { @"ContactorName", opm.ContactorName },
                                    { @"Telephone", opm.Telephone }
                                };
                                saCustomers.Add(dic);
                            }
                            //if (!string.IsNullOrEmpty(opm.Agent) && !saCustomers.Any(x => (x["guid"].ToString() == opm.Agent)))
                            //{
                            //    Dictionary<string, object> dic = new Dictionary<string, object>
                            //    {
                            //        { "RowIndex", saCustomers.Count + 1 },
                            //        { "guid", opm.Agent },
                            //        { "AgentCName", string.IsNullOrEmpty(opm.AgentCName) ? opm.AgentEName : opm.AgentCName },
                            //        { "CustomerCName", "" },
                            //        { "CustomerEName", "" },
                            //        { "ContactorName", opm.AgentContactorName },
                            //        { "Telephone", opm.AgentTelephone }
                            //    };
                            //    saCustomers.Add(dic);
                            //}
                        }
                    }
                    //其他（駒驛）
                    var saOtherTG = db.Queryable<OTB_OPM_OtherExhibitionTG, OTB_CRM_Customers>((t1, t2) => new object[] {
                          JoinType.Left,t1.Agent==t2.guid && t1.OrgID==t2.OrgID && t2.Effective == "Y"})
                          .Select((t1, t2) => new {t1.IsVoid, t1.OrgID, t1.ExhibitionNO, t1.Exhibitors, t1.Agent, AgentCName = t2.CustomerCName, AgentEName = t2.CustomerEName, t1.AgentContactorName, t1.AgentTelephone }).MergeTable()
                          .Where(it => it.ExhibitionNO == sSN && it.OrgID == i_crm.ORIGID && it.IsVoid == "N").ToList();
                    if (saOtherTG.Count > 0)
                    {
                        foreach (var opm in saOtherTG)
                        {
                            if (!string.IsNullOrEmpty(opm.Exhibitors))
                            {
                                var saExhibitors = (JArray)JsonConvert.DeserializeObject(opm.Exhibitors);
                                foreach (JObject jo in saExhibitors)
                                {
                                    if (jo[@"SupplierID"] != null && jo[@"SupplierID"].ToString() != @"")
                                    //if (jo[@"SupplierID"] != null && jo[@"SupplierID"].ToString() != @"" && !saCustomers.Any(x => (x[@"guid"].ToString() == jo[@"SupplierID"].ToString())))
                                    {
                                        var dic = new Dictionary<string, object>
                                        {
                                            { @"RowIndex", saCustomers.Count + 1 },
                                            { @"guid", jo[@"SupplierID"].ToString() },
                                            { @"AgentCName", string.IsNullOrEmpty(opm.AgentCName) ? opm.AgentEName : opm.AgentCName },
                                            { @"CustomerCName", jo[@"SupplierName"] == null ? @"" : jo[@"SupplierName"].ToString() },
                                            { @"CustomerEName", jo[@"SupplierEName"] == null ? @"" : jo[@"SupplierEName"].ToString() },
                                            { @"ContactorName", jo[@"ContactorName"] == null ? @"" : jo[@"ContactorName"].ToString() },
                                            { @"Telephone", jo[@"Telephone"] == null ? @"" : jo[@"Telephone"].ToString() }
                                        };
                                        saCustomers.Add(dic);
                                    }
                                }
                            }
                            //if (!string.IsNullOrEmpty(opm.Agent) && !saCustomers.Any(x => (x["guid"].ToString() == opm.Agent)))
                            //{
                            //    Dictionary<string, object> dic = new Dictionary<string, object>
                            //    {
                            //        { "RowIndex", saCustomers.Count + 1 },
                            //        { "guid", opm.Agent },
                            //        { "AgentCName", string.IsNullOrEmpty(opm.AgentCName) ? opm.AgentEName : opm.AgentCName },
                            //        { "CustomerCName", "" },
                            //        { "CustomerEName", "" },
                            //        { "ContactorName", opm.AgentContactorName },
                            //        { "Telephone", opm.AgentTelephone }
                            //    };
                            //    saCustomers.Add(dic);
                            //}
                        }
                    }
                    rm = new SuccessResponseMessage(null, i_crm);
                    var saGCs = saCustomers.GroupBy(x => x["guid"].ToString());
                    foreach (var item in saGCs)
                    {
                        var AllAgentCName = item.Where(c => c["AgentCName"] != null).Select(c => c["AgentCName"]).Distinct().ToList();
                        var Customer = item.First();
                        Dictionary<string, object> dic = new Dictionary<string, object>
                        {
                            { "RowIndex", saCustomersD.Count + 1 },
                            { "guid", Customer["guid"] },
                            { "AgentCName", string.Join(" ◆",AllAgentCName) },
                            { "CustomerCName", Customer["CustomerCName"] },
                            { "CustomerEName", Customer["CustomerEName"] },
                            { "ContactorName", Customer["ContactorName"] },
                            { "Telephone", Customer["Telephone"] }
                        };
                        saCustomersD.Add(dic);
                    }
                    if (sFlag == @"export")
                    {
                        var dicHeader = new Dictionary<string, object> {
                            {@"RowIndex",@"項次" },
                            { @"AgentCName",@"國外代理" },
                            { @"CustomerCName",@"公司中文名稱" },
                            { @"CustomerEName",@"公司英文名稱" },
                            { @"ContactorName",@"聯絡人" },
                            { @"Telephone",@"聯絡電話/手機" }
                        };
                        var oExhibition = db.Queryable<OTB_OPM_Exhibition>().Single(x => x.SN == sSN.ObjToInt());
                        var bOk = new ExcelService().CreateExcel(saCustomersD, out string sPath, dicHeader, oExhibition.Exhibitioname_TW, oExhibition.Exhibitioname_TW);
                        rm.DATA.Add(BLWording.REL, sPath);
                    }
                    else
                    {
                        rm.DATA.Add(BLWording.REL, saCustomersD);
                    }
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Exhibition_UpdService), @"展覽管理", @"GetCustomers（獲取參加該展覽的所有廠商）", @"", @"", @"");
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

        #endregion 獲取參加該展覽的所有廠商

        #region 獲取展覽資料

        /// <summary>
        /// 獲取展覽資料
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on GetExhibitions</param>
        /// <returns></returns>
        public ResponseMessage GetExhibitions(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sSN = _fetchString(i_crm, @"SN");
                    var saExhibition = db.Queryable<OTB_OPM_Exhibition>()
                          .OrderBy(x => x.ExhibitionCode, OrderByType.Desc)
                          .Where(x => x.Effective == @"Y")
                          //.WhereIF((i_crm.ORIGID == "SG" || i_crm.ORIGID == "SE"), x => x.OrgID == i_crm.ORIGID)
                          .WhereIF((i_crm.ORIGID != "TE" && i_crm.ORIGID != "TG"), x => x.OrgID == i_crm.ORIGID)
                          .WhereIF((i_crm.ORIGID == "TE" || i_crm.ORIGID == "TG"), x => x.OrgID == "TE" || x.OrgID == "TG")
                          .WhereIF(!string.IsNullOrEmpty(sSN), x => x.SN == sSN.ObjToInt())
                          .Select(x => new
                          {
                              x.SN,
                              x.ExhibitionCode,
                              x.Exhibitioname_TW,
                              x.Exhibitioname_EN,
                              ExhibitioFullName = SqlFunc.IIF(SqlFunc.HasValue(x.ExhibitioShotName_TW), "（" + SqlFunc.IsNull(x.ExhibitioShotName_TW, "") + "）", "") + x.Exhibitioname_TW,
                              x.ExhibitioShotName_TW,
                              x.ExhibitionDateStart,
                              x.ExhibitionDateEnd,
                              x.ExhibitionAddress,
                              ResponsiblePerson = SqlFunc.MappingColumn(x.Exhibitioname_TW, "dbo.[OFN_SYS_MemberNameByMemberID](OrgID,CreateUser)"),
                              x.CreateDate
                          })
                          .ToList();
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, saExhibition);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Exhibition_UpdService), @"展覽管理", @"GetExhibitions（獲取展覽資料）", @"", @"", @"");
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


        /// <summary>
        /// 獲取展覽資料
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on GetExhibitions</param>
        /// <returns></returns>
        public ResponseMessage CheckExhibitionName(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var Type = _fetchString(i_crm, @"Type").ToLower();
                    var sSN = _fetchString(i_crm, @"SN");
                    int.TryParse(sSN, out int SN);
                    var sExhibitioname_TW = _fetchString(i_crm, @"Exhibitioname_TW");
                    var sExhibitioname_TW_Alt = GetTWSameName(sExhibitioname_TW);
                    var sExhibitioShotName_TW = _fetchString(i_crm, @"ExhibitioShotName_TW");
                    var sExhibitioShotName_TW_Alt = GetTWSameName(sExhibitioShotName_TW);
                    var sExhibitioname_CN = _fetchString(i_crm, @"Exhibitioname_CN");
                    var sExhibitioname_EN = _fetchString(i_crm, @"Exhibitioname_EN");

                    //var saExhibitions = db.Queryable<OTB_OPM_Exhibition>().Where(x => x.OrgID == i_crm.ORIGID && x.Effective == @"Y" && x.SN != SN).ToList();
                    var saExhibitions = db.Queryable<OTB_OPM_Exhibition>()
                            .Where(x => x.Effective == @"Y" && x.SN != SN)
                            //.WhereIF((i_crm.ORIGID == "SG" || i_crm.ORIGID == "SE"), x => x.OrgID == i_crm.ORIGID)
                            .WhereIF((i_crm.ORIGID != "TE" && i_crm.ORIGID != "TG"), x => x.OrgID == i_crm.ORIGID)
                            .WhereIF((i_crm.ORIGID == "TE" || i_crm.ORIGID == "TG"), x => x.OrgID == "TE" || x.OrgID == "TG")
                            .ToList();
                    var RepeatShotName = saExhibitions.Any(x => x.ExhibitioShotName_TW.Trim() == sExhibitioShotName_TW || x.ExhibitioShotName_TW.Trim() == sExhibitioShotName_TW_Alt);
                    var RepeatNameTW = saExhibitions.Any(x => x.Exhibitioname_TW.Trim() == sExhibitioname_TW || x.Exhibitioname_TW.Trim() == sExhibitioname_TW_Alt);
                    var RepeatNameCN = saExhibitions.Any(x => x.Exhibitioname_CN.Trim() == sExhibitioname_CN);
                    var RepeatNameEN = saExhibitions.Any(x => x.Exhibitioname_EN.Trim() == sExhibitioname_EN);
                    if (RepeatShotName && RepeatNameTW)
                    {
                        sMsg = "活動/展覽簡稱、展覽名稱重複，請重新輸入。";
                    }
                    else if (RepeatShotName)
                    {
                        sMsg = "活動/展覽簡稱重複，請重新輸入。";
                    }
                    else if (RepeatNameTW)
                    {
                        sMsg = "展覽名稱重複，請重新輸入。";
                    }
                    else if (RepeatNameCN)
                    {
                        sMsg = "簡體名稱重複，請重新輸入。";
                    }
                    else if (RepeatNameEN)
                    {
                        sMsg = "英文展名重複，請重新輸入。";
                    }

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, "OK");
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Exhibition_UpdService), @"展覽管理", @"GetExhibitions（獲取展覽資料）", @"", @"", @"");
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

        private string GetTWSameName(string OriName)
        {
            if (string.IsNullOrWhiteSpace(OriName))
            {
                return "";
            }

            if (OriName.Contains("台灣"))
            {
                return OriName.Replace("台灣", "臺灣");
            }
            else
            {
                return OriName.Replace("臺灣", "台灣");
            }
        }
        #endregion 獲取展覽資料

        #region 獲取匯入廠商資料

        /// <summary>
        /// 獲取匯入廠商資料
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on GetImportCustomers</param>
        /// <returns></returns>
        public ResponseMessage GetImportCustomers(RequestMessage i_crm)
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
                    var iSN = _fetchInt(i_crm, @"SN");
                    var sExhibitionArea = _fetchString(i_crm, @"ExhibitionArea");
                    var sMuseumMumber = _fetchString(i_crm, @"MuseumMumber");
                    var sCustomerCName = _fetchString(i_crm, @"CustomerCName");
                    var sCustomerEName = _fetchString(i_crm, @"CustomerEName");
                    var sUniCode = _fetchString(i_crm, @"UniCode");
                    var sContactor = _fetchString(i_crm, @"Contactor");
                    var sTelephone = _fetchString(i_crm, @"Telephone");
                    var sEmail = _fetchString(i_crm, @"Email");
                    var sAddress = _fetchString(i_crm, @"Address");
                    var sMemo = _fetchString(i_crm, @"Memo");

                    pml.DataList = db.Queryable<OTB_CRM_ImportCustomers, OTB_OPM_Exhibition, OTB_WSM_PackingOrder>
                        ((t1, t2, t3) => new object[]
                                              {
                                                JoinType.Inner, t1.OrgID == t2.OrgID && t1.ExhibitionNO == t2.SN,
                                                JoinType.Left, t1.OrgID == t3.OrgID && t1.guid == t3.CustomerId
                                              }
                        )
                        .Where((t1, t2, t3) => t1.ExhibitionNO == iSN && t1.OrgID == i_crm.ORIGID)
                        .WhereIF(!String.IsNullOrEmpty(sExhibitionArea), (t1, t2, t3) => t1.ExhibitionArea.Contains(sExhibitionArea))
                        .WhereIF(!String.IsNullOrEmpty(sMuseumMumber), (t1, t2, t3) => t1.MuseumMumber.Contains(sMuseumMumber))
                        .WhereIF(!String.IsNullOrEmpty(sCustomerCName), (t1, t2, t3) => t1.CustomerCName.Contains(sCustomerCName))
                        .WhereIF(!String.IsNullOrEmpty(sCustomerEName), (t1, t2, t3) => t1.CustomerEName.Contains(sCustomerEName))
                        .WhereIF(!String.IsNullOrEmpty(sUniCode), (t1, t2, t3) => t1.UniCode.Contains(sUniCode))
                        .WhereIF(!String.IsNullOrEmpty(sContactor), (t1, t2, t3) => t1.Contactor.Contains(sContactor))
                        .WhereIF(!String.IsNullOrEmpty(sTelephone), (t1, t2, t3) => t1.Telephone.Contains(sTelephone))
                        .WhereIF(!String.IsNullOrEmpty(sEmail), (t1, t2, t3) => t1.Email.Contains(sEmail))
                        .WhereIF(!String.IsNullOrEmpty(sAddress), (t1, t2, t3) => t1.Address.Contains(sAddress))
                        .WhereIF(!String.IsNullOrEmpty(sMemo), (t1, t2, t3) => t1.Memo.Contains(sMemo))
                        .Select((t1, t2, t3) => new View_CRM_ImportCustomers
                        {
                            guid = SqlFunc.GetSelfAndAutoFill(t1.guid),
                            Exhibitioname_TW = t2.Exhibitioname_TW,
                            Exhibitioname_EN = t2.Exhibitioname_EN,
                            IsAppoint = SqlFunc.IIF(SqlFunc.HasValue(t1.AppointNO), "Y", "N")
                        })
                        .MergeTable()
                        .OrderBy(sSortField, sSortOrder)
                        .ToPageList(pml.PageIndex, pml.PageSize, ref iPageCount);
                    pml.Total = iPageCount;
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, pml);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Exhibition_UpdService), @"展覽管理", @"GetImportCustomers（獲取匯入廠商資料）", @"", @"", @"");
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

        #endregion 獲取匯入廠商資料

        #region 匯入廠商資料

        /// <summary>
        /// 匯入廠商資料
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on ImportCustomers</param>
        /// <returns></returns>
        public ResponseMessage ImportCustomers(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sFileId = _fetchString(i_crm, @"FileId");
                    var sFileName = _fetchString(i_crm, @"FileName");
                    var iSN = _fetchInt(i_crm, @"SN");
                    var sRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"OutFiles\Temporary\");//Word模版路徑
                    var sfileName = sFileName.Split(new string[] { @"." }, StringSplitOptions.RemoveEmptyEntries);
                    var sSubFileName = sfileName.LastOrDefault();     //副檔名
                    sFileName = sRoot + sFileId + @"." + sSubFileName;

                    var book = new Workbook(sFileName);

                    var sheet = book.Worksheets[0];
                    var cells = sheet.Cells;
                    var tbFeeItems = cells.ExportDataTableAsString(1, 0, cells.MaxDataRow, cells.MaxDataColumn + 1, false);

                    if (tbFeeItems.Rows.Count > 0)
                    {
                        var saImportCustomers = new List<OTB_CRM_ImportCustomers>();
                        foreach (DataRow row in tbFeeItems.Rows)
                        {
                            try
                            {
                                var sExhibitionArea = row[@"Column1"].ToString();// 展覽地區
                                var sMuseumMumber = row[@"Column2"].ToString();// 攤位編號
                                var sUniCode = row[@"Column3"].ToString();// 統一編號
                                var sCustomerCName = row[@"Column4"].ToString();// 公司中文名稱
                                var sCustomerEName = row[@"Column5"].ToString();// 公司英文名稱
                                var sContactor = row[@"Column7"].ToString();// 聯絡人1
                                var sTelephone = row[@"Column8"].ToString();// 電話
                                var sAddress = row[@"Column9"].ToString();//地址
                                var sEmail = row[@"Column10"].ToString();// 郵箱
                                var sMemo = row[@"Column11"].ToString();// 備註
                                sCustomerCName = sCustomerCName.Trim();// 公司中文名字去空格
                                var saImportCustomers_Exsit = db.Queryable<OTB_CRM_ImportCustomers>()
                                      .Where(x => x.OrgID == i_crm.ORIGID)
                                      .Where(x => x.CustomerCName == sCustomerCName)
                                      .ToList();
                                var saCustomers_Exsit = db.Queryable<OTB_CRM_Customers>()
                                      .Where(x => x.OrgID == i_crm.ORIGID && x.CustomerCName == sCustomerCName)
                                      .ToList();
                                var saImportCustomers_Cur = saImportCustomers_Exsit.Where(x => x.ExhibitionNO == iSN).ToList();
                                if (saImportCustomers_Cur.Count == 0)
                                {
                                    var oImportCustomers = new OTB_CRM_ImportCustomers
                                    {
                                        guid = Guid.NewGuid().ToString(),
                                        OrgID = i_crm.ORIGID,
                                        ExhibitionNO = iSN,
                                        MuseumMumber = sMuseumMumber,
                                        ExhibitionArea = sExhibitionArea,
                                        CustomerCName = sCustomerCName,
                                        CustomerEName = sCustomerEName,
                                        UniCode = sUniCode,
                                        Contactor = sContactor,
                                        Telephone = sTelephone,
                                        Address = sAddress,
                                        Email = sEmail,
                                        Memo = sMemo,
                                        IsFormal = saCustomers_Exsit.Count > 0 ? true : saImportCustomers_Exsit.Count > 0 ? saImportCustomers_Exsit.First().IsFormal : false,
                                        TransactionType = "",
                                        CreateUser = i_crm.USERID,
                                        CreateDate = DateTime.Now,
                                        ModifyUser = i_crm.USERID,
                                        ModifyDate = DateTime.Now
                                    };
                                    if (oImportCustomers.IsFormal == true)
                                    {
                                        oImportCustomers.FormalGuid = saCustomers_Exsit.Count > 0 ? saCustomers_Exsit.First().guid : saImportCustomers_Exsit.First().guid;
                                    }
                                    saImportCustomers.Add(oImportCustomers);
                                }
                            }
                            catch { }
                        }
                        if (saImportCustomers.Count > 0)
                        {
                            db.Insertable(saImportCustomers).ExecuteCommand();
                        }
                    }
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, true);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Exhibition_UpdService), @"展覽管理", @"ImportCustomers（匯入廠商資料）", @"", @"", @"");
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

        #endregion 匯入廠商資料

        #region 匯入廠商資料匯出

        /// <summary>
        /// 匯入廠商資料匯出
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on ImportCustomers</param>
        /// <returns></returns>
        public ResponseMessage ExportCustomers(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var iSN = _fetchInt(i_crm, @"SN");

                    var saImportCustomers = db.Queryable<OTB_CRM_ImportCustomers, OTB_OPM_Exhibition, OTB_WSM_PackingOrder>
                        ((t1, t2, t3) => new object[]
                                              {
                                                JoinType.Inner, t1.OrgID == t2.OrgID && t1.ExhibitionNO == t2.SN,
                                                JoinType.Left, t1.OrgID == t3.OrgID && t1.guid == t3.CustomerId
                                              }
                        )
                        .Where((t1, t2, t3) => t1.ExhibitionNO == iSN && t1.OrgID == i_crm.ORIGID)
                        .Select((t1, t2, t3) => new View_CRM_ImportCustomers
                        {
                            guid = SqlFunc.GetSelfAndAutoFill(t1.guid),
                            Exhibitioname_TW = t2.Exhibitioname_TW,
                            Exhibitioname_EN = t2.Exhibitioname_EN,
                            IsAppoint = SqlFunc.IIF(SqlFunc.HasValue(t1.AppointNO), "已預約", "未預約")
                        })
                        .ToPageList(1, 100000);

                    foreach (var item in saImportCustomers)
                    {
                        item.CustomerShotEName = (bool)item.IsFormal ? "已轉正" : "未轉正";
                    }

                    // var dtImportCustomers = saImportCustomers.ListToDataTable();

                    const string sFileName = "匯入廠商匯出資料";
                    var oHeader = new Dictionary<string, string>
                        {
                            { "RowIndex", "項次" },
                            { "ExhibitionArea", "展區" },
                            { "MuseumMumber", "攤位" },
                            { "CustomerCName", "客戶中文名稱" },
                            { "CustomerEName", "客戶英文名稱" },
                            { "UniCode", "統編號碼" },
                            { "Contactor", "聯絡人" },
                            { "Telephone", "電話" },
                            { "Email", "郵箱" },
                            { "Address", "地址" },
                            { "Memo", "備註" },
                            { "CustomerShotEName", "轉正狀態" },//暫時用英文簡稱代替
                            { "IsAppoint", "預約狀態" }
                        };
                    var dicAlain = ExcelService.GetExportAlain(oHeader, "RowIndex,MuseumMumber,UniCode,Contactor,Telephone,Email,CustomerShotEName,IsAppoint");
                    var listMerge = new List<Dictionary<string, int>>();

                    var bOk = new ExcelService().CreateExcelByList(saImportCustomers, out string sPath, oHeader, dicAlain, sFileName);

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, sPath);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Exhibition_UpdService), @"展覽管理", @"ExportCustomers（匯入廠商資料匯出）", @"", @"", @"");
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

        #endregion 匯入廠商資料匯出

        #region 修改匯入廠商轉正標記

        /// <summary>
        /// 修改匯入廠商轉正標記
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on UpdateCustomerTag</param>
        /// <returns></returns>
        public ResponseMessage UpdateCustomerTag(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sPrevId = _fetchString(i_crm, @"PrevId");
                    var sAfterId = _fetchString(i_crm, @"AfterId");

                    var oCustomers = db.Queryable<OTB_CRM_Customers>()
                        .Single(x => x.guid == sAfterId && x.OrgID == i_crm.ORIGID);

                    var oImportCustomers = db.Queryable<OTB_CRM_ImportCustomers>()
                        .Single(x => x.guid == sPrevId && x.OrgID == i_crm.ORIGID);

                    oImportCustomers.IsFormal = true;
                    oImportCustomers.FormalGuid = oCustomers.guid;
                    //oImportCustomers.CustomerCName = oCustomers.CustomerCName;

                    var iRel = db.Updateable(oImportCustomers)
                        .UpdateColumns(x => new { x.IsFormal, x.FormalGuid, x.CustomerCName })
                        .Where(x => x.OrgID == i_crm.ORIGID && x.CustomerCName == oImportCustomers.CustomerCName)
                        .ExecuteCommand();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iRel);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Exhibition_UpdService), @"展覽管理", @"UpdateCustomerTag（修改匯入廠商轉正標記）", @"", @"", @"");
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

        #endregion 修改匯入廠商轉正標記

        #region 新增匯入廠商

        /// <summary>
        /// 新增匯入廠商
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on InsertImportCustomers</param>
        /// <returns></returns>
        public ResponseMessage InsertImportCustomers(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sCustomerCName = _fetchString(i_crm, @"CustomerCName");
                    var iExhibitionNO = _fetchInt(i_crm, @"ExhibitionNO");

                    if (db.Queryable<OTB_CRM_ImportCustomers>()
                        .Any(x => x.OrgID == i_crm.ORIGID && x.ExhibitionNO == iExhibitionNO && x.CustomerCName == sCustomerCName))
                    {
                        sMsg = "該展覽已存在同名廠商資料";
                        break;
                    }

                    sCustomerCName = sCustomerCName.Trim();
                    var saImportCustomers_Exsit = db.Queryable<OTB_CRM_ImportCustomers>()
                          .Where(x => x.OrgID == i_crm.ORIGID && x.CustomerCName == sCustomerCName)
                          .ToList();
                    var saCustomers_Exsit = db.Queryable<OTB_CRM_Customers>()
                          .Where(x => x.OrgID == i_crm.ORIGID && x.CustomerCName == sCustomerCName)
                          .ToList();

                    var oImportCustomers_Add = _fetchEntity<OTB_CRM_ImportCustomers>(i_crm);
                    oImportCustomers_Add.OrgID = i_crm.ORIGID;
                    oImportCustomers_Add.guid = Guid.NewGuid().ToString();
                    oImportCustomers_Add.ExhibitionNO = iExhibitionNO;
                    oImportCustomers_Add.IsFormal = saCustomers_Exsit.Count > 0 ? true : saImportCustomers_Exsit.Count > 0 ? saImportCustomers_Exsit.First().IsFormal : false;
                    if (oImportCustomers_Add.IsFormal == true)
                    {
                        oImportCustomers_Add.FormalGuid = saCustomers_Exsit.Count > 0 ? saCustomers_Exsit.First().guid : saImportCustomers_Exsit.First().guid;
                    }

                    var iRel = db.Insertable(oImportCustomers_Add).ExecuteCommand();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iRel);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Exhibition_UpdService), @"展覽管理", @"InsertImportCustomers（新增匯入廠商）", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    if (i_crm.LANG == @"zh")
                    {
                        sMsg = ChineseStringUtility.ToSimplified(sMsg);
                    }
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
            }
            return rm;
        }

        #endregion 新增匯入廠商

        #region 修改匯入廠商

        /// <summary>
        /// 修改匯入廠商
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on UpdImportCustomers</param>
        /// <returns></returns>
        public ResponseMessage UpdImportCustomers(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var oEntity = _fetchEntity<OTB_CRM_ImportCustomers>(i_crm);
                    var iRel = db.Updateable(oEntity)
                        .UpdateColumns(x => new
                        {
                            x.UniCode,
                            x.CustomerCName,
                            x.CustomerEName,
                            x.ExhibitionArea,
                            x.Contactor,
                            x.Telephone,
                            x.Email,
                            x.Address,
                            x.Memo,
                            x.MuseumMumber
                        }).ExecuteCommand();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iRel);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Exhibition_UpdService), @"展覽管理", @"UpdImportCustomers（修改匯入廠商）", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    if (i_crm.LANG == @"zh")
                    {
                        sMsg = ChineseStringUtility.ToSimplified(sMsg);
                    }
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
            }
            return rm;
        }

        #endregion 修改匯入廠商

        #region 從名單移除

        /// <summary>
        /// 從名單移除
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter</param>
        /// <returns></returns>
        public ResponseMessage RemoveFromList(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    int iRel = 1;

                    string strSN = _fetchString(i_crm, @"SN");
                    string strGuids = _fetchString(i_crm, @"Guid");
                    string[] arrGuid = strGuids.Split(',');

                    foreach (string strGuid in arrGuid)
                    {
                        if (strGuid != "")
                        {
                            if (iRel != 1)
                            {
                                break;
                            }

                            //刪除聯絡人關聯
                            iRel = db.Deleteable<OTB_OPM_ExhibitionContactors>().Where(x => x.ExhibitionNO == strSN && x.CustomerId == strGuid).ExecuteCommand();

                            //刪除客戶關聯
                            iRel = db.Deleteable<OTB_OPM_ExhibitionCustomers>().Where(x => x.ExhibitionNO == strSN && x.CustomerId == strGuid).ExecuteCommand();
                        }
                    }

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iRel);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Exhibition_UpdService), @"展覽管理", @"RemoveFromList（從名單移除）", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    if (i_crm.LANG == @"zh")
                    {
                        sMsg = ChineseStringUtility.ToSimplified(sMsg);
                    }
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
            }
            return rm;
        }

        #endregion 從名單移除

        #region 新增名單-資料庫匯入

        /// <summary>
        /// 新增名單-資料庫匯入
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter</param>
        /// <returns></returns>
        public ResponseMessage AddListDB(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var iRel = 1;

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iRel);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Exhibition_UpdService), @"展覽管理", @"AddListDB（新增名單-資料庫匯入）", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    if (i_crm.LANG == @"zh")
                    {
                        sMsg = ChineseStringUtility.ToSimplified(sMsg);
                    }
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
            }
            return rm;
        }

        #endregion 新增名單-資料庫匯入

        #region 名單表格下載

        /// <summary>
        /// 名單表格下載
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter</param>
        /// <returns></returns>
        public ResponseMessage DownloadTemplate(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var oTempl = db.Queryable<OTB_SYS_OfficeTemplate>().Single(it => it.OrgID == i_crm.ORIGID && it.TemplID == "ExhibitionCustomerList");
                    if (oTempl == null)
                    {
                        sMsg = @"請檢查模版設定";
                        break;
                    }

                    var oFile = db.Queryable<OTB_SYS_Files>().Single(it => it.OrgID == i_crm.ORIGID && it.ParentID == oTempl.FileID);
                    if (oFile == null)
                    {
                        sMsg = @"系統找不到對應的報表模版";
                        break;
                    } else
                    {
                        oFile.FileName = Path.GetFileNameWithoutExtension(oFile.FileName);
                    }

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, oFile);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Exhibition_UpdService), @"展覽管理", @"DownloadTemplate（名單表格下載）", @"", @"", @"");
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

        #endregion 名單表格下載

        #region 展覽管理（客戶查詢帶入）

        /// <summary>
        /// 客戶管理（單筆查詢）
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage CustomerQuery(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sId = _fetchString(i_crm, @"guid");
                    var sUniCode = _fetchString(i_crm, @"unicode");

                    var oEntity = db.Queryable<OTB_CRM_Customers>()
                            //.WhereIF((i_crm.ORIGID == "SG" || i_crm.ORIGID == "SE"), x => x.OrgID == i_crm.ORIGID && x.Effective == "Y")
                            .WhereIF((i_crm.ORIGID != "TE" && i_crm.ORIGID != "TG"), x => x.OrgID == i_crm.ORIGID && x.Effective == "Y")
                            .WhereIF((i_crm.ORIGID == "TE" || i_crm.ORIGID == "TG"), x => (x.OrgID == "TE" || x.OrgID == "TG") && x.Effective == "Y")
                            .WhereIF(!string.IsNullOrEmpty(sId), x => x.guid == sId)
                            .WhereIF((!string.IsNullOrEmpty(sUniCode) && string.IsNullOrEmpty(sId)), x => x.UniCode == sUniCode)
                            .Select(x => new
                            {
                                x.guid,
                                x.CustomerCName,
                                x.CustomerEName,
                                x.UniCode,
                                x.Address,
                                x.WebsiteAdress
                            })
                            .First();

                        //.Single(x => x.OrgID == i_crm.ORIGID && (x.guid == sId || x.UniCode == sUniCode));

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, oEntity);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Exhibition_UpdService), "", "CustomerQuery（展覽管理（客戶查詢帶入））", "", "", "");
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

        #endregion 展覽管理（客戶查詢帶入）

        #region 新增展覽名單 單筆新增

        /// <summary>
        /// 新增展覽名單 單筆新增
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on InsertExhibitionList</param>
        /// <returns></returns>
        public ResponseMessage InsertExhibitionListSingle(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    int iRel = 1;

                    int intCount = 0;
                    string strExhibitionNO = "";
                    string strCustomerId = "";

                    string sOrgID = i_crm.ORIGID;
                    strExhibitionNO = _fetchString(i_crm, @"ExhibitionNO");
                    strCustomerId = _fetchString(i_crm, @"CustomerId");

                    //沒有guid，新客戶
                    if (strCustomerId == "")
                    {
                        OTB_CRM_Customers oCustomer = _fetchEntity<OTB_CRM_Customers>(i_crm);
                        _setEntityBase(oCustomer, i_crm);

                        //檢查重複客戶
                        intCount = db.Queryable<OTB_CRM_Customers>()
                            .Where(x => x.Effective == "Y" && ((x.CustomerCName != "" && x.CustomerCName == oCustomer.CustomerCName) || (x.CustomerEName != "" && x.CustomerEName == oCustomer.CustomerEName) || (x.UniCode != "" && x.UniCode == oCustomer.UniCode)))
                            //.WhereIF((i_crm.ORIGID == "SG" || i_crm.ORIGID == "SE"), x => x.OrgID == i_crm.ORIGID)
                            .WhereIF((i_crm.ORIGID != "TE" && i_crm.ORIGID != "TG"), x => x.OrgID == i_crm.ORIGID)
                            .WhereIF((i_crm.ORIGID == "TE" || i_crm.ORIGID == "TG"), x => x.OrgID == "TE" || x.OrgID == "TG")
                            .Count();
                        if (intCount > 0)
                        {
                            sMsg = "客戶統編或中文名稱或英文名稱重複，無法新增";
                            iRel = 0;
                        } else
                        {
                            //新增客戶資料
                            strCustomerId = Guid.NewGuid().ToString();
                            oCustomer.guid = strCustomerId;
                            oCustomer.TransactionType = "";
                            oCustomer.CustomerNO = "    ";
                            oCustomer.IsAudit = "N";
                            oCustomer.Effective = "Y";

                            //客戶資料表頭
                            OTB_CRM_CustomersMST oMstEntity = _fetchEntity<OTB_CRM_CustomersMST>(i_crm);
                            _setEntityBase(oMstEntity, i_crm);
                            oMstEntity.guid = Guid.NewGuid().ToString();
                            oMstEntity.CustomerNO = "    ";
                            oMstEntity.customer_guid = strCustomerId;
                            oMstEntity.Effective = "Y";

                            iRel = db.Insertable(oCustomer).ExecuteCommand();
                            if (iRel == 1)
                            {
                                iRel = db.Insertable(oMstEntity).ExecuteCommand();
                            }
                        }
                    } else
                    {
                        //檢查該展覽是否已存在同名廠商資料
                        intCount = 0;
                        intCount = db.Queryable<OTB_OPM_ExhibitionCustomers>().Where(x => x.ExhibitionNO == strExhibitionNO  && x.CustomerId == strCustomerId).Count();
                        if (intCount > 0)
                        {
                            sMsg = "該展覽已存在同名廠商資料，無法新增";
                            iRel = 0;
                        }
                    }

                    if(iRel == 1)
                    {
                        //處理展覽客戶關聯資料表
                        OTB_OPM_ExhibitionCustomers oExCustomerEntity = _fetchEntity<OTB_OPM_ExhibitionCustomers>(i_crm);
                        _setEntityBase(oExCustomerEntity, i_crm);
                        oExCustomerEntity.CustomerId = strCustomerId;
                        iRel = db.Insertable(oExCustomerEntity).ExecuteCommand();

                        if (iRel == 1)
                        {
                            List<OTB_CRM_Contactors> listAdd = new List<OTB_CRM_Contactors>();
                            List<string> listAddContactors = new List<string>();
                            //新增聯絡人資料
                            JArray jaAddContactors = (JArray)JsonConvert.DeserializeObject(_fetchString(i_crm, @"AddContactors"));

                            if (jaAddContactors.Count > 0)
                            {
                                foreach (JObject joContactors in jaAddContactors)
                                {
                                    //intMax++;

                                    OTB_CRM_Contactors Contactors = new OTB_CRM_Contactors();
                                    _setEntityBase(Contactors, i_crm);

                                    Contactors.guid = Guid.NewGuid().ToString();
                                    Contactors.CustomerId = strCustomerId;
                                    Contactors.ContactorName = joContactors["ContactorName"].ToString();
                                    Contactors.NickName = joContactors["NickName"].ToString();
                                    Contactors.Call = joContactors["Call"].ToString();
                                    Contactors.Birthday = joContactors["Birthday"].ToString();
                                    Contactors.MaritalStatus = joContactors["MaritalStatus"].ToString();
                                    Contactors.PersonalMobilePhone = joContactors["PersonalMobilePhone"].ToString();
                                    Contactors.PersonalEmail = joContactors["PersonalEmail"].ToString();
                                    Contactors.LINE = joContactors["LINE"].ToString();
                                    Contactors.WECHAT = joContactors["WECHAT"].ToString();
                                    Contactors.Personality = joContactors["Personality"].ToString();
                                    Contactors.Preferences = joContactors["Preferences"].ToString();
                                    Contactors.PersonalAddress = joContactors["PersonalAddress"].ToString();
                                    Contactors.Memo = joContactors["Memo"].ToString();
                                    Contactors.ImmediateSupervisor = joContactors["ImmediateSupervisor"].ToString();
                                    Contactors.JobTitle = joContactors["JobTitle"].ToString();
                                    Contactors.Department = joContactors["Department"].ToString();
                                    Contactors.Email1 = joContactors["Email1"].ToString();
                                    Contactors.Email2 = joContactors["Email2"].ToString();
                                    Contactors.Telephone1 = joContactors["Telephone1"].ToString();
                                    Contactors.Telephone2 = joContactors["Telephone2"].ToString();
                                    Contactors.Ext1 = joContactors["Ext1"].ToString();
                                    Contactors.Ext2 = joContactors["Ext2"].ToString();
                                    Contactors.ChoseReason = joContactors["ChoseReason"].ToString();
                                    //Contactors.OrderByValue = intMax.ToString();

                                    int iReljaAddContactors = db.Insertable(Contactors).ExecuteCommand();

                                    listAddContactors.Add(Contactors.guid);
                                    listAdd.Add(Contactors);
                                }
                            }

                            //新增展覽客戶聯絡人關聯資料表
                            JArray jaContactors = (JArray)JsonConvert.DeserializeObject(_fetchString(i_crm, @"Contactors"));
                            if (jaContactors.Count > 0)
                            {
                                foreach (JObject joContactors in jaContactors)
                                {
                                    OTB_OPM_ExhibitionContactors ExhibitionContactors = new OTB_OPM_ExhibitionContactors();

                                    _setEntityBase(ExhibitionContactors, i_crm);

                                    ExhibitionContactors.ExhibitionNO = strExhibitionNO;
                                    ExhibitionContactors.CustomerId = strCustomerId;

                                    ExhibitionContactors.SourceType = "1";
                                    ExhibitionContactors.IsFormal = "Y";

                                    if (joContactors["guid"] == null)
                                    {
                                        ExhibitionContactors.ContactorId = listAdd.Where(t => t.ContactorName == joContactors["ContactorName"].ToString()).ToList()[0].guid;
                                    }
                                    else
                                    {
                                        ExhibitionContactors.ContactorId = joContactors["guid"].ToString();
                                    }

                                    int iRelExhibitionContactors = db.Insertable(ExhibitionContactors).ExecuteCommand();
                                }
                            }
                        }
                    }

                    //iRel = 1;

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iRel);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Exhibition_UpdService), @"展覽管理", @"InsertExhibitionListSingle（新增展覽名單 單筆新增）", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    if (i_crm.LANG == @"zh")
                    {
                        sMsg = ChineseStringUtility.ToSimplified(sMsg);
                    }
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
            }
            return rm;
        }

        #endregion 新增展覽名單 單筆新增

        #region 獲取展覽名單

        /// <summary>
        /// 獲取展覽名單
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on GetCustomers</param>
        /// <returns></returns>
        public ResponseMessage GetExhibitionList(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    string sSN = _fetchString(i_crm, @"SN");
                    string sSortField = _fetchString(i_crm, @"sortField");
                    string sSortOrder = _fetchString(i_crm, @"sortOrder");
                    var bExcel = _fetchBool(i_crm, @"Excel");

                    if (string.IsNullOrEmpty(sSortField))
                    {
                        sSortField = "CASE TransportRequire WHEN '未接' THEN 1 WHEN '尚未決定' THEN 2 WHEN '有需求' THEN 3 WHEN '線上預約' THEN 4 WHEN '無需求' THEN 5 WHEN '無需求(手提)' THEN 6 WHEN '無需求(其他代理運輸)' THEN 7 WHEN '無需求(當地出貨)' THEN 8 WHEN '無需求(其他)' THEN 9 ELSE 10 END";
                    } else
                    {
                        if (sSortField == "TransportRequire")
                        {
                            sSortField = "CASE TransportRequire WHEN '未接' THEN 1 WHEN '尚未決定' THEN 2 WHEN '有需求' THEN 3 WHEN '線上預約' THEN 4 WHEN '無需求' THEN 5 WHEN '無需求(手提)' THEN 6 WHEN '無需求(其他代理運輸)' THEN 7 WHEN '無需求(當地出貨)' THEN 8 WHEN '無需求(其他)' THEN 9 ELSE 10 END";
                        }
                    }

                    if (string.IsNullOrEmpty(sSortOrder))
                    {
                        sSortOrder = "ASC";
                    }

                    var listCustomer = db.Queryable<OTB_OPM_ExhibitionCustomers, OTB_CRM_Customers, OTB_CRM_Customers>((t1, t2, t3) => new object[] {
                            JoinType.Inner,t1.CustomerId==t2.guid,
                            JoinType.Left,t1.ListSource==t3.guid,
                            })
                            .Select((t1, t2, t3) => new {
                                t1.SN, t2.guid, t1.ExhibitionNO, t1.CustomerId, t2.CustomerCName, t2.CustomerEName, t1.VolumeForecasting, t1.NumberOfBooths, ContactorName = "", Telephone1 = "",
                                t1.ListSource, ListSourceName = t3.CustomerShotCName, IsFormal = t2.IsAudit, t2.Telephone, t2.FAX, t2.Address, t1.ModifyDate, t1.Memo, t2.IsImporter,
                                TransportRequire = SqlFunc.MappingColumn(t1.ExhibitionNO, "dbo.[OFN_SYS_ArgumentValueByArgumentID]('TE', t1.TransportRequire,'TransportRequire')"),
                                TransportationMode = SqlFunc.MappingColumn(t1.ExhibitionNO, "dbo.[OFN_SYS_ArgumentValueByArgumentID]('TE', t1.TransportationMode,'Transport')"),
                            }).MergeTable().Where(t1 => t1.ExhibitionNO == sSN).OrderBy(sSortField, sSortOrder).ToList();

                    var listContactorFormal = db.Queryable<OTB_OPM_ExhibitionContactors, OTB_CRM_Contactors>((t1, t2) => new object[] {
                            JoinType.Inner,t1.ContactorId==t2.guid})
                            .Select((t1, t2) => new { t1.ExhibitionNO, t1.CustomerId, t2.ContactorName, t2.Telephone1, t2.Ext1, t1.IsMain, t2.OrderByValue }).MergeTable()
                            .Where(t1 => t1.ExhibitionNO == sSN).OrderBy("OrderByValue").ToList();

                    var listContactorInFormal = db.Queryable<OTB_OPM_ExhibitionContactors, OTB_CRM_ContactorsTemp>((t1, t2) => new object[] {
                            JoinType.Inner,t1.ContactorId==t2.guid})
                            .Select((t1, t2) => new { t1.ExhibitionNO, t1.CustomerId, t2.ContactorName, t2.Telephone1, t2.Ext1, t1.IsMain, t2.OrderByValue }).MergeTable()
                            .Where(t1 => t1.ExhibitionNO == sSN).OrderBy("OrderByValue").ToList();

                    List<OTB_OPM_ExportExhibition> listExportExhibition = new List<OTB_OPM_ExportExhibition>();
                    List<OTB_OPM_OtherExhibitionTG> listOtherExhibitionTG = new List<OTB_OPM_OtherExhibitionTG>();
                    List<OTB_OPM_ImportExhibition> listImportExhibition = new List<OTB_OPM_ImportExhibition>();
                    List<OTB_OPM_OtherExhibition> listOtherExhibition = new List<OTB_OPM_OtherExhibition>();

                    listExportExhibition = db.Queryable<OTB_OPM_ExportExhibition>().Where(x => x.ExhibitionNO == sSN && x.IsVoid == "N").ToList();
                    listOtherExhibitionTG = db.Queryable<OTB_OPM_OtherExhibitionTG>().Where(x => x.ExhibitionNO == sSN && x.IsVoid == "N").ToList();
                    listImportExhibition = db.Queryable<OTB_OPM_ImportExhibition>().Where(x => x.ExhibitionNO == sSN && x.IsVoid == "N").ToList();
                    listOtherExhibition = db.Queryable<OTB_OPM_OtherExhibition>().Where(x => x.ExhibitionNO == sSN && x.IsVoid == "N").ToList();

                    int i = 1;
                    List<View_OPM_ExhibitionCustomers> listEntity = new List<View_OPM_ExhibitionCustomers>();
                    foreach (var oCustomer in listCustomer)
                    {
                        View_OPM_ExhibitionCustomers oEntity = new View_OPM_ExhibitionCustomers();

                        oEntity.RowIndex = i;
                        oEntity.guid = oCustomer.guid;
                        oEntity.CustomerCName = oCustomer.CustomerCName;
                        oEntity.CustomerEName = oCustomer.CustomerEName;
                        oEntity.ListSource = oCustomer.ListSource;
                        oEntity.ListSourceName = oCustomer.ListSourceName;
                        oEntity.IsFormal = oCustomer.IsFormal;
                        oEntity.Telephone = ""; // oCustomer.Telephone;
                        oEntity.Ext = "";
                        oEntity.FAX = oCustomer.FAX;
                        oEntity.Address = oCustomer.Address;

                        oEntity.VolumeForecasting = oCustomer.VolumeForecasting;
                        oEntity.NumberOfBooths = oCustomer.NumberOfBooths;
                        oEntity.TransportRequire = oCustomer.TransportRequire;
                        oEntity.TransportationMode = oCustomer.TransportationMode;
                        oEntity.Memo = oCustomer.Memo;
                        oEntity.IsImporter = oCustomer.IsImporter;

                        oEntity.ModifyDate = oCustomer.ModifyDate;

                        var listCustomerContactor = listContactorFormal.Where(t => t.CustomerId == oCustomer.CustomerId).ToList();

                        if (listCustomerContactor.Count > 0)
                        {
                            var listMain = listCustomerContactor.Where(t => t.IsMain == "Y").ToList();
                            if (listMain.Count == 0)
                            {
                                listMain = listCustomerContactor;
                            }
                            oEntity.ContactorName = listMain[0].ContactorName;
                            oEntity.Telephone = listMain[0].Telephone1;
                            oEntity.Ext = listMain[0].Ext1;
                        } else
                        {
                            listCustomerContactor = listContactorInFormal.Where(t => t.CustomerId == oCustomer.CustomerId).ToList();

                            if (listCustomerContactor.Count > 0)
                            {
                                var listMain = listCustomerContactor.Where(t => t.IsMain == "Y").ToList();
                                if (listMain.Count == 0)
                                {
                                    listMain = listCustomerContactor;
                                }
                                oEntity.ContactorName = listMain[0].ContactorName;
                                oEntity.Telephone = listMain[0].Telephone1;
                                oEntity.Ext = listMain[0].Ext1;
                            }
                        }

                        oEntity.IsDeal = "N";

                        var listMatchCustomer = db.Queryable<OTB_CRM_Customers, OTB_CRM_CustomersMST>((t1, t2) => new object[] { JoinType.Inner, t1.CustomerNO == t2.CustomerNO })
                            .Where((t1, t2) => t2.customer_guid == oCustomer.guid && t2.Memo == "CustomerCombine")
                            .Select((t1, t2) => new { t1.guid, t2.customer_guid, t2.Memo }).MergeTable()
                            .ToList();

                        if (listMatchCustomer.Count == 0)
                        {
                            if(listImportExhibition.Where(x => x.Supplier == oCustomer.guid).Count() > 0)
                            {
                                oEntity.IsDeal = "Y";
                            }

                            if (oEntity.IsDeal == "N")
                            {
                                if (listOtherExhibition.Where(x => x.Supplier == oCustomer.guid).Count() > 0)
                                {
                                    oEntity.IsDeal = "Y";
                                }

                                if (oEntity.IsDeal == "N")
                                {
                                    foreach (OTB_OPM_ExportExhibition oExportExhibition in listExportExhibition)
                                    {
                                        var jaExhibitors = (JArray)JsonConvert.DeserializeObject(oExportExhibition.Exhibitors);

                                        foreach (JObject joExhibitors in jaExhibitors)
                                        {
                                            if (joExhibitors["SupplierID"].ToString() == oCustomer.guid && joExhibitors["VoidContent"] == null)
                                            {
                                                oEntity.IsDeal = "Y";
                                                break;
                                            }
                                        }
                                        if (oEntity.IsDeal == "Y")
                                        {
                                            break;
                                        }
                                    }

                                    if (oEntity.IsDeal == "N")
                                    {
                                        foreach (OTB_OPM_OtherExhibitionTG oOtherExhibitionTG in listOtherExhibitionTG)
                                        {

                                            var jaExhibitors = (JArray)JsonConvert.DeserializeObject(oOtherExhibitionTG.Exhibitors);

                                            foreach (JObject joExhibitors in jaExhibitors)
                                            {
                                                if (joExhibitors["SupplierID"].ToString() == oCustomer.guid && joExhibitors["VoidContent"] == null)
                                                {
                                                    oEntity.IsDeal = "Y";
                                                    break;
                                                }
                                            }
                                            if (oEntity.IsDeal == "Y")
                                            {
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        } else
                        {
                            string strMatchId = listMatchCustomer[0].guid;

                            if (listImportExhibition.Where(x => x.Supplier == oCustomer.guid || x.Supplier == strMatchId).Count() > 0)
                            {
                                oEntity.IsDeal = "Y";
                            }

                            if (oEntity.IsDeal == "N")
                            {
                                if (listOtherExhibition.Where(x => x.Supplier == oCustomer.guid || x.Supplier == strMatchId).Count() > 0)
                                {
                                    oEntity.IsDeal = "Y";
                                }

                                if (oEntity.IsDeal == "N")
                                {
                                    foreach (OTB_OPM_ExportExhibition oExportExhibition in listExportExhibition)
                                    {
                                        var jaExhibitors = (JArray)JsonConvert.DeserializeObject(oExportExhibition.Exhibitors);

                                        foreach (JObject joExhibitors in jaExhibitors)
                                        {
                                            if ((joExhibitors["SupplierID"].ToString() == oCustomer.guid && joExhibitors["VoidContent"] == null) ||
                                                (joExhibitors["SupplierID"].ToString() == strMatchId && joExhibitors["VoidContent"] == null)
                                                )
                                            {
                                                oEntity.IsDeal = "Y";
                                                break;
                                            }
                                        }
                                        if (oEntity.IsDeal == "Y")
                                        {
                                            break;
                                        }
                                    }

                                    if (oEntity.IsDeal == "N")
                                    {
                                        foreach (OTB_OPM_OtherExhibitionTG oOtherExhibitionTG in listOtherExhibitionTG)
                                        {

                                            var jaExhibitors = (JArray)JsonConvert.DeserializeObject(oOtherExhibitionTG.Exhibitors);

                                            foreach (JObject joExhibitors in jaExhibitors)
                                            {
                                                if ((joExhibitors["SupplierID"].ToString() == oCustomer.guid && joExhibitors["VoidContent"] == null) ||
                                                    (joExhibitors["SupplierID"].ToString() == strMatchId && joExhibitors["VoidContent"] == null)
                                                    )
                                                {
                                                    oEntity.IsDeal = "Y";
                                                    break;
                                                }
                                            }
                                            if (oEntity.IsDeal == "Y")
                                            {
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        listEntity.Add(oEntity);
                        i++;
                    }

                    rm = new SuccessResponseMessage(null, i_crm);
                    //rm.DATA.Add(BLWording.REL, listEntity);
                    if (bExcel)
                    {
                        List<OTB_CRM_Callout> listCalloutAll = db.Queryable<OTB_CRM_Callout>().Where(x => x.ExhibitionNO == sSN).ToList();

                        var sFileName = "";
                        var oHeader = new Dictionary<string, string>();
                        var listMerge = new List<Dictionary<string, int>>();
                        var dicAlain = new Dictionary<string, string>();
                        var dt_new = new DataTable();
                        string strCallout = "";
                        sFileName = "展覽名單";
                        oHeader = new Dictionary<string, string>
                                                {
                                                    { "TransportRequire", "運輸需求" },
                                                    { "TransportationMode", "運輸方式" },
                                                    { "VolumeForecasting", "貨量" },
                                                    { "NumberOfBooths", "欄位數" },
                                                    { "CustomerCName", "客戶中文名稱" },
                                                    { "CustomerEName", "客戶英文名稱" },
                                                    { "ContactorName", "聯絡人" },
                                                    { "ListSourceName", "名單來源" },
                                                    { "ModifyDate", "最新修改時間" },
                                                    { "IsDeal", "是否成交" },
                                                    { "IsFormal", "是否為正式客戶" },
                                                    { "IsImporter", "是否為進口商" },
                                                    { "Memo", "備註" },
                                                    { "Callout", "最新Callout記錄" }
                                                };
                        dt_new.Columns.Add("TransportRequire");
                        dt_new.Columns.Add("TransportationMode");
                        dt_new.Columns.Add("VolumeForecasting");
                        dt_new.Columns.Add("NumberOfBooths");
                        dt_new.Columns.Add("CustomerCName");
                        dt_new.Columns.Add("CustomerEName");
                        dt_new.Columns.Add("ContactorName");
                        //dt_new.Columns.Add("Telephone");
                        //dt_new.Columns.Add("Ext");
                        dt_new.Columns.Add("ListSourceName");
                        dt_new.Columns.Add("ModifyDate");
                        dt_new.Columns.Add("IsDeal");
                        dt_new.Columns.Add("IsFormal");
                        dt_new.Columns.Add("IsImporter");
                        dt_new.Columns.Add("Memo");
                        dt_new.Columns.Add("Callout");
                        foreach (var item in listEntity)
                        {
                            strCallout = "";
                            List<OTB_CRM_Callout> listCallout = listCalloutAll.Where(x => x.CustomerId == item.guid).OrderByDescending(x => x.CreateDate).ToList();
                            if (listCallout.Count > 0)
                            {
                                strCallout = listCallout[0].Memo;
                            }

                            var row_new = dt_new.NewRow();
                            row_new["TransportRequire"] = item.TransportRequire;
                            row_new["TransportationMode"] = item.TransportationMode;
                            row_new["VolumeForecasting"] = item.VolumeForecasting;
                            row_new["NumberOfBooths"] = item.NumberOfBooths;
                            row_new["CustomerCName"] = item.CustomerCName;
                            row_new["CustomerEName"] = item.CustomerEName;
                            row_new["ContactorName"] = item.ContactorName;
                            //row_new["Telephone"] = item.Telephone;
                            //row_new["Ext"] = item.Ext;
                            if (item.ListSource == "SelfCome")
                            {
                                row_new["ListSourceName"] = "自來";
                            } else if (item.ListSource == "ImportFromDB")
                            {
                                row_new["ListSourceName"] = "資料庫匯入";
                            } else
                            {
                                row_new["ListSourceName"] = item.ListSourceName;
                            }
                            row_new["ModifyDate"] = item.ModifyDate;
                            if (item.IsDeal == "N")
                            {
                                row_new["IsDeal"] = "未成交";
                            }
                            else
                            {
                                row_new["IsDeal"] = "已成交";
                            }
                            if (item.IsFormal == "N")
                            {
                                row_new["IsFormal"] = "非正式客戶";
                            }
                            else {
                                row_new["IsFormal"] = "正式客戶";
                            }
                            if (item.IsImporter == "N")
                            {
                                row_new["IsImporter"] = "否";
                            }
                            else
                            {
                                row_new["IsImporter"] = "是";
                            }
                            row_new["Memo"] = item.Memo;
                            row_new["Callout"] = strCallout;
                            dt_new.Rows.Add(row_new);
                        }
                        dicAlain = ExcelService.GetExportAlain(oHeader, "TransportRequire,TransportationMode,VolumeForecasting,NumberOfBooths,ModifyDate,IsDeal,IsFormal,IsImporter");
                        var bOk = new ExcelService().CreateExcelByTb(dt_new, out string sPath, oHeader, dicAlain, listMerge, sFileName);

                        rm.DATA.Add(BLWording.REL, sPath);
                    }
                    else
                    {
                        rm.DATA.Add(BLWording.REL, listEntity);
                    }
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Exhibition_UpdService), @"展覽管理", @"GetExhibitionList（獲取展覽名單）", @"", @"", @"");
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

        #endregion 獲取展覽名單

        #region 新增更新名單 資料庫匯入

        /// <summary>
        /// 新增更新名單 資料庫匯入
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on GetCustomers</param>
        /// <returns></returns>
        public ResponseMessage AddUpdateList(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    int iRel = 1;

                    string sSN = _fetchString(i_crm, @"SN");
                    string sType = _fetchString(i_crm, @"Type");
                    string sChooseSN = _fetchString(i_crm, @"ChooseSN");
                    string sUSERID = i_crm.USERID;
                    DateTime dtNow = DateTime.Now;

                    int intCount = 0;

                    var listCustomers = db.Queryable<OTB_OPM_ExhibitionCustomers, OTB_CRM_Customers>((t1, t2) => new object[] {
                          JoinType.Inner,t1.CustomerId==t2.guid})
                          .Where(t1 => t1.ExhibitionNO == sSN)
                          .Select((t1, t2) => new View_OPM_ExhibitionCustomers {
                              SN = t1.SN,
                              ExhibitionNO = t1.ExhibitionNO,
                              CustomerId = t1.CustomerId,
                              CustomerCName = t2.CustomerCName,
                              UniCode = t2.UniCode,
                              BoothNumber = t1.BoothNumber,
                              NumberOfBooths = t1.NumberOfBooths,
                              ListSource = t1.ListSource,
                              IsAudit = t2.IsAudit
                          }).MergeTable().ToList();

                    var listChooseCustomers = db.Queryable<OTB_OPM_ExhibitionCustomers, OTB_CRM_Customers>((t1, t2) => new object[] {
                          JoinType.Inner,t1.CustomerId==t2.guid})
                          .Where(t1 => t1.ExhibitionNO == sChooseSN)
                          .Select((t1, t2) => new View_OPM_ExhibitionCustomers
                          {
                              SN = t1.SN,
                              ExhibitionNO = t1.ExhibitionNO,
                              CustomerId = t1.CustomerId,
                              CustomerCName = t2.CustomerCName,
                              BoothNumber = t1.BoothNumber,
                              NumberOfBooths = t1.NumberOfBooths,
                              ListSource = t1.ListSource,
                              IsAudit = t2.IsAudit
                          }).MergeTable().ToList();

                    var listContactorsFormal = db.Queryable<OTB_OPM_ExhibitionContactors, OTB_CRM_Contactors>((t1, t2) => new object[] {
                          JoinType.Inner,t1.ContactorId==t2.guid})
                          .Where(t1 => t1.ExhibitionNO == sSN)
                          .Select((t1, t2) => new View_OPM_ExhibitionContactors {
                              SN = t1.SN,
                              ExhibitionNO = t1.ExhibitionNO,
                              CustomerId = t1.CustomerId,
                              ContactorId = t1.ContactorId,
                              ContactorName = t2.ContactorName,
                              SourceType = t1.SourceType
                          }).MergeTable().ToList();

                    var listContactorsInFormal = db.Queryable<OTB_OPM_ExhibitionContactors, OTB_CRM_ContactorsTemp>((t1, t2) => new object[] {
                          JoinType.Inner,t1.ContactorId==t2.guid})
                          .Where(t1 => t1.ExhibitionNO == sSN)
                          .Select((t1, t2) => new View_OPM_ExhibitionContactors
                          {
                              SN = t1.SN,
                              ExhibitionNO = t1.ExhibitionNO,
                              CustomerId = t1.CustomerId,
                              ContactorId = t1.ContactorId,
                              ContactorName = t2.ContactorName,
                              SourceType = t1.SourceType
                          }).MergeTable().ToList();

                    var listChooseContactorsFormal = db.Queryable<OTB_OPM_ExhibitionContactors, OTB_CRM_Contactors>((t1, t2) => new object[] {
                          JoinType.Inner,t1.ContactorId==t2.guid})
                          .Where(t1 => t1.ExhibitionNO == sChooseSN)
                          .Select((t1, t2) => new View_OPM_ExhibitionContactors
                          {
                              SN = t1.SN,
                              ExhibitionNO = t1.ExhibitionNO,
                              CustomerId = t1.CustomerId,
                              ContactorId = t1.ContactorId,
                              ContactorName = t2.ContactorName,
                              SourceType = t1.SourceType
                          }).MergeTable().ToList();

                    var listChooseContactorsInFormal = db.Queryable<OTB_OPM_ExhibitionContactors, OTB_CRM_ContactorsTemp>((t1, t2) => new object[] {
                          JoinType.Inner,t1.ContactorId==t2.guid})
                          .Where(t1 => t1.ExhibitionNO == sChooseSN)
                          .Select((t1, t2) => new View_OPM_ExhibitionContactors
                          {
                              SN = t1.SN,
                              ExhibitionNO = t1.ExhibitionNO,
                              CustomerId = t1.CustomerId,
                              ContactorId = t1.ContactorId,
                              ContactorName = t2.ContactorName,
                              SourceType = t1.SourceType
                          }).MergeTable().ToList();

                    List<View_OPM_ExhibitionCustomers> listSameCustomer = new List<View_OPM_ExhibitionCustomers>();

                    switch (sType)
                    {
                        case "insert":
                            foreach (View_OPM_ExhibitionCustomers oChooseCustomer in listChooseCustomers)
                            {
                                //確認名單中是否有重複客戶，沒有時進行新增
                                listSameCustomer = listCustomers.Where(t => t.CustomerId == oChooseCustomer.CustomerId).ToList();

                                if (listSameCustomer.Count == 0)
                                {
                                    OTB_OPM_ExhibitionCustomers oInsertCustomer = new OTB_OPM_ExhibitionCustomers();
                                    oInsertCustomer.ExhibitionNO = sSN;
                                    oInsertCustomer.CustomerId = oChooseCustomer.CustomerId;
                                    oInsertCustomer.SourceType = "3";
                                    oInsertCustomer.ListSource = "ImportFromDB"; //oChooseCustomer.ListSource;
                                    oInsertCustomer.BoothNumber = oChooseCustomer.BoothNumber;
                                    oInsertCustomer.NumberOfBooths = oChooseCustomer.NumberOfBooths;
                                    oInsertCustomer.CreateUser = sUSERID;
                                    oInsertCustomer.ModifyUser = sUSERID;
                                    oInsertCustomer.CreateDate = dtNow;
                                    oInsertCustomer.ModifyDate = dtNow;

                                   //新增客戶關聯
                                    iRel = db.Insertable(oInsertCustomer).ExecuteCommand();
                                    
                                    //新增正式聯絡人關聯
                                    List<View_OPM_ExhibitionContactors> listFiltChooseContactors = listChooseContactorsFormal.Where(t => t.CustomerId == oChooseCustomer.CustomerId).ToList();
                                    foreach (View_OPM_ExhibitionContactors oFiltChooseContactors in listFiltChooseContactors)
                                    {
                                        OTB_OPM_ExhibitionContactors oInsertContactor = new OTB_OPM_ExhibitionContactors();
                                        oInsertContactor.ExhibitionNO = sSN;
                                        oInsertContactor.CustomerId = oChooseCustomer.CustomerId;
                                        oInsertContactor.ContactorId = oFiltChooseContactors.ContactorId;
                                        oInsertContactor.SourceType = "3";
                                        oInsertContactor.CreateUser = sUSERID;
                                        oInsertContactor.ModifyUser = sUSERID;
                                        oInsertContactor.CreateDate = dtNow;
                                        oInsertContactor.ModifyDate = dtNow;
                                        oInsertContactor.IsFormal = "Y";
                                        iRel = db.Insertable(oInsertContactor).ExecuteCommand();
                                    }

                                    //新增非正式聯絡人關聯
                                    listFiltChooseContactors = new List<View_OPM_ExhibitionContactors>();
                                    listFiltChooseContactors = listChooseContactorsInFormal.Where(t => t.CustomerId == oChooseCustomer.CustomerId).ToList();
                                    foreach (View_OPM_ExhibitionContactors oFiltChooseContactors in listFiltChooseContactors)
                                    {
                                        OTB_OPM_ExhibitionContactors oInsertContactor = new OTB_OPM_ExhibitionContactors();
                                        oInsertContactor.ExhibitionNO = sSN;
                                        oInsertContactor.CustomerId = oChooseCustomer.CustomerId;
                                        oInsertContactor.ContactorId = oFiltChooseContactors.ContactorId;
                                        oInsertContactor.SourceType = "3";
                                        oInsertContactor.CreateUser = sUSERID;
                                        oInsertContactor.ModifyUser = sUSERID;
                                        oInsertContactor.CreateDate = dtNow;
                                        oInsertContactor.ModifyDate = dtNow;
                                        oInsertContactor.IsFormal = "N";
                                        iRel = db.Insertable(oInsertContactor).ExecuteCommand();
                                    }
                                }
                                else //名單中有重複客戶
                                {
                                    //判斷有無重複聯絡人並新增
                                    foreach (View_OPM_ExhibitionCustomers oSameCustoemr in listSameCustomer)
                                    {
                                        //抓取兩邊名單中的正式聯絡人進行比對
                                        List<View_OPM_ExhibitionContactors> listFiltContactors = listContactorsFormal.Where(t => t.CustomerId == oSameCustoemr.CustomerId).ToList();
                                        List<View_OPM_ExhibitionContactors> listFiltChooseContactors = listChooseContactorsFormal.Where(t => t.CustomerId == oChooseCustomer.CustomerId).ToList();

                                        foreach (View_OPM_ExhibitionContactors oFiltChooseContactors in listFiltChooseContactors)
                                        {
                                            intCount = 0;
                                            intCount = listFiltContactors.Where(t => t.ContactorId == oFiltChooseContactors.ContactorId).Count();

                                            if (intCount == 0)
                                            {
                                                OTB_OPM_ExhibitionContactors oInsertContactor = new OTB_OPM_ExhibitionContactors();
                                                oInsertContactor.ExhibitionNO = sSN;
                                                oInsertContactor.CustomerId = oSameCustoemr.CustomerId;
                                                oInsertContactor.ContactorId = oFiltChooseContactors.ContactorId;
                                                oInsertContactor.SourceType = "3";
                                                oInsertContactor.CreateUser = sUSERID;
                                                oInsertContactor.ModifyUser = sUSERID;
                                                oInsertContactor.CreateDate = dtNow;
                                                oInsertContactor.ModifyDate = dtNow;
                                                oInsertContactor.IsFormal = "Y";
                                                iRel = db.Insertable(oInsertContactor).ExecuteCommand();
                                            }
                                        }

                                        //非正式聯絡人
                                        listFiltContactors = listContactorsInFormal.Where(t => t.CustomerId == oSameCustoemr.CustomerId).ToList();
                                        listFiltChooseContactors = listChooseContactorsInFormal.Where(t => t.CustomerId == oChooseCustomer.CustomerId).ToList();

                                        foreach (View_OPM_ExhibitionContactors oFiltChooseContactors in listFiltChooseContactors)
                                        {
                                            intCount = 0;
                                            intCount = listFiltContactors.Where(t => t.ContactorName == oFiltChooseContactors.ContactorName).Count();

                                            if (intCount == 0)
                                            {
                                                string strContactorId = "";
                                                List<OTB_CRM_ContactorsTemp> listContactorTemp = db.Queryable<OTB_CRM_ContactorsTemp>().Where(x => x.CustomerId == oFiltChooseContactors.CustomerId && x.ContactorName == oFiltChooseContactors.ContactorName).ToList();
                                                if (listContactorTemp.Count == 0)
                                                {
                                                    strContactorId = Guid.NewGuid().ToString();
                                                    SugarParameter[] parameterValue = new SugarParameter[]
                                                    {
                                                            new SugarParameter("@Type", "2"),
                                                            new SugarParameter("@Guid", strContactorId),
                                                            new SugarParameter("@CustomerId", oSameCustoemr.CustomerId),
                                                            new SugarParameter("@ContactorId", oFiltChooseContactors.ContactorId),
                                                            new SugarParameter("@CreateUser", sUSERID),
                                                            new SugarParameter("@CreateDate", dtNow)
                                                    };
                                                    iRel = db.Ado.UseStoredProcedure().ExecuteCommand("OSP_OTB_CRM_Contactors_DBImportInsert", parameterValue);
                                                } else
                                                {
                                                    strContactorId = listContactorTemp[0].guid;
                                                }

                                                OTB_OPM_ExhibitionContactors oInsertContactor = new OTB_OPM_ExhibitionContactors();
                                                oInsertContactor.ExhibitionNO = sSN;
                                                oInsertContactor.CustomerId = oSameCustoemr.CustomerId;
                                                oInsertContactor.ContactorId = strContactorId;
                                                oInsertContactor.SourceType = "3";
                                                oInsertContactor.CreateUser = sUSERID;
                                                oInsertContactor.ModifyUser = sUSERID;
                                                oInsertContactor.CreateDate = dtNow;
                                                oInsertContactor.ModifyDate = dtNow;
                                                oInsertContactor.IsFormal = "N";
                                                iRel = db.Insertable(oInsertContactor).ExecuteCommand();
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case "update":
                            foreach (View_OPM_ExhibitionCustomers oChooseCustomer in listChooseCustomers)
                            {
                                //確認是否有重複客戶，如果有，更新聯絡人資料
                                //listSameCustomer = listCustomers.Where(t => (t.CustomerCName != "" && t.CustomerCName == oChooseCustomer.CustomerCName) || (t.CustomerEName != "" && t.CustomerEName == oChooseCustomer.CustomerEName) || (t.UniCode != "" && t.UniCode == oChooseCustomer.UniCode)).ToList();
                                //listSameCustomer = listCustomers.Where(t => t.CustomerId == oChooseCustomer.CustomerId).ToList();

                                //foreach (View_OPM_ExhibitionCustomers oSameCustoemr in listSameCustomer)
                                //{
                                //    if (oSameCustoemr.IsAudit != "Y")
                                //    {
                                //        SugarParameter[] parameterValue1 = new SugarParameter[]
                                //        {
                                //                new SugarParameter("@Guid1", oSameCustoemr.CustomerId),
                                //                new SugarParameter("@Guid2", oChooseCustomer.CustomerId),
                                //                new SugarParameter("@ModifyUser", sUSERID),
                                //                new SugarParameter("@ModifyDate", dtNow)
                                //        };

                                //        iRel = db.Ado.UseStoredProcedure().ExecuteCommand("OSP_OTB_CRM_Customers_DBImportUpdate", parameterValue1);
                                //    }

                                    //List<View_OPM_ExhibitionContactors> listFiltContactors = listContactorsFormal.Where(t => t.CustomerId == oSameCustoemr.CustomerId).ToList();
                                    //List<View_OPM_ExhibitionContactors> listFiltChooseContactors = listChooseContactorsFormal.Where(t => t.CustomerId == oChooseCustomer.CustomerId).ToList();

                                    //foreach (View_OPM_ExhibitionContactors oFiltChooseContactors in listFiltChooseContactors)
                                    //{
                                    //    List<View_OPM_ExhibitionContactors> listSameContactor = listFiltContactors.Where(t => t.ContactorName == oFiltChooseContactors.ContactorName).ToList();

                                    //    foreach (View_OPM_ExhibitionContactors oSameContactor in listSameContactor)
                                    //    {
                                    //        SugarParameter[] parameterValue2 = new SugarParameter[]
                                    //        {
                                    //            new SugarParameter("@Type", "1"),
                                    //            new SugarParameter("@Guid1", oSameContactor.ContactorId),
                                    //            new SugarParameter("@Guid2", oFiltChooseContactors.ContactorId),
                                    //            new SugarParameter("@ModifyUser", sUSERID),
                                    //            new SugarParameter("@ModifyDate", dtNow)
                                    //        };

                                    //        iRel = db.Ado.UseStoredProcedure().ExecuteCommand("OSP_OTB_CRM_Contactors_DBImportUpdate", parameterValue2);
                                    //    }
                                    //}

                                    //listFiltContactors = listContactorsInFormal.Where(t => t.CustomerId == oSameCustoemr.CustomerId).ToList();
                                    //listFiltChooseContactors = listChooseContactorsInFormal.Where(t => t.CustomerId == oChooseCustomer.CustomerId).ToList();

                                    //foreach (View_OPM_ExhibitionContactors oFiltChooseContactors in listFiltChooseContactors)
                                    //{
                                    //    List<View_OPM_ExhibitionContactors> listSameContactor = listFiltContactors.Where(t => t.ContactorName == oFiltChooseContactors.ContactorName).ToList();

                                    //    foreach (View_OPM_ExhibitionContactors oSameContactor in listSameContactor)
                                    //    {
                                    //        SugarParameter[] parameterValue2 = new SugarParameter[]
                                    //        {
                                    //            new SugarParameter("@Type", "2"),
                                    //            new SugarParameter("@Guid1", oSameContactor.ContactorId),
                                    //            new SugarParameter("@Guid2", oFiltChooseContactors.ContactorId),
                                    //            new SugarParameter("@ModifyUser", sUSERID),
                                    //            new SugarParameter("@ModifyDate", dtNow)
                                    //        };

                                    //        iRel = db.Ado.UseStoredProcedure().ExecuteCommand("OSP_OTB_CRM_Contactors_DBImportUpdate", parameterValue2);
                                    //    }
                                    //}
                                //}
                            }
                            break;
                        case "insertupdate":
                            //foreach (View_OPM_ExhibitionCustomers oChooseCustomer in listChooseCustomers)
                            //{
                            //    //確認名單中是否有重複客戶，沒有時進行新增
                            //    listSameCustomer = listCustomers.Where(t => (t.CustomerCName != "" && t.CustomerCName == oChooseCustomer.CustomerCName) || (t.CustomerEName != "" && t.CustomerEName == oChooseCustomer.CustomerEName) || (t.UniCode != "" && t.UniCode == oChooseCustomer.UniCode)).ToList();
                                
                            //    if (listSameCustomer.Count == 0)
                            //    {
                            //        OTB_OPM_ExhibitionCustomers oInsertCustomer = new OTB_OPM_ExhibitionCustomers();
                            //        oInsertCustomer.ExhibitionNO = sSN;
                            //        oInsertCustomer.CustomerId = oChooseCustomer.CustomerId;
                            //        oInsertCustomer.SourceType = "3";
                            //        oInsertCustomer.ListSource = "ImportFromDB"; //oChooseCustomer.ListSource;
                            //        oInsertCustomer.BoothNumber = oChooseCustomer.BoothNumber;
                            //        oInsertCustomer.NumberOfBooths = oChooseCustomer.NumberOfBooths;
                            //        oInsertCustomer.CreateUser = sUSERID;
                            //        oInsertCustomer.ModifyUser = sUSERID;
                            //        oInsertCustomer.CreateDate = dtNow;
                            //        oInsertCustomer.ModifyDate = dtNow;

                            //        //新增客戶關聯
                            //        iRel = db.Insertable(oInsertCustomer).ExecuteCommand();

                            //        //新增正式聯絡人關聯
                            //        List<View_OPM_ExhibitionContactors> listFiltChooseContactors = listChooseContactorsFormal.Where(t => t.CustomerId == oChooseCustomer.CustomerId).ToList();
                            //        foreach (View_OPM_ExhibitionContactors oFiltChooseContactors in listFiltChooseContactors)
                            //        {
                            //            OTB_OPM_ExhibitionContactors oInsertContactor = new OTB_OPM_ExhibitionContactors();
                            //            oInsertContactor.ExhibitionNO = sSN;
                            //            oInsertContactor.CustomerId = oChooseCustomer.CustomerId;
                            //            oInsertContactor.ContactorId = oFiltChooseContactors.ContactorId;
                            //            oInsertContactor.SourceType = "3";
                            //            oInsertContactor.CreateUser = sUSERID;
                            //            oInsertContactor.ModifyUser = sUSERID;
                            //            oInsertContactor.CreateDate = dtNow;
                            //            oInsertContactor.ModifyDate = dtNow;
                            //            oInsertContactor.IsFormal = "Y";
                            //            iRel = db.Insertable(oInsertContactor).ExecuteCommand();
                            //        }

                            //        //新增非正式聯絡人關聯
                            //        listFiltChooseContactors = new List<View_OPM_ExhibitionContactors>();
                            //        listFiltChooseContactors = listChooseContactorsInFormal.Where(t => t.CustomerId == oChooseCustomer.CustomerId).ToList();
                            //        foreach (View_OPM_ExhibitionContactors oFiltChooseContactors in listFiltChooseContactors)
                            //        {
                            //            OTB_OPM_ExhibitionContactors oInsertContactor = new OTB_OPM_ExhibitionContactors();
                            //            oInsertContactor.ExhibitionNO = sSN;
                            //            oInsertContactor.CustomerId = oChooseCustomer.CustomerId;
                            //            oInsertContactor.ContactorId = oFiltChooseContactors.ContactorId;
                            //            oInsertContactor.SourceType = "3";
                            //            oInsertContactor.CreateUser = sUSERID;
                            //            oInsertContactor.ModifyUser = sUSERID;
                            //            oInsertContactor.CreateDate = dtNow;
                            //            oInsertContactor.ModifyDate = dtNow;
                            //            oInsertContactor.IsFormal = "N";
                            //            iRel = db.Insertable(oInsertContactor).ExecuteCommand();
                            //        }
                            //    }
                            //    else //名單中有重複客戶
                            //    {
                            //        foreach (View_OPM_ExhibitionCustomers oSameCustoemr in listSameCustomer)
                            //        {
                            //            //更新客戶
                            //            if (oSameCustoemr.IsAudit != "Y")
                            //            {
                            //                SugarParameter[] parameterValue1 = new SugarParameter[]
                            //                {
                            //                    new SugarParameter("@Guid1", oSameCustoemr.CustomerId),
                            //                    new SugarParameter("@Guid2", oChooseCustomer.CustomerId),
                            //                    new SugarParameter("@ModifyUser", sUSERID),
                            //                    new SugarParameter("@ModifyDate", dtNow)
                            //                };

                            //                iRel = db.Ado.UseStoredProcedure().ExecuteCommand("OSP_OTB_CRM_Customers_DBImportUpdate", parameterValue1);
                            //            }

                            //            //更新正式聯絡人
                            //            List<View_OPM_ExhibitionContactors> listFiltContactors = listContactorsFormal.Where(t => t.CustomerId == oSameCustoemr.CustomerId).ToList();
                            //            List<View_OPM_ExhibitionContactors> listFiltChooseContactors = listChooseContactorsFormal.Where(t => t.CustomerId == oChooseCustomer.CustomerId).ToList();

                            //            foreach (View_OPM_ExhibitionContactors oFiltChooseContactors in listFiltChooseContactors)
                            //            {
                            //                List<View_OPM_ExhibitionContactors> listSameContactor = listFiltContactors.Where(t => t.ContactorName == oFiltChooseContactors.ContactorName).ToList();

                            //                foreach (View_OPM_ExhibitionContactors oSameContactor in listSameContactor)
                            //                {
                            //                    SugarParameter[] parameterValue2 = new SugarParameter[]
                            //                    {
                            //                    new SugarParameter("@Type", "1"),
                            //                    new SugarParameter("@Guid1", oSameContactor.ContactorId),
                            //                    new SugarParameter("@Guid2", oFiltChooseContactors.ContactorId),
                            //                    new SugarParameter("@ModifyUser", sUSERID),
                            //                    new SugarParameter("@ModifyDate", dtNow)
                            //                    };

                            //                    iRel = db.Ado.UseStoredProcedure().ExecuteCommand("OSP_OTB_CRM_Contactors_DBImportUpdate", parameterValue2);
                            //                }
                            //            }

                            //            //更新非正式聯絡人
                            //            listFiltContactors = listContactorsInFormal.Where(t => t.CustomerId == oSameCustoemr.CustomerId).ToList();
                            //            listFiltChooseContactors = listChooseContactorsInFormal.Where(t => t.CustomerId == oChooseCustomer.CustomerId).ToList();

                            //            foreach (View_OPM_ExhibitionContactors oFiltChooseContactors in listFiltChooseContactors)
                            //            {
                            //                List<View_OPM_ExhibitionContactors> listSameContactor = listFiltContactors.Where(t => t.ContactorName == oFiltChooseContactors.ContactorName).ToList();

                            //                foreach (View_OPM_ExhibitionContactors oSameContactor in listSameContactor)
                            //                {
                            //                    SugarParameter[] parameterValue2 = new SugarParameter[]
                            //                    {
                            //                    new SugarParameter("@Type", "2"),
                            //                    new SugarParameter("@Guid1", oSameContactor.ContactorId),
                            //                    new SugarParameter("@Guid2", oFiltChooseContactors.ContactorId),
                            //                    new SugarParameter("@ModifyUser", sUSERID),
                            //                    new SugarParameter("@ModifyDate", dtNow)
                            //                    };

                            //                    iRel = db.Ado.UseStoredProcedure().ExecuteCommand("OSP_OTB_CRM_Contactors_DBImportUpdate", parameterValue2);
                            //                }
                            //            }
                                        
                            //            //抓取兩邊名單中的正式聯絡人進行比對後處理聯絡人關聯
                            //            listFiltContactors = listContactorsFormal.Where(t => t.CustomerId == oSameCustoemr.CustomerId).ToList();
                            //            listFiltChooseContactors = listChooseContactorsFormal.Where(t => t.CustomerId == oChooseCustomer.CustomerId).ToList();

                            //            foreach (View_OPM_ExhibitionContactors oFiltChooseContactors in listFiltChooseContactors)
                            //            {
                            //                intCount = 0;
                            //                intCount = listFiltContactors.Where(t => t.ContactorName == oFiltChooseContactors.ContactorName).Count();

                            //                if (intCount == 0)
                            //                {
                            //                    string strContactorId = oFiltChooseContactors.ContactorId;
                            //                    intCount = db.Queryable<OTB_CRM_Contactors>().Where(x => x.CustomerId == oFiltChooseContactors.CustomerId && x.ContactorName == oFiltChooseContactors.ContactorName).Count();
                            //                    if (intCount == 0)
                            //                    {
                            //                        strContactorId = Guid.NewGuid().ToString();
                            //                        SugarParameter[] parameterValue = new SugarParameter[]
                            //                        {
                            //                                new SugarParameter("@Type", "1"),
                            //                                new SugarParameter("@Guid", strContactorId),
                            //                                new SugarParameter("@CustomerId", oSameCustoemr.CustomerId),
                            //                                new SugarParameter("@ContactorId", oFiltChooseContactors.ContactorId),
                            //                                new SugarParameter("@CreateUser", sUSERID),
                            //                                new SugarParameter("@CreateDate", dtNow)
                            //                        };
                            //                        iRel = db.Ado.UseStoredProcedure().ExecuteCommand("OSP_OTB_CRM_Contactors_DBImportInsert", parameterValue);
                            //                    }

                            //                    OTB_OPM_ExhibitionContactors oInsertContactor = new OTB_OPM_ExhibitionContactors();
                            //                    oInsertContactor.ExhibitionNO = sSN;
                            //                    oInsertContactor.CustomerId = oSameCustoemr.CustomerId;
                            //                    oInsertContactor.ContactorId = strContactorId;
                            //                    oInsertContactor.SourceType = "3";
                            //                    oInsertContactor.CreateUser = sUSERID;
                            //                    oInsertContactor.ModifyUser = sUSERID;
                            //                    oInsertContactor.CreateDate = dtNow;
                            //                    oInsertContactor.ModifyDate = dtNow;
                            //                    oInsertContactor.IsFormal = "Y";
                            //                    iRel = db.Insertable(oInsertContactor).ExecuteCommand();
                            //                }
                            //            }

                            //            //非正式聯絡人
                            //            listFiltContactors = listContactorsInFormal.Where(t => t.CustomerId == oSameCustoemr.CustomerId).ToList();
                            //            listFiltChooseContactors = listChooseContactorsInFormal.Where(t => t.CustomerId == oChooseCustomer.CustomerId).ToList();

                            //            foreach (View_OPM_ExhibitionContactors oFiltChooseContactors in listFiltChooseContactors)
                            //            {
                            //                intCount = 0;
                            //                intCount = listFiltContactors.Where(t => t.ContactorName == oFiltChooseContactors.ContactorName).Count();

                            //                if (intCount == 0)
                            //                {
                            //                    string strContactorId = oFiltChooseContactors.ContactorId;
                            //                    intCount = db.Queryable<OTB_CRM_ContactorsTemp>().Where(x => x.CustomerId == oFiltChooseContactors.CustomerId && x.ContactorName == oFiltChooseContactors.ContactorName).Count();
                            //                    if (intCount == 0)
                            //                    {
                            //                        strContactorId = Guid.NewGuid().ToString();
                            //                        SugarParameter[] parameterValue = new SugarParameter[]
                            //                        {
                            //                                new SugarParameter("@Type", "2"),
                            //                                new SugarParameter("@Guid", strContactorId),
                            //                                new SugarParameter("@CustomerId", oSameCustoemr.CustomerId),
                            //                                new SugarParameter("@ContactorId", oFiltChooseContactors.ContactorId),
                            //                                new SugarParameter("@CreateUser", sUSERID),
                            //                                new SugarParameter("@CreateDate", dtNow)
                            //                        };
                            //                        iRel = db.Ado.UseStoredProcedure().ExecuteCommand("OSP_OTB_CRM_Contactors_DBImportInsert", parameterValue);
                            //                    }

                            //                    OTB_OPM_ExhibitionContactors oInsertContactor = new OTB_OPM_ExhibitionContactors();
                            //                    oInsertContactor.ExhibitionNO = sSN;
                            //                    oInsertContactor.CustomerId = oSameCustoemr.CustomerId;
                            //                    oInsertContactor.ContactorId = strContactorId;
                            //                    oInsertContactor.SourceType = "3";
                            //                    oInsertContactor.CreateUser = sUSERID;
                            //                    oInsertContactor.ModifyUser = sUSERID;
                            //                    oInsertContactor.CreateDate = dtNow;
                            //                    oInsertContactor.ModifyDate = dtNow;
                            //                    oInsertContactor.IsFormal = "N";
                            //                    iRel = db.Insertable(oInsertContactor).ExecuteCommand();
                            //                }
                            //            }
                            //        }
                            //    }
                            //}
                            
                            break;
                    }
                    
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iRel);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Exhibition_UpdService), @"展覽管理", @"AddUpdateList（新增更新名單 資料庫匯入）", @"", @"", @"");
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

        #endregion 新增更新名單 資料庫匯入

        #region 匯入展覽名單

        /// <summary>
        /// 匯入展覽名單
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on ImportCustomers</param>
        /// <returns></returns>
        public ResponseMessage ImportExhibitionList(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    int iRel = 0;

                    var sFileId = _fetchString(i_crm, @"FileId");
                    var sFileName = _fetchString(i_crm, @"FileName");
                    var iSN = _fetchString(i_crm, @"SN");
                    var sRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"OutFiles\Temporary\");//Word模版路徑
                    var sfileName = sFileName.Split(new string[] { @"." }, StringSplitOptions.RemoveEmptyEntries);
                    var sSubFileName = sfileName.LastOrDefault();     //副檔名
                    sFileName = sRoot + sFileId + @"." + sSubFileName;

                    var book = new Workbook(sFileName);

                    var sheet = book.Worksheets[0];
                    var cells = sheet.Cells;
                    var tbFeeItems = cells.ExportDataTableAsString(1, 0, cells.MaxDataRow, cells.MaxDataColumn + 1, false);

                    int i = 1;

                    if (tbFeeItems.Rows.Count > 0)
                    {
                        List<OTB_CRM_ImportExhibitionList> listExhibitionList = new List<OTB_CRM_ImportExhibitionList>();
                        foreach (DataRow row in tbFeeItems.Rows)
                        {
                            try
                            {
                                if (listExhibitionList.Count > 0)
                                {
                                    int intCount = listExhibitionList.Where(t => 
                                        (t.CustomerCName != "" && t.CustomerCName == row[@"Column1"].ToString()) || 
                                        (t.CustomerEName != "" && t.CustomerEName == row[@"Column2"].ToString()) || 
                                        (t.UniCode != "" && t.UniCode == row[@"Column3"].ToString())
                                        ).Count();

                                    if (intCount > 0)
                                    {
                                        if (sMsg == null)
                                        {
                                            sMsg = "";
                                        }
                                        sMsg += "第" + i.ToString() + "列：" + "公司中文名稱「" + row[@"Column1"].ToString() + "」公司英文名稱「" + row[@"Column2"].ToString() + "」統一編號「" + row[@"Column3"].ToString() + "」，為重複資料，無法匯入;<br>";
                                    }
                                }

                                OTB_CRM_ImportExhibitionList oExhibitionList = new OTB_CRM_ImportExhibitionList();
                                oExhibitionList.ExhibitionNO = iSN;
                                oExhibitionList.CustomerCName = row[@"Column1"].ToString();// 公司中文名稱
                                oExhibitionList.CustomerEName = row[@"Column2"].ToString();// 公司英文名稱
                                oExhibitionList.UniCode = row[@"Column3"].ToString();// 統一編號

                                oExhibitionList.BoothNumber = row[@"Column4"].ToString();// 攤位號
                                oExhibitionList.NumberOfBooths = row[@"Column5"].ToString();// 攤位數
                                oExhibitionList.Telephone = row[@"Column6"].ToString();// 電話
                                oExhibitionList.FAX = row[@"Column7"].ToString();// 傳真
                                oExhibitionList.Address = row[@"Column8"].ToString();// 地址
                                oExhibitionList.WebsiteAdress = row[@"Column9"].ToString();// 網址
                                oExhibitionList.Contactor1 = row[@"Column10"].ToString();// 聯絡人1
                                oExhibitionList.JobTitleC1 = row[@"Column11"].ToString();// 職稱1
                                oExhibitionList.Telephone1C1 = row[@"Column12"].ToString();// 電話1
                                oExhibitionList.Ext1C1 = row[@"Column13"].ToString();// 分機1
                                oExhibitionList.Telephone2C1 = row[@"Column14"].ToString();// 電話2
                                oExhibitionList.Ext2C1 = row[@"Column15"].ToString();// 分機2
                                oExhibitionList.Email1C1 = row[@"Column16"].ToString();// 郵箱1
                                oExhibitionList.Email2C1 = row[@"Column17"].ToString();// 郵箱2
                                oExhibitionList.Contactor2 = row[@"Column18"].ToString();// 聯絡人2
                                oExhibitionList.JobTitleC2 = row[@"Column19"].ToString();// 職稱1
                                oExhibitionList.Telephone1C2 = row[@"Column20"].ToString();// 電話1
                                oExhibitionList.Ext1C2 = row[@"Column21"].ToString();// 分機1
                                oExhibitionList.Telephone2C2 = row[@"Column22"].ToString();// 電話2
                                oExhibitionList.Ext2C2 = row[@"Column23"].ToString();// 分機1
                                oExhibitionList.Email1C2 = row[@"Column24"].ToString();// 郵箱1
                                oExhibitionList.Email2C2 = row[@"Column25"].ToString();// 郵箱2
                                oExhibitionList.Contactor3 = row[@"Column26"].ToString();// 聯絡人3
                                oExhibitionList.JobTitleC3 = row[@"Column27"].ToString();// 職稱1
                                oExhibitionList.Telephone1C3 = row[@"Column28"].ToString();// 電話1
                                oExhibitionList.Ext1C3 = row[@"Column29"].ToString();// 分機1
                                oExhibitionList.Telephone2C3 = row[@"Column30"].ToString();// 電話2
                                oExhibitionList.Ext2C3 = row[@"Column31"].ToString();// 分機2
                                oExhibitionList.Email1C3 = row[@"Column32"].ToString();// 郵箱1
                                oExhibitionList.Email2C3 = row[@"Column33"].ToString();// 郵箱2
                                oExhibitionList.Seq = i.ToString();

                                listExhibitionList.Add(oExhibitionList);
                                //int iRel = db.Deleteable<OTB_CRM_Customers>().Where(x => x.guid == sId).ExecuteCommand();
                            }
                            catch (Exception ex)
                            {
                                if (sMsg == null)
                                {
                                    sMsg = "";
                                }
                                sMsg += "第" + i.ToString() + "列：" + "公司中文名稱「" + row[@"Column1"].ToString() + "」公司英文名稱「" + row[@"Column2"].ToString() + "」統一編號「" + row[@"Column3"].ToString() + "」 " +  ex.Message + ";<br>";
                            }
                            i++;
                        }
                        if (listExhibitionList.Count > 0 && sMsg == null)
                        {
                            iRel = db.Deleteable<OTB_CRM_ImportExhibitionList>().Where(x => x.ExhibitionNO == iSN).ExecuteCommand();
                            iRel = db.Insertable(listExhibitionList).ExecuteCommand();
                        }
                    }
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iRel);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Exhibition_UpdService), @"展覽管理", @"ImportExhibitionList（匯入展覽名單）", @"", @"", @"");
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

        #endregion 匯入展覽名單

        #region 獲取匯入的展覽名單

        /// <summary>
        /// 獲取匯入的展覽名單
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on GetCustomers</param>
        /// <returns></returns>
        public ResponseMessage GetExhibitionListFile(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    string sSN = _fetchString(i_crm, @"SN");
                    int intPageCount = 0;

                    var oImportExhibitionList = db.Queryable<OTB_CRM_ImportExhibitionList>().Where(x => x.ExhibitionNO == sSN).ToPageList(1, 999999999, ref intPageCount);
                    
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, oImportExhibitionList);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Exhibition_UpdService), @"展覽管理", @"GetExhibitionListFile（獲取匯入的展覽名單）", @"", @"", @"");
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

        #endregion 獲取匯入的展覽名單

        #region 新增更新名單 檔案匯入

        /// <summary>
        /// 新增更新名單 檔案匯入
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on GetCustomers</param>
        /// <returns></returns>
        public ResponseMessage AddUpdateListFile(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();

            string strCustomerCName = "";
            string strCustomerEName = "";
            string strUnicode = "";

            try
            {
                do
                {
                    int iRel = 1;
                    string sOrgID = i_crm.ORIGID;
                    string sSN = _fetchString(i_crm, @"SN");
                    string sType = _fetchString(i_crm, @"Type");
                    string sListSource = _fetchString(i_crm, @"ListSource");

                    string sUSERID = i_crm.USERID;
                    DateTime dtNow = DateTime.Now;
                    
                    var listCustomers = db.Queryable<OTB_OPM_ExhibitionCustomers, OTB_CRM_Customers>((t1, t2) => new object[] {
                          JoinType.Inner,t1.CustomerId==t2.guid})
                          .Where(t1 => t1.ExhibitionNO == sSN)
                          .Select((t1, t2) => new View_OPM_ExhibitionCustomers
                          {
                              SN = t1.SN,
                              ExhibitionNO = t1.ExhibitionNO,
                              CustomerId = t1.CustomerId,
                              CustomerCName = t2.CustomerCName,
                              CustomerEName = t2.CustomerEName,
                              CustomerShotCName = t2.CustomerShotCName,
                              UniCode = t2.UniCode,
                              BoothNumber = t1.BoothNumber,
                              NumberOfBooths = t1.NumberOfBooths,
                              ListSource = t1.ListSource,
                              IsAudit = t2.IsAudit,
                          }).MergeTable().ToList();

                    var listContactorsInFormal = db.Queryable<OTB_OPM_ExhibitionContactors, OTB_CRM_ContactorsTemp>((t1, t2) => new object[] {
                          JoinType.Inner,t1.ContactorId==t2.guid})
                          .Where(t1 => t1.ExhibitionNO == sSN)
                          .Select((t1, t2) => new View_OPM_ExhibitionContactors
                          {
                              SN = t1.SN,
                              ExhibitionNO = t1.ExhibitionNO,
                              CustomerId = t1.CustomerId,
                              ContactorId = t1.ContactorId,
                              ContactorName = t2.ContactorName,
                              SourceType = t1.SourceType
                          }).MergeTable().ToList();

                    var oImportExhibitionList = db.Queryable<OTB_CRM_ImportExhibitionList>().Where(x => x.ExhibitionNO == sSN).ToList();

                    List<View_OPM_ExhibitionCustomers> listSameCustomer = new List<View_OPM_ExhibitionCustomers>();
                    
                    switch (sType)
                    {
                        case "insert":
                            foreach (OTB_CRM_ImportExhibitionList oImportCustomer in oImportExhibitionList)
                            {
                                strCustomerCName = oImportCustomer.CustomerCName;
                                strCustomerEName = oImportCustomer.CustomerEName;
                                strUnicode = oImportCustomer.UniCode;

                                string strCustomerId = "";

                                //確認名單中是否有同名或同統編之客戶，沒有時進行新增
                                listSameCustomer = listCustomers.Where(t => (t.CustomerCName != "" && (t.CustomerCName == oImportCustomer.CustomerCName || t.CustomerShotCName == oImportCustomer.CustomerCName)) || (t.CustomerEName != "" && t.CustomerEName == oImportCustomer.CustomerEName) || (t.UniCode != "" && t.UniCode == oImportCustomer.UniCode)).ToList();

                                if (listSameCustomer.Count == 0) //名單中沒有重複客戶時
                                {
                                    var listCheckCustomer = db.Queryable<OTB_CRM_Customers>()
                                            .Where(x => x.Effective == "Y" && ((x.CustomerCName != "" && (x.CustomerCName == oImportCustomer.CustomerCName || x.CustomerShotCName == oImportCustomer.CustomerCName)) || (x.CustomerEName != "" && x.CustomerEName == oImportCustomer.CustomerEName) || (x.UniCode != "" && x.UniCode == oImportCustomer.UniCode)))
                                            //.WhereIF((i_crm.ORIGID == "SG" || i_crm.ORIGID == "SE"), x => x.OrgID == i_crm.ORIGID)
                                            .WhereIF((i_crm.ORIGID != "TE" && i_crm.ORIGID != "TG"), x => x.OrgID == i_crm.ORIGID)
                                            .WhereIF((i_crm.ORIGID == "TE" || i_crm.ORIGID == "TG"), x => x.OrgID == "TE" || x.OrgID == "TG")
                                            .ToList();

                                    if (listCheckCustomer.Count == 0) //名單中沒有且為新客戶時
                                    {
                                        //新增客戶
                                        strCustomerId = Guid.NewGuid().ToString();
                                        OTB_CRM_Customers oInsertCustomer = new OTB_CRM_Customers();
                                        oInsertCustomer.OrgID = "TE";
                                        oInsertCustomer.guid = strCustomerId;
                                        oInsertCustomer.TransactionType = "";
                                        oInsertCustomer.CustomerNO = "    ";
                                        oInsertCustomer.CustomerCName = oImportCustomer.CustomerCName;
                                        oInsertCustomer.CustomerEName = oImportCustomer.CustomerEName;
                                        oInsertCustomer.UniCode = oImportCustomer.UniCode;
                                        oInsertCustomer.Telephone = oImportCustomer.Telephone;
                                        oInsertCustomer.FAX = oImportCustomer.FAX;
                                        oInsertCustomer.Address = oImportCustomer.Address;
                                        oInsertCustomer.WebsiteAdress = oImportCustomer.WebsiteAdress;
                                        oInsertCustomer.IsAudit = "N";
                                        oInsertCustomer.Effective = "Y";
                                        oInsertCustomer.CreateUser = sUSERID;
                                        oInsertCustomer.ModifyUser = sUSERID;
                                        oInsertCustomer.CreateDate = dtNow;
                                        oInsertCustomer.ModifyDate = dtNow;

                                        OTB_CRM_CustomersMST oInsertCustomerMST = new OTB_CRM_CustomersMST();
                                        oInsertCustomerMST.OrgID = "TE";
                                        oInsertCustomerMST.guid = Guid.NewGuid().ToString();
                                        oInsertCustomerMST.customer_guid = strCustomerId;
                                        oInsertCustomerMST.CustomerNO = "";
                                        oInsertCustomerMST.Effective = "Y";
                                        oInsertCustomerMST.CreateUser = sUSERID;
                                        oInsertCustomerMST.ModifyUser = sUSERID;
                                        oInsertCustomerMST.CreateDate = dtNow;
                                        oInsertCustomerMST.ModifyDate = dtNow;

                                        iRel = db.Insertable(oInsertCustomer).ExecuteCommand();
                                        iRel = db.Insertable(oInsertCustomerMST).ExecuteCommand();

                                        OTB_OPM_ExhibitionCustomers oInsertExhibitionCustomers = new OTB_OPM_ExhibitionCustomers();
                                        oInsertExhibitionCustomers.ExhibitionNO = sSN;
                                        oInsertExhibitionCustomers.CustomerId = strCustomerId;
                                        oInsertExhibitionCustomers.SourceType = "2";
                                        oInsertExhibitionCustomers.ListSource = sListSource;
                                        oInsertExhibitionCustomers.BoothNumber = oImportCustomer.BoothNumber;
                                        oInsertExhibitionCustomers.NumberOfBooths = oImportCustomer.NumberOfBooths;
                                        oInsertExhibitionCustomers.CreateUser = sUSERID;
                                        oInsertExhibitionCustomers.ModifyUser = sUSERID;
                                        oInsertExhibitionCustomers.CreateDate = dtNow;
                                        oInsertExhibitionCustomers.ModifyDate = dtNow;

                                        //新增客戶關聯
                                        iRel = db.Insertable(oInsertExhibitionCustomers).ExecuteCommand();

                                        if (oImportCustomer.Contactor1 != "")
                                        {
                                            //新增暫存聯絡人1
                                            OTB_CRM_ContactorsTemp oInsertContactors1 = new OTB_CRM_ContactorsTemp();
                                            oInsertContactors1.guid = Guid.NewGuid().ToString();
                                            oInsertContactors1.CustomerId = strCustomerId;
                                            oInsertContactors1.ContactorName = oImportCustomer.Contactor1;
                                            oInsertContactors1.JobTitle = oImportCustomer.JobTitleC1;
                                            oInsertContactors1.Telephone1 = oImportCustomer.Telephone1C1;
                                            oInsertContactors1.Telephone2 = oImportCustomer.Telephone2C1;
                                            oInsertContactors1.Ext1 = oImportCustomer.Ext1C1;
                                            oInsertContactors1.Ext2 = oImportCustomer.Ext2C1;
                                            oInsertContactors1.Email1 = oImportCustomer.Email1C1;
                                            oInsertContactors1.Email2 = oImportCustomer.Email2C1;
                                            oInsertContactors1.CreateUser = sUSERID;
                                            oInsertContactors1.ModifyUser = sUSERID;
                                            oInsertContactors1.CreateDate = dtNow;
                                            oInsertContactors1.ModifyDate = dtNow;
                                            iRel = db.Insertable(oInsertContactors1).ExecuteCommand();

                                            //新增聯絡人1關聯
                                            OTB_OPM_ExhibitionContactors oInsertExhibitionContactors1 = new OTB_OPM_ExhibitionContactors();
                                            oInsertExhibitionContactors1.ExhibitionNO = sSN;
                                            oInsertExhibitionContactors1.CustomerId = strCustomerId;
                                            oInsertExhibitionContactors1.ContactorId = oInsertContactors1.guid;
                                            oInsertExhibitionContactors1.SourceType = "2";
                                            oInsertExhibitionContactors1.CreateUser = sUSERID;
                                            oInsertExhibitionContactors1.ModifyUser = sUSERID;
                                            oInsertExhibitionContactors1.CreateDate = dtNow;
                                            oInsertExhibitionContactors1.ModifyDate = dtNow;
                                            oInsertExhibitionContactors1.IsFormal = "N";
                                            iRel = db.Insertable(oInsertExhibitionContactors1).ExecuteCommand();
                                        }

                                        if (oImportCustomer.Contactor2 != "")
                                        {
                                            //新增暫存聯絡人2
                                            OTB_CRM_ContactorsTemp oInsertContactors2 = new OTB_CRM_ContactorsTemp();
                                            oInsertContactors2.guid = Guid.NewGuid().ToString();
                                            oInsertContactors2.CustomerId = strCustomerId;
                                            oInsertContactors2.ContactorName = oImportCustomer.Contactor2;
                                            oInsertContactors2.JobTitle = oImportCustomer.JobTitleC2;
                                            oInsertContactors2.Telephone1 = oImportCustomer.Telephone1C2;
                                            oInsertContactors2.Telephone2 = oImportCustomer.Telephone2C2;
                                            oInsertContactors2.Ext1 = oImportCustomer.Ext1C2;
                                            oInsertContactors2.Ext2 = oImportCustomer.Ext2C2;
                                            oInsertContactors2.Email1 = oImportCustomer.Email1C2;
                                            oInsertContactors2.Email2 = oImportCustomer.Email2C2;
                                            oInsertContactors2.CreateUser = sUSERID;
                                            oInsertContactors2.ModifyUser = sUSERID;
                                            oInsertContactors2.CreateDate = dtNow;
                                            oInsertContactors2.ModifyDate = dtNow;
                                            iRel = db.Insertable(oInsertContactors2).ExecuteCommand();

                                            //新增聯絡人2關聯
                                            OTB_OPM_ExhibitionContactors oInsertExhibitionContactors2 = new OTB_OPM_ExhibitionContactors();
                                            oInsertExhibitionContactors2.ExhibitionNO = sSN;
                                            oInsertExhibitionContactors2.CustomerId = strCustomerId;
                                            oInsertExhibitionContactors2.ContactorId = oInsertContactors2.guid;
                                            oInsertExhibitionContactors2.SourceType = "2";
                                            oInsertExhibitionContactors2.CreateUser = sUSERID;
                                            oInsertExhibitionContactors2.ModifyUser = sUSERID;
                                            oInsertExhibitionContactors2.CreateDate = dtNow;
                                            oInsertExhibitionContactors2.ModifyDate = dtNow;
                                            oInsertExhibitionContactors2.IsFormal = "N";
                                            iRel = db.Insertable(oInsertExhibitionContactors2).ExecuteCommand();
                                        }

                                        if (oImportCustomer.Contactor3 != "")
                                        {
                                            //新增暫存聯絡人3
                                            OTB_CRM_ContactorsTemp oInsertContactors3 = new OTB_CRM_ContactorsTemp();
                                            oInsertContactors3.guid = Guid.NewGuid().ToString();
                                            oInsertContactors3.CustomerId = strCustomerId;
                                            oInsertContactors3.ContactorName = oImportCustomer.Contactor3;
                                            oInsertContactors3.JobTitle = oImportCustomer.JobTitleC3;
                                            oInsertContactors3.Telephone1 = oImportCustomer.Telephone1C3;
                                            oInsertContactors3.Telephone2 = oImportCustomer.Telephone2C3;
                                            oInsertContactors3.Ext1 = oImportCustomer.Ext1C3;
                                            oInsertContactors3.Ext2 = oImportCustomer.Ext2C3;
                                            oInsertContactors3.Email1 = oImportCustomer.Email1C3;
                                            oInsertContactors3.Email2 = oImportCustomer.Email2C3;
                                            oInsertContactors3.CreateUser = sUSERID;
                                            oInsertContactors3.ModifyUser = sUSERID;
                                            oInsertContactors3.CreateDate = dtNow;
                                            oInsertContactors3.ModifyDate = dtNow;
                                            iRel = db.Insertable(oInsertContactors3).ExecuteCommand();

                                            //新增聯絡人3關聯
                                            OTB_OPM_ExhibitionContactors oInsertExhibitionContactors3 = new OTB_OPM_ExhibitionContactors();
                                            oInsertExhibitionContactors3.ExhibitionNO = sSN;
                                            oInsertExhibitionContactors3.CustomerId = strCustomerId;
                                            oInsertExhibitionContactors3.ContactorId = oInsertContactors3.guid;
                                            oInsertExhibitionContactors3.SourceType = "3";
                                            oInsertExhibitionContactors3.CreateUser = sUSERID;
                                            oInsertExhibitionContactors3.ModifyUser = sUSERID;
                                            oInsertExhibitionContactors3.CreateDate = dtNow;
                                            oInsertExhibitionContactors3.ModifyDate = dtNow;
                                            oInsertExhibitionContactors3.IsFormal = "N";
                                            iRel = db.Insertable(oInsertExhibitionContactors3).ExecuteCommand();
                                        }
                                    }
                                    else //名單中沒有但是既有客戶時
                                    {
                                        strCustomerId = listCheckCustomer[0].guid;

                                        OTB_OPM_ExhibitionCustomers oInsertExhibitionCustomers = new OTB_OPM_ExhibitionCustomers();
                                        oInsertExhibitionCustomers.ExhibitionNO = sSN;
                                        oInsertExhibitionCustomers.CustomerId = strCustomerId;
                                        oInsertExhibitionCustomers.SourceType = "2";
                                        oInsertExhibitionCustomers.ListSource = sListSource;
                                        oInsertExhibitionCustomers.BoothNumber = oImportCustomer.BoothNumber;
                                        oInsertExhibitionCustomers.NumberOfBooths = oImportCustomer.NumberOfBooths;
                                        oInsertExhibitionCustomers.CreateUser = sUSERID;
                                        oInsertExhibitionCustomers.ModifyUser = sUSERID;
                                        oInsertExhibitionCustomers.CreateDate = dtNow;
                                        oInsertExhibitionCustomers.ModifyDate = dtNow;

                                        //新增客戶關聯
                                        iRel = db.Insertable(oInsertExhibitionCustomers).ExecuteCommand();

                                        if (oImportCustomer.Contactor1 != "")
                                        {
                                            //新增暫存聯絡人1
                                            OTB_CRM_ContactorsTemp oInsertContactors1 = new OTB_CRM_ContactorsTemp();
                                            oInsertContactors1.guid = Guid.NewGuid().ToString();
                                            oInsertContactors1.CustomerId = strCustomerId;
                                            oInsertContactors1.ContactorName = oImportCustomer.Contactor1;
                                            oInsertContactors1.JobTitle = oImportCustomer.JobTitleC1;
                                            oInsertContactors1.Telephone1 = oImportCustomer.Telephone1C1;
                                            oInsertContactors1.Telephone2 = oImportCustomer.Telephone2C1;
                                            oInsertContactors1.Ext1 = oImportCustomer.Ext1C1;
                                            oInsertContactors1.Ext2 = oImportCustomer.Ext2C1;
                                            oInsertContactors1.Email1 = oImportCustomer.Email1C1;
                                            oInsertContactors1.Email2 = oImportCustomer.Email2C1;
                                            oInsertContactors1.CreateUser = sUSERID;
                                            oInsertContactors1.ModifyUser = sUSERID;
                                            oInsertContactors1.CreateDate = dtNow;
                                            oInsertContactors1.ModifyDate = dtNow;
                                            iRel = db.Insertable(oInsertContactors1).ExecuteCommand();

                                            //新增聯絡人1關聯
                                            OTB_OPM_ExhibitionContactors oInsertExhibitionContactors1 = new OTB_OPM_ExhibitionContactors();
                                            oInsertExhibitionContactors1.ExhibitionNO = sSN;
                                            oInsertExhibitionContactors1.CustomerId = strCustomerId;
                                            oInsertExhibitionContactors1.ContactorId = oInsertContactors1.guid;
                                            oInsertExhibitionContactors1.SourceType = "2";
                                            oInsertExhibitionContactors1.CreateUser = sUSERID;
                                            oInsertExhibitionContactors1.ModifyUser = sUSERID;
                                            oInsertExhibitionContactors1.CreateDate = dtNow;
                                            oInsertExhibitionContactors1.ModifyDate = dtNow;
                                            oInsertExhibitionContactors1.IsFormal = "N";
                                            iRel = db.Insertable(oInsertExhibitionContactors1).ExecuteCommand();
                                        }

                                        if (oImportCustomer.Contactor2 != "")
                                        {
                                            //新增暫存聯絡人2
                                            OTB_CRM_ContactorsTemp oInsertContactors2 = new OTB_CRM_ContactorsTemp();
                                            oInsertContactors2.guid = Guid.NewGuid().ToString();
                                            oInsertContactors2.CustomerId = strCustomerId;
                                            oInsertContactors2.ContactorName = oImportCustomer.Contactor2;
                                            oInsertContactors2.JobTitle = oImportCustomer.JobTitleC2;
                                            oInsertContactors2.Telephone1 = oImportCustomer.Telephone1C2;
                                            oInsertContactors2.Telephone2 = oImportCustomer.Telephone2C2;
                                            oInsertContactors2.Ext1 = oImportCustomer.Ext1C2;
                                            oInsertContactors2.Ext2 = oImportCustomer.Ext2C2;
                                            oInsertContactors2.Email1 = oImportCustomer.Email1C2;
                                            oInsertContactors2.Email2 = oImportCustomer.Email2C2;
                                            oInsertContactors2.CreateUser = sUSERID;
                                            oInsertContactors2.ModifyUser = sUSERID;
                                            oInsertContactors2.CreateDate = dtNow;
                                            oInsertContactors2.ModifyDate = dtNow;
                                            iRel = db.Insertable(oInsertContactors2).ExecuteCommand();

                                            //新增聯絡人2關聯
                                            OTB_OPM_ExhibitionContactors oInsertExhibitionContactors2 = new OTB_OPM_ExhibitionContactors();
                                            oInsertExhibitionContactors2.ExhibitionNO = sSN;
                                            oInsertExhibitionContactors2.CustomerId = strCustomerId;
                                            oInsertExhibitionContactors2.ContactorId = oInsertContactors2.guid;
                                            oInsertExhibitionContactors2.SourceType = "2";
                                            oInsertExhibitionContactors2.CreateUser = sUSERID;
                                            oInsertExhibitionContactors2.ModifyUser = sUSERID;
                                            oInsertExhibitionContactors2.CreateDate = dtNow;
                                            oInsertExhibitionContactors2.ModifyDate = dtNow;
                                            oInsertExhibitionContactors2.IsFormal = "N";
                                            iRel = db.Insertable(oInsertExhibitionContactors2).ExecuteCommand();
                                        }

                                        if (oImportCustomer.Contactor3 != "")
                                        {
                                            //新增暫存聯絡人3
                                            OTB_CRM_ContactorsTemp oInsertContactors3 = new OTB_CRM_ContactorsTemp();
                                            oInsertContactors3.guid = Guid.NewGuid().ToString();
                                            oInsertContactors3.CustomerId = strCustomerId;
                                            oInsertContactors3.ContactorName = oImportCustomer.Contactor3;
                                            oInsertContactors3.JobTitle = oImportCustomer.JobTitleC3;
                                            oInsertContactors3.Telephone1 = oImportCustomer.Telephone1C3;
                                            oInsertContactors3.Telephone2 = oImportCustomer.Telephone2C3;
                                            oInsertContactors3.Ext1 = oImportCustomer.Ext1C3;
                                            oInsertContactors3.Ext2 = oImportCustomer.Ext2C3;
                                            oInsertContactors3.Email1 = oImportCustomer.Email1C3;
                                            oInsertContactors3.Email2 = oImportCustomer.Email2C3;
                                            oInsertContactors3.CreateUser = sUSERID;
                                            oInsertContactors3.ModifyUser = sUSERID;
                                            oInsertContactors3.CreateDate = dtNow;
                                            oInsertContactors3.ModifyDate = dtNow;
                                            iRel = db.Insertable(oInsertContactors3).ExecuteCommand();

                                            //新增聯絡人3關聯
                                            OTB_OPM_ExhibitionContactors oInsertExhibitionContactors3 = new OTB_OPM_ExhibitionContactors();
                                            oInsertExhibitionContactors3.ExhibitionNO = sSN;
                                            oInsertExhibitionContactors3.CustomerId = strCustomerId;
                                            oInsertExhibitionContactors3.ContactorId = oInsertContactors3.guid;
                                            oInsertExhibitionContactors3.SourceType = "2";
                                            oInsertExhibitionContactors3.CreateUser = sUSERID;
                                            oInsertExhibitionContactors3.ModifyUser = sUSERID;
                                            oInsertExhibitionContactors3.CreateDate = dtNow;
                                            oInsertExhibitionContactors3.ModifyDate = dtNow;
                                            oInsertExhibitionContactors3.IsFormal = "N";
                                            iRel = db.Insertable(oInsertExhibitionContactors3).ExecuteCommand();
                                        }
                                    }
                                }
                                else //名單中已有同名或同統編之客戶
                                {
                                    strCustomerId = listSameCustomer[0].CustomerId;

                                    if (oImportCustomer.Contactor1 != "")
                                    {
                                        //檢查關聯的暫存聯絡人中是否有重複
                                        var listCheckContactor1 = listContactorsInFormal.Where(x => x.CustomerId == strCustomerId && x.ContactorName == oImportCustomer.Contactor1).ToList();
                                        if (listCheckContactor1.Count == 0)
                                        {
                                            //無重複，新增暫存聯絡人1
                                            OTB_CRM_ContactorsTemp oInsertContactors1 = new OTB_CRM_ContactorsTemp();
                                            oInsertContactors1.guid = Guid.NewGuid().ToString();
                                            oInsertContactors1.CustomerId = strCustomerId;
                                            oInsertContactors1.ContactorName = oImportCustomer.Contactor1;
                                            oInsertContactors1.JobTitle = oImportCustomer.JobTitleC1;
                                            oInsertContactors1.Telephone1 = oImportCustomer.Telephone1C1;
                                            oInsertContactors1.Telephone2 = oImportCustomer.Telephone2C1;
                                            oInsertContactors1.Ext1 = oImportCustomer.Ext1C1;
                                            oInsertContactors1.Ext2 = oImportCustomer.Ext2C1;
                                            oInsertContactors1.Email1 = oImportCustomer.Email1C1;
                                            oInsertContactors1.Email2 = oImportCustomer.Email2C1;
                                            oInsertContactors1.CreateUser = sUSERID;
                                            oInsertContactors1.ModifyUser = sUSERID;
                                            oInsertContactors1.CreateDate = dtNow;
                                            oInsertContactors1.ModifyDate = dtNow;
                                            iRel = db.Insertable(oInsertContactors1).ExecuteCommand();

                                            //新增聯絡人1關聯
                                            OTB_OPM_ExhibitionContactors oInsertExhibitionContactors1 = new OTB_OPM_ExhibitionContactors();
                                            oInsertExhibitionContactors1.ExhibitionNO = sSN;
                                            oInsertExhibitionContactors1.CustomerId = strCustomerId;
                                            oInsertExhibitionContactors1.ContactorId = oInsertContactors1.guid;
                                            oInsertExhibitionContactors1.SourceType = "2";
                                            oInsertExhibitionContactors1.CreateUser = sUSERID;
                                            oInsertExhibitionContactors1.ModifyUser = sUSERID;
                                            oInsertExhibitionContactors1.CreateDate = dtNow;
                                            oInsertExhibitionContactors1.ModifyDate = dtNow;
                                            oInsertExhibitionContactors1.IsFormal = "N";
                                            iRel = db.Insertable(oInsertExhibitionContactors1).ExecuteCommand();
                                        }
                                    }

                                    if (oImportCustomer.Contactor2 != "")
                                    {
                                        //檢查關聯的暫存聯絡人中是否有重複
                                        var listCheckContactor2 = listContactorsInFormal.Where(x => x.CustomerId == strCustomerId && x.ContactorName == oImportCustomer.Contactor2).ToList();
                                        if (listCheckContactor2.Count == 0)
                                        {
                                            //無重複，新增聯絡人2
                                            OTB_CRM_ContactorsTemp oInsertContactors2 = new OTB_CRM_ContactorsTemp();
                                            oInsertContactors2.guid = Guid.NewGuid().ToString();
                                            oInsertContactors2.CustomerId = strCustomerId;
                                            oInsertContactors2.ContactorName = oImportCustomer.Contactor2;
                                            oInsertContactors2.JobTitle = oImportCustomer.JobTitleC2;
                                            oInsertContactors2.Telephone1 = oImportCustomer.Telephone1C2;
                                            oInsertContactors2.Telephone2 = oImportCustomer.Telephone2C2;
                                            oInsertContactors2.Ext1 = oImportCustomer.Ext1C2;
                                            oInsertContactors2.Ext2 = oImportCustomer.Ext2C2;
                                            oInsertContactors2.Email1 = oImportCustomer.Email1C2;
                                            oInsertContactors2.Email2 = oImportCustomer.Email2C2;
                                            oInsertContactors2.CreateUser = sUSERID;
                                            oInsertContactors2.ModifyUser = sUSERID;
                                            oInsertContactors2.CreateDate = dtNow;
                                            oInsertContactors2.ModifyDate = dtNow;
                                            iRel = db.Insertable(oInsertContactors2).ExecuteCommand();

                                            //新增聯絡人2關聯
                                            OTB_OPM_ExhibitionContactors oInsertExhibitionContactors2 = new OTB_OPM_ExhibitionContactors();
                                            oInsertExhibitionContactors2.ExhibitionNO = sSN;
                                            oInsertExhibitionContactors2.CustomerId = strCustomerId;
                                            oInsertExhibitionContactors2.ContactorId = oInsertContactors2.guid;
                                            oInsertExhibitionContactors2.SourceType = "2";
                                            oInsertExhibitionContactors2.CreateUser = sUSERID;
                                            oInsertExhibitionContactors2.ModifyUser = sUSERID;
                                            oInsertExhibitionContactors2.CreateDate = dtNow;
                                            oInsertExhibitionContactors2.ModifyDate = dtNow;
                                            oInsertExhibitionContactors2.IsFormal = "N";
                                            iRel = db.Insertable(oInsertExhibitionContactors2).ExecuteCommand();
                                        }
                                    }

                                    if (oImportCustomer.Contactor3 != "")
                                    {
                                        //檢查關聯的暫存聯絡人中是否有重複
                                        var listCheckContactor3 = listContactorsInFormal.Where(x => x.CustomerId == strCustomerId && x.ContactorName == oImportCustomer.Contactor3).ToList();
                                        if (listCheckContactor3.Count == 0)
                                        {
                                            //無重複，新增聯絡人3
                                            OTB_CRM_ContactorsTemp oInsertContactors3 = new OTB_CRM_ContactorsTemp();
                                            oInsertContactors3.guid = Guid.NewGuid().ToString();
                                            oInsertContactors3.CustomerId = strCustomerId;
                                            oInsertContactors3.ContactorName = oImportCustomer.Contactor3;
                                            oInsertContactors3.JobTitle = oImportCustomer.JobTitleC3;
                                            oInsertContactors3.Telephone1 = oImportCustomer.Telephone1C3;
                                            oInsertContactors3.Telephone2 = oImportCustomer.Telephone2C3;
                                            oInsertContactors3.Ext1 = oImportCustomer.Ext1C3;
                                            oInsertContactors3.Ext2 = oImportCustomer.Ext2C3;
                                            oInsertContactors3.Email1 = oImportCustomer.Email1C3;
                                            oInsertContactors3.Email2 = oImportCustomer.Email2C3;
                                            oInsertContactors3.CreateUser = sUSERID;
                                            oInsertContactors3.ModifyUser = sUSERID;
                                            oInsertContactors3.CreateDate = dtNow;
                                            oInsertContactors3.ModifyDate = dtNow;
                                            iRel = db.Insertable(oInsertContactors3).ExecuteCommand();

                                            //新增聯絡人3關聯
                                            OTB_OPM_ExhibitionContactors oInsertExhibitionContactors3 = new OTB_OPM_ExhibitionContactors();
                                            oInsertExhibitionContactors3.ExhibitionNO = sSN;
                                            oInsertExhibitionContactors3.CustomerId = strCustomerId;
                                            oInsertExhibitionContactors3.ContactorId = oInsertContactors3.guid;
                                            oInsertExhibitionContactors3.SourceType = "2";
                                            oInsertExhibitionContactors3.CreateUser = sUSERID;
                                            oInsertExhibitionContactors3.ModifyUser = sUSERID;
                                            oInsertExhibitionContactors3.CreateDate = dtNow;
                                            oInsertExhibitionContactors3.ModifyDate = dtNow;
                                            oInsertExhibitionContactors3.IsFormal = "N";
                                            iRel = db.Insertable(oInsertExhibitionContactors3).ExecuteCommand();
                                        }
                                    }
                                }
                            }
                            break;
                        case "update":
                            foreach (OTB_CRM_ImportExhibitionList oImportCustomer in oImportExhibitionList)
                            {
                                //確認是否有重複客戶，如果有，更新客戶資料
                                listSameCustomer = listCustomers.Where(t => (t.CustomerCName != "" && (t.CustomerCName == oImportCustomer.CustomerCName || t.CustomerShotCName == oImportCustomer.CustomerCName)) || (t.CustomerEName != "" && t.CustomerEName == oImportCustomer.CustomerEName) || (t.UniCode != "" && t.UniCode == oImportCustomer.UniCode)).ToList();

                                foreach (View_OPM_ExhibitionCustomers oSameCustoemr in listSameCustomer)
                                {
                                    if (oSameCustoemr.IsAudit != "Y" && oSameCustoemr.IsAudit != "A" && oSameCustoemr.IsAudit != "P" && oSameCustoemr.IsAudit != "Z")
                                    {
                                        OTB_CRM_Customers oUpdateCustomer = new OTB_CRM_Customers
                                        {
                                            CustomerCName = oImportCustomer.CustomerCName,
                                            UniCode = oImportCustomer.UniCode,
                                            Telephone = oImportCustomer.Telephone,
                                            FAX = oImportCustomer.FAX,
                                            Address = oImportCustomer.Address,
                                            WebsiteAdress = oImportCustomer.WebsiteAdress,
                                            ModifyUser = sUSERID,
                                            ModifyDate = dtNow
                                        };
                                        iRel = db.Updateable(oUpdateCustomer).UpdateColumns(it => new { it.CustomerCName, it.UniCode, it.Telephone, it.FAX, it.Address, it.WebsiteAdress, it.ModifyUser, it.ModifyDate })
                                            .Where(x => x.guid == oSameCustoemr.CustomerId).ExecuteCommand();
                                    }

                                    OTB_OPM_ExhibitionCustomers oUpdateExhibitionCustomer = new OTB_OPM_ExhibitionCustomers
                                    {
                                        ListSource = sListSource,
                                        BoothNumber = oImportCustomer.BoothNumber,
                                        NumberOfBooths = oImportCustomer.NumberOfBooths,
                                        ModifyUser = sUSERID,
                                        ModifyDate = dtNow
                                    };
                                    iRel = db.Updateable(oUpdateExhibitionCustomer).UpdateColumns(it => new { it.ListSource, it.BoothNumber, it.NumberOfBooths, it.ModifyUser, it.ModifyDate })
                                        .Where(x => x.ExhibitionNO == sSN && x.CustomerId == oSameCustoemr.CustomerId).ExecuteCommand();

                                    //更新聯絡人1資料
                                    if (oImportCustomer.Contactor1 != "")
                                    {
                                        //檢查重複聯絡人1
                                        var listCheckContactor1 = listContactorsInFormal.Where(x => x.CustomerId == oSameCustoemr.CustomerId && x.ContactorName == oImportCustomer.Contactor1).ToList();
                                        if (listCheckContactor1.Count != 0)
                                        {
                                            //更新聯絡人1
                                            OTB_CRM_ContactorsTemp oUpdateContactor = new OTB_CRM_ContactorsTemp
                                            {
                                                JobTitle = oImportCustomer.JobTitleC1,
                                                Telephone1 = oImportCustomer.Telephone1C1,
                                                Telephone2 = oImportCustomer.Telephone2C1,
                                                Ext1 = oImportCustomer.Ext1C1,
                                                Ext2 = oImportCustomer.Ext2C1,
                                                Email1 = oImportCustomer.Email1C1,
                                                Email2 = oImportCustomer.Email2C1,
                                                ModifyUser = sUSERID,
                                                ModifyDate = dtNow
                                            };

                                            iRel = db.Updateable(oUpdateContactor).UpdateColumns(it => new { it.JobTitle, it.Telephone1, it.Telephone2, it.Ext1, it.Ext2, it.Email1, it.ModifyUser, it.ModifyDate })
                                                .Where(x => x.guid == listCheckContactor1[0].ContactorId).ExecuteCommand();
                                        }
                                    }

                                    //更新聯絡人2資料
                                    if (oImportCustomer.Contactor2 != "")
                                    {
                                        //檢查重複聯絡人2
                                        var listCheckContactor2 = listContactorsInFormal.Where(x => x.CustomerId == oSameCustoemr.CustomerId && x.ContactorName == oImportCustomer.Contactor2).ToList();
                                        if (listCheckContactor2.Count != 0)
                                        {
                                            //更新聯絡人2
                                            OTB_CRM_ContactorsTemp oUpdateContactor = new OTB_CRM_ContactorsTemp
                                            {
                                                JobTitle = oImportCustomer.JobTitleC2,
                                                Telephone1 = oImportCustomer.Telephone1C2,
                                                Telephone2 = oImportCustomer.Telephone2C2,
                                                Ext1 = oImportCustomer.Ext1C2,
                                                Ext2 = oImportCustomer.Ext2C2,
                                                Email1 = oImportCustomer.Email1C2,
                                                Email2 = oImportCustomer.Email2C2,
                                                ModifyUser = sUSERID,
                                                ModifyDate = dtNow
                                            };

                                            iRel = db.Updateable(oUpdateContactor).UpdateColumns(it => new { it.JobTitle, it.Telephone1, it.Telephone2, it.Ext1, it.Ext2, it.Email1, it.ModifyUser, it.ModifyDate })
                                                .Where(x => x.guid == listCheckContactor2[0].ContactorId).ExecuteCommand();
                                        }
                                    }

                                    //更新聯絡人3資料
                                    if (oImportCustomer.Contactor3 != "")
                                    {
                                        //檢查重複聯絡人3
                                        var listCheckContactor3 = listContactorsInFormal.Where(x => x.CustomerId == oSameCustoemr.CustomerId && x.ContactorName == oImportCustomer.Contactor3).ToList();
                                        if (listCheckContactor3.Count != 0)
                                        {
                                            //更新聯絡人3
                                            OTB_CRM_ContactorsTemp oUpdateContactor = new OTB_CRM_ContactorsTemp
                                            {
                                                JobTitle = oImportCustomer.JobTitleC3,
                                                Telephone1 = oImportCustomer.Telephone1C3,
                                                Telephone2 = oImportCustomer.Telephone2C3,
                                                Ext1 = oImportCustomer.Ext1C3,
                                                Ext2 = oImportCustomer.Ext2C3,
                                                Email1 = oImportCustomer.Email1C3,
                                                Email2 = oImportCustomer.Email2C3,
                                                ModifyUser = sUSERID,
                                                ModifyDate = dtNow
                                            };

                                            iRel = db.Updateable(oUpdateContactor).UpdateColumns(it => new { it.JobTitle, it.Telephone1, it.Telephone2, it.Ext1, it.Ext2, it.Email1, it.ModifyUser, it.ModifyDate })
                                                .Where(x => x.guid == listCheckContactor3[0].ContactorId).ExecuteCommand();
                                        }
                                    }
                                }
                            }
                            break;
                        case "insertupdate":
                            foreach (OTB_CRM_ImportExhibitionList oImportCustomer in oImportExhibitionList)
                            {
                                string strCustomerId = "";

                                //確認名單中是否有同名或同統編之客戶，沒有時進行新增
                                listSameCustomer = listCustomers.Where(t => (t.CustomerCName != "" && (t.CustomerCName == oImportCustomer.CustomerCName || t.CustomerShotCName == oImportCustomer.CustomerCName)) || (t.CustomerEName != "" && t.CustomerEName == oImportCustomer.CustomerEName) || (t.UniCode != "" && t.UniCode == oImportCustomer.UniCode)).ToList();
                                
                                if (listSameCustomer.Count == 0) //名單中沒有重複客戶時
                                {
                                    var listCheckCustomer = db.Queryable<OTB_CRM_Customers>()
                                            .Where(x => x.Effective == "Y" && ((x.CustomerCName != "" && (x.CustomerCName == oImportCustomer.CustomerCName || x.CustomerShotCName == oImportCustomer.CustomerCName)) || (x.CustomerEName != "" && x.CustomerEName == oImportCustomer.CustomerEName) || (x.UniCode != "" && x.UniCode == oImportCustomer.UniCode)))
                                            //.WhereIF((i_crm.ORIGID == "SG" || i_crm.ORIGID == "SE"), x => x.OrgID == i_crm.ORIGID)
                                            .WhereIF((i_crm.ORIGID != "TE" && i_crm.ORIGID != "TG"), x => x.OrgID == i_crm.ORIGID)
                                            .WhereIF((i_crm.ORIGID == "TE" || i_crm.ORIGID == "TG"), x => x.OrgID == "TE" || x.OrgID == "TG")
                                            .ToList();

                                    if (listCheckCustomer.Count == 0) //名單中沒有且為新客戶時
                                    {
                                        //新增客戶
                                        strCustomerId = Guid.NewGuid().ToString();
                                        OTB_CRM_Customers oInsertCustomer = new OTB_CRM_Customers();
                                        oInsertCustomer.OrgID = "TE";
                                        oInsertCustomer.guid = strCustomerId;
                                        oInsertCustomer.TransactionType = "";
                                        oInsertCustomer.CustomerNO = "    ";
                                        oInsertCustomer.CustomerCName = oImportCustomer.CustomerCName;
                                        oInsertCustomer.CustomerEName = oImportCustomer.CustomerEName;
                                        oInsertCustomer.UniCode = oImportCustomer.UniCode;
                                        oInsertCustomer.Telephone = "";
                                        oInsertCustomer.FAX = "";
                                        oInsertCustomer.Address = oImportCustomer.Address;
                                        oInsertCustomer.WebsiteAdress = oImportCustomer.WebsiteAdress;
                                        oInsertCustomer.IsAudit = "N";
                                        oInsertCustomer.Effective = "Y";
                                        oInsertCustomer.CreateUser = sUSERID;
                                        oInsertCustomer.ModifyUser = sUSERID;
                                        oInsertCustomer.CreateDate = dtNow;
                                        oInsertCustomer.ModifyDate = dtNow;

                                        OTB_CRM_CustomersMST oInsertCustomerMST = new OTB_CRM_CustomersMST();
                                        oInsertCustomerMST.OrgID = "TE";
                                        oInsertCustomerMST.guid = Guid.NewGuid().ToString();
                                        oInsertCustomerMST.customer_guid = strCustomerId;
                                        oInsertCustomerMST.CustomerNO = "";
                                        oInsertCustomerMST.Effective = "Y";
                                        oInsertCustomerMST.CreateUser = sUSERID;
                                        oInsertCustomerMST.ModifyUser = sUSERID;
                                        oInsertCustomerMST.CreateDate = dtNow;
                                        oInsertCustomerMST.ModifyDate = dtNow;

                                        iRel = db.Insertable(oInsertCustomer).ExecuteCommand();
                                        iRel = db.Insertable(oInsertCustomerMST).ExecuteCommand();

                                        OTB_OPM_ExhibitionCustomers oInsertExhibitionCustomers = new OTB_OPM_ExhibitionCustomers();
                                        oInsertExhibitionCustomers.ExhibitionNO = sSN;
                                        oInsertExhibitionCustomers.CustomerId = strCustomerId;
                                        oInsertExhibitionCustomers.SourceType = "2";
                                        oInsertExhibitionCustomers.ListSource = sListSource;
                                        oInsertExhibitionCustomers.BoothNumber = oImportCustomer.BoothNumber;
                                        oInsertExhibitionCustomers.NumberOfBooths = oImportCustomer.NumberOfBooths;
                                        oInsertExhibitionCustomers.CreateUser = sUSERID;
                                        oInsertExhibitionCustomers.ModifyUser = sUSERID;
                                        oInsertExhibitionCustomers.CreateDate = dtNow;
                                        oInsertExhibitionCustomers.ModifyDate = dtNow;

                                        //新增客戶關聯
                                        iRel = db.Insertable(oInsertExhibitionCustomers).ExecuteCommand();

                                        if (oImportCustomer.Contactor1 != "")
                                        {
                                            //新增暫存聯絡人1
                                            OTB_CRM_ContactorsTemp oInsertContactors1 = new OTB_CRM_ContactorsTemp();
                                            oInsertContactors1.guid = Guid.NewGuid().ToString();
                                            oInsertContactors1.CustomerId = strCustomerId;
                                            oInsertContactors1.ContactorName = oImportCustomer.Contactor1;
                                            oInsertContactors1.JobTitle = oImportCustomer.JobTitleC1;
                                            oInsertContactors1.Telephone1 = oImportCustomer.Telephone1C1;
                                            oInsertContactors1.Telephone2 = oImportCustomer.Telephone2C1;
                                            oInsertContactors1.Ext1 = oImportCustomer.Ext1C1;
                                            oInsertContactors1.Ext2 = oImportCustomer.Ext2C1;
                                            oInsertContactors1.Email1 = oImportCustomer.Email1C1;
                                            oInsertContactors1.Email2 = oImportCustomer.Email2C1;
                                            oInsertContactors1.CreateUser = sUSERID;
                                            oInsertContactors1.ModifyUser = sUSERID;
                                            oInsertContactors1.CreateDate = dtNow;
                                            oInsertContactors1.ModifyDate = dtNow;
                                            iRel = db.Insertable(oInsertContactors1).ExecuteCommand();

                                            //新增聯絡人1關聯
                                            OTB_OPM_ExhibitionContactors oInsertExhibitionContactors1 = new OTB_OPM_ExhibitionContactors();
                                            oInsertExhibitionContactors1.ExhibitionNO = sSN;
                                            oInsertExhibitionContactors1.CustomerId = strCustomerId;
                                            oInsertExhibitionContactors1.ContactorId = oInsertContactors1.guid;
                                            oInsertExhibitionContactors1.SourceType = "2";
                                            oInsertExhibitionContactors1.CreateUser = sUSERID;
                                            oInsertExhibitionContactors1.ModifyUser = sUSERID;
                                            oInsertExhibitionContactors1.CreateDate = dtNow;
                                            oInsertExhibitionContactors1.ModifyDate = dtNow;
                                            oInsertExhibitionContactors1.IsFormal = "N";
                                            iRel = db.Insertable(oInsertExhibitionContactors1).ExecuteCommand();
                                        }

                                        if (oImportCustomer.Contactor2 != "")
                                        {
                                            //新增暫存聯絡人2
                                            OTB_CRM_ContactorsTemp oInsertContactors2 = new OTB_CRM_ContactorsTemp();
                                            oInsertContactors2.guid = Guid.NewGuid().ToString();
                                            oInsertContactors2.CustomerId = strCustomerId;
                                            oInsertContactors2.ContactorName = oImportCustomer.Contactor2;
                                            oInsertContactors2.JobTitle = oImportCustomer.JobTitleC2;
                                            oInsertContactors2.Telephone1 = oImportCustomer.Telephone1C2;
                                            oInsertContactors2.Telephone2 = oImportCustomer.Telephone2C2;
                                            oInsertContactors2.Ext1 = oImportCustomer.Ext1C2;
                                            oInsertContactors2.Ext2 = oImportCustomer.Ext2C2;
                                            oInsertContactors2.Email1 = oImportCustomer.Email1C2;
                                            oInsertContactors2.Email2 = oImportCustomer.Email2C2;
                                            oInsertContactors2.CreateUser = sUSERID;
                                            oInsertContactors2.ModifyUser = sUSERID;
                                            oInsertContactors2.CreateDate = dtNow;
                                            oInsertContactors2.ModifyDate = dtNow;
                                            iRel = db.Insertable(oInsertContactors2).ExecuteCommand();

                                            //新增聯絡人2關聯
                                            OTB_OPM_ExhibitionContactors oInsertExhibitionContactors2 = new OTB_OPM_ExhibitionContactors();
                                            oInsertExhibitionContactors2.ExhibitionNO = sSN;
                                            oInsertExhibitionContactors2.CustomerId = strCustomerId;
                                            oInsertExhibitionContactors2.ContactorId = oInsertContactors2.guid;
                                            oInsertExhibitionContactors2.SourceType = "2";
                                            oInsertExhibitionContactors2.CreateUser = sUSERID;
                                            oInsertExhibitionContactors2.ModifyUser = sUSERID;
                                            oInsertExhibitionContactors2.CreateDate = dtNow;
                                            oInsertExhibitionContactors2.ModifyDate = dtNow;
                                            oInsertExhibitionContactors2.IsFormal = "N";
                                            iRel = db.Insertable(oInsertExhibitionContactors2).ExecuteCommand();
                                        }

                                        if (oImportCustomer.Contactor3 != "")
                                        {
                                            //新增暫存聯絡人3
                                            OTB_CRM_ContactorsTemp oInsertContactors3 = new OTB_CRM_ContactorsTemp();
                                            oInsertContactors3.guid = Guid.NewGuid().ToString();
                                            oInsertContactors3.CustomerId = strCustomerId;
                                            oInsertContactors3.ContactorName = oImportCustomer.Contactor3;
                                            oInsertContactors3.JobTitle = oImportCustomer.JobTitleC3;
                                            oInsertContactors3.Telephone1 = oImportCustomer.Telephone1C3;
                                            oInsertContactors3.Telephone2 = oImportCustomer.Telephone2C3;
                                            oInsertContactors3.Ext1 = oImportCustomer.Ext1C3;
                                            oInsertContactors3.Ext2 = oImportCustomer.Ext2C3;
                                            oInsertContactors3.Email1 = oImportCustomer.Email1C3;
                                            oInsertContactors3.Email2 = oImportCustomer.Email2C3;
                                            oInsertContactors3.CreateUser = sUSERID;
                                            oInsertContactors3.ModifyUser = sUSERID;
                                            oInsertContactors3.CreateDate = dtNow;
                                            oInsertContactors3.ModifyDate = dtNow;
                                            iRel = db.Insertable(oInsertContactors3).ExecuteCommand();

                                            //新增聯絡人3關聯
                                            OTB_OPM_ExhibitionContactors oInsertExhibitionContactors3 = new OTB_OPM_ExhibitionContactors();
                                            oInsertExhibitionContactors3.ExhibitionNO = sSN;
                                            oInsertExhibitionContactors3.CustomerId = strCustomerId;
                                            oInsertExhibitionContactors3.ContactorId = oInsertContactors3.guid;
                                            oInsertExhibitionContactors3.SourceType = "3";
                                            oInsertExhibitionContactors3.CreateUser = sUSERID;
                                            oInsertExhibitionContactors3.ModifyUser = sUSERID;
                                            oInsertExhibitionContactors3.CreateDate = dtNow;
                                            oInsertExhibitionContactors3.ModifyDate = dtNow;
                                            oInsertExhibitionContactors3.IsFormal = "N";
                                            iRel = db.Insertable(oInsertExhibitionContactors3).ExecuteCommand();
                                        }
                                    }
                                    else //名單中沒有但是既有客戶時
                                    {
                                        strCustomerId = listCheckCustomer[0].guid;

                                        OTB_OPM_ExhibitionCustomers oInsertExhibitionCustomers = new OTB_OPM_ExhibitionCustomers();
                                        oInsertExhibitionCustomers.ExhibitionNO = sSN;
                                        oInsertExhibitionCustomers.CustomerId = strCustomerId;
                                        oInsertExhibitionCustomers.SourceType = "2";
                                        oInsertExhibitionCustomers.ListSource = sListSource;
                                        oInsertExhibitionCustomers.BoothNumber = oImportCustomer.BoothNumber;
                                        oInsertExhibitionCustomers.NumberOfBooths = oImportCustomer.NumberOfBooths;
                                        oInsertExhibitionCustomers.CreateUser = sUSERID;
                                        oInsertExhibitionCustomers.ModifyUser = sUSERID;
                                        oInsertExhibitionCustomers.CreateDate = dtNow;
                                        oInsertExhibitionCustomers.ModifyDate = dtNow;

                                        //新增客戶關聯
                                        iRel = db.Insertable(oInsertExhibitionCustomers).ExecuteCommand();

                                        if (oImportCustomer.Contactor1 != "")
                                        {
                                            //新增暫存聯絡人1
                                            OTB_CRM_ContactorsTemp oInsertContactors1 = new OTB_CRM_ContactorsTemp();
                                            oInsertContactors1.guid = Guid.NewGuid().ToString();
                                            oInsertContactors1.CustomerId = strCustomerId;
                                            oInsertContactors1.ContactorName = oImportCustomer.Contactor1;
                                            oInsertContactors1.JobTitle = oImportCustomer.JobTitleC1;
                                            oInsertContactors1.Telephone1 = oImportCustomer.Telephone1C1;
                                            oInsertContactors1.Telephone2 = oImportCustomer.Telephone2C1;
                                            oInsertContactors1.Ext1 = oImportCustomer.Ext1C1;
                                            oInsertContactors1.Ext2 = oImportCustomer.Ext2C1;
                                            oInsertContactors1.Email1 = oImportCustomer.Email1C1;
                                            oInsertContactors1.Email2 = oImportCustomer.Email2C1;
                                            oInsertContactors1.CreateUser = sUSERID;
                                            oInsertContactors1.ModifyUser = sUSERID;
                                            oInsertContactors1.CreateDate = dtNow;
                                            oInsertContactors1.ModifyDate = dtNow;
                                            iRel = db.Insertable(oInsertContactors1).ExecuteCommand();

                                            //新增聯絡人1關聯
                                            OTB_OPM_ExhibitionContactors oInsertExhibitionContactors1 = new OTB_OPM_ExhibitionContactors();
                                            oInsertExhibitionContactors1.ExhibitionNO = sSN;
                                            oInsertExhibitionContactors1.CustomerId = strCustomerId;
                                            oInsertExhibitionContactors1.ContactorId = oInsertContactors1.guid;
                                            oInsertExhibitionContactors1.SourceType = "2";
                                            oInsertExhibitionContactors1.CreateUser = sUSERID;
                                            oInsertExhibitionContactors1.ModifyUser = sUSERID;
                                            oInsertExhibitionContactors1.CreateDate = dtNow;
                                            oInsertExhibitionContactors1.ModifyDate = dtNow;
                                            oInsertExhibitionContactors1.IsFormal = "N";
                                            iRel = db.Insertable(oInsertExhibitionContactors1).ExecuteCommand();
                                        }

                                        if (oImportCustomer.Contactor2 != "")
                                        {
                                            //新增暫存聯絡人2
                                            OTB_CRM_ContactorsTemp oInsertContactors2 = new OTB_CRM_ContactorsTemp();
                                            oInsertContactors2.guid = Guid.NewGuid().ToString();
                                            oInsertContactors2.CustomerId = strCustomerId;
                                            oInsertContactors2.ContactorName = oImportCustomer.Contactor2;
                                            oInsertContactors2.JobTitle = oImportCustomer.JobTitleC2;
                                            oInsertContactors2.Telephone1 = oImportCustomer.Telephone1C2;
                                            oInsertContactors2.Telephone2 = oImportCustomer.Telephone2C2;
                                            oInsertContactors2.Ext1 = oImportCustomer.Ext1C2;
                                            oInsertContactors2.Ext2 = oImportCustomer.Ext2C2;
                                            oInsertContactors2.Email1 = oImportCustomer.Email1C2;
                                            oInsertContactors2.Email2 = oImportCustomer.Email2C2;
                                            oInsertContactors2.CreateUser = sUSERID;
                                            oInsertContactors2.ModifyUser = sUSERID;
                                            oInsertContactors2.CreateDate = dtNow;
                                            oInsertContactors2.ModifyDate = dtNow;
                                            iRel = db.Insertable(oInsertContactors2).ExecuteCommand();

                                            //新增聯絡人2關聯
                                            OTB_OPM_ExhibitionContactors oInsertExhibitionContactors2 = new OTB_OPM_ExhibitionContactors();
                                            oInsertExhibitionContactors2.ExhibitionNO = sSN;
                                            oInsertExhibitionContactors2.CustomerId = strCustomerId;
                                            oInsertExhibitionContactors2.ContactorId = oInsertContactors2.guid;
                                            oInsertExhibitionContactors2.SourceType = "2";
                                            oInsertExhibitionContactors2.CreateUser = sUSERID;
                                            oInsertExhibitionContactors2.ModifyUser = sUSERID;
                                            oInsertExhibitionContactors2.CreateDate = dtNow;
                                            oInsertExhibitionContactors2.ModifyDate = dtNow;
                                            oInsertExhibitionContactors2.IsFormal = "N";
                                            iRel = db.Insertable(oInsertExhibitionContactors2).ExecuteCommand();
                                        }

                                        if (oImportCustomer.Contactor3 != "")
                                        {
                                            //新增暫存聯絡人3
                                            OTB_CRM_ContactorsTemp oInsertContactors3 = new OTB_CRM_ContactorsTemp();
                                            oInsertContactors3.guid = Guid.NewGuid().ToString();
                                            oInsertContactors3.CustomerId = strCustomerId;
                                            oInsertContactors3.ContactorName = oImportCustomer.Contactor3;
                                            oInsertContactors3.JobTitle = oImportCustomer.JobTitleC3;
                                            oInsertContactors3.Telephone1 = oImportCustomer.Telephone1C3;
                                            oInsertContactors3.Telephone2 = oImportCustomer.Telephone2C3;
                                            oInsertContactors3.Ext1 = oImportCustomer.Ext1C3;
                                            oInsertContactors3.Ext2 = oImportCustomer.Ext2C3;
                                            oInsertContactors3.Email1 = oImportCustomer.Email1C3;
                                            oInsertContactors3.Email2 = oImportCustomer.Email2C3;
                                            oInsertContactors3.CreateUser = sUSERID;
                                            oInsertContactors3.ModifyUser = sUSERID;
                                            oInsertContactors3.CreateDate = dtNow;
                                            oInsertContactors3.ModifyDate = dtNow;
                                            iRel = db.Insertable(oInsertContactors3).ExecuteCommand();

                                            //新增聯絡人3關聯
                                            OTB_OPM_ExhibitionContactors oInsertExhibitionContactors3 = new OTB_OPM_ExhibitionContactors();
                                            oInsertExhibitionContactors3.ExhibitionNO = sSN;
                                            oInsertExhibitionContactors3.CustomerId = strCustomerId;
                                            oInsertExhibitionContactors3.ContactorId = oInsertContactors3.guid;
                                            oInsertExhibitionContactors3.SourceType = "2";
                                            oInsertExhibitionContactors3.CreateUser = sUSERID;
                                            oInsertExhibitionContactors3.ModifyUser = sUSERID;
                                            oInsertExhibitionContactors3.CreateDate = dtNow;
                                            oInsertExhibitionContactors3.ModifyDate = dtNow;
                                            oInsertExhibitionContactors3.IsFormal = "N";
                                            iRel = db.Insertable(oInsertExhibitionContactors3).ExecuteCommand();
                                        }
                                    }
                                }
                                else //名單中已有同名或同統編之客戶
                                {
                                    strCustomerId = listSameCustomer[0].CustomerId;
                                    
                                    //更新客戶
                                    if (listSameCustomer[0].IsAudit != "Y" && listSameCustomer[0].IsAudit != "A" && listSameCustomer[0].IsAudit != "P" && listSameCustomer[0].IsAudit != "Z")
                                    {
                                        OTB_CRM_Customers oUpdateCustomer = new OTB_CRM_Customers
                                        {
                                            CustomerCName = oImportCustomer.CustomerCName,
                                            UniCode = oImportCustomer.UniCode,
                                            Telephone = oImportCustomer.Telephone,
                                            FAX = oImportCustomer.FAX,
                                            Address = oImportCustomer.Address,
                                            WebsiteAdress = oImportCustomer.WebsiteAdress,
                                            ModifyUser = sUSERID,
                                            ModifyDate = dtNow
                                        };
                                        iRel = db.Updateable(oUpdateCustomer).UpdateColumns(it => new { it.CustomerCName, it.UniCode, it.Telephone, it.FAX, it.Address, it.WebsiteAdress, it.ModifyUser, it.ModifyDate })
                                            .Where(x => x.guid == listSameCustomer[0].CustomerId).ExecuteCommand();
                                    }

                                    //更新客戶關聯
                                    OTB_OPM_ExhibitionCustomers oUpdateExhibitionCustomer = new OTB_OPM_ExhibitionCustomers
                                    {
                                        ListSource = sListSource,
                                        BoothNumber = oImportCustomer.BoothNumber,
                                        NumberOfBooths = oImportCustomer.NumberOfBooths,
                                        ModifyUser = sUSERID,
                                        ModifyDate = dtNow
                                    };
                                    iRel = db.Updateable(oUpdateExhibitionCustomer).UpdateColumns(it => new { it.ListSource, it.BoothNumber, it.NumberOfBooths, it.ModifyUser, it.ModifyDate })
                                        .Where(x => x.ExhibitionNO == sSN && x.CustomerId == listSameCustomer[0].CustomerId).ExecuteCommand();

                                    if (oImportCustomer.Contactor1 != "")
                                    {
                                        //檢查重複聯絡人1
                                        var listCheckContactor1 = listContactorsInFormal.Where(x => x.CustomerId == strCustomerId && x.ContactorName == oImportCustomer.Contactor1).ToList();
                                        if (listCheckContactor1.Count == 0)
                                        {
                                            //無重複，新增聯絡人1
                                            OTB_CRM_ContactorsTemp oInsertContactors1 = new OTB_CRM_ContactorsTemp();
                                            oInsertContactors1.guid = Guid.NewGuid().ToString();
                                            oInsertContactors1.CustomerId = strCustomerId;
                                            oInsertContactors1.ContactorName = oImportCustomer.Contactor1;
                                            oInsertContactors1.JobTitle = oImportCustomer.JobTitleC1;
                                            oInsertContactors1.Telephone1 = oImportCustomer.Telephone1C1;
                                            oInsertContactors1.Telephone2 = oImportCustomer.Telephone2C1;
                                            oInsertContactors1.Ext1 = oImportCustomer.Ext1C1;
                                            oInsertContactors1.Ext2 = oImportCustomer.Ext2C1;
                                            oInsertContactors1.Email1 = oImportCustomer.Email1C1;
                                            oInsertContactors1.Email2 = oImportCustomer.Email2C1;
                                            oInsertContactors1.CreateUser = sUSERID;
                                            oInsertContactors1.ModifyUser = sUSERID;
                                            oInsertContactors1.CreateDate = dtNow;
                                            oInsertContactors1.ModifyDate = dtNow;
                                            iRel = db.Insertable(oInsertContactors1).ExecuteCommand();

                                            //新增聯絡人1關聯
                                            OTB_OPM_ExhibitionContactors oInsertExhibitionContactors1 = new OTB_OPM_ExhibitionContactors();
                                            oInsertExhibitionContactors1.ExhibitionNO = sSN;
                                            oInsertExhibitionContactors1.CustomerId = strCustomerId;
                                            oInsertExhibitionContactors1.ContactorId = oInsertContactors1.guid;
                                            oInsertExhibitionContactors1.SourceType = "2";
                                            oInsertExhibitionContactors1.CreateUser = sUSERID;
                                            oInsertExhibitionContactors1.ModifyUser = sUSERID;
                                            oInsertExhibitionContactors1.CreateDate = dtNow;
                                            oInsertExhibitionContactors1.ModifyDate = dtNow;
                                            oInsertExhibitionContactors1.IsFormal = "N";
                                            iRel = db.Insertable(oInsertExhibitionContactors1).ExecuteCommand();
                                        }
                                        else
                                        {
                                            //更新聯絡人1
                                            OTB_CRM_ContactorsTemp oUpdateContactor = new OTB_CRM_ContactorsTemp
                                            {
                                                JobTitle = oImportCustomer.JobTitleC1,
                                                Telephone1 = oImportCustomer.Telephone1C1,
                                                Telephone2 = oImportCustomer.Telephone2C1,
                                                Ext1 = oImportCustomer.Ext1C1,
                                                Ext2 = oImportCustomer.Ext2C1,
                                                Email1 = oImportCustomer.Email1C1,
                                                Email2 = oImportCustomer.Email2C1,
                                                ModifyUser = sUSERID,
                                                ModifyDate = dtNow
                                            };

                                            iRel = db.Updateable(oUpdateContactor).UpdateColumns(it => new { it.JobTitle, it.Telephone1, it.Telephone2, it.Ext1, it.Ext2, it.Email1, it.ModifyUser, it.ModifyDate })
                                                .Where(x => x.guid == listCheckContactor1[0].ContactorId).ExecuteCommand();
                                        }
                                    }

                                    if (oImportCustomer.Contactor2 != "")
                                    {
                                        //檢查重複聯絡人2
                                        var listCheckContactor2 = listContactorsInFormal.Where(x => x.CustomerId == strCustomerId && x.ContactorName == oImportCustomer.Contactor2).ToList();
                                        if (listCheckContactor2.Count == 0)
                                        {
                                            //無重複，新增聯絡人2
                                            OTB_CRM_ContactorsTemp oInsertContactors2 = new OTB_CRM_ContactorsTemp();
                                            oInsertContactors2.guid = Guid.NewGuid().ToString();
                                            oInsertContactors2.CustomerId = strCustomerId;
                                            oInsertContactors2.ContactorName = oImportCustomer.Contactor2;
                                            oInsertContactors2.JobTitle = oImportCustomer.JobTitleC2;
                                            oInsertContactors2.Telephone1 = oImportCustomer.Telephone1C2;
                                            oInsertContactors2.Telephone2 = oImportCustomer.Telephone2C2;
                                            oInsertContactors2.Ext1 = oImportCustomer.Ext1C2;
                                            oInsertContactors2.Ext2 = oImportCustomer.Ext2C2;
                                            oInsertContactors2.Email1 = oImportCustomer.Email1C2;
                                            oInsertContactors2.Email2 = oImportCustomer.Email2C2;
                                            oInsertContactors2.CreateUser = sUSERID;
                                            oInsertContactors2.ModifyUser = sUSERID;
                                            oInsertContactors2.CreateDate = dtNow;
                                            oInsertContactors2.ModifyDate = dtNow;
                                            iRel = db.Insertable(oInsertContactors2).ExecuteCommand();

                                            //新增聯絡人2關聯
                                            OTB_OPM_ExhibitionContactors oInsertExhibitionContactors2 = new OTB_OPM_ExhibitionContactors();
                                            oInsertExhibitionContactors2.ExhibitionNO = sSN;
                                            oInsertExhibitionContactors2.CustomerId = strCustomerId;
                                            oInsertExhibitionContactors2.ContactorId = oInsertContactors2.guid;
                                            oInsertExhibitionContactors2.SourceType = "2";
                                            oInsertExhibitionContactors2.CreateUser = sUSERID;
                                            oInsertExhibitionContactors2.ModifyUser = sUSERID;
                                            oInsertExhibitionContactors2.CreateDate = dtNow;
                                            oInsertExhibitionContactors2.ModifyDate = dtNow;
                                            oInsertExhibitionContactors2.IsFormal = "N";
                                            iRel = db.Insertable(oInsertExhibitionContactors2).ExecuteCommand();
                                        }
                                        else
                                        {
                                            //更新聯絡人2
                                            OTB_CRM_ContactorsTemp oUpdateContactor = new OTB_CRM_ContactorsTemp
                                            {
                                                JobTitle = oImportCustomer.JobTitleC2,
                                                Telephone1 = oImportCustomer.Telephone1C2,
                                                Telephone2 = oImportCustomer.Telephone2C2,
                                                Ext1 = oImportCustomer.Ext1C2,
                                                Ext2 = oImportCustomer.Ext2C2,
                                                Email1 = oImportCustomer.Email1C2,
                                                Email2 = oImportCustomer.Email2C2,
                                                ModifyUser = sUSERID,
                                                ModifyDate = dtNow
                                            };

                                            iRel = db.Updateable(oUpdateContactor).UpdateColumns(it => new { it.JobTitle, it.Telephone1, it.Telephone2, it.Ext1, it.Ext2, it.Email1, it.ModifyUser, it.ModifyDate })
                                                .Where(x => x.guid == listCheckContactor2[0].ContactorId).ExecuteCommand();
                                        }
                                    }

                                    if (oImportCustomer.Contactor3 != "")
                                    {
                                        //檢查重複聯絡人3
                                        var listCheckContactor3 = listContactorsInFormal.Where(x => x.CustomerId == strCustomerId && x.ContactorName == oImportCustomer.Contactor3).ToList();
                                        if (listCheckContactor3.Count == 0)
                                        {
                                            //無重複，新增聯絡人3
                                            OTB_CRM_ContactorsTemp oInsertContactors3 = new OTB_CRM_ContactorsTemp();
                                            oInsertContactors3.guid = Guid.NewGuid().ToString();
                                            oInsertContactors3.CustomerId = strCustomerId;
                                            oInsertContactors3.ContactorName = oImportCustomer.Contactor3;
                                            oInsertContactors3.JobTitle = oImportCustomer.JobTitleC3;
                                            oInsertContactors3.Telephone1 = oImportCustomer.Telephone1C3;
                                            oInsertContactors3.Telephone2 = oImportCustomer.Telephone2C3;
                                            oInsertContactors3.Ext1 = oImportCustomer.Ext1C3;
                                            oInsertContactors3.Ext2 = oImportCustomer.Ext2C3;
                                            oInsertContactors3.Email1 = oImportCustomer.Email1C3;
                                            oInsertContactors3.Email2 = oImportCustomer.Email2C3;
                                            oInsertContactors3.CreateUser = sUSERID;
                                            oInsertContactors3.ModifyUser = sUSERID;
                                            oInsertContactors3.CreateDate = dtNow;
                                            oInsertContactors3.ModifyDate = dtNow;
                                            iRel = db.Insertable(oInsertContactors3).ExecuteCommand();

                                            //新增聯絡人3關聯
                                            OTB_OPM_ExhibitionContactors oInsertExhibitionContactors3 = new OTB_OPM_ExhibitionContactors();
                                            oInsertExhibitionContactors3.ExhibitionNO = sSN;
                                            oInsertExhibitionContactors3.CustomerId = strCustomerId;
                                            oInsertExhibitionContactors3.ContactorId = oInsertContactors3.guid;
                                            oInsertExhibitionContactors3.SourceType = "2";
                                            oInsertExhibitionContactors3.CreateUser = sUSERID;
                                            oInsertExhibitionContactors3.ModifyUser = sUSERID;
                                            oInsertExhibitionContactors3.CreateDate = dtNow;
                                            oInsertExhibitionContactors3.ModifyDate = dtNow;
                                            oInsertExhibitionContactors3.IsFormal = "N";
                                            iRel = db.Insertable(oInsertExhibitionContactors3).ExecuteCommand();
                                        }
                                        else
                                        {
                                            //更新聯絡人3
                                            OTB_CRM_ContactorsTemp oUpdateContactor = new OTB_CRM_ContactorsTemp
                                            {
                                                JobTitle = oImportCustomer.JobTitleC3,
                                                Telephone1 = oImportCustomer.Telephone1C3,
                                                Telephone2 = oImportCustomer.Telephone2C3,
                                                Ext1 = oImportCustomer.Ext1C3,
                                                Ext2 = oImportCustomer.Ext2C3,
                                                Email1 = oImportCustomer.Email1C3,
                                                Email2 = oImportCustomer.Email2C3,
                                                ModifyUser = sUSERID,
                                                ModifyDate = dtNow
                                            };

                                            iRel = db.Updateable(oUpdateContactor).UpdateColumns(it => new { it.JobTitle, it.Telephone1, it.Telephone2, it.Ext1, it.Ext2, it.Email1, it.ModifyUser, it.ModifyDate })
                                                .Where(x => x.guid == listCheckContactor3[0].ContactorId).ExecuteCommand();
                                        }
                                    }
                                }
                            }
                            break;
                    }

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iRel);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = strCustomerCName + ";" + strCustomerEName + ";" + strUnicode + ";" + Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Exhibition_UpdService), @"展覽管理", @"AddUpdateListFile（新增更新名單 檔案匯入）", @"", @"", @"");
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

        #endregion 新增更新名單 檔案匯入
        
        #region 獲取由名單過來的參展廠商資料

        /// <summary>
        /// 獲取由名單過來的參展廠商資料
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetNewCustomers(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    string sSN = _fetchString(i_crm, @"SN");
                    string sId = _fetchString(i_crm, @"Guid");

                    string[] arrCustomers = sId.TrimEnd(',').Split(',');

                    List<OTB_CRM_Customers> listCustomer = db.Queryable<OTB_CRM_Customers>().Where(x => SqlFunc.ContainsArray(arrCustomers, x.guid)).ToList();

                    var listContactorFormal = db.Queryable<OTB_OPM_ExhibitionContactors, OTB_CRM_Contactors>((t1, t2) => new object[] {
                            JoinType.Inner,t1.ContactorId==t2.guid})
                            .Select((t1, t2) => new { t1.ExhibitionNO, t1.CustomerId, t1.ContactorId, t2.ContactorName, t2.Telephone1, t2.Email1, t1.IsMain, t2.OrderByValue }).MergeTable()
                            .Where(t1 => t1.ExhibitionNO == sSN && SqlFunc.ContainsArray(arrCustomers, t1.CustomerId)).OrderBy("OrderByValue").ToList();

                    int i = 1;
                    List<View_OPM_ExhibitionCustomers> listEntity = new List<View_OPM_ExhibitionCustomers>();
                    foreach (var oCustomer in listCustomer)
                    {
                        View_OPM_ExhibitionCustomers oEntity = new View_OPM_ExhibitionCustomers();

                        oEntity.RowIndex = i;
                        oEntity.guid = oCustomer.guid;
                        oEntity.UniCode = oCustomer.UniCode;
                        oEntity.CustomerNO = oCustomer.CustomerNO;
                        oEntity.CustomerCName = oCustomer.CustomerCName;
                        oEntity.CustomerEName = oCustomer.CustomerEName;
                        oEntity.Telephone = oCustomer.Telephone;
                        oEntity.FAX = oCustomer.FAX;
                        oEntity.Email = oCustomer.Email;
                        oEntity.Address = oCustomer.Address;
                        oEntity.ModifyDate = oCustomer.ModifyDate;

                        var listCustomerContactor = listContactorFormal.Where(t => t.CustomerId == oCustomer.guid).ToList();

                        if (listCustomerContactor.Count > 0)
                        {
                            var listMain = listCustomerContactor.Where(t => t.IsMain == "Y").ToList();
                            if (listMain.Count == 0)
                            {
                                listMain = listCustomerContactor;
                            }
                            oEntity.ContactorId = listMain[0].ContactorId;
                            oEntity.ContactorName = listMain[0].ContactorName;
                            oEntity.Telephone = listMain[0].Telephone1;
                            oEntity.Email = listMain[0].Email1;
                        }

                        listEntity.Add(oEntity);
                        i++;
                    }

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, listEntity);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBAPP.OPM.ImportExhibition_QryService", "", "GetNewCustomers（獲取由名單過來的參展廠商資料）", "", "", "");
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

        #endregion 獲取由名單過來的參展廠商資料

        #region 刪除展覽

        /// <summary>
        /// 刪除展覽
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
                        int iRel = 0;
                        var sSN = _fetchString(i_crm, @"SN");

                        List<OTB_OPM_ExhibitionCustomers> listExhibitionCustomers = db.Queryable<OTB_OPM_ExhibitionCustomers>().Where(x => x.ExhibitionNO == sSN).ToList();

                        //刪除Callout紀錄
                        iRel = db.Deleteable<OTB_CRM_Callout>().Where(x => x.ExhibitionNO == sSN).ExecuteCommand();

                        //刪除展覽聯絡人關聯
                        iRel = db.Deleteable<OTB_OPM_ExhibitionContactors>().Where(x => x.ExhibitionNO == sSN).ExecuteCommand();

                        //逐筆刪除展覽聯絡人關聯
                        foreach (OTB_OPM_ExhibitionCustomers oExhibitionCustomer in listExhibitionCustomers)
                        {
                            iRel = db.Deleteable<OTB_OPM_ExhibitionCustomers>().Where(x => x.ExhibitionNO == sSN && x.CustomerId == oExhibitionCustomer.CustomerId).ExecuteCommand();
                        }

                        iRel = db.Deleteable<OTB_OPM_Exhibition>().Where(x => x.SN.ToString() == sSN).ExecuteCommand();
                        
                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, iRel);
                    } while (false);

                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Exhibition_UpdService), @"展覽管理", @"Delete（刪除展覽）", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }

                Logger.Debug(@"Exhibition_UpdService.Delete Debug（Param：" + JsonToString(i_crm) + @"；Response：" + JsonToString(rm) + @"）------------------");
            }
            return rm;
        }

        #endregion 刪除展覽

    }
}