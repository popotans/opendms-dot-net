using System;

namespace OpenDMS.Networking.Protocols.Http.Message
{
    public class ChunkedTransferEncodingHeader : Header
    {
        public static override string NAME { get { return "Transfer-Encoding"; } }

        public ChunkedTransferEncodingHeader()
            : base(new Token(NAME), "chunked")
        {
        }
    }
}
