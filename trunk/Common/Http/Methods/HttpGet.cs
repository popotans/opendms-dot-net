using System;

namespace Common.Http.Methods
{
    public class HttpGet : HttpRequest
    {
        private const string METHOD = "GET";

        public override string Method { get { return METHOD; } }

        public HttpGet(Uri uri)
            : base(uri)
        {
        }
    }
}
