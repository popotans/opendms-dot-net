using System;
using System.Web;
using Tcp = OpenDMS.Networking.Protocols.Tcp;
using Http = OpenDMS.Networking.Protocols.Http;
using Newtonsoft.Json.Linq;

namespace OpenDMS.Networking.Api.Requests
{
    /// <summary>
    /// xxxxx\0{
    ///     Type: xxxxxx,
    ///     Timestamp: xxxxx,
    ///     Duration: xxxxxx,
    ///     JsonLength: xxxx
    /// }[data]
    /// </summary>
    public class Ping : RequestBase
    {
        public Ping()
        {
        }

        public Ping(JObject fullContent)
        {
            FullContent = fullContent;
        }

        public Ping(RequestBase request)
            : this(request.FullContent)
        {
            Stream = request.Stream;
        }

        public override Protocols.Http.Methods.Base GetMethod()
        {
            return new Protocols.Http.Methods.Get();
        }

        public static Ping BuildFrom(HttpApplication app)
        {
            return new Ping(Parse(app.Request));
        }

        public override Protocols.Http.Request CreateRequest(Uri uri, string contentType)
        {
            Protocols.Http.Methods.Get method;

            method = (Protocols.Http.Methods.Get)GetMethod();

            return CreateRequest(method, uri, contentType);
        }
    }
}
