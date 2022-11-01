using System;
using System.Web;
using System.Web.Caching;

namespace EasyBL
{
    /// <summary>
    /// HttpRuntime Cache读取设置缓存信息封装 <auther><name>John</name><date>20185-8-11</date></auther>
    /// 使用描述：给缓存赋值使用HttpRuntimeCache.Set(key,value....)等参数(第三个参数可以传递文件的路径(HttpContext.Current.Server.MapPath()))
    /// 读取缓存中的值使用JObject jObject=HttpRuntimeCache.Get(key) as JObject，读取到值之后就可以进行一系列判断
    /// </summary>
    public static class HttpRuntimeCache
    {
        /// <summary>
        /// 设置缓存时间，配置(从配置文件中读取)
        /// </summary>
        private const double Seconds = 30 * 24 * 60 * 60;

        /// <summary>
        /// 缓存指定对象，设置缓存
        /// </summary>
        /// <param name="key">todo: describe key parameter on Set</param>
        /// <param name="value">todo: describe value parameter on Set</param>
        public static bool Set(string key, object value)
        {
            return Set(key, value, null, DateTime.Now.AddSeconds(Seconds), Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
        }

        /// <summary>
        /// 缓存指定对象，设置缓存
        /// </summary>
        /// <param name="key">todo: describe key parameter on Set</param>
        /// <param name="value">todo: describe value parameter on Set</param>
        /// <param name="path">todo: describe path parameter on Set</param>
        public static bool Set(string key, object value, string path)
        {
            try
            {
                var cacheDependency = new CacheDependency(path);
                return Set(key, value, cacheDependency);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 缓存指定对象，设置缓存
        /// </summary>
        /// <param name="key">todo: describe key parameter on Set</param>
        /// <param name="value">todo: describe value parameter on Set</param>
        /// <param name="cacheDependency">todo: describe cacheDependency parameter on Set</param>
        public static bool Set(string key, object value, CacheDependency cacheDependency)
        {
            return Set(key, value, cacheDependency, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
        }

        /// <summary>
        /// 缓存指定对象，设置缓存
        /// </summary>
        /// <param name="key">todo: describe key parameter on Set</param>
        /// <param name="value">todo: describe value parameter on Set</param>
        /// <param name="seconds">todo: describe seconds parameter on Set</param>
        /// <param name="isAbsulute">todo: describe isAbsulute parameter on Set</param>
        public static bool Set(string key, object value, double seconds, bool isAbsulute)
        {
            return Set(key, value, null, (isAbsulute ? DateTime.Now.AddSeconds(seconds) : Cache.NoAbsoluteExpiration),
                (isAbsulute ? Cache.NoSlidingExpiration : TimeSpan.FromSeconds(seconds)), CacheItemPriority.Default, null);
        }

        /// <summary>
        /// 获取缓存对象
        /// </summary>
        /// <param name="key">todo: describe key parameter on Get</param>
        public static object Get(string key)
        {
            return GetPrivate(key);
        }

        /// <summary>
        /// 判断缓存中是否含有缓存该键
        /// </summary>
        /// <param name="key">todo: describe key parameter on Exists</param>
        public static bool Exists(string key)
        {
            return (GetPrivate(key) != null);
        }

        /// <summary>
        /// 移除缓存对象
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool Remove(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }
            HttpRuntime.Cache.Remove(key);
            return true;
        }

        /// <summary>
        /// 移除所有缓存
        /// </summary>
        /// <returns></returns>
        public static bool RemoveAll()
        {
            var iDictionaryEnumerator = HttpRuntime.Cache.GetEnumerator();
            while (iDictionaryEnumerator.MoveNext())
            {
                HttpRuntime.Cache.Remove(Convert.ToString(iDictionaryEnumerator.Key));
            }
            return true;
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key">todo: describe key parameter on Set</param>
        /// <param name="value">todo: describe value parameter on Set</param>
        /// <param name="cacheDependency">todo: describe cacheDependency parameter on Set</param>
        /// <param name="dateTime">todo: describe dateTime parameter on Set</param>
        /// <param name="timeSpan">todo: describe timeSpan parameter on Set</param>
        /// <param name="cacheItemPriority">todo: describe cacheItemPriority parameter on Set</param>
        /// <param name="cacheItemRemovedCallback">
        /// todo: describe cacheItemRemovedCallback parameter on Set
        /// </param>
        public static bool Set(string key, object value, CacheDependency cacheDependency, DateTime dateTime,
            TimeSpan timeSpan, CacheItemPriority cacheItemPriority, CacheItemRemovedCallback cacheItemRemovedCallback)
        {
            if (string.IsNullOrEmpty(key) || value == null)
            {
                return false;
            }
            HttpRuntime.Cache.Insert(key, value, cacheDependency, dateTime, timeSpan, cacheItemPriority, cacheItemRemovedCallback);
            return true;
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="key">todo: describe key parameter on GetPrivate</param>
        private static object GetPrivate(string key)
        {
            return string.IsNullOrEmpty(key) ? null : HttpRuntime.Cache.Get(key);
        }
    }
}