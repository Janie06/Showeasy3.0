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
using System.Linq;
using System.Text.RegularExpressions;

namespace EasyBL.WEBAPP.CRM
{
    public class Customers_QryService : ServiceBase
    {
        #region 客戶管理（分頁查詢）

        /// <summary>
        /// 客戶管理（分頁查詢）
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

                    var sCustomerNO = _fetchString(i_crm, @"CustomerNO");
                    var sUniCode = _fetchString(i_crm, @"UniCode");
                    var sCustomerName = _fetchString(i_crm, @"CustomerName");
                    var sCreateUser = _fetchString(i_crm, @"CreateUser");
                    var sTransactionType = _fetchString(i_crm, @"TransactionType");
                    var sEffective = _fetchString(i_crm, @"Effective");
                    var sState = _fetchString(i_crm, @"State");
                    var sIsAudit = _fetchString(i_crm, @"IsAudit");
                    var sCreateDateStart = _fetchString(i_crm, @"CreateDateStart");
                    var sCreateDateEnd = _fetchString(i_crm, @"CreateDateEnd");
                    var sModifyDateStart = _fetchString(i_crm, @"ModifyDateStart");
                    var sModifyDateEnd = _fetchString(i_crm, @"ModifyDateEnd");
                    var bExcel = _fetchBool(i_crm, @"Excel");
                    var sExcelType = _fetchString(i_crm, @"ExcelType");

                    string[] saEffective = null;
                    string[] saIsAudit = null;
                    string[] saTransactionType = null;
                    if (!string.IsNullOrEmpty(sEffective))
                    {
                        saEffective = sEffective.Split(',');
                    }
                    if (!string.IsNullOrEmpty(sIsAudit))
                    {
                        saIsAudit = sIsAudit.Split(',');
                    }
                    if (!string.IsNullOrEmpty(sTransactionType))
                    {
                        saTransactionType = sTransactionType.Split(',');
                    }
                    var rCreateDateStart = new DateTime();
                    var rCreateDateEnd = new DateTime();
                    var rModifyDateStart = new DateTime();
                    var rModifyDateEnd = new DateTime();
                    if (!string.IsNullOrEmpty(sCreateDateStart))
                    {
                        rCreateDateStart = SqlFunc.ToDate(sCreateDateStart);
                    }
                    if (!string.IsNullOrEmpty(sCreateDateEnd))
                    {
                        rCreateDateEnd = SqlFunc.ToDate(sCreateDateEnd).AddDays(1);
                    }
                    if (!string.IsNullOrEmpty(sModifyDateStart))
                    {
                        rModifyDateStart = SqlFunc.ToDate(sModifyDateStart);
                    }
                    if (!string.IsNullOrEmpty(sModifyDateEnd))
                    {
                        rModifyDateEnd = SqlFunc.ToDate(sModifyDateEnd).AddDays(1);
                    }

                    pml.DataList = db.Queryable<OTB_CRM_Customers, OTB_SYS_Members, OTB_CRM_CustomersMST>
                        ((t1, t2, t3) =>
                        new object[] {
                                JoinType.Inner, t1.OrgID == t2.OrgID && t1.CreateUser == t2.MemberID,
                                JoinType.Inner, t3.customer_guid == t1.guid
                              }
                        )
                        .Where((t1, t2) => t1.OrgID == i_crm.ORIGID && t1.UniCode.Contains(sUniCode) && SqlFunc.ContainsArray(saTransactionType, t1.TransactionType) && SqlFunc.ContainsArray(saEffective, t1.Effective) && SqlFunc.ContainsArray(saIsAudit, t1.IsAudit) && (t1.CustomerCName.Contains(sCustomerName) || t1.CustomerEName.Contains(sCustomerName) || t1.CustomerShotCName.Contains(sCustomerName)))
                        .WhereIF(!string.IsNullOrEmpty(sCustomerNO), (t3) => t3.CustomerNO.Contains(sCustomerNO))
                        .WhereIF(string.IsNullOrEmpty(sCustomerNO), (t3) => t3.Effective == "Y")
                        .WhereIF(!string.IsNullOrEmpty(sCreateUser), (t1, t2) => t1.CreateUser == sCreateUser)
                        .WhereIF(!string.IsNullOrEmpty(sCreateDateStart), (t1, t2) => t1.CreateDate >= rCreateDateStart.Date)
                        .WhereIF(!string.IsNullOrEmpty(sCreateDateEnd), (t1, t2) => t1.CreateDate <= rCreateDateEnd.Date)
                        .WhereIF(!string.IsNullOrEmpty(sModifyDateStart), (t1, t2) => t1.ModifyDate >= rModifyDateStart.Date)
                        .WhereIF(!string.IsNullOrEmpty(sModifyDateEnd), (t1, t2) => t1.ModifyDate <= rModifyDateEnd.Date)
                        .WhereIF(!string.IsNullOrEmpty(sState), (t1, t2) => t1.State == sState)
                        .Select((t1, t2) => new OTB_CRM_Customers
                        {
                            guid = SqlFunc.GetSelfAndAutoFill(t1.guid),
                            CreateUserName = t2.MemberName
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
                        var saCustomers = pml.DataList as List<OTB_CRM_Customers>;
                        switch (sExcelType)
                        {
                            case "Cus_BasicInformation":
                                {
                                    sFileName = "客戶基本資料";
                                    oHeader = new Dictionary<string, string>
                                                {
                                                    { "RowIndex", "項次" },
                                                    { "CustomerNO", "客戶代號" },
                                                    { "CustomerCName", "公司中文名稱" },
                                                    { "CustomerEName", "公司英文名稱" },
                                                    { "CustomerShotCName", "客戶簡稱" },
                                                    { "UniCode", "統一編號" },
                                                    { "Telephone", "公司電話" },
                                                    { "FAX", "公司傳真" },
                                                    { "Address", "地址" },
                                                    { "CreateUser", "創建人代號" },
                                                    { "CreateUserName", "創建人名稱" },
                                                    { "CreateDate", "創建時間" },
                                                    { "IsAudit", "審核狀態" }
                                                };
                                    dt_new.Columns.Add("RowIndex");
                                    dt_new.Columns.Add("CustomerNO");
                                    dt_new.Columns.Add("CustomerCName");
                                    dt_new.Columns.Add("CustomerEName");
                                    dt_new.Columns.Add("CustomerShotCName");
                                    dt_new.Columns.Add("UniCode");
                                    dt_new.Columns.Add("Telephone");
                                    dt_new.Columns.Add("FAX");
                                    dt_new.Columns.Add("Address");
                                    dt_new.Columns.Add("CreateUser");
                                    dt_new.Columns.Add("CreateUserName");
                                    dt_new.Columns.Add("CreateDate");
                                    dt_new.Columns.Add("IsAudit");
                                    foreach (var customer in saCustomers)
                                    {
                                        var row_new = dt_new.NewRow();
                                        row_new["RowIndex"] = customer.RowIndex;
                                        row_new["CustomerNO"] = customer.CustomerNO;
                                        row_new["CustomerCName"] = customer.CustomerCName;
                                        row_new["CustomerEName"] = customer.CustomerEName;
                                        row_new["CustomerShotCName"] = customer.CustomerShotCName;
                                        row_new["UniCode"] = customer.UniCode;
                                        row_new["Telephone"] = customer.Telephone;
                                        row_new["FAX"] = customer.FAX;
                                        row_new["Address"] = customer.Address;
                                        row_new["CreateUser"] = customer.CreateUser;
                                        row_new["CreateUserName"] = customer.CreateUserName;
                                        row_new["CreateDate"] = Convert.ToDateTime(customer.CreateDate).ToString("yyyy/MM/dd"); ;
                                        row_new["IsAudit"] = customer.IsAudit == "Y" ? "已審核" : customer.IsAudit == "N" ? "未審核" : "審核中";
                                        dt_new.Rows.Add(row_new);
                                    }
                                    dicAlain = ExcelService.GetExportAlain(oHeader, "CustomerNO,UniCode,Telephone,FAX,CreateUser,CreateUserName,CreateDate,IsAudit");
                                }
                                break;

                            case "Cus_Email":
                                {
                                    sFileName = "客戶資料(名稱^Email)";
                                    oHeader = new Dictionary<string, string>
                                                {
                                                    { "RowIndex", "項次" },
                                                    { "CustomerCName", "公司中文名稱" },
                                                    { "CustomerEName", "公司英文名稱" },
                                                    { "Email", "Email" },
                                                    { "Contactor", "聯絡人" },
                                                    { "ContactorEmail", "聯絡人Email" }
                                                };
                                    dicAlain = ExcelService.GetExportAlain(oHeader, new string[] { "Email", "Contactor", "ContactorEmail" });
                                    dt_new.Columns.Add("RowIndex");
                                    dt_new.Columns.Add("CustomerCName");
                                    dt_new.Columns.Add("CustomerEName");
                                    dt_new.Columns.Add("Email");
                                    dt_new.Columns.Add("Contactor");
                                    dt_new.Columns.Add("ContactorEmail");
                                    var iChildIndex = 0;
                                    foreach (var customer in saCustomers)
                                    {
                                        var iLastIndex = iChildIndex;
                                        var row_new = dt_new.NewRow();
                                        row_new["RowIndex"] = customer.RowIndex;
                                        row_new["CustomerCName"] = customer.CustomerCName;
                                        row_new["CustomerEName"] = customer.CustomerEName;
                                        row_new["Email"] = customer.Email;
                                        row_new["Contactor"] = "";
                                        row_new["ContactorEmail"] = "";
                                        var jaContactors = (JArray)JsonConvert.DeserializeObject(customer.Contactors);
                                        if (jaContactors != null && jaContactors.Count > 0)
                                        {
                                            var idx = 0;
                                            foreach (JObject jo in jaContactors)
                                            {
                                                if (idx != 0)
                                                {
                                                    row_new = dt_new.NewRow();
                                                    row_new["RowIndex"] = "";
                                                    row_new["CustomerCName"] = "";
                                                    row_new["CustomerEName"] = "";
                                                    row_new["Email"] = "";
                                                    row_new["Contactor"] = jo["FullName"];
                                                    row_new["ContactorEmail"] = jo["Email"];
                                                    dt_new.Rows.InsertAt(row_new, iChildIndex);
                                                }
                                                else
                                                {
                                                    row_new["Contactor"] = jo["FullName"];
                                                    row_new["ContactorEmail"] = jo["Email"];
                                                    dt_new.Rows.Add(row_new);
                                                }
                                                idx++;
                                                iChildIndex++;
                                            }
                                            if (jaContactors.Count > 1)
                                            {
                                                var dicMerge = new Dictionary<string, int>
                                                                    {
                                                                        { "FirstRow", iLastIndex + 3 },
                                                                        { "FirstCol", 0 },
                                                                        { "RowCount", jaContactors.Count },
                                                                        { "ColCount", 1 }
                                                                    };
                                                listMerge.Add(dicMerge);
                                                dicMerge = new Dictionary<string, int>
                                                                    {
                                                                        { "FirstRow", iLastIndex + 3 },
                                                                        { "FirstCol", 1 },
                                                                        { "RowCount", jaContactors.Count },
                                                                        { "ColCount", 1 }
                                                                    };
                                                listMerge.Add(dicMerge);
                                                dicMerge = new Dictionary<string, int>
                                                                    {
                                                                        { "FirstRow", iLastIndex + 3 },
                                                                        { "FirstCol", 2 },
                                                                        { "RowCount", jaContactors.Count },
                                                                        { "ColCount", 1 }
                                                                    };
                                                listMerge.Add(dicMerge);
                                                dicMerge = new Dictionary<string, int>
                                                                    {
                                                                        { "FirstRow", iLastIndex + 3 },
                                                                        { "FirstCol", 3 },
                                                                        { "RowCount", jaContactors.Count },
                                                                        { "ColCount", 1 }
                                                                    };
                                                listMerge.Add(dicMerge);
                                            }
                                        }
                                        else
                                        {
                                            dt_new.Rows.Add(row_new);
                                            iChildIndex++;
                                        }
                                    }
                                }
                                break;

                            case "Cus_WenzhongCusFile":
                                {
                                    sFileName = "文中客供商檔";
                                    oHeader = new Dictionary<string, string>
                                            {
                                                { "RowIndex", "項次" },
                                                { "CustomerNO", "客戶供應商代號" },
                                                { "CusField1", "客戶供應商類別" },
                                                { "CustomerShotCName", "客戶供應商簡稱" },
                                                { "CustomerCName", "客戶供應商全稱" },
                                                { "CusField2", "行業別" },
                                                { "CusField3", "類別科目代號" },
                                                { "UniCode", "統一編號" },
                                                { "CusField4", "稅籍編號" },
                                                { "CusField5", "郵遞區號" },
                                                { "InvoiceAddress", "發票地址" },
                                                { "Address", " 聯絡地址" },
                                                { "CusField6", "送貨地址" },
                                                { "CusField7", "電話(發票地址)" },
                                                { "Telephone", "電話(公司地址)" },
                                                { "CusField8", "電話(送貨地址)" },
                                                { "FAX", "傳真" },
                                                { "CusField9", "數據機種類" },
                                                { "CusField10", "傳呼機號碼" },
                                                { "CusField11", "行動電話" },
                                                { "CusField12", "網址" },
                                                { "CusField13", "負責人" },
                                                { "CusField14", "聯絡人" },
                                                { "Memo", "備註(30C" },
                                                { "CusField15", "銷售折數" },
                                                { "CusField16", "等級" },
                                                { "CusField17", "區域" },
                                                { "CusField18", "進貨折數" },
                                                { "CusField19", "部門|工地編號" },
                                                { "CusField20", "業務員代號" },
                                                { "CusField21", "服務人員" },
                                                { "CusField22", "建立日期" },
                                                { "CusField23", "最近交易日" },
                                                { "CusField24", "信用額度" },
                                                { "CusField25", "保證額度" },
                                                { "CusField26", "抵押額度" },
                                                { "CusField27", "已用額度" },
                                                { "CusField28", "開立發票方式" },
                                                { "CusField29", "收款方式" },
                                                { "CusField30", "匯款銀行代號" },
                                                { "CusField31", "匯款帳號" },
                                                { "CusField32", "結帳方式(作廢不使用)" },
                                                { "CusField33", "銷貨後幾個月結帳" },
                                                { "CusField34", "銷貨後逢幾日結帳" },
                                                { "CusField35", "結帳後幾個月收款" },
                                                { "CusField36", "結帳後逢幾日收款" },
                                                { "CusField37", "收款後幾個月兌現" },
                                                { "CusField38", "收款後逢幾日兌現" },
                                                { "CusField39", "進貨後幾個月結帳" },
                                                { "CusField40", "進貨後逢幾日結帳" },
                                                { "CusField41", "結帳後幾個月付款" },
                                                { "CusField42", "結帳後逢幾日付款" },
                                                { "CusField43", "付款後幾個月兌現" },
                                                { "CusField44", "付款後逢幾日兌現" },
                                                { "CusField45", "郵遞區號(聯絡地址)" },
                                                { "CusField46", "郵遞區號(送貨地址)" },
                                                { "CusField47", "職稱" },
                                                { "CusField48", "專案|項目編號" },
                                                { "CusField49", "請款客戶" },
                                                { "CusField50", "EAMIL ADDRS" },
                                                { "CusField51", "收款/付款方式(描述)" },
                                                { "CusField52", "交貨/收貨方式" },
                                                { "CusField53", "進出口交易方式" },
                                                { "CusField54", "交易幣別" },
                                                { "CusField55", "英文負責人" },
                                                { "CusField56", "英文聯絡人" },
                                                { "CusField57", "電子發票通知方式" },
                                                { "CusField58", "發票預設捐贈" },
                                                { "CusField59", "預設發票捐贈對象" },
                                                { "CusField60", "自定義欄位一" },
                                                { "CusField61", "自定義欄位二" },
                                                { "CusField62", "自定義欄位三" },
                                                { "CusField63", "自定義欄位四" },
                                                { "CusField64", "自定義欄位五" },
                                                { "CusField65", "自定義欄位六" },
                                                { "CusField66", "自定義欄位七" },
                                                { "CusField67", "自定義欄位八" },
                                                { "CusField68", "自定義欄位九" },
                                                { "CusField69", "自定義欄位十" },
                                                { "CusField71", "自定義欄位十一" },
                                                { "CusField72", "自定義欄位十二" },
                                                { "CusField73", "會員卡號" },
                                                { "CusField74", "客供商英文名稱" },
                                                { "CusField75", "客戶英文地址" },
                                                { "CusField76", "銷貨結帳終止日" },
                                                { "CusField77", "進貨結帳終止日" },
                                                { "CusField78", "銷貨收款週期選項" },
                                                { "CusField79", "進貨付款週期選項" },
                                                { "CusField80", "進貨收付條件" },
                                                { "CusField81", "客供商英文聯絡電話" },
                                                { "CusField82", "匯款戶名" },
                                                { "CusField83", "使用電子發票" },
                                                { "CusField84", "單價含稅否" },
                                                { "CusField85", "批次結帳" },
                                                { "CusField86", "發票服務平台登入密碼" },
                                                { "CusField87", "絡地址經度" },
                                                { "CusField88", "聯絡地址緯度" }
                                            };

                                    dt_new.Columns.Add("RowIndex");
                                    dt_new.Columns.Add("CustomerNO");
                                    dt_new.Columns.Add("CusField1");
                                    dt_new.Columns.Add("CustomerShotCName");
                                    dt_new.Columns.Add("CustomerCName");
                                    dt_new.Columns.Add("CusField2");
                                    dt_new.Columns.Add("CusField3");
                                    dt_new.Columns.Add("UniCode");
                                    dt_new.Columns.Add("CusField4");
                                    dt_new.Columns.Add("CusField5");
                                    dt_new.Columns.Add("InvoiceAddress");
                                    dt_new.Columns.Add("Address");
                                    dt_new.Columns.Add("CusField6");
                                    dt_new.Columns.Add("CusField7");
                                    dt_new.Columns.Add("Telephone");
                                    dt_new.Columns.Add("CusField8");
                                    dt_new.Columns.Add("FAX");
                                    dt_new.Columns.Add("CusField9");
                                    dt_new.Columns.Add("CusField10");
                                    dt_new.Columns.Add("CusField11");
                                    dt_new.Columns.Add("CusField12");
                                    dt_new.Columns.Add("CusField13");
                                    dt_new.Columns.Add("CusField14");
                                    dt_new.Columns.Add("Memo");
                                    dt_new.Columns.Add("CusField15");
                                    dt_new.Columns.Add("CusField16");
                                    dt_new.Columns.Add("CusField17");
                                    dt_new.Columns.Add("CusField18");
                                    dt_new.Columns.Add("CusField19");
                                    dt_new.Columns.Add("CusField20");
                                    dt_new.Columns.Add("CusField21");
                                    dt_new.Columns.Add("CusField22");
                                    dt_new.Columns.Add("CusField23");
                                    dt_new.Columns.Add("CusField24");
                                    dt_new.Columns.Add("CusField25");
                                    dt_new.Columns.Add("CusField26");
                                    dt_new.Columns.Add("CusField27");
                                    dt_new.Columns.Add("CusField28");
                                    dt_new.Columns.Add("CusField29");
                                    dt_new.Columns.Add("CusField30");
                                    dt_new.Columns.Add("CusField31");
                                    dt_new.Columns.Add("CusField32");
                                    dt_new.Columns.Add("CusField33");
                                    dt_new.Columns.Add("CusField34");
                                    dt_new.Columns.Add("CusField35");
                                    dt_new.Columns.Add("CusField36");
                                    dt_new.Columns.Add("CusField37");
                                    dt_new.Columns.Add("CusField38");
                                    dt_new.Columns.Add("CusField39");
                                    dt_new.Columns.Add("CusField40");
                                    dt_new.Columns.Add("CusField41");
                                    dt_new.Columns.Add("CusField42");
                                    dt_new.Columns.Add("CusField43");
                                    dt_new.Columns.Add("CusField44");
                                    dt_new.Columns.Add("CusField45");
                                    dt_new.Columns.Add("CusField46");
                                    dt_new.Columns.Add("CusField47");
                                    dt_new.Columns.Add("CusField48");
                                    dt_new.Columns.Add("CusField49");
                                    dt_new.Columns.Add("CusField50");
                                    dt_new.Columns.Add("CusField51");
                                    dt_new.Columns.Add("CusField52");
                                    dt_new.Columns.Add("CusField53");
                                    dt_new.Columns.Add("CusField54");
                                    dt_new.Columns.Add("CusField55");
                                    dt_new.Columns.Add("CusField56");
                                    dt_new.Columns.Add("CusField57");
                                    dt_new.Columns.Add("CusField58");
                                    dt_new.Columns.Add("CusField59");
                                    dt_new.Columns.Add("CusField60");
                                    dt_new.Columns.Add("CusField61");
                                    dt_new.Columns.Add("CusField62");
                                    dt_new.Columns.Add("CusField63");
                                    dt_new.Columns.Add("CusField64");
                                    dt_new.Columns.Add("CusField65");
                                    dt_new.Columns.Add("CusField66");
                                    dt_new.Columns.Add("CusField67");
                                    dt_new.Columns.Add("CusField68");
                                    dt_new.Columns.Add("CusField69");
                                    dt_new.Columns.Add("CusField70");
                                    dt_new.Columns.Add("CusField71");
                                    dt_new.Columns.Add("CusField72");
                                    dt_new.Columns.Add("CusField73");
                                    dt_new.Columns.Add("CusField74");
                                    dt_new.Columns.Add("CusField75");
                                    dt_new.Columns.Add("CusField76");
                                    dt_new.Columns.Add("CusField77");
                                    dt_new.Columns.Add("CusField78");
                                    dt_new.Columns.Add("CusField79");
                                    dt_new.Columns.Add("CusField80");
                                    dt_new.Columns.Add("CusField81");
                                    dt_new.Columns.Add("CusField82");
                                    dt_new.Columns.Add("CusField83");
                                    dt_new.Columns.Add("CusField84");
                                    dt_new.Columns.Add("CusField85");
                                    dt_new.Columns.Add("CusField86");
                                    dt_new.Columns.Add("CusField87");
                                    dt_new.Columns.Add("CusField88");

                                    foreach (var customer in saCustomers)
                                    {
                                        var sName = string.IsNullOrEmpty(customer.CustomerCName) ? customer.CustomerEName : customer.CustomerCName;
                                        var sEName = customer.CustomerEName;
                                        if (sName.Trim() == "")
                                        {
                                            sName = sEName;
                                        }
                                        var sMemo = customer.Memo;
                                        var sInvoiceAddress = customer.InvoiceAddress;
                                        var sAddress = customer.Address;
                                        var sTelephone = customer.Telephone;

                                        var row_new = dt_new.NewRow();
                                        row_new["RowIndex"] = customer.RowIndex;
                                        row_new["CustomerCName"] = Common.CutByteString(sName, 60);
                                        row_new["InvoiceAddress"] = Common.CutByteString(sInvoiceAddress, 60);
                                        row_new["Address"] = Common.CutByteString(sAddress, 60);
                                        row_new["Telephone"] = Common.CutByteString(sTelephone, 60);
                                        row_new["Memo"] = Common.CutByteString(sMemo, 30);
                                        row_new["CusField1"] = "0";
                                        row_new["CusField2"] = "";
                                        row_new["CusField3"] = "";
                                        row_new["CusField4"] = "";
                                        row_new["CusField5"] = "";
                                        row_new["CusField6"] = "";
                                        row_new["CusField7"] = "";
                                        row_new["CusField8"] = "";
                                        row_new["CusField9"] = "";
                                        row_new["CusField10"] = "";
                                        row_new["CusField11"] = "";
                                        row_new["CusField12"] = "";
                                        row_new["CusField13"] = "";
                                        row_new["CusField14"] = "";
                                        row_new["CusField15"] = "";
                                        row_new["CusField16"] = "";
                                        row_new["CusField17"] = "";
                                        row_new["CusField18"] = "";
                                        row_new["CusField19"] = "";
                                        var _CreateUser = customer.CreateUser;
                                        sCreateUser = _CreateUser.Split('.')[0];
                                        row_new["CusField20"] = Common.CutByteString(_CreateUser, 30); ;
                                        row_new["CusField21"] = "";
                                        row_new["CusField22"] = "";
                                        row_new["CusField23"] = "";
                                        row_new["CusField24"] = "";
                                        row_new["CusField25"] = "";
                                        row_new["CusField26"] = "";
                                        row_new["CusField27"] = "";
                                        row_new["CusField28"] = "B,C".IndexOf(customer.TransactionType.Trim()) > -1 ? "6" : "5";
                                        row_new["CusField29"] = "";
                                        row_new["CusField30"] = "";
                                        row_new["CusField31"] = "";
                                        row_new["CusField32"] = "";
                                        row_new["CusField33"] = "";
                                        row_new["CusField34"] = "";
                                        row_new["CusField35"] = "";
                                        row_new["CusField36"] = "";
                                        row_new["CusField37"] = "";
                                        row_new["CusField38"] = "";
                                        row_new["CusField39"] = "";
                                        row_new["CusField40"] = "";
                                        row_new["CusField41"] = "";
                                        row_new["CusField42"] = "";
                                        row_new["CusField43"] = "";
                                        row_new["CusField44"] = "";
                                        row_new["CusField45"] = "";
                                        row_new["CusField46"] = "";
                                        row_new["CusField47"] = "";
                                        row_new["CusField48"] = "";
                                        row_new["CusField49"] = customer.CustomerNO;
                                        row_new["CusField50"] = "";
                                        row_new["CusField51"] = "";
                                        row_new["CusField52"] = "";
                                        row_new["CusField53"] = "";
                                        row_new["CusField54"] = "";
                                        row_new["CusField55"] = "";
                                        row_new["CusField56"] = "";
                                        row_new["CusField57"] = "";
                                        row_new["CusField58"] = "";
                                        row_new["CusField59"] = "";
                                        row_new["CusField60"] = "";
                                        row_new["CusField61"] = "";
                                        row_new["CusField62"] = "";
                                        row_new["CusField63"] = "";
                                        row_new["CusField64"] = "";
                                        row_new["CusField65"] = "";
                                        row_new["CusField66"] = "";
                                        row_new["CusField67"] = "";
                                        row_new["CusField68"] = "";
                                        row_new["CusField69"] = "";
                                        row_new["CusField70"] = "";
                                        row_new["CusField71"] = "";
                                        row_new["CusField72"] = "";
                                        row_new["CusField73"] = "";
                                        row_new["CusField74"] = Common.CutByteString(customer.CustomerEName, 120);
                                        if (Regex.IsMatch(sAddress, "[\u4e00-\u9fa5]+"))
                                        {
                                            sAddress = "";
                                        }
                                        row_new["CusField75"] = Common.CutByteString(sAddress, 240);
                                        row_new["CusField76"] = "";
                                        row_new["CusField77"] = "";
                                        row_new["CusField78"] = "";
                                        row_new["CusField79"] = "";
                                        row_new["CusField80"] = "";
                                        row_new["CusField81"] = "";
                                        row_new["CusField82"] = "";
                                        row_new["CusField83"] = "";
                                        row_new["CusField84"] = "";
                                        row_new["CusField85"] = "";
                                        row_new["CusField86"] = "";
                                        row_new["CusField87"] = "";
                                        row_new["CusField88"] = "";
                                        dt_new.Rows.Add(row_new);
                                    }
                                    dicAlain = ExcelService.GetExportAlain(oHeader, "CustomerNO,UniCode,Telephone,FAX,CusField1");
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
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Customers_QryService), "", "QueryPage（客戶管理（分頁查詢））", "", "", "");
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

        #endregion 客戶管理（分頁查詢）

        #region 客戶管理（單筆查詢）

        /// <summary>
        /// 客戶管理（單筆查詢）
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

                    var oEntity = db.Queryable<OTB_CRM_Customers>().Single(x => x.OrgID == i_crm.ORIGID && x.guid == sId);

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, oEntity);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Customers_QryService), "", "QueryOne（客戶管理（單筆查詢））", "", "", "");
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

        #endregion 客戶管理（單筆查詢）
    }
}