using System;
using System.Configuration;
using System.IO;
using System.Xml;

namespace SqlSugar
{
    public static class DBUnit
    {
        #region GetAppSettings

        /// <summary>
        /// 獲取WebService的配置信息
        /// </summary>
        /// <param name="sKey">todo: describe sKey parameter on GetAppSettings</param>
        /// <example></example>
        /// <returns>appSettings中配置的value值</returns>
        public static string GetAppSettings(string sKey)
        {
            var sVal = ConfigurationManager.AppSettings[sKey];
            return sVal ?? "";
        }

        #endregion GetAppSettings

        #region ConfigGetValue

        /// <summary>
        /// 读操作
        /// </summary>
        /// <param name="appKey"></param>
        /// <param name="sExecutablePath">todo: describe sExecutablePath parameter on ConfigGetValue</param>
        /// <returns></returns>
        public static string ConfigGetValue(string sExecutablePath, string appKey)
        {
            if (!Directory.Exists(sExecutablePath))
            {
                sExecutablePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "");
            }
            var xDoc = new XmlDocument();
            try
            {
                xDoc.Load(sExecutablePath + "Web.config");

                XmlNode xNode;
                XmlElement xElem;
                xNode = xDoc.SelectSingleNode("//appSettings");
                xElem = (XmlElement)xNode.SelectSingleNode("//add[@key='" + appKey + "']");
                if (xElem != null)
                    return xElem.GetAttribute("value");
                else
                    return "";
            }
            catch (Exception)
            {
                return "";
            }
        }

        #endregion ConfigGetValue
    }
}