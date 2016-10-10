using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace log4net.ElasticSearch.Infrastructure
{
    class RemoveDotesJsonWriter : JsonTextWriter
    {
        public RemoveDotesJsonWriter(TextWriter textWriter) : base(textWriter)
        {
        }

        public override void WritePropertyName(string name)
        {
            base.WritePropertyName(Escape(name));
        }

        public override void WritePropertyName(string name, bool escape)
        {
            base.WritePropertyName(Escape(name), escape);
        }

        private string Escape(string name)
        {
            return name.ReplaceDots();
        }
    }
}
