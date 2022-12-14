using System;

namespace SqlSugar
{
    internal class CacheSchemeMain
    {
        public static T GetOrCreate<T>(ICacheService cacheService, QueryBuilder queryBuilder, Func<T> getData, int cacheDurationInSeconds, SqlSugarClient context)
        {
            var key = CacheKeyBuider.GetKey(context, queryBuilder);
            var keyString = key.ToString();
            var result = cacheService.GetOrCreate(keyString, getData, cacheDurationInSeconds);
            return result;
        }

        public static void RemoveCache(ICacheService cacheService, string tableName)
        {
            var keys = cacheService.GetAllKey<string>();
            if (keys.HasValue())
            {
                foreach (var item in keys)
                {
                    if (item.ToLower().Contains(UtilConstants.Dot + tableName.ToLower() + UtilConstants.Dot))
                    {
                        cacheService.Remove<string>(item);
                    }
                }
            }
        }
    }
}
