using System;

namespace OpenDMS.Networking.Protocols.Http.Methods
{
    public class Put 
        : Base
    {
        private const string METHOD = "PUT";

        public Put()
            : base(METHOD)
        {
        }
    }
}
