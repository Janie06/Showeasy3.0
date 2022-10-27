using EasyBL.WebApi.Message;
using Entity;
using Entity.Sugar;
using Entity.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SqlSugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;
using System.Data;

namespace EasyBL.WEBAPP.WSM
{
    public class PackingOrder_QryService : ServiceBase
    {
        #region 預約記錄分頁查詢

        /// <summary>
        /// 預約記錄分頁查詢
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

                    var sMuseumMumber = _fetchString(i_crm, @"MuseumMumber");
                    var sSearchWords = _fetchString(i_crm, @"SearchWords");
                    var sAppointDateTimeStart = _fetchString(i_crm, @"AppointDateTimeStart");
                    var sAppointDateTimeEnd = _fetchString(i_crm, @"AppointDateTimeEnd");
                    var sApproachTime = _fetchString(i_crm, @"ApproachTime");
                    var sExitTime = _fetchString(i_crm, @"ExitTime");
                    var bExcel = _fetchBool(i_crm, @"Excel");

                    var rAppointDateTimeStart = new DateTime();
                    var rAppointDateTimeEnd = new DateTime();
                    var rApproachTime = new DateTime();
                    var rExitTime = new DateTime();
                    if (!string.IsNullOrEmpty(sAppointDateTimeStart))
                    {
                        rAppointDateTimeStart = SqlFunc.ToDate(sAppointDateTimeStart);
                    }
                    if (!string.IsNullOrEmpty(sAppointDateTimeEnd))
                    {
                        rAppointDateTimeEnd = SqlFunc.ToDate(sAppointDateTimeEnd).AddDays(1);
                    }
                    if (!string.IsNullOrEmpty(sApproachTime))
                    {
                        rApproachTime = SqlFunc.ToDate(sApproachTime);
                    }
                    if (!string.IsNullOrEmpty(sExitTime))
                    {
                        rExitTime = SqlFunc.ToDate(sExitTime).AddDays(1);
                    }

                    if (i_crm.ORIGID == "TE")
                    {
                        pml.DataList = db.Queryable<OTB_WSM_PackingOrder, OTB_OPM_Exhibition, OTB_CRM_Customers>
                        ((t1, t2, t3) =>
                        new object[] {
                                JoinType.Left, t1.ExhibitionNO == t2.SN.ToString(),
                                JoinType.Left, t1.CustomerId == t3.guid
                              }
                        )
                        .Where((t1, t2, t3) => t1.OrgID == i_crm.ORIGID && t1.MuseumMumber.Contains(sMuseumMumber))
                        .Where((t1, t2, t3) => t1.CompName.Contains(sSearchWords) || t1.AppointNO.Contains(sSearchWords) || t1.AppointUser.Contains(sSearchWords) || t1.AppointTel.Contains(sSearchWords) || t1.AppointEmail.Contains(sSearchWords) || t1.Contactor.Contains(sSearchWords) || t1.ContactTel.Contains(sSearchWords) || t1.PackingInfo.Contains(sSearchWords) || t2.Exhibitioname_TW.Contains(sSearchWords) || t2.Exhibitioname_EN.Contains(sSearchWords))
                        .WhereIF(!string.IsNullOrEmpty(sAppointDateTimeStart), (t1, t2, t3) => t1.CreateDate >= rAppointDateTimeStart.Date)
                        .WhereIF(!string.IsNullOrEmpty(sAppointDateTimeEnd), (t1, t2, t3) => t1.CreateDate <= rAppointDateTimeEnd.Date)
                        .WhereIF(!string.IsNullOrEmpty(sApproachTime), (t1, t2, t3) => t1.ExitTime >= rApproachTime.Date)
                        .WhereIF(!string.IsNullOrEmpty(sExitTime), (t1, t2, t3) => t1.ApproachTime <= rExitTime.Date)
                        .Select((t1, t2, t3) => new View_WSM_PackingOrder
                        {
                            AppointNO = t1.AppointNO,
                            OrgID = t1.OrgID,
                            ExhibitionNO = t1.ExhibitionNO,
                            CompName = t1.CompName,
                            Unicode = t1.Unicode,
                            MuseumMumber = t1.MuseumMumber,
                            AppointUser = t1.AppointUser,
                            AppointTel = t1.AppointTel + "(" + t1.AppointExt + ")",
                            AppointEmail = t1.AppointEmail,
                            Contactor = t1.Contactor,
                            ContactTel = t1.ContactTel,
                            ApproachTime = t1.ApproachTime,
                            ExitTime = t1.ExitTime,
                            PackingInfo = t1.PackingInfo,
                            Total = t1.Total,
                            Memo = t1.Memo,
                            PaymentWay = t1.PaymentWay,
                            CreateUser = t1.CreateUser,
                            CreateDate = t1.CreateDate,
                            CustomerId = t1.CustomerId,
                            OtherId = t1.OtherId,
                            AppointDateTime = t1.AppointDateTime,
                            IsKeyMode = t1.IsKeyMode,
                            OtherIdFrom = t1.OtherIdFrom,
                            Exhibitioname_TW = t2.Exhibitioname_TW,
                            Exhibitioname_EN = t2.Exhibitioname_EN,
                            IsFormal = SqlFunc.IIF(t3.IsAudit == "N", false, true)
                        })
                        .MergeTable()
                        .OrderBy(sSortField, sSortOrder)
                        .ToPageList(pml.PageIndex, pml.PageSize, ref iPageCount);
                        pml.Total = iPageCount;
                    } else
                    {
                        pml.DataList = db.Queryable<OTB_WSM_PackingOrder, OTB_OPM_Exhibition, OTB_CRM_ImportCustomers>
                        ((t1, t2, t3) =>
                        new object[] {
                                JoinType.Left, t1.OrgID == t2.OrgID && t1.ExhibitionNO == t2.SN.ToString(),
                                JoinType.Left, t1.OrgID == t3.OrgID && t1.ExhibitionNO == t3.ExhibitionNO.ToString() && t1.CustomerId == t3.guid
                              }
                        )
                        .Where((t1, t2, t3) => t1.OrgID == i_crm.ORIGID && t1.MuseumMumber.Contains(sMuseumMumber))
                        .Where((t1, t2, t3) => t1.CompName.Contains(sSearchWords) || t1.AppointNO.Contains(sSearchWords) || t1.AppointUser.Contains(sSearchWords) || t1.AppointTel.Contains(sSearchWords) || t1.AppointEmail.Contains(sSearchWords) || t1.Contactor.Contains(sSearchWords) || t1.ContactTel.Contains(sSearchWords) || t1.PackingInfo.Contains(sSearchWords) || t2.Exhibitioname_TW.Contains(sSearchWords) || t2.Exhibitioname_EN.Contains(sSearchWords))
                        .WhereIF(!string.IsNullOrEmpty(sAppointDateTimeStart), (t1, t2, t3) => t1.CreateDate >= rAppointDateTimeStart.Date)
                        .WhereIF(!string.IsNullOrEmpty(sAppointDateTimeEnd), (t1, t2, t3) => t1.CreateDate <= rAppointDateTimeEnd.Date)
                        .WhereIF(!string.IsNullOrEmpty(sApproachTime), (t1, t2, t3) => t1.ExitTime >= rApproachTime.Date)
                        .WhereIF(!string.IsNullOrEmpty(sExitTime), (t1, t2, t3) => t1.ApproachTime <= rExitTime.Date)
                        .Select((t1, t2, t3) => new View_WSM_PackingOrder
                        {
                            AppointNO = t1.AppointNO,
                            OrgID = t1.OrgID,
                            ExhibitionNO = t1.ExhibitionNO,
                            CompName = t1.CompName,
                            MuseumMumber = t1.MuseumMumber,
                            AppointUser = t1.AppointUser,
                            AppointTel = t1.AppointTel,
                            AppointEmail = t1.AppointEmail,
                            Contactor = t1.Contactor,
                            ContactTel = t1.ContactTel,
                            ApproachTime = t1.ApproachTime,
                            ExitTime = t1.ExitTime,
                            PackingInfo = t1.PackingInfo,
                            Total = t1.Total,
                            Memo = t1.Memo,
                            CreateUser = t1.CreateUser,
                            CreateDate = t1.CreateDate,
                            CustomerId = t1.CustomerId,
                            OtherId = t1.OtherId,
                            AppointDateTime = t1.AppointDateTime,
                            IsKeyMode = t1.IsKeyMode,
                            OtherIdFrom = t1.OtherIdFrom,
                            Exhibitioname_TW = t2.Exhibitioname_TW,
                            Exhibitioname_EN = t2.Exhibitioname_EN,
                            IsFormal = SqlFunc.IIF(t3.IsFormal == null, false, t3.IsFormal)
                        })
                        .MergeTable()
                        .OrderBy(sSortField, sSortOrder)
                        .ToPageList(pml.PageIndex, pml.PageSize, ref iPageCount);
                        pml.Total = iPageCount;
                    }
                    
                    rm = new SuccessResponseMessage(null, i_crm);
                    if (bExcel)
                    {
                        string sFileName = "奕達預約記錄";
                        if (i_crm.ORIGID == "TG")
                        {
                            sFileName = "駒驛預約記錄";
                        }
                        var oHeader = new Dictionary<string, string>
                        {
                            { "Exhibitioname_TW", "展覽名稱" },
                            { "MuseumMumber", "攤位編號" },
                            { "CompName", "公司名稱" },
                            { "Unicode", "統一編號" },
                            { "Field0", "攤位數" },
                            { "Contactor", "聯絡人1" },
                            { "ContactTel", "電話" },
                            { "Field1", "進場日期" },
                            { "Field2", "進場時間" },
                            { "Field3", "退場日期" },
                            { "Field4", "退場日期" },
                            { "Field5", "件數" },
                            { "Field6", "總重" },
                            { "Field7", "長" },
                            { "Field8", "寬" },
                            { "Field9", "高" },
                            { "Field10", "CBM" },
                            { "Field11", "包裝" },
                            { "Field12", "備註" },
                        };
                        var dt_new = new DataTable();
                        dt_new.Columns.Add("Exhibitioname_TW");
                        dt_new.Columns.Add("MuseumMumber");
                        dt_new.Columns.Add("CompName");
                        dt_new.Columns.Add("Unicode");
                        dt_new.Columns.Add("Field0");
                        dt_new.Columns.Add("Contactor");
                        dt_new.Columns.Add("ContactTel");
                        dt_new.Columns.Add("Field1");
                        dt_new.Columns.Add("Field2");
                        dt_new.Columns.Add("Field3");
                        dt_new.Columns.Add("Field4");
                        dt_new.Columns.Add("Field5");
                        dt_new.Columns.Add("Field6");
                        dt_new.Columns.Add("Field7");
                        dt_new.Columns.Add("Field8");
                        dt_new.Columns.Add("Field9");
                        dt_new.Columns.Add("Field10");
                        dt_new.Columns.Add("Field11");
                        dt_new.Columns.Add("Field12");

                        var listMerge = new List<Dictionary<string, int>>();
                        var oExpoType_TW = new Map { { "01", "裸機" }, { "02", "木箱" }, { "03", "散貨" }, { "04", "打板" }, { "05", "其他" } };
                        var iPackingChildIndex = 0;
                        var saPackingOrder = pml.DataList as List<View_WSM_PackingOrder>;
                        foreach (var item in saPackingOrder)
                        {
                            var iPackingIndex = iPackingChildIndex;
                            var jaPackingInfo = (JArray)JsonConvert.DeserializeObject(item.PackingInfo);
                            if (jaPackingInfo.Count > 0)
                            {
                                foreach (JObject packinfo in jaPackingInfo)
                                {
                                    var row_new = dt_new.NewRow();
                                    var sExpoType = packinfo["ExpoType"].ToString();
                                    var sExpoLen = packinfo["ExpoLen"].ToString();
                                    var sExpoWidth = packinfo["ExpoWidth"].ToString();
                                    var sExpoHeight = packinfo["ExpoHeight"].ToString();
                                    var sExpoWeight = packinfo["ExpoWeight"].ToString();
                                    var sExpoNumber = packinfo["ExpoNumber"].ToString();

                                    row_new["Exhibitioname_TW"] = item.Exhibitioname_TW;
                                    row_new["CompName"] = item.CompName;
                                    row_new["Unicode"] = item.Unicode;
                                    row_new["ContactTel"] = item.ContactTel;
                                    row_new["Field12"] = item.Memo;
                                    row_new["MuseumMumber"] = item.MuseumMumber;
                                    row_new["Field0"] = "";
                                    row_new["Contactor"] = item.Contactor;
                                    row_new["Field1"] = Convert.ToDateTime(item.ApproachTime).ToString("yyyy/MM/dd");
                                    row_new["Field2"] = Convert.ToDateTime(item.ApproachTime).ToString("HH:mm");
                                    row_new["Field3"] = Convert.ToDateTime(item.ExitTime).ToString("yyyy/MM/dd");
                                    row_new["Field4"] = Convert.ToDateTime(item.ExitTime).ToString("HH:mm");
                                    row_new["Field5"] = sExpoNumber;
                                    row_new["Field6"] = sExpoWeight;
                                    row_new["Field7"] = sExpoLen;
                                    row_new["Field8"] = sExpoWidth;
                                    row_new["Field9"] = sExpoHeight;
                                    row_new["Field10"] = Math.Round(Convert.ToDecimal(sExpoLen) * Convert.ToDecimal(sExpoWidth) * Convert.ToDecimal(sExpoHeight) / Convert.ToDecimal("1000000"), 2);
                                    row_new["Field11"] = oExpoType_TW[sExpoType].ToString();

                                    dt_new.Rows.Add(row_new);
                                    iPackingChildIndex++;
                                }

                                if (jaPackingInfo.Count > 1)
                                {
                                    for (var i = 0; i < 10; i++)
                                    {
                                        var dicMerge = new Dictionary<string, int>
                                        {
                                            { "FirstRow", iPackingIndex + 3 },
                                            { "FirstCol", i },
                                            { "RowCount", jaPackingInfo.Count },
                                            { "ColCount", 1 }
                                        };
                                        listMerge.Add(dicMerge);
                                    }
                                }
                            }
                        }
                        var dicAlain = ExcelService.GetExportAlain(oHeader, "MuseumMumber,Contactor,Field0,Field1,Field2,Field3,Field4,Field5,Field6,Field7,Field8,Field9,Field10,Field11");

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
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBAPP.WSM.PackingOrder_QryService", "", "QueryPage（預約記錄分頁查詢）", "", "", "");
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

        #endregion 預約記錄分頁查詢
    }
}