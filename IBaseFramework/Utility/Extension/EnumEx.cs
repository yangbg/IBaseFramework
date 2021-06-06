using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace IBaseFramework.Utility.Extension
{
    public static class EnumEx
    {
        #region 获取枚举的描述文本
        /// <summary>
        /// 获取枚举的描述文本
        /// </summary>
        /// <param name="enumObj"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetEnumText(this Type enumObj, string value)
        {
            try
            {
                var strValue = value;
                var name = enumObj.IsValueType ? Enum.GetName(enumObj, Convert.ToInt32(value)) : enumObj.GetName(value);

                var fileInfo = enumObj.GetField(name);
                if (fileInfo == null)
                    return strValue;

                var objs = fileInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (objs.Length <= 0)
                    return fileInfo.Name;

                var da = (DescriptionAttribute)objs[0];
                strValue = da.Description;

                return strValue;
            }
            catch (Exception)
            {
                return "";
            }
        }

        private static string GetName(this Type type, string value)
        {
            if (type == null) return string.Empty;

            var firstOrDefault = type.GetFields().FirstOrDefault(u => (string)u.GetValue(new object()) == value);

            return firstOrDefault?.Name ?? string.Empty;
        }

        #endregion

        /// <summary>
        /// 获取枚举的描述信息
        /// </summary>
        /// <param name="enum"></param>
        /// <returns></returns>
        public static string GetEnumDescription(this System.Enum @enum)
        {
            return @enum.GetType().GetField(@enum.ToString()).GetCustomAttribute<DescriptionAttribute>()?.Description;
        }

        /// <summary>
        /// 枚举值转Byte
        /// </summary>
        /// <param name="enum"></param>
        /// <returns></returns>
        public static byte ToByte(this System.Enum @enum)
        {
            return Convert.ToByte(@enum);
        }

        /// <summary>
        /// 获取Int值
        /// </summary>
        /// <param name="enum"></param>
        /// <returns></returns>
        public static int ToInt(this System.Enum @enum)
        {
            return Convert.ToInt32(@enum);
        }
    }
}
