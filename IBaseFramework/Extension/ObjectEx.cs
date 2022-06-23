using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Reflection;

namespace IBaseFramework.Extension
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

                return conversionType != null ? Convert.ChangeType(value, conversionType) : null;
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
                    var dVal = val.ToDynamic();
                    expando.Add(property.Name, dVal);
                }
                else
                {
                    expando.Add(property.Name, val);
                }
            }
            return (ExpandoObject)expando;
        }

        /// <summary>
        /// 将对象属性转换为key-value对
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static Dictionary<string, object> ToDictionary(this object o)
        {
            var map = new Dictionary<string, object>();
            foreach (var p in o.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!p.CanRead)
                    continue;

                var value = p.GetValue(o);
                if (value == null)
                    continue;

                if (p.PropertyType.AssemblyQualifiedName != null && (p.PropertyType.IsClass && !p.PropertyType.AssemblyQualifiedName.Contains("System")))
                {
                    map.Add(p.Name, value.ToDictionary());
                }
                else
                    map.Add(p.Name, value);
            }
            return map;
        }

        /// <summary>
        /// 字典类型转化为对象
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        public static T ToObject<T>(this IDictionary<string, object> dic) where T : new()
        {
            var md = new T();

            foreach (var d in dic)
            {
                var prop = md.GetType().GetProperty(d.Key);
                if (prop == null || !prop.CanWrite)
                    continue;

                if (prop.PropertyType.AssemblyQualifiedName != null && (prop.PropertyType.IsClass && !prop.PropertyType.AssemblyQualifiedName.Contains("System")))
                {
                    if (d.Value is Dictionary<string, object> dict)
                        prop.SetValue(md, dict.ToObject(prop.PropertyType));
                }
                else
                    prop.SetValue(md, d.Value);
            }
            return md;
        }

        private static object ToObject(this IDictionary<string, object> dic, Type type)
        {
            var md = Activator.CreateInstance(type);

            foreach (var d in dic)
            {
                var prop = md.GetType().GetProperty(d.Key);
                if (prop == null || !prop.CanWrite)
                    continue;

                if (prop.PropertyType.AssemblyQualifiedName != null && (prop.PropertyType.IsClass && !prop.PropertyType.AssemblyQualifiedName.Contains("System")))
                {
                    if (d.Value is Dictionary<string, object> dict)
                        prop.SetValue(md, dict.ToObject(prop.PropertyType));
                }
                else
                    prop.SetValue(md, d.Value);
            }
            return md;
        }

    }
}