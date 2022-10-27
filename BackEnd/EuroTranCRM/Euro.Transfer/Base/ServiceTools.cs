using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Euro.Transfer.Base
{
    /// <summary>
    /// 工具类
    /// </summary>
    public class ServiceTools : ServiceBase, IConfigurationSectionHandler
    {
        /// <summary>
        /// 获取configSections节点
        /// </summary>
        /// <returns></returns>
        public static XmlNode GetConfigSections()
        {
            var doc = new XmlDocument();
            doc.Load(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).FilePath);
            return doc.DocumentElement.FirstChild;
        }

        /// <summary>
        /// 获取section节点
        /// </summary>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        public static NameValueCollection GetSection(string nodeName)
        {
            return (NameValueCollection)ConfigurationManager.GetSection(nodeName);
        }

        /// <summary>
        /// 停止Windows服务
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        public static void WindowsServiceStop(string serviceName)
        {
            var control = new System.ServiceProcess.ServiceController(serviceName);
            control.Stop();
            control.Dispose();
        }

        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="path">日志文件</param>
        /// <param name="cont">日志内容</param>
        /// <param name="isAppend">是否追加方式</param>
        /// <param name="foder">todo: describe foder parameter on WriteWordLog</param>
        public static void WriteWordLog(string path, string cont, bool isAppend, string foder)
        {
            if (path == "")
            {
                path = System.Windows.Forms.Application.StartupPath.ToString() + @"\WordLogs\" + foder + "\\";
            }
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            path += DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
            using (StreamWriter sw = new StreamWriter(path, isAppend, Encoding.UTF8))
            {
                sw.WriteLine(DateTime.Now);
                sw.WriteLine(cont);
                sw.WriteLine("");
                sw.Close();
            }
        }

        /// <summary>
        /// 写錯誤日志
        /// </summary>
        /// <param name="path">日志文件</param>
        /// <param name="cont">日志内容</param>
        /// <param name="isAppend">是否追加方式</param>
        public static void WriteLog(string path, string cont, bool isAppend)
        {
            if (path == "")
            {
                path = System.Windows.Forms.Application.StartupPath.ToString() + @"\ErrorLogs\";
            }
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            path += "//" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
            using (StreamWriter sw = new StreamWriter(path, isAppend, Encoding.UTF8))
            {
                sw.WriteLine(DateTime.Now);
                sw.WriteLine(cont);
                sw.WriteLine("");
                sw.Close();
            }
        }

        /// <summary>
        /// 写文字檔
        /// </summary>
        /// <param name="cont">文字檔内容</param>
        /// <param name="subname">文字檔副檔名</param>
        /// <param name="prename">文字檔前最</param>
        /// <param name="orgid">todo: describe orgid parameter on WriteWords</param>
        public static void WriteWords(string cont, string orgid, string subname, string prename = "")
        {
            var path = Common.ConfigGetValue("", "WriteWordPath") + "\\";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var sTransferID = Common.ConfigGetValue("", "TransferID");
            var name = Common.GetSystemSetting(orgid, sTransferID);
            path += prename + name + subname;
            using (StreamWriter sw = new StreamWriter(path, true, Encoding.Default))
            {
                sw.WriteLine(cont);
                sw.Close();
            }
        }

        /// <summary>
        /// 实现接口以读写app.config
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="configContext"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        public object Create(object parent, object configContext, XmlNode section)
        {
            var handler = new NameValueSectionHandler();
            return handler.Create(parent, configContext, section);
        }
    }
}