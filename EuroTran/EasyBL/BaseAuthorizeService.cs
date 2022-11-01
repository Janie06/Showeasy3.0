using EasyBL.WebApi;
using EasyBL.WebApi.Common;
using EasyBL.WebApi.Message;
using EasyNet;
using Entity.Sugar;
using Newtonsoft.Json;
using SqlSugar.Base;
using System;
using System.Net.Http;

namespace EasyBL
{
    public class BaseAuthorizeService : ServiceBase
    {
        #region 根据用户名获取token

        /// <summary>
        /// 函式名稱:GetToken
        /// 函式說明:获取token
        /// </summary>
        /// <param name="orgId">todo: describe orgId parameter on GetToken</param>
        /// <param name="userId">todo: describe userId parameter on GetToken</param>
        /// <param name="pasWd">todo: describe pasWd parameter on GetToken</param>
        /// <returns>
        /// 回傳 rm(Object)
        ///</returns>
        public HttpResponseMessage GetToken(string orgId, string userId, string pasWd)
        {
            SuccessResponseMessage srm = null;
            string sError = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    //判断参数是否合法
                    if (string.IsNullOrEmpty(orgId) || string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(pasWd))
                    {
                        srm = new SuccessResponseMessage(null, null)
                        {
                            STATUSCODE = (int)StatusCodeEnum.ParameterError,
                            MSG = StatusCodeEnum.ParameterError.GetEnumText()
                        };
                        srm.DATA.Add("token", "");
                        return HttpResponseExtension.ToJson(JsonConvert.SerializeObject(srm));
                    }

                    var oTicket = (OTB_SYS_TicketAuth)HttpRuntimeCache.Get(orgId + userId);
                    if (oTicket == null || oTicket.ExpireTime < DateTime.Now)
                    {
                        var sEncryptPwd = SecurityUtil.Encrypt(pasWd);//將輸入之密碼轉換驗證格式
                        var oUser = db.Queryable<OTB_SYS_Members>().Single(it => it.OrgID == orgId && it.MemberID == userId && it.Password == sEncryptPwd);

                        if (oUser != null)
                        {
                            oTicket = db.Queryable<OTB_SYS_TicketAuth>().Single(it => it.OrgID == orgId && it.UserID == userId);

                            if (oTicket == null)
                            {
                                oTicket = new OTB_SYS_TicketAuth();
                            }
                            oTicket.Token = SignExtension.CreateToken();
                            var iExpireTime = 240;
                            var sExpireTime = Common.GetSystemSetting(db, orgId, "ExpireTime");
                            if (!string.IsNullOrEmpty(sExpireTime))
                            {
                                iExpireTime = int.Parse(sExpireTime);
                            }
                            else
                            {
                                iExpireTime = int.Parse(WebSettingsConfig.ExpireTime);
                            }
                            oTicket.ExpireTime = DateTime.Now.AddMinutes(iExpireTime); //30分钟过期
                            if (oTicket.UserID != null && oTicket.OrgID != null)
                            {
                                var iRel = db.Updateable(oTicket).IgnoreColumns(it => new { it.NO })
                                    .Where(it => it.NO == oTicket.NO).ExecuteCommand();
                            }
                            else
                            {
                                oTicket.OrgID = orgId;
                                oTicket.UserID = userId;
                                oTicket.UserName = "";
                                oTicket.LoginIp = "";
                                oTicket.LoginTime = DateTime.Now;
                                oTicket.CreateTime = DateTime.Now;
                                db.Insertable(oTicket).ExecuteCommand();
                            }
                            HttpRuntimeCache.Set(oTicket.OrgID + oTicket.UserID, oTicket, iExpireTime * 60, true);
                        }
                        else
                        {
                            oTicket = new OTB_SYS_TicketAuth();
                        }
                    }

                    //返回token信息
                    srm = new SuccessResponseMessage(null, null);
                    srm.DATA.Add("token", oTicket.Token);
                } while (false);
            }
            catch (Exception ex)
            {
                sError = Util.GetLastExceptionMsg(ex);
                srm = new SuccessResponseMessage(null, null)
                {
                    STATUSCODE = (int)StatusCodeEnum.Error,
                    MSG = StatusCodeEnum.Error.GetEnumText()
                };
                srm.DATA.Add("token", "");
                LogAndSendEmail(sError + " Param：" + orgId + "|" + userId + "|" + pasWd, ex, orgId, userId, "AuthorizeService", nameof(GetToken), "GetToken（获取token）", "", "", "");
            }
            return HttpResponseExtension.ToJson(JsonConvert.SerializeObject(srm));
        }

        #endregion 根据用户名获取token
    }
}