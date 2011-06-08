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
            Headers.Add("Content-Type", contentType);
        }
    }
}
