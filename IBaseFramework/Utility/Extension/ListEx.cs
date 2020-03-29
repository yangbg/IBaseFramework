using System;
using System.Collections.Generic;
using System.Linq;

namespace IBaseFramework.Utility.Extension
{
    public static class ListExtension
    {
        public static bool IsNullOrEmpty<T>(this ICollection<T> source)
        {
            return source == null || source.Count <= 0;
        }

        /// <summary>
        /// 遍历当前对象，并且调用方法进行处理
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="instance">实例</param>
        /// <param name="action">方法</param>
        /// <returns>当前集合</returns>
        public static void Each<T>(this IEnumerable<T> instance, Action<T> action)
        {
            foreach (var item in instance)
            {
                action(item);
            }
        }

        /// <summary>
        /// 比较数组相等
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">源列表</param>
        /// <param name="target">目标列表</param>
        /// <param name="allowRepeat">是否允许重复</param>
        /// <returns></returns>
        public static bool ArrayEquals<T>(this IEnumerable<T> source, IEnumerable<T> target, bool allowRepeat = false)
        {
            if (source == null || target == null)
            {
                return source == null && target == null;
            }
            if (allowRepeat)
            {
                source = source.Distinct();
                target = target.Distinct();
            }
            var sourceList = source as IList<T> ?? source.ToList();
            var targetList = target as IList<T> ?? target.ToList();
            return sourceList.Count() == targetList.Count() && sourceList.All(targetList.Contains);
        }

        #region Distinct扩展

        public static IEnumerable<T> Distinct<T, TV>(this IEnumerable<T> source, Func<T, TV> keySelector)
        {
            return source.Distinct(new CommonEqualityComparer<T, TV>(keySelector));
        }

        public static IEnumerable<T> Distinct<T, TV>(this IEnumerable<T> source, Func<T, TV> keySelector, IEqualityComparer<TV> comparer)
        {
            return source.Distinct(new CommonEqualityComparer<T, TV>(keySelector, comparer));
        }

        /// <summary>
        /// DistinctBy 过滤扩展
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            return source.Where(element => seenKeys.Add(keySelector(element)));
        }
        #endregion
    }

    public class CommonEqualityComparer<T, TV> : IEqualityComparer<T>
    {
        private readonly Func<T, TV> _keySelector;
        private readonly IEqualityComparer<TV> _comparer;

        public CommonEqualityComparer(Func<T, TV> keySelector, IEqualityComparer<TV> comparer)
        {
            _keySelector = keySelector;
            _comparer = comparer;
        }

        public CommonEqualityComparer(Func<T, TV> keySelector)
            : this(keySelector, EqualityComparer<TV>.Default)
        { }

        #region IEqualityComparer<T> 成员

        public bool Equals(T x, T y)
        {
            return _comparer.Equals(_keySelector(x), _keySelector(y));
        }

        public int GetHashCode(T obj)
        {
            return _comparer.GetHashCode(_keySelector(obj));
        }

        #endregion
    }
}
