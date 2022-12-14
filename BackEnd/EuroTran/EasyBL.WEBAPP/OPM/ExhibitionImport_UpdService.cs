using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using EasyBL.WebApi.Message;
using EasyNet;
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
using System.Threading;

namespace EasyBL.WEBAPP.OPM
{
    public class ExhibitionImport_UpdService : ServiceBase
    {
        #region 進口報價和預估成本提交審核

        /// <summary>
        /// 進口報價和預估成本提交審核
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on ToAuditForQuote</param>
        /// <returns></returns>
        public ResponseMessage ToAuditForQuote(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var data = i_crm.DATA as Dictionary<string, object>;
                        var sImportBillNO = _fetchString(i_crm, OTB_OPM_ImportExhibition.CN_IMPORTBILLNO);
                        var sSourceID = ((JObject)data[OTB_OPM_ImportExhibition.CN_QUOTE])[EasyNetGlobalConstWord.GUID].ToString();

                        var sdb = new SimpleClient<OTB_OPM_ImportExhibition>(db);
                        var oOpm = sdb.GetById(sImportBillNO);
                        if (oOpm == null)
                        {
                            sMsg = @"系統找不到對應的基本資料，請核查！";
                            break;
                        }

                        var sTitle = @"「進口」" + oOpm.ImportBillName + @"（" + oOpm.RefNumber + @"）" + @"「報價單/預估成本審核」";

                        if (i_crm.LANG == @"zh")
                        {
                            sTitle = ChineseStringUtility.ToSimplified(sTitle);
                        }
                        //更新基本資料
                        var oOpmUpd = new OTB_OPM_ImportExhibition
                        {
                            Quote = data[OTB_OPM_ImportExhibition.CN_QUOTE].ToString(),
                            EstimatedCost = data[OTB_OPM_ImportExhibition.CN_ESTIMATEDCOST].ToString(),
                            ModifyUser = i_crm.USERID,
                            ModifyDate = DateTime.Now
                        };
                        db.Updateable(oOpmUpd)
                            .UpdateColumns(it => new { it.Quote, it.EstimatedCost, it.ModifyUser, it.ModifyDate })
                            .Where(it => it.ImportBillNO == sImportBillNO).ExecuteCommand();
                        //更新代辦
                        SYS.Task_QryService.TaskStatusUpd(db, i_crm.ORIGID, sSourceID);

                        var saSupervisor = new List<string>();
                        var member = db.Queryable<OTB_SYS_Members>().Single(x => x.OrgID == i_crm.ORIGID && x.MemberID == i_crm.USERID);
                        if (member.MemberID != null)
                        {
                            if (!string.IsNullOrEmpty(member.ImmediateSupervisor))
                            {
                                saSupervisor.Add(member.ImmediateSupervisor);
                            }

                            var oDept = db.Queryable<OTB_SYS_Departments>().Single(it => it.OrgID == i_crm.ORIGID && it.DepartmentID == member.DepartmentID);
                            if (oDept != null)
                            {
                                saSupervisor.Add(oDept.ChiefOfDepartmentID);
                            }
                        }
                        var sBillAuditor = Common.GetSystemSetting(db, i_crm.ORIGID, WebAppGlobalConstWord.BILLAUDITOR);
                        if (sBillAuditor != @"")
                        {
                            var saBillAuditor = sBillAuditor.Split(new string[] { @";", @",", @"，", @"|" }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string auditor in saBillAuditor)
                            {
                                saSupervisor.Add(auditor);
                            }
                        }

                        saSupervisor = saSupervisor.Distinct<string>().ToList();
                        var listTips = new List<OTB_SYS_Tips>();
                        var listTask = new List<OTB_SYS_Task>();

                        if (saSupervisor.Count > 0)
                        {
                            foreach (string supervisor in saSupervisor)
                            {
                                //添加提醒消息
                                var oTipsAdd = SYS.Task_QryService.TipsAdd(i_crm, sTitle, supervisor, i_crm.CUSTOMDATA[@"program_id"] + @"|?Action=Upd&GoTab=3&ImportBillNO=" + oOpm.ImportBillNO, WebAppGlobalConstWord.BELL);
                                listTips.Add(oTipsAdd);
                                //添加代辦
                                var oTaskAdd = SYS.Task_QryService.TaskAdd(i_crm, sSourceID, supervisor, sTitle, @"ExhibitionImport_Qry", @"?Action=Upd&GoTab=3&ImportBillNO=" + oOpm.ImportBillNO);
                                listTask.Add(oTaskAdd);
                            }
                        }
                        else
                        {
                            sMsg = @"您沒有對應的直屬主管或部門主管，請核查！";
                            break;
                        }

                        if (listTips.Count > 0)
                        {
                            db.Insertable(listTips).ExecuteCommand();
                        }
                        if (listTask.Count > 0)
                        {
                            db.Insertable(listTask).ExecuteCommand();
                        }

                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, saSupervisor);
                    } while (false);
                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ExhibitionImport_UpdService), @"進口", @"ToAuditForQuote（進口報價和預估成本提交審核）", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
                Logger.Debug(@"ExhibitionImport_UpdService.ToAuditForQuote Debug（Param：" + JsonToString(i_crm) + @"；Response：" + JsonToString(rm) + @"）------------------");
            }
            return rm;
        }

        #endregion 進口報價和預估成本提交審核

        #region 進口報價和預估成本（主管）審核

        /// <summary>
        /// 進口報價和預估成本（主管）審核
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on AuditForQuote</param>
        /// <returns></returns>
        public ResponseMessage AuditForQuote(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var data = i_crm.DATA as Dictionary<string, object>;
                        var sImportBillNO = _fetchString(i_crm, OTB_OPM_ImportExhibition.CN_IMPORTBILLNO);
                        var sSourceID = ((JObject)data[OTB_OPM_ImportExhibition.CN_QUOTE])[EasyNetGlobalConstWord.GUID].ToString();
                        var TipsType = WebAppGlobalConstWord.CHECK;

                        var sdb = new SimpleClient<OTB_OPM_ImportExhibition>(db);
                        var oOpm = sdb.GetById(sImportBillNO);
                        if (oOpm == null)
                        {
                            sMsg = @"系統找不到對應的基本資料，請核查！";
                            break;
                        }

                        var member = db.Queryable<OTB_SYS_Members>().Single(x => x.OrgID == i_crm.ORIGID && x.MemberID == i_crm.USERID);
                        var sTitle = member.MemberName + @"審核了您的「進口」活動名稱：" + oOpm.ImportBillName + @"）的「報價單/預估成本審核」";

                        //更新報價和預估成本
                        var oOpmUpd = new OTB_OPM_ImportExhibition
                        {
                            Quote = data[OTB_OPM_ImportExhibition.CN_QUOTE].ToString(),
                            EstimatedCost = data[OTB_OPM_ImportExhibition.CN_ESTIMATEDCOST].ToString(),
                            Bills = data[OTB_OPM_ImportExhibition.CN_BILLS].ToString(),
                            ModifyUser = i_crm.USERID,
                            ModifyDate = DateTime.Now
                        };
                        var sAuditVal = ((JObject)data[OTB_OPM_ImportExhibition.CN_QUOTE])[@"AuditVal"].ToString();
                        if (sAuditVal == @"2")
                        {
                            sTitle += @"，審核結果：通過";
                        }
                        else
                        {
                            sTitle += @"，審核結果：不通過";
                            TipsType = WebAppGlobalConstWord.FAIL;
                        }

                        if (i_crm.LANG == @"zh")
                        {
                            sTitle = ChineseStringUtility.ToSimplified(sTitle);
                        }
                        db.Updateable(oOpmUpd)
                            .UpdateColumns(it => new { it.Quote, it.EstimatedCost, it.Bills, it.ModifyUser, it.ModifyDate })
                            .Where(it => it.ImportBillNO == sImportBillNO).ExecuteCommand();

                        //更新代辦
                        SYS.Task_QryService.TaskStatusUpd(db, i_crm.ORIGID, sSourceID);

                        //添加提醒消息
                        var oTipsAdd = SYS.Task_QryService.TipsAdd(i_crm, sTitle, oOpm.ResponsiblePerson, i_crm.CUSTOMDATA[@"program_id"] + @"|?Action=Upd&GoTab=3&ImportBillNO=" + oOpm.ImportBillNO, TipsType);
                        db.Insertable(oTipsAdd).ExecuteCommand();
                        if (sAuditVal != @"2")
                        {
                            //添加代辦
                            var oTaskAdd = SYS.Task_QryService.TaskAdd(i_crm, sSourceID, oOpm.ResponsiblePerson, sTitle, @"ExhibitionImport_Qry", @"?Action=Upd&GoTab=3&ImportBillNO=" + oOpm.ImportBillNO);
                            db.Insertable(oTaskAdd).ExecuteCommand();
                        }

                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, oOpm.ResponsiblePerson);
                    } while (false);
                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ExhibitionImport_UpdService), @"進口", @"AuditForQuote（進口報價和預估成本（主管）審核）", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
                Logger.Debug(@"ExhibitionImport_UpdService.AuditForQuote Debug（Param：" + JsonToString(i_crm) + @"；Response：" + JsonToString(rm) + @"）------------------");
            }
            return rm;
        }

        #endregion 進口報價和預估成本（主管）審核

        #region 進口帳單提交審核

        /// <summary>
        /// 進口帳單提交審核
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on ToAuditForBill</param>
        /// <returns></returns>
        public ResponseMessage ToAuditForBill(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var data = i_crm.DATA as Dictionary<string, object>;
                        var oBill = (JObject)data[@"Bill"];
                        var sImportBillNO = _fetchString(i_crm, OTB_OPM_ImportExhibition.CN_IMPORTBILLNO);
                        var sSourceID = oBill[EasyNetGlobalConstWord.GUID].ToString();
                        var sBillNO = oBill[@"BillNO"].ToString();

                        var sdb = new SimpleClient<OTB_OPM_ImportExhibition>(db);
                        var oOpm = sdb.GetById(sImportBillNO);
                        if (oOpm == null)
                        {
                            sMsg = @"系統找不到對應的基本資料，請核查！";
                            break;
                        }

                        var sTitle = @"「進口」" + oOpm.ImportBillName + @"帳單（" + sBillNO + @"）" + @"審核";
                        if (i_crm.LANG == @"zh")
                        {
                            sTitle = ChineseStringUtility.ToSimplified(sTitle);
                        }
                        //更新帳單
                        var oOpmUpd = new OTB_OPM_ImportExhibition
                        {
                            Bills = data[OTB_OPM_ImportExhibition.CN_BILLS].ToString(),
                            ModifyUser = i_crm.USERID,
                            ModifyDate = DateTime.Now
                        };
                        db.Updateable(oOpmUpd).UpdateColumns(it => new { it.Bills, it.ModifyUser, it.ModifyDate })
                            .Where(it => it.ImportBillNO == sImportBillNO).ExecuteCommand();

                        //更新代辦
                        SYS.Task_QryService.TaskStatusUpd(db, i_crm.ORIGID, sSourceID);

                        var saSupervisor = new List<string>();
                        var member = db.Queryable<OTB_SYS_Members>().Single(x => x.OrgID == i_crm.ORIGID && x.MemberID == i_crm.USERID);
                        if (member.MemberID != null)
                        {
                            if (!string.IsNullOrEmpty(member.ImmediateSupervisor))
                            {
                                saSupervisor.Add(member.ImmediateSupervisor);
                            }

                            var oDept = db.Queryable<OTB_SYS_Departments>().Single(it => it.OrgID == i_crm.ORIGID && it.DepartmentID == member.DepartmentID);
                            if (oDept != null)
                            {
                                saSupervisor.Add(oDept.ChiefOfDepartmentID);
                            }
                        }
                        var sBillAuditor = Common.GetSystemSetting(db, i_crm.ORIGID, WebAppGlobalConstWord.BILLAUDITOR);
                        if (sBillAuditor != @"")
                        {
                            var saBillAuditor = sBillAuditor.Split(new string[] { @";", @",", @"，", @"|" }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string auditor in saBillAuditor)
                            {
                                saSupervisor.Add(auditor);
                            }
                        }

                        saSupervisor = saSupervisor.Distinct<string>().ToList();
                        var listTips = new List<OTB_SYS_Tips>();
                        var listTask = new List<OTB_SYS_Task>();

                        if (saSupervisor.Count > 0)
                        {
                            foreach (string supervisor in saSupervisor)
                            {
                                //添加提醒消息
                                var oTipsAdd = SYS.Task_QryService.TipsAdd(i_crm, sTitle, supervisor, i_crm.CUSTOMDATA[@"program_id"] + @"|?Action=Upd&GoTab=3&ImportBillNO=" + oOpm.ImportBillNO + @"&BillNO=" + sBillNO, WebAppGlobalConstWord.BELL);
                                listTips.Add(oTipsAdd);
                                //添加代辦
                                var oTaskAdd = SYS.Task_QryService.TaskAdd(i_crm, sSourceID, supervisor, sTitle, @"ExhibitionImport_Qry", @"?Action=Upd&GoTab=3&ImportBillNO=" + oOpm.ImportBillNO + @"&BillNO=" + sBillNO);
                                listTask.Add(oTaskAdd);
                            }
                        }
                        else
                        {
                            sMsg = @"您沒有對應的直屬主管或部門主管，請核查！";
                            break;
                        }

                        if (listTips.Count > 0)
                        {
                            db.Insertable(listTips).ExecuteCommand();
                        }
                        if (listTask.Count > 0)
                        {
                            db.Insertable(listTask).ExecuteCommand();
                        }

                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, saSupervisor);
                    } while (false);
                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ExhibitionImport_UpdService), @"進口", @"ToAuditForBill（進口帳單提交審核）", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
                Logger.Debug(@"ExhibitionImport_UpdService.ToAuditForBill Debug（Param：" + JsonToString(i_crm) + @"；Response：" + JsonToString(rm) + @"）------------------");
            }
            return rm;
        }

        #endregion 進口帳單提交審核

        #region 進口帳單（主管）審核

        /// <summary>
        /// 進口帳單（主管）審核
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on AuditForBill</param>
        /// <returns></returns>
        public ResponseMessage AuditForBill(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var data = i_crm.DATA as Dictionary<string, object>;
                        var oBill = (JObject)data[@"Bill"];
                        var pm = new Dictionary<string, object>();
                        var sImportBillNO = _fetchString(i_crm, OTB_OPM_ImportExhibition.CN_IMPORTBILLNO);
                        var sBillNO = oBill[@"BillNO"].ToString();
                        var sSourceID = oBill[EasyNetGlobalConstWord.GUID].ToString();
                        var TipsType = WebAppGlobalConstWord.CHECK;

                        var sdb = new SimpleClient<OTB_OPM_ImportExhibition>(db);
                        var oOpm = sdb.GetById(sImportBillNO);
                        if (oOpm == null)
                        {
                            sMsg = @"系統找不到對應的基本資料，請核查！";
                            break;
                        }

                        var oPayer = new OTB_CRM_Customers();
                        if (!string.IsNullOrEmpty(oBill[@"Payer"].ToString()))
                        {
                            oPayer = db.Queryable<OTB_CRM_Customers>().Single(it => it.guid == oBill[@"Payer"].ToString());
                            if (oPayer == null)
                            {
                                sMsg = @"系統找不到付款人資訊";
                                break;
                            }
                        }

                        var member = db.Queryable<OTB_SYS_Members>().Single(x => x.OrgID == i_crm.ORIGID && x.MemberID == i_crm.USERID);
                        var sTitle = member.MemberName + @"審核了您的「進口」（活動名稱：" + oOpm.ImportBillName + @"）的「帳單（" + sBillNO + @"）」";

                        var sAuditVal = oBill[@"AuditVal"].ToString();
                        if (sAuditVal == @"2")
                        {
                            sTitle += @"，審核結果：通過";
                        }
                        else
                        {
                            sTitle += @"，審核結果：不通過";
                            TipsType = WebAppGlobalConstWord.FAIL;
                        }
                        if (i_crm.LANG == @"zh")
                        {
                            sTitle = ChineseStringUtility.ToSimplified(sTitle);
                        }
                        //更新帳單
                        var oOpmUpd = new OTB_OPM_ImportExhibition
                        {
                            Bills = data[OTB_OPM_ImportExhibition.CN_BILLS].ToString(),
                            ModifyUser = i_crm.USERID,
                            ModifyDate = DateTime.Now
                        };
                        db.Updateable(oOpmUpd).UpdateColumns(it => new { it.Bills, it.ModifyUser, it.ModifyDate })
                            .Where(it => it.ImportBillNO == sImportBillNO).ExecuteCommand();

                        //更新代辦
                        SYS.Task_QryService.TaskStatusUpd(db, i_crm.ORIGID, sSourceID);

                        //添加提醒消息
                        var oTipsAdd = SYS.Task_QryService.TipsAdd(i_crm, sTitle, oOpm.ResponsiblePerson, i_crm.CUSTOMDATA[@"program_id"] + @"|?Action=Upd&GoTab=3&ImportBillNO=" + oOpm.ImportBillNO + @"&BillNO=" + sBillNO, TipsType);
                        db.Insertable(oTipsAdd).ExecuteCommand();
                        if (sAuditVal != @"2")
                        {
                            //添加代辦
                            var oTaskAdd = SYS.Task_QryService.TaskAdd(i_crm, sSourceID, oOpm.ResponsiblePerson, sTitle, @"ExhibitionImport_Qry", @"?Action=Upd&GoTab=3&ImportBillNO=" + oOpm.ImportBillNO + @"&BillNO=" + sBillNO);
                            db.Insertable(oTaskAdd).ExecuteCommand();
                        }

                        if (sAuditVal == @"2")
                        {
                            var oBillsAdd = new OTB_OPM_Bills
                            {
                                OrgID = i_crm.ORIGID,
                                BillNO = oBill[@"BillNO"].ToString(), //帳款號碼(1)
                                CheckDate = Common.FnToTWDate(oBill[@"BillFirstCheckDate"]),//對帳日期(2)
                                BillType = @"20",//帳別(收付)(3)
                                CustomerCode = oPayer.CustomerNO //客戶供應商代號(4)
                            };
                            var sResponsiblePerson = oOpm.ResponsiblePerson.Split('.')[0];
                            oBillsAdd.ResponsiblePersonCode = Common.CutByteString(sResponsiblePerson, 11); //業務員代號(5)
                            oBillsAdd.ResponsiblePersonFullCode = oOpm.ResponsiblePerson; //業務員全代號
                            oBillsAdd.LastGetBillDate = @""; //最近收付日(6)
                            oBillsAdd.LastGetBillNO = @""; //最近收付單號(7)
                            oBillsAdd.TaxType = decimal.Parse(oBill[@"TaxSum"].ToString().Replace(@",", @"")) > 0 ? @"5" : @"6";//稅別(8)
                            oBillsAdd.NOTaxAmount = oBill[@"AmountSum"].ToString(); //未稅金額(9)
                            oBillsAdd.BillAmount = oBill[@"AmountTaxSum"].ToString(); //帳款金額(10)
                            oBillsAdd.PaymentAmount = @"0"; //收付金額(11)
                            oBillsAdd.Allowance = @"0"; //折讓金額(12)
                            oBillsAdd.DebtAmount = @"0"; //呆帳金額(13)
                            oBillsAdd.ExchangeAmount = @"0"; //匯兌損益金額(14)
                            oBillsAdd.Settle = @"N"; //結清否(15)
                            oBillsAdd.InvoiceStartNumber = @""; //發票號碼(起)(16)
                            oBillsAdd.InvoiceEndNumber = @"";//發票號碼(迄)(17)
                            oBillsAdd.Category = @"";//傳票類別(18)
                            oBillsAdd.OrderNo = @"";//訂單單號(19)
                            oBillsAdd.ClosingNote = @"N"; //結帳註記(20)
                            oBillsAdd.GeneralInvoiceNumber = @""; //立帳總帳傳票號碼(21)
                            oBillsAdd.GeneralSerialNumber = @""; //立帳總帳傳票序號(22)
                            oBillsAdd.Remark1 = @""; //備註一(30C)(23)
                            oBillsAdd.AccountSource = @"0"; //帳款來源(24)
                            oBillsAdd.UpdateDate = @""; //更新日期(25)
                            oBillsAdd.UpdatePersonnel = @""; //更新人員(26)
                            oBillsAdd.DepartmentSiteNumber = oOpm.DepartmentID; //部門\ 工地編號(27)
                            if (!string.IsNullOrWhiteSpace(oOpm.ExhibitionNO))
                            {
                                var oExhibition = db.Queryable<OTB_OPM_Exhibition>().Single(it => it.SN == oOpm.ExhibitionNO.ObjToInt());
                                oBillsAdd.ProjectNumber = oExhibition == null ? @"" : oExhibition.ExhibitionCode; //專案\ 項目編號(28)
                            }
                            else
                            {
                                oBillsAdd.ProjectNumber = @""; //專案\ 項目編號(28)
                            }
                            oBillsAdd.TransferBNotes = @""; //轉B 帳註記(29)
                            oBillsAdd.ABNumber = @""; //A|B 帳唯一流水號(30)
                            oBillsAdd.EnterNumber = @""; //進銷單號(31)
                            var sCurrency = oBill[@"Currency"].ToString();
                            if (i_crm.ORIGID == "SG")
                            {
                                if (sCurrency == "RMB")
                                {
                                    sCurrency = "";
                                }
                            }
                            else
                            {
                                if (sCurrency == "NTD")
                                {
                                    sCurrency = "";
                                }
                            }
                            oBillsAdd.ForeignCurrencyCode = sCurrency; //外幣代號(32)
                            var sExchangeRate = oBill[@"ExchangeRate"].ToString();
                            oBillsAdd.ExchangeRate = sExchangeRate; //匯率(33)
                            oBillsAdd.ForeignAmount = (decimal.Parse(oBill[@"AmountTaxSum"].ToString()) * decimal.Parse(sExchangeRate == @"" ? @"1" : sExchangeRate)).ToString(); //外幣金額(34)
                            oBillsAdd.PayAmount = @"0"; //收付沖抵金額(35)
                            oBillsAdd.RefundAmount = @"0"; //退款金額(36)
                            oBillsAdd.PaymentTerms = @""; //收付條件(37)
                            oBillsAdd.AccountDate = Common.FnToTWDate(oBill[@"BillFirstCheckDate"]); //帳款日期(38)
                            oBillsAdd.DCreditCardNumber = @""; //預設信用卡號(39)
                            oBillsAdd.ClosingDate = @""; //結帳日期(40)
                            oBillsAdd.CusField1 = @""; //自定義欄位一(41)
                            oBillsAdd.CusField2 = @""; //自定義欄位二(42)
                            oBillsAdd.CusField3 = @""; //自定義欄位三(43)
                            oBillsAdd.CusField4 = @""; //自定義欄位四(44)
                            oBillsAdd.CusField5 = @""; //自定義欄位五(45)
                            oBillsAdd.CusField6 = @"0"; //自定義欄位六(46)
                            oBillsAdd.CusField7 = @"0"; //自定義欄位七(47)
                            oBillsAdd.CusField8 = @"0"; //自定義欄位八(48)
                            oBillsAdd.CusField9 = @"0"; //自定義欄位九(49)
                            oBillsAdd.CusField10 = @"0";  //自定義欄位十(50)
                            oBillsAdd.CusField11 = @""; //自定義欄位十一(51)
                            oBillsAdd.CusField12 = @""; //自定義欄位十二(52)
                            oBillsAdd.Remark2 = @""; //備註二(M)(53)
                            oBillsAdd.TWNOTaxAmount = oBill[@"AmountSum"].ToString(); //台幣未稅金額(54)
                            oBillsAdd.TWAmount = oBill[@"AmountTaxSum"].ToString(); //台幣帳款金額(55)
                            oBillsAdd.CreateUser = i_crm.USERID;
                            oBillsAdd.CreateDate = oBill[@"CreateDate"] == null ? DateTime.Now.ToString(@"yyyy/MM/dd HH:mm:ss") : oBill[@"CreateDate"].ToString();
                            oBillsAdd.BillFirstCheckDate = oBill[@"BillFirstCheckDate"] == null ? DateTime.Now.ToString(@"yyyy/MM/dd HH:mm:ss") : oBill[@"BillFirstCheckDate"].ToString();
                            oBillsAdd.Advance = oBill[@"Advance"].ToString(); //預收
                            oBillsAdd.TaxSum = oBill[@"TaxSum"].ToString(); //稅額
                            oBillsAdd.TotalReceivable = oBill[@"TotalReceivable"].ToString(); //總應收
                            oBillsAdd.IsRetn = oBill[@"IsRetn"] == null ? @"N" : oBill[@"IsRetn"].ToString(); //是否為退運
                            oBillsAdd.Url = i_crm.CUSTOMDATA[@"program_id"] + @"|?Action=Upd&GoTab=3&ImportBillNO=" + oOpm.ImportBillNO + @"&BillNO=" + sBillNO; //Url
                            db.Insertable(oBillsAdd).ExecuteCommand();
                        }

                        var oLogInfo = new OTB_SYS_LogInfo
                        {
                            OrgID = i_crm.ORIGID,
                            SouseId = sBillNO,
                            LogType = "billnoupdate",
                            LogInfo = data[@"Bill"].ToString(),
                            CreateUser = i_crm.USERID,
                            CreateDate = DateTime.Now,
                            Memo = "「進口帳單」（主管）審核"
                        };
                        db.Insertable(oLogInfo).ExecuteCommand();

                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, oOpm.ResponsiblePerson);
                    } while (false);
                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ExhibitionImport_UpdService), @"進口", @"ToAuditForBill（進口帳單（主管）審核）", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
                Logger.Debug(@"ExhibitionImport_UpdService.ToAuditForBill Debug（Param：" + JsonToString(i_crm) + @"；Response：" + JsonToString(rm) + @"）------------------");
            }
            return rm;
        }

        #endregion 進口帳單（主管）審核

        #region 進口帳單（會計）審核

        /// <summary>
        /// 進口帳單（會計）審核
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on AccountAuditForBill</param>
        /// <returns></returns>
        public ResponseMessage AccountAuditForBill(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var data = i_crm.DATA as Dictionary<string, object>;
                        var oBill = (JObject)data[@"Bill"];
                        var sImportBillNO = _fetchString(i_crm, OTB_OPM_ImportExhibition.CN_IMPORTBILLNO);
                        var sSourceID = oBill[EasyNetGlobalConstWord.GUID].ToString();
                        var sBillNO = oBill[@"BillNO"].ToString();

                        var sdb = new SimpleClient<OTB_OPM_ImportExhibition>(db);
                        var oOpm = sdb.GetById(sImportBillNO);
                        if (oOpm == null)
                        {
                            sMsg = @"系統找不到對應的基本資料，請核查！";
                            break;
                        }

                        var member = db.Queryable<OTB_SYS_Members>().Single(x => x.OrgID == i_crm.ORIGID && x.MemberID == i_crm.USERID);
                        var sTitle = @"會計（" + member.MemberName + @"）審核了您的「進口」（活動名稱：" + oOpm.ImportBillName + @"）的「帳單(" + sBillNO + @")」";
                        if (i_crm.LANG == @"zh")
                        {
                            sTitle = ChineseStringUtility.ToSimplified(sTitle);
                        }
                        //更新帳單
                        var oOpmUpd = new OTB_OPM_ImportExhibition
                        {
                            Bills = data[OTB_OPM_ImportExhibition.CN_BILLS].ToString(),
                            ModifyUser = i_crm.USERID,
                            ModifyDate = DateTime.Now
                        };
                        db.Updateable(oOpmUpd).UpdateColumns(it => new { it.Bills, it.ModifyUser, it.ModifyDate })
                            .Where(it => it.ImportBillNO == sImportBillNO).ExecuteCommand();

                        //更新代辦
                        SYS.Task_QryService.TaskStatusUpd(db, i_crm.ORIGID, sSourceID);

                        //添加提醒消息
                        var oTipsAdd = SYS.Task_QryService.TipsAdd(i_crm, sTitle, oOpm.ResponsiblePerson, i_crm.CUSTOMDATA[@"program_id"] + @"|?Action=Upd&GoTab=3&ImportBillNO=" + oOpm.ImportBillNO + @"&BillNO=" + sBillNO, WebAppGlobalConstWord.CHECK);
                        db.Insertable(oTipsAdd).ExecuteCommand();

                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, oOpm.ResponsiblePerson);
                    } while (false);
                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ExhibitionImport_UpdService), @"進口", @"AccountAuditForBill（進口帳單（會計）審核）", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
                Logger.Debug(@"ExhibitionImport_UpdService.AccountAuditForBill Debug（Param：" + JsonToString(i_crm) + @"；Response：" + JsonToString(rm) + @"）------------------");
            }
            return rm;
        }

        #endregion 進口帳單（會計）審核

        #region （會計）取消審核

        /// <summary>
        /// （會計）取消審核
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on CancelAudit</param>
        /// <returns></returns>
        public ResponseMessage CancelAudit(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var data = i_crm.DATA as Dictionary<string, object>;
                        var oBill = (JObject)data[@"Bill"];
                        var sImportBillNO = _fetchString(i_crm, OTB_OPM_ImportExhibition.CN_IMPORTBILLNO);
                        var sBillNO = oBill[@"BillNO"].ToString();
                        var sSourceID = oBill[EasyNetGlobalConstWord.GUID].ToString();

                        var sdb = new SimpleClient<OTB_OPM_ImportExhibition>(db);
                        var oOpm = sdb.GetById(sImportBillNO);
                        if (oOpm == null)
                        {
                            sMsg = @"系統找不到對應的基本資料，請核查！";
                            break;
                        }

                        var LogData = _fetchString(i_crm, @"LogData");
                        var LogResult = BillLogs.InsertBillChangeLog(db, LogData, ActionType.取消審核, i_crm.ORIGID, i_crm.USERID);
                        if (!LogResult.Item1)
                        {
                            sMsg = LogResult.Item2;
                            break;
                        }
                        var member = db.Queryable<OTB_SYS_Members>().Single(x => x.OrgID == i_crm.ORIGID && x.MemberID == i_crm.USERID);
                        var sTitle = @"會計（" + member.MemberName + @"）取消審核了您的「進口」（活動名稱：" + oOpm.ImportBillName + @"）的帳單（" + sBillNO + @"）";
                        if (i_crm.LANG == @"zh")
                        {
                            sTitle = ChineseStringUtility.ToSimplified(sTitle);
                        }

                        //更新帳單
                        var oOpmUpd = new OTB_OPM_ImportExhibition
                        {
                            Bills = data[OTB_OPM_ImportExhibition.CN_BILLS].ToString(),
                            ModifyUser = i_crm.USERID,
                            ModifyDate = DateTime.Now
                        };
                        db.Updateable(oOpmUpd).UpdateColumns(it => new { it.Bills, it.ModifyUser, it.ModifyDate })
                            .Where(it => it.ImportBillNO == sImportBillNO).ExecuteCommand();

                        //更新代辦
                        SYS.Task_QryService.TaskStatusUpd(db, i_crm.ORIGID, sSourceID);

                        //添加提醒消息
                        var oTipsAdd = SYS.Task_QryService.TipsAdd(i_crm, sTitle, oOpm.ResponsiblePerson, i_crm.CUSTOMDATA[@"program_id"] + @"|?Action=Upd&GoTab=3&ImportBillNO=" + oOpm.ImportBillNO + @"&BillNO=" + sBillNO, WebAppGlobalConstWord.BELL);
                        db.Insertable(oTipsAdd).ExecuteCommand();
                        //添加代辦
                        var oTaskAdd = SYS.Task_QryService.TaskAdd(i_crm, sSourceID, oOpm.ResponsiblePerson, sTitle, @"ExhibitionImport_Qry", @"?Action=Upd&GoTab=3&ImportBillNO=" + oOpm.ImportBillNO + @"&BillNO=" + sBillNO);
                        db.Insertable(oTaskAdd).ExecuteCommand();

                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, oOpm.ResponsiblePerson);
                    } while (false);
                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ExhibitionImport_UpdService), @"進口", @"CancelAudit（（會計）取消審核）", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
                Logger.Debug(@"ExhibitionImport_UpdService.CancelAudit Debug（Param：" + JsonToString(i_crm) + @"；Response：" + JsonToString(rm) + @"）------------------");
            }
            return rm;
        }

        #endregion （會計）取消審核

        #region （會計）銷帳

        /// <summary>
        /// （會計）銷帳
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on WriteOff</param>
        /// <returns></returns>
        public ResponseMessage WriteOff(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var data = i_crm.DATA as Dictionary<string, object>;
                        var oBill = (JObject)data[@"Bill"];
                        var sImportBillNO = _fetchString(i_crm, OTB_OPM_ImportExhibition.CN_IMPORTBILLNO);
                        var sBillNO = oBill[@"BillNO"].ToString();

                        var sdb = new SimpleClient<OTB_OPM_ImportExhibition>(db);
                        var oOpm = sdb.GetById(sImportBillNO);
                        if (oOpm == null)
                        {
                            sMsg = @"系統找不到對應的基本資料，請核查！";
                            break;
                        }

                        var member = db.Queryable<OTB_SYS_Members>().Single(x => x.OrgID == i_crm.ORIGID && x.MemberID == i_crm.USERID);
                        var sTitle = @"會計（" + member.MemberName + @"）對您的「進口」（活動名稱：" + oOpm.ImportBillName + @"）的帳單（" + sBillNO + @"）進行了銷帳";
                        if (i_crm.LANG == @"zh")
                        {
                            sTitle = ChineseStringUtility.ToSimplified(sTitle);
                        }

                        //更新帳單
                        var oOpmUpd = new OTB_OPM_ImportExhibition
                        {
                            Bills = data[OTB_OPM_ImportExhibition.CN_BILLS].ToString(),
                            ModifyUser = i_crm.USERID,
                            ModifyDate = DateTime.Now
                        };
                        db.Updateable(oOpmUpd).UpdateColumns(it => new { it.Bills, it.ModifyUser, it.ModifyDate })
                            .Where(it => it.ImportBillNO == sImportBillNO).ExecuteCommand();

                        //添加提醒消息
                        var oTipsAdd = SYS.Task_QryService.TipsAdd(i_crm, sTitle, oOpm.ResponsiblePerson, i_crm.CUSTOMDATA[@"program_id"] + @"|?Action=Upd&GoTab=3&ImportBillNO=" + oOpm.ImportBillNO + @"&BillNO=" + sBillNO, WebAppGlobalConstWord.BELL);
                        db.Insertable(oTipsAdd).ExecuteCommand();

                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, oOpm.ResponsiblePerson);
                    } while (false);
                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ExhibitionImport_UpdService), @"進口", @"WriteOff（（會計）銷帳）", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
                Logger.Debug(@"ExhibitionImport_UpdService.WriteOff Debug（Param：" + JsonToString(i_crm) + @"；Response：" + JsonToString(rm) + @"）------------------");
            }
            return rm;
        }

        #endregion （會計）銷帳

        #region （會計）取消銷帳

        /// <summary>
        /// （會計）取消銷帳
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on CancelWriteOff</param>
        /// <returns></returns>
        public ResponseMessage CancelWriteOff(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var data = i_crm.DATA as Dictionary<string, object>;
                    var oBill = (JObject)data[@"Bill"];
                    var sImportBillNO = _fetchString(i_crm, OTB_OPM_ImportExhibition.CN_IMPORTBILLNO);
                    var sBillNO = oBill[@"BillNO"].ToString();

                    var sdb = new SimpleClient<OTB_OPM_ImportExhibition>(db);
                    var oOpm = sdb.GetById(sImportBillNO);
                    if (oOpm == null)
                    {
                        sMsg = @"系統找不到對應的基本資料，請核查！";
                        break;
                    }
                    var LogData = _fetchString(i_crm, @"LogData");
                    var LogResult = BillLogs.InsertBillChangeLog(db, LogData, ActionType.取消銷帳, i_crm.ORIGID, i_crm.USERID);
                    if (!LogResult.Item1)
                    {
                        sMsg = LogResult.Item2;
                        break;
                    }
                    //更新帳單
                    var oOpmUpd = new OTB_OPM_ImportExhibition
                    {
                        Bills = data[OTB_OPM_ImportExhibition.CN_BILLS].ToString(),
                        ModifyUser = i_crm.USERID,
                        ModifyDate = DateTime.Now
                    };
                    var iResult = db.Updateable(oOpmUpd).UpdateColumns(it => new { it.Bills, it.ModifyUser, it.ModifyDate })
                        .Where(it => it.ImportBillNO == sImportBillNO).ExecuteCommand();

                    if (iResult > 0)
                    {
                        rm = new SuccessResponseMessage(null, i_crm);
                    }
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ExhibitionImport_UpdService), @"進口", @"CancelWriteOff（（會計）取消銷帳）", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
                Logger.Debug(@"ExhibitionImport_UpdService.CancelWriteOff Debug（Param：" + JsonToString(i_crm) + @"；Response：" + JsonToString(rm) + @"）------------------");
            }
            return rm;
        }

        #endregion （會計）取消銷帳

        #region 過帳

        /// <summary>
        /// 過帳
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on BillPost</param>
        /// <returns></returns>
        public ResponseMessage BillPost(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var data = i_crm.DATA as Dictionary<string, object>;
                        var oBill = (JObject)data[@"Bill"];
                        var sImportBillNO = _fetchString(i_crm, OTB_OPM_ImportExhibition.CN_IMPORTBILLNO);
                        var sBillNO = oBill[@"BillNO"].ToString();

                        var sdb = new SimpleClient<OTB_OPM_ImportExhibition>(db);
                        var oOpm = sdb.GetById(sImportBillNO);
                        if (oOpm == null)
                        {
                            sMsg = @"系統找不到對應的基本資料，請核查！";
                            break;
                        }

                        var oPayer = new OTB_CRM_Customers();
                        if (!string.IsNullOrEmpty(oBill[@"Payer"].ToString()))
                        {
                            oPayer = db.Queryable<OTB_CRM_Customers>().Single(it => it.guid == oBill[@"Payer"].ToString());
                            if (oPayer == null)
                            {
                                sMsg = @"系統找不到付款人資訊";
                                break;
                            }
                        }

                        var sTitle = @"「進口」（活動名稱：" + oOpm.ImportBillName + @"）的帳單（" + sBillNO + @"）已過帳";
                        if (i_crm.LANG == @"zh")
                        {
                            sTitle = ChineseStringUtility.ToSimplified(sTitle);
                        }
                        //更新帳單
                        var oOpmUpd = new OTB_OPM_ImportExhibition
                        {
                            Bills = data[OTB_OPM_ImportExhibition.CN_BILLS].ToString(),
                            ModifyUser = i_crm.USERID,
                            ModifyDate = DateTime.Now
                        };
                        db.Updateable(oOpmUpd).UpdateColumns(it => new { it.Bills, it.ModifyUser, it.ModifyDate })
                           .Where(it => it.ImportBillNO == sImportBillNO).ExecuteCommand();

                        var listAccounts = db.Queryable<OTB_SYS_MembersToRule>()
                            .Where(it => it.OrgID == i_crm.ORIGID && it.RuleID == @"Account").ToList();
                        var listTips = new List<OTB_SYS_Tips>();
                        if (listAccounts.Count > 0)
                        {
                            foreach (OTB_SYS_MembersToRule account in listAccounts)
                            {
                                //添加提醒消息
                                var oTipsAdd = SYS.Task_QryService.TipsAdd(i_crm, sTitle, account.MemberID, i_crm.CUSTOMDATA[@"program_id"] + @"|?Action=Upd&GoTab=3&ImportBillNO=" + oOpm.ImportBillNO + @"&BillNO=" + sBillNO, WebAppGlobalConstWord.BELL);
                                listTips.Add(oTipsAdd);
                            }
                        }
                        if (listTips.Count > 0)
                        {
                            db.Insertable(listTips).ExecuteCommand();
                        }

                        var oBillsAdd = new OTB_OPM_Bills
                        {
                            OrgID = i_crm.ORIGID,
                            BillNO = oBill[@"BillNO"].ToString(), //帳款號碼(1)
                            CheckDate = Common.FnToTWDate(oBill[@"BillFirstCheckDate"]),//對帳日期(2)
                            BillType = @"20",//帳別(收付)(3)
                            CustomerCode = oPayer.CustomerNO //客戶供應商代號(4)
                        };
                        var sResponsiblePerson = oOpm.ResponsiblePerson.Split('.')[0];
                        oBillsAdd.ResponsiblePersonCode = Common.CutByteString(sResponsiblePerson, 11); //業務員代號(5)
                        oBillsAdd.ResponsiblePersonFullCode = oOpm.ResponsiblePerson; //業務員全代號
                        oBillsAdd.LastGetBillDate = @""; //最近收付日(6)
                        oBillsAdd.LastGetBillNO = @""; //最近收付單號(7)
                        oBillsAdd.TaxType = decimal.Parse(oBill[@"TaxSum"].ToString().Replace(@",", @"")) > 0 ? @"5" : @"6";//稅別(8)
                        oBillsAdd.NOTaxAmount = oBill[@"AmountSum"].ToString(); //未稅金額(9)
                        oBillsAdd.BillAmount = oBill[@"AmountTaxSum"].ToString(); //帳款金額(10)
                        oBillsAdd.PaymentAmount = @"0"; //收付金額(11)
                        oBillsAdd.Allowance = @"0"; //折讓金額(12)
                        oBillsAdd.DebtAmount = @"0"; //呆帳金額(13)
                        oBillsAdd.ExchangeAmount = @"0"; //匯兌損益金額(14)
                        oBillsAdd.Settle = @"N"; //結清否(15)
                        oBillsAdd.InvoiceStartNumber = oBill[@"InvoiceNumber"].ToString(); //發票號碼(起)(16)
                        oBillsAdd.InvoiceEndNumber = oBill[@"InvoiceNumber"].ToString();//發票號碼(迄)(17)
                        oBillsAdd.Category = @"";//傳票類別(18)
                        oBillsAdd.OrderNo = @"";//訂單單號(19)
                        oBillsAdd.ClosingNote = @"N"; //結帳註記(20)
                        oBillsAdd.GeneralInvoiceNumber = @""; //立帳總帳傳票號碼(21)
                        oBillsAdd.GeneralSerialNumber = @""; //立帳總帳傳票序號(22)
                        oBillsAdd.Remark1 = @""; //備註一(30C)(23)
                        oBillsAdd.AccountSource = @"0"; //帳款來源(24)
                        oBillsAdd.UpdateDate = @""; //更新日期(25)
                        oBillsAdd.UpdatePersonnel = @""; //更新人員(26)
                        oBillsAdd.DepartmentSiteNumber = oOpm.DepartmentID; //部門\ 工地編號(27)
                        if (!string.IsNullOrWhiteSpace(oOpm.ExhibitionNO))
                        {
                            var oExhibition = db.Queryable<OTB_OPM_Exhibition>().Single(it => it.SN == oOpm.ExhibitionNO.ObjToInt());
                            oBillsAdd.ProjectNumber = oExhibition == null ? @"" : oExhibition.ExhibitionCode; //專案\ 項目編號(28)
                        }
                        else
                        {
                            oBillsAdd.ProjectNumber = @""; //專案\ 項目編號(28)
                        }
                        oBillsAdd.TransferBNotes = @""; //轉B 帳註記(29)
                        oBillsAdd.ABNumber = @""; //A|B 帳唯一流水號(30)
                        oBillsAdd.EnterNumber = @""; //進銷單號(31)
                        var sCurrency = oBill[@"Currency"].ToString();
                        if (i_crm.ORIGID == "SG")
                        {
                            if (sCurrency == "RMB")
                            {
                                sCurrency = "";
                            }
                        }
                        else
                        {
                            if (sCurrency == "NTD")
                            {
                                sCurrency = "";
                            }
                        }
                        oBillsAdd.ForeignCurrencyCode = sCurrency; //外幣代號(32)
                        var sExchangeRate = oBill[@"ExchangeRate"].ToString();
                        oBillsAdd.ExchangeRate = sExchangeRate; //匯率(33)
                        oBillsAdd.ForeignAmount = (decimal.Parse(oBill[@"AmountTaxSum"].ToString()) * decimal.Parse(sExchangeRate == @"" ? @"1" : sExchangeRate)).ToString(); //外幣金額(34)
                        oBillsAdd.PayAmount = @"0"; //收付沖抵金額(35)
                        oBillsAdd.RefundAmount = @"0"; //退款金額(36)
                        oBillsAdd.PaymentTerms = @""; //收付條件(37)
                        oBillsAdd.AccountDate = Common.FnToTWDate(oBill[@"BillFirstCheckDate"]); //帳款日期(38)
                        oBillsAdd.DCreditCardNumber = @""; //預設信用卡號(39)
                        oBillsAdd.ClosingDate = @""; //結帳日期(40)
                        oBillsAdd.CusField1 = @""; //自定義欄位一(41)
                        oBillsAdd.CusField2 = @""; //自定義欄位二(42)
                        oBillsAdd.CusField3 = @""; //自定義欄位三(43)
                        oBillsAdd.CusField4 = @""; //自定義欄位四(44)
                        oBillsAdd.CusField5 = @""; //自定義欄位五(45)
                        oBillsAdd.CusField6 = @"0"; //自定義欄位六(46)
                        oBillsAdd.CusField7 = @"0"; //自定義欄位七(47)
                        oBillsAdd.CusField8 = @"0"; //自定義欄位八(48)
                        oBillsAdd.CusField9 = @"0"; //自定義欄位九(49)
                        oBillsAdd.CusField10 = @"0";  //自定義欄位十(50)
                        oBillsAdd.CusField11 = @""; //自定義欄位十一(51)
                        oBillsAdd.CusField12 = @""; //自定義欄位十二(52)
                        oBillsAdd.Remark2 = @""; //備註二(M)(53)
                        oBillsAdd.TWNOTaxAmount = oBill[@"AmountSum"].ToString(); //台幣未稅金額(54)
                        oBillsAdd.TWAmount = oBill[@"AmountTaxSum"].ToString(); //台幣帳款金額(55)
                        oBillsAdd.CreateUser = i_crm.USERID;
                        oBillsAdd.CreateDate = oBill[@"CreateDate"] == null ? DateTime.Now.ToString(@"yyyy/MM/dd HH:mm:ss") : oBill[@"CreateDate"].ToString();
                        oBillsAdd.BillFirstCheckDate = oBill[@"BillFirstCheckDate"] == null ? DateTime.Now.ToString(@"yyyy/MM/dd HH:mm:ss") : oBill[@"BillFirstCheckDate"].ToString();
                        oBillsAdd.Advance = oBill[@"Advance"].ToString(); //預收
                        oBillsAdd.TaxSum = oBill[@"TaxSum"].ToString(); //稅額
                        oBillsAdd.TotalReceivable = oBill[@"TotalReceivable"].ToString(); //總應收
                        oBillsAdd.IsRetn = oBill[@"IsRetn"] == null ? @"N" : oBill[@"IsRetn"].ToString(); //是否為退運
                        oBillsAdd.Url = i_crm.CUSTOMDATA[@"program_id"] + @"|?Action=Upd&GoTab=3&ImportBillNO=" + oOpm.ImportBillNO + @"&BillNO=" + sBillNO; //Url
                        db.Insertable(oBillsAdd).ExecuteCommand();

                        var oLogInfo = new OTB_SYS_LogInfo
                        {
                            OrgID = i_crm.ORIGID,
                            SouseId = sBillNO,
                            LogType = "billnoupdate",
                            LogInfo = data[@"Bill"].ToString(),
                            CreateUser = i_crm.USERID,
                            CreateDate = DateTime.Now,
                            Memo = "「進口帳單」（會計）過帳"
                        };
                        db.Insertable(oLogInfo).ExecuteCommand();

                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, oOpm.ResponsiblePerson);
                    } while (false);
                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ExhibitionImport_UpdService), @"進口", @"BillPost（過帳）", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
                Logger.Debug(@"ExhibitionImport_UpdService.BillPost Debug（Param：" + JsonToString(i_crm) + @"；Response：" + JsonToString(rm) + @"）------------------");
            }
            return rm;
        }

        #endregion 過帳

        #region 取消過帳

        /// <summary>
        /// 取消過帳
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on BillCancelPost</param>
        /// <returns></returns>
        public ResponseMessage BillCancelPost(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var data = i_crm.DATA as Dictionary<string, object>;
                        var oBill = (JObject)data[@"Bill"];
                        var sImportBillNO = _fetchString(i_crm, OTB_OPM_ImportExhibition.CN_IMPORTBILLNO);
                        var sBillNO = oBill[@"BillNO"].ToString();

                        var sdb = new SimpleClient<OTB_OPM_ImportExhibition>(db);
                        var oOpm = sdb.GetById(sImportBillNO);
                        if (oOpm == null)
                        {
                            sMsg = @"系統找不到對應的基本資料，請核查！";
                            break;
                        }
                        var LogData = _fetchString(i_crm, @"LogData");
                        var LogResult = BillLogs.InsertBillChangeLog(db, LogData, ActionType.取消過帳, i_crm.ORIGID, i_crm.USERID);
                        if (!LogResult.Item1)
                        {
                            sMsg = LogResult.Item2;
                            break;
                        }
                        var member = db.Queryable<OTB_SYS_Members>().Single(x => x.OrgID == i_crm.ORIGID && x.MemberID == i_crm.USERID);
                        var sTitle = @"會計（" + member.MemberName + @"）對您的「進口」（活動名稱：" + oOpm.ImportBillName + @"）的帳單（" + sBillNO + @"）取消了過帳";
                        if (i_crm.LANG == @"zh")
                        {
                            sTitle = ChineseStringUtility.ToSimplified(sTitle);
                        }

                        //更新帳單
                        var oOpmUpd = new OTB_OPM_ImportExhibition
                        {
                            Bills = data[OTB_OPM_ImportExhibition.CN_BILLS].ToString(),
                            ModifyUser = i_crm.USERID,
                            ModifyDate = DateTime.Now
                        };
                        db.Updateable(oOpmUpd).UpdateColumns(it => new { it.Bills, it.ModifyUser, it.ModifyDate })
                            .Where(it => it.ImportBillNO == sImportBillNO).ExecuteCommand();

                        //添加提醒消息
                        var oTipsAdd = SYS.Task_QryService.TipsAdd(i_crm, sTitle, oOpm.ResponsiblePerson, i_crm.CUSTOMDATA[@"program_id"] + @"|?Action=Upd&GoTab=3&ImportBillNO=" + oOpm.ImportBillNO + @"&BillNO=" + sBillNO, WebAppGlobalConstWord.BELL);
                        db.Insertable(oTipsAdd).ExecuteCommand();

                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, oOpm.ResponsiblePerson);
                    } while (false);
                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ExhibitionImport_UpdService), @"進口", @"BillCancelPost（取消過帳）", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
                Logger.Debug(@"ExhibitionImport_UpdService.BillCancelPost Debug（Param：" + JsonToString(i_crm) + @"；Response：" + JsonToString(rm) + @"）------------------");
            }
            return rm;
        }

        #endregion 取消過帳

        #region 作廢帳單

        /// <summary>
        /// 作廢帳單
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on BillVoid</param>
        /// <returns></returns>
        public ResponseMessage BillVoid(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var data = i_crm.DATA as Dictionary<string, object>;
                        var oBill = (JObject)data[@"Bill"];
                        var sImportBillNO = _fetchString(i_crm, OTB_OPM_ImportExhibition.CN_IMPORTBILLNO);
                        var sBillNO = oBill[@"BillNO"].ToString();
                        var sSourceID = oBill[EasyNetGlobalConstWord.GUID].ToString();

                        var sdb = new SimpleClient<OTB_OPM_ImportExhibition>(db);
                        var oOpm = sdb.GetById(sImportBillNO);
                        if (oOpm == null)
                        {
                            sMsg = @"系統找不到對應的基本資料，請核查！";
                            break;
                        }
                        var LogData = _fetchString(i_crm, @"LogData");
                        var LogResult = BillLogs.InsertBillChangeLog(db, LogData, ActionType.帳單作廢, i_crm.ORIGID, i_crm.USERID);
                        if (!LogResult.Item1)
                        {
                            sMsg = LogResult.Item2;
                            break;
                        }
                        var member = db.Queryable<OTB_SYS_Members>().Single(x => x.OrgID == i_crm.ORIGID && x.MemberID == i_crm.USERID);
                        var sTitle = @"會計（" + member.MemberName + @"）作廢了您的「進口」（活動名稱：" + oOpm.ImportBillName + @"）的帳單（" + sBillNO + @"）";
                        if (i_crm.LANG == @"zh")
                        {
                            sTitle = ChineseStringUtility.ToSimplified(sTitle);
                        }

                        //更新帳單
                        var oOpmUpd = new OTB_OPM_ImportExhibition
                        {
                            Bills = data[OTB_OPM_ImportExhibition.CN_BILLS].ToString(),
                            ModifyUser = i_crm.USERID,
                            ModifyDate = DateTime.Now
                        };
                        db.Updateable(oOpmUpd).UpdateColumns(it => new { it.Bills, it.ModifyUser, it.ModifyDate })
                            .Where(it => it.ImportBillNO == sImportBillNO).ExecuteCommand();

                        //添加提醒消息
                        var oTipsAdd = SYS.Task_QryService.TipsAdd(i_crm, sTitle, oOpm.ResponsiblePerson, i_crm.CUSTOMDATA[@"program_id"] + @"|?Action=Upd&GoTab=2&ImportBillNO=" + oOpm.ImportBillNO + @"&BillNO=" + sBillNO, WebAppGlobalConstWord.BELL);
                        db.Insertable(oTipsAdd).ExecuteCommand();

                        db.Deleteable<OTB_SYS_Task>().Where(it => it.OrgID == i_crm.ORIGID && it.SourceID == sSourceID).ExecuteCommand();

                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, oOpm.ResponsiblePerson);
                    } while (false);
                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ExhibitionImport_UpdService), @"進口", @"BillVoid（作廢帳單）", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
                Logger.Debug(@"ExhibitionImport_UpdService.BillVoid Debug（Param：" + JsonToString(i_crm) + @"；Response：" + JsonToString(rm) + @"）------------------");
            }
            return rm;
        }

        #endregion 作廢帳單

        #region 刪除帳單

        /// <summary>
        /// 刪除帳單
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on BillDelete</param>
        /// <returns></returns>
        public ResponseMessage BillDelete(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var data = i_crm.DATA as Dictionary<string, object>;
                        var oBill = (JObject)data[@"Bill"];
                        var sImportBillNO = _fetchString(i_crm, OTB_OPM_ImportExhibition.CN_IMPORTBILLNO);
                        var sBillNO = oBill[@"BillNO"].ToString();
                        var sSourceID = oBill[EasyNetGlobalConstWord.GUID].ToString();

                        var sdb = new SimpleClient<OTB_OPM_ImportExhibition>(db);
                        var oOpm = sdb.GetById(sImportBillNO);
                        if (oOpm == null)
                        {
                            sMsg = @"系統找不到對應的基本資料，請核查！";
                            break;
                        }
                        var LogData = _fetchString(i_crm, @"LogData");
                        var LogResult = BillLogs.InsertBillChangeLog(db, LogData, ActionType.帳單刪除, i_crm.ORIGID, i_crm.USERID);
                        if (!LogResult.Item1)
                        {
                            sMsg = LogResult.Item2;
                            break;
                        }
                        var member = db.Queryable<OTB_SYS_Members>().Single(x => x.OrgID == i_crm.ORIGID && x.MemberID == i_crm.USERID);
                        var sTitle = @"管理員（" + member.MemberName + @"）刪除了您的「進口」（活動名稱：" + oOpm.ImportBillName + @"）的帳單（" + sBillNO + @"）";
                        if (i_crm.LANG == @"zh")
                        {
                            sTitle = ChineseStringUtility.ToSimplified(sTitle);
                        }

                        //更新帳單
                        var oOpmUpd = new OTB_OPM_ImportExhibition
                        {
                            Bills = data[OTB_OPM_ImportExhibition.CN_BILLS].ToString(),
                            ModifyUser = i_crm.USERID,
                            ModifyDate = DateTime.Now
                        };
                        db.Updateable(oOpmUpd).UpdateColumns(it => new { it.Bills, it.ModifyUser, it.ModifyDate })
                            .Where(it => it.ImportBillNO == sImportBillNO).ExecuteCommand();

                        //添加提醒消息
                        var oTipsAdd = SYS.Task_QryService.TipsAdd(i_crm, sTitle, oOpm.ResponsiblePerson, i_crm.CUSTOMDATA[@"program_id"] + @"|?Action=Upd&GoTab=2&ImportBillNO=" + oOpm.ImportBillNO + @"&BillNO=" + sBillNO, WebAppGlobalConstWord.BELL);
                        db.Insertable(oTipsAdd).ExecuteCommand();

                        db.Deleteable<OTB_SYS_Task>().Where(it => it.OrgID == i_crm.ORIGID && it.SourceID == sSourceID).ExecuteCommand();

                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, oOpm.ResponsiblePerson);
                    } while (false);
                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ExhibitionImport_UpdService), @"進口", @"BillDelete（刪除帳單）", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
                Logger.Debug(@"ExhibitionImport_UpdService.BillDelete Debug（Param：" + JsonToString(i_crm) + @"；Response：" + JsonToString(rm) + @"）------------------");
            }
            return rm;
        }

        #endregion 刪除帳單

        #region 進出口程式下載簽收證明文件

        /// <summary>
        /// 進出口程式下載簽收證明文件
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on OutputSignDocuments</param>
        /// <returns></returns>
        public ResponseMessage OutputSignDocuments(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            var sOutPut = Common.ConfigGetValue(@"", @"OutFilesPath");
            try
            {
                do
                {
                    var sImportBillNO = _fetchString(i_crm, OTB_OPM_ImportExhibition.CN_IMPORTBILLNO);
                    var sType = _fetchString(i_crm, @"Type");
                    var sFileType = _fetchString(i_crm, @"FileType");
                    var sSupplierID = _fetchString(i_crm, @"SupplierID");
                    var bSingleSupplier = sSupplierID == @"";

                    var oOpm = db.Queryable<OTB_OPM_ImportExhibition>().Single(it => it.ImportBillNO == sImportBillNO);
                    if (oOpm == null)
                    {
                        sMsg = @"系統找不到對應的基本資料，請核查！";
                        break;
                    }

                    var joSupplier = new JObject();
                    var oCus = new OTB_CRM_Customers();
                    if (!bSingleSupplier)
                    {
                        oCus = db.Queryable<OTB_CRM_Customers>().Single(it => it.guid == sSupplierID);
                        if (oCus == null)
                        {
                            oCus = new OTB_CRM_Customers();
                        }
                        var jaSuppliers = (JArray)JsonConvert.DeserializeObject(oOpm.Suppliers);
                        foreach (JObject jo in jaSuppliers)
                        {
                            if (jo[@"SupplierID"].ToString() == sSupplierID)
                            {
                                joSupplier = jo;
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (oOpm.SupplierType == @"S")
                        {
                            if (!string.IsNullOrEmpty(oOpm.Supplier))
                            {
                                oCus = db.Queryable<OTB_CRM_Customers>().Single(it => it.guid == oOpm.Supplier);
                                if (oCus == null)
                                {
                                    sMsg = @"系統找不到客戶/參展商資訊";
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(oOpm.Agent))
                            {
                                oCus = db.Queryable<OTB_CRM_Customers>().Single(it => it.guid == oOpm.Agent);
                                if (oCus == null)
                                {
                                    sMsg = @"系統找不到代理商資訊";
                                    break;
                                }
                            }
                        }
                    }

                    var sTemplId = sType == @"Import" ? @"SignDocumentsTmp" : @"ReturnSheet";
                    var oTempl = db.Queryable<OTB_SYS_OfficeTemplate>().Single(it => it.OrgID == i_crm.ORIGID && it.TemplID == sTemplId);
                    if (oTempl == null)
                    {
                        sMsg = sType == @"Import" ? @"請檢查簽收證明模版設定" : @"請檢查回運指示單模版設定";
                        break;
                    }

                    var oFile = db.Queryable<OTB_SYS_Files>()
                        .Single(it => it.OrgID == i_crm.ORIGID && it.ParentID == oTempl.FileID);
                    if (oFile == null)
                    {
                        sMsg = sType == @"Import" ? @"系統找不到簽收證明模版" : @"系統找不到回運指示單模版";
                        break;
                    }

                    var sTempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, oFile.FilePath);//Word模版路徑
                    var sBase = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"");
                    sOutPut = sBase + sOutPut;
                    Common.FnCreateDir(sOutPut);//如果不存在就創建文件夾
                    var sDocxName = oOpm.RefNumber.Trim() + @"_" + sType;
                    //建立臨時文件
                    var sTempFile = Path.GetTempPath() + sDocxName + @".docx";
                    sOutPut += sDocxName + @".docx";
                    if (File.Exists(sTempFile))
                    {
                        File.Delete(sTempFile);
                    }
                    File.Copy(sTempPath, sTempFile);
                    var JReImport = new JObject();
                    var JReImportData = new JObject();
                    if (sType != @"Import")
                    {
                        var jaReImports = (JArray)JsonConvert.DeserializeObject(oOpm.ReImports);
                        if (jaReImports != null && jaReImports.Count > 0)
                        {
                            var iIndex = sType == @"ReImport" ? 1 : int.Parse(sType.Split('-')[1]);
                            var iType = 1;
                            foreach (JObject jo in jaReImports)
                            {
                                if (iType == iIndex)
                                {
                                    JReImport = (JObject)jo[@"ReImport"];
                                    JReImportData = (JObject)jo[@"ReImportData"];
                                    break;
                                }
                                iType++;
                            }
                        }
                    }
                    var oExhibition = db.Queryable<OTB_OPM_Exhibition>().Single(it => it.SN == oOpm.ExhibitionNO.ObjToInt());
                    if (oExhibition == null)
                    {
                        oExhibition = new OTB_OPM_Exhibition();
                    }
                    var JTmpKeys = (JArray)JsonConvert.DeserializeObject(oTempl.TemplKeys);
                    using (WordprocessingDocument doc = WordprocessingDocument.Open(sTempFile, true))//顯示Word
                    {
                        //替換變數
                        var sBody = doc.MainDocumentPart.Document.Body.InnerXml;

                        if (sType != @"Import")
                        {
                            sBody = sBody.Replace(@"[$Name$]", Common.EncodeEscapeChar(oExhibition.Exhibitioname_TW + (oExhibition.Exhibitioname_EN != @"" ? @"(" + oExhibition.Exhibitioname_EN + @")" : @"")))
                                    .Replace(@"[$Exhibitor$]", Common.EncodeEscapeChar((oCus.CustomerCName ?? @"") + (!string.IsNullOrEmpty(oCus.CustomerEName) ? @"(" + oCus.CustomerEName + @")" : @"")))
                                    .Replace(@"[$Booth$]", oOpm.MuseumMumber)
                                    .Replace(@"[$Contact$]", Common.EncodeEscapeChar(oOpm.SitiContactor))
                                    .Replace(@"[$Tel$]", oOpm.SitiTelephone)
                                    .Replace(@"[$Destination$]", JReImportData[@"ReShipmentPort"].ToString());
                        }
                        else
                        {
                            var sHall = @"";
                            if ((bSingleSupplier && !string.IsNullOrWhiteSpace(oOpm.Hall)) || (!bSingleSupplier && joSupplier[@"Hall"].ToString() != @""))
                            {
                                sHall = bSingleSupplier ? oOpm.Hall : joSupplier[@"Hall"].ToString();
                                var oArguments = db.Queryable<OTB_SYS_Arguments>()
                                    .Single(it => it.OrgID == i_crm.ORIGID && it.ArgumentClassID == @"Hall" && it.ArgumentID == sHall);
                                if (oArguments != null)
                                {
                                    sHall = oArguments.ArgumentValue ?? @"";
                                }
                            }
                            sBody = sBody.Replace(@"[$Date$]", DateTime.Now.ToString(@"yyyy/MM/dd"))
                                    .Replace(@"[$Supplier$]", Common.EncodeEscapeChar((oCus.CustomerCName ?? @"") + (!string.IsNullOrEmpty(oCus.CustomerEName) ? @"(" + oCus.CustomerEName + @")" : @"")))
                                    .Replace(@"[$Hall$]", sHall)
                                    .Replace(@"[$MNO$]", bSingleSupplier ? oOpm.MuseumMumber : joSupplier[@"MuseumMumber"].ToString())
                                    .Replace(@"[$Contact$]", Common.EncodeEscapeChar(bSingleSupplier ? oOpm.SitiContactor : joSupplier[@"ContactorName"].ToString()))
                                    .Replace(@"[$Tel$]", bSingleSupplier ? oOpm.SitiTelephone : joSupplier[@"Telephone"].ToString())
                                    .Replace(@"[$Unit$]", bSingleSupplier ? oOpm.Unit : joSupplier[@"Unit"].ToString())
                                    .Replace(@"[$BNO$]", oOpm.BillLadNO)
                                    .Replace(@"[$Exhibition$]", Common.EncodeEscapeChar(oExhibition.Exhibitioname_TW + (oExhibition.Exhibitioname_EN != @"" ? @"(" + oExhibition.Exhibitioname_EN + @")" : @"")))
                                    .Replace(@"[$Number$]", $@"{(bSingleSupplier ? oOpm.BoxNo : Convert.ToInt32(joSupplier[@"BoxNo"].ToString() == @"" ? @"0" : joSupplier[@"BoxNo"].ToString())):N0}")
                                    .Replace(@"[$Weight$]", $@"{(bSingleSupplier ? oOpm.Weight : Convert.ToDecimal(joSupplier[@"Weight"].ToString() == @"" ? @"0" : joSupplier[@"Weight"].ToString())):N3}")
                                    .Replace(@"[$Numbers$]", $@"{(bSingleSupplier ? oOpm.BoxNo : Convert.ToInt32(joSupplier[@"BoxNo"].ToString() == @"" ? @"0" : joSupplier[@"BoxNo"].ToString())):N0}")
                                    .Replace(@"[$Weights$]", $@"{(bSingleSupplier ? oOpm.Weight : Convert.ToDecimal(joSupplier[@"Weight"].ToString() == @"" ? @"0" : joSupplier[@"Weight"].ToString())):N3}")
                                    .Replace(@"[$Sign$]", bSingleSupplier ? (oOpm.ApproachTime == null ? @"" : Convert.ToDateTime(oOpm.ApproachTime).ToString(@"yyyy/MM/dd HH:mm")) : (joSupplier[@"ApproachTime"].ToString() == @"" ? @"" : Convert.ToDateTime(joSupplier[@"ApproachTime"].ToString()).ToString(@"yyyy/MM/dd HH:mm")));
                        }
                        foreach (JObject jo in JTmpKeys)
                        {
                            if (jo.Property(@"TemplKey") != null && jo[@"TemplKey"].ToString() != @"")
                            {
                                sBody = sBody.Replace(jo[@"TemplKey"].ToString(), Common.EncodeEscapeChar(jo[@"TemplKeyValue"].ToString()));
                            }
                        }

                        doc.MainDocumentPart.Document.Body.InnerXml = sBody;
                        doc.MainDocumentPart.Document.Save();
                    }
                    if (File.Exists(sOutPut))
                    {
                        File.Delete(sOutPut);
                    }
                    if (sFileType == @"pdf")
                    {
                        sOutPut = sOutPut.Replace(@"docx", @"pdf");
                        var bOk = Common.WordToPDF(sTempFile, sOutPut);
                        if (!bOk)
                        {
                            sOutPut = @"";
                        }
                    }
                    else
                    {
                        File.Copy(sTempFile, sOutPut);
                    }
                    Thread.Sleep(500);
                    File.Delete(sTempFile);   //刪除臨時文件
                    sOutPut = sOutPut.Replace(sBase, @"");
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, sOutPut);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ExhibitionImport_UpdService), @"進口", @"OutputSignDocuments（進出口程式下載簽收證明文件）", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
                Logger.Debug(@"ExhibitionImport_UpdService.OutputSignDocuments Debug（Param：" + JsonToString(i_crm) + @"；Response：" + JsonToString(rm) + @"）------------------");
            }
            return rm;
        }

        #endregion 進出口程式下載簽收證明文件

        #region 進口帳單列印下載

        /// <summary>
        /// 進口帳單列印下載
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on PrintBill</param>
        /// <returns></returns>
        public ResponseMessage PrintBill(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            var sBase = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"");
            var sOutPut = Common.ConfigGetValue(@"", @"OutFilesPath");
            sOutPut = sBase + sOutPut;
            try
            {
                do
                {
                    var sImportBillNO = _fetchString(i_crm, OTB_OPM_ImportExhibition.CN_IMPORTBILLNO);
                    var sTemplID = _fetchString(i_crm, OTB_SYS_OfficeTemplate.CN_TEMPLID);
                    var sBill = _fetchString(i_crm, @"Bill");
                    var sAction = _fetchString(i_crm, @"Action");
                    var sPayDateText = _fetchString(i_crm, @"PayDateText");
                    var bTW = sTemplID.EndsWith(@"TW");
                    var sdb = new SimpleClient<OTB_OPM_ImportExhibition>(db);
                    var oOpm = sdb.GetById(sImportBillNO);
                    if (oOpm == null)
                    {
                        sMsg = @"系統找不到對應的基本資料，請核查！";
                        break;
                    }

                    var oTempl = db.Queryable<OTB_SYS_OfficeTemplate>().Single(it => it.OrgID == i_crm.ORIGID && it.TemplID == sTemplID);
                    if (oTempl == null)
                    {
                        sMsg = @"請檢查模版設定";
                        break;
                    }

                    var oFile = db.Queryable<OTB_SYS_Files>()
                        .Single(it => it.OrgID == i_crm.ORIGID && it.ParentID == oTempl.FileID);
                    if (oFile == null)
                    {
                        sMsg = @"系統找不到對應的帳單模版";
                        break;
                    }

                    var oBill = (JObject)JsonConvert.DeserializeObject(sBill);
                    var bTWCurrency = oBill[@"Currency"].ToString() == @"NTD";
                    var oPayer = new OTB_CRM_Customers();
                    if (!string.IsNullOrEmpty(oBill[@"Payer"].ToString()))
                    {
                        oPayer = db.Queryable<OTB_CRM_Customers>().Single(it => it.guid == oBill[@"Payer"].ToString());
                        if (oPayer == null)
                        {
                            sMsg = @"系統找不到付款人資訊";
                            break;
                        }
                    }
                    var WatermarkInfo = Common.GetWatermarkInfo(oBill[@"AuditVal"]?.ToString(), sTemplID, i_crm.ORIGID);
                    var sTempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, oFile.FilePath);//Word模版路徑
                    Common.FnCreateDir(sOutPut);//如果不存在就創建文件夾
                    var sDocxName = @"Invoice-" + oBill[@"BillNO"];
                    //建立臨時文件
                    var sTempFile = Path.GetTempPath() + sDocxName + @".docx";
                    sOutPut += sDocxName + @".docx";
                    if (File.Exists(sTempFile))
                    {
                        File.Delete(sTempFile);
                    }
                    File.Copy(sTempPath, sTempFile);
                    var JTmpKeys = (JArray)JsonConvert.DeserializeObject(oTempl.TemplKeys);
                    double iPARM17 = 0;
                    using (WordprocessingDocument doc = WordprocessingDocument.Open(sTempFile, true))//顯示Word
                    {
                        var body = doc.MainDocumentPart.Document.Body;
                        var jaFeeItems = (JArray)JsonConvert.DeserializeObject(oBill[@"FeeItems"].ToString());
                        //查找:獲取第三個表格
                        var table = body.Elements<Table>().ElementAt(2);
                        if (jaFeeItems.Count > 0)
                        {
                            var trow_Amount = table.Elements<TableRow>().ElementAt(4).Clone() as TableRow;//小計行
                            var trow_Tax = table.Elements<TableRow>().ElementAt(5).Clone() as TableRow;//營業稅
                            var trow_Total = table.Elements<TableRow>().ElementAt(7).Clone() as TableRow;//總應收
                            var trow_PrePay = table.Elements<TableRow>().ElementAt(8).Clone() as TableRow;//預收
                            var jaFeeItems_Tax = new JArray();
                            var jaFeeItems_NoTax = new JArray();
                            var iDefultRow = 10;
                            var iInsertRow = 1;
                            var iFeeItemsRow = 1;

                            foreach (JObject jo in jaFeeItems)
                            {
                                if (jo[@"FinancialTaxRate"].ToString().Replace(@"%", @"").Trim() == @"0")
                                {
                                    jaFeeItems_NoTax.Add(jo);
                                }
                                else
                                {
                                    jaFeeItems_Tax.Add(jo);
                                }
                            }

                            if (jaFeeItems_Tax.Count > 0)
                            {
                                var jaFeeItems_TaxSorted = new JArray(jaFeeItems_Tax.OrderBy(p => p[@"OrderBy"]));
                                foreach (JObject jo in jaFeeItems_TaxSorted)
                                {
                                    var trow_Item = table.Elements<TableRow>().ElementAt(2).Clone() as TableRow;//費用明細行
                                    var iCellCount = trow_Item.Elements<TableCell>().Count();
                                    if (iCellCount == 9)
                                    {
                                        ExhibitionHelper.RenderFeeItemsTW(trow_Item, jo, iCellCount, iFeeItemsRow);
                                    }
                                    else
                                    {
                                        ExhibitionHelper.RenderFeeItemsFR(trow_Item, jo, iCellCount, iFeeItemsRow);
                                    }
                                    table.InsertAt<TableRow>(trow_Item, iInsertRow + iDefultRow); //插入一行費用
                                    iInsertRow++;
                                    iFeeItemsRow++;
                                    iPARM17 += double.Parse(jo[@"FinancialTWAmount"].ToString());
                                }
                                table.InsertAt<TableRow>(trow_Amount, iInsertRow + iDefultRow);//插入一行小計行
                                iInsertRow++;
                                //營業稅
                                table.InsertAt<TableRow>(trow_Tax, iInsertRow + iDefultRow);//插入一行營業稅
                                iInsertRow++;
                            }
                            if (jaFeeItems_NoTax.Count > 0)
                            {
                                var jaFeeItems_NoTaxSorted = new JArray(jaFeeItems_NoTax.OrderBy(p => p[@"OrderBy"]));
                                var iCellCount = 0;
                                foreach (JObject jo in jaFeeItems_NoTaxSorted)
                                {
                                    var trow_Item = table.Elements<TableRow>().ElementAt(2).Clone() as TableRow;//費用明細行
                                    iCellCount = trow_Item.Elements<TableCell>().Count();
                                    if (iCellCount == 9)
                                    {
                                        ExhibitionHelper.RenderFeeItemsTW(trow_Item, jo, iCellCount, iFeeItemsRow);
                                    }
                                    else
                                    {
                                        ExhibitionHelper.RenderFeeItemsFR(trow_Item, jo, iCellCount, iFeeItemsRow);
                                    }
                                    //插入一行
                                    table.InsertAt<TableRow>(trow_Item, iInsertRow + iDefultRow);
                                    iInsertRow++;
                                    iFeeItemsRow++;
                                }
                                if (iCellCount == 9)
                                {
                                    table.InsertAt<TableRow>(trow_Total, iInsertRow + iDefultRow);//插入一行總應收
                                    iInsertRow++;
                                    table.InsertAt<TableRow>(trow_PrePay, iInsertRow + iDefultRow);//插入一行預收
                                }
                            }
                            //刪除模板行
                            for (int i = 0; i < 8; i++)
                            {
                                table.Elements<TableRow>().ElementAt(1).Remove();
                            }
                        }
                        var oExhibition = db.Queryable<OTB_OPM_Exhibition>().Single(it => it.SN == oOpm.ExhibitionNO.ObjToInt());
                        if (oExhibition == null)
                        {
                            oExhibition = new OTB_OPM_Exhibition();
                        }

                        string sBillNO = oBill["BillNO"] != null ? oBill["BillNO"].ToString() : string.Empty;
                        string sTaxSum = oBill["TaxSum"] != null ? oBill["TaxSum"].ToString() : string.Empty;
                        string sAmountTaxSum = oBill["AmountTaxSum"] != null ? oBill["AmountTaxSum"].ToString() : string.Empty;
                        string sAdvance = oBill["Advance"] != null ? oBill["Advance"].ToString() : string.Empty;
                        string sTotalReceivable = oBill["TotalReceivable"] != null ? oBill["TotalReceivable"].ToString() : string.Empty;
                        string sCurrency = oBill["Currency"] != null ? oBill["Currency"].ToString() : string.Empty;
                        string sWeight = oBill["Weight"] != null ? oBill["Weight"].ToString() : string.Empty;
                        string sVolume = oBill["Volume"] != null ? oBill["Volume"].ToString() : string.Empty;
                        string sNumber = oBill["Number"] != null ? oBill["Number"].ToString() : string.Empty;
                        string sUnit = oBill["Unit"] != null ? oBill["Unit"].ToString() : string.Empty;
                        string sContactorName = oBill["ContactorName"] != null ? oBill["ContactorName"].ToString() : string.Empty;
                        string sTelephone = oBill["Telephone"] != null ? oBill["Telephone"].ToString() : string.Empty;
                        string sMemo = oBill["Memo"] != null ? oBill["Memo"].ToString() : string.Empty;


                        var ReturnType = oBill[@"IsRetn"] != null && oBill[@"IsRetn"].ToString() == @"Y";
                        var sPARM3 = @"";
                        var sPARM4 = @"";
                        var sPARM7 = oExhibition.Exhibitioname_TW + ExhibitionHelper.GetExhibitioImportComment(false, ReturnType);
                        var sPARM14 = @"";
                        var sPARM16 = @"";
                        var iPARM18 = double.Parse(sTaxSum);
                        var iPARM19 = double.Parse(sAmountTaxSum);
                        var iPARM20 = double.Parse(sAdvance);
                        var iPARM21 = double.Parse(sTotalReceivable);
                        var sPARM27 = @"";
                        var sPARM29 = ExhibitionHelper.GetEnglishName(i_crm.USERID);
                        var sPARM30 = sCurrency;
                        if (!string.IsNullOrEmpty(sWeight))
                        {
                            sPARM16 = sWeight + @"KG";
                        }
                        if (!string.IsNullOrEmpty(sVolume))
                        {
                            sPARM16 = sPARM16 == @"" ? sVolume + @"CBM" : sPARM16 + @" / " + sVolume + @"CBM";
                        }
                        if (!string.IsNullOrEmpty(sNumber))
                        {
                            sPARM14 = sNumber + sUnit;
                        }
                        if (!string.IsNullOrEmpty(sContactorName))
                        {
                            sPARM3 = sContactorName;
                        }
                        if (!string.IsNullOrEmpty(sTelephone))
                        {
                            sPARM4 = sTelephone;
                        }
                        if (!string.IsNullOrEmpty(sMemo))
                        {
                            sPARM27 = System.Security.SecurityElement.Escape(sMemo);
                        }
                        if (bTWCurrency)
                        {
                            iPARM17 = ExhibitionHelper.Round(iPARM17, 0);
                            iPARM18 = ExhibitionHelper.Round(iPARM18, 0);
                            iPARM19 = ExhibitionHelper.Round(iPARM19, 0);
                            iPARM20 = ExhibitionHelper.Round(iPARM20, 0);
                            iPARM21 = ExhibitionHelper.Round(iPARM21, 0);
                        }
                        else
                        {
                            sPARM7 = oExhibition.Exhibitioname_EN + ExhibitionHelper.GetExhibitioImportComment(true, ReturnType);
                        }
                        if (bTW)
                        {
                            var member = db.Queryable<OTB_SYS_Members>().Single(x => x.OrgID == i_crm.ORIGID && x.MemberID == i_crm.USERID);
                            sPARM29 = member.MemberName;
                        }
                        ExhibitionHelper.RenderMemo(body, sPARM27);
                        var sShipAndVoyage = oOpm.ShipAndVoyage ?? @"";
                        var sBillLadNO = oOpm.BillLadNO ?? @"";
                        var sBillLadNOSub = oOpm.BillLadNOSub ?? @"";
                        var sShipmentPort = oOpm.ShipmentPort ?? @"";
                        var sDestinationPort = oOpm.DestinationPort ?? @"";
                        string sIsRetn = oBill["IsRetn"] != null ? oBill["IsRetn"].ToString() : string.Empty;
                        string sReImportNum = oBill["ReImportNum"] != null ? oBill["ReImportNum"].ToString() : string.Empty;
                        if (sIsRetn == "Y" && !string.IsNullOrEmpty(sReImportNum))
                        {
                            var iReImportNum = (int)oBill[@"ReImportNum"];
                            var iIndex = 1;
                            var saReImports = (JArray)JsonConvert.DeserializeObject(oOpm.ReImports);
                            foreach (JObject jo in saReImports)
                            {
                                if (iIndex == iReImportNum)
                                {
                                    var oReImportData = (JObject)jo[@"ReImportData"];
                                    sShipAndVoyage = oReImportData["FlightInformation"] != null ? oReImportData["FlightInformation"].ToString() : string.Empty; //航班欄位
                                    sBillLadNO = oReImportData["BillLadNO"] != null ? oReImportData["BillLadNO"].ToString() : string.Empty;
                                    sBillLadNOSub = oReImportData["BillLadNOSub"] != null ? oReImportData["BillLadNOSub"].ToString() : string.Empty;
                                    sShipmentPort = oReImportData["DestinationPort"] != null ? oReImportData["DestinationPort"].ToString() : string.Empty;
                                    sDestinationPort = oReImportData["ReShipmentPort"] != null ? oReImportData["ReShipmentPort"].ToString() : string.Empty;
                                    break;
                                }
                                iIndex++;
                            }
                        }
                        string sBillCheckDate = oBill["BillCheckDate"] != null ? oBill["BillCheckDate"].ToString() : string.Empty;
                        var BillCheckDate = string.IsNullOrEmpty(sBillCheckDate) ? "" : Convert.ToDateTime(sBillCheckDate).ToString(@"yyyy/MM/dd");
                        var sBody = body.InnerXml
                                .Replace(@"[PARM1]", string.IsNullOrEmpty(oPayer.CustomerCName) ? Common.EncodeEscapeChar(oPayer.CustomerEName) : Common.EncodeEscapeChar(oPayer.CustomerCName))
                                .Replace(@"[PARM2]", ExhibitionHelper.GetBillAddress(oPayer))
                                .Replace(@"[PARM3]", Common.EncodeEscapeChar(sPARM3))
                                .Replace(@"[PARM4]", Common.EncodeEscapeChar(sPARM4))
                                .Replace(@"[PARM5]", Common.EncodeEscapeChar(oPayer.FAX ?? @""))
                                .Replace(@"[PARM6]", Common.EncodeEscapeChar(oPayer.UniCode ?? @""))
                                .Replace(@"[PARM7]", Common.EncodeEscapeChar(sPARM7))
                                .Replace(@"[PARM8]", sBillNO)
                                .Replace(@"[PARM9]", BillCheckDate)
                                .Replace(@"[PARM10]", Common.EncodeEscapeChar(sShipAndVoyage))
                                .Replace(@"[PARM11]", Common.EncodeEscapeChar(sBillLadNO))
                                .Replace(@"[PARM12]", Common.EncodeEscapeChar(sShipmentPort))
                                .Replace(@"[PARM13]", Common.EncodeEscapeChar(sDestinationPort))
                                .Replace(@"[PARM14]", Common.EncodeEscapeChar(sPARM14))
                                .Replace(@"[PARM16]", Common.EncodeEscapeChar(sPARM16))
                                .Replace(@"[PARM17]", bTWCurrency ? $@"{iPARM17:N0}" : $@"{iPARM17:N2}")
                                .Replace(@"[PARM18]", bTWCurrency ? $@"{iPARM18:N0}" : $@"{iPARM18:N2}")
                                .Replace(@"[PARM19]", bTWCurrency ? $@"{iPARM19:N0}" : $@"{iPARM19:N2}")
                                .Replace(@"[PARM20]", bTWCurrency ? $@"{iPARM20:N0}" : $@"{iPARM20:N2}")
                                .Replace(@"[PARM21]", bTWCurrency ? $@"{iPARM21:N0}" : $@"{iPARM21:N2}")
                                .Replace(@"[PARM22]", Common.EncodeEscapeChar(sPayDateText))
                                .Replace(@"[PARM27]", Common.EncodeEscapeChar(sPARM27))
                                .Replace(@"[PARM28]", Common.EncodeEscapeChar(sBillLadNOSub))
                                .Replace(@"[PARM29]", Common.EncodeEscapeChar(sPARM29))
                                .Replace(@"[PARM30]", Common.EncodeEscapeChar(sPARM30));
                        foreach (JObject jo in JTmpKeys)
                        {
                            string sTempKey = jo["TemplKey"] != null ? jo["TemplKey"].ToString() : string.Empty;
                            string sTemplKeyValue = jo["TemplKeyValue"] != null ? jo["TemplKeyValue"].ToString() : string.Empty;

                            if (!string.IsNullOrEmpty(sTempKey))
                            {
                                var Value = Common.EncodeEscapeChar(sTemplKeyValue);
                                var Key = sTempKey;
                                if (Key.Contains("PARM") && WatermarkInfo.Item1)
                                {
                                    Value = "";
                                }
                                sBody = sBody.Replace(Key, Value);
                            }
                        }
                        doc.MainDocumentPart.Document.Body.InnerXml = sBody;
                        doc.MainDocumentPart.Document.Save();
                        if (WatermarkInfo.Item1)
                        {
                            Common.WordAddWatermartText(doc, WatermarkInfo.Item2);
                        }
                    }
                    if (File.Exists(sOutPut))
                    {
                        File.Delete(sOutPut);
                    }
                    if (sAction.StartsWith(@"Print"))
                    {
                        sOutPut = sOutPut.Replace(@"docx", @"pdf");
                        var bOk = Common.WordToPDF(sTempFile, sOutPut);
                        if (!bOk)
                        {
                            sOutPut = @"";
                        }
                    }
                    else
                    {
                        File.Copy(sTempFile, sOutPut);
                    }
                    Thread.Sleep(500);
                    File.Delete(sTempFile);   //刪除臨時文件
                    sOutPut = sOutPut.Replace(sBase, @"");
                } while (false);

                rm = new SuccessResponseMessage(null, i_crm);
                rm.DATA.Add(BLWording.REL, sOutPut);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ExhibitionImport_UpdService), @"進口", @"PrintBill（進口帳單列印下載）", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
                Logger.Debug(@"ExhibitionImport_UpdService.PrintBill Debug（Param：" + JsonToString(i_crm) + @"；Response：" + JsonToString(rm) + @"）------------------");
            }
            return rm;
        }

        #endregion 進口帳單列印下載

        #region 進口收據列印下載

        /// <summary>
        /// 進口收據列印下載
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on PrintReceipt</param>
        /// <returns></returns>
        public ResponseMessage PrintReceipt(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            var sOutPut = Common.ConfigGetValue(@"", @"OutFilesPath");
            try
            {
                do
                {
                    var sImportBillNO = _fetchString(i_crm, OTB_OPM_ImportExhibition.CN_IMPORTBILLNO);
                    var sTemplID = _fetchString(i_crm, OTB_SYS_OfficeTemplate.CN_TEMPLID);
                    var sBill = _fetchString(i_crm, @"Bill");
                    var sAction = _fetchString(i_crm, @"Action");

                    var sdb = new SimpleClient<OTB_OPM_ImportExhibition>(db);
                    var oOpm = sdb.GetById(sImportBillNO);
                    if (oOpm == null)
                    {
                        sMsg = @"系統找不到對應的基本資料，請核查！";
                        break;
                    }

                    var oTempl = db.Queryable<OTB_SYS_OfficeTemplate>().Single(it => it.OrgID == i_crm.ORIGID && it.TemplID == sTemplID);
                    if (oTempl == null)
                    {
                        sMsg = @"請檢查模版設定";
                        break;
                    }

                    var oFile = db.Queryable<OTB_SYS_Files>()
                        .Single(it => it.OrgID == i_crm.ORIGID && it.ParentID == oTempl.FileID);
                    if (oFile == null)
                    {
                        sMsg = @"系統找不到對應的帳單模版";
                        break;
                    }

                    var oBill = (JObject)JsonConvert.DeserializeObject(sBill);
                    var WatermarkInfo = Common.GetWatermarkInfo(oBill[@"AuditVal"]?.ToString(), sTemplID, i_crm.ORIGID);
                    var bTWCurrency = oBill[@"Currency"].ToString() == @"NTD";
                    var oPayer = new OTB_CRM_Customers();
                    if (!string.IsNullOrEmpty(oBill[@"Payer"].ToString()))
                    {
                        oPayer = db.Queryable<OTB_CRM_Customers>().Single(it => it.guid == oBill[@"Payer"].ToString());
                        if (oPayer == null)
                        {
                            sMsg = @"系統找不到付款人資訊";
                            break;
                        }
                    }

                    var sTempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, oFile.FilePath);//Word模版路徑
                    var sBase = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"");
                    sOutPut = sBase + sOutPut;
                    Common.FnCreateDir(sOutPut);//如果不存在就創建文件夾
                    var sDocxName = @"Receipt-" + oBill[@"BillNO"];
                    //建立臨時文件
                    var sTempFile = Path.GetTempPath() + sDocxName + @".docx";
                    sOutPut += sDocxName + @".docx";
                    if (File.Exists(sTempFile))
                    {
                        File.Delete(sTempFile);
                    }
                    File.Copy(sTempPath, sTempFile);
                    var JTmpKeys = (JArray)JsonConvert.DeserializeObject(oTempl.TemplKeys);
                    double iAcountAll = 0;
                    using (WordprocessingDocument doc = WordprocessingDocument.Open(sTempFile, true))//顯示Word
                    {
                        var body = doc.MainDocumentPart.Document.Body;
                        var jaFeeItems = (JArray)JsonConvert.DeserializeObject(oBill[@"FeeItems"].ToString());
                        //查找:獲取第三個表格
                        var table = body.Elements<Table>().ElementAt(3);
                        if (jaFeeItems.Count > 0)
                        {
                            var jaFeeItems_NoTax = new JArray();
                            var iInsertRow = 5;

                            foreach (JObject jo in jaFeeItems)
                            {
                                if (jo[@"FinancialTaxRate"].ToString().Replace(@"%", @"").Trim() == @"0")
                                {
                                    jaFeeItems_NoTax.Add(jo);
                                }
                            }
                            if (jaFeeItems_NoTax.Count > 0)
                            {
                                var jaFeeItems_NoTaxSorted = new JArray(jaFeeItems_NoTax.OrderBy(p => p[@"OrderBy"]));
                                foreach (JObject jo in jaFeeItems_NoTaxSorted)
                                {
                                    var trow_First = table.Elements<TableRow>().ElementAt(1);//費用明細第一行
                                    var trow_Item = table.Elements<TableRow>().ElementAt(2).Clone() as TableRow;//費用明細行
                                    if (iInsertRow == 5)
                                    {
                                        for (int i = 0; i < 5; i++)
                                        {
                                            var tcell = trow_First.Elements<TableCell>().ElementAt(i);
                                            var tmpPa = tcell.Elements<Paragraph>().FirstOrDefault();
                                            var tmpRun = tmpPa.Elements<Run>().FirstOrDefault();
                                            var tmpText = tmpRun.Elements<Text>().FirstOrDefault();
                                            switch (i)
                                            {
                                                case 0:
                                                    var sRemark = jo[@"Memo"] == null ? @"" : jo[@"Memo"].ToString();
                                                    tmpText.Text = jo[@"FinancialCode"].ToString() == @"TE001" ? (sRemark == @"" ? jo[@"FinancialCostStatement"].ToString() : sRemark) : jo[@"FinancialCostStatement"] + (sRemark == @"" ? @"" : @"（" + sRemark + @"）");
                                                    break;

                                                case 1:
                                                    tmpText.Text = jo[@"FinancialCurrency"].ToString();
                                                    break;

                                                case 2:
                                                    tmpText.Text = jo[@"FinancialExchangeRate"].ToString();
                                                    break;

                                                case 3:
                                                    var sFinancialAmount = jo[@"FinancialTWAmount"].ToString();
                                                    tmpText.Text = $@"{Convert.ToDecimal(sFinancialAmount.Trim() == @"" ? @"0" : sFinancialAmount):N}";
                                                    break;

                                                case 4:
                                                    var sFinancialAmount4 = jo[@"FinancialTWAmount"].ToString();
                                                    tmpText.Text = $@"{Convert.ToDecimal(sFinancialAmount4.Trim() == @"" ? @"0" : sFinancialAmount4):N}";
                                                    break;

                                                default:
                                                    break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        for (int i = 0; i < 5; i++)
                                        {
                                            var tcell = trow_Item.Elements<TableCell>().ElementAt(i);
                                            var tmpPa = tcell.Elements<Paragraph>().FirstOrDefault();
                                            var tmpRun = tmpPa.Elements<Run>().FirstOrDefault();
                                            var tmpText = tmpRun.Elements<Text>().FirstOrDefault();
                                            switch (i)
                                            {
                                                case 0:
                                                    var sRemark = jo[@"Memo"] == null ? @"" : jo[@"Memo"].ToString();
                                                    tmpText.Text = jo[@"FinancialCode"].ToString() == @"TE001" ? (sRemark == @"" ? jo[@"FinancialCostStatement"].ToString() : sRemark) : jo[@"FinancialCostStatement"] + (sRemark == @"" ? @"" : @"（" + sRemark + @"）");
                                                    break;

                                                case 1:
                                                    tmpText.Text = jo[@"FinancialCurrency"].ToString();
                                                    break;

                                                case 2:
                                                    tmpText.Text = jo[@"FinancialExchangeRate"].ToString();
                                                    break;

                                                case 3:
                                                    var sFinancialAmount = jo[@"FinancialTWAmount"].ToString();
                                                    tmpText.Text = $@"{Convert.ToDecimal(sFinancialAmount.Trim() == @"" ? @"0" : sFinancialAmount):N}";
                                                    break;

                                                case 4:
                                                    var sFinancialAmount4 = jo[@"FinancialTWAmount"].ToString();
                                                    tmpText.Text = $@"{Convert.ToDecimal(sFinancialAmount4.Trim() == @"" ? @"0" : sFinancialAmount4):N}";
                                                    break;

                                                default:
                                                    break;
                                            }
                                        }
                                    }
                                    iAcountAll += double.Parse(jo[@"FinancialTWAmount"].ToString());
                                    //插入一行
                                    table.InsertAt<TableRow>(trow_Item, iInsertRow);
                                    iInsertRow++;
                                }
                                table.Elements<TableRow>().ElementAt(2).Remove();
                                table.Elements<TableRow>().ElementAt(2).Remove();
                            }
                            else
                            {
                                table.Elements<TableRow>().ElementAt(1).Remove();
                                table.Elements<TableRow>().ElementAt(1).Remove();
                            }
                            //刪除模板行
                        }
                        var oExhibition = db.Queryable<OTB_OPM_Exhibition>().Single(it => it.SN == oOpm.ExhibitionNO.ObjToInt());
                        if (oExhibition == null)
                        {
                            oExhibition = new OTB_OPM_Exhibition();
                        }

                        var sPARM1 = string.IsNullOrEmpty(oPayer.CustomerCName) ? oPayer.CustomerEName : oPayer.CustomerCName;
                        var sPARM2 = oBill[@"ReceiptDate"].ToString();
                        var sPARM3 = oPayer.UniCode;
                        var sPARM4 = oBill[@"ReceiptNumber"].ToString();
                        var sPARM5 = ExhibitionHelper.GetBillAddress(oPayer);
                        var sPARM6 = oExhibition.Exhibitioname_TW;
                        var sPARM7 = oBill[@"TaxSum"].ToString();
                        var sPARM8 = @"";
                        var sPARM9 = oBill[@"BillNO"].ToString();
                        var iPARM7 = iAcountAll;
                        if (sPARM2 != @"")
                        {
                            sPARM2 = Common.DateToTw(sPARM2);
                            var saPARM2 = sPARM2.Split('/');
                            sPARM2 = saPARM2[0] + @"年" + saPARM2[1] + @"月" + saPARM2[2] + @"日";
                        }
                        iPARM7 = ExhibitionHelper.Round(iPARM7, 0);
                        sPARM8 = Common.MoneyToUpper(iPARM7.ToString());
                        sPARM7 = $@"{iPARM7:N0}";
                        var sBody = body.InnerXml
                                .Replace(@"[PARM1]", Common.EncodeEscapeChar(sPARM1))
                                .Replace(@"[PARM2]", Common.EncodeEscapeChar(sPARM2))
                                .Replace(@"[PARM3]", Common.EncodeEscapeChar(sPARM3))
                                .Replace(@"[PARM4]", Common.EncodeEscapeChar(sPARM4))
                                .Replace(@"[PARM5]", Common.EncodeEscapeChar(sPARM5))
                                .Replace(@"[PARM6]", Common.EncodeEscapeChar(sPARM6))
                                .Replace(@"[PARM7]", Common.EncodeEscapeChar(sPARM7))
                                .Replace(@"[PARM8]", Common.EncodeEscapeChar(sPARM8))
                                .Replace(@"[PARM9]", Common.EncodeEscapeChar(sPARM9));
                        foreach (JObject jo in JTmpKeys)
                        {
                            if (jo.Property(@"TemplKey") != null && jo[@"TemplKey"].ToString() != @"")
                            {
                                sBody = sBody.Replace(jo[@"TemplKey"].ToString(), Common.EncodeEscapeChar(jo[@"TemplKeyValue"].ToString()));
                            }
                        }
                        doc.MainDocumentPart.Document.Body.InnerXml = sBody + sBody;
                        doc.MainDocumentPart.Document.Save();
                        if (WatermarkInfo.Item1)
                        {
                            Common.WordAddWatermartText(doc, WatermarkInfo.Item2);
                        }
                    }
                    if (File.Exists(sOutPut))
                    {
                        File.Delete(sOutPut);
                    }

                    if (sAction.StartsWith(@"Print"))
                    {
                        sOutPut = sOutPut.Replace(@"docx", @"pdf");
                        var bOk = Common.WordToPDF(sTempFile, sOutPut);
                        if (!bOk)
                        {
                            sOutPut = @"";
                        }
                    }
                    else
                    {
                        File.Copy(sTempFile, sOutPut);
                    }
                    Thread.Sleep(500);
                    File.Delete(sTempFile);   //刪除臨時文件
                    sOutPut = sOutPut.Replace(sBase, @"");
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, sOutPut);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ExhibitionImport_UpdService), @"進口", @"PrintReceipt（進口收據列印下載）", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
                Logger.Debug(@"ExhibitionImport_UpdService.PrintReceipt Debug（Param：" + JsonToString(i_crm) + @"；Response：" + JsonToString(rm) + @"）------------------");
            }
            return rm;
        }

        #endregion 進口收據列印下載

        #region 進口(退運)報價和預估成本提交審核

        /// <summary>
        /// 進口(退運)報價和預估成本提交審核
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on ReturnToAuditForQuote</param>
        /// <returns></returns>
        public ResponseMessage ReturnToAuditForQuote(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var data = i_crm.DATA as Dictionary<string, object>;
                        var sImportBillNO = _fetchString(i_crm, OTB_OPM_ImportExhibition.CN_IMPORTBILLNO);
                        var sSourceID = _fetchString(i_crm, @"SourceID");
                        var sIndex = _fetchString(i_crm, @"Index");

                        var sdb = new SimpleClient<OTB_OPM_ImportExhibition>(db);
                        var oOpm = sdb.GetById(sImportBillNO);
                        if (oOpm == null)
                        {
                            sMsg = @"系統找不到對應的基本資料，請核查！";
                            break;
                        }

                        var sTitle = @"「進口」" + oOpm.ImportBillName + @"（" + oOpm.RefNumber + @"）" + @"「退運" + sIndex + @"報價單/預估成本審核」";
                        if (i_crm.LANG == @"zh")
                        {
                            sTitle = ChineseStringUtility.ToSimplified(sTitle);
                        }
                        //更新基本資料
                        var oOpmUpd = new OTB_OPM_ImportExhibition
                        {
                            ReturnBills = data[OTB_OPM_ImportExhibition.CN_RETURNBILLS].ToString(),
                            ModifyUser = i_crm.USERID,
                            ModifyDate = DateTime.Now
                        };
                        db.Updateable(oOpmUpd).UpdateColumns(it => new { it.ReturnBills, it.ModifyUser, it.ModifyDate })
                            .Where(it => it.ImportBillNO == sImportBillNO).ExecuteCommand();
                        //更新代辦
                        SYS.Task_QryService.TaskStatusUpd(db, i_crm.ORIGID, sSourceID);

                        var saSupervisor = new List<string>();
                        var member = db.Queryable<OTB_SYS_Members>().Single(x => x.OrgID == i_crm.ORIGID && x.MemberID == i_crm.USERID);
                        if (member.MemberID != null)
                        {
                            if (!string.IsNullOrEmpty(member.ImmediateSupervisor))
                            {
                                saSupervisor.Add(member.ImmediateSupervisor);
                            }

                            var oDept = db.Queryable<OTB_SYS_Departments>().Single(it => it.OrgID == i_crm.ORIGID && it.DepartmentID == member.DepartmentID);
                            if (oDept != null)
                            {
                                saSupervisor.Add(oDept.ChiefOfDepartmentID);
                            }
                        }
                        var sBillAuditor = Common.GetSystemSetting(db, i_crm.ORIGID, WebAppGlobalConstWord.BILLAUDITOR);
                        if (sBillAuditor != @"")
                        {
                            var saBillAuditor = sBillAuditor.Split(new string[] { @";", @",", @"，", @"|" }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string auditor in saBillAuditor)
                            {
                                saSupervisor.Add(auditor);
                            }
                        }

                        saSupervisor = saSupervisor.Distinct<string>().ToList();
                        var listTips = new List<OTB_SYS_Tips>();
                        var listTask = new List<OTB_SYS_Task>();

                        if (saSupervisor.Count > 0)
                        {
                            foreach (string supervisor in saSupervisor)
                            {
                                //添加提醒消息
                                var oTipsAdd = SYS.Task_QryService.TipsAdd(i_crm, sTitle, supervisor, i_crm.CUSTOMDATA[@"program_id"] + @"|?Action=Upd&GoTab=9&ImportBillNO=" + oOpm.ImportBillNO, WebAppGlobalConstWord.BELL);
                                listTips.Add(oTipsAdd);
                                //添加代辦
                                var oTaskAdd = SYS.Task_QryService.TaskAdd(i_crm, sSourceID, supervisor, sTitle, @"ExhibitionImport_Qry", @"?Action=Upd&GoTab=9&ImportBillNO=" + oOpm.ImportBillNO);
                                listTask.Add(oTaskAdd);
                            }
                        }
                        else
                        {
                            sMsg = @"您沒有對應的直屬主管或部門主管，請核查！";
                            break;
                        }

                        if (listTips.Count > 0)
                        {
                            db.Insertable(listTips).ExecuteCommand();
                        }
                        if (listTask.Count > 0)
                        {
                            db.Insertable(listTask).ExecuteCommand();
                        }

                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, saSupervisor);
                    } while (false);
                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ExhibitionImport_UpdService), @"進口", @"ReturnToAuditForQuote（報價和預估成本提交審核）", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
                Logger.Debug(@"ExhibitionImport_UpdService.ReturnToAuditForQuote Debug（Param：" + JsonToString(i_crm) + @"；Response：" + JsonToString(rm) + @"）------------------");
            }
            return rm;
        }

        #endregion 進口(退運)報價和預估成本提交審核

        #region 進口(退運)報價和預估成本（主管）審核

        /// <summary>
        /// 進口(退運)報價和預估成本（主管）審核
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on ReturnAuditForQuote</param>
        /// <returns></returns>
        public ResponseMessage ReturnAuditForQuote(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var data = i_crm.DATA as Dictionary<string, object>;
                        var sImportBillNO = _fetchString(i_crm, OTB_OPM_ImportExhibition.CN_IMPORTBILLNO);
                        var sAuditVal = _fetchString(i_crm, @"AuditVal");
                        var sSourceID = _fetchString(i_crm, @"SourceID");
                        var sIndex = _fetchString(i_crm, @"Index");
                        var TipsType = WebAppGlobalConstWord.CHECK;

                        var sdb = new SimpleClient<OTB_OPM_ImportExhibition>(db);
                        var oOpm = sdb.GetById(sImportBillNO);
                        if (oOpm == null)
                        {
                            sMsg = @"系統找不到對應的基本資料，請核查！";
                            break;
                        }

                        var member = db.Queryable<OTB_SYS_Members>().Single(x => x.OrgID == i_crm.ORIGID && x.MemberID == i_crm.USERID);
                        var sTitle = member.MemberName + @"審核了您的「進口」活動名稱：" + oOpm.ImportBillName + @"）的「退運" + sIndex + @"報價單/預估成本審核」";

                        //更新報價和預估成本
                        var oOpmUpd = new OTB_OPM_ImportExhibition
                        {
                            ReturnBills = data[OTB_OPM_ImportExhibition.CN_RETURNBILLS].ToString(),
                            ModifyUser = i_crm.USERID,
                            ModifyDate = DateTime.Now
                        };
                        db.Updateable(oOpmUpd).UpdateColumns(it => new { it.ReturnBills, it.ModifyUser, it.ModifyDate })
                            .Where(it => it.ImportBillNO == sImportBillNO).ExecuteCommand();

                        //更新代辦
                        SYS.Task_QryService.TaskStatusUpd(db, i_crm.ORIGID, sSourceID);

                        if (sAuditVal == @"2")
                        {
                            sTitle += @"，審核結果：通過";
                        }
                        else
                        {
                            sTitle += @"，審核結果：不通過";
                            TipsType = WebAppGlobalConstWord.FAIL;
                        }
                        if (i_crm.LANG == @"zh")
                        {
                            sTitle = ChineseStringUtility.ToSimplified(sTitle);
                        }

                        //添加提醒消息
                        var oTipsAdd = SYS.Task_QryService.TipsAdd(i_crm, sTitle, oOpm.ResponsiblePerson, i_crm.CUSTOMDATA[@"program_id"] + @"|?Action=Upd&GoTab=9&ImportBillNO=" + oOpm.ImportBillNO, TipsType);
                        db.Insertable(oTipsAdd).ExecuteCommand();
                        if (sAuditVal != @"2")
                        {
                            //添加代辦
                            var oTaskAdd = SYS.Task_QryService.TaskAdd(i_crm, sSourceID, oOpm.ResponsiblePerson, sTitle, @"ExhibitionImport_Qry", @"?Action=Upd&GoTab=9&ImportBillNO=" + oOpm.ImportBillNO);
                            db.Insertable(oTaskAdd).ExecuteCommand();
                        }

                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, oOpm.ResponsiblePerson);
                    } while (false);
                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ExhibitionImport_UpdService), @"進口", @"ReturnAuditForQuote（進口(退運)報價和預估成本（主管）審核）", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
                Logger.Debug(@"ExhibitionImport_UpdService.ReturnAuditForQuote Debug（Param：" + JsonToString(i_crm) + @"；Response：" + JsonToString(rm) + @"）------------------");
            }
            return rm;
        }

        #endregion 進口(退運)報價和預估成本（主管）審核

        #region 進口（退運）帳單提交審核

        /// <summary>
        /// 進口（退運）帳單提交審核
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on ReturnToAuditForBill</param>
        /// <returns></returns>
        public ResponseMessage ReturnToAuditForBill(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var data = i_crm.DATA as Dictionary<string, object>;
                        var oBill = (JObject)data[@"Bill"];
                        var sImportBillNO = _fetchString(i_crm, OTB_OPM_ImportExhibition.CN_IMPORTBILLNO);
                        var sSourceID = oBill[EasyNetGlobalConstWord.GUID].ToString();
                        var sBillNO = oBill[@"BillNO"].ToString();

                        var sdb = new SimpleClient<OTB_OPM_ImportExhibition>(db);
                        var oOpm = sdb.GetById(sImportBillNO);
                        if (oOpm == null)
                        {
                            sMsg = @"系統找不到對應的基本資料，請核查！";
                            break;
                        }

                        var sTitle = @"「進口」" + oOpm.ImportBillName + @"退運帳單（" + sBillNO + @"）" + @"審核";
                        if (i_crm.LANG == @"zh")
                        {
                            sTitle = ChineseStringUtility.ToSimplified(sTitle);
                        }

                        //更新報價和預估成本
                        var oOpmUpd = new OTB_OPM_ImportExhibition
                        {
                            ReturnBills = data[OTB_OPM_ImportExhibition.CN_RETURNBILLS].ToString(),
                            ModifyUser = i_crm.USERID,
                            ModifyDate = DateTime.Now
                        };
                        db.Updateable(oOpmUpd).UpdateColumns(it => new { it.ReturnBills, it.ModifyUser, it.ModifyDate })
                            .Where(it => it.ImportBillNO == sImportBillNO).ExecuteCommand();

                        //更新代辦
                        SYS.Task_QryService.TaskStatusUpd(db, i_crm.ORIGID, sSourceID);

                        var saSupervisor = new List<string>();
                        var member = db.Queryable<OTB_SYS_Members>().Single(x => x.OrgID == i_crm.ORIGID && x.MemberID == i_crm.USERID);
                        if (member.MemberID != null)
                        {
                            if (!string.IsNullOrEmpty(member.ImmediateSupervisor))
                            {
                                saSupervisor.Add(member.ImmediateSupervisor);
                            }

                            var oDept = db.Queryable<OTB_SYS_Departments>().Single(it => it.OrgID == i_crm.ORIGID && it.DepartmentID == member.DepartmentID);
                            if (oDept != null)
                            {
                                saSupervisor.Add(oDept.ChiefOfDepartmentID);
                            }
                        }
                        var sBillAuditor = Common.GetSystemSetting(db, i_crm.ORIGID, WebAppGlobalConstWord.BILLAUDITOR);
                        if (sBillAuditor != @"")
                        {
                            var saBillAuditor = sBillAuditor.Split(new string[] { @";", @",", @"，", @"|" }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string auditor in saBillAuditor)
                            {
                                saSupervisor.Add(auditor);
                            }
                        }

                        saSupervisor = saSupervisor.Distinct<string>().ToList();
                        var listTips = new List<OTB_SYS_Tips>();
                        var listTask = new List<OTB_SYS_Task>();

                        if (saSupervisor.Count > 0)
                        {
                            foreach (string supervisor in saSupervisor)
                            {
                                //添加提醒消息
                                var oTipsAdd = SYS.Task_QryService.TipsAdd(i_crm, sTitle, supervisor, i_crm.CUSTOMDATA[@"program_id"] + @"|?Action=Upd&GoTab=9&ImportBillNO=" + oOpm.ImportBillNO + @"&BillNO=" + sBillNO, WebAppGlobalConstWord.BELL);
                                listTips.Add(oTipsAdd);
                                //添加代辦
                                var oTaskAdd = SYS.Task_QryService.TaskAdd(i_crm, sSourceID, supervisor, sTitle, @"ExhibitionImport_Qry", @"?Action=Upd&GoTab=9&ImportBillNO=" + oOpm.ImportBillNO + @"&BillNO=" + sBillNO);
                                listTask.Add(oTaskAdd);
                            }
                        }
                        else
                        {
                            sMsg = @"您沒有對應的直屬主管或部門主管，請核查！";
                            break;
                        }

                        if (listTips.Count > 0)
                        {
                            db.Insertable(listTips).ExecuteCommand();
                        }
                        if (listTask.Count > 0)
                        {
                            db.Insertable(listTask).ExecuteCommand();
                        }

                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, saSupervisor);
                    } while (false);
                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ExhibitionImport_UpdService), @"進口", @"ReturnToAuditForBill（進口（退運）帳單提交審核）", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
                Logger.Debug(@"ExhibitionImport_UpdService.ReturnToAuditForBill Debug（Param：" + JsonToString(i_crm) + @"；Response：" + JsonToString(rm) + @"）------------------");
            }
            return rm;
        }

        #endregion 進口（退運）帳單提交審核

        #region 進口（退運）帳單（主管）審核

        /// <summary>
        /// 進口（退運）帳單（主管）審核
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on ReturnAuditForBill</param>
        /// <returns></returns>
        public ResponseMessage ReturnAuditForBill(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var data = i_crm.DATA as Dictionary<string, object>;
                        var oBill = (JObject)data[@"Bill"];
                        var sImportBillNO = _fetchString(i_crm, OTB_OPM_ImportExhibition.CN_IMPORTBILLNO);
                        var sSourceID = oBill[EasyNetGlobalConstWord.GUID].ToString();
                        var sBillNO = oBill[@"BillNO"].ToString();
                        var TipsType = WebAppGlobalConstWord.CHECK;

                        var sdb = new SimpleClient<OTB_OPM_ImportExhibition>(db);
                        var oOpm = sdb.GetById(sImportBillNO);
                        if (oOpm == null)
                        {
                            sMsg = @"系統找不到對應的基本資料，請核查！";
                            break;
                        }

                        var member = db.Queryable<OTB_SYS_Members>().Single(x => x.OrgID == i_crm.ORIGID && x.MemberID == i_crm.USERID);
                        var sTitle = member.MemberName + @"審核了您的「進口」（活動名稱：" + oOpm.ImportBillName + @"）的「退運帳單（" + sBillNO + @"）」";

                        //更新報價和預估成本
                        var sAuditVal = oBill[@"AuditVal"].ToString();
                        if (sAuditVal == @"2")
                        {
                            sTitle += @"，審核結果：通過";
                        }
                        else
                        {
                            sTitle += @"，審核結果：不通過";
                            TipsType = WebAppGlobalConstWord.FAIL;
                        }
                        if (i_crm.LANG == @"zh")
                        {
                            sTitle = ChineseStringUtility.ToSimplified(sTitle);
                        }
                        var oPayer = new OTB_CRM_Customers();
                        if (!string.IsNullOrEmpty(oBill[@"Payer"].ToString()))
                        {
                            oPayer = db.Queryable<OTB_CRM_Customers>().Single(it => it.guid == oBill[@"Payer"].ToString());
                            if (oPayer == null)
                            {
                                sMsg = @"系統找不到付款人資訊";
                                break;
                            }
                        }

                        //更新報價和預估成本
                        var oOpmUpd = new OTB_OPM_ImportExhibition
                        {
                            ReturnBills = data[OTB_OPM_ImportExhibition.CN_RETURNBILLS].ToString(),
                            ModifyUser = i_crm.USERID,
                            ModifyDate = DateTime.Now
                        };
                        db.Updateable(oOpmUpd).UpdateColumns(it => new { it.ReturnBills, it.ModifyUser, it.ModifyDate })
                            .Where(it => it.ImportBillNO == sImportBillNO).ExecuteCommand();

                        //更新代辦
                        SYS.Task_QryService.TaskStatusUpd(db, i_crm.ORIGID, sSourceID);

                        //添加提醒消息
                        var oTipsAdd = SYS.Task_QryService.TipsAdd(i_crm, sTitle, oOpm.ResponsiblePerson, i_crm.CUSTOMDATA[@"program_id"] + @"|?Action=Upd&GoTab=9&ImportBillNO=" + oOpm.ImportBillNO + @"&BillNO=" + sBillNO, TipsType);
                        db.Insertable(oTipsAdd).ExecuteCommand();
                        if (sAuditVal != @"2")
                        {
                            //添加代辦
                            var oTaskAdd = SYS.Task_QryService.TaskAdd(i_crm, sSourceID, oOpm.ResponsiblePerson, sTitle, @"ExhibitionImport_Qry", @"?Action=Upd&GoTab=9&ImportBillNO=" + oOpm.ImportBillNO + @"&BillNO=" + sBillNO);
                            db.Insertable(oTaskAdd).ExecuteCommand();
                        }

                        if (sAuditVal == @"2")
                        {
                            var oBillsAdd = new OTB_OPM_Bills
                            {
                                OrgID = i_crm.ORIGID,
                                BillNO = oBill[@"BillNO"].ToString(), //帳款號碼(1)
                                CheckDate = Common.FnToTWDate(oBill[@"BillFirstCheckDate"]),//對帳日期(2)
                                BillType = @"20",//帳別(收付)(3)
                                CustomerCode = oPayer.CustomerNO //客戶供應商代號(4)
                            };
                            var sResponsiblePerson = oOpm.ResponsiblePerson.Split('.')[0];
                            oBillsAdd.ResponsiblePersonCode = Common.CutByteString(sResponsiblePerson, 11); //業務員代號(5)
                            oBillsAdd.ResponsiblePersonFullCode = oOpm.ResponsiblePerson; //業務員全代號
                            oBillsAdd.LastGetBillDate = @""; //最近收付日(6)
                            oBillsAdd.LastGetBillNO = @""; //最近收付單號(7)
                            oBillsAdd.TaxType = decimal.Parse(oBill[@"TaxSum"].ToString().Replace(@",", @"")) > 0 ? @"5" : @"6";//稅別(8)
                            oBillsAdd.NOTaxAmount = oBill[@"AmountSum"].ToString(); //未稅金額(9)
                            oBillsAdd.BillAmount = oBill[@"AmountTaxSum"].ToString(); //帳款金額(10)
                            oBillsAdd.PaymentAmount = @"0"; //收付金額(11)
                            oBillsAdd.Allowance = @"0"; //折讓金額(12)
                            oBillsAdd.DebtAmount = @"0"; //呆帳金額(13)
                            oBillsAdd.ExchangeAmount = @"0"; //匯兌損益金額(14)
                            oBillsAdd.Settle = @"N"; //結清否(15)
                            oBillsAdd.InvoiceStartNumber = @""; //發票號碼(起)(16)
                            oBillsAdd.InvoiceEndNumber = @"";//發票號碼(迄)(17)
                            oBillsAdd.Category = @"";//傳票類別(18)
                            oBillsAdd.OrderNo = @"";//訂單單號(19)
                            oBillsAdd.ClosingNote = @"N"; //結帳註記(20)
                            oBillsAdd.GeneralInvoiceNumber = @""; //立帳總帳傳票號碼(21)
                            oBillsAdd.GeneralSerialNumber = @""; //立帳總帳傳票序號(22)
                            oBillsAdd.Remark1 = @""; //備註一(30C)(23)
                            oBillsAdd.AccountSource = @"0"; //帳款來源(24)
                            oBillsAdd.UpdateDate = @""; //更新日期(25)
                            oBillsAdd.UpdatePersonnel = @""; //更新人員(26)
                            oBillsAdd.DepartmentSiteNumber = oOpm.DepartmentID; //部門\ 工地編號(27)
                            if (!string.IsNullOrWhiteSpace(oOpm.ExhibitionNO))
                            {
                                var oExhibition = db.Queryable<OTB_OPM_Exhibition>().Single(it => it.SN == oOpm.ExhibitionNO.ObjToInt());
                                oBillsAdd.ProjectNumber = oExhibition == null ? @"" : oExhibition.ExhibitionCode; //專案\ 項目編號(28)
                            }
                            else
                            {
                                oBillsAdd.ProjectNumber = @""; //專案\ 項目編號(28)
                            }
                            oBillsAdd.TransferBNotes = @""; //轉B 帳註記(29)
                            oBillsAdd.ABNumber = @""; //A|B 帳唯一流水號(30)
                            oBillsAdd.EnterNumber = @""; //進銷單號(31)
                            var sCurrency = oBill[@"Currency"].ToString();
                            if (i_crm.ORIGID == "SG")
                            {
                                if (sCurrency == "RMB")
                                {
                                    sCurrency = "";
                                }
                            }
                            else
                            {
                                if (sCurrency == "NTD")
                                {
                                    sCurrency = "";
                                }
                            }
                            oBillsAdd.ForeignCurrencyCode = sCurrency; //外幣代號(32)
                            var sExchangeRate = oBill[@"ExchangeRate"].ToString();
                            oBillsAdd.ExchangeRate = sExchangeRate; //匯率(33)
                            oBillsAdd.ForeignAmount = (decimal.Parse(oBill[@"AmountTaxSum"].ToString()) * decimal.Parse(sExchangeRate == @"" ? @"1" : sExchangeRate)).ToString(); //外幣金額(34)
                            oBillsAdd.PayAmount = @"0"; //收付沖抵金額(35)
                            oBillsAdd.RefundAmount = @"0"; //退款金額(36)
                            oBillsAdd.PaymentTerms = @""; //收付條件(37)
                            oBillsAdd.AccountDate = Common.FnToTWDate(oBill[@"BillFirstCheckDate"]); //帳款日期(38)
                            oBillsAdd.DCreditCardNumber = @""; //預設信用卡號(39)
                            oBillsAdd.ClosingDate = @""; //結帳日期(40)
                            oBillsAdd.CusField1 = @""; //自定義欄位一(41)
                            oBillsAdd.CusField2 = @""; //自定義欄位二(42)
                            oBillsAdd.CusField3 = @""; //自定義欄位三(43)
                            oBillsAdd.CusField4 = @""; //自定義欄位四(44)
                            oBillsAdd.CusField5 = @""; //自定義欄位五(45)
                            oBillsAdd.CusField6 = @"0"; //自定義欄位六(46)
                            oBillsAdd.CusField7 = @"0"; //自定義欄位七(47)
                            oBillsAdd.CusField8 = @"0"; //自定義欄位八(48)
                            oBillsAdd.CusField9 = @"0"; //自定義欄位九(49)
                            oBillsAdd.CusField10 = @"0";  //自定義欄位十(50)
                            oBillsAdd.CusField11 = @""; //自定義欄位十一(51)
                            oBillsAdd.CusField12 = @""; //自定義欄位十二(52)
                            oBillsAdd.Remark2 = @""; //備註二(M)(53)
                            oBillsAdd.TWNOTaxAmount = oBill[@"AmountSum"].ToString(); //台幣未稅金額(54)
                            oBillsAdd.TWAmount = oBill[@"AmountTaxSum"].ToString(); //台幣帳款金額(55)
                            oBillsAdd.CreateUser = i_crm.USERID;
                            oBillsAdd.CreateDate = oBill[@"CreateDate"] == null ? DateTime.Now.ToString(@"yyyy/MM/dd HH:mm:ss") : oBill[@"CreateDate"].ToString();
                            oBillsAdd.BillFirstCheckDate = oBill[@"BillFirstCheckDate"] == null ? DateTime.Now.ToString(@"yyyy/MM/dd HH:mm:ss") : oBill[@"BillFirstCheckDate"].ToString();
                            oBillsAdd.Advance = oBill[@"Advance"].ToString(); //預收
                            oBillsAdd.TaxSum = oBill[@"TaxSum"].ToString(); //稅額
                            oBillsAdd.TotalReceivable = oBill[@"TotalReceivable"].ToString(); //總應收
                            oBillsAdd.IsRetn = oBill[@"IsRetn"] == null ? @"N" : oBill[@"IsRetn"].ToString(); //是否為退運
                            oBillsAdd.Url = i_crm.CUSTOMDATA[@"program_id"] + @"|?Action=Upd&GoTab=9&ImportBillNO=" + oOpm.ImportBillNO + @"&BillNO=" + sBillNO; //Url
                            db.Insertable(oBillsAdd).ExecuteCommand();
                        }

                        var oLogInfo = new OTB_SYS_LogInfo
                        {
                            OrgID = i_crm.ORIGID,
                            SouseId = sBillNO,
                            LogType = "billnoupdate",
                            LogInfo = data[@"Bill"].ToString(),
                            CreateUser = i_crm.USERID,
                            CreateDate = DateTime.Now,
                            Memo = "「進口（退運）帳單」（主管）審核"
                        };
                        db.Insertable(oLogInfo).ExecuteCommand();

                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, oOpm.ResponsiblePerson);
                    } while (false);
                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ExhibitionImport_UpdService), @"進口", @"ReturnAuditForBill（進口（退運）帳單（主管）審核）", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
                Logger.Debug(@"ExhibitionImport_UpdService.ReturnAuditForBill Debug（Param：" + JsonToString(i_crm) + @"；Response：" + JsonToString(rm) + @"）------------------");
            }
            return rm;
        }

        #endregion 進口（退運）帳單（主管）審核

        #region （會計）取消審核（退運）

        /// <summary>
        /// （會計）取消審核（退運）
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on ReturnCancelAudit</param>
        /// <returns></returns>
        public ResponseMessage ReturnCancelAudit(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var data = i_crm.DATA as Dictionary<string, object>;
                        var oBill = (JObject)data[@"Bill"];
                        var sImportBillNO = _fetchString(i_crm, OTB_OPM_ImportExhibition.CN_IMPORTBILLNO);
                        var sBillNO = oBill[@"BillNO"].ToString();
                        var sSourceID = oBill[EasyNetGlobalConstWord.GUID].ToString();

                        var sdb = new SimpleClient<OTB_OPM_ImportExhibition>(db);
                        var oOpm = sdb.GetById(sImportBillNO);
                        if (oOpm == null)
                        {
                            sMsg = @"系統找不到對應的基本資料，請核查！";
                            break;
                        }
                        var LogData = _fetchString(i_crm, @"LogData");
                        var LogResult = BillLogs.InsertBillChangeLog(db, LogData, ActionType.取消退運審核, i_crm.ORIGID, i_crm.USERID);
                        if (!LogResult.Item1)
                        {
                            sMsg = LogResult.Item2;
                            break;
                        }
                        var member = db.Queryable<OTB_SYS_Members>().Single(x => x.OrgID == i_crm.ORIGID && x.MemberID == i_crm.USERID);
                        var sTitle = @"會計（" + member.MemberName + @"）取消審核了您的「進口」（活動名稱：" + oOpm.ImportBillName + @"）的退運帳單(" + sBillNO + @")」";
                        if (i_crm.LANG == @"zh")
                        {
                            sTitle = ChineseStringUtility.ToSimplified(sTitle);
                        }

                        //更新報價和預估成本
                        var oOpmUpd = new OTB_OPM_ImportExhibition
                        {
                            ReturnBills = data[OTB_OPM_ImportExhibition.CN_RETURNBILLS].ToString(),
                            ModifyUser = i_crm.USERID,
                            ModifyDate = DateTime.Now
                        };
                        db.Updateable(oOpmUpd).UpdateColumns(it => new { it.ReturnBills, it.ModifyUser, it.ModifyDate })
                            .Where(it => it.ImportBillNO == sImportBillNO).ExecuteCommand();

                        //更新代辦
                        SYS.Task_QryService.TaskStatusUpd(db, i_crm.ORIGID, sSourceID);

                        //添加提醒消息
                        var oTipsAdd = SYS.Task_QryService.TipsAdd(i_crm, sTitle, oOpm.ResponsiblePerson, i_crm.CUSTOMDATA[@"program_id"] + @"|?Action=Upd&GoTab=9&ImportBillNO=" + oOpm.ImportBillNO + @"&BillNO=" + sBillNO, WebAppGlobalConstWord.BELL);
                        db.Insertable(oTipsAdd).ExecuteCommand();

                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, oOpm.ResponsiblePerson);
                    } while (false);
                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ExhibitionImport_UpdService), @"進口", @"ReturnCancelAudit（（會計）取消審核（退運））", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
                Logger.Debug(@"ExhibitionImport_UpdService.ReturnCancelAudit Debug（Param：" + JsonToString(i_crm) + @"；Response：" + JsonToString(rm) + @"）------------------");
            }
            return rm;
        }

        #endregion （會計）取消審核（退運）

        #region （會計）銷帳（退運）

        /// <summary>
        /// （會計）銷帳（退運）
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on ReturnWriteOff</param>
        /// <returns></returns>
        public ResponseMessage ReturnWriteOff(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var data = i_crm.DATA as Dictionary<string, object>;
                        var oBill = (JObject)data[@"Bill"];
                        var sImportBillNO = _fetchString(i_crm, OTB_OPM_ImportExhibition.CN_IMPORTBILLNO);
                        var sBillNO = oBill[@"BillNO"].ToString();

                        var sdb = new SimpleClient<OTB_OPM_ImportExhibition>(db);
                        var oOpm = sdb.GetById(sImportBillNO);
                        if (oOpm == null)
                        {
                            sMsg = @"系統找不到對應的基本資料，請核查！";
                            break;
                        }

                        var member = db.Queryable<OTB_SYS_Members>().Single(x => x.OrgID == i_crm.ORIGID && x.MemberID == i_crm.USERID);
                        var sTitle = @"會計（" + member.MemberName + @"）對您的「進口」（活動名稱：" + oOpm.ImportBillName + @"）的退運帳單（" + sBillNO + @"）進行了銷帳";
                        if (i_crm.LANG == @"zh")
                        {
                            sTitle = ChineseStringUtility.ToSimplified(sTitle);
                        }

                        //更新報價和預估成本
                        var oOpmUpd = new OTB_OPM_ImportExhibition
                        {
                            ReturnBills = data[OTB_OPM_ImportExhibition.CN_RETURNBILLS].ToString(),
                            ModifyUser = i_crm.USERID,
                            ModifyDate = DateTime.Now
                        };
                        db.Updateable(oOpmUpd).UpdateColumns(it => new { it.ReturnBills, it.ModifyUser, it.ModifyDate })
                            .Where(it => it.ImportBillNO == sImportBillNO).ExecuteCommand();

                        //添加提醒消息
                        var oTipsAdd = SYS.Task_QryService.TipsAdd(i_crm, sTitle, oOpm.ResponsiblePerson, i_crm.CUSTOMDATA[@"program_id"] + @"|?Action=Upd&GoTab=9&ImportBillNO=" + oOpm.ImportBillNO + @"&BillNO=" + sBillNO, WebAppGlobalConstWord.BELL);
                        db.Insertable(oTipsAdd).ExecuteCommand();

                        db.Ado.CommitTran();
                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, oOpm.ResponsiblePerson);
                    } while (false);
                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ExhibitionImport_UpdService), @"進口", @"ReturnWriteOff（（會計）銷帳（退運））", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
                Logger.Debug(@"ExhibitionImport_UpdService.ReturnWriteOff Debug（Param：" + JsonToString(i_crm) + @"；Response：" + JsonToString(rm) + @"）------------------");
            }
            return rm;
        }

        #endregion （會計）銷帳（退運）

        #region （會計）取消銷帳（退運）

        /// <summary>
        /// （會計）取消銷帳
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on ReturnCancelWriteOff</param>
        /// <returns></returns>
        public ResponseMessage ReturnCancelWriteOff(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var data = i_crm.DATA as Dictionary<string, object>;
                    var oBill = (JObject)data[@"Bill"];
                    var sImportBillNO = _fetchString(i_crm, OTB_OPM_ImportExhibition.CN_IMPORTBILLNO);
                    var sBillNO = oBill[@"BillNO"].ToString();

                    var sdb = new SimpleClient<OTB_OPM_ImportExhibition>(db);
                    var oOpm = sdb.GetById(sImportBillNO);
                    if (oOpm == null)
                    {
                        sMsg = @"系統找不到對應的基本資料，請核查！";
                        break;
                    }
                    var LogData = _fetchString(i_crm, @"LogData");
                    var LogResult = BillLogs.InsertBillChangeLog(db, LogData, ActionType.取消退運銷帳, i_crm.ORIGID, i_crm.USERID);
                    if (!LogResult.Item1)
                    {
                        sMsg = LogResult.Item2;
                        break;
                    }
                    //更新報價和預估成本
                    var oOpmUpd = new OTB_OPM_ImportExhibition
                    {
                        ReturnBills = data[OTB_OPM_ImportExhibition.CN_RETURNBILLS].ToString(),
                        ModifyUser = i_crm.USERID,
                        ModifyDate = DateTime.Now
                    };
                    var iResult = db.Updateable(oOpmUpd).UpdateColumns(it => new { it.ReturnBills, it.ModifyUser, it.ModifyDate })
                        .Where(it => it.ImportBillNO == sImportBillNO).ExecuteCommand();

                    if (iResult > 0)
                    {
                        rm = new SuccessResponseMessage(null, i_crm);
                    }
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ExhibitionImport_UpdService), @"進口", @"ReturnCancelWriteOff（（會計）取消銷帳（退運））", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
                Logger.Debug(@"ExhibitionImport_UpdService.ReturnCancelWriteOff Debug（Param：" + JsonToString(i_crm) + @"；Response：" + JsonToString(rm) + @"）------------------");
            }
            return rm;
        }

        #endregion （會計）取消銷帳（退運）

        #region 過帳（退運）

        /// <summary>
        /// 過帳（退運）
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on ReturnBillPost</param>
        /// <returns></returns>
        public ResponseMessage ReturnBillPost(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var data = i_crm.DATA as Dictionary<string, object>;
                        var oBill = (JObject)data[@"Bill"];
                        var sImportBillNO = _fetchString(i_crm, OTB_OPM_ImportExhibition.CN_IMPORTBILLNO);
                        var sBillNO = oBill[@"BillNO"].ToString();

                        var sdb = new SimpleClient<OTB_OPM_ImportExhibition>(db);
                        var oOpm = sdb.GetById(sImportBillNO);
                        if (oOpm == null)
                        {
                            sMsg = @"系統找不到對應的基本資料，請核查！";
                            break;
                        }

                        var oPayer = new OTB_CRM_Customers();
                        if (!string.IsNullOrEmpty(oBill[@"Payer"].ToString()))
                        {
                            oPayer = db.Queryable<OTB_CRM_Customers>().Single(it => it.guid == oBill[@"Payer"].ToString());
                            if (oPayer == null)
                            {
                                sMsg = @"系統找不到付款人資訊";
                                break;
                            }
                        }

                        var sTitle = @"「進口」（活動名稱：" + oOpm.ImportBillName + @"）的退運帳單（" + sBillNO + @"）已過帳";
                        if (i_crm.LANG == @"zh")
                        {
                            sTitle = ChineseStringUtility.ToSimplified(sTitle);
                        }
                        //更新報價和預估成本
                        var oOpmUpd = new OTB_OPM_ImportExhibition
                        {
                            ReturnBills = data[OTB_OPM_ImportExhibition.CN_RETURNBILLS].ToString(),
                            ModifyUser = i_crm.USERID,
                            ModifyDate = DateTime.Now
                        };
                        db.Updateable(oOpmUpd).UpdateColumns(it => new { it.ReturnBills, it.ModifyUser, it.ModifyDate })
                             .Where(it => it.ImportBillNO == sImportBillNO).ExecuteCommand();

                        var listAccounts = db.Queryable<OTB_SYS_MembersToRule>()
                            .Where(it => it.OrgID == i_crm.ORIGID && it.RuleID == @"Account").ToList();
                        var listTips = new List<OTB_SYS_Tips>();
                        if (listAccounts.Count > 0)
                        {
                            foreach (OTB_SYS_MembersToRule account in listAccounts)
                            {
                                //添加提醒消息
                                var oTipsAdd = SYS.Task_QryService.TipsAdd(i_crm, sTitle, account.MemberID, i_crm.CUSTOMDATA[@"program_id"] + @"|?Action=Upd&GoTab=9&ImportBillNO=" + oOpm.ImportBillNO + @"&BillNO=" + sBillNO, WebAppGlobalConstWord.BELL);
                                listTips.Add(oTipsAdd);
                            }
                        }

                        if (listTips.Count > 0)
                        {
                            db.Insertable(listTips).ExecuteCommand();
                        }

                        var oBillsAdd = new OTB_OPM_Bills
                        {
                            OrgID = i_crm.ORIGID,
                            BillNO = oBill[@"BillNO"].ToString(), //帳款號碼(1)
                            CheckDate = Common.FnToTWDate(oBill[@"BillFirstCheckDate"]),//對帳日期(2)
                            BillType = @"20",//帳別(收付)(3)
                            CustomerCode = oPayer.CustomerNO //客戶供應商代號(4)
                        };
                        var sResponsiblePerson = oOpm.ResponsiblePerson.Split('.')[0];
                        oBillsAdd.ResponsiblePersonCode = Common.CutByteString(sResponsiblePerson, 11); //業務員代號(5)
                        oBillsAdd.ResponsiblePersonFullCode = oOpm.ResponsiblePerson; //業務員全代號
                        oBillsAdd.LastGetBillDate = @""; //最近收付日(6)
                        oBillsAdd.LastGetBillNO = @""; //最近收付單號(7)
                        oBillsAdd.TaxType = decimal.Parse(oBill[@"TaxSum"].ToString().Replace(@",", @"")) > 0 ? @"5" : @"6";//稅別(8)
                        oBillsAdd.NOTaxAmount = oBill[@"AmountSum"].ToString(); //未稅金額(9)
                        oBillsAdd.BillAmount = oBill[@"AmountTaxSum"].ToString(); //帳款金額(10)
                        oBillsAdd.PaymentAmount = @"0"; //收付金額(11)
                        oBillsAdd.Allowance = @"0"; //折讓金額(12)
                        oBillsAdd.DebtAmount = @"0"; //呆帳金額(13)
                        oBillsAdd.ExchangeAmount = @"0"; //匯兌損益金額(14)
                        oBillsAdd.Settle = @"N"; //結清否(15)
                        oBillsAdd.InvoiceStartNumber = oBill[@"InvoiceNumber"].ToString(); //發票號碼(起)(16)
                        oBillsAdd.InvoiceEndNumber = oBill[@"InvoiceNumber"].ToString();//發票號碼(迄)(17)
                        oBillsAdd.Category = @"";//傳票類別(18)
                        oBillsAdd.OrderNo = @"";//訂單單號(19)
                        oBillsAdd.ClosingNote = @"N"; //結帳註記(20)
                        oBillsAdd.GeneralInvoiceNumber = @""; //立帳總帳傳票號碼(21)
                        oBillsAdd.GeneralSerialNumber = @""; //立帳總帳傳票序號(22)
                        oBillsAdd.Remark1 = @""; //備註一(30C)(23)
                        oBillsAdd.AccountSource = @"0"; //帳款來源(24)
                        oBillsAdd.UpdateDate = @""; //更新日期(25)
                        oBillsAdd.UpdatePersonnel = @""; //更新人員(26)
                        oBillsAdd.DepartmentSiteNumber = oOpm.DepartmentID; //部門\ 工地編號(27)
                        if (!string.IsNullOrWhiteSpace(oOpm.ExhibitionNO))
                        {
                            var oExhibition = db.Queryable<OTB_OPM_Exhibition>().Single(it => it.SN == oOpm.ExhibitionNO.ObjToInt());
                            oBillsAdd.ProjectNumber = oExhibition == null ? @"" : oExhibition.ExhibitionCode; //專案\ 項目編號(28)
                        }
                        else
                        {
                            oBillsAdd.ProjectNumber = @""; //專案\ 項目編號(28)
                        }
                        oBillsAdd.TransferBNotes = @""; //轉B 帳註記(29)
                        oBillsAdd.ABNumber = @""; //A|B 帳唯一流水號(30)
                        oBillsAdd.EnterNumber = @""; //進銷單號(31)
                        var sCurrency = oBill[@"Currency"].ToString();
                        if (i_crm.ORIGID == "SG")
                        {
                            if (sCurrency == "RMB")
                            {
                                sCurrency = "";
                            }
                        }
                        else
                        {
                            if (sCurrency == "NTD")
                            {
                                sCurrency = "";
                            }
                        }
                        oBillsAdd.ForeignCurrencyCode = sCurrency; //外幣代號(32)
                        var sExchangeRate = oBill[@"ExchangeRate"].ToString();
                        oBillsAdd.ExchangeRate = sExchangeRate; //匯率(33)
                        oBillsAdd.ForeignAmount = (decimal.Parse(oBill[@"AmountTaxSum"].ToString()) * decimal.Parse(sExchangeRate == @"" ? @"1" : sExchangeRate)).ToString(); //外幣金額(34)
                        oBillsAdd.PayAmount = @"0"; //收付沖抵金額(35)
                        oBillsAdd.RefundAmount = @"0"; //退款金額(36)
                        oBillsAdd.PaymentTerms = @""; //收付條件(37)
                        oBillsAdd.AccountDate = Common.FnToTWDate(oBill[@"BillFirstCheckDate"]); //帳款日期(38)
                        oBillsAdd.DCreditCardNumber = @""; //預設信用卡號(39)
                        oBillsAdd.ClosingDate = @""; //結帳日期(40)
                        oBillsAdd.CusField1 = @""; //自定義欄位一(41)
                        oBillsAdd.CusField2 = @""; //自定義欄位二(42)
                        oBillsAdd.CusField3 = @""; //自定義欄位三(43)
                        oBillsAdd.CusField4 = @""; //自定義欄位四(44)
                        oBillsAdd.CusField5 = @""; //自定義欄位五(45)
                        oBillsAdd.CusField6 = @"0"; //自定義欄位六(46)
                        oBillsAdd.CusField7 = @"0"; //自定義欄位七(47)
                        oBillsAdd.CusField8 = @"0"; //自定義欄位八(48)
                        oBillsAdd.CusField9 = @"0"; //自定義欄位九(49)
                        oBillsAdd.CusField10 = @"0";  //自定義欄位十(50)
                        oBillsAdd.CusField11 = @""; //自定義欄位十一(51)
                        oBillsAdd.CusField12 = @""; //自定義欄位十二(52)
                        oBillsAdd.Remark2 = @""; //備註二(M)(53)
                        oBillsAdd.TWNOTaxAmount = oBill[@"AmountSum"].ToString(); //台幣未稅金額(54)
                        oBillsAdd.TWAmount = oBill[@"AmountTaxSum"].ToString(); //台幣帳款金額(55)
                        oBillsAdd.CreateUser = i_crm.USERID;
                        oBillsAdd.CreateDate = oBill[@"CreateDate"] == null ? DateTime.Now.ToString(@"yyyy/MM/dd HH:mm:ss") : oBill[@"CreateDate"].ToString();
                        oBillsAdd.BillFirstCheckDate = oBill[@"BillFirstCheckDate"] == null ? DateTime.Now.ToString(@"yyyy/MM/dd HH:mm:ss") : oBill[@"BillFirstCheckDate"].ToString();
                        oBillsAdd.Advance = oBill[@"Advance"].ToString(); //預收
                        oBillsAdd.TaxSum = oBill[@"TaxSum"].ToString(); //稅額
                        oBillsAdd.TotalReceivable = oBill[@"TotalReceivable"].ToString(); //總應收
                        oBillsAdd.IsRetn = oBill[@"IsRetn"] == null ? @"N" : oBill[@"IsRetn"].ToString(); //是否為退運
                        oBillsAdd.Url = i_crm.CUSTOMDATA[@"program_id"] + @"|?Action=Upd&GoTab=9&ImportBillNO=" + oOpm.ImportBillNO + @"&BillNO=" + sBillNO; //Url
                        db.Insertable(oBillsAdd).ExecuteCommand();

                        var oLogInfo = new OTB_SYS_LogInfo
                        {
                            OrgID = i_crm.ORIGID,
                            SouseId = sBillNO,
                            LogType = "billnoupdate",
                            LogInfo = data[@"Bill"].ToString(),
                            CreateUser = i_crm.USERID,
                            CreateDate = DateTime.Now,
                            Memo = "「進口（退運）帳單」（會計）過帳"
                        };
                        db.Insertable(oLogInfo).ExecuteCommand();

                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, oOpm.ResponsiblePerson);
                    } while (false);
                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ExhibitionImport_UpdService), @"進口", @"ReturnBillPost（過帳（退運））", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
                Logger.Debug(@"ExhibitionImport_UpdService.ReturnBillPost Debug（Param：" + JsonToString(i_crm) + @"；Response：" + JsonToString(rm) + @"）------------------");
            }
            return rm;
        }

        #endregion 過帳（退運）

        #region 取消過帳（退運）

        /// <summary>
        /// 取消過帳（退運）
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on ReturnBillCancelPost</param>
        /// <returns></returns>
        public ResponseMessage ReturnBillCancelPost(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var data = i_crm.DATA as Dictionary<string, object>;
                        var oBill = (JObject)data[@"Bill"];
                        var sImportBillNO = _fetchString(i_crm, OTB_OPM_ImportExhibition.CN_IMPORTBILLNO);
                        var sBillNO = oBill[@"BillNO"].ToString();

                        var sdb = new SimpleClient<OTB_OPM_ImportExhibition>(db);
                        var oOpm = sdb.GetById(sImportBillNO);
                        if (oOpm == null)
                        {
                            sMsg = @"系統找不到對應的基本資料，請核查！";
                            break;
                        }
                        var LogData = _fetchString(i_crm, @"LogData");
                        var LogResult = BillLogs.InsertBillChangeLog(db, LogData, ActionType.取消退運過帳, i_crm.ORIGID, i_crm.USERID);
                        if (!LogResult.Item1)
                        {
                            sMsg = LogResult.Item2;
                            break;
                        }
                        var member = db.Queryable<OTB_SYS_Members>().Single(x => x.OrgID == i_crm.ORIGID && x.MemberID == i_crm.USERID);
                        var sTitle = @"會計（" + member.MemberName + @"）對您的「進口」（活動名稱：" + oOpm.ImportBillName + @"）的退運帳單（" + sBillNO + @"）取消了過帳";
                        if (i_crm.LANG == @"zh")
                        {
                            sTitle = ChineseStringUtility.ToSimplified(sTitle);
                        }
                        //更新報價和預估成本
                        var oOpmUpd = new OTB_OPM_ImportExhibition
                        {
                            ReturnBills = data[OTB_OPM_ImportExhibition.CN_RETURNBILLS].ToString(),
                            ModifyUser = i_crm.USERID,
                            ModifyDate = DateTime.Now
                        };
                        db.Updateable(oOpmUpd).UpdateColumns(it => new { it.ReturnBills, it.ModifyUser, it.ModifyDate })
                             .Where(it => it.ImportBillNO == sImportBillNO).ExecuteCommand();

                        //添加提醒消息
                        var oTipsAdd = SYS.Task_QryService.TipsAdd(i_crm, sTitle, oOpm.ResponsiblePerson, i_crm.CUSTOMDATA[@"program_id"] + @"|?Action=Upd&GoTab=9&ImportBillNO=" + oOpm.ImportBillNO + @"&BillNO=" + sBillNO, WebAppGlobalConstWord.BELL);
                        db.Insertable(oTipsAdd).ExecuteCommand();

                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, oOpm.ResponsiblePerson);
                    } while (false);
                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ExhibitionImport_UpdService), @"進口", @"ReturnBillCancelPost（取消過帳（退運））", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
                Logger.Debug(@"ExhibitionImport_UpdService.ReturnBillCancelPost Debug（Param：" + JsonToString(i_crm) + @"；Response：" + JsonToString(rm) + @"）------------------");
            }
            return rm;
        }

        #endregion 取消過帳（退運）

        #region 作廢（退運）帳單

        /// <summary>
        /// 作廢（退運）帳單
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on ReturnBillVoid</param>
        /// <returns></returns>
        public ResponseMessage ReturnBillVoid(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var data = i_crm.DATA as Dictionary<string, object>;
                        var oBill = (JObject)data[@"Bill"];
                        var sImportBillNO = _fetchString(i_crm, OTB_OPM_ImportExhibition.CN_IMPORTBILLNO);
                        var sBillNO = oBill[@"BillNO"].ToString();
                        var sSourceID = oBill[EasyNetGlobalConstWord.GUID].ToString();

                        var sdb = new SimpleClient<OTB_OPM_ImportExhibition>(db);
                        var oOpm = sdb.GetById(sImportBillNO);
                        if (oOpm == null)
                        {
                            sMsg = @"系統找不到對應的基本資料，請核查！";
                            break;
                        }
                        var LogData = _fetchString(i_crm, @"LogData");
                        var LogResult = BillLogs.InsertBillChangeLog(db, LogData, ActionType.退運帳單作廢, i_crm.ORIGID, i_crm.USERID);
                        if (!LogResult.Item1)
                        {
                            sMsg = LogResult.Item2;
                            break;
                        }
                        var member = db.Queryable<OTB_SYS_Members>().Single(x => x.OrgID == i_crm.ORIGID && x.MemberID == i_crm.USERID);
                        var sTitle = @"會計（" + member.MemberName + @"）作廢了您的「進口」（活動名稱：" + oOpm.ImportBillName + @"）的退運帳單（" + sBillNO + @"）";
                        if (i_crm.LANG == @"zh")
                        {
                            sTitle = ChineseStringUtility.ToSimplified(sTitle);
                        }
                        //更新報價和預估成本
                        var oOpmUpd = new OTB_OPM_ImportExhibition
                        {
                            ReturnBills = data[OTB_OPM_ImportExhibition.CN_RETURNBILLS].ToString(),
                            ModifyUser = i_crm.USERID,
                            ModifyDate = DateTime.Now
                        };
                        db.Updateable(oOpmUpd).UpdateColumns(it => new { it.ReturnBills, it.ModifyUser, it.ModifyDate })
                            .Where(it => it.ImportBillNO == sImportBillNO).ExecuteCommand();

                        //添加提醒消息
                        var oTipsAdd = SYS.Task_QryService.TipsAdd(i_crm, sTitle, oOpm.ResponsiblePerson, i_crm.CUSTOMDATA[@"program_id"] + @"|?Action=Upd&GoTab=9&ImportBillNO=" + oOpm.ImportBillNO + @"&BillNO=" + sBillNO, WebAppGlobalConstWord.BELL);
                        db.Insertable(oTipsAdd).ExecuteCommand();

                        db.Deleteable<OTB_SYS_Task>().Where(it => it.OrgID == i_crm.ORIGID && it.SourceID == sSourceID).ExecuteCommand();

                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, oOpm.ResponsiblePerson);
                    } while (false);
                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ExhibitionImport_UpdService), @"進口", @"ReturnBillVoid（作廢（退運）帳單）", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
                Logger.Debug(@"ExhibitionImport_UpdService.ReturnBillVoid Debug（Param：" + JsonToString(i_crm) + @"；Response：" + JsonToString(rm) + @"）------------------");
            }
            return rm;
        }

        #endregion 作廢（退運）帳單

        #region 刪除（退運）帳單

        /// <summary>
        /// 刪除（退運）帳單
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on ReturnBillDelete</param>
        /// <returns></returns>
        public ResponseMessage ReturnBillDelete(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var data = i_crm.DATA as Dictionary<string, object>;
                        var oBill = (JObject)data[@"Bill"];
                        var sImportBillNO = _fetchString(i_crm, OTB_OPM_ImportExhibition.CN_IMPORTBILLNO);
                        var sBillNO = oBill[@"BillNO"].ToString();
                        var sSourceID = oBill[EasyNetGlobalConstWord.GUID].ToString();

                        var sdb = new SimpleClient<OTB_OPM_ImportExhibition>(db);
                        var oOpm = sdb.GetById(sImportBillNO);
                        if (oOpm == null)
                        {
                            sMsg = @"系統找不到對應的基本資料，請核查！";
                            break;
                        }
                        var LogData = _fetchString(i_crm, @"LogData");
                        var LogResult = BillLogs.InsertBillChangeLog(db, LogData, ActionType.退運帳單刪除, i_crm.ORIGID, i_crm.USERID);
                        if (!LogResult.Item1)
                        {
                            sMsg = LogResult.Item2;
                            break;
                        }
                        var member = db.Queryable<OTB_SYS_Members>().Single(x => x.OrgID == i_crm.ORIGID && x.MemberID == i_crm.USERID);
                        var sTitle = @"管理員（" + member.MemberName + @"）刪除了您的「進口」（活動名稱：" + oOpm.ImportBillName + @"）的退運帳單（" + sBillNO + @"）";
                        if (i_crm.LANG == @"zh")
                        {
                            sTitle = ChineseStringUtility.ToSimplified(sTitle);
                        }
                        //更新報價和預估成本
                        var oOpmUpd = new OTB_OPM_ImportExhibition
                        {
                            ReturnBills = data[OTB_OPM_ImportExhibition.CN_RETURNBILLS].ToString(),
                            ModifyUser = i_crm.USERID,
                            ModifyDate = DateTime.Now
                        };
                        db.Updateable(oOpmUpd).UpdateColumns(it => new { it.ReturnBills, it.ModifyUser, it.ModifyDate })
                           .Where(it => it.ImportBillNO == sImportBillNO).ExecuteCommand();

                        //添加提醒消息
                        var oTipsAdd = SYS.Task_QryService.TipsAdd(i_crm, sTitle, oOpm.ResponsiblePerson, i_crm.CUSTOMDATA[@"program_id"] + @"|?Action=Upd&GoTab=9&ImportBillNO=" + oOpm.ImportBillNO + @"&BillNO=" + sBillNO, WebAppGlobalConstWord.BELL);
                        db.Insertable(oTipsAdd).ExecuteCommand();

                        db.Deleteable<OTB_SYS_Task>().Where(it => it.OrgID == i_crm.ORIGID && it.SourceID == sSourceID).ExecuteCommand();

                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, oOpm.ResponsiblePerson);
                    } while (false);
                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ExhibitionImport_UpdService), @"進口", @"ReturnBillDelete（刪除（退運）帳單）", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
                Logger.Debug(@"ExhibitionImport_UpdService.ReturnBillDelete Debug（Param：" + JsonToString(i_crm) + @"；Response：" + JsonToString(rm) + @"）------------------");
            }
            return rm;
        }

        #endregion 刪除（退運）帳單

        #region 更新帳單明細

        /// <summary>
        /// 更新帳單明細
        /// </summary>
        /// <param name="i_crm">帳單資料</param>
        /// <returns></returns>
        public ResponseMessage UpdateBillInfo(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sId = _fetchString(i_crm, @"Guid");
                    var sBillNO = _fetchString(i_crm, @"BillNO");

                    var sdb = new SimpleClient<OTB_OPM_ImportExhibition>(db);
                    var oOpm = sdb.GetById(sId);
                    if (oOpm == null)
                    {
                        break;
                    }

                    var jaBills = new JArray();
                    var jaReturns = new JArray();
                    jaBills = (JArray)JsonConvert.DeserializeObject(oOpm.Bills);
                    jaReturns = (JArray)JsonConvert.DeserializeObject(oOpm.ReturnBills);
                    foreach (JObject jo in jaReturns)
                    {
                        var jaReBills = new JArray();
                        jaReBills = (JArray)JsonConvert.DeserializeObject(jo[@"Bills"].ToString());
                        foreach (JObject jobill in jaReBills)
                        {
                            jaBills.Add(jobill);
                        }
                    }

                    if (jaBills.Count > 0)
                    {
                        foreach (JObject jo in jaBills)
                        {
                            var oBillInfo_New = ExhibitionService.GetNewBillInfo(jo);
                            oBillInfo_New.OrgID = i_crm.ORIGID;
                            oBillInfo_New.BillType = i_crm.MODULE;
                            oBillInfo_New.ParentId = sId;
                            oBillInfo_New.RefNumber = oOpm.RefNumber;
                            oBillInfo_New.ExhibitionNO = oOpm.ExhibitionNO;
                            oBillInfo_New.ResponsiblePerson = oOpm.ResponsiblePerson;
                            oBillInfo_New.ModifyDate = DateTime.Now;
                            oBillInfo_New.ModifyUser = i_crm.USERID;
                            oBillInfo_New.AuditVal = jo[@"AuditVal"].ToString();
                            oBillInfo_New.ReFlow = jo[@"ReImportNum"] == null ? @"" : jo[@"ReImportNum"].ToString();
                            if (string.IsNullOrEmpty(sBillNO))
                            {
                                var s_BillNO = jo[@"BillNO"].ToString();
                                var oBillInfo = db.Queryable<OTB_OPM_BillInfo>()
                                    .Single(it => it.OrgID == i_crm.ORIGID && it.BillNO == s_BillNO);
                                if (oBillInfo == null)
                                {
                                    oBillInfo_New.CreateDate = DateTime.Now;
                                    oBillInfo_New.CreateUser = i_crm.USERID;
                                    db.Insertable(oBillInfo_New).ExecuteCommand();
                                }
                                else
                                {
                                    oBillInfo_New.CreateDate = oBillInfo.CreateDate;
                                    oBillInfo_New.CreateUser = oBillInfo.CreateUser;
                                    db.Updateable(oBillInfo_New).Where(it => it.SN == oBillInfo.SN).ExecuteCommand();
                                }
                            }
                            else
                            {
                                if (sBillNO == jo[@"BillNO"].ToString())
                                {
                                    var s_BillNO = jo[@"BillNO"].ToString();
                                    var oBillInfo = db.Queryable<OTB_OPM_BillInfo>()
                                        .Single(it => it.OrgID == i_crm.ORIGID && it.BillNO == sBillNO);
                                    if (oBillInfo == null)
                                    {
                                        oBillInfo_New.CreateDate = DateTime.Now;
                                        oBillInfo_New.CreateUser = i_crm.USERID;
                                        db.Insertable(oBillInfo_New).ExecuteCommand();
                                    }
                                    else
                                    {
                                        oBillInfo_New.CreateDate = oBillInfo.CreateDate;
                                        oBillInfo_New.CreateUser = oBillInfo.CreateUser;
                                        db.Updateable(oBillInfo_New).Where(it => it.SN == oBillInfo.SN).ExecuteCommand();
                                    }
                                    break;
                                }
                            }
                        }
                    }

                    rm = new SuccessResponseMessage(null, i_crm);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ExhibitionImport_UpdService), @"其他", @"UpdateBillInfo（更新帳單明細）", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
                Logger.Debug(@"ExhibitionImport_UpdService.UpdateBillInfo Debug（Param：" + JsonToString(i_crm) + @"；Response：" + JsonToString(rm) + @"）------------------");
            }
            return rm;
        }

        #endregion 更新帳單明細
    }
}