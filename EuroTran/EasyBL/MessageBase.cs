using EasyBL.WebApi.Message;
using EasyNet.Common;
using Entity.Sugar;
using log4net;
using SqlSugar.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EasyBL
{
    public class MessageBase
    {
        protected static string _getCustomData(RequestMessage i_joRequest, string i_sKey)
        {
            string sRes = null;

            if (i_joRequest.CUSTOMDATA != null && i_joRequest.CUSTOMDATA.ContainsKey(i_sKey))
            {
                sRes = i_joRequest.CUSTOMDATA[i_sKey];
            }

            return sRes;
        }

        public static T _fetchEntity<T>(RequestMessage i_joRequest) where T : new()
        {
            //var properties = TypeDescriptor.GetProperties(typeof(T));
            //var t = Activator.CreateInstance<T>();
            var entity = new T();
            var properties = ReflectionHelper.GetProperties(entity.GetType());
            foreach (PropertyInfo prop in properties)
            {
                try
                {
                    ReflectionHelper.SetPropertyValue(entity, prop, _fetchObject(i_joRequest.DATA, prop.Name));
                }
                catch (Exception)
                {
                    prop.SetValue(entity, null);
                }
            }
            return entity;
        }

        protected static string _fetchString(RequestMessage i_joRequest, string i_sKey)
        {
            return _fetchString(i_joRequest.DATA, i_sKey);
        }

        protected static int _fetchInt(RequestMessage i_joRequest, string i_sKey)
        {
            var sRes = _fetchString(i_joRequest.DATA, i_sKey);
            return sRes != null ? int.Parse(sRes) : -1;
        }

        protected static bool _fetchBool(RequestMessage i_joRequest, string i_sKey)
        {
            var sRes = _fetchString(i_joRequest.DATA, i_sKey);
            return sRes != null ? Convert.ToBoolean(sRes) : false;
        }

        protected static object _fetchObject(Dictionary<string, object> i_dic, string i_sKey)
        {
            object sRes = null;
            if (i_dic.ContainsKey(i_sKey))
            {
                sRes = i_dic[i_sKey];
            }
            return sRes;
        }

        protected static string _fetchString(Dictionary<string, object> i_dic, string i_sKey)
        {
            string sRes = null;

            if (i_dic.ContainsKey(i_sKey))
            {
                var obj = i_dic[i_sKey];
                if (null != obj)
                {
                    sRes = obj.ToString();
                }
            }
            return sRes;
        }

        protected static string _getKeyStr(Dictionary<string, object> i_dic, string i_sKey)
        {
            var sRes = "";

            if (i_dic.ContainsKey(i_sKey))
            {
                var obj = i_dic[i_sKey];
                if (null != obj)
                {
                    sRes = obj.ToString();
                }
            }
            return sRes;
        }

        protected static void _setEntityBase<T>(T i_entity, RequestMessage i_joRequest)
        {
            var properties = ReflectionHelper.GetProperties(i_entity.GetType());
            foreach (PropertyInfo prop in properties)
            {
                if (prop.Name == "OrgID")
                {
                    prop.SetValue(i_entity, i_joRequest.ORIGID);
                }
                else if ("ModifyUser,CreateUser".Contains(prop.Name))
                {
                    prop.SetValue(i_entity, i_joRequest.USERID);
                }
                else if ("ModifyDate,CreateDate".Contains(prop.Name))
                {
                    prop.SetValue(i_entity, DateTime.Now);
                }
            }
        }

        private ILog _inst = null;
        protected ILog Logger
        {
            get
            {
                if (_inst == null)
                {
                    _inst = LogManager.GetLogger(this.GetType());
                }
                return _inst;
            }
        }

        public void LogAndSendEmail(string sErrorMessage, Exception Exception, string sOrgID, string sUserID, string sProgramId, string sProgramName, string sFunctionName, string sErrorSource, string sErrorlineNO, string sErrorcolNO)
        {
            
            Logger.Error(sProgramName + sFunctionName + " Error:" + sErrorMessage, Exception);

            var db = SugarBase.GetIntance();
            string sError = null;
            string sUserFromName = null;
            var sEmailBody = "";
            if (string.IsNullOrWhiteSpace(sProgramId))
            {
                //拆分JS錯誤來源網址
                var saErrorSource = sErrorSource.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                sProgramId = saErrorSource.LastOrDefault();
            }
            if (string.IsNullOrWhiteSpace(sProgramName))
            {
                sProgramName = "";
            }
            if (string.IsNullOrWhiteSpace(sProgramName))
            {
                sFunctionName = "";
            }
            if (Exception != null)
            {
                sErrorMessage += "<br/>" + Exception.ToString();
            }

            //查詢工程師郵件,拆分維護工程師郵件
            var saEmails = Common.GetSystemSetting(db, sOrgID, "ErrorEngineer").Split(new string[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries);
            //獲取Email郵件格式
            var oErrorMessage = db.Queryable<OTB_SYS_Email>().Single(it => it.OrgID == sOrgID && it.EmailID == nameof(ErrorMessage));

            if (oErrorMessage != null)
            {
                if (!string.IsNullOrWhiteSpace(sUserID))
                {
                    var oUserFrom = db.Queryable<OTB_SYS_Members>().Single(it => it.OrgID == sOrgID && it.MemberID == sUserID);

                    if (oUserFrom != null)
                    {
                        sUserFromName = oUserFrom.MemberName;
                    }
                }

                //寄信開始
                foreach (string email in saEmails)
                {
                    //利用郵件獲取工程師ID和名稱
                    var oUserTo = db.Queryable<OTB_SYS_Members>().Single(it => it.OrgID == sOrgID && it.Email == email);

                    if (oUserTo == null)
                    {
                        oUserTo = new OTB_SYS_Members();
                    }
                    sEmailBody = oErrorMessage.BodyHtml.Replace("{{:UserName}}", oUserTo.MemberName)
                        .Replace("{{:UseMember}}", sUserFromName ?? sUserID)
                        .Replace("{{:FunctionName}}", sFunctionName)
                        .Replace("{{:ErrorRow}}", sErrorlineNO)
                        .Replace("{{:ErrorColumn}}", sErrorcolNO)
                        .Replace("{{:ProgramId}}", sProgramId)
                        .Replace("{{:ProgramName}}", sProgramName)
                        .Replace("{{:error}}", sErrorMessage);

                    var oEmail = new Emails();
                    var saEmailTo = new List<EmailTo>();   //收件人
                    var oEmailTo = new EmailTo
                    {
                        ToUserID = oUserTo.MemberID,
                        ToUserName = oUserTo.MemberName,
                        ToEmail = email,
                        Type = "to"
                    };
                    saEmailTo.Add(oEmailTo);

                    oEmail.FromUserName = "系統自動發送";//取fonfig
                    oEmail.Title = "奕達運通管理系統錯誤信息派送";//取fonfig
                    oEmail.EmailBody = sEmailBody;
                    oEmail.IsCCSelf = false;
                    oEmail.Attachments = null;
                    oEmail.EmailTo = saEmailTo;

                    var bSend = new MailService(sOrgID, true).MailFactory(oEmail, out sError);
                }
            }
        }

        public ResponseMessage ErrorMessage(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                var sErrorSource = _fetchString(i_crm, "ErrorSource");
                var sErrorlineNO = _fetchString(i_crm, "Errorlineno");
                var sErrorcolNO = _fetchString(i_crm, "Errorcolno");
                var sErrorMessage = _fetchString(i_crm, nameof(ErrorMessage));

                LogAndSendEmail(sErrorMessage, null, i_crm.ORIGID, i_crm.USERID, "", "", "", sErrorSource, sErrorlineNO, sErrorcolNO);

                rm = new SuccessResponseMessage(null, i_crm);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
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

    }
}