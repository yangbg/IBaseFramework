using System;
using System.Text.RegularExpressions;

namespace IBaseFramework.Utility.Helper
{
    public static class ValidateHelper
    {
        #region 验证邮箱验证邮箱
        /// <summary>
        /// 验证邮箱
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsEmail(this string source)
        {
            return Regex.IsMatch(source, @"^[A-Za-z0-9](([_\.\-]?[a-zA-Z0-9]+)*)@([A-Za-z0-9]+)(([\.\-]?[a-zA-Z0-9]+)*)\.([A-Za-z]{2,})$", RegexOptions.IgnoreCase);
        }
        public static bool HasEmail(this string source)
        {
            return Regex.IsMatch(source, @"[A-Za-z0-9](([_\.\-]?[a-zA-Z0-9]+)*)@([A-Za-z0-9]+)(([\.\-]?[a-zA-Z0-9]+)*)\.([A-Za-z]{2,})", RegexOptions.IgnoreCase);
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
            return Regex.IsMatch(strUrl, @"^(((file|gopher|news|nntp|telnet|http|ftp|https|ftps|sftp)://)|(www\.))+(([a-zA-Z0-9\._-]+\.[a-zA-Z]{2,6})|([0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}))(/[a-zA-Z0-9\&amp;%_\./-~-]*)?$");
        }
        public static bool HasUrl(this string source)
        {
            return Regex.IsMatch(source, @"(((file|gopher|news|nntp|telnet|http|ftp|https|ftps|sftp)://)|(www\.))+(([a-zA-Z0-9\._-]+\.[a-zA-Z]{2,6})|([0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}))(/[a-zA-Z0-9\&amp;%_\./-~-]*)?", RegexOptions.IgnoreCase);
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
            return Regex.IsMatch(source, @"0?(13|14|15|17|18|19)[0-9]{9}", RegexOptions.IgnoreCase);
        }

        public static bool HasMobile(this string source)
        {
            return Regex.IsMatch(source, @"0?(13|14|15|17|18|19)[0-9]{9}", RegexOptions.IgnoreCase);
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
            return Regex.IsMatch(source, @"^(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9])\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])$", RegexOptions.IgnoreCase);
        }
        public static bool HasIp(this string source)
        {
            return Regex.IsMatch(source, @"(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9])\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])", RegexOptions.IgnoreCase);
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
            if (id.Length == 18)
            {
                var check = IsIdCard18(id);
                return check;
            }
            else if (id.Length == 15)
            {
                var check = IsIdCard15(id);
                return check;
            }
            else
            {
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
            long n = 0;
            if (long.TryParse(id.Remove(17), out n) == false || n < Math.Pow(10, 16) || long.TryParse(id.Replace('x', '0').Replace('X', '0'), out n) == false)
            {


                return false;//数字验证
            }
            var address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
            if (address.IndexOf(id.Remove

            (2)) == -1)
            {
                return false;//省份验证

            }
            var birth = id.Substring(6, 8).Insert(6, "-").Insert(4, "-");
            var time = new DateTime();
            if (DateTime.TryParse(birth, out time) == false)
            {
                return false;//生日验证
            }
            var

            arrVarifyCode = ("1,0,x,9,8,7,6,5,4,3,2").Split(',');
            var wi = ("7,9,10,5,8,4,2,1,6,3,7,9,10,5,8,4,2").Split(',');
            var ai = id.Remove(17).ToCharArray();
            var sum = 0;
            for (var

            i = 0; i < 17; i++)
            {
                sum += int.Parse(wi[i]) * int.Parse(ai[i].ToString());
            }
            var y = -1;
            Math.DivRem(sum, 11, out y);
            if (arrVarifyCode[y] != id.Substring(17, 1).ToLower())
            {
                return

                false;//校验码验证
            }
            return true;//符合GB11643-1999标准
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
            long n = 0;
            if (long.TryParse(id, out n) == false || n < Math.Pow(10, 14))
            {


                return false;//数字验证
            }
            var address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
            if (address.IndexOf(id.Remove

            (2)) == -1)
            {
                return false;//省份验证
            }
            var birth = id.Substring(6, 6).Insert(4, "-").Insert(2, "-");
            var time = new DateTime();
            if (DateTime.TryParse(birth, out time) == false)
            {


                return false;//生日验证
            }
            return true;//符合15位身份证标准
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
            if ((length <= begin) && (length >= end))
            {
                return false;
            }
            return true;
        }
        #endregion

        #region 验证中国座机电话，格式010-85849685
        /// <summary>
        /// 验证中国座机电话，格式010-85849685
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsTel(this string source)
        {
            return Regex.IsMatch(source, @"^\d{3,4}-?\d{6,8}$", RegexOptions.IgnoreCase);
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
            return Regex.IsMatch(source, @"^\d{6}$", RegexOptions.IgnoreCase);
        }
        #endregion

        #region 验证中国座机或者手机号码
        /// <summary>
        /// 验证中国座机电话，格式010-85849685
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsTelPhone(this string source)
        {
            return Regex.IsMatch(source, @"^((0\d{2,3}\d{7,8})|(0\d{2,3}-\d{7,8})|(1[34578]\d{9}))$", RegexOptions.IgnoreCase);
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
            return Regex.IsMatch(source, @"^[\u4e00-\u9fa5]+$", RegexOptions.IgnoreCase);
        }
        public static bool HasChinese(this string source)
        {
            return Regex.IsMatch(source, @"[\u4e00-\u9fa5]+", RegexOptions.IgnoreCase);
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
            return Regex.IsMatch(source, @"[\w\d_]+", RegexOptions.IgnoreCase);
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

            //模式字符串
            var pattern = @"^(-?\d+)(\.\d+)?$";

            //验证
            return Regex.IsMatch(floatNum, pattern, RegexOptions.IgnoreCase);
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

            //模式字符串
            var pattern = @"^[0-9]+[0-9]*$";

            //验证
            return Regex.IsMatch(number, pattern, RegexOptions.IgnoreCase);
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

            //模式字符串
            var pattern = @"^[0-9]+[0-9]*[.]?[0-9]*$";

            //验证
            return Regex.IsMatch(number, pattern, RegexOptions.IgnoreCase);
        }
        #endregion

        #region 验证日期
        /// <summary>
        /// 判断用户输入是否为日期
        /// </summary>
        /// <param name="strln"></param>
        /// <returns></returns>
        /// <remarks>
        /// 可判断格式如下（其中-可替换为/，不影响验证)
        /// YYYY | YYYY-MM | YYYY-MM-DD | YYYY-MM-DD HH:MM:SS | YYYY-MM-DD HH:MM:SS.FFF
        /// </remarks>
        public static bool IsDateTime(this string strln)
        {
            if (string.IsNullOrEmpty(strln))
                return false;
            if (null == strln)
            {
                return false;
            }
            var regexDate = @"[1-2]{1}[0-9]{3}((-|\/|\.){1}(([0]?[1-9]{1})|(1[0-2]{1}))((-|\/|\.){1}((([0]?[1-9]{1})|([1-2]{1}[0-9]{1})|(3[0-1]{1})))( (([0-1]{1}[0-9]{1})|2[0-3]{1}):([0-5]{1}[0-9]{1}):([0-5]{1}[0-9]{1})(\.[0-9]{3})?)?)?)?$";
            if (Regex.IsMatch(strln, regexDate))
            {
                //以下各月份日期验证，保证验证的完整性
                var indexY = -1;
                var indexM = -1;
                var indexD = -1;
                if (-1 != (indexY = strln.IndexOf("-")))
                {
                    indexM = strln.IndexOf("-", indexY + 1);
                    indexD = strln.IndexOf(":");
                }
                else
                {
                    indexY = strln.IndexOf("/");
                    indexM = strln.IndexOf("/", indexY + 1);
                    indexD = strln.IndexOf(":");
                }
                //不包含日期部分，直接返回true
                if (-1 == indexM)
                    return true;
                if (-1 == indexD)
                {
                    indexD = strln.Length + 3;
                }
                var iYear = Convert.ToInt32(strln.Substring(0, indexY));
                var iMonth = Convert.ToInt32(strln.Substring(indexY + 1, indexM - indexY - 1));
                var iDate = Convert.ToInt32(strln.Substring(indexM + 1, indexD - indexM - 4));
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
            }
            return false;
        }
        #endregion

        #region 验证对象是否为空
        /// <summary>
        /// 验证对象是否为空，为空返回true
        /// </summary>
        /// <typeparam name="T">要验证的对象的类型</typeparam>
        /// <param name="data">要验证的对象</param>        
        public static bool IsNullOrEmpty<T>(this T data)
        {
            //如果为null
            if (data == null)
            {
                return true;
            }

            //如果为""
            if (data.GetType() == typeof(String))
            {
                if (string.IsNullOrEmpty(data.ToString().Trim()) || data.ToString() == "")
                {
                    return true;
                }
            }

            //如果为DBNull
            if (data.GetType() == typeof(DBNull))
            {
                return true;
            }

            //不为空
            return false;
        }

        /// <summary>
        /// 判断对象是否为空，为空返回true
        /// </summary>
        /// <param name="data">要验证的对象</param>
        public static bool IsNullOrEmpty(this object data)
        {
            //如果为null
            if (data == null)
            {
                return true;
            }

            //如果为""
            if (data.GetType() == typeof(String))
            {
                if (string.IsNullOrEmpty(data.ToString().Trim()))
                {
                    return true;
                }
            }

            //如果为DBNull
            if (data.GetType() == typeof(DBNull))
            {
                return true;
            }

            //不为空
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
            return Regex.IsMatch(str, @"[A-Za-z0-9\+\/\=]");
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
            string strBadChar, tempChar;
            string[] arrBadChar;
            strBadChar = "@@,+,',--,%,^,&,?,(,), <,>,[,],{,},/,\\,;,:,\",\"\"";
            arrBadChar = strBadChar.Split(',');
            tempChar = str;
            for (var i = 0; i < arrBadChar.Length; i++)
            {
                if (tempChar.IndexOf(arrBadChar[i]) >= 0)
                    result = true;
            }
            return result;
        }
        #endregion

        #region 验证是否是有效传真号码
        /// <summary>
        /// 验证是否是有效传真号码
        /// </summary>
        /// <param name="strln">输入的字符</param>
        /// <returns></returns>
        public static bool IsFax(this string strln)
        {
            if (string.IsNullOrEmpty(strln))
                return false;
            return Regex.IsMatch(strln, @"^[+]{0,1}(\d){1,3}[ ]?([-]?((\d)|[ ]){1,12})+$");
        }
        #endregion

        #region 验证是否只含有汉字
        /// <summary>
        /// 验证是否只含有汉字
        /// </summary>
        /// <param name="strln">输入的字符</param>
        /// <returns></returns>
        public static bool IsOnllyChinese(this string strln)
        {
            if (string.IsNullOrEmpty(strln))
                return false;
            return Regex.IsMatch(strln, @"^[\u4e00-\u9fa5]+$");
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
            if (string.IsNullOrEmpty(str))
                return false;
            return Regex.IsMatch(str, @"^\d{16,19}$");
        }
        #endregion

        #region 检测是否是经度
        /// <summary>
        /// 检测是否是经度
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsLng(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return false;
            return Regex.IsMatch(str, @"^[\-\+]?(0?\d{1,2}\.\d{1,8}|1[0-7]?\d{1}\.\d{1,8}|180\.0{1,8})$");
        }
        /// <summary>
        /// 检测是否是纬度
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsLat(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return false;
            return Regex.IsMatch(str, @"^[\-\+]?([0-8]?\d{1}\.\d{1,8}|90\.0{1,8})$");
        }

        #endregion
    }
}
