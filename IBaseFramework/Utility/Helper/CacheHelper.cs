using Microsoft.Extensions.Caching.Memory;
using System;
using IBaseFramework.Logger.Logging;
using IBaseFramework.Base;

namespace IBaseFramework.Utility.Helper
{
    public interface ICacheHelper : IDependency
    {
        bool Add(object key, object obj, int minutes);
        T Get<T>(object key);
        void Delete(object key);
        bool Exists(object key);
    }

    /// <summary>
    /// 缓存辅助类
    /// </summary>
    public class MemoryCacheHelper : ICacheHelper
    {
        private readonly IMemoryCache _cache = null;

        public MemoryCacheHelper(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
        }

        /// <summary>
        /// 添加缓存
        /// </summary>
        /// <param name="key">缓存 键</param>
        /// <param name="obj">缓存对象</param>
        /// <param name="minutes">有效时间(分钟)</param>
        /// <returns></returns>
        public bool Add(object key, object obj, int minutes)
        {
            try
            {
                if (key == null || minutes <= 0)
                    return false;

                _cache.Set(key, obj, TimeSpan.FromMinutes(minutes));

                return true;
            }
            catch (Exception ex)
            {
                var logger = LoggerManager.Logger<MemoryCacheHelper>();
                logger.Error(ex.Message, ex);

                return false;
            }
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="key">缓存 键</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>(object key)
        {
            try
            {
                return _cache.Get<T>(key);
            }
            catch (Exception ex)
            {
                var logger = LoggerManager.Logger<MemoryCacheHelper>();
                logger.Error(ex.Message, ex);

                return default;
            }

        }

        /// <summary>
        /// 删除缓存
        /// </summary>
        /// <param name="key"></param>
        public void Delete(object key)
        {
            if (key == null)
                return;

            _cache.Remove(key);
        }

        /// <summary>
        /// 是否存在
        /// </summary>
        /// <param name="key"></param>
        public bool Exists(object key)
        {
            if (key == null)
                return false;

            return _cache.TryGetValue(key, out _);
        }
    }
}
