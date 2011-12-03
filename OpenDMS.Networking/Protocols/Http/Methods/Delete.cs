using System;

namespace OpenDMS.Networking.Protocols.Http.Methods
{
    public class Delete 
        : Base
    {
        private const string METHOD = "DELETE";

        public Delete()
            : base(METHOD)
        {
        }
    }
}
