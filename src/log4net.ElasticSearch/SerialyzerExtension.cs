using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace log4net.ElasticSearch
{
    public static class SerialyzerExtension
    {
        public static string SerialyzeToString(this JsonSerializer serializer, object value)
        {
            var sb = new StringBuilder(256);
            var sw = new StringWriter(sb, CultureInfo.InvariantCulture);
            using (var jsonWriter = new JsonTextWriter(sw))
            {
                jsonWriter.Formatting = Formatting.None;

                serializer.Serialize(jsonWriter, value);
            }

            return sw.ToString();
        }
    }
}
