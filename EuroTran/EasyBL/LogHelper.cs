///-----------------------------------------------------------------------
/// <copyright file="LogHelper.cs" company="Eurotran">
///  程式代號: LogHelper
///  程式名稱: LogHelper
///  程式說明: 
///  起始作者: 
///  起始日期: 2017/05/09 16:14:59
///  最新修改人: 
///  最新修日期: 2017/05/18 17:45:54
/// </copyright>
///-----------------------------------------------------------------------

namespace EasyBL
{
    using log4net;
    using log4net.Appender;
    using log4net.Repository;
    using log4net.Repository.Hierarchy;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;

    /// <summary>
    ///   類別名稱:LogHelper
    ///   類別說明:
    ///   起始作者:
    ///   起始日期:
    ///   最新修改人:
    ///   最新修改日:
    /// </summary>
    public static class LogHelper
    {
        /// <summary>
        ///  函式名稱:Flush
        ///  函式說明:
        ///  起始作者:
        ///  起始日期:
        ///  最新修改人:
        ///  最新修改日:
        /// </summary>
        /// <param name="i_iLog">
        ///  參數說明
        /// </param> 
        /// <returns>
        ///  回傳
        /// </returns>
        public static void Flush(ILog i_iLog)
        {
            //ILoggerRepository rep = LogManager.GetRepository();
            //foreach (IAppender appender in rep.GetAppenders())
            //{
            //    var buffered = appender as BufferingAppenderSkeleton;
            //    if (buffered != null)
            //    {
            //        buffered.Flush();
            //    }
            //}

            var logger = i_iLog.Logger as Logger;
            if (logger != null)
            {
                foreach (IAppender appender in logger.Appenders)
                {
                    var buffered = appender as BufferingAppenderSkeleton;
                    if (buffered != null)
                    {
                        buffered.Flush();
                    }
                }
            }
        }
        /// <summary>
        ///  函式名稱:MakeDirs
        ///  函式說明:
        ///  起始作者:
        ///  起始日期:
        ///  最新修改人:
        ///  最新修改日:
        /// </summary>
        /// <param name="i_sRootPath"></param>
        /// <param name="i_sPath">
        ///  參數說明
        /// </param> 
        /// <returns>
        ///  回傳
        /// </returns>
        private static void MakeDirs(string i_sRootPath, string i_sPath)
        {

            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(i_sPath);

                XmlNodeList nodeList = xmlDoc.SelectNodes("/configuration/log4net/appender/file");

                foreach (XmlNode folderPath in nodeList)
                {
                    string sPath = System.IO.Path.Combine(i_sRootPath, folderPath.Attributes["value"].Value.ToString());

                    if (Directory.Exists(sPath) == false)
                    {
                        Directory.CreateDirectory(sPath);
                    }
                }
            }
            catch
            {

            }
        }
        /// <summary>
        ///  函式名稱:Init
        ///  函式說明:
        ///  起始作者:
        ///  起始日期:
        ///  最新修改人:
        ///  最新修改日:
        /// </summary>
        /// <param name="i_sWorkSpace"></param>
        /// <param name="i_sConfigPath">
        ///  參數說明
        /// </param> 
        /// <returns>
        ///  回傳
        /// </returns>
        public static void Init(string i_sWorkSpace, string i_sConfigPath)
        {

            MakeDirs(i_sWorkSpace, i_sConfigPath);
            //string sConfigContent = File.ReadAllText(i_sConfigPath, Encoding.UTF8);
            //MemoryStream mStrm = new MemoryStream(Encoding.UTF8.GetBytes(sConfigContent));
            //log4net.Config.XmlConfigurator.Configure(mStrm);

        }
    }
}
