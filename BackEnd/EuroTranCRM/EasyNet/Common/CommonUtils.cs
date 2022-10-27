using System;
using System.Configuration;

namespace EasyNet.Common
{
    public class CommonUtils
    {
        /// <summary>
        /// 用於字串和枚舉類型的轉換
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T EnumParse<T>(string value)
        {
            try
            {
                return (T)Enum.Parse(typeof(T), value);
            }
            catch
            {
                throw new Exception("傳入的值與枚舉值不匹配。");
            }
        }

        /// <summary>
        /// 根據傳入的Key獲取設定檔中的Value值
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        public static string GetConfigValueByKey(string Key)
        {
            return ConfigurationManager.AppSettings[Key];
        }

        public static Boolean IsNullOrEmpty(Object value)
        {
            if (value == null)
                return true;
            if (String.IsNullOrEmpty(value.ToString()))
                return true;
            return false;
        }
    }
}