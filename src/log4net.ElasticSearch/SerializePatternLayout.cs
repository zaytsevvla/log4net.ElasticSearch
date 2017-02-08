using log4net.Layout;
using log4net.Util;

namespace log4net.ElasticSearch
{
    public class SerializePatternLayout : PatternLayout
    {
        public SerializePatternLayout()
        {
            AddConverter(new ConverterInfo
            {
                Name = "message",
                Type = typeof(SerializeMessageConverter)
            });
        }
    }
}
