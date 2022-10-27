using EasyBL.WebApi.Message;
using System;
using System.Windows.Forms;

namespace EasyBL
{
    public class WebBrowserService : ServiceBase
    {
        /// <summary>
        /// 錯誤信息派送
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on ErrorMessage</param>
        /// <returns></returns>
        public ResponseMessage ErrorMessage(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                var sUrl = _fetchString(i_crm, "Url ");
                using (var webbrowser = new WebBrowser())
                {
                    webbrowser.Navigate(sUrl);//浏览urlString表示的网址

                    rm = new SuccessResponseMessage(null, i_crm);
                }
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