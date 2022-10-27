using EasyBL;
using Entity.Sugar;
using Microsoft.Office365.OutlookServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using WebApp.Outlook.Models;

namespace WebApp.Outlook
{
    public class ServiceHelper
    {
        public static string RedirectUri
        {
            get
            {
                return Common.GetAppSettings("ida:RedirectUri");
            }
        }

        public static string AppId
        {
            get
            {
                return Common.GetAppSettings("ida:AppId");
            }
        }

        public static string AppSecret
        {
            get
            {
                return Common.GetAppSettings("ida:AppPassword");
            }
        }

        public static string Scopes
        {
            get
            {
                return Common.GetAppSettings("ida:AppScopes");
                //return Common.GetAppSettings("ida:AppScopes").Replace(" ", ",https://outlook.office.com/").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public static string Scopes_Outlook
        {
            get
            {
                return Common.GetAppSettings("ida:AppScopes").Replace(" ", ",https://outlook.office.com/").Replace(",", " ");
            }
        }

        public static async Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, String endPoint, string accessToken, dynamic content = null)
        {
            HttpResponseMessage response = null;
            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage(method, endPoint))
                {
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    if (content != null)
                    {
                        string c;
                        c = content is string ? content : JsonConvert.SerializeObject(content);
                        request.Content = new StringContent(c, Encoding.UTF8, "application/json");
                    }

                    response = await client.SendAsync(request);
                }
            }
            return response;
        }

        /// <summary>
        /// Helper function to prepare the ResultsItem list from request response.
        /// </summary>
        /// <param name="response">Request response</param>
        /// <param name="idPropertyName">Property name of the item Id</param>
        /// <param name="displayPropertyName">Property name of the item display name</param>
        /// <param name="resourcePropId">todo: describe resourcePropId parameter on GetResultsItemAsync</param>
        /// <returns></returns>
        public static async Task<List<ResultsItem>> GetResultsItemAsync(
            HttpResponseMessage response, string idPropertyName, string displayPropertyName, string resourcePropId)
        {
            var items = new List<ResultsItem>();

            var json = JObject.Parse(await response.Content.ReadAsStringAsync());
            foreach (JProperty content in json.Children<JProperty>())
            {
                if (content.Name.Equals("value"))
                {
                    var res = content.Value.AsJEnumerable().GetEnumerator();
                    res.MoveNext();

                    while (res.Current != null)
                    {
                        var display = "";
                        var id = "";

                        foreach (JProperty prop in res.Current.Children<JProperty>())
                        {
                            if (prop.Name.Equals(idPropertyName))
                            {
                                id = prop.Value.ToString();
                            }

                            if (prop.Name.Equals(displayPropertyName))
                            {
                                display = prop.Value.ToString();
                            }
                        }

                        items.Add(new ResultsItem
                        {
                            Display = display,
                            Id = id,
                            Properties = new Dictionary<string, object>
                                            {
                                                { resourcePropId, id }
                                            }
                        });

                        res.MoveNext();
                    }
                }
            }

            return items;
        }

        public static Microsoft.Graph.Event BuildEvents(SqlSugarClient db, OTB_SYS_Calendar evnt, bool requested = false)
        {
            var saAttendee = new List<Microsoft.Graph.Attendee>();
            if (evnt.OpenMent == "G")
            {
                var saGroupMembers = evnt.GroupMembers.Split(new string[] { ",", "，" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string member in saGroupMembers)
                {
                    var oMembers = db.Queryable<OTB_SYS_Members>().Single(x => x.MemberID == member && x.OrgID == evnt.OrgID);

                    if (oMembers == null)
                    {//外部人員
                        var oOuterUsers = db.Queryable<OTB_SYS_OuterUsers>().Single(x => x.Guid == member);
                        if (oOuterUsers != null)
                        {
                            saAttendee.Add(new Microsoft.Graph.Attendee
                            {
                                EmailAddress = new Microsoft.Graph.EmailAddress
                                {
                                    Address = oOuterUsers.Email,
                                    Name = oOuterUsers.UserName
                                },
                                Type = Microsoft.Graph.AttendeeType.Resource
                            });
                        }
                    }
                    else
                    {//內部人員
                        saAttendee.Add(new Microsoft.Graph.Attendee
                        {
                            EmailAddress = new Microsoft.Graph.EmailAddress
                            {
                                Address = oMembers.OutlookAccount,
                                Name = oMembers.MemberName
                            },
                            Type = Microsoft.Graph.AttendeeType.Resource
                        });
                    }
                }
            }
            else if (evnt.OpenMent == "C")
            {
                var saMembers = db.Queryable<OTB_SYS_Members>().Where(x => x.Effective == "Y" && x.OrgID == evnt.OrgID).ToList();

                foreach (var _user in saMembers)
                {//公司所有人
                    saAttendee.Add(new Microsoft.Graph.Attendee
                    {
                        EmailAddress = new Microsoft.Graph.EmailAddress
                        {
                            Address = _user.OutlookAccount,
                            Name = _user.MemberName
                        },
                        Type = Microsoft.Graph.AttendeeType.Resource
                    });
                }
            }
            var _event = new Microsoft.Graph.Event
            {
                Subject = evnt.Title,
                Importance = evnt.Importment == "H" ? Microsoft.Graph.Importance.High : Microsoft.Graph.Importance.Normal,
                IsAllDay = evnt.AllDay,
                Body = new Microsoft.Graph.ItemBody
                {
                    ContentType = Microsoft.Graph.BodyType.Html,
                    Content = evnt.Description
                },
                BodyPreview = evnt.Description,
                Start = new Microsoft.Graph.DateTimeTimeZone
                {
                    DateTime = evnt.StartDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    TimeZone = "Asia/Shanghai"
                },
                End = new Microsoft.Graph.DateTimeTimeZone
                {
                    DateTime = evnt.EndDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    TimeZone = "Asia/Shanghai"
                },
                Attendees = saAttendee,//邀請的所有人員
                ResponseRequested = false//如果發送方希望在接受或拒絕事件時響應，則設置為true
            };
            return _event;
        }

        public static Event BuildOutlookEvents(SqlSugarClient db, OTB_SYS_Calendar evnt, bool leave_privilege = false)
        {
            var saAttendee = new List<Attendee>();
            if (evnt.OpenMent == "G")
            {
                var saGroupMembers = evnt.GroupMembers.Split(new string[] { ",", "，" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string member in saGroupMembers)
                {
                    var oMembers = db.Queryable<OTB_SYS_Members>().Single(x => x.MemberID == member && x.OrgID == evnt.OrgID);

                    if (oMembers == null)
                    {//外部人員
                        var oOuterUsers = db.Queryable<OTB_SYS_OuterUsers>().Single(x => x.Guid == member);
                        if (oOuterUsers != null)
                        {
                            saAttendee.Add(new Attendee
                            {
                                EmailAddress = new EmailAddress
                                {
                                    Address = oOuterUsers.Email,
                                    Name = oOuterUsers.UserName
                                },
                                Type = AttendeeType.Resource
                            });
                        }
                    }
                    else
                    {//內部人員
                        saAttendee.Add(new Attendee
                        {
                            EmailAddress = new EmailAddress
                            {
                                Address = oMembers.OutlookAccount,
                                Name = oMembers.MemberName
                            },
                            Type = AttendeeType.Resource
                        });
                    }
                }
            }
            else if (evnt.OpenMent == "C")
            {
                var saMembers = db.Queryable<OTB_SYS_Members>().Where(x => x.Effective == "Y" && x.OrgID == evnt.OrgID).ToList();

                if (evnt.Memo == "leave")
                {//如果是請假的話，（依據系統設定抓去能看到所有資訊的人來分類同步）
                    var oSystemSetting = db.Queryable<OTB_SYS_SystemSetting>().Single(x => x.Effective == "Y" && x.OrgID == evnt.OrgID && x.SettingItem == "Leave_Privilege");

                    if (leave_privilege)
                    {//特權
                        if (oSystemSetting != null)
                        {
                            var saMembersPrivileges = saMembers.FindAll(x => oSystemSetting.SettingValue.IndexOf(x.MemberID) > -1);
                            foreach (var privilege in saMembersPrivileges)
                            {
                                saAttendee.Add(new Attendee
                                {
                                    EmailAddress = new EmailAddress
                                    {
                                        Address = privilege.OutlookAccount,
                                        Name = privilege.MemberName
                                    },
                                    Type = AttendeeType.Resource
                                });
                            }
                        }
                    }
                    else
                    {//非特權
                        var saMembersUnPrivileges = saMembers;
                        if (oSystemSetting != null)
                        {
                            var saMembersPrivileges = saMembers.FindAll(x => oSystemSetting.SettingValue.IndexOf(x.MemberID) == -1);
                        }
                        foreach (var unprivilege in saMembersUnPrivileges)
                        {
                            saAttendee.Add(new Attendee
                            {
                                EmailAddress = new EmailAddress
                                {
                                    Address = unprivilege.OutlookAccount,
                                    Name = unprivilege.MemberName
                                },
                                Type = AttendeeType.Resource
                            });
                        }
                    }
                }
                else
                {//其他
                    foreach (var _user in saMembers)
                    {//公司所有人
                        saAttendee.Add(new Attendee
                        {
                            EmailAddress = new EmailAddress
                            {
                                Address = _user.OutlookAccount,
                                Name = _user.MemberName
                            },
                            Type = AttendeeType.Resource
                        });
                    }
                }
            }
            var _event = new Event
            {
                Subject = evnt.Title,
                Importance = evnt.Importment == "H" ? Importance.High : Importance.Normal,
                IsAllDay = evnt.AllDay,
                Body = new ItemBody
                {
                    ContentType = BodyType.HTML,
                    Content = evnt.Description
                },
                BodyPreview = evnt.Description,
                Start = new DateTimeTimeZone
                {
                    DateTime = evnt.StartDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    TimeZone = "Asia/Shanghai"
                },
                End = new DateTimeTimeZone
                {
                    DateTime = evnt.EndDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    TimeZone = "Asia/Shanghai"
                },
                Attendees = saAttendee,
                ResponseRequested = false
            };
            return _event;
        }
    }
}