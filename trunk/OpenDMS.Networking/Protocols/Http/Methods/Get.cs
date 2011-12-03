using System;

namespace OpenDMS.Networking.Protocols.Http.Methods
{
    public class Get 
        : Base
    {
        private const string METHOD = "GET";

        public Get()
            : base(METHOD)
        {
        }
    }
}
