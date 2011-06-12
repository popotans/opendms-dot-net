using System;

namespace OpenDMS.Networking.Http.Methods
{
    public class Put : Request
    {
        private const string METHOD = "PUT";

        public override string Method { get { return METHOD; } }

        public Put(Uri uri, string contentType)
            : base(uri)
        {
            ContentType = ContentType;
        }

        public Put(Uri uri, string contentType, ulong contentLength)
            : base(uri)
        {
            ContentType = ContentType;
            ContentLength = contentLength.ToString();
        }
    }
}
