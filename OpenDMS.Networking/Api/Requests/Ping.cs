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

        public override Protocols.Http.Methods.Base GetMethod()
        {
            return new Protocols.Http.Methods.Get();
        }

        public override Protocols.Http.Request CreateRequest(Uri uri, string contentType)
        {
            Protocols.Http.Methods.Get method;

            method = (Protocols.Http.Methods.Get)GetMethod();

            return CreateRequest(method, uri, contentType);
        }
    }
}
