using System;

namespace OpenDMS.Networking.Protocols.Http.Message
{
    public class AcceptHeader : Header
    {
        public static new string NAME { get { return "Accept"; } }

        public AcceptHeader(string value)
            : base(new Token(NAME), value)
        {
        }
    }
}
