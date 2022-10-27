using Entity.Sugar;
using Euro.Transfer.Base;
using SqlSugar.Base;
using System;
using System.Configuration;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

namespace Euro.Transfer
{
    public class Common
    {
        #region GetAppSetting

        /// <summary>
        /// 獲取WebService的配置信息
        /// </summary>
        /// <param name="sKey">appSettings中配置的Key值</param>
        /// <example></example>
        /// <returns>appSettings中配置的value值</returns>
        public static string GetAppSetting(string sKey)
        {
            return ConfigurationManager.AppSettings[sKey].ToString();
        }

        #endregion GetAppSetting

        #region UpdateAppSettings

        /// <summary>
        /// 修改config配置
        /// </summary>
        /// <param name="key">要修改的key</param>
        /// <param name="value">修改的值</param>
        /// <returns></returns>
        public static bool UpdateAppSettings(string key, string value)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (!config.HasFile)
            {
                throw new ArgumentException("程序配置文件缺失！");
            }
            var _key = config.AppSettings.Settings[key];
            if (_key == null)
                config.AppSettings.Settings.Add(key, value);
            else
                config.AppSettings.Settings[key].Value = value;
            config.Save(ConfigurationSaveMode.Modified);
            return true;
        }

        #endregion UpdateAppSettings

        #region ConfigSetValue

        /// <summary>
        /// 写操作
        /// </summary>
        /// <param name="AppKey"></param>
        /// <param name="AppValue"></param>
        /// <param name="sExecutablePath">todo: describe sExecutablePath parameter on ConfigSetValue</param>
        public static void ConfigSetValue(string sExecutablePath, string AppKey, string AppValue)
        {
            if (!Directory.Exists(sExecutablePath))
            {
                sExecutablePath = System.Windows.Forms.Application.StartupPath.ToString();
            }
            var xDoc = new XmlDocument();
            //获取可执行文件的路径和名称
            xDoc.Load(sExecutablePath + ".config");

            XmlNode xNode;
            XmlElement xElem1;
            XmlElement xElem2;
            xNode = xDoc.SelectSingleNode("//connectionStrings");
            // xDoc.Load(System.Windows.Forms.Application.ExecutablePath + ".config");
            xElem1 = (XmlElement)xNode.SelectSingleNode("//add[@name='" + AppKey + "']");
            if (xElem1 != null) xElem1.SetAttribute("connectionString", AppValue);
            else
            {
                xElem2 = xDoc.CreateElement("add");
                xElem2.SetAttribute("name", AppKey);
                xElem2.SetAttribute("connectionString", AppValue);
                xNode.AppendChild(xElem2);
            }
            xDoc.Save(sExecutablePath + "Euro.Transfer.exe.config");
        }

        #endregion ConfigSetValue

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
                sExecutablePath = System.Windows.Forms.Application.StartupPath.ToString() + "/";
            }
            var xDoc = new XmlDocument();
            try
            {
                xDoc.Load(sExecutablePath + "Euro.Transfer.exe.config");

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

        #region fnCreateDir

        /// <summary>
        /// 創建目錄
        /// </summary>
        /// <param name="strPath"></param>
        public static void FnCreateDir(string strPath)
        {
            if (!Directory.Exists(strPath))
            {
                Directory.CreateDirectory(strPath);
            }
        }

        #endregion fnCreateDir

        #region GetSystemSetting

        /// <summary>
        /// 獲取系統設定
        /// </summary>
        /// <param name="sOrgID">todo: describe sOrgID parameter on GetSystemSetting</param>
        /// <param name="sItemID">todo: describe sItemID parameter on GetSystemSetting</param>
        /// <returns></returns>
        public static string GetSystemSetting(string sOrgID, string sItemID)
        {
            var sSettingValue = string.Empty;
            if (!string.IsNullOrWhiteSpace(sOrgID) && !string.IsNullOrWhiteSpace(sItemID))
            {
                try
                {
                    var db = SugarBase.DB;
                    var oSet = db.Queryable<OTB_SYS_SystemSetting>().Single(it => it.OrgID == sOrgID && it.SettingItem == sItemID);
                    if (oSet != null)
                    {
                        sSettingValue = oSet.SettingValue;
                    }
                }
                catch (Exception ex)
                {
                    //写错误日志
                    ServiceTools.WriteLog(ServiceBase.Errorlog_Path, "Euro.Transfer.Common.GetSystemSetting（獲取系統設定） Error:" + ex.Message + " sOrgID：" + sOrgID + ";sItemID：" + sItemID, true);
                }
            }
            return sSettingValue;
        }

        #endregion GetSystemSetting

        #region PadRightEx

        /// <summary>
        /// </summary>
        /// <param name="str"></param>
        /// <param name="totalByteCount"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static string PadRightEx(string str, int totalByteCount, char c)
        {
            var coding = Encoding.GetEncoding("gb2312");
            var dcount = 0;
            foreach (char ch in str.ToCharArray())
            {
                if (coding.GetByteCount(ch.ToString()) == 2)
                    dcount++;
            }
            var w = str.PadRight(totalByteCount - dcount, c);
            return w;
        }

        #endregion PadRightEx
    }

    /// <summary>
    /// 中文字符轉換工具
    /// </summary>
    public class ChineseStringUtility
    {
        private const int LOCALE_SYSTEM_DEFAULT = 0x0800;
        private const int LCMAP_SIMPLIFIED_CHINESE = 0x02000000;
        private const int LCMAP_TRADITIONAL_CHINESE = 0x04000000;

        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int LCMapString(int Locale, int dwMapFlags, string lpSrcStr, int cchSrc, [Out] string lpDestStr, int cchDest);

        /// <summary>
        /// 将字符转换成简体中文
        /// </summary>
        /// <param name="source">输入要转换的字符串</param>
        /// <returns>转换完成后的字符串</returns>
        public static string ToSimplified(string source)
        {
            var target = new String(' ', source.Length);
            var ret = LCMapString(LOCALE_SYSTEM_DEFAULT, LCMAP_SIMPLIFIED_CHINESE, source, source.Length, target, source.Length);
            return target;
        }

        /// <summary>
        /// 将字符转换为繁体中文
        /// </summary>
        /// <param name="source">输入要转换的字符串</param>
        /// <returns>转换完成后的字符串</returns>
        public static string ToTraditional(string source)
        {
            var target = new String(' ', source.Length);
            var ret = LCMapString(LOCALE_SYSTEM_DEFAULT, LCMAP_TRADITIONAL_CHINESE, source, source.Length, target, source.Length);
            return target;
        }
    }
}