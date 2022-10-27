using EasyBL.WebApi.Common;
using EasyBL.WebApi.Message;
using EasyBL.WebApi.Models;
using EasyNet;
using Entity;
using Entity.Sugar;
using Newtonsoft.Json;
using SqlSugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Net;
using System.Web.Http;

namespace EasyBL.WEBAPP
{
    public class AuthorizeService : ServiceBase
    {
        public HttpResponseMessage GetLogin([FromBody]dynamic i_value, HttpRequestMessage i_rRequest)
        {
            string sRes = null;

            try
            {
                string pm = CmdService.DecodeParm(i_value);
                var crm = JsonConvert.DeserializeObject<RequestMessage>(pm);
                crm.ClientIP = GetClientIp(i_rRequest);
                var auth = new AuthorizeService();
                sRes = JsonConvert.SerializeObject(auth.Entry(crm));
            }
            catch (Exception ex)
            {
                var exCur = ex;
                while (null != exCur.InnerException)
                {
                    exCur = exCur.InnerException;
                }
                sRes = JsonConvert.SerializeObject(new ErrorResponseMessage(exCur.Message));
            }

            return new HttpResponseMessage
            {
                Content = new StringContent(sRes, System.Text.Encoding.UTF8, @"application/json")
            };
        }

        //public static string MyToString<T>(T str)
        //{
        //    throw new NotSupportedException("Can only be used in expressions");
        //}

        #region 系統登入

        /// <summary>
        /// 函式名稱:Login
        /// 函式說明:系統登入
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on Login</param>
        /// <returns>
        /// 回傳 rm(Object)
        ///</returns>
        public ResponseMessage Login(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sError = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sOrgID = _fetchString(i_crm, @"OrgID");
                    var sUserID = _fetchString(i_crm, @"UserID");
                    var sPassword = _fetchString(i_crm, @"Pwd");
                    var bOutklook = _fetchBool(i_crm, @"Outklook");
                    var bRelogin = _fetchBool(i_crm, @"Relogin");
                    var sIP = i_crm.ClientIP;

                    if (string.IsNullOrWhiteSpace(sOrgID))
                    {
                        sError = @"組織代號不能為空";   //組織代號不能為空
                        break;
                    }
                    if (string.IsNullOrWhiteSpace(sUserID))
                    {
                        sError = @"帳號不能為空";   //帳號不能為空
                        break;
                    }
                    if (string.IsNullOrWhiteSpace(sPassword))
                    {
                        sError = @"密碼不能為空";   //密碼不能為空
                        break;
                    }

                    var oOrg = db.Queryable<OTB_SYS_Organization>().Single(x => x.OrgID == sOrgID);

                    if (oOrg == null)
                    {
                        sError = @"組織代號不正確";  //組織代號不正確
                        break;
                    }

                    if (oOrg.Effective != @"Y")
                    {
                        sError = @"該組織無效";   //該組織無效
                        break;
                    }

                    var sEncryptPwd = SecurityUtil.Encrypt(sPassword);//將輸入之密碼轉換驗證格式
                                                                      //string sPwd1 = SecurityUtil.Decrypt("wTBo6uXVBlVH8Ms76xiE4w==");
                                                                      //string sPwd1 = SecurityUtil.Decrypt("3EOyqH52VBUg3pj5Wy0rwQ==");
                    var oUser = db.Queryable<OTB_SYS_Members>().Single(x => x.OrgID == sOrgID && (x.MemberID == sUserID || x.Email == sUserID) && x.Password == sEncryptPwd);

                    if (oUser == null)
                    {
                        sError = @"帳號或密碼不正確";   //帳號或密碼不正確
                        break;
                    }

                    if (oUser.Effective != @"Y")
                    {
                        sError = @"該帳號無效";   //該帳號無效
                        break;
                    }

                    if (bOutklook && string.IsNullOrWhiteSpace(oUser.OutlookAccount))
                    {
                        sError = @"Outlook帳號未設定，請管理員幫您設定Outlook帳號";
                        break;
                    }


                    var sIsCheckNet = Common.GetAppSettings(@"IsCheckNet");
                    if (sIsCheckNet == @"true")
                    {
                        var sUrl = HttpContext.Current.Request.Url.ToString();

                        if (sUrl.IndexOf(@"localhost") == -1)
                        {
                            var sClientIP = sIP;
                            if (!oUser.NetworkLogin)
                            {
                                var CheckList = new List<bool>();
                                var LocalList = new List<IPAddressRange>()
                                {
                                    new IPAddressRange(IPAddress.Parse("10.0.0.0"), IPAddress.Parse("10.255.255.255")),
                                    new IPAddressRange(IPAddress.Parse("172.16.0.0"), IPAddress.Parse("172.31.255.255")),
                                    new IPAddressRange(IPAddress.Parse("192.168.0.0"), IPAddress.Parse("192.168.255.255")),
                                };
                                foreach (var AddressRange in LocalList)
                                {
                                    var PassThisRange = AddressRange.IsInRange(IPAddress.Parse(sClientIP));
                                    CheckList.Add(PassThisRange);
                                }

                                var Pass = CheckList.Any(c => c);
                                if (!Pass)
                                {
                                    sError = @"您的帳號不允許外網登錄";   //您的帳號不允許外網登錄
                                    break;
                                }

                            }
                        }
                    }
                    var saOnlineUsers = db.Queryable<OTB_SYS_OnlineUsers>().Where(x => x.OrgID != sOrgID && x.UserID == oUser.MemberID).ToList();
                    if (saOnlineUsers.Count > 0)
                    {
                        if (!bRelogin)
                        {
                            string sLocation = string.Empty;
                            sError = @"Tips：此帳號已於";

                            switch (saOnlineUsers.First().OrgID)
                            {
                                case "TE":
                                    sLocation = "台北奕達";
                                    break;
                                case "TG":
                                    sLocation = "台北駒驛";
                                    break;
                                case "SG":
                                    sLocation = "上海駒驛";
                                    break;
                                case "SE":
                                    sLocation = "簡單平台";
                                    break;
                                case "HY":
                                    sLocation = "好有創意";
                                    break;

                            };

                            //此帳號已於[上海駒驛]登入，請先登出該系統後再重新登入
                            sError = $"{sError}{sLocation}登入，是否繼續登入當前賬號？";

                            break;
                        }
                        else
                        {
                            db.Deleteable<OTB_SYS_OnlineUsers>(saOnlineUsers).ExecuteCommand();
                        }
                    }

                    var ticket = new OTB_SYS_TicketAuth
                    {
                        OrgID = oOrg.OrgID,
                        UserID = oUser.MemberID,
                        UserName = oUser.MemberName,
                        Token = SignExtension.CreateToken(),
                        LoginIp = i_crm.ClientIP,
                        LoginTime = DateTime.Now
                    };
                    var iExpireTime = 240;
                    var sExpireTime = Common.GetSystemSetting(db, oOrg.OrgID, @"ExpireTime");
                    if (!string.IsNullOrEmpty(sExpireTime))
                    {
                        iExpireTime = int.Parse(sExpireTime);
                    }
                    else
                    {
                        iExpireTime = int.Parse(Common.GetAppSettings(@"ExpireTime"));
                    }
                    ticket.ExpireTime = DateTime.Now.AddMinutes(iExpireTime); //30分钟过期
                    ticket.IsVerify = @"Y";
                    var oTicket = db.Queryable<OTB_SYS_TicketAuth>().Single(x => x.OrgID == sOrgID && x.UserID == oUser.MemberID);
                    if (oTicket != null)
                    {
                        db.Updateable(ticket).IgnoreColumns(x => x.OutlookId).Where(x => x.NO == oTicket.NO).ExecuteCommand();
                    }
                    else
                    {
                        ticket.CreateTime = DateTime.Now;
                        ticket = db.Insertable(ticket).ExecuteReturnEntity();
                    }
                    //記錄log日誌
                    db.Insertable(new OTB_SYS_LoginLog
                    {
                        OrgId = ticket.OrgID,
                        UserId = ticket.UserID,
                        UserName = ticket.UserName,
                        LoginIp = ticket.LoginIp,
                        LoginTime = ticket.LoginTime
                    }).ExecuteCommand();
                    HttpRuntimeCache.Set(ticket.OrgID + ticket.UserID, ticket, iExpireTime * 60, true);
                    HttpContext.Current.Session.Add(@"orgid", ticket.OrgID);
                    HttpContext.Current.Session.Add(@"userid", ticket.UserID);
                    HttpCookie cookie = new HttpCookie("EURO_COOKIE");//初始化並設置Cookie的名稱
                    DateTime dt = DateTime.Now;
                    TimeSpan ts = new TimeSpan(0, 0, 1, 0, 0);//過期時間為1分鐘
                    cookie.Expires = dt.Add(ts);//設置過期時間
                    cookie.Values.Add("orgid", ticket.OrgID);
                    cookie.Values.Add("userid", ticket.UserID);
                    HttpContext.Current.Response.AppendCookie(cookie);
                    var jo = new SetMap
                    {
                        { @"orgid", ticket.OrgID },
                        { @"userid", ticket.UserID },
                        { @"loginname", ticket.UserName },
                        { @"usertype", @"inner" },
                        { @"mode", oUser.SysShowMode },
                        { @"token", ticket.Token },
                        { @"outklook", bOutklook }
                    };
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, jo);
                } while (false);
            }
            catch (Exception ex)
            {
                sError = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sError + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(AuthorizeService), nameof(Login), @"Login（系統登入）", @"", @"", @"");
            }
            finally
            {
                if (null != sError)
                {
                    rm = new ErrorResponseMessage(sError, i_crm);
                }
            }
            return rm;
        }

        #endregion 系統登入

        #region 獲取個人信息

        /// <summary>
        /// 函式名稱:Login
        /// 函式說明:系統登入
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on GetUserInfo</param>
        /// <returns>
        /// 回傳 rm(Object)
        ///</returns>
        public ResponseMessage GetUserInfo(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sError = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var userInfo = db.Queryable<OTB_SYS_Members, OTB_SYS_Departments, OTB_SYS_Jobtitle>((a, b, c) => new object[] {
                         JoinType.Inner, a.OrgID==b.OrgID && a.DepartmentID==b.DepartmentID,
                         JoinType.Inner, a.OrgID==c.OrgID && a.JobTitle==c.JobtitleID
                    })
                           .Where((a) => a.OrgID == i_crm.ORIGID && a.MemberID == i_crm.USERID)
                           .Select((a, b, c) =>
                           new UserInfo
                           {
                               MemberID = a.MemberID,
                               MemberName = a.MemberName,
                               Email = a.Email,
                               OutlookAccount = a.OutlookAccount,
                               DepartmentID = a.DepartmentID,
                               Effective = a.Effective,
                               CalColor = a.CalColor,
                               MemberPic = a.MemberPic,
                               SysShowMode = a.SysShowMode,
                               Country = a.Country,
                               ServiceCode = a.ServiceCode,
                               Address = a.Address,
                               DepartmentName = b.DepartmentName,
                               JobtitleName = c.JobtitleName,
                               Supervisors = a.ImmediateSupervisor + @","
                           }).Single();

                    if (userInfo != null)
                    {
                        var saRoles = db.Queryable<OTB_SYS_MembersToRule>()
                            .Where(x => x.OrgID == i_crm.ORIGID && x.MemberID == i_crm.USERID)
                            .Select(x => x.RuleID)
                            .ToList();
                        userInfo.roles = string.Join(@",", saRoles);

                        var saDepartments = db.Queryable<OTB_SYS_Departments>()
                            .Where(x => x.OrgID == i_crm.ORIGID && x.ChiefOfDepartmentID == i_crm.USERID)
                            .Select(x => x.DepartmentID)
                            .ToList();
                        var saUsersDown = db.Queryable<OTB_SYS_Members>()
                            .Where(x => x.OrgID == i_crm.ORIGID && saDepartments.Contains(x.DepartmentID))
                            .Select(x => SqlFunc.IsNull(x.MemberID, @""))
                            .ToList();
                        userInfo.UsersDown = string.Join(@",", saUsersDown);
                        var saUsersBranch = db.Queryable<OTB_SYS_Members>()
                            .Where(x => x.OrgID == i_crm.ORIGID && x.ImmediateSupervisor == i_crm.USERID)
                            .Select(x => SqlFunc.IsNull(x.MemberID, @""))
                            .ToList();
                        userInfo.UsersBranch = string.Join(@",", saUsersBranch);

                        var oDepartments = db.Queryable<OTB_SYS_Departments>().Single(x => x.OrgID == i_crm.ORIGID && x.DepartmentID == userInfo.DepartmentID);
                        userInfo.Supervisors += oDepartments.ChiefOfDepartmentID ?? @"";
                    }

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, userInfo);
                } while (false);
            }
            catch (Exception ex)
            {
                sError = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sError + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(AuthorizeService), @"", @"GetUserInfo（獲取個人信息）", @"", @"", @"");
            }
            finally
            {
                if (null != sError)
                {
                    rm = new ErrorResponseMessage(sError, i_crm);
                }
            }
            return rm;
        }

        #endregion 獲取個人信息

        #region 獲取程式權限

        /// <summary>
        /// 函式名稱:UpdataPsw
        /// 函式說明:獲取程式權限
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on GetAuthorize</param>
        /// <returns>
        /// 回傳 rm(Object)
        ///</returns>
        public ResponseMessage GetAuthorize(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sProgramID = _fetchString(i_crm, @"ProgramID");
                    var sTopModuleID = _fetchString(i_crm, @"TopModuleID");
                    var saRoles = db.Queryable<OTB_SYS_MembersToRule>()
                        .Where(x => x.OrgID == i_crm.ORIGID && x.MemberID == i_crm.USERID)
                        .Select(x => x.RuleID)
                        .ToList();
                    var oDepartments = db.Queryable<OTB_SYS_Members>().Single(x => x.OrgID == i_crm.ORIGID && x.MemberID == i_crm.USERID);

                    var saAuthorize = db.UnionAll(
                        db.Queryable<OTB_SYS_Authorize>()
                        .Where(x => x.OrgID == i_crm.ORIGID && x.ProgramID == sProgramID && x.TopModuleID == sTopModuleID && x.AllowRight != @"")
                        .Where(x => saRoles.Contains(x.RuleID))
                        .Select(x => new AuthorizeInfo
                        {
                            RuleID = x.RuleID,
                            ProgramID = x.ProgramID,
                            AllowRight = x.AllowRight,
                            TopModuleID = x.TopModuleID
                        }),
                        db.Queryable<OTB_SYS_AuthorizeForDept>()
                        .Where(x => x.OrgID == i_crm.ORIGID && x.ProgramID == sProgramID && x.TopModuleID == sTopModuleID && x.AllowRight != @"" && x.DepartmentID == oDepartments.DepartmentID)
                        .Select(x => new AuthorizeInfo
                        {
                            RuleID = x.DepartmentID,
                            ProgramID = x.ProgramID,
                            AllowRight = x.AllowRight,
                            TopModuleID = x.TopModuleID
                        }),
                        db.Queryable<OTB_SYS_AuthorizeForMember>()
                        .Where(x => x.OrgID == i_crm.ORIGID && x.ProgramID == sProgramID && x.TopModuleID == sTopModuleID && x.AllowRight != @"" && x.MemberID == i_crm.USERID)
                        .Select(x => new AuthorizeInfo
                        {
                            RuleID = x.MemberID,
                            ProgramID = x.ProgramID,
                            AllowRight = x.AllowRight,
                            TopModuleID = x.TopModuleID
                        })
                        ).ToList();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, saAuthorize);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(AuthorizeService), @"", @"GetAuthorize（獲取程式權限）", @"", @"", @"");
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

        #endregion 獲取程式權限

        #region 修改個人密碼

        /// <summary>
        /// 函式名稱:UpdataPsw
        /// 函式說明:修改個人密碼
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on UpdataPsw</param>
        /// <returns>
        /// 回傳 rm(Object)
        ///</returns>
        public ResponseMessage UpdataPsw(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sUserName = _fetchString(i_crm, @"UserName");
                    var sOldPsw = _fetchString(i_crm, @"OldPsw");
                    var sNewPsw = _fetchString(i_crm, @"NewPsw");
                    var sCalColor = _fetchString(i_crm, @"CalColor");
                    var sMemberPic = _fetchString(i_crm, @"MemberPic");
                    var oUser = db.Queryable<OTB_SYS_Members>().Single(it => it.OrgID == i_crm.ORIGID && it.MemberID == i_crm.USERID);

                    var dicUpdcols = new Dictionary<string, object>();
                    var sNewPwd_Encrypt = @"";
                    if (sOldPsw != @"" && sNewPsw != @"")
                    {
                        var sOldPwd = SecurityUtil.Encrypt(sOldPsw);//將輸入之密碼轉換驗證格式

                        if (oUser.Password != sOldPwd)   //舊密碼驗證失敗
                        {
                            sMsg = @"1";
                            break;
                        }
                        sNewPwd_Encrypt = SecurityUtil.Encrypt(sNewPsw);//將輸入之密碼轉換驗證格式
                        dicUpdcols.Add(OTB_SYS_Members.CN_PASSWORD, sNewPwd_Encrypt);
                    }

                    dicUpdcols.Add(OTB_SYS_Members.CN_MEMBERNAME, sUserName);
                    dicUpdcols.Add(OTB_SYS_Members.CN_CALCOLOR, sCalColor);
                    dicUpdcols.Add(OTB_SYS_Members.CN_MEMBERPIC, sMemberPic);

                    var iRel = db.Updateable<OTB_SYS_Members>(dicUpdcols)
                        .Where(x => x.OrgID == i_crm.ORIGID && x.MemberID == i_crm.USERID).ExecuteCommand();
                    if (iRel <= 0)
                    {
                        sMsg = @"2";
                        break;
                    }
                    rm = new SuccessResponseMessage(null, i_crm);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(AuthorizeService), nameof(Login), @"UpdataPsw（修改個人資料）", @"", @"", @"");
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

        #endregion 修改個人密碼

        #region 驗證會員帳號

        /// <summary>
        /// 函式名稱:CheckMember
        /// 函式說明:驗證會員帳號
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on CheckMember</param>
        /// <returns>
        /// 回傳 rm(Object)
        ///</returns>
        public ResponseMessage CheckMember(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var sOrgID = _fetchString(i_crm, @"OrgID");
                        var sUserID = _fetchString(i_crm, @"UserID");
                        //載入資訊
                        var oUser = db.Queryable<OTB_SYS_Members>().Single(it => it.OrgID == sOrgID && it.MemberID == sUserID);

                        if (oUser == null)   //驗證帳號或密碼是否正確
                        {
                            sMsg = @"1";
                            break;
                        }
                        db.Deleteable<OTB_SYS_ForgetPassword>().Where(x => x.OrgID == sOrgID && x.MemberID == sUserID).ExecuteCommand();

                        var sRomd = SecurityUtil.GetRandomString(6);
                        var oForgetPasswordAdd = new OTB_SYS_ForgetPassword
                        {
                            OrgID = sOrgID,
                            MemberID = sUserID,
                            VerificationCode = SecurityUtil.Encrypt(sRomd),//將輸入之密碼轉換驗證格式
                            ModifyDate = DateTime.Now
                        };
                        var oForgetPassword = db.Insertable(oForgetPasswordAdd).ExecuteReturnEntity();

                        if (oForgetPassword == null)   // 刪除或新增驗證碼成功與否
                        {
                            sMsg = @"2";
                            break;//儲存失敗
                        }

                        //儲存驗證碼成功準備寄信
                        var oEmailInfo = db.Queryable<OTB_SYS_Email>().Single(x => x.OrgID == sOrgID && x.EmailID == @"getNewPsw");

                        if (oEmailInfo == null)
                        {
                            sMsg = @"系統找不到對應的郵件模版";
                            break;//儲存失敗
                        }

                        var sEmailBody = @"";
                        sEmailBody = oEmailInfo.BodyHtml.Replace(@"{{:UserName}}", oUser.MemberName).Replace(@"{{:MemberPwd}}", sRomd);

                        var oEmail = new Emails();
                        var saEmailTo = new List<EmailTo>();
                        //收件人
                        var oEmailTo = new EmailTo
                        {
                            ToUserID = oUser.MemberID,
                            ToUserName = oUser.MemberName,
                            ToEmail = oUser.Email,
                            Type = @"to"
                        };
                        saEmailTo.Add(oEmailTo);

                        oEmail.FromUserName = @"系統自動發送";//取fonfig
                        oEmail.Title = @"驗證碼";//取fonfig
                        oEmail.EmailBody = sEmailBody;
                        oEmail.IsCCSelf = false;
                        oEmail.Attachments = null;
                        oEmail.EmailTo = saEmailTo;

                        var bSend = new MailService(sOrgID).MailFactory(oEmail, out sMsg);
                        if (sMsg != null)
                        {
                            break;
                        }
                        oForgetPassword.ModifyDate = DateTime.Now;
                        db.Updateable(oForgetPassword).UpdateColumns(x => x.ModifyDate).ExecuteCommand();
                        rm = new SuccessResponseMessage(null, i_crm);
                    } while (false);
                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(AuthorizeService), nameof(Login), @"CheckMember（驗證會員帳號）", @"", @"", @"");
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

        #endregion 驗證會員帳號

        #region 重設密碼

        /// <summary>
        /// 函式名稱:Check
        /// 函式說明:重設密碼
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on ReSetPassword</param>
        /// <returns>
        /// 回傳 rm(Object)
        ///</returns>
        public ResponseMessage ReSetPassword(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var sOrgID = _fetchString(i_crm, @"OrgID");
                        var sUserID = _fetchString(i_crm, @"UserID");
                        var sVerificationCode = _fetchString(i_crm, @"VerificationCode");
                        var sNewPsw = _fetchString(i_crm, @"NewPsw");

                        var oUser = db.Queryable<OTB_SYS_Members>().Single(it => it.OrgID == sOrgID && it.MemberID == sUserID);

                        if (oUser.MemberID == null)  //查無此會員資料或MemberID為空
                        {
                            sMsg = @"1";
                            break;
                        }

                        var oForgetPassword = db.Queryable<OTB_SYS_ForgetPassword>().Single(x => x.OrgID == sOrgID && x.MemberID == oUser.MemberID);

                        if (oForgetPassword == null)
                        {
                            sMsg = @"4";
                            break;
                        }
                        var sEncryptVerificationCode = SecurityUtil.Encrypt(sVerificationCode);//將輸入之密碼轉換驗證格式
                        if (oForgetPassword.MemberID != sUserID || oForgetPassword.VerificationCode != sEncryptVerificationCode)
                        {
                            sMsg = @"0";
                            break;
                        }

                        var Time = DateTime.Now;
                        var DataNow = new TimeSpan(Time.Ticks);
                        var CeateDate = new TimeSpan(oForgetPassword.ModifyDate.Value.Ticks);
                        var ts = DataNow - CeateDate;

                        var ts5 = DataNow.Subtract(CeateDate);

                        var RunTime = int.Parse(ts5.TotalSeconds.ToString().Split('.')[0].ToString());

                        if (RunTime > 60)   //驗證碼超出限制時間刪除該筆資料
                        {
                            var iDel = db.Deleteable<OTB_SYS_ForgetPassword>().Where(x => x.OrgID == sOrgID && x.MemberID == sUserID).ExecuteCommand();
                            if (iDel > 0)
                            {
                                sMsg = @"2";
                                break;
                            }
                        }
                        var sNewPwd = SecurityUtil.Encrypt(sNewPsw);//將輸入之密碼轉換驗證格式
                        var oMembers = new OTB_SYS_Members
                        {
                            Password = sNewPwd
                        };

                        var iRel = db.Updateable(oMembers).UpdateColumns(x => new { x.Password }).Where(x => x.OrgID == sOrgID && x.MemberID == sUserID).ExecuteCommand();

                        if (iRel == 0)   //更新資料失敗
                        {
                            sMsg = @"3";
                            break;
                        }

                        var iDel2 = db.Deleteable<OTB_SYS_ForgetPassword>().Where(x => x.OrgID == sOrgID && x.MemberID == sUserID).ExecuteCommand();

                        rm = new SuccessResponseMessage(null, i_crm);
                    } while (false);
                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(AuthorizeService), nameof(Login), @"Check（驗證碼時間檢驗）", @"", @"", @"");
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

        #endregion 重設密碼

        #region 新增帳號寄送初始密碼給新帳號人員

        /// <summary>
        /// 函式名稱:SendPswToNewMember
        /// 函式說明:新增帳號寄送初始密碼給新帳號人員
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on SendPswToNewMember</param>
        /// <returns>
        /// 回傳 rm(Object)
        ///</returns>
        public ResponseMessage SendPswToNewMember(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sOrgID = _fetchString(i_crm, @"OrgID");
                    var sUserID = _fetchString(i_crm, @"UserID");
                    var sEmailBody = @"";

                    var oUser = db.Queryable<OTB_SYS_Members>().Single(it => it.OrgID == i_crm.ORIGID && it.MemberID == i_crm.USERID);
                    var oEmail_O = db.Queryable<OTB_SYS_Email>().Single(x => x.OrgID == sOrgID && x.EmailID == @"Member");

                    if (oEmail_O == null)
                    {
                        sMsg = @"系統找不到對應的郵件模版";
                        break;
                    }

                    sEmailBody = oEmail_O.BodyHtml
                        .Replace(@"{{:UserName}}", oUser.MemberName)
                        .Replace(@"{{:MemberPwd}}", SecurityUtil.Decrypt(oUser.Password));

                    var oEmail = new Emails();
                    var saEmailTo = new List<EmailTo>();
                    //收件人
                    var oEmailTo = new EmailTo
                    {
                        ToUserID = oUser.MemberID,
                        ToUserName = oUser.MemberName,
                        ToEmail = oUser.Email,
                        Type = @"to"
                    };
                    saEmailTo.Add(oEmailTo);

                    oEmail.FromUserName = @"系統自動發送";//取fonfig
                    oEmail.Title = @"初始密碼";//取fonfig
                    oEmail.EmailBody = sEmailBody;
                    oEmail.IsCCSelf = false;
                    oEmail.Attachments = null;
                    oEmail.EmailTo = saEmailTo;

                    var bSend = new MailService(i_crm.ORIGID).MailFactory(oEmail, out sMsg);
                    if (sMsg != null)
                    {
                        break;
                    }

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, bSend);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(AuthorizeService), nameof(Login), @"SendPswToNewMember（新增帳號寄送初始密碼給新帳號人員）", @"", @"", @"");
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

        #endregion 新增帳號寄送初始密碼給新帳號人員

        /// <summary>
        /// </summary>
        /// <param name="i_rRequest"></param>
        /// <returns></returns>
        public static string GetClientIp(HttpRequestMessage i_rRequest) => i_rRequest.Properties.ContainsKey(@"MS_HttpContext") ? ((HttpContextWrapper)i_rRequest.Properties[@"MS_HttpContext"]).Request.UserHostAddress : HttpContext.Current?.Request.UserHostAddress;

        private class UserInfo
        {
            public UserInfo()
            {
                roles = @"";
                Supervisors = @"";
                UsersDown = @"";
                UsersBranch = @"";
            }

            public string MemberID { get; set; }
            public string MemberName { get; set; }
            public string Email { get; set; }
            public string DepartmentID { get; set; }
            public string Effective { get; set; }
            public string CalColor { get; set; }
            public string MemberPic { get; set; }
            public string SysShowMode { get; set; }
            public string Country { get; set; }
            public string ServiceCode { get; set; }
            public string Address { get; set; }
            public string DepartmentName { get; set; }
            public string JobtitleName { get; set; }
            public string roles { get; set; }
            public string Supervisors { get; set; }
            public string UsersDown { get; set; }
            public string UsersBranch { get; set; }
            public string OutlookAccount { get; set; }
        }

        private class AuthorizeInfo
        {
            public string RuleID { get; set; }
            public string ProgramID { get; set; }
            public string AllowRight { get; set; }
            public string TopModuleID { get; set; }
        }
    }
}