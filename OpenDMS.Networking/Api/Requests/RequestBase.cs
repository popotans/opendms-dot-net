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

        public abstract Protocols.Http.Methods.Base GetMethod();

        public abstract Protocols.Http.Request CreateRequest(Uri uri, string contentType);

        protected Protocols.Http.Request CreateRequest(Protocols.Http.Methods.Base method, Uri uri,
            string contentType)
        {
            Protocols.Http.Request request;
            MultisourcedStream fullStream;
            long fullContentLength;

            request = new Protocols.Http.Request(method, uri);

            fullStream = MakeStream(out fullContentLength);

            request.Headers.Add(new Protocols.Http.Message.ContentLengthHeader(fullContentLength));

            request.Body.IsChunked = false;
            request.Body.SendStream = fullStream;

            return request;
        }
    }
}
