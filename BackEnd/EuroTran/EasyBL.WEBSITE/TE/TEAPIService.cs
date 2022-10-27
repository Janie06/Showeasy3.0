using EasyBL.WebApi.Message;
using Entity;
using Entity.Sugar;
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
    }
}