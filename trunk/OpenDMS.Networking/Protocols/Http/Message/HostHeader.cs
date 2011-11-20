using System;

namespace OpenDMS.Networking.Protocols.Http.Message
{
    public class HostHeader : Header
    {
        public static new string NAME { get { return "Host"; } }

        public HostHeader(string value)
            : base(new Token(NAME), value)
        {
        }
    }
}
