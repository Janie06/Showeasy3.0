using System.Configuration;
using System.Runtime.CompilerServices;

namespace EasyBL.WebApi.Common
{
    public class WebSettingsConfig
    {
        public static string UrlExpireTime
        {
            get
            {
                return AppSettingValue();
            }
        }

        public static string TokenApi
        {
            get
            {
                return AppSettingValue();
            }
        }

        public static string GetApi
        {
            get
            {
                return AppSettingValue();
            }
        }

        public static string OrgId
        {
            get
            {
                return AppSettingValue();
            }
        }

        public static string UserId
        {
            get
            {
                return AppSettingValue();
            }
        }

        public static string PassWd
        {
            get
            {
                return AppSettingValue();
            }
        }

        public static string ExpireTime
        {
            get
            {
                return AppSettingValue();
            }
        }

        private static string AppSettingValue([CallerMemberName] string key = null)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}