using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using log4net.Core;
using log4net.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace log4net.ElasticSearch
{
    public class SerializeMessageConverter : PatternConverter
    {
        private static readonly ConcurrentDictionary<Type, bool> Types = new ConcurrentDictionary<Type, bool>();
        private readonly JsonSerializer _serializer;

        public SerializeMessageConverter()
        {
            _serializer = new JsonSerializer
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            _serializer.Converters.Add(new StringEnumConverter());
        }

        protected override void Convert(TextWriter writer, object state)
        {
            var loggingEvent = state as LoggingEvent;
            if (loggingEvent == null)
            {
                writer.Write(SystemInfo.NullText);
                return;
            }
            var message = loggingEvent.MessageObject;
            if (message == null)
            {
                writer.Write("null");
                return;
            }
            bool anonymousType;
            var type = message.GetType();
            if (!Types.TryGetValue(type, out anonymousType))
            {
                anonymousType = IsAnonymousType(type);
                Types[type] = anonymousType;
            }

            var strMessage = anonymousType ? Serialize(message) : message.ToString();
            writer.Write(strMessage);
        }

        private string Serialize(object message)
        {
            return _serializer.SerialyzeToString(message);
        }

        private bool IsAnonymousType(Type type)
        {
            var hasCompilerGeneratedAttribute = type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Length > 0;
            var nameContainsAnonymousType = type.FullName.Contains("AnonymousType");
            var isAnonymousType = hasCompilerGeneratedAttribute && nameContainsAnonymousType;

            return isAnonymousType;
        }
    }
}
