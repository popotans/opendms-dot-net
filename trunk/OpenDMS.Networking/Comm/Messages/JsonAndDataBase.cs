using System;
using System.IO;
using System.Web;
using Newtonsoft.Json.Linq;

namespace OpenDMS.Networking.Comm.Messages
{
    public class JsonAndDataBase : JsonOnlyBase
    {
        public Stream DataStream { get; set; }

        public JsonAndDataBase()
            : base()
        {
        }

        public JsonAndDataBase(HttpResponse response, Guid conversationId)
            : base(response, conversationId)
        {
        }
    }
}
