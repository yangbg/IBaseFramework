using System;
using IBaseFramework.Utility.Helper;

namespace IBaseFramework.Utility.Extension
{
    public static class ConvertEx
    {
        private static readonly DateTime DefaultTime = DateTime.Parse("1900-01-01");

        #region ToFloat
        /// <summary>
        /// string转换为float
        /// </summary>
        /// <param name="strValue"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static float ToFloat(this string strValue, float defaultValue)
        {
            if ((strValue == null) || (strValue.Length > 10))
                return defaultValue;

            var intValue = defaultValue;
            var isFloat = strValue.IsFloat();
            if (isFloat)
                float.TryParse(strValue, out intValue);
            return intValue;
        }

        /// <summary>
        /// object转化为float
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static float ToFloat(this object obj, float defaultValue)
        {
            if (obj == null)
                return defaultValue;
            return ToFloat(obj.ToString(), defaultValue);
        }

        #endregion

        #region ToInt
        /// <summary>
        /// string转化为int
        /// </summary>
        /// <param name="str">字符</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns></returns>
        public static int ToInt(this string str, int defaultValue)
        {
            if (string.IsNullOrEmpty(str) || str.Trim().Length >= 11 ||
                !str.IsFloat())
                return defaultValue;

            if (int.TryParse(str, out var rv))
                return rv;

            return Convert.ToInt32(ToFloat(str, defaultValue));
        }

        /// <summary>
        /// object转化为int
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static int ToInt(this object obj, int defaultValue)
        {
            if (obj == null)
                return defaultValue;
            return ToInt(obj.ToString(), defaultValue);
        }
        #endregion

        #region ToLong

        /// <summary>
        /// string型转换为long型
        /// </summary>
        /// <param name="strValue"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static long ToLong(this string strValue, long defaultValue)
        {
            if (strValue == null) return defaultValue;

            if (long.TryParse(strValue, out var num))
            {
                return num;
            }
            else
            {
                if (decimal.TryParse(strValue, out var dem))
                {
                    return Convert.ToInt64(dem);
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// object转化为int
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static long ToLong(this object obj, long defaultValue)
        {
            if (obj == null)
                return defaultValue;
            return ToLong(obj.ToString(), defaultValue);
        }

        #endregion

        #region ToDecimal
        public static decimal ToDecimal(this string c, decimal defaultValue)
        {
            return (decimal)c.ToFloat((float)defaultValue);
        }

        public static decimal ToDecimal(this object c, decimal defaultValue)
        {
            return (decimal)c.ToFloat((float)defaultValue);
        }
        #endregion

        #region ToDateTime
        /// <summary>
        /// 将对象转换为日期时间类型
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this string str, DateTime defaultValue)
        {
            if (string.IsNullOrEmpty(str)) return defaultValue;

            if (DateTime.TryParse(str, out var dateTime))
                return dateTime;

            return defaultValue;
        }

        /// <summary>
        /// 将对象转换为日期时间类型
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this object obj, DateTime defaultValue)
        {
            if (obj == null) return defaultValue;
            return ToDateTime(obj.ToString(), defaultValue);
        }

        /// <summary>
        /// 将对象转换为日期时间类型
        /// </summary>
        /// <param name="str">要转换的字符串</param>
        /// <returns>转换后的int类型结果</returns>
        public static DateTime ToDateTime(this string str)
        {
            return ToDateTime(str, DefaultTime);
        }
        #endregion

        #region UtcDateTimeEx

        private static readonly long StartTicks = new DateTime(1970, 1, 1).Ticks;

        /// <summary>
        /// 获取距离 1970-01-01（格林威治时间）的秒数
        /// </summary>
        /// <param name="localTime"></param>
        /// <returns></returns>
        public static long ToUtcSeconds(this DateTime localTime)
        {
            return (localTime.ToUniversalTime().Ticks - StartTicks) / 10000000;
        }

        /// <summary>
        /// 距离 1970-01-01（格林威治时间）的秒数转换为当前时间
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static DateTime FromUtcSeconds(this long seconds)
        {
            return DateTimeOffset.FromUnixTimeSeconds(seconds).LocalDateTime;// new DateTime(1970, 1, 1).AddSeconds(seconds).ToLocalTime();
        }

        /// <summary>
        /// 获取距离 1970-01-01（格林威治时间）的毫秒数
        /// </summary>
        /// <param name="localTime"></param>
        /// <returns></returns>
        public static long ToUtcMillSeconds(this DateTime localTime)
        {
            return (localTime.ToUniversalTime().Ticks - StartTicks) / 10000;
        }

        /// <summary>
        /// 距离 1970-01-01（格林威治时间）的秒数转换为当前时间
        /// </summary>
        /// <param name="millSeconds"></param>
        /// <returns></returns>
        public static DateTime FromUtcMillSeconds(this long millSeconds)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(millSeconds).LocalDateTime;
        }

        /// <summary>
        /// 获取距离 1970-01-01（格林威治时间）Ticks  精确到0.1微秒（千万分之一秒）
        /// </summary>
        /// <param name="localTime"></param>
        /// <returns></returns>
        public static long ToUtcTicks(this DateTime localTime)
        {
            return localTime.ToUniversalTime().Ticks - StartTicks;
        }

        /// <summary>
        /// 获取距离 1970-01-01（本地/北京时间）的秒数
        /// </summary>
        /// <param name="localTime"></param>
        /// <returns></returns>
        public static long ToLocalSeconds(this DateTime localTime)
        {
            return (localTime.Ticks - StartTicks) / 10000000;
        }

        /// <summary>
        /// 距离 1970-01-01（本地/北京时间）的秒数转换为当前时间
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static DateTime FromLocalSeconds(this long seconds)
        {
            return new DateTime(1970, 1, 1).AddSeconds(seconds);
        } 

        #endregion

    }
}
