using System;

namespace Common.Http.Methods
{
    public class HttpPut : HttpRequest
    {
        private const string METHOD = "PUT";

        public override string Method { get { return METHOD; } }

        public HttpPut(Uri uri, string contentType)
            : base(uri)
        {
            Headers.Add("Content-Type", contentType);
        }
    }
}
