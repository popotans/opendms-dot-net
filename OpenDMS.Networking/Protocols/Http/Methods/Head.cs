using System;

namespace OpenDMS.Networking.Protocols.Http.Methods
{
    public class Head
        : Base
    {
        private const string METHOD = "HEAD";

        public Head()
            : base(METHOD)
        {
        }
    }
}
