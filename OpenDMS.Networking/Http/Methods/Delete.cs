using System;

namespace OpenDMS.Networking.Http.Methods
{
    public class Delete : Request
    {
        private const string METHOD = "DELETE";

        public override string Method { get { return METHOD; } }

        public Delete(Uri uri)
            : base(uri)
        {
        }
    }
}
