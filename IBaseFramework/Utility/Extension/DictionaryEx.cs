using System.Collections.Concurrent;
using System.Collections.Generic;

namespace IBaseFramework.Utility.Extension
{
    public static class DictionaryEx
    {
        /// <summary>
        /// 新增或修改
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void AddOrUpdate<TKey,TValue>(this Dictionary<TKey, TValue> dic, TKey key, TValue value)
        {
            if (dic.ContainsKey(key))
            {
                dic[key] = value;
            }
            else
            {
                dic.Add(key, value);
            }
        }

        /// <summary>
        /// 新增或者累加修改
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void AddOrUpdateChange(this Dictionary<long, decimal> dic, long key, decimal value)
        {
            if (dic.ContainsKey(key))
            {
                var totalValue = dic[key] + value;
                dic[key] = totalValue;
            }
            else
            {
                dic.Add(key, value);
            }
        }

        /// <summary>
        /// 新增或者累加修改
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void AddOrUpdateChange(this Dictionary<long, int> dic, long key, int value)
        {
            if (dic.ContainsKey(key))
            {
                var totalValue = dic[key] + value;
                dic[key] = totalValue;
            }
            else
            {
                dic.Add(key, value);
            }
        }

        /// <summary>
        /// 新增或者累加修改
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void AddOrUpdateChange(this ConcurrentDictionary<long, int> dic, long key, int value)
        {
            dic.AddOrUpdate(key, value, (oldKey, oldValue) => oldValue + value);
        }

        /// <summary>
        /// 新增或者累加修改
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void AddOrUpdateChange(this ConcurrentDictionary<long, decimal> dic, long key, decimal value)
        {
            dic.AddOrUpdate(key, value, (oldKey, oldValue) => oldValue + value);
        }
    }
}