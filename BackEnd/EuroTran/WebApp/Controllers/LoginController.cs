using EasyBL;
using Entity;
using Entity.Sugar;
using Microsoft.Identity.Client;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SqlSugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebApp.Outlook;
using WebApp.Outlook.AuthProvider;
using WebApp.Outlook.Models;
using WebApp.Outlook.TokenStorage;

namespace ErpApp.Controllers
{
    public class LoginController : Controller
    {
        private readonly GraphService graphService = new GraphService();

        public LoginController()
        {
        }

        public ActionResult Index(string orgid, string userid)
        {
            if (orgid != null && userid != null)
            {
                if (Request.IsAuthenticated)
                {
                    if (orgid == "" || userid == "")
                    {
                        return Redirect("~/Login/SignOut");
                    }
                    return CheckToken(orgid, userid);
                }
                return Redirect("~/Login/SignIn");
            }
            else
            {
                return Request.IsAuthenticated ? (ActionResult)CheckToken(orgid, userid) : View();
            }
        }

        /// <summary>
        /// Outlook登入
        /// </summary>
        public void SignIn()
        {
            if (!Request.IsAuthenticated)
            {
                // Signal OWIN to send an authorization request to Azure
                HttpContext.GetOwinContext().Authentication.Challenge(
                  new AuthenticationProperties { RedirectUri = "/" },
                  OpenIdConnectAuthenticationDefaults.AuthenticationType);
            }
            else
            {
                Response.Redirect("~/Page/MainPage.html");
            }
        }

        /// <summary>
        /// Outlook登出
        /// </summary>
        public void SignOut()
        {
            ToSignOut();
            Response.Redirect("/");
        }

        /// <summary>
        /// 判斷Token
        /// </summary>
        /// <param name="orgid">todo: describe orgid parameter on CheckToken</param>
        /// <param name="userid">todo: describe userid parameter on CheckToken</param>
        /// <returns></returns>
        private RedirectResult CheckToken(string orgid, string userid)
        {
            var outlook_userName = ClaimsPrincipal.Current.FindFirst("name").Value;
            var outlook_userId = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
            var outlook_eamil = ClaimsPrincipal.Current.FindFirst("preferred_username").Value;
            if (string.IsNullOrEmpty(outlook_userName) || string.IsNullOrEmpty(outlook_userId))
            {
                // Invalid principal, sign out
                return Redirect("~/Login/SignOut");
            }

            // Since we cache tokens in the session, if the server restarts but the browser still has
            // a cached cookie, we may be authenticated but not have a valid token cache. Check for
            // this and force sign out.
            var tokenCache = new SessionTokenCache(outlook_userId, HttpContext).GetMsalCacheInstance();
            if (tokenCache.HasStateChanged)
            {
                // Cache is empty, sign out
                return Redirect("~/Login/SignOut");
            }
            var db = SugarBase.GetIntance();
            if (!string.IsNullOrEmpty(orgid) && !string.IsNullOrEmpty(userid))
            {
                var oTicket = db.Queryable<OTB_SYS_TicketAuth>().Single(x => x.OrgID == orgid && x.UserID == userid);
                if (oTicket == null || oTicket.ExpireTime < DateTime.Now)
                {
                    return Redirect("~/Login/SignOut");
                }
                else
                {
                    if (oTicket.OutlookId != outlook_userId)
                    {
                        ToSignOut();
                        return Redirect("~/Login/SignIn");
                    }
                }
            }
            else
            {
                string sOrgId = string.Empty;
                string sUserId = string.Empty;

                sOrgId = (Request.Cookies["EURO_COOKIE"]?[nameof(orgid)] ?? "").ToString();
                sUserId = (Request.Cookies["EURO_COOKIE"]?[nameof(userid)] ?? "").ToString();

                LogService.mo_Log.Info($"ORGID={ sOrgId },USERID={ sUserId }");
                var oTicket = (OTB_SYS_TicketAuth)HttpRuntimeCache.Get(sOrgId + sUserId);
                if (oTicket != null)
                {
                    var oMembers = db.Queryable<OTB_SYS_Members>().Single(x => x.OrgID == sOrgId && x.MemberID == sUserId);
                    if (oMembers.OutlookAccount != outlook_eamil)
                    {//如果帳號設定中outlook帳號與當前登入的outlook帳號不一致，就強制登出
                        ToSignOut();
                        return Redirect("~/Login/SignIn");
                    }
                    oTicket.OutlookId = outlook_userId;
                    db.Updateable(oTicket).Where(x => x.OrgID == sOrgId && x.UserID == sUserId).ExecuteCommand();
                }
                else
                {
                    return Redirect("~/Login/SignOut");
                }
            }
            return Redirect("~/Page/MainPage.html");
        }

        /// <summary>
        /// Outlook登出
        /// </summary>
        private void ToSignOut()
        {
            if (Request.IsAuthenticated)
            {
                var outlook_userId = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;

                if (!string.IsNullOrEmpty(outlook_userId))
                {
                    var tokenCache = new SessionTokenCache(outlook_userId, HttpContext);
                    HttpContext.GetOwinContext().Authentication.SignOut(OpenIdConnectAuthenticationDefaults.AuthenticationType, CookieAuthenticationDefaults.AuthenticationType);
                }
            }
            // Send an OpenID Connect sign-out request.
            HttpContext.GetOwinContext().Authentication.SignOut(
              CookieAuthenticationDefaults.AuthenticationType);
        }

        public ActionResult Error(string message, string debug)
        {
            ViewBag.Message = message;
            ViewBag.Debug = debug;
            return View(nameof(Error));
        }

        /// <summary>
        /// 查詢事件
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public async Task<string> CalendarQryAsync()
        {
            var token = await AuthProvider.Instance.GetOutlookAccessTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                // If there's no token in the session, redirect to Home
                return "[]";
            }
            try
            {
                var client = await AuthProvider.Instance.GetRequestAsync();
                DateTimeOffset rStartDate = DateTime.Now.AddDays(30);
                DateTimeOffset rEndDate = DateTime.Now;
                var eventResults = await client.Me.Events.OrderByDescending(e => e.Start.DateTime)
                                    .Where(x => x.CreatedDateTime >= rStartDate && x.CreatedDateTime <= rEndDate)
                                    .ExecuteAsync();

                return ServiceBase.JsonToString(eventResults.CurrentPage);
            }
            catch (MsalException ex)
            {
                return ServiceBase.JsonToString(new { message = "ERROR retrieving messages", debug = ex.Message });
            }
        }

        /// <summary>
        /// 新增事件
        /// </summary>
        /// <param name="data">事件ID</param>
        /// <returns></returns>
        [Authorize]
        public async Task<string> CalendarAddAsync(string data)
        {
            var token = await AuthProvider.Instance.GetOutlookAccessTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                // If there's no token in the session, redirect to Home
                return "";
            }
            try
            {
                var joParam = (JObject)JsonConvert.DeserializeObject(data);
                var db = SugarBase.GetIntance();
                var iNo = int.Parse(joParam.GetValue("NO").ToString());
                var oCalendar = db.Queryable<OTB_SYS_Calendar>().Single(x => x.NO == iNo);
                var client = await AuthProvider.Instance.GetRequestAsync();
                var _event = ServiceHelper.BuildOutlookEvents(db, oCalendar, true);
                await client.Me.Events.AddEventAsync(_event);
                if (oCalendar.Memo == "leave")
                {
                    var sAskTheDummy = (joParam.GetValue("AskTheDummy") ?? "").ToString();
                    var sHolidayCategoryName = (joParam.GetValue("HolidayCategoryName") ?? "").ToString();
                    var _event_ex = ServiceHelper.BuildOutlookEvents(db, oCalendar, false);
                    _event_ex.Body.Content = oCalendar.RelationId;
                    _event_ex.Subject = sAskTheDummy + "（" + sHolidayCategoryName + "）";
                    await client.Me.Events.AddEventAsync(_event_ex);
                }
                oCalendar.OutlookId = _event.Organizer.EmailAddress.Address;
                oCalendar.OutlookEventId = _event.Id;
                oCalendar.OutlookCalUId = _event.iCalUId;
                oCalendar.OutlookInfo = ServiceBase.JsonToString(_event);
                oCalendar.OutlookRead = "N";
                var iRel = db.Updateable(oCalendar).ExecuteCommand();
                return ServiceBase.JsonToString("OK");
            }
            catch (MsalException ex)
            {
                return ServiceBase.JsonToString(new { message = "ERROR retrieving messages", debug = ex.Message });
            }
        }

        /// <summary>
        /// 修改事件
        /// </summary>
        /// <param name="data">事件ID</param>
        /// <returns></returns>
        [Authorize]
        public async Task<string> CalendarUpdAsync(string data)
        {
            var token = await AuthProvider.Instance.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                // If there's no token in the session, redirect to Home
                return "";
            }
            try
            {
                var sRel = "";
                var joParam = (JObject)JsonConvert.DeserializeObject(data);
                var db = SugarBase.GetIntance();
                var iNo = int.Parse(joParam.GetValue("NO").ToString());
                var oCalendar = db.Queryable<OTB_SYS_Calendar>().Single(x => x.NO == iNo);
                if (!string.IsNullOrEmpty(oCalendar.OutlookEventId))
                {
                    //OutlookServicesClient client = await AuthProvider.Instance.GetRequestAsync();
                    //var _event = await client.Me.Events.Where(x => x.iCalUId == oCalendar.OutlookCalUId).ExecuteSingleAsync();
                    //if (_event != null)
                    //{
                    //}
                    var event_Mew = ServiceHelper.BuildEvents(db, oCalendar);
                    var token_Outlook = await AuthProvider.Instance.GetAccessTokenAsync();
                    //  var result = graphService.UpdateMyEventsAsync(token_Outlook, oCalendar.OutlookEventId, event_Mew);
                    sRel = await graphService.UpdateMyEventsAsync(token_Outlook, oCalendar.OutlookEventId, event_Mew);
                }
                else
                {
                    var _event = ServiceHelper.BuildOutlookEvents(db, oCalendar);
                    var client = await AuthProvider.Instance.GetRequestAsync();
                    await client.Me.Events.AddEventAsync(_event);
                    oCalendar.OutlookId = _event.Organizer.EmailAddress.Address;
                    oCalendar.OutlookEventId = _event.Id;
                    oCalendar.OutlookCalUId = _event.iCalUId;
                    oCalendar.OutlookInfo = ServiceBase.JsonToString(_event);
                    oCalendar.OutlookRead = "N";
                    var iRel = db.Updateable(oCalendar).UpdateColumns(x => new { x.OutlookId, x.OutlookEventId, x.OutlookCalUId, x.OutlookInfo, x.OutlookRead }).ExecuteCommand();
                }

                return sRel;
            }
            catch (Exception ex)
            {
                return ServiceBase.JsonToString(new { message = "ERROR retrieving messages", debug = ex.Message });
            }
        }

        /// <summary>
        /// 刪除事件
        /// </summary>
        /// <param name="data">事件ID</param>
        /// <returns></returns>
        [Authorize]
        public async Task<string> CalendarDelAsync(string data)
        {
            var token = await AuthProvider.Instance.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                // If there's no token in the session, redirect to Home
                return "";
            }
            try
            {
                var joParam = (JObject)JsonConvert.DeserializeObject(data);
                var db = SugarBase.GetIntance();
                var sOutlookEventId = joParam.GetValue("OutlookEventId").ToString();
                var bRel = await graphService.DeleteMyEventsAsync(token, sOutlookEventId);
                return bRel.ToString();
            }
            catch (MsalException ex)
            {
                return ServiceBase.JsonToString(new
                {
                    message = "ERROR retrieving messages",
                    debug = ex.Message
                });
            }
        }

        /// <summary>
        /// 同步Outlook資料
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<int> SynChronousAsync(string data)
        {
            // 測試方法:
            // 起因:outlook需要SSL才能不透過程式寫死同步。故UAT要到奕達正式機測試
            // 步驟:
            //       0.先寫死sOrgId = "TE"; sUserId = "apadmin"; 因為HttpContext.Session["orgid"]與HttpContext.Session["userid"] 都是null
            //       1.本機測試方法:lunach後，輸入帳密、勾選【同步登入Outlook】，會導向outlook登入頁面，登入成功後因為沒有SSL會重新導向登入頁面/Page/Login.html
            //       2.接著直接輸入/Page/MainPage.html，直接進入主頁面。
            //       3.開啟F12，會出現Access to XMLHttpRequest at 'https://login.microsoftonline.com/com/......'，點選該網頁登入。
            //       4.點選行事曆 => 同步Outlook 即可測試 SynChronousAsync
            var token = await AuthProvider.Instance.GetOutlookAccessTokenAsync();
            HttpRuntimeCache.Set(@"outlookaccesstoken", token, 60, true);
            if (string.IsNullOrEmpty(token))
            {
                return 0;
            }

            var sOrgId = (Request.Cookies["EURO_COOKIE"]?["orgid"] ?? "").ToString();
            var sUserId = (Request.Cookies["EURO_COOKIE"]?["userid"] ?? "").ToString();

            try
            {
                //string userEmail = await AuthProvider.Instance.GetUserEmailAsync();
                var joParam = (JObject)JsonConvert.DeserializeObject(data);
                var sFlag = joParam.GetValue("flag");
                var sMemo = joParam.GetValue("memo");

                var thread = new Thread(async delegate ()
                {
                    //登陸后異步同步outlook資料（不會影響其他操作）
                    var outlook_eamil = ClaimsPrincipal.Current.FindFirst("preferred_username").Value;
                    var client = await AuthProvider.Instance.GetRequestAsync(outlook_eamil);
                    DateTimeOffset rStartDate = DateTime.Now.AddDays(-15);
                    var eventResults = await client.Me.Events.OrderByDescending(e => e.Start.DateTime)
                                        .Where(x => x.CreatedDateTime >= rStartDate).Take(5000)
                                        .ExecuteAsync();

                    var db = SugarBase.GetIntance();
                    var oMembers = db.Queryable<OTB_SYS_Members>().Single(x => x.OrgID == sOrgId && x.MemberID == sUserId);
                    var saCalendar = db.Queryable<OTB_SYS_Calendar>()
                        .Where(x => x.OrgID == sOrgId && x.StartDate.Date >= rStartDate.AddDays(-30).Date)
                        .Where(x => x.UserID == sUserId
                        || (x.OpenMent == @"G" && x.GroupMembers.Contains(sUserId))
                        || x.OpenMent == @"C").ToList();
                    var activeCalendars = saCalendar.Where(x => !x.DelStatus).ToList();
                    var delRelationIds = saCalendar.Where(x => !SqlFunc.IsNullOrEmpty(x.RelationId) && x.DelStatus)
                        .Select(x => x.RelationId).ToList();
                    var saCalendar_Add = new List<OTB_SYS_Calendar>();
                    var saCalendar_Upd = new List<OTB_SYS_Calendar>();

                    var saSubjects = db.Queryable<OTB_SYS_Arguments>()
                                       .Where(x => x.OrgID == sOrgId && x.ArgumentClassID == "LeaveType")
                                       .Select(x => "（" + x.ArgumentValue + "）")
                                       .ToList();
                    foreach (var _event in eventResults.CurrentPage)
                    {
                        var bExsit = true;
                        foreach (string str in saSubjects)
                        {
                            if (_event.Subject.Contains(str))
                            {
                                bExsit = false;
                                break;
                            }
                        }
                        if (bExsit)
                        {//解決outlook同步行事曆重複問題
                            if (activeCalendars.Any(x => x.OutlookEventId == _event.Id || x.OutlookCalUId == _event.iCalUId))
                            {
                                var oCalendar_Upd = activeCalendars.Single(x => x.OutlookEventId == _event.Id || x.OutlookCalUId == _event.iCalUId);
                                int intAddHour = 8;
                                if (oCalendar_Upd.AllDay)
                                {
                                    intAddHour = 0; //選擇全日擇無需增加8小時，會造成修改錯誤
                                }
                                oCalendar_Upd.Title = _event.Subject;
                                oCalendar_Upd.Description = _event.BodyPreview;
                                oCalendar_Upd.StartDate = Convert.ToDateTime(_event.Start.DateTime);
                                oCalendar_Upd.StartDate = oCalendar_Upd.StartDate.AddHours(intAddHour);
                                oCalendar_Upd.EndDate = Convert.ToDateTime(_event.End.DateTime);
                                oCalendar_Upd.EndDate = oCalendar_Upd.EndDate.AddHours(intAddHour);
                                oCalendar_Upd.AllDay = (bool)_event.IsAllDay;
                                oCalendar_Upd.Url = _event.WebLink;
                                oCalendar_Upd.OutlookInfo = ServiceBase.JsonToString(_event);
                                saCalendar_Upd.Add(oCalendar_Upd);
                            }
                            else
                            {
                                var oCalendar_Add = new OTB_SYS_Calendar
                                {
                                    OrgID = oMembers.OrgID,
                                    UserID = oMembers.MemberID,
                                    CalType = "03",
                                    Title = _event.Subject,
                                    Description = _event.BodyPreview,
                                    StartDate = Convert.ToDateTime(_event.Start.DateTime).AddHours(8),
                                    EndDate = Convert.ToDateTime(_event.End.DateTime).AddHours(8),
                                    Importment = _event.Importance == Microsoft.Office365.OutlookServices.Importance.High ? "H" : "M",
                                    Color = oMembers.CalColor,
                                    AllDay = (bool)_event.IsAllDay,
                                    OpenMent = "P",
                                    GroupMembers = "",
                                    Url = _event.WebLink,
                                    Editable = false,
                                    ClassName = "",
                                    Memo = "outlook",
                                    CreateUser = "System",
                                    CreateDate = DateTime.Now,
                                    ModifyUser = "System",
                                    ModifyDate = DateTime.Now,
                                    OutlookId = _event.Organizer.EmailAddress.Address,
                                    OutlookEventId = _event.Id,
                                    OutlookCalUId = _event.iCalUId,
                                    OutlookRead = "N",
                                    OutlookInfo = ServiceBase.JsonToString(_event),
                                    DelStatus = false,
                                };
                                saCalendar_Add.Add(oCalendar_Add);
                            }
                        }

                        //刪除已經抽單的行事曆
                        if (delRelationIds.Any(x => _event.Body.Content.Contains(x)))
                        {
                            // https://docs.microsoft.com/en-us/graph/api/group-delete-event?view=graph-rest-beta&tabs=csharp
                            await _event.DeleteAsync();
                        }

                    }
                    if (saCalendar_Add.Count > 0)
                    {
                        db.Insertable(saCalendar_Add).ExecuteCommand();
                    }
                    if (saCalendar_Upd.Count > 0)
                    {
                        db.Updateable(saCalendar_Upd).ExecuteCommand();
                    }
                    Thread.Sleep(2000); //延时两秒
                    var context = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<WebApp.Hubs.MsgHub>();
                    await context.Clients.All.message(new Map {
                        { "Type", "OutlookSynChronous" },
                        { "Flag", sFlag },
                        { "Message", outlook_eamil },
                        { "Memo", sMemo }
                    }); //通知前台已同步完成
                });

                thread.Start();

                return 1;
            }
            catch (Exception ex)
            {
                LogService.MailSend(ex.Message + @"Param：" + ServiceBase.JsonToString(ex), ex, sOrgId, sUserId, nameof(LoginController), @"同步行事曆", @"SynChronousAsync（同步Outlook資料）", @"", @"", @"");
                return -1;
            }
        }

        /// <summary>
        /// 新增郵件
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public async Task<string> InboxAddAsync()
        {
            var token = await AuthProvider.Instance.GetOutlookAccessTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                // If there's no token in the session, redirect to Home
                return "[]";
            }
            var client = await AuthProvider.Instance.GetRequestAsync();
            try
            {
                return ServiceBase.JsonToString("");
            }
            catch (MsalException ex)
            {
                return ServiceBase.JsonToString(new { message = "ERROR retrieving messages", debug = ex.Message });
            }
        }

        /// <summary>
        /// 刪除郵件
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public async Task<string> InboxDelAsync()
        {
            var token = await AuthProvider.Instance.GetOutlookAccessTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                // If there's no token in the session, redirect to Home
                return "[]";
            }
            var client = await AuthProvider.Instance.GetRequestAsync();
            try
            {
                return ServiceBase.JsonToString("");
            }
            catch (MsalException ex)
            {
                return ServiceBase.JsonToString(new { message = "ERROR retrieving messages", debug = ex.Message });
            }
        }

        /// <summary>
        /// 查詢郵件
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public async Task<string> InboxQryAsync()
        {
            var token = await AuthProvider.Instance.GetOutlookAccessTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                // If there's no token in the session, redirect to Home
                return "[]";
            }
            var client = await AuthProvider.Instance.GetRequestAsync();
            try
            {
                var mailResults = await client.Me.Messages
                                    .OrderByDescending(m => m.ReceivedDateTime)
                                    .Take(10)
                                    .ExecuteAsync();

                var displayResults = mailResults.CurrentPage.Select(m => new Messages(m.Subject, m.ReceivedDateTime, m.From));

                return ServiceBase.JsonToString(displayResults);
            }
            catch (MsalException ex)
            {
                return ServiceBase.JsonToString(new { message = "ERROR retrieving messages", debug = ex.Message });
            }
        }

        /// <summary>
        /// 修改郵件
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public async Task<string> InboxUpdAsync()
        {
            var token = await AuthProvider.Instance.GetOutlookAccessTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                // If there's no token in the session, redirect to Home
                return "[]";
            }
            var client = await AuthProvider.Instance.GetRequestAsync();
            try
            {
                return ServiceBase.JsonToString("");
            }
            catch (MsalException ex)
            {
                return ServiceBase.JsonToString(new { message = "ERROR retrieving messages", debug = ex.Message });
            }
        }

        /// <summary>
        /// 測試
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public async Task<string> TestAsync()
        {
            var token = await AuthProvider.Instance.GetOutlookAccessTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                // If there's no token in the session, redirect to Home
                return "";
            }
            try
            {
                var client = await AuthProvider.Instance.GetRequestAsync();

                var contactResults2 = await client.Me.Messages.ExecuteAsync();

                var contactResults4 = await client.Me.Events.ExecuteAsync();

                var contactResults5 = await client.Me.Calendar.ExecuteAsync();

                var contactResults6 = await client.Me.CalendarGroups.ExecuteAsync();

                var contactResults7 = await client.Me.Calendars.ExecuteAsync();

                var contactResults9 = await client.Me.ContactFolders.ExecuteAsync();
                foreach (var folder in contactResults9.CurrentPage)
                {
                    var contact_c = await client.Me.ContactFolders.GetById(folder.Id).Contacts.ExecuteAsync();
                }

                var contactResults11 = await client.Me.ExecuteAsync();

                var contactResults = await client.Me.Contacts.ExecuteAsync();

                return ServiceBase.JsonToString(contactResults.CurrentPage);
            }
            catch (MsalException ex)
            {
                return ServiceBase.JsonToString(new { message = "ERROR retrieving messages", debug = ex.Message });
            }
        }

        private static bool CheckSubject(SqlSugarClient db, string subject)
        {
            var bExsit = true;
            return bExsit;
        }
    }
}