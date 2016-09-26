using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace log4net.ElasticSearch.Infrastructure
{
    class RemoveDotesJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var t = JToken.FromObject(value);
            if (t.Type != JTokenType.Object)
            {
                t.WriteTo(writer);
            }
            else
            {
                RemoveDotes((JObject)t).WriteTo(writer);
            }
        }

        private JObject RemoveDotes(JObject source)
        {
            var result = new JObject();
            foreach (var item in source)
            {
                var value = item.Value;
                var obj = value as JObject;
                value = obj != null ? RemoveDotes(obj) : value;

                var key = item.Key;
                if (key.Contains("."))
                {
                    var replaced = key.ReplaceDots();
                    var newKey = replaced;
                    var i = 0;
                    JToken v;
                    while (source.TryGetValue(newKey, out v))
                    {
                        newKey = replaced + "_" + i++;
                    }
                    key = newKey;
                }
                result[key] = value;
            }
            return result;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead => false;
    }
}
