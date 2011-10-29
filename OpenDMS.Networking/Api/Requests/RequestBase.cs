using System;
using System.Web;
using Newtonsoft.Json.Linq;

namespace OpenDMS.Networking.Api.Requests
{
    public abstract class RequestBase : MessageBase
    {
        public RequestBase()
        {
        }

        public RequestBase(JObject fullContent)
            : base(fullContent)
        {
            FullContent = fullContent;
        }

        public abstract Http.Methods.Request CreateRequest(Uri uri, string contentType, long contentLength);
    }
}
