using System;
using System.Net.Http;
using System.Text;
using System.Web.Script.Serialization;

namespace EasyBL.WebApi.Common
{
    public class HttpResponseExtension
    {
        public static HttpResponseMessage ToJson(Object obj)
        {
            String str;
            if (obj is String || obj is Char)
            {
                str = obj.ToString();
            }
            else
            {
                var serializer = new JavaScriptSerializer();
                str = serializer.Serialize(obj);
            }
            var result = new HttpResponseMessage { Content = new StringContent(str, Encoding.GetEncoding("UTF-8"), "application/json") };
            return result;
        }
    }
}