using System;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace IBaseFramework.Extension
{
    public static class StringAndConvertEx
    {
        #region ToFloat
        /// <summary>
        /// string转换为float
        /// </summary>
        /// <param name="strValue"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static float ToFloat(this string strValue, float defaultValue)
        {
            if (strValue.IsNullOrEmpty())
                return defaultValue;

            return float.TryParse(strValue, out var result) ? result : defaultValue;
        }

        /// <summary>
        /// object转化为float
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static float ToFloat(this object obj, float defaultValue)
        {
            return obj == null ? defaultValue : ToFloat(obj.ToString(), defaultValue);
        }

        #endregion

        #region ToInt
        /// <summary>
        /// string转化为int
        /// </summary>
        /// <param name="strValue">字符</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns></returns>
        public static int ToInt(this string strValue, int defaultValue)
        {
            if (strValue.IsNullOrEmpty())
                return defaultValue;

            return int.TryParse(strValue, out var result) ? result : defaultValue;
        }

        /// <summary>
        /// object转化为int
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static int ToInt(this object obj, int defaultValue)
        {
            return obj == null ? defaultValue : ToInt(obj.ToString(), defaultValue);
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
            if (strValue.IsNullOrEmpty())
                return defaultValue;

            return long.TryParse(strValue, out var result) ? result : defaultValue;
        }

        /// <summary>
        /// object转化为int
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static long ToLong(this object obj, long defaultValue)
        {
            return obj == null ? defaultValue : ToLong(obj.ToString(), defaultValue);
        }

        #endregion

        #region ToDecimal
        public static decimal ToDecimal(this string strValue, decimal defaultValue)
        {
            if (strValue.IsNullOrEmpty())
                return defaultValue;

            return decimal.TryParse(strValue, out var result) ? result : defaultValue;
        }

        public static decimal ToDecimal(this object obj, decimal defaultValue)
        {
            return obj == null ? defaultValue : ToDecimal(obj.ToString(), defaultValue);
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

            return DateTime.TryParse(str, out var dateTime) ? dateTime : defaultValue;
        }

        /// <summary>
        /// 将对象转换为日期时间类型
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this object obj, DateTime defaultValue)
        {
            return obj == null ? defaultValue : ToDateTime(obj.ToString(), defaultValue);
        }

        private static readonly DateTime DefaultTime = DateTime.Parse("1900-01-01");

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


        #region IsNullOrEmpty
        public static bool IsNullOrEmpty(this string strValue)
        {
            return string.IsNullOrEmpty(strValue);
        }
        #endregion

        #region Encode/Decode

        /// <summary>
        ///     Converts a string to an HTML-encoded string.
        /// </summary>
        /// <param name="s">The string to encode.</param>
        /// <returns>An encoded string.</returns>
        public static string HtmlEncode([NotNull] this string s)
        {
            return HttpUtility.HtmlEncode(s);
        }

        /// <summary>
        ///     Converts a string that has been HTML-encoded for HTTP transmission into a decoded string.
        /// </summary>
        /// <param name="s">The string to decode.</param>
        /// <returns>A decoded string.</returns>
        public static string HtmlDecode([NotNull] this string s)
        {
            return HttpUtility.HtmlDecode(s);
        }

        /// <summary>
        ///     Encodes a string.
        /// </summary>
        /// <param name="value">A string to encode.</param>
        /// <returns>An encoded string.</returns>
        public static string JavaScriptStringEncode([NotNull] this string value)
        {
            return HttpUtility.JavaScriptStringEncode(value);
        }

        /// <summary>
        ///     Encodes a string.
        /// </summary>
        /// <param name="value">A string to encode.</param>
        /// <param name="addDoubleQuotes">
        ///     A value that indicates whether double quotation marks will be included around the
        ///     encoded string.
        /// </param>
        /// <returns>An encoded string.</returns>
        public static string JavaScriptStringEncode([NotNull] this string value, bool addDoubleQuotes)
        {
            return HttpUtility.JavaScriptStringEncode(value, addDoubleQuotes);
        }

        /// <summary>
        ///     Parses a query string into a  using  encoding.
        /// </summary>
        /// <param name="query">The query string to parse.</param>
        /// <returns>A  of query parameters and values.</returns>
        public static NameValueCollection ParseQueryString([NotNull] this string query)
        {
            return HttpUtility.ParseQueryString(query);
        }

        /// <summary>
        ///     Parses a query string into a  using the specified .
        /// </summary>
        /// <param name="query">The query string to parse.</param>
        /// <param name="encoding">The  to use.</param>
        /// <returns>A  of query parameters and values.</returns>
        public static NameValueCollection ParseQueryString([NotNull] this string query, Encoding encoding)
        {
            return HttpUtility.ParseQueryString(query, encoding);
        }

        /// <summary>
        ///     Converts a string that has been encoded for transmission in a URL into a decoded string.
        /// </summary>
        /// <param name="str">The string to decode.</param>
        /// <returns>A decoded string.</returns>
        public static string UrlDecode([NotNull] this string str)
        {
            return HttpUtility.UrlDecode(str);
        }

        /// <summary>
        ///     Converts a URL-encoded string into a decoded string, using the specified encoding object.
        /// </summary>
        /// <param name="str">The string to decode.</param>
        /// <param name="e">The  that specifies the decoding scheme.</param>
        /// <returns>A decoded string.</returns>
        public static string UrlDecode([NotNull] this string str, Encoding e)
        {
            return HttpUtility.UrlDecode(str, e);
        }

        /// <summary>
        ///     Encodes a URL string.
        /// </summary>
        /// <param name="str">The text to encode.</param>
        /// <returns>An encoded string.</returns>
        public static string UrlEncode([NotNull] this string str)
        {
            return HttpUtility.UrlEncode(str);
        }

        /// <summary>
        ///     Encodes a URL string using the specified encoding object.
        /// </summary>
        /// <param name="str">The text to encode.</param>
        /// <param name="e">The  object that specifies the encoding scheme.</param>
        /// <returns>An encoded string.</returns>
        public static string UrlEncode([NotNull] this string str, Encoding e)
        {
            return HttpUtility.UrlEncode(str, e);
        }

        /// <summary>
        /// Base64Encode with utf8 encoding
        /// </summary>
        /// <param name="str">source string</param>
        /// <returns>base64 encoded string</returns>
        public static string Base64Encode([NotNull] this string str) => Base64Encode(str, Encoding.UTF8);

        /// <summary>
        /// Base64Encode
        /// </summary>
        /// <param name="str">source string</param>
        /// <param name="encoding">encoding</param>
        /// <returns>base64 encoded string</returns>
        public static string Base64Encode([NotNull] this string str, Encoding encoding) => Convert.ToBase64String(encoding.GetBytes(str));

        /// <summary>
        /// Base64Decode with ytf8 encoding
        /// </summary>
        /// <param name="str">base64 encoded source string</param>
        /// <returns>base64 decoded string</returns>
        public static string Base64Decode([NotNull] this string str) => Base64Decode(str, Encoding.UTF8);

        /// <summary>
        /// Base64Decode
        /// </summary>
        /// <param name="str">base64 encoded source string</param>
        /// <param name="encoding">encoding</param>
        /// <returns>base64 decoded string</returns>
        public static string Base64Decode([NotNull] this string str, Encoding encoding) => encoding.GetString(Convert.FromBase64String(str));

        #endregion Encode/Decode


        #region 验证邮箱验证邮箱
        /// <summary>
        /// 验证邮箱
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsEmail(this string source)
        {
            return Regex.IsMatch(source, RegexConst.RegexEmail, RegexOptions.IgnoreCase);
        }
        #endregion

        #region 检测是否是正确的Url
        /// <summary>
        /// 检测是否是正确的Url
        /// </summary>
        /// <param name="strUrl">要验证的Url</param>
        /// <returns>判断结果</returns>
        public static bool IsUrl(this string strUrl)
        {
            return Regex.IsMatch(strUrl, RegexConst.RegexUrl);
        }
        #endregion

        #region 验证手机号
        /// <summary>
        /// 验证手机号
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsMobile(this string source)
        {
            return Regex.IsMatch(source, RegexConst.RegexMobile, RegexOptions.IgnoreCase);
        }
        #endregion

        #region 验证IP
        /// <summary>
        /// 验证IP
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsIp(this string source)
        {
            return Regex.IsMatch(source, RegexConst.RegexIp, RegexOptions.IgnoreCase);
        }
        #endregion

        #region 验证身份证是否有效
        /// <summary>
        /// 验证身份证是否有效
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsIdCard(this string id)
        {
            switch (id.Length)
            {
                case 18:
                    {
                        var check = IsIdCard18(id);
                        return check;
                    }
                case 15:
                    {
                        var check = IsIdCard15(id);
                        return check;
                    }
                default:
                    return false;
            }
        }

        #region 18位
        /// <summary>
        /// 18位
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsIdCard18(string id)
        {
            if (long.TryParse(id.Remove(17), out var n) == false || n < Math.Pow(10, 16) || long.TryParse(id.Replace('x', '0').Replace('X', '0'), out n) == false)
                return false; //数字验证

            const string address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
            if (address.IndexOf(id.Remove(2), StringComparison.Ordinal) == -1)
                return false; //省份验证

            var birth = id.Substring(6, 8).Insert(6, "-").Insert(4, "-");
            if (DateTime.TryParse(birth, out _) == false)
                return false; //生日验证

            var arrArrifyCode = ("1,0,x,9,8,7,6,5,4,3,2").Split(',');
            var wi = ("7,9,10,5,8,4,2,1,6,3,7,9,10,5,8,4,2").Split(',');
            var ai = id.Remove(17).ToCharArray();
            var sum = 0;
            for (var i = 0; i < 17; i++)
            {
                sum += int.Parse(wi[i]) * int.Parse(ai[i].ToString());
            }

            Math.DivRem(sum, 11, out var y);
            return arrArrifyCode[y] == id.Substring(17, 1).ToLower();
        }
        #endregion

        #region 15位
        /// <summary>
        /// 15位
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsIdCard15(string id)
        {
            if (long.TryParse(id, out var n) == false || n < Math.Pow(10, 14))
                return false; //数字验证

            const string address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
            if (address.IndexOf(id.Remove(2), StringComparison.Ordinal) == -1)
                return false; //省份验证

            var birth = id.Substring(6, 6).Insert(4, "-").Insert(2, "-");
            return DateTime.TryParse(birth, out _);
        }
        #endregion

        #endregion

        #region 验证字符串的长度是不是在限定数之间 一个中文为两个字符
        /// <summary>
        /// 验证字符串的长度是不是在限定数之间 一个中文为两个字符
        /// </summary>
        /// <param name="source">字符串</param>
        /// <param name="begin">大于等于</param>
        /// <param name="end">小于等于</param>
        /// <returns></returns>
        public static bool IsLengthStr(this string source, int begin, int end)
        {
            var length = Regex.Replace(source, @"[^\x00-\xff]", "OK").Length;
            return length > begin || length < end;
        }
        #endregion

        #region 验证邮政编码 6个数字
        /// <summary>
        /// 验证邮政编码 6个数字
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsPostCode(this string source)
        {
            return Regex.IsMatch(source, RegexConst.RegexPostCode, RegexOptions.IgnoreCase);
        }
        #endregion

        #region 验证中国座机
        /// <summary>
        /// 验证中国座机电话，格式010-85849685
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsTel(this string source)
        {
            return Regex.IsMatch(source, RegexConst.RegexTel, RegexOptions.IgnoreCase);
        }
        #endregion

        #region 验证中文
        /// <summary>
        /// 验证中文
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsChinese(this string source)
        {
            return Regex.IsMatch(source, RegexConst.RegexChinese, RegexOptions.IgnoreCase);
        }
        #endregion

        #region 验证正常字符 字母，数字，下划线的组合
        /// <summary>
        /// 验证正常字符 字母，数字，下划线的组合
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsNormalChar(this string source)
        {
            return Regex.IsMatch(source, RegexConst.RegexNormalStr, RegexOptions.IgnoreCase);
        }
        #endregion

        #region 验证浮点数
        /// <summary>
        /// 验证浮点数
        /// </summary>
        /// <param name="floatNum"></param>
        /// <returns></returns>
        public static bool IsFloat(this string floatNum)
        {
            //如果为空，认为验证不合格
            if (IsNullOrEmpty(floatNum))
            {
                return false;
            }
            //清除要验证字符串中的空格
            floatNum = floatNum.Trim();

            //验证
            return Regex.IsMatch(floatNum, RegexConst.RegexFloat, RegexOptions.IgnoreCase);
        }
        #endregion

        #region 验证整数
        /// <summary>
        /// 验证整数 如果为空，认为验证不合格 返回false
        /// </summary>
        /// <param name="number">要验证的整数</param>
        public static bool IsInt(this string number)
        {
            //如果为空，认为验证不合格
            if (IsNullOrEmpty(number))
            {
                return false;
            }

            //清除要验证字符串中的空格
            number = number.Trim();

            //验证
            return Regex.IsMatch(number, RegexConst.RegexInt, RegexOptions.IgnoreCase);
        }
        #endregion

        #region 验证数字
        /// <summary>
        /// 验证数字
        /// </summary>
        /// <param name="number">要验证的数字</param>        
        public static bool IsNumber(this string number)
        {
            //如果为空，认为验证不合格
            if (IsNullOrEmpty(number))
            {
                return false;
            }

            //清除要验证字符串中的空格
            number = number.Trim();
            
            //验证
            return Regex.IsMatch(number, RegexConst.RegexNumber, RegexOptions.IgnoreCase);
        }
        #endregion

        #region 验证日期
        /// <summary>
        /// 判断用户输入是否为日期
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        /// <remarks>
        /// 可判断格式如下（其中-可替换为/，不影响验证)
        /// YYYY | YYYY-MM | YYYY-MM-DD | YYYY-MM-DD HH:MM:SS | YYYY-MM-DD HH:MM:SS.FFF
        /// </remarks>
        public static bool IsDateTime(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return false;

            if (!Regex.IsMatch(str, RegexConst.RegexDate)) return false;

            //以下各月份日期验证，保证验证的完整性
            int indexY;
            int indexM;
            int indexD;
            if (-1 != (indexY = str.IndexOf("-", StringComparison.Ordinal)))
            {
                indexM = str.IndexOf("-", indexY + 1, StringComparison.Ordinal);
                indexD = str.IndexOf(":", StringComparison.Ordinal);
            }
            else
            {
                indexY = str.IndexOf("/", StringComparison.Ordinal);
                indexM = str.IndexOf("/", indexY + 1, StringComparison.Ordinal);
                indexD = str.IndexOf(":", StringComparison.Ordinal);
            }
            //不包含日期部分，直接返回true
            if (-1 == indexM)
                return true;
            if (-1 == indexD)
            {
                indexD = str.Length + 3;
            }
            var iYear = Convert.ToInt32(str.Substring(0, indexY));
            var iMonth = Convert.ToInt32(str.Substring(indexY + 1, indexM - indexY - 1));
            var iDate = Convert.ToInt32(str.Substring(indexM + 1, indexD - indexM - 4));
            //判断月份日期
            if ((iMonth < 8 && 1 == iMonth % 2) || (iMonth > 8 && 0 == iMonth % 2))
            {
                if (iDate < 32)
                    return true;
            }
            else
            {
                if (iMonth != 2)
                {
                    if (iDate < 31)
                        return true;
                }
                else
                {
                    //闰年
                    if ((0 == iYear % 400) || (0 == iYear % 4 && 0 < iYear % 100))
                    {
                        if (iDate < 30)
                            return true;
                    }
                    else
                    {
                        if (iDate < 29)
                            return true;
                    }
                }
            }
            return false;
        }
        #endregion

        #region 验证base64字符串
        /// <summary>
        /// 验证base64字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsBase64String(this string str)
        {
            //A-Z, a-z, 0-9, +, /, =
            return Regex.IsMatch(str, RegexConst.RegexBase64Str);
        }
        #endregion

        #region 验证是否有Sql危险字符
        /// <summary>
        /// 验证是否有Sql危险字符
        /// </summary>
        /// <param name="str">要判断字符串</param>
        /// <returns>判断结果</returns>
        public static bool IsSafeSqlString(this string str)
        {
            return !(Regex.IsMatch(str, @"[- | |, |// |/( |/) |/[ |/] |/} |/{ |% |@ |/* |! |/']", RegexOptions.IgnoreCase) || Regex.IsMatch(str, @"/s+exec(/s |/+)+(s |x)p/w+", RegexOptions.IgnoreCase));
        }
        #endregion

        #region 验证是否含有非法字符ChkBadChar
        /// <summary> 
        /// 验证是否含有非法字符 
        /// </summary> 
        /// <param name="str">要检查的字符串 </param> 
        /// <returns> </returns> 
        public static bool HasBadChar(this string str)
        {
            var result = false;
            if (string.IsNullOrEmpty(str))
                return true;
            const string strBadChar = "@@,+,',--,%,^,&,?,(,), <,>,[,],{,},/,\\,;,:,\",\"\"";
            var arrBadChar = strBadChar.Split(',');
            var tempChar = str;
            foreach (var t in arrBadChar)
            {
                if (tempChar.IndexOf(t, StringComparison.Ordinal) >= 0)
                    result = true;
            }
            return result;
        }
        #endregion

        #region 验证是否是有效传真号码
        /// <summary>
        /// 验证是否是有效传真号码
        /// </summary>
        /// <param name="str">输入的字符</param>
        /// <returns></returns>
        public static bool IsFax(this string str)
        {
            return !string.IsNullOrEmpty(str) && Regex.IsMatch(str, RegexConst.RegexFat);
        }
        #endregion
        
        #region 检测是否是银行卡号
        /// <summary>
        /// 检测是否是银行卡号
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsBankCardCode(this string str)
        {
            return !string.IsNullOrEmpty(str) && Regex.IsMatch(str, RegexConst.RegexBankCard);
        }
        #endregion

        #region 检测是否是经纬度
        /// <summary>
        /// 检测是否是经度
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsLng(this string str)
        {
            return !string.IsNullOrEmpty(str) && Regex.IsMatch(str, RegexConst.RegexLng);
        }

        /// <summary>
        /// 检测是否是纬度
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsLat(this string str)
        {
            return !string.IsNullOrEmpty(str) && Regex.IsMatch(str, RegexConst.RegexLat);
        }

        #endregion
    }
}
