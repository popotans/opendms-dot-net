using System;
using System.Web;
using Newtonsoft.Json.Linq;

namespace OpenDMS.Networking.Api.Requests
{
    public class Ping : RequestBase
    {
        public Ping()
        {
        }

        public Ping(JObject fullContent)
        {
            FullContent = fullContent;
        }

        public override Http.Methods.Request CreateRequest(Uri uri, string contentType, long contentLength)
        {
            return new Http.Methods.Get(uri);
        }
    }
}
