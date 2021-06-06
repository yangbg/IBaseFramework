using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace IBaseFramework.Utility.Extension
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