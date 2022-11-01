using EasyBL.WebApi.Message;
using Entity.Sugar;
using HtmlAgilityPack;
using SqlSugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Services.Protocols;

namespace EasyBL
{
    public class MailService : ServiceBase
    {
        #region Declare Region

        private string _sErrorMessages = string.Empty;

        #endregion Declare Region

        #region Property Region

        /// <summary>
        /// 組織ID
        /// </summary>
        public string OrgID { get; set; } = string.Empty;

        /// <summary>
        /// 組織ID
        /// </summary>
        public bool IsConfig { get; set; } = false;

        /// <summary>
        /// 錯誤信息
        /// </summary>
        public string ErrorMessages
        {
            set { _sErrorMessages += value + ";"; }
            get { return _sErrorMessages; }
        }

        #endregion Property Region

        public readonly EasyNet.Common.Map mailSetting = new EasyNet.Common.Map();

        public MailService(string orgid, bool isconfig = false)
        {
            OrgID = orgid;
            IsConfig = isconfig;
            var array = new string[] { @"MailEncoding", "FromName", "FromEmail", "FromUserId", "FromPassword", "Server", "ServerPort", "Timeout", "SSL" };
            mailSetting = Common.GetSystemSettings(OrgID, array);
        }

        #region 郵件寄送

        /// <summary>
        /// 郵件寄送
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on SendMail</param>
        /// <returns></returns>
        public ResponseMessage SendMail(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                do
                {
                    //string sParams = _fetchString(i_crm, "Params");
                    var bSend = SendMail(i_crm.DATA["Params"], out sMsg);

                    if (sMsg != null)
                    {
                        break;
                    }
                } while (false);
                rm = new SuccessResponseMessage(null, i_crm);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(MailService), nameof(MailService), "SendMail（郵件寄送）", "", "", "");
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

        #endregion 郵件寄送

        public bool SendMail(Object Params, out string o_sError)
        {
            var bSend = false;
            string sError = null;
            try
            {
                do
                {
                    var db = SugarBase.GetIntance();
                    var mail = new Emails();
                    var dic = Params as Dictionary<string, object>;

                    if (!dic.ContainsKey(BLWording.EMAILTO))
                    {
                        sError = "缺少必要的參數：‘" + BLWording.EMAILTO + "’"; break;
                    }

                    if (!dic.ContainsKey(BLWording.MAILTEMPID) && !dic.ContainsKey(BLWording.EMAILBODY))
                    {
                        sError = "缺少必要的參數：‘" + BLWording.MAILTEMPID + "’ 或 ‘" + BLWording.EMAILBODY + "’"; break;
                    }

                    if (dic.ContainsKey(BLWording.EMAILBODY) && !dic.ContainsKey(BLWording.TITLE))
                    {
                        sError = "如果不是配置郵件模板，‘" + BLWording.TITLE + "’是必要的參數."; break;
                    }

                    if (dic.ContainsKey(BLWording.FROMUSERID) && !dic.ContainsKey(BLWording.FROMEMAIL))
                    {
                        //抓取當前使用者資料
                        var sFromUserId = dic[BLWording.FROMUSERID].ToString();
                        var saUserFrom = db.UnionAll(
                            db.Queryable<OTB_SYS_Members>().Where(it => it.Effective == "Y").Select(x => new AllUsers
                            {
                                OrgID = x.OrgID,
                                MemberID = x.MemberID,
                                MemberName = x.MemberName,
                                Email = x.Email
                            }),
                            db.Queryable<OTB_CRM_Customers>().Where(it => it.Effective == "Y").Select(x => new AllUsers
                            {
                                OrgID = x.OrgID,
                                MemberID = x.guid,
                                MemberName = SqlFunc.IIF(SqlFunc.HasValue(x.CustomerCName), x.CustomerCName, x.CustomerEName),
                                Email = x.Email
                            }))
                            .Where(it => it.MemberID == sFromUserId && it.OrgID == OrgID).ToList()
                            .ToList();
                        if (saUserFrom.Count == 0)
                        {
                            sError = "發件人信息有誤";
                            break;
                        }
                        var oUserFrom = saUserFrom.First();
                        mail.FromUserID = oUserFrom.MemberID;
                        mail.FromUserName = oUserFrom.MemberName;
                        mail.FromEmail = oUserFrom.Email;
                    }

                    if (dic.ContainsKey(BLWording.FROMUSERNAME) && string.IsNullOrWhiteSpace(mail.FromUserName))
                    {
                        mail.FromUserName = dic[BLWording.FROMUSERNAME].ToString();
                    }

                    if (dic.ContainsKey(BLWording.FROMEMAIL) && string.IsNullOrWhiteSpace(mail.FromEmail))
                    {
                        mail.FromEmail = dic[BLWording.FROMEMAIL].ToString();
                    }

                    var Tos = new List<EmailTo>();
                    var listTo = dic[BLWording.EMAILTO] as Object[];
                    var sToUserName = "";
                    var builder = new StringBuilder();
                    builder.Append(sToUserName);
                    foreach (Object mailto in listTo)
                    {
                        var dicto = mailto as Dictionary<string, object>;
                        var to = new EmailTo();

                        if (dicto.Keys.Contains("ToEmail"))
                        {
                            to.ToUserName = dicto["ToUserName"].ToString();
                            to.ToEmail = dicto["ToEmail"].ToString();
                        }
                        else
                        {
                            //抓取當前使用者資料
                            var sToUserId = dic[BLWording.TOUSERID].ToString();
                            var saUserTo = db.UnionAll(
                                db.Queryable<OTB_SYS_Members>().Where(it => it.Effective == "Y").Select(x => new AllUsers
                                {
                                    OrgID = x.OrgID,
                                    MemberID = x.MemberID,
                                    MemberName = x.MemberName,
                                    Email = x.Email
                                }),
                                db.Queryable<OTB_CRM_Customers>().Where(it => it.Effective == "Y").Select(x => new AllUsers
                                {
                                    OrgID = x.OrgID,
                                    MemberID = x.guid,
                                    MemberName = SqlFunc.IIF(SqlFunc.HasValue(x.CustomerCName), x.CustomerCName, x.CustomerEName),
                                    Email = x.Email
                                }))
                                .Where(it => it.MemberID == sToUserId && it.OrgID == OrgID).ToList()
                                .ToList();
                            if (saUserTo.Count == 0)
                            {
                                break;
                            }
                            var oUserTo = saUserTo.First();
                            to.ToUserID = oUserTo.MemberID;
                            to.ToUserName = oUserTo.MemberName;
                            to.ToEmail = oUserTo.Email;
                        }
                        to.Type = (dicto["Type"] != null && dicto["Type"].ToString() != "") ? dicto["Type"].ToString() : nameof(to);
                        builder.Append(to.ToUserName + ",");
                        Tos.Add(to);
                    }
                    sToUserName = builder.ToString();
                    sToUserName = (sToUserName + ",").Replace(",,", "");
                    if (Tos.Count == 0) { sError = "收件人信息有誤"; break; }
                    mail.EmailTo = Tos;

                    if (dic.ContainsKey(BLWording.EMAILBODY))
                    {
                        mail.EmailBody = dic[BLWording.EMAILBODY].ToString();
                    }
                    else
                    {
                        //獲取Email郵件格式
                        var sEmailTemplId = dic[BLWording.MAILTEMPID].ToString();
                        var oEmailTempl = new OTB_SYS_Email();
                        var saEmailTempl = db.Queryable<OTB_SYS_Email>()
                            .Where(it => it.EmailID == sEmailTemplId && it.OrgID == OrgID).ToList();
                        if (saEmailTempl.Count == 0)
                        {
                            sError = "找不到對應的模板";
                            break;
                        }
                        oEmailTempl = saEmailTempl.First();

                        mail.EmailBody = oEmailTempl.BodyHtml;
                        mail.Title = oEmailTempl.EmailSubject;
                    }

                    if (dic.ContainsKey(BLWording.TITLE))
                    {
                        mail.Title = dic[BLWording.TITLE].ToString();
                    }

                    if (dic.ContainsKey(BLWording.ATTACHMENTS))
                    {
                        mail.Attachments = dic[BLWording.ATTACHMENTS] as Object[];
                    }

                    var sBaseUrl = HttpContext.Current.Request.Url.ToString().Replace(HttpContext.Current.Request.RawUrl, "");

                    mail.EmailBody = mail.EmailBody.Replace("{{:Url}}", sBaseUrl).Replace("{{:FromUserName}}", mail.FromUserName);

                    if (dic.ContainsKey(BLWording.MAILDATA))
                    {
                        var dicData = dic[BLWording.MAILDATA] as Dictionary<string, object>;
                        foreach (var key in dicData.Keys)
                        {
                            mail.EmailBody = mail.EmailBody.Replace("{{:" + key + "}}", dicData[key].ToString());
                        }
                        mail.EmailBody = mail.EmailBody.Replace("{{:ToUserName}}", sToUserName);
                    }

                    //處理需要隱藏的tr標籤
                    var doc = new HtmlDocument();
                    doc.LoadHtml(mail.EmailBody);
                    foreach (HtmlNode NodeTb in doc.DocumentNode.SelectNodes("//tr"))
                    {
                        if (NodeTb.Attributes["style"] != null && NodeTb.Attributes["style"].Value.IndexOf("display:none") > -1)
                        {
                            NodeTb.Remove();
                        }
                    }
                    mail.EmailBody = doc.DocumentNode.OuterHtml;  //總模版

                    bSend = MailFactory(mail, out sError);
                } while (false);
                return bSend;
            }
            catch (Exception ex)
            {
                sError = ex.Message;
                Logger.Error("MailService.SendMail Error:" + sError + "  Params：" + JsonToString(Params), ex);
                return bSend;
            }
            finally
            {
                o_sError = sError;
            }
        }

        public bool MailFactory(Emails i_oEmail, out string o_sError)
        {
            var bSend = false;
            string sError = null;
            try
            {
                do
                {
                    var objEmail = new MailMessage();
                    //**寄件者信箱**
                    var sFromName = mailSetting["FromName"] != null ? mailSetting["FromName"].ToString() : Common.GetAppSettings("FromName").Trim();         //顯示發件人的名字代替Mail地址
                    var sMailFrom = mailSetting["FromEmail"] != null ? mailSetting["FromEmail"].ToString() : Common.GetAppSettings("FromEmail").Trim();       //發件人地址
                    sMailFrom = i_oEmail.FromEmail ?? sMailFrom;    //發件人地址
                    sFromName = i_oEmail.FromUserName ?? sFromName;  //系統郵件[請勿回覆]

                    var o_MailFrom = new MailAddress(sMailFrom, sFromName, Encoding.Default);
                    objEmail.From = o_MailFrom;

                    //**收件者信箱**//
                    objEmail.To.Clear();
                    objEmail.CC.Clear();
                    objEmail.Bcc.Clear();
                    MailAddress o_MailTo;
                    foreach (EmailTo _EmailTo in i_oEmail.EmailTo)
                    {
                        if (_EmailTo.ToEmail != "")
                        {
                            o_MailTo = new MailAddress(_EmailTo.ToEmail, _EmailTo.ToUserName);
                            switch (_EmailTo.Type)
                            {
                                case "to":
                                    objEmail.To.Add(o_MailTo);
                                    break;

                                case "cc":
                                    objEmail.CC.Add(o_MailTo);
                                    break;

                                case "bcc":
                                    objEmail.Bcc.Add(o_MailTo);
                                    break;

                                default:
                                    break;
                            }
                        }
                    }
                    if (i_oEmail.Attachments != null && i_oEmail.Attachments.Length > 0)
                    {
                        foreach (string Attachment in i_oEmail.Attachments)
                        {
                            var sPath = Regex.Replace(Attachment, @"//|/", @"\");
                            var attachment = new Attachment(sPath);//<-這是附件部分~先用附件的物件把路徑指定進去~
                            objEmail.Attachments.Add(attachment); //<-郵件訊息中加入附件
                        }
                    }
                    //**信件標題**//
                    objEmail.Subject = i_oEmail.Title;
                    //**信件主旨**//
                    objEmail.Body = i_oEmail.EmailBody;
                    objEmail.Body = HttpContext.Current.Server.HtmlDecode(objEmail.Body);
                    objEmail.IsBodyHtml = true;
                    //'**设置正文的编码形式.这里的设置为取系统默认编码
                    //objEmail.BodyEncoding = System.Text.Encoding.Default;
                    objEmail.BodyEncoding = Encoding.UTF8;
                    //'**设置主题的编码形式.这里的设置为取系统默认编码
                    //objEmail.SubjectEncoding = System.Text.Encoding.Default;
                    objEmail.SubjectEncoding = Encoding.UTF8;
                    objEmail.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                    //**信件送出**
                    if (i_oEmail.IsCCSelf && i_oEmail.FromEmail != null && i_oEmail.FromEmail != "")
                    {
                        bSend = this.SendMailNET(objEmail);
                    }
                    else
                    {
                        bSend = this.SendMailNET_NoSelf(objEmail);
                    }
                } while (false);
                return bSend;
            }
            catch (Exception ex)
            {
                sError = ex.Message;
                Logger.Error("MailService.MailFactory Error:" + sError + "  Params：" + JsonToString(i_oEmail), ex);
                return bSend;
            }
            finally
            {
                o_sError = sError;
            }
        }

        #region Definition Public Methods

        #region SendMail

        /// <summary>
        /// 同步發送電子郵件
        /// </summary>
        /// <param name="o_myMessage">電子郵件</param>
        [SoapRpcMethod(OneWay = true)]
        public void SendMail(MailMessage o_myMessage)
        {
            #region "重構在 GetEmailConfig()"

            var o_myMailServer = GetEmailConfig(); //new SmtpClient(sServer);//寄件服務器地址,端口

            #endregion "重構在 GetEmailConfig()"

            try
            {
                object userState = o_myMailServer;

                //o_myMailServer.SendCompleted += new SendCompletedEventHandler(SendCompletedCallback);

                o_myMailServer.SendAsync(o_myMessage, userState);
            }
            catch (SmtpException ex)
            {
                ErrorMessages = ex.Message;
                Logger.Error(ex);
            }
        }

        /// <summary>
        /// 同步發送電子郵件組
        /// </summary>
        /// <param name="o_myMessage">電子郵件組</param>
        [SoapRpcMethod(OneWay = true)]
        public void SendMail(MailMessage[] o_myMessage)
        {
            try
            {
                for (int i = 0; i <= o_myMessage.Length - 1; i++)
                {
                    //Add by Alina 20131216 170 146 信件通知超連結 Star

                    o_myMessage[i].Body = o_myMessage[i].Body;
                    //Add by Alina 20131216 170 146 信件通知超連結 End
                    SendMail(o_myMessage[i]);//發送
                }
            }
            catch (SmtpException ex)
            {
                ErrorMessages = ex.Message;
                Logger.Error(ex);
            }
        }

        #endregion SendMail

        #region SendMailNET

        /// <summary>
        /// 同步發送電子郵件
        /// </summary>
        /// <param name="o_myMessage">電子郵件</param>
        /// <returns></returns>
        public bool SendMailNET_NoSelf(MailMessage o_myMessage)
        {
            try
            {
                Logger.Debug("發送郵件開始");
                var o_myMailServer = GetEmailConfig(); //new SmtpClient(sServer);//寄件服務器地址,端口

                //發送Mail
                o_myMailServer.Send(o_myMessage);
                Logger.Debug("FormName:" + o_myMessage.From.DisplayName + ";FormMail:" + o_myMessage.From.Address);
                Logger.Debug("發送郵件結束");
                return true;
            }
            catch (SmtpException ex)
            {
                ErrorMessages = ex.Message;
                Logger.Error(ex);
                return false;
            }
        }

        /// <summary>
        /// 同步發送電子郵件
        /// </summary>
        /// <param name="o_myMessage">電子郵件</param>
        /// <returns></returns>
        public bool SendMailNET(MailMessage o_myMessage)
        {
            try
            {
                Logger.Debug("發送郵件開始");
                var o_myMailServer = GetEmailConfig(); //new SmtpClient(sServer);//寄件服務器地址,端口

                //Add by Alina  需要把發件者的信息添加到抄送人中
                var reg = new Regex(@"(\w*)(\()*");
                var strFromName = o_myMessage.From.DisplayName;//人員名稱
                var strFromMail = o_myMessage.From.Address;//人員Mail
                var m = reg.Match(strFromName);
                strFromName = m.Groups[1].Value;
                var o_MailTo = new MailAddress(strFromMail, strFromName);
                o_myMessage.From = o_MailTo;//發件者的信息
                o_myMessage.CC.Add(o_myMessage.From);//把發件者的信息添加到抄送人中
                //發送Mail
                Logger.Debug("FormName:" + o_myMessage.From.DisplayName + ";FormMail:" + o_myMessage.From.Address);
                o_myMailServer.Send(o_myMessage);
                return true;
            }
            catch (SmtpException ex)
            {
                ErrorMessages = ex.Message;
                Logger.Error(ex);
                return false;
            }
        }

        /// <summary>
        /// 同步發送電子郵件組
        /// </summary>
        /// <param name="o_myMessage">電子郵件組</param>
        /// <returns></returns>
        public bool SendMailNET(MailMessage[] o_myMessage)
        {
            var blOK = true;
            try
            {
                for (int i = 0; i <= o_myMessage.Length - 1; i++)
                {
                    var blThisOK = SendMailNET(o_myMessage[i]);//發送
                    if (blThisOK == false)
                    {
                        //只記錄發送失敗的
                        blOK = false;
                    }
                }
            }
            catch (SmtpException ex)
            {
                ErrorMessages = ex.Message;
                Logger.Error(ex);
            }
            return blOK;
        }

        #endregion SendMailNET

        #endregion Definition Public Methods

        #region Definition Private Methods

        #region "GetEmailConfig"

        /// <summary>
        /// 提供本類中其它公有郵件功能,取得基本smtp設定.
        /// </summary>
        /// <returns></returns>
        private SmtpClient GetEmailConfig()
        {
            var sMailEncoding = IsConfig || mailSetting["MailEncoding"] == null ? ConfigurationManager.AppSettings["MailEncoding"].ToString().Trim() : mailSetting["MailEncoding"].ToString().Trim(); //設置字集防止亂碼
            var sFromUserid = IsConfig || mailSetting["FromUserId"] == null ? ConfigurationManager.AppSettings["FromUserId"].ToString().Trim() : mailSetting["FromUserId"].ToString().Trim();     //發件人的帳號
            var sFromPassword = IsConfig || mailSetting["FromPassword"] == null ? ConfigurationManager.AppSettings["FromPassword"].Trim() : mailSetting["FromPassword"].ToString(); //發件人的密碼

            var sServer = IsConfig || mailSetting["Server"] == null ? ConfigurationManager.AppSettings["Server"].ToString().Trim() : mailSetting["Server"].ToString().Trim();             //發送郵件服務器的IP地址
            int i32ServerPort;
            int.TryParse(IsConfig || mailSetting["ServerPort"] == null ? ConfigurationManager.AppSettings["ServerPort"].ToString().Trim() : mailSetting["ServerPort"].ToString().Trim(), out i32ServerPort);       //發送郵件服務器的端口
            int iTimeout;
            int.TryParse(IsConfig || mailSetting["Timeout"] == null ? ConfigurationManager.AppSettings["Timeout"].ToString().Trim() : mailSetting["Timeout"].ToString(), out iTimeout);               //發送郵件超時時間
            var blSSLYES = (IsConfig || mailSetting["SSL"] == null ? ConfigurationManager.AppSettings["SSL"].ToString().Trim() : mailSetting["SSL"].ToString().Trim()).Equals("true") ? true : false;//是否開啟SSL驗證

            var o_myMailServer = new SmtpClient(sServer)
            {
                Host = sServer
            };//寄件服務器地址,端口
            if (i32ServerPort != 0)
            {
                o_myMailServer.Port = i32ServerPort;
            }
            o_myMailServer.EnableSsl = blSSLYES;//是否開啟SSL驗證
            //o_myMailServer.DeliveryMethod = SmtpDeliveryMethod.Network;
            if (iTimeout != 0)
            {
                o_myMailServer.Timeout = iTimeout * 1000;  //若郵件過大,請手動加長時間, 1000約等於1秒
            }
            if (!sFromPassword.Equals(""))
            {
                o_myMailServer.UseDefaultCredentials = false;
                o_myMailServer.Credentials = new System.Net.NetworkCredential(sFromUserid, sFromPassword);//登錄會員名和密碼
            }
            return o_myMailServer;
        }

        #endregion "GetEmailConfig"

        #endregion Definition Private Methods

        #region Page Control Event

        //#region SendMail
        //private void test()
        //{
        //    System.Net.Mail.MailMessage objEmail = new System.Net.Mail.MailMessage();

        // //**寄件者信箱** string strMailFrom = ""; string FromName = ""; if (strMailFrom.Equals("") ==
        // false) { System.Net.Mail.MailAddress o_MailFrom = new
        // System.Net.Mail.MailAddress(strMailFrom, FromName, Encoding.Default); objEmail.From =
        // o_MailFrom; }

        // //**收件者信箱** string strMailTo = ""; if (strMailTo.Equals("") == false) {
        // System.Net.Mail.MailAddress o_MailTo = new System.Net.Mail.MailAddress(strMailTo);
        // objEmail.To.Add(o_MailTo); }

        // //**副本收件者信箱** string strMailCC = ""; if (strMailCC.Equals("") == false) { //隱藏副本收件者信箱不為空
        // objEmail.CC.Add(strMailCC); }

        // //**隱藏副本收件者信箱** string strMailbcc = ""; if (strMailbcc.Equals("") == false) {
        // //隱藏副本收件者信箱不為空 objEmail.Bcc.Add(strMailbcc); }

        // //**信件主旨** objEmail.Subject = "信件主旨";

        // //**信件主體(HTML模式)** //objEmail.HTMLbody = ""

        // //**信件主體(文字模式)** objEmail.Body = "信件主體";

        // //'**设置正文的编码形式.这里的设置为取系统默认编码 objEmail.BodyEncoding = System.Text.Encoding.Default;

        // //'**设置主题的编码形式.这里的设置为取系统默认编码 objEmail.SubjectEncoding = System.Text.Encoding.Default;

        // //'**描述Mail傳遞告知選項 // Delay：通知传送是否延迟。 // Never：从不通知。 // None：没有通知。 // OnFailure：通知传送是否失败。
        // // OnSuccess：通知传送是否成功。 objEmail.DeliveryNotificationOptions = System.Net.Mail.DeliveryNotificationOptions.OnFailure;

        // //**信件送出** SendMailNET(objEmail); SendMailAsync(objEmail);

        //}
        //#endregion

        #endregion Page Control Event
    }

    public class Emails
    {
        public string FromOrgID { get; set; }
        public string FromUserID { get; set; }
        public string FromUserName { get; set; }
        public string FromEmail { get; set; }
        public string Title { get; set; }
        public string EmailBody { get; set; }
        public bool IsCCSelf { get; set; }
        public List<EmailTo> EmailTo { get; set; }
        public object[] Attachments { get; set; }
    }

    public class EmailTo
    {
        public string ToUserID { get; set; }
        public string ToUserName { get; set; }
        public string ToEmail { get; set; }
        public string Type { get; set; }
    }

    public class AllUsers
    {
        public string OrgID { get; set; }
        public string MemberID { get; set; }
        public string MemberName { get; set; }
        public string Email { get; set; }
    }

    public class OutlookDotComMail
    {
        //string mailUser = "dd24166@outlook.com";
        //string mailUserPwd = "654321zzz";

        //var sender = new OutlookDotComMail(mailUser, mailUserPwd);
        //sender.SendMail("john.yuan@origtek.com.cn", "Test Mail", "Hello!");
        //string mailUser = "info@eurotran.com";
        //string mailUserPwd = "@in27856000!";

        //var sender = new OutlookDotComMail(mailUser, mailUserPwd);
        //sender.SendMail("john.yuan@origtek.com.cn", "Test Mail!", "Hello!!!!");

        private string _sender = "";
        private string _password = "";

        public OutlookDotComMail(string sender, string password)
        {
            _sender = sender;
            _password = password;
        }

        public void SendMail(string recipient, string subject, string message)
        {
            //SmtpClient client = new SmtpClient("smtp-mail.outlook.com");
            using (var client = new SmtpClient("smtp.office365.com")
            {
                Port = 587,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            })
            {
                var credentials = new System.Net.NetworkCredential(_sender, _password);
                client.EnableSsl = true;
                client.Credentials = credentials;

                try
                {
                    var mail = new MailMessage(_sender.Trim(), recipient.Trim())
                    {
                        Subject = subject,
                        Body = message
                    };
                    client.Send(mail);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    throw;
                }
            }
        }
    }
}