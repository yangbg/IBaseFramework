using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace IBaseFramework.Extension
{
    public static class JsonEx
    {
        /// <summary>
        /// 序列化为json格式
        /// </summary>
        /// <param name="jsonObj"></param>
        /// <returns></returns>
        public static string ToJson(this object jsonObj)
        {
            return JsonConvert.SerializeObject(jsonObj);
        }

        /// <summary> 反序列化到匿名对象 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param> 
        /// <returns></returns>
        public static T JsonToObj<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        ///将 object 对象(一般是实体类等)序列化为 XML 字符串。如果参数为null，则返回空字符串
        /// </summary>
        /// <param name="this">扩展对象</param>
        /// <param name="isFormat">是否需要格式化，缩进显示</param>
        /// <returns></returns>
        public static string ToXml(this object @this, bool isFormat = false)
        {
            if (null == @this)
                return string.Empty;

            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            var encoding = new UTF8Encoding(false);
            var stream = new MemoryStream();
            var writer = new XmlTextWriter(stream, encoding)
            {
                Formatting = isFormat ? System.Xml.Formatting.Indented : System.Xml.Formatting.None
            };

            try
            {
                var serializer = new XmlSerializer(@this.GetType());
                serializer.Serialize(writer, @this, ns);
            }
            catch (InvalidOperationException)
            {
                throw new InvalidOperationException("无法转换为xml。");
            }
            finally
            {
                writer.Close();
            }

            return encoding.GetString(stream.ToArray());
        }


        /// <summary>
        /// 将 XML 字符串反序列化为指定类型的对象。类及属性需要做xml特性标记，与xml文件格式对应。
        /// </summary>
        /// <typeparam name="T">泛型类型参数，例如 typeof(实体、DataTable、List集合等对象)</typeparam>
        /// <param name="this">XML字符串</param>
        /// <returns></returns>
        public static T XmlToObj<T>(this string @this) where T : class
        {
            object obj;
            var serializer = new XmlSerializer(typeof(T));
            var strReader = new StringReader(@this);
            XmlReader reader = new XmlTextReader(strReader);

            try
            {
                obj = serializer.Deserialize(reader);
            }
            catch (InvalidOperationException ie)
            {
                throw new InvalidOperationException("无法将xml文件转换为object对象。", ie);
            }
            finally
            {
                reader.Close();
            }
            return (T)obj;
        }

        ///  <summary>
        /// 将泛型类型对象(一般是实体类等)序列化为二进制字符串
        ///  </summary>
        ///  <param name="this">扩展对象</param>
        ///  <param name="encoding"></param>
        ///  <returns></returns>
        public static string ToBinary(this object @this, Encoding encoding = null)
        {
            if (null == @this)
                return string.Empty;

            var binaryWrite = new BinaryFormatter();
            using (var memoryStream = new MemoryStream())
            {
                binaryWrite.Serialize(memoryStream, @this);
                return encoding == null ? Encoding.Default.GetString(memoryStream.ToArray()) : encoding.GetString(memoryStream.ToArray());
            }
        }

        /// <summary>
        /// 将二进制字符串反序列化为泛型类型对象
        /// </summary>
        /// <param name="this">扩展对象。二进制字符串</param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static T BinaryToObj<T>(this string @this, Encoding encoding = null)
        {
            using (var stream = new MemoryStream(encoding == null ? Encoding.Default.GetBytes(@this) : encoding.GetBytes(@this)))
            {
                var binaryRead = new BinaryFormatter();
                return (T)binaryRead.Deserialize(stream);
            }
        }
    }

    /// <summary>
    /// 将long类型转换成字符串类型
    /// </summary>
    public class LongJsonConverter : JsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jt = JToken.ReadFrom(reader);

            return jt.Value<long>();
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(long) == objectType || typeof(long?) == objectType;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.ToString());
        }
    }

    public class NullToEmptyResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            property.ValueProvider = new NullToEmptyStringValueProvider(property.ValueProvider, property.PropertyType);

            return property;
        }
    }

    sealed class NullToEmptyStringValueProvider : IValueProvider
    {
        readonly IValueProvider _valueProvider;
        readonly Type _type;
        public NullToEmptyStringValueProvider(IValueProvider valueProvider, Type type)
        {
            _valueProvider = valueProvider;
            _type = type;
        }

        public object GetValue(object target)
        {
            if (_type == typeof(string))
            {
                return _valueProvider.GetValue(target) ?? string.Empty;
            }

            return _valueProvider.GetValue(target);
        }

        public void SetValue(object target, object value)
        {
            _valueProvider.SetValue(target, value);
        }
    }
}