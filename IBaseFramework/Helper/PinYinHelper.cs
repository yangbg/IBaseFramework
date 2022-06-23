using System.Linq;
using System.Text;
using IBaseFramework.Helper.ChineseData;

namespace IBaseFramework.Helper
{
    /// <summary>
    /// 汉字转拼音帮助类
    /// </summary>
    public class PinYinHelper
    {
        /// <summary>
        /// 获取文本的拼音
        /// </summary>
        /// <param name="inputStr">要获取拼音的文本</param>
        /// <param name="separator">单个拼音分隔符</param>
        /// <returns></returns>
        public static string GetPinyin(string inputStr, string separator = " ")
        {
            if (string.IsNullOrEmpty(inputStr))
                return inputStr;

            // 没有提供字典或选择器，按单字符转换输出
            var builder1 = new StringBuilder();
            for (var i = 0; i < inputStr.Length; i++)
            {
                builder1.Append(GetPinyin(inputStr[i]));
                if (i != inputStr.Length - 1)
                {
                    builder1.Append(separator);
                }
            }

            return builder1.ToString();
        }

        /// <summary>
        /// 获取拼音首字母
        /// </summary>
        /// <param name="inputStr"></param>
        /// <returns></returns>
        public static string GetPinyinInitials(string inputStr)
        {
            var result = GetPinyin(inputStr, "|");
            return string.Join("", result.Split('|').Select(x => x.Substring(0, 1)).ToArray());
        }

        private static int GetPinyinCode(char c)
        {
            var offset = c - PinyinData.MinValue;
            if (0 <= offset && offset < PinyinData.PinyinCode1Offset)
            {
                return DecodeIndex(PinyinCode1.PinyinCodePadding, PinyinCode1.PinyinCode, offset);
            }
            else if (PinyinData.PinyinCode1Offset <= offset
                  && offset < PinyinData.PinyinCode2Offset)
            {
                return DecodeIndex(PinyinCode2.PinyinCodePadding, PinyinCode2.PinyinCode,
                        offset - PinyinData.PinyinCode1Offset);
            }
            else
            {
                return DecodeIndex(PinyinCode3.PinyinCodePadding, PinyinCode3.PinyinCode,
                        offset - PinyinData.PinyinCode2Offset);
            }
        }

        private static short DecodeIndex(byte[] paddings, byte[] indexes, int offset)
        {
            var index1 = offset / 8;
            var index2 = offset % 8;
            var realIndex = (short)(indexes[offset] & 0xff);
            if ((paddings[index1] & PinyinData.BitMasks[index2]) != 0)
            {
                realIndex = (short)(realIndex | PinyinData.PaddingMask);
            }
            return realIndex;
        }

        /// <summary>
        /// 判断给定字符是否是中文
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private static bool IsChinese(char c)
        {
            return (PinyinData.MinValue <= c && c <= PinyinData.MaxValue && GetPinyinCode(c) > 0) || PinyinData.Char12295 == c;
        }

        /// <summary>
        /// 获取单个字符的拼音
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private static string GetPinyin(char c)
        {
            if (IsChinese(c))
            {
                return c == PinyinData.Char12295 ? PinyinData.Pinyin12295 : PinyinData.PinyinTable[GetPinyinCode(c)];
            }
            else
            {
                return c.ToString();
            }
        }
    }
}
