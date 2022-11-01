using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Net;
using System.Text;
using System.Web.Services.Description;

namespace EasyBL
{
    public class WebServiceUtil
    {
        /// <summary>
        /// 实例化WebServices
        /// </summary>
        /// <param name="url">WebServices地址</param>
        /// <param name="methodname">调用的方法</param>
        /// <param name="args">把webservices里需要的参数按顺序放到这个object[]里</param>
        public static object InvokeWebService(string url, string methodname, object[] args)
        {
            //这里的namespace是需引用的webservices的命名空间，在这里是写死的，大家可以加一个参数从外面传进来。
            var @namespace = "client";
            try
            {
                //获取WSDL
                using (var wc = new WebClient())
                {
                    var stream = wc.OpenRead(url + "?WSDL");
                    var sd = ServiceDescription.Read(stream);
                    var classname = sd.Services[0].Name;
                    var sdi = new ServiceDescriptionImporter();
                    sdi.AddServiceDescription(sd, "", "");
                    var cn = new CodeNamespace(@namespace);

                    //生成客户端代理类代码
                    var ccu = new CodeCompileUnit();
                    ccu.Namespaces.Add(cn);
                    sdi.Import(cn, ccu);
                    using (var csc = new CSharpCodeProvider())
                    {
#pragma warning disable CS0618 // 类型或成员已过时
                        var icc = csc.CreateCompiler();
#pragma warning restore CS0618 // 类型或成员已过时

                        //设定编译参数
#pragma warning disable IDE0017 // 简化对象初始化
                        var cplist = new CompilerParameters
                        {
#pragma warning restore IDE0017 // 简化对象初始化
                            GenerateExecutable = false,
                            GenerateInMemory = true
                        };
                        cplist.ReferencedAssemblies.Add("System.dll");
                        cplist.ReferencedAssemblies.Add("System.XML.dll");
                        cplist.ReferencedAssemblies.Add("System.Web.Services.dll");
                        cplist.ReferencedAssemblies.Add("System.Data.dll");

                        //编译代理类
                        var cr = icc.CompileAssemblyFromDom(cplist, ccu);
                        if (cr.Errors.HasErrors)
                        {
                            var sb = new StringBuilder();
                            foreach (CompilerError ce in cr.Errors)
                            {
                                sb.Append(ce.ToString());
                                sb.Append(Environment.NewLine);
                            }
                            throw new Exception(sb.ToString());
                        }

                        //生成代理实例，并调用方法
                        var assembly = cr.CompiledAssembly;
                        var t = assembly.GetType(@namespace + "." + classname, true, true);
                        var obj = Activator.CreateInstance(t);
                        var mi = t.GetMethod(methodname);

                        return mi.Invoke(obj, args);
                    }
                }
            }
            catch
            {
                return null;
            }
        }
    }
}