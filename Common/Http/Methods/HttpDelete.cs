using System;

namespace Common.Http.Methods
{
    public class HttpDelete : HttpRequest
    {
        private const string METHOD = "DELETE";

        public override string Method { get { return METHOD; } }

        public HttpDelete(Uri uri)
            : base(uri)
        {
        }
    }
}
