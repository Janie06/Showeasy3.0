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
                          .Where(x => x.OrgID == i_crm.ORIGID && x.Effective == @"Y")
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
                              x.ExhibitionAddress
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

                    var saExhibitions = db.Queryable<OTB_OPM_Exhibition>().Where(x => x.OrgID == i_crm.ORIGID && x.Effective == @"Y" && x.SN != SN).ToList();
                    var RepeatShotName = saExhibitions.Any(x => x.ExhibitioShotName_TW.Trim() == sExhibitioShotName_TW || x.ExhibitioShotName_TW.Trim() == sExhibitioShotName_TW_Alt);
                    var RepeatNameTW = saExhibitions.Any(x => x.Exhibitioname_TW.Trim() == sExhibitioname_TW || x.Exhibitioname_TW.Trim() == sExhibitioname_TW_Alt);
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
    }
}