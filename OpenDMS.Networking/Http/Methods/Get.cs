using System;

namespace OpenDMS.Networking.Http.Methods
{
    public class Get : Request
    {
        private const string METHOD = "GET";

        public override string Method { get { return METHOD; } }

        public Get(Uri uri)
            : base(uri)
        {
        }
    }
}
