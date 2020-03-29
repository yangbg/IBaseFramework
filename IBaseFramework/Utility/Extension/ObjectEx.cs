using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using IBaseFramework.Utility.Helper;

namespace IBaseFramework.Utility.Extension
{
    /// <summary>
    /// 对象扩展辅助
    /// </summary>
    public static class ObjectExtension
    {

        /// <summary>
        /// 对象转换为泛型
        /// </summary>
        /// <param name="obj"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T CastTo<T>(this object obj)
        {
            return obj.CastTo(default(T));
        }

        /// <summary>
        /// 对象转换为泛型
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="def"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T CastTo<T>(this object obj, T def)
        {
            var value = obj.CastTo(typeof(T));
            if (value == null)
                return def;
            return (T)value;
        }

        /// <summary>
        /// 把对象类型转换为指定类型
        /// </summary>
        /// <param name="value"></param>
        /// <param name="conversionType"></param>
        /// <returns></returns>
        public static object CastTo(this object value, Type conversionType)
        {
            if (value == null) return null;
            if (((conversionType != null) && conversionType.IsGenericType) && (conversionType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                conversionType = conversionType.GetGenericArguments()[0];
            try
            {
                if (conversionType != null && conversionType.IsEnum)
                    return Enum.Parse(conversionType, value.ToString());
                if (conversionType == typeof(Guid))
                    return Guid.Parse(value.ToString());
                return Convert.ChangeType(value, conversionType);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 将对象[主要是匿名对象]转换为dynamic
        /// </summary>
        public static dynamic ToDynamic(this object value)
        {
            IDictionary<string, object> expando = new ExpandoObject();
            var type = value.GetType();
            var properties = TypeDescriptor.GetProperties(type);
            foreach (PropertyDescriptor property in properties)
            {
                var val = property.GetValue(value);
                if (property.PropertyType.FullName != null && property.PropertyType.FullName.StartsWith("<>f__AnonymousType"))
                {
                    var dval = val.ToDynamic();
                    expando.Add(property.Name, dval);
                }
                else
                {
                    expando.Add(property.Name, val);
                }
            }
            return (ExpandoObject)expando;
        }

        /// <summary>
        /// 写异常信息
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="path"></param>
        public static void WriteTo(this Exception ex, string path)
        {
            FileHelper.WriteException(path, ex);
        }

        /// <summary>
        /// 异常信息格式化
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="isHideStackTrace"></param>
        /// <returns></returns>
        public static string Format(this Exception ex, bool isHideStackTrace = false)
        {
            var sb = new StringBuilder();
            var count = 0;
            var appString = string.Empty;
            while (ex != null)
            {
                if (count > 0)
                {
                    appString += "  ";
                }
                sb.AppendLine($"{appString}异常消息：{ex.Message}");
                sb.AppendLine($"{appString}异常类型：{ex.GetType().FullName}");
                sb.AppendLine($"{appString}异常方法：{(ex.TargetSite == null ? null : ex.TargetSite.Name)}");
                sb.AppendLine($"{appString}异常源：{ex.Source}");
                if (!isHideStackTrace && ex.StackTrace != null)
                {
                    sb.AppendLine($"{appString}异常堆栈：{ex.StackTrace}");
                }
                if (ex.InnerException != null)
                {
                    sb.AppendLine($"{appString}内部异常：");
                    count++;
                }
                ex = ex.InnerException;
            }
            return sb.ToString();
        }

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

        #region 根据值获取枚举的name

        public static string GetName(this Type type, string value)
        {
            if (type == null) return string.Empty;

            var firstOrDefault = type.GetFields().FirstOrDefault(u => (string)u.GetValue(new object()) == value);

            return firstOrDefault?.Name ?? string.Empty;
        }

        #endregion
    }
}