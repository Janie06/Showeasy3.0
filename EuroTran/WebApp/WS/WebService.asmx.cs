using EasyBL;
using Entity.Sugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;
using System.Web.Script.Services;
using System.Web.Services;

namespace WebApp.WS
{
    /// <summary>
    ///WebService 的摘要描述
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允許使用 ASP.NET AJAX 從指令碼呼叫此 Web 服務，請取消註解下列一行。
    [ScriptService]
    public class WebService : System.Web.Services.WebService
    {
        public WebService()
        {
            //如果使用設計的元件，請取消註解下列一行
            //InitializeComponent();
        }

        #region QueryList

        /// <summary>
        /// 資料查詢List
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetOrgs()
        {
            string sMsg = null;
            Dictionary<string, object> dicResult = new Dictionary<string, object>
            {
                { "RESULT", 0 },
                { "DATA" , null },
                { "MSG", string.Empty }
            };

            try
            {
                do
                {
                    var db = SugarBase.DB;
                    var saOrgs = db.Queryable<OTB_SYS_Organization>().Where(x => x.Effective == "Y").OrderBy(x => x.CreateDate)
                        .Select(x => new { x.OrgID, x.OrgName }).ToList();

                    dicResult["RESULT"] = 1;
                    dicResult["DATA"] = saOrgs;

                } while (false);
            }
            catch (Exception ex)
            {
                LogService.MailSend(ex.Message, ex, "", "", nameof(WebService), "", nameof(GetOrgs), "", "", "");
                sMsg = ex.Message;
            }

            if (sMsg != null)
            {
                dicResult["MSG"] = $"查詢資料異常，請聯繫IT人員 ,ERROR MSG：{ sMsg }";
            }

            return ServiceBase.JsonToString(dicResult);
        }

        #endregion QueryList
    }
}