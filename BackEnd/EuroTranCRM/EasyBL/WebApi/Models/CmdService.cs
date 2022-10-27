using EasyBL.WebApi.Message;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;
using System.Collections.Generic;

namespace EasyBL.WebApi.Models
{
    public partial class CmdService : ApiServiceBase
    {
        static public string DecodeParm(string i_sEncodedData)
        {
            i_sEncodedData = i_sEncodedData.Replace(" ", "+");
            var encodedDataAsBytes = Convert.FromBase64String(i_sEncodedData);
            var returnValue = ASCIIEncoding.ASCII.GetString(encodedDataAsBytes);
            return HttpUtility.UrlDecode(returnValue);
        }

        public HttpResponseMessage GetData([FromBody]dynamic i_value, bool i_bDecode, HttpRequestMessage i_rRequest)
        {
            string sRes = null;

            try
            {
                string value = (i_bDecode) ? DecodeParm(i_value) : null;

                sRes = ProgessJson(value, i_rRequest);
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
                Content = new StringContent(sRes, Encoding.UTF8, "application/json")
            };
        }

        class ModuleType
        {
            public string Module { get; set; }
            public string Type { get; set; }
            public string Demo { get; set; }
        }

        protected override string HandleRequest(RequestMessage i_joRequest, HttpRequestMessage i_rRequest)
        {
            var sRes = "";

            try
            {
                do
                {
                    bool blnDemo = false;
                    string strPage = "/Demo/ModuleType.json";
                    string strPath = HttpContext.Current.Server.MapPath(strPage);

                    if (System.IO.File.Exists(strPath))
                    {
                        string content = System.IO.File.ReadAllText(strPath);

                        JavaScriptSerializer js = new JavaScriptSerializer();
                        List<ModuleType> list = js.Deserialize<List<ModuleType>>(content);

                        foreach (ModuleType vo in list)
                        {
                            if (vo.Demo == "Y" && i_joRequest.TYPE == vo.Type && i_joRequest.MODULE == vo.Module)
                            {
                                blnDemo = true;
                            }
                        }

                        strPage = "/Demo/" + i_joRequest.MODULE + "_" + i_joRequest.TYPE + ".json";
                        strPath = HttpContext.Current.Server.MapPath(strPage);
                    }

                    //file exsist
                    if (blnDemo && System.IO.File.Exists(strPath))
                    {
                        sRes = System.IO.File.ReadAllText(strPath);

                    } else
                    {
                        if (i_joRequest == null || string.IsNullOrEmpty(i_joRequest.TYPE) || string.IsNullOrEmpty(i_joRequest.MODULE))
                        {
                            sRes = MakeErrorReturn(i_joRequest, string.Format(BLWording.REQUEST_IS_NULL));
                            break;
                        }

                        var sModuleType = i_joRequest.CUSTOMDATA.ContainsKey("module_id") ? i_joRequest.CUSTOMDATA["module_id"] : "";
                        var sModuleName = i_joRequest.MODULE + "Service";
                        var sCreateError = GetInstByClassName(sModuleName, sModuleType, out object oModule);

                        if (sCreateError != null || oModule == null)
                        {
                            sRes = MakeErrorReturn(i_joRequest, sCreateError);
                            break;
                        }

                        if (!(oModule is ServiceBase bls))
                        {
                            sRes = MakeErrorReturn(i_joRequest, BLWording.COVERT_FAIL);
                            break;
                        }
                        sRes = MakeMessage(bls.Entry(i_joRequest));
                    }
                }
                while (false);
            }
            catch (Exception e)
            {
                sRes = MakeErrorReturn(i_joRequest, e.Message);
            }

            if (string.IsNullOrWhiteSpace(sRes))
            {
                sRes = MakeErrorReturn(i_joRequest, "Unknow Error");
            }
            return sRes;
        }

        /// <summary>
        /// </summary>
        /// <param name="i_sTypeName"></param>
        /// <param name="i_sModuleId"></param>
        /// <param name="o_oRes"></param>
        /// <returns></returns>
        public static string GetInstByClassName(string i_sTypeName, string i_sModuleId, out object o_oRes)
        {
            object obj2 = null;
            string str = null;
            var typeByTypeName = GetTypeByTypeName(i_sTypeName, i_sModuleId);
            if (typeByTypeName == null)
            {
                str = "NO THIS ENTITY";
            }
            else
            {
                obj2 = Activator.CreateInstance(typeByTypeName);
                if (obj2 == null)
                {
                    str = "ENTITY CREATE FAIL";
                }
            }
            o_oRes = obj2;
            return str;
        }

        /// <summary>
        /// </summary>
        /// <param name="i_sTypeName"></param>
        /// <param name="i_sModuleId"></param>
        /// <returns></returns>
        public static Type GetTypeByTypeName(string i_sTypeName, string i_sModuleId)//
        {
            Type type = null;
            var codeBase = Assembly.GetExecutingAssembly().GetName().CodeBase;
            codeBase = codeBase.Substring(0, codeBase.LastIndexOf("/"));
            var assemblyArray = (from f in AppDomain.CurrentDomain.GetAssemblies()
                                 where !f.IsDynamic
                                 && f.CodeBase != null
                                 && f.CodeBase.StartsWith(codeBase, StringComparison.Ordinal)
                                 && f.IsDefined(typeof(AssemblyCompanyAttribute), false)
                                 && f.IsDefined(typeof(AssemblyDescriptionAttribute), false)
                                 && f.IsDefined(typeof(AssemblyProductAttribute), false)
                                 && f.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false).OfType<AssemblyDescriptionAttribute>().FirstOrDefault().Description == "Service"
                                 && ((f.GetCustomAttributes(typeof(AssemblyProductAttribute), false).OfType<AssemblyProductAttribute>().FirstOrDefault().Product.Equals(nameof(EasyBL)))
                                 || (f.GetCustomAttributes(typeof(AssemblyProductAttribute), false).OfType<AssemblyProductAttribute>().FirstOrDefault().Product.Contains("EasyBL." + i_sModuleId.ToUpper())))
                                 orderby f.GetCustomAttributes(typeof(AssemblyProductAttribute), false).OfType<AssemblyProductAttribute>().FirstOrDefault().Product descending
                                 select f).ToArray<Assembly>();

            // && f.GetCustomAttributes(typeof(AssemblyCompanyAttribute),
            // false).OfType<AssemblyCompanyAttribute>().FirstOrDefault() != null &&
            // f.GetCustomAttributes(typeof(AssemblyDescriptionAttribute),
            // false).OfType<AssemblyDescriptionAttribute>().FirstOrDefault() != null &&
            // f.GetCustomAttributes(typeof(AssemblyProductAttribute),
            // false).OfType<AssemblyProductAttribute>().FirstOrDefault() != null

            foreach (Assembly assembly2 in assemblyArray)
            {
                var typeArray = assembly2.GetTypes().Where(x => x.IsSubclassOf(typeof(ServiceBase))).ToArray<Type>();
                foreach (Type classType in typeArray)
                {
                    if (classType.IsClass && (classType.Name == i_sTypeName))
                    {
                        type = classType;
                        return type;
                    }
                }
            }
            return type;
        }
    }
}