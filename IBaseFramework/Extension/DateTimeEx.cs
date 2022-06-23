using System;

namespace IBaseFramework.Extension
{
    public static class DateTimeEx
    {
        /// <summary>
        /// 获取日期差
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static string ToFormatText(this DateTime datetime)
        {
            string result;

            var currentSecond = (long)(DateTime.Now - datetime).TotalSeconds;

            long minSecond = 60;                //60s = 1min  
            var hourSecond = minSecond * 60;   //60*60s = 1 hour  
            var daySecond = hourSecond * 24;   //60*60*24s = 1 day  
            var weekSecond = daySecond * 7;    //60*60*24*7s = 1 week  
            var monthSecond = daySecond * 30;  //60*60*24*30s = 1 month  
            var yearSecond = daySecond * 365;  //60*60*24*365s = 1 year  

            if (currentSecond >= yearSecond)
            {
                var year = (int)(currentSecond / yearSecond);
                result = $"{year}年前";
            }
            else if (currentSecond < yearSecond && currentSecond >= monthSecond)
            {
                var month = (int)(currentSecond / monthSecond);
                result = $"{month}个月前";
            }
            else if (currentSecond < monthSecond && currentSecond >= weekSecond)
            {
                var week = (int)(currentSecond / weekSecond);
                result = $"{week}周前";
            }
            else if (currentSecond < weekSecond && currentSecond >= daySecond)
            {
                var day = (int)(currentSecond / daySecond);
                result = $"{day}天前";
            }
            else if (currentSecond < daySecond && currentSecond >= hourSecond)
            {
                var hour = (int)(currentSecond / hourSecond);
                result = $"{hour}小时前";
            }
            else if (currentSecond < hourSecond && currentSecond >= minSecond)
            {
                var min = (int)(currentSecond / minSecond);
                result = $"{min}分钟前";
            }
            else if (currentSecond < minSecond && currentSecond >= 0)
            {
                result = "刚刚";
            }
            else
            {
                result = datetime.ToString("yyyy/MM/dd HH:mm:ss");
            }
            return result;
        }

        /// <summary>
        /// 返回"yyyy-MM-dd HH:mm:ss.fff"格式的字符串
        /// </summary>
        /// <returns></returns>
        public static string ToCommonString(this DateTime value)
        {
            return value.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }

        /// <summary>
        /// 返回"yyyy-MM-dd"格式的字符串
        /// </summary>
        /// <returns></returns>
        public static string ToCommonDateString(this DateTime value)
        {
            return value.ToString("yyyy-MM-dd");
        }

        /// <summary>
        /// 返回"yyyy-MM-dd HH:mm:ss"格式的字符串
        /// </summary>
        /// <returns></returns>
        public static string ToCommonDateTimeString(this DateTime value)
        {
            return value.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
