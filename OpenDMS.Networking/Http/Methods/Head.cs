using System;

namespace OpenDMS.Networking.Http.Methods
{
    public class Head : Request
    {
        private const string METHOD = "HEAD";

        public override string Method { get { return METHOD; } }

        public Head(Uri uri)
            : base(uri)
        {
        }
    }
}
