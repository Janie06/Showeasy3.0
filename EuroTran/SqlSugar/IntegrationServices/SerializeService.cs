using Newtonsoft.Json;

namespace SqlSugar
{
    public class SerializeService:ISerializeService
    {
        public string SerializeObject(object value)
        {
            return JsonConvert.SerializeObject(value);
        }

        public T DeserializeObject<T>(string value)
        {
            var jSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            return JsonConvert.DeserializeObject<T>(value, jSetting);
        }
    }
}
