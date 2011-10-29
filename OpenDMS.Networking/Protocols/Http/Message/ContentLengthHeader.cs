using System;

namespace OpenDMS.Networking.Protocols.Http.Message
{
    public class ContentLengthHeader : Header
    {
        public static override string NAME { get { return "Content-Length"; } }

        public ContentLengthHeader(int value)
            : base(new Token(NAME), value.ToString())
        {
        }

        public ContentLengthHeader(long value)
            : base(new Token(NAME), value.ToString())
        {
        }
    }
}
