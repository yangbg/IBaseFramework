using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace IBaseFramework.Utility.Extension
{
    public static class JsonEx
    {
        /// <summary> 序列化为json格式 </summary>
        /// <typeparam name="T"></typeparam>
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

            return jt.Value<Int64>();
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(Int64) == objectType || typeof(Int64?) == objectType;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.ToString());
        }
    }
}