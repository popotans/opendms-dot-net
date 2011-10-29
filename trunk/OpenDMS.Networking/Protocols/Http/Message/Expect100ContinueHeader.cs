using System;

namespace OpenDMS.Networking.Protocols.Http.Message
{
    public class Expect100ContinueHeader : Header
    {
        public static override string NAME { get { return "Expect"; } }

        public Expect100ContinueHeader()
            : base(new Token(NAME), "100-continue")
        {
        }
    }
}
