using Newtonsoft.Json;

namespace CouchN
{
    public static class SerializerHelper
    {
        public static string Serialize(this object value)
        {
            if (value == null) return null;
            return JsonConvert.SerializeObject(value, Formatting.None, new JsonSerializerSettings(){ NullValueHandling = NullValueHandling.Ignore});
        }

        public static T DeserializeObject<T>(this string value)
        {
            if (value == null) return default(T);
            return JsonConvert.DeserializeObject<T>(value, new JsonSerializerSettings(){ NullValueHandling = NullValueHandling.Ignore});
        }
    }
}
