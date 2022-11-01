using EasyBL.WebApi.Message;
using Entity.Sugar;
using Entity.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SqlSugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;
using System.Data;

namespace EasyBL.WEBAPP.OPM
{
    public class TransferBills_QryService : ServiceBase
    {
        #region 賬單拋轉（分頁查詢）

        /// <summary>
        /// 賬單拋轉（分頁查詢）
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

                    var sBillNO = _fetchString(i_crm, @"BillNO");
                    var sTransferDateStart = _fetchString(i_crm, @"TransferDateStart");
                    var sTransferDateEnd = _fetchString(i_crm, @"TransferDateEnd");
                    var bExcel = _fetchBool(i_crm, @"Excel");

                    var rTransferDateStart = new DateTime();
                    var rTransferDateEnd = new DateTime();
                    if (!string.IsNullOrEmpty(sTransferDateStart))
                    {
                        rTransferDateStart = SqlFunc.ToDate(sTransferDateStart);
                    }
                    if (!string.IsNullOrEmpty(sTransferDateEnd))
                    {
                        rTransferDateEnd = SqlFunc.ToDate(sTransferDateEnd).AddDays(1);
                    }

                    pml.DataList = db.Queryable<OVW_OPM_Bills>()
                        .Where(x => x.OrgID == i_crm.ORIGID)
                        .WhereIF(!string.IsNullOrEmpty(sBillNO), (t1) => t1.BillNO.Contains(sBillNO))
                        .WhereIF(!string.IsNullOrEmpty(sTransferDateStart), (t1) => SqlFunc.ToDate(t1.CreateDate) >= rTransferDateStart.Date)
                        .WhereIF(!string.IsNullOrEmpty(sTransferDateEnd), (t1) => SqlFunc.ToDate(t1.CreateDate) < rTransferDateEnd.Date)
                        .OrderBy(sSortField, sSortOrder)
                        .ToPageList(pml.PageIndex, bExcel ? 100000 : pml.PageSize, ref iPageCount);
                    pml.Total = iPageCount;

                    rm = new SuccessResponseMessage(null, i_crm);
                    if (bExcel)
                    {
                        var sFileName = "拋轉資料";
                        var oHeader = new Dictionary<string, string>();
                        var listMerge = new List<Dictionary<string, int>>();
                        var dicAlain = new Dictionary<string, string>();
                        var saCustomers1 = pml.DataList;
                        var saBills = pml.DataList as List<OVW_OPM_Bills>;
                        var dt_new = saBills.ListToDataTable<OVW_OPM_Bills>();
                        oHeader = new Dictionary<string, string>
                                                {
                                                    { "RowIndex", "項次" },
                                                    { "BillNO", "帳單號碼" },
                                                    { "ResponsiblePersonCodeName", "業務人員" },
                                                    { "Payer", "付款人" },
                                                    { "ForeignCurrencyCode", "幣別代號" },
                                                    { "ExchangeRate", "匯率" },
                                                    { "Advance", "預收金額" },
                                                    { "TWNOTaxAmount", "未稅金額" },
                                                    { "TaxSum", "稅金" },
                                                    { "BillAmount", "含稅金額" },
                                                    { "TotalReceivable", "總應收 " },
                                                    { "CreateUser", "審核人員" },
                                                    { "BillFirstCheckDate", "第一次審核時間" },
                                                    { "CreateDate", "最後審核時間" }
                                                };
                        foreach (DataRow row in dt_new.Rows)
                        {
                            row["Advance"] = row["Advance"].ToString() == "" ? "0" : $"{Convert.ToDouble(row["Advance"].ToString()):N2}";
                            row["TWNOTaxAmount"] = row["TWNOTaxAmount"].ToString() == "" ? "0" : $"{Convert.ToDouble(row["TWNOTaxAmount"].ToString()):N2}";
                            row["TaxSum"] = row["TaxSum"].ToString() == "" ? "0" : $"{Convert.ToDouble(row["TaxSum"].ToString()):N2}";
                            row["BillAmount"] = row["BillAmount"].ToString() == "" ? "0" : $"{Convert.ToDouble(row["BillAmount"].ToString()):N2}";
                            row["TotalReceivable"] = row["TotalReceivable"].ToString() == "" ? "0" : $"{Convert.ToDouble(row["TotalReceivable"].ToString().Replace(",", "")):N2}";
                        }
                        dicAlain = ExcelService.GetExportAlain(oHeader, "RowIndex", "Advance,TWNOTaxAmount,TaxSum,TotalReceivable,BillAmount");
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
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(TransferBills_QryService), "", "QueryPage（賬單拋轉（分頁查詢））", "", "", "");
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

        #endregion 賬單拋轉（分頁查詢）
    }
}