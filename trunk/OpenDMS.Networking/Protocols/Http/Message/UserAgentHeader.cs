using System;

namespace OpenDMS.Networking.Protocols.Http.Message
{
    public class UserAgentHeader : Header
    {
        public static new string NAME { get { return "User-Agent"; } }

        public UserAgentHeader(string value)
            : base(new Token(NAME), value)
        {
        }
    }
}
