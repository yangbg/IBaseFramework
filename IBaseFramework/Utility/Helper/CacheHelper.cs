using Microsoft.Extensions.Caching.Memory;
using System;
using System.Text;
using IBaseFramework.Logger.Logging;

namespace IBaseFramework.Utility.Helper
{
    /// <summary>
    /// 缓存辅助类
    /// </summary>
    public class CacheHelper
    {
        private static readonly ILogger Logger = LoggerManager.Logger<CacheHelper>();
        
        private static readonly IMemoryCache Cache = new MemoryCache(new MemoryCacheOptions());

        /// <summary>
        /// 添加缓存
        /// </summary>
        /// <param name="key">缓存 键</param>
        /// <param name="obj">缓存对象</param>
        /// <param name="minutes">有效时间(分钟)</param>
        /// <returns></returns>
        public static bool Add(string key, object obj, int minutes)
        {
            if (string.IsNullOrEmpty(key) || minutes <= 0)
                return false;

            Cache.Set(key, obj, TimeSpan.FromMinutes(minutes));

            return true;
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="key">缓存 键</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Get<T>(string key)
        {
            try
            {
                if (!string.IsNullOrEmpty(key) && Cache.TryGetValue(key, out T val))
                {
                    return val;
                }
                return default(T);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return default(T);
            }

        }
        
        /// <summary>
        /// 删除缓存
        /// </summary>
        /// <param name="key"></param>
        public static void Delete(string key)
        {
            if (string.IsNullOrEmpty(key))
                return;

            Cache.Remove(key);
        }
    }
}
