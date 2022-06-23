using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace IBaseFramework.Extension
{
    public static class EnumEx
    {
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

                var obj = fileInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (obj.Length <= 0)
                    return fileInfo.Name;

                var da = (DescriptionAttribute)obj[0];
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

        /// <summary>
        /// 获取枚举的描述信息
        /// </summary>
        /// <param name="enum"></param>
        /// <returns></returns>
        public static string GetEnumText(this Enum @enum)
        {
            return @enum.GetType().GetField(Enum.GetName(@enum.GetType(), @enum))?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? string.Empty;
        }

        /// <summary>
        /// 枚举值转Byte
        /// </summary>
        /// <param name="enum"></param>
        /// <returns></returns>
        public static byte ToByte(this Enum @enum)
        {
            return Convert.ToByte(@enum);
        }

        /// <summary>
        /// 获取Int值
        /// </summary>
        /// <param name="enum"></param>
        /// <returns></returns>
        public static int ToInt(this Enum @enum)
        {
            return Convert.ToInt32(@enum);
        }

        /// <summary>
        /// Converts string to enum value.
        /// </summary>
        /// <typeparam name="T">Type of enum</typeparam>
        /// <param name="value">String value to convert</param>
        /// <param name="ignoreCase"></param>
        /// <returns>Returns enum object</returns>
        public static T ToEnum<T>(this object value, bool ignoreCase = true) where T : struct
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return (T)Enum.Parse(typeof(T), value.ToString(), ignoreCase);
        }
    }
}
