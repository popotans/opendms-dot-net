using System;

namespace OpenDMS.Networking.Protocols.Http.Message
{
    public class ContentTypeHeader : Header
    {
        public static new string NAME { get { return "Content-Type"; } }

        public ContentTypeHeader(string value)
            : base(new Token(NAME), value)
        {
        }
    }
}
