using System;

namespace OpenDMS.Networking.Protocols.Http.Message
{
    public class CloseConnectionHeader : Header
    {
        public static new string NAME { get { return "Connection"; } }

        public CloseConnectionHeader()
            : base(new Token(NAME), "close")
        {
        }
    }
}
