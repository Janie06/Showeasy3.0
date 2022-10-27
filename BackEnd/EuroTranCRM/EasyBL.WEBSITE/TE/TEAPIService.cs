using EasyBL.WebApi.Message;
using EasyNet;
using Entity;
using Entity.Sugar;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SqlSugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;
using System.Web;

namespace EasyBL.WEBSITE.TE
{
    public class TEAPIService : ServiceBase
    {
        #region 獲取最新消息個數（近一個月內）

        /// <summary>
        /// 獲取最新消息個數（近一個月內）
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetNewsCount(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.DB;
            try
            {
                do
                {
                    var sLang = _fetchString(i_crm, "Lang") ?? i_crm.LANG;
                    var rDate = DateTime.Now.AddDays(-30);
                    var iCount = db.Queryable<OTB_WSM_News>()
                        .Count(x => x.OrgID == i_crm.ORIGID && x.News_Show == "Y" && x.News_LanguageType == sLang && x.CreateDate >= rDate);
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iCount);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WebSite.TE.TEAPIService", "獲取最新消息個數（近一個月內）", nameof(GetNewsCount), "", "", "");
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

        #endregion 獲取最新消息個數（近一個月內）

        #region 前臺Tracking查詢

        /// <summary>
        /// 前臺Tracking查詢
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on GetTrackingProgress</param>
        /// <returns></returns>
        public ResponseMessage GetTrackingProgress(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            var oExportMap = new SetMap();
            try
            {
                do
                {
                    var sQueryNum = _fetchString(i_crm, "QueryNum");
                    if (sQueryNum.Length < 12)
                    {
                        sMsg = "查詢碼錯誤";
                        if (i_crm.LANG == "zh")
                        {
                            sMsg = "查询码错误";
                        }
                        else if (i_crm.LANG == "en")
                        {
                            sMsg = "Query code error";
                        }
                        break;
                    }
                    var sValidCode = _fetchString(i_crm, "ValidCode");
                    var sIP = _fetchString(i_crm, "IP");
                    var sIPInfo = _fetchString(i_crm, "IPInfo");
                    var sType = sQueryNum.Substring(3, 1);
                    var sParentId = "";
                    var sCustomerName = "";
                    var sAgentName = "";
                    var sResponsiblePerson = "";
                    var sFlag = _fetchString(i_crm, "flag");
                    var sCaptcha = "";
                    if (sValidCode != null)
                    {
                        if (HttpContext.Current.Session[BLWording.CAPTCHA + sFlag] != null)
                        {
                            sCaptcha = HttpContext.Current.Session[BLWording.CAPTCHA + sFlag].ToString();
                        }
                        if (sCaptcha != sValidCode)
                        {
                            sMsg = "驗證碼錯誤";
                            if (i_crm.LANG == "zh")
                            {
                                sMsg = "验证码错误";
                            }
                            else if (i_crm.LANG == "en")
                            {
                                sMsg = "Incorrect verification code";
                            }
                            break;
                        }
                    }

                    if (sType == "I")
                    {
                        var oImport = db.Queryable<OTB_OPM_ImportExhibition>()
                            .Single(it => it.RefNumber == sQueryNum && it.OrgID == i_crm.ORIGID);

                        if (oImport != null)
                        {
                            sParentId = oImport.ImportBillNO;
                            sResponsiblePerson = oImport.ResponsiblePerson;
                            var JImport = (JObject)JsonConvert.DeserializeObject(oImport.Import);
                            var saFlows = new List<SetMap>();
                            oExportMap.Put("Type", "IM");
                            var oCus = new OTB_CRM_Customers();
                            OTB_OPM_Exhibition oExhibition = null;
                            if (oImport.SupplierType == "S")
                            {
                                if (!string.IsNullOrEmpty(oImport.Supplier))
                                {
                                    oCus = db.Queryable<OTB_CRM_Customers>().Single(it => it.guid == oImport.Supplier);
                                    sCustomerName = string.IsNullOrEmpty(oCus.CustomerCName) ? oCus.CustomerEName : oCus.CustomerCName;
                                }
                                if (!string.IsNullOrEmpty(oImport.Agent))
                                {
                                    var oAgent = db.Queryable<OTB_CRM_Customers>().Single(it => it.guid == oImport.Agent);
                                    sAgentName = string.IsNullOrEmpty(oAgent.CustomerCName) ? oAgent.CustomerEName : oAgent.CustomerCName;
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(oImport.Agent))
                                {
                                    oCus = db.Queryable<OTB_CRM_Customers>().Single(it => it.guid == oImport.Agent);
                                    sAgentName = string.IsNullOrEmpty(oCus.CustomerCName) ? oCus.CustomerEName : oCus.CustomerCName;
                                }
                                if (!string.IsNullOrEmpty(oImport.Supplier))
                                {
                                    var oSupplier = db.Queryable<OTB_CRM_Customers>().Single(it => it.guid == oImport.Supplier);
                                    sCustomerName = string.IsNullOrEmpty(oSupplier.CustomerCName) ? oSupplier.CustomerEName : oSupplier.CustomerCName;
                                }
                            }
                            if (oCus == null)
                            {
                                oCus = new OTB_CRM_Customers();
                            }
                            if (oImport.ExhibitionNO != "")
                            {
                                oExhibition = db.Queryable<OTB_OPM_Exhibition>()
                                    .Single(it => it.SN == oImport.ExhibitionNO.ObjToInt());
                                if (oExhibition == null)
                                {
                                    oExhibition = new OTB_OPM_Exhibition();
                                }
                            }

                            oExportMap.Put("SupplierName", oCus.CustomerCName ?? "");
                            oExportMap.Put("SupplierEName", oCus.CustomerEName ?? "");
                            oExportMap.Put("Exhibitioname_TW", oExhibition.Exhibitioname_TW ?? "");
                            oExportMap.Put("Exhibitioname_CN", oExhibition.Exhibitioname_CN ?? "");
                            oExportMap.Put("Exhibitioname_EN", oExhibition.Exhibitioname_EN ?? "");
                            oExportMap.Put("PrgCode", "ExhibitionImport_Upd");

                            var Flow = new SetMap();
                            Flow.Put("Parent", true);
                            Flow.Put("Index", 0);
                            Flow.Put("ShotName", "IMPORT");
                            Flow.Put("FlowName", "ExhibitionImport_Upd.Import");
                            Flow.Put("Number", oImport.BoxNo);
                            Flow.Put("Unit", oImport.Unit);
                            Flow.Put("BillLadNO", oImport.BillLadNO);
                            Flow.Put("BillLadNOSub", oImport.BillLadNOSub);
                            Flow.Put("ContainerNumber", oImport.ContainerNumber);
                            Flow.Put("Data", oImport.Import);
                            Flow.Put("ShipmentPort", oImport.ShipmentPortCode);
                            Flow.Put("Destination", oImport.DestinationPortCode);
                            var sPath = "";
                            if (oImport.SignatureFileId != null && oImport.SignatureFileId != "")
                            {
                                var oFile = db.Queryable<OTB_SYS_Files>()
                                    .Single(it => it.OrgID == i_crm.ORIGID && it.ParentID == oImport.SignatureFileId);
                                if (oFile != null)
                                {
                                    sPath = oFile.FilePath;
                                }
                            }
                            Flow.Put("SignatureFile", sPath);
                            saFlows.Add(Flow);

                            var jaReImports = (JArray)JsonConvert.DeserializeObject(oImport.ReImports);
                            if (jaReImports != null && jaReImports.Count > 0)
                            {
                                var iIndex = 1;
                                foreach (JObject jo in jaReImports)
                                {
                                    var sFilePath = "";
                                    var sIndex = jaReImports.Count > 1 ? ("-" + iIndex) : "";
                                    var sSignatureFileId = jo["SignatureFileId"].ToString();
                                    var JReImport = (JObject)jo["ReImport"];
                                    var JReImportData = (JObject)jo["ReImportData"];
                                    Flow = new SetMap();
                                    Flow.Put("Index", sIndex);
                                    Flow.Put("ShotName", "RETURN" + sIndex);
                                    Flow.Put("FlowName", "ExhibitionImport_Upd.ReImport" + (jaReImports.Count > 1 ? iIndex.ToString() : ""));
                                    Flow.Put("Number", JReImportData["Number"] ?? "");
                                    Flow.Put("Unit", JReImportData["Unit"] ?? "");
                                    Flow.Put("BillLadNO", JReImportData["BillLadNO"] ?? "");
                                    Flow.Put("BillLadNOSub", JReImportData["BillLadNOSub"] ?? "");
                                    Flow.Put("ContainerNumber", JReImportData["ContainerNumber"] ?? "");
                                    Flow.Put("Data", JReImport.ToString());
                                    Flow.Put("ShipmentPort", JReImportData["DestinationPortCode"] ?? "");
                                    Flow.Put("Destination", JReImportData["ReShipmentPortCode"] ?? "");
                                    if (sSignatureFileId != "")
                                    {
                                        var oFile = db.Queryable<OTB_SYS_Files>()
                                            .Single(it => it.OrgID == i_crm.ORIGID && it.ParentID == sSignatureFileId);
                                        if (oFile != null)
                                        {
                                            sPath = oFile.FilePath;
                                        }
                                    }
                                    Flow.Put("SignatureFile", sFilePath);
                                    saFlows.Add(Flow);
                                    iIndex++;
                                }
                            }
                            oExportMap.Put("Flows", saFlows);
                        }
                    }
                    else if (sType == "E")
                    {
                        var sParentQueryNum = sQueryNum.Substring(0, sQueryNum.Length - 3);
                        var oExport = db.Queryable<OTB_OPM_ExportExhibition>()
                            .Single(it => it.RefNumber == sParentQueryNum && it.OrgID == i_crm.ORIGID);
                        if (oExport != null)
                        {
                            sParentId = oExport.ExportBillNO;
                            sResponsiblePerson = oExport.ResponsiblePerson;
                            if (!string.IsNullOrEmpty(oExport.Agent))
                            {
                                var oCus = db.Queryable<OTB_CRM_Customers>().Single(it => it.guid == oExport.Agent);
                                sAgentName = string.IsNullOrEmpty(oCus.CustomerCName) ? oCus.CustomerEName : oCus.CustomerCName;
                            }
                            var oExhibition = new OTB_OPM_Exhibition();
                            if (oExport.ExhibitionNO != "")
                            {
                                oExhibition = db.Queryable<OTB_OPM_Exhibition>()
                                    .Single(it => it.SN == oExport.ExhibitionNO.ObjToInt());
                                if (oExhibition == null)
                                {
                                    oExhibition = new OTB_OPM_Exhibition();
                                }
                            }
                            var Exhibitors = (JArray)JsonConvert.DeserializeObject(oExport.Exhibitors);
                            foreach (JObject Exhibitor in Exhibitors)
                            {
                                if (Exhibitor["RefSupplierNo"] != null && Exhibitor["RefSupplierNo"].ToString() == sQueryNum)
                                {
                                    var JExportData = (JObject)Exhibitor["ExportData"];
                                    var iIndex = 0;

                                    var saFlows = new List<SetMap>();
                                    oExportMap.Put("Type", "EX");
                                    oExportMap.Put("SupplierName", Exhibitor["SupplierName"] ?? "");
                                    oExportMap.Put("SupplierEName", Exhibitor["SupplierEName"] ?? "");
                                    oExportMap.Put("Exhibitioname_TW", oExhibition.Exhibitioname_TW);
                                    oExportMap.Put("Exhibitioname_CN", oExhibition.Exhibitioname_CN);
                                    oExportMap.Put("Exhibitioname_EN", oExhibition.Exhibitioname_EN);
                                    oExportMap.Put("PrgCode", "ExhibitionExport_Upd");
                                    sCustomerName = oExportMap["SupplierName"].ToString() == "" ? oExportMap["SupplierEName"].ToString() : oExportMap["SupplierName"].ToString();

                                    var Flow = new SetMap();
                                    Flow.Put("Parent", true);
                                    Flow.Put("Index", iIndex);
                                    Flow.Put("ShotName", "EXPORT");
                                    Flow.Put("FlowName", "ExhibitionExport_Upd.ExportData");
                                    Flow.Put("Number", ((JObject)JExportData["Intowarehouse"])["Number"] ?? "");
                                    Flow.Put("Unit", ((JObject)JExportData["Intowarehouse"])["Unit"] ?? "");
                                    Flow.Put("BillLadNO", oExport.BillLadNO);
                                    Flow.Put("BillLadNOSub", oExport.BillLadNOSub);
                                    Flow.Put("ContainerNumber", oExport.ContainerNumber);
                                    var JClearanceData = (JObject)Exhibitor["ClearanceData"];
                                    JExportData["GoodsArrival"] = JClearanceData["GoodsArrival"];
                                    JExportData["CargoRelease"] = JClearanceData["CargoRelease"];
                                    JExportData["WaitingApproach"] = JClearanceData["WaitingApproach"];
                                    JExportData["ServiceBooth"] = JClearanceData["ServiceBooth"];
                                    //JExportData["Sign"] = JClearanceData["Sign"];
                                    Flow.Put("Data", JExportData.ToString());
                                    Flow.Put("ShipmentPort", oExport.ShipmentPortCode);
                                    Flow.Put("Destination", oExport.DestinationCode);
                                    Flow.Put("SitiContactor1", oExport.SitiContactor1);
                                    Flow.Put("SitiTelephone1", oExport.SitiTelephone1);
                                    Flow.Put("SitiContactor2", oExport.SitiContactor2);
                                    Flow.Put("SitiTelephone2", oExport.SitiTelephone2);
                                    saFlows.Add(Flow);
                                    iIndex++;

                                    var sReturnType = Exhibitor["ReturnType"].ToString();

                                    if (sReturnType == "H") //退運回台
                                    {
                                        var JReImport = (JObject)Exhibitor["ReImport"];
                                        var bReturn_ReImport = ((JObject)JReImport["CargoCollection"])["Checked"] != null ? true : false;
                                        if (bReturn_ReImport)
                                        {
                                            Flow = new SetMap();
                                            Flow.Put("Index", iIndex);
                                            Flow.Put("ShotName", "RE-IMPORT");
                                            Flow.Put("FlowName", "ExhibitionExport_Upd.ReImport");
                                            Flow.Put("Number", JReImport["Number"] ?? "");
                                            Flow.Put("Unit", JReImport["Unit"] ?? "");
                                            Flow.Put("BillLadNO", "");
                                            Flow.Put("BillLadNOSub", "");
                                            Flow.Put("ContainerNumber", "");
                                            Flow.Put("Data", JReImport.ToString());
                                            Flow.Put("ShipmentPort", JReImport["ShipmentPortCode"] ?? "");
                                            Flow.Put("Destination", JReImport["DestinationCode"] ?? "");
                                            saFlows.Add(Flow);
                                            iIndex++;
                                        }
                                    }
                                    else if (sReturnType == "T") //出貨至第三地
                                    {
                                        var JTranserThird = (JObject)Exhibitor["TranserThird"];
                                        var bReturn_TranserThird = ((JObject)JTranserThird["CargoCollection"])["Checked"] != null ? true : false;
                                        if (bReturn_TranserThird)
                                        {
                                            Flow = new SetMap();
                                            Flow.Put("Index", iIndex);
                                            Flow.Put("ShotName", "TRANSFER");
                                            Flow.Put("FlowName", "ExhibitionExport_Upd.TransferThirdPlace");
                                            Flow.Put("Number", JTranserThird["Number"] ?? "");
                                            Flow.Put("Unit", JTranserThird["Unit"] ?? "");
                                            Flow.Put("BillLadNO", "");
                                            Flow.Put("BillLadNOSub", "");
                                            Flow.Put("ContainerNumber", "");
                                            Flow.Put("Data", JTranserThird.ToString());
                                            Flow.Put("ShipmentPort", JTranserThird["ShipmentPortCode"] ?? "");
                                            Flow.Put("Destination", JTranserThird["DestinationCode"] ?? "");
                                            saFlows.Add(Flow);
                                            iIndex++;

                                            if (Exhibitor["LastReturnType"] != null)
                                            {
                                                var sLastReturnType = Exhibitor["LastReturnType"].ToString();
                                                if (sLastReturnType == "F")
                                                {
                                                    var JTransferFour = (JObject)Exhibitor["TransferFour"];
                                                    var bReturn_TransferFour = ((JObject)JTransferFour["CargoCollection"])["Checked"] != null ? true : false;
                                                    if (bReturn_TransferFour)
                                                    {
                                                        Flow = new SetMap();
                                                        Flow.Put("Index", iIndex);
                                                        Flow.Put("ShotName", "TRANSFER");
                                                        Flow.Put("FlowName", "ExhibitionExport_Upd.TransferFourPlace");
                                                        Flow.Put("Number", JTransferFour["Number"] ?? "");
                                                        Flow.Put("Unit", JTransferFour["Unit"] ?? "");
                                                        Flow.Put("BillLadNO", "");
                                                        Flow.Put("BillLadNOSub", "");
                                                        Flow.Put("ContainerNumber", "");
                                                        Flow.Put("Data", JTransferFour.ToString());
                                                        Flow.Put("ShipmentPort", JTransferFour["ShipmentPortCode"] ?? "");
                                                        Flow.Put("Destination", JTransferFour["DestinationCode"] ?? "");
                                                        saFlows.Add(Flow);
                                                        iIndex++;
                                                    }
                                                    if (Exhibitor["ReturnType_4"] != null)
                                                    {
                                                        var sReturnType_4 = Exhibitor["ReturnType_4"].ToString();
                                                        if (sReturnType_4 == "F")
                                                        {
                                                            var JTransferFive = (JObject)Exhibitor["TransferFive"];
                                                            var bReturn_TransferFive = ((JObject)JTransferFive["CargoCollection"])["Checked"] != null ? true : false;
                                                            if (bReturn_TransferFive)
                                                            {
                                                                Flow = new SetMap();
                                                                Flow.Put("Index", iIndex);
                                                                Flow.Put("ShotName", "TRANSFER");
                                                                Flow.Put("FlowName", "ExhibitionExport_Upd.TransferFivePlace");
                                                                Flow.Put("Number", JTransferFive["Number"] ?? "");
                                                                Flow.Put("Unit", JTransferFive["Unit"] ?? "");
                                                                Flow.Put("BillLadNO", "");
                                                                Flow.Put("BillLadNOSub", "");
                                                                Flow.Put("ContainerNumber", "");
                                                                Flow.Put("Data", JTransferFive.ToString());
                                                                Flow.Put("ShipmentPort", JTransferFive["ShipmentPortCode"] ?? "");
                                                                Flow.Put("Destination", JTransferFive["DestinationCode"] ?? "");
                                                                saFlows.Add(Flow);
                                                                iIndex++;
                                                            }

                                                            if (Exhibitor["ReturnType_5"] != null)
                                                            {
                                                                var sReturnType_5 = Exhibitor["ReturnType_5"].ToString();
                                                                if (sReturnType_5 == "F")
                                                                {
                                                                    var JTransferSix = (JObject)Exhibitor["TransferSix"];
                                                                    var bReturn_TransferSix = ((JObject)JTransferSix["CargoCollection"])["Checked"] != null ? true : false;
                                                                    if (bReturn_TransferSix)
                                                                    {
                                                                        Flow = new SetMap();
                                                                        Flow.Put("Index", iIndex);
                                                                        Flow.Put("ShotName", "TRANSFER");
                                                                        Flow.Put("FlowName", "ExhibitionExport_Upd.TransferSixPlace");
                                                                        Flow.Put("Number", JTransferSix["Number"] ?? "");
                                                                        Flow.Put("Unit", JTransferSix["Unit"] ?? "");
                                                                        Flow.Put("BillLadNO", "");
                                                                        Flow.Put("BillLadNOSub", "");
                                                                        Flow.Put("ContainerNumber", "");
                                                                        Flow.Put("Data", JTransferSix.ToString());
                                                                        Flow.Put("ShipmentPort", JTransferSix["ShipmentPortCode"] ?? "");
                                                                        Flow.Put("Destination", JTransferSix["DestinationCode"] ?? "");
                                                                        saFlows.Add(Flow);
                                                                        iIndex++;
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    var JReImportSix = (JObject)Exhibitor["ReImportSix"];
                                                                    if (JReImportSix != null)
                                                                    {
                                                                        var bReturn_ReImportSix = ((JObject)JReImportSix["CargoCollection"])["Checked"] != null ? true : false;
                                                                        if (bReturn_ReImportSix)
                                                                        {
                                                                            Flow = new SetMap();
                                                                            Flow.Put("Index", iIndex);
                                                                            Flow.Put("ShotName", "RE-IMPORT");
                                                                            Flow.Put("FlowName", "ExhibitionExport_Upd.ReImport");
                                                                            Flow.Put("Number", JReImportSix["Number"] ?? "");
                                                                            Flow.Put("Unit", JReImportSix["Unit"] ?? "");
                                                                            Flow.Put("BillLadNO", "");
                                                                            Flow.Put("BillLadNOSub", "");
                                                                            Flow.Put("ContainerNumber", "");
                                                                            Flow.Put("Data", JReImportSix.ToString());
                                                                            Flow.Put("ShipmentPort", JReImportSix["ShipmentPortCode"] ?? "");
                                                                            Flow.Put("Destination", JReImportSix["DestinationCode"] ?? "");
                                                                            saFlows.Add(Flow);
                                                                            iIndex++;
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            var JReImportFive = (JObject)Exhibitor["ReImportFive"];
                                                            if (JReImportFive != null)
                                                            {
                                                                var bReturn_ReImportFive = ((JObject)JReImportFive["CargoCollection"])["Checked"] != null ? true : false;
                                                                if (bReturn_ReImportFive)
                                                                {
                                                                    Flow = new SetMap();
                                                                    Flow.Put("Index", iIndex);
                                                                    Flow.Put("ShotName", "RE-IMPORT");
                                                                    Flow.Put("FlowName", "ExhibitionExport_Upd.ReImport");
                                                                    Flow.Put("Number", JReImportFive["Number"] ?? "");
                                                                    Flow.Put("Unit", JReImportFive["Unit"] ?? "");
                                                                    Flow.Put("BillLadNO", "");
                                                                    Flow.Put("BillLadNOSub", "");
                                                                    Flow.Put("ContainerNumber", "");
                                                                    Flow.Put("Data", JReImportFive.ToString());
                                                                    Flow.Put("ShipmentPort", JReImportFive["ShipmentPortCode"] ?? "");
                                                                    Flow.Put("Destination", JReImportFive["DestinationCode"] ?? "");
                                                                    saFlows.Add(Flow);
                                                                    iIndex++;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    var JReImportFour = (JObject)Exhibitor["ReImportFour"];
                                                    if (JReImportFour != null)
                                                    {
                                                        var bReturn_ReImportFour = ((JObject)JReImportFour["CargoCollection"])["Checked"] != null ? true : false;
                                                        if (bReturn_ReImportFour)
                                                        {
                                                            Flow = new SetMap();
                                                            Flow.Put("Index", iIndex);
                                                            Flow.Put("ShotName", "RE-IMPORT");
                                                            Flow.Put("FlowName", "ExhibitionExport_Upd.ReImport");
                                                            Flow.Put("Number", JReImportFour["Number"] ?? "");
                                                            Flow.Put("Unit", JReImportFour["Unit"] ?? "");
                                                            Flow.Put("BillLadNO", "");
                                                            Flow.Put("BillLadNOSub", "");
                                                            Flow.Put("ContainerNumber", "");
                                                            Flow.Put("Data", JReImportFour.ToString());
                                                            Flow.Put("ShipmentPort", JReImportFour["ShipmentPortCode"] ?? "");
                                                            Flow.Put("Destination", JReImportFour["DestinationCode"] ?? "");
                                                            saFlows.Add(Flow);
                                                            iIndex++;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else if (sReturnType == "P")
                                    { //部分運至第三地部分退運回台
                                        var JPartThird = (JObject)Exhibitor["PartThird"];
                                        var JPartReImport = (JObject)Exhibitor["PartReImport"];
                                        if (JPartThird != null)
                                        {
                                            var bReturn_PartThird = ((JObject)JPartThird["CargoCollection"])["Checked"] != null ? true : false;
                                            if (bReturn_PartThird)
                                            {
                                                Flow = new SetMap();
                                                Flow.Put("Index", iIndex);
                                                Flow.Put("ShotName", "TRANSFER");
                                                Flow.Put("FlowName", "ExhibitionExport_Upd.TransferThirdPlace");
                                                Flow.Put("Number", JPartThird["Number"] ?? "");
                                                Flow.Put("Unit", JPartThird["Unit"] ?? "");
                                                Flow.Put("BillLadNO", "");
                                                Flow.Put("BillLadNOSub", "");
                                                Flow.Put("ContainerNumber", "");
                                                Flow.Put("Data", JPartThird.ToString());
                                                Flow.Put("ShipmentPort", JPartThird["ShipmentPortCode"] ?? "");
                                                Flow.Put("Destination", JPartThird["DestinationCode"] ?? "");
                                                saFlows.Add(Flow);
                                                iIndex++;
                                            }
                                        }

                                        if (JPartReImport != null)
                                        {
                                            var bReturn_PartReImport = ((JObject)JPartReImport["CargoCollection"])["Checked"] != null ? true : false;
                                            if (bReturn_PartReImport)
                                            {
                                                Flow = new SetMap();
                                                Flow.Put("Index", iIndex);
                                                Flow.Put("ShotName", "RE-IMPORT");
                                                Flow.Put("FlowName", "ExhibitionExport_Upd.ReturnedHome");
                                                Flow.Put("Number", JPartReImport["Number"] ?? "");
                                                Flow.Put("Unit", JPartReImport["Unit"] ?? "");
                                                Flow.Put("BillLadNO", "");
                                                Flow.Put("BillLadNOSub", "");
                                                Flow.Put("ContainerNumber", "");
                                                Flow.Put("Data", JPartReImport.ToString());
                                                Flow.Put("ShipmentPort", JPartReImport["ShipmentPortCode"] ?? "");
                                                Flow.Put("Destination", JPartReImport["DestinationCode"] ?? "");
                                                saFlows.Add(Flow);
                                                iIndex++;
                                            }
                                        }
                                    }

                                    oExportMap.Put("Flows", saFlows);
                                    break;
                                }
                            }
                        }
                    }
                    if (oExportMap.Keys.Count > 0)
                    {
                        var spm = new List<SugarParameter> {
                            new SugarParameter("@OrgID", i_crm.ORIGID),
                            new SugarParameter("@ResponsiblePerson", sResponsiblePerson),
                        };
                        var sDepartmentIDs = db.Ado.GetString(@"select DepartmentID+',' from [dbo].[OFN_SYS_GetParentDepartmentIdByUserID](@OrgID,@ResponsiblePerson) for xml path('')", spm);
                        var oTrackingLog = new OTB_WSM_TrackingLog
                        {
                            OrgID = i_crm.ORIGID,
                            QueryNumber = sQueryNum,
                            QueryIp = string.IsNullOrEmpty(sIP) ? i_crm.ClientIP : sIP,
                            IPInfo = sIPInfo,
                            QueryTime = DateTime.Now,
                            ParentId = sParentId,
                            CustomerName = sCustomerName,
                            AgentName = sAgentName,
                            DepartmentIDs = sDepartmentIDs
                        };
                        db.Insertable<OTB_WSM_TrackingLog>(oTrackingLog).ExecuteCommand();
                    }
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, oExportMap);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WebSite.TE.TEAPIService", "", "GetTrackingProgress（前臺Tracking查詢）", "", "", "");
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

        #endregion 前臺Tracking查詢

        #region 在線預約

        /// <summary>
        /// 在線預約
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage Appoint(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var bSend = false;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sExhibitionNO = _fetchString(i_crm, "ExhibitionNO");
                    var sCompName = _fetchString(i_crm, "CompName");
                    var oExhibition = db.Queryable<OTB_OPM_Exhibition>().Single(x => x.SN == int.Parse(sExhibitionNO));
                    var sCurDate = DateTime.Now.ToString("yyyyMMdd");
                    var sUnicode = _fetchString(i_crm, "Unicode");

                    //var oImportCustomers = db.Queryable<OTB_CRM_ImportCustomers>()
                    //    .First(x => x.OrgID == i_crm.ORIGID && x.ExhibitionNO.ToString() == sExhibitionNO && x.CustomerCName == sCompName);

                    var oImportCustomers = db.Queryable<OTB_OPM_ExhibitionCustomers, OTB_CRM_Customers>((t1, t2) => new object[] {
                          JoinType.Inner,t1.CustomerId==t2.guid})
                          .Where((t1, t2) => t1.ExhibitionNO == sExhibitionNO && t2.Effective == "Y" && (t2.CustomerCName == sCompName || t2.CustomerEName == sCompName || t2.CustomerShotCName == sCompName || t2.UniCode == sUnicode))
                          .First();

                    var oPackingOrder = new OTB_WSM_PackingOrder
                    {
                        AppointNO = SerialNumber.GetMaxNumberByType(i_crm.ORIGID, oExhibition.ExhibitionCode, MaxNumberType.Empty, i_crm.USERID, 3, sCurDate),
                        OrgID = i_crm.ORIGID,
                        ExhibitionNO = sExhibitionNO,
                        CustomerId = oImportCustomers?.CustomerId,
                        CompName = sCompName,
                        MuseumMumber = _fetchString(i_crm, "MuseumMumber"),
                        AppointUser = _fetchString(i_crm, "AppointUser"),
                        AppointTel = _fetchString(i_crm, "AppointTel"),
                        AppointEmail = _fetchString(i_crm, "AppointEmail"),
                        Contactor = _fetchString(i_crm, "Contactor"),
                        ContactTel = _fetchString(i_crm, "ContactTel"),
                        ApproachTime = Convert.ToDateTime(_fetchString(i_crm, "ApproachTime") + " " + _fetchString(i_crm, "ApproachTime_Hour") + ":" + _fetchString(i_crm, "ApproachTime_Min")),
                        ExitTime = Convert.ToDateTime(_fetchString(i_crm, "ExitTime") + " " + _fetchString(i_crm, "ExitTime_Hour") + ":" + _fetchString(i_crm, "ExitTime_Min")),
                        PackingInfo = _fetchString(i_crm, "PackingInfo"),
                        Total = Convert.ToDecimal(_fetchString(i_crm, "Total")),
                        Unicode = _fetchString(i_crm, "Unicode"),
                        AppointExt = _fetchString(i_crm, "AppointExt"),
                        PaymentWay = _fetchString(i_crm, "PaymentWay"),
                        AppointDateTime = DateTime.Now,
                        CreateDate = DateTime.Now,
                        ModifyDate = DateTime.Now
                    };

                    //獲取Email郵件格式
                    var sTemplId = "Appoint_TE" + (i_crm.LANG == "en" ? "_en" : "");
                    var oEmailTempl = db.Queryable<OTB_SYS_Email>().Single(it => it.OrgID == i_crm.ORIGID && it.EmailID == sTemplId);

                    if (oEmailTempl != null)
                    {
                        //寄信開始
                        var sEmailBody = oEmailTempl.BodyHtml
                                           .Replace("{{:AppointNO}}", oPackingOrder.AppointNO)
                                           .Replace("{{:Unicode}}", oPackingOrder.Unicode)
                                           .Replace("{{:CompName}}", oPackingOrder.CompName)
                                           .Replace("{{:ExpoName}}", i_crm.LANG == "en" ? oExhibition.Exhibitioname_EN : oExhibition.Exhibitioname_TW)
                                           .Replace("{{:MuseumMumber}}", oPackingOrder.MuseumMumber)
                                           .Replace("{{:AppointUser}}", oPackingOrder.AppointUser)
                                           .Replace("{{:AppointTel}}", oPackingOrder.AppointTel)
                                           .Replace("{{:AppointExt}}", oPackingOrder.AppointExt)
                                           .Replace("{{:AppointEmail}}", oPackingOrder.AppointEmail)
                                           .Replace("{{:Contactor}}", oPackingOrder.Contactor)
                                           .Replace("{{:ContactTel}}", oPackingOrder.ContactTel)
                                           .Replace("{{:PaymentWay}}", oPackingOrder.PaymentWay == "1" ? "匯款" : "現場付現")
                                           .Replace("{{:ApproachTime}}", Convert.ToDateTime(oPackingOrder.ApproachTime).ToString("yyyy/MM/dd HH:mm"))
                                           .Replace("{{:ExitTime}}", Convert.ToDateTime(oPackingOrder.ExitTime).ToString("yyyy/MM/dd HH:mm"))
                                           .Replace("{{:Total}}", String.Format("{0:N0}", oPackingOrder.Total));

                        if (!string.IsNullOrEmpty(oExhibition.CostRulesId))
                        {
                            var oExhibitionRules = db.Queryable<OTB_WSM_ExhibitionRules>().Single(x => x.Guid == oExhibition.CostRulesId);
                            sEmailBody = sEmailBody.Replace("{{:ServiceInstruction}}", i_crm.LANG == "en" ? oExhibitionRules.ServiceInstruction_EN : oExhibitionRules.ServiceInstruction);
                        }

                        var doc = new HtmlDocument();
                        doc.LoadHtml(sEmailBody);

                        HtmlNode hService_Temple = null; //航班信息模版
                        foreach (HtmlNode NodeTb in doc.DocumentNode.SelectNodes("//tr"))        //按照<table>節點尋找
                        {
                            if (NodeTb.Attributes["data-repeat"] != null && NodeTb.Attributes["data-repeat"].Value == "Y")
                            {
                                hService_Temple = NodeTb;
                                var hReplace = HtmlNode.CreateNode("[servicetemple]");
                                NodeTb.ParentNode.InsertAfter(hReplace, NodeTb);
                                NodeTb.Remove();
                                break;
                            }
                        }
                        sEmailBody = doc.DocumentNode.OuterHtml;  //總模版
                        var sService_Html = "";
                        var sService_Temple = hService_Temple.OuterHtml;   //服務信息模板
                        var ja = (JArray)JsonConvert.DeserializeObject(oPackingOrder.PackingInfo);
                        var oExpoType_TW = new Map { { "01", "裸機" }, { "02", "木箱" }, { "03", "散貨" }, { "04", "打板" }, { "05", "其他" } };
                        var oExpoType_EN = new Map { { "01", "Unwrapped" }, { "02", "Wooden Crate" }, { "03", "Bulk Cargo" }, { "04", "Pallet" }, { "05", "Other" } };
                        var saService_TW = new List<string> { "堆高機服務", "拆箱", "裝箱", "空箱收送與儲存(展覽期間)" };
                        var saService_EN = new List<string> { "Forklift", "Unpacking", "Packing", "'Empty Crate Transport And StorageEmpty Crate Transport and Storage During the Exhibition" };
                        var builder = new System.Text.StringBuilder();
                        builder.Append(sService_Html);
                        foreach (JObject jo in ja)
                        {
                            var sExpoType = jo["ExpoType"].ToString();
                            var sExpoLen = jo["ExpoLen"].ToString();
                            var sExpoWidth = jo["ExpoWidth"].ToString();
                            var sExpoHeight = jo["ExpoHeight"].ToString();
                            var sExpoWeight = jo["ExpoWeight"].ToString();
                            var sExpoNumber = jo["ExpoNumber"].ToString();
                            var sExpoStack = jo["ExpoStack"].ToString();//堆高機
                            var sExpoSplit = jo["ExpoSplit"].ToString();//拆箱
                            var sExpoPack = jo["ExpoPack"].ToString();//裝箱
                            var sExpoFeed = jo["ExpoFeed"].ToString();//空箱收送與儲存(展覽期間)
                            var sSubTotal = jo["SubTotal"].ToString();
                            var dExpoLen = Convert.ToDecimal(sExpoLen == "" ? "0" : sExpoLen);
                            var dExpoWidth = Convert.ToDecimal(sExpoWidth == "" ? "0" : sExpoWidth);
                            var dExpoHeight = Convert.ToDecimal(sExpoHeight == "" ? "0" : sExpoHeight);
                            var dExpoWeight = Convert.ToDecimal(sExpoWeight == "" ? "0" : sExpoWeight);
                            var dSubTotal = Convert.ToDecimal(sSubTotal);
                            var saText = new List<string>();
                            if (sExpoStack == "True")
                            {
                                saText.Add(i_crm.LANG == "en" ? saService_EN[0].ToString() : saService_TW[0].ToString());
                            }
                            if (sExpoSplit == "True")
                            {
                                saText.Add(i_crm.LANG == "en" ? saService_EN[1].ToString() : saService_TW[1].ToString());
                            }
                            if (sExpoPack == "True")
                            {
                                saText.Add(i_crm.LANG == "en" ? saService_EN[2].ToString() : saService_TW[2].ToString());
                            }
                            if (sExpoFeed == "True")
                            {
                                saText.Add(i_crm.LANG == "en" ? saService_EN[3].ToString() : saService_TW[3].ToString());
                            }

                            builder.Append(sService_Temple
                                .Replace("{{:ExpoType}}", i_crm.LANG == "en" ? oExpoType_EN[sExpoType].ToString() : oExpoType_TW[sExpoType].ToString())
                                .Replace("{{:ExpoSize}}", String.Format("{0:N0}", dExpoLen) + "*" + String.Format("{0:N0}", dExpoWidth) + "*" + String.Format("{0:N0}", dExpoHeight))
                                .Replace("{{:ExpoWeight}}", String.Format("{0:N0}", dExpoWeight))
                                .Replace("{{:ExpoNumber}}", sExpoNumber)
                                .Replace("{{:ServiceText}}", string.Join("，", saText))
                                .Replace("{{:SubTotal}}", String.Format("{0:N0}", dSubTotal)));
                        }
                        sService_Html = builder.ToString();
                        sEmailBody = sEmailBody.Replace("[servicetemple]", sService_Html);

                        var oEmail = new Emails();
                        var saEmailTo = new List<EmailTo>();   //收件人
                        var oEmailTo = new EmailTo
                        {
                            ToUserID = "",
                            ToUserName = oPackingOrder.AppointUser,
                            ToEmail = oPackingOrder.AppointEmail,
                            Type = "to"
                        };
                        saEmailTo.Add(oEmailTo);
                        var sServiceEmail = Common.GetSystemSetting(db, i_crm.ORIGID, "ServiceEmail");
                        var saServiceEmail = sServiceEmail.Split(new string[] { @";", @",", @"，", @"|" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var _email in saServiceEmail)
                        {
                            var oEmailBc = new EmailTo
                            {
                                ToUserID = "",
                                ToUserName = _email,
                                ToEmail = _email,
                                Type = "bcc"
                            };
                            saEmailTo.Add(oEmailBc);
                        }

                        oEmail.FromUserName = i_crm.LANG == "en" ? "Online Booking" : "線上預約";
                        oEmail.Title = oEmailTempl.EmailSubject + (i_crm.LANG == "en" ? "(Booking No.：" : "（單號：") + oPackingOrder.AppointNO + ")";
                        oEmail.EmailBody = sEmailBody;
                        oEmail.IsCCSelf = false;
                        oEmail.Attachments = null;
                        oEmail.EmailTo = saEmailTo;

                        //bSend = new MailService(i_crm.ORIGID, true).MailFactory(oEmail, out sMsg);
                        bSend = new MailService(i_crm.ORIGID).MailFactory(oEmail, out sMsg);
                        if (bSend || oPackingOrder.AppointUser.IndexOf("***TEST***") > -1)
                        {
                            db.Insertable(oPackingOrder).ExecuteCommand();
                        }
                    }
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, bSend);
                    rm.DATA.Add("AppointNO", oPackingOrder.AppointNO);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WebSite.TE.TEAPIService", "", "Appoint（在線預約）", "", "", "");
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

        #endregion 在線預約

        #region 獲取預約明細

        /// <summary>
        /// 獲取預約明細
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetAppointInfo(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sAppointNO = _fetchString(i_crm, "AppointNO");
                    var sdb = new SimpleClient<OTB_WSM_PackingOrder>(db);
                    var oPackingOrder = sdb.GetById(sAppointNO);
                    var oRules = db.Queryable<OTB_WSM_ExhibitionRules, OTB_OPM_Exhibition>((t1, t2) => t1.OrgID == t2.OrgID && t1.Guid == t2.CostRulesId)
                        .Where((t1, t2) => t2.SN == int.Parse(oPackingOrder.ExhibitionNO))
                        .Select((t1, t2) => new { Info = t1, ExpoName = t2.Exhibitioname_TW })
                        .Single();
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, oPackingOrder);
                    rm.DATA.Add("rule", oRules);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WebSite.TE.TEAPIService", "", "GetAppointInfo(獲取預約明細)", "", "", "");
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

        #endregion 獲取預約明細
    }
}