using System;
using System.Collections.Generic;
using System.Linq;
using log4net.Core;
using log4net.ElasticSearch.Infrastructure;
using Newtonsoft.Json;

namespace log4net.ElasticSearch.Models
{
    /// <summary>
    /// Primary object which will get serialized into a json object to pass to ES. Deviating from CamelCase
    /// class members so that we can stick with the build in serializer and not take a dependency on another lib. ES
    /// exepects fields to start with lowercase letters.
    /// </summary>
    public class logEvent
    {
        public logEvent()
        {
            properties = new Dictionary<string, string>();
        }

        public string timeStamp { get; set; }

        public string message { get; set; }

        public string serializedMessage { get; set; }

        public object exception { get; set; }

        public string serializedException { get; set; }

        public string loggerName { get; set; }

        public string domain { get; set; }

        public string identity { get; set; }

        public string level { get; set; }

        public string className { get; set; }

        public string fileName { get; set; }

        public string lineNumber { get; set; }

        public string fullInfo { get; set; }

        public string methodName { get; set; }

        public string fix { get; set; }

        public IDictionary<string, string> properties { get; set; }

        public string userName { get; set; }

        public string threadName { get; set; }

        public string hostName { get; set; }

        public static IEnumerable<logEvent> CreateMany(IEnumerable<LoggingEvent> loggingEvents, JsonSerializer jsonSerializer)
        {
            return loggingEvents.Select(@event => Create(@event, jsonSerializer)).ToArray();
        }

        static logEvent Create(LoggingEvent loggingEvent, JsonSerializer jsonSerializer)
        {
            var logEvent = new logEvent
            {
                loggerName = loggingEvent.LoggerName,
                domain = loggingEvent.Domain,
                identity = loggingEvent.Identity,
                threadName = loggingEvent.ThreadName,
                userName = loggingEvent.UserName,
                timeStamp = loggingEvent.TimeStamp.ToUniversalTime().ToString("O"),
                exception = loggingEvent.ExceptionObject == null ? new object() : JsonSerializableException.Create(loggingEvent.ExceptionObject),
                serializedException = loggingEvent.ExceptionObject == null ? default(string) : SerializeException(loggingEvent.ExceptionObject, jsonSerializer),
                message = loggingEvent.RenderedMessage,
                fix = loggingEvent.Fix.ToString(),
                hostName = Environment.MachineName,
                level = loggingEvent.Level == null ? null : loggingEvent.Level.DisplayName
            };

            // Added special handling of the MessageObject since it may be an exception. 
            // Exception Types require specialized serialization to prevent serialization exceptions.
            if (loggingEvent.MessageObject != null && loggingEvent.MessageObject.GetType() != typeof(string))
            {
                if (loggingEvent.MessageObject is Exception)
                {
                    if(loggingEvent.MessageObject != loggingEvent.ExceptionObject)
                        logEvent.serializedMessage = SerializeException((Exception)loggingEvent.MessageObject, jsonSerializer);
                }
                else
                {
                    logEvent.serializedMessage = Serialize(loggingEvent.MessageObject, jsonSerializer);
                }
            }

            if (loggingEvent.LocationInformation != null)
            {
                logEvent.className = loggingEvent.LocationInformation.ClassName;
                logEvent.fileName = loggingEvent.LocationInformation.FileName;
                logEvent.lineNumber = loggingEvent.LocationInformation.LineNumber;
                logEvent.fullInfo = loggingEvent.LocationInformation.FullInfo;
                logEvent.methodName = loggingEvent.LocationInformation.MethodName;
            }

            AddProperties(loggingEvent, logEvent);

            return logEvent;
        }

        private static string SerializeException(Exception exception, JsonSerializer jsonSerializer)
        {
            return Serialize(exception, jsonSerializer)
                   ?? Serialize(JsonSerializableException.Create(exception), jsonSerializer);
        }

        static void AddProperties(LoggingEvent loggingEvent, logEvent logEvent)
        {
            loggingEvent.Properties().Union(AppenderPropertiesFor(loggingEvent)).
                         Do(pair => logEvent.properties.Add(pair.Key, pair.Value));
        }

        static IEnumerable<KeyValuePair<string, string>> AppenderPropertiesFor(LoggingEvent loggingEvent)
        {
            yield return Pair.For("@timestamp", loggingEvent.TimeStamp.ToUniversalTime().ToString("O"));
        }

        static string Serialize(object value, JsonSerializer serializer)
        {
            try
            {
                return serializer.SerialyzeToString(value);
            }
            catch
            {
                return null;
            }
        }
    }
}