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

namespace EasyBL.WEBSITE.TG
{
    public class TGAPIService : ServiceBase
    {
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
                    var oImportCustomers = db.Queryable<OTB_CRM_ImportCustomers>()
                        .First(x => x.OrgID == i_crm.ORIGID && x.ExhibitionNO.ToString() == sExhibitionNO && x.CustomerCName == sCompName);

                    var oPackingOrder = new OTB_WSM_PackingOrder
                    {
                        AppointNO = SerialNumber.GetMaxNumberByType(i_crm.ORIGID, oExhibition.ExhibitionCode, MaxNumberType.Empty, i_crm.USERID, 3, sCurDate),
                        OrgID = i_crm.ORIGID,
                        ExhibitionNO = sExhibitionNO,
                        CustomerId = oImportCustomers?.guid,
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
                        AppointDateTime = DateTime.Now,
                        CreateDate = DateTime.Now,
                        ModifyDate = DateTime.Now
                    };

                    //獲取Email郵件格式
                    var sTemplId = "Appoint_TG" + (i_crm.LANG == "en" ? "_en" : "");
                    var oEmailTempl = db.Queryable<OTB_SYS_Email>().Single(it => it.OrgID == i_crm.ORIGID && it.EmailID == sTemplId);

                    if (oEmailTempl != null)
                    {
                        //寄信開始
                        var sEmailBody = oEmailTempl.BodyHtml
                                           .Replace("{{:AppointNO}}", oPackingOrder.AppointNO)
                                           .Replace("{{:CompName}}", oPackingOrder.CompName)
                                           .Replace("{{:ExpoName}}", i_crm.LANG == "en" ? oExhibition.Exhibitioname_EN : oExhibition.Exhibitioname_TW)
                                           .Replace("{{:MuseumMumber}}", oPackingOrder.MuseumMumber)
                                           .Replace("{{:AppointUser}}", oPackingOrder.AppointUser)
                                           .Replace("{{:AppointTel}}", oPackingOrder.AppointTel)
                                           .Replace("{{:AppointEmail}}", oPackingOrder.AppointEmail)
                                           .Replace("{{:Contactor}}", oPackingOrder.Contactor)
                                           .Replace("{{:ContactTel}}", oPackingOrder.ContactTel)
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
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WebSite.TG.TGAPIService", "", "Appoint（在線預約）", "", "", "");
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
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WebSite.TG.TGAPIService", "", "GetAppointInfo(獲取預約明細)", "", "", "");
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